using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Mobile.Models;
using Android.Net;
using Android.Content;
using Org.Apache.Http.Client;
using Android.OS;
using Newtonsoft.Json.Linq;
using Android.Widget;

namespace Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly Context _context;

        // Special IP address for Android Emulator
        private const string EmulatorBaseUrl = "http://10.0.2.2:5063/api/";

        // Your computer's actual local network IP address
        // This should match what you see in your ipconfig output for the WiFi adapter
        private const string PhysicalDeviceBaseUrl = "http://192.168.1.15:5063/api/";

        public ApiService(Context context = null)
        {
            _context = context;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);

            // Choose the right base URL based on whether we're running in an emulator
            string baseUrl = IsRunningOnEmulator() ? EmulatorBaseUrl : PhysicalDeviceBaseUrl;

            _httpClient.BaseAddress = new System.Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(15);

            // Load token from shared preferences if available
            if (context != null)
            {
                string savedToken = TokenManager.GetToken(context);
                if (!string.IsNullOrEmpty(savedToken))
                {
                    // Set the token in the HttpClient headers
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
                    Console.WriteLine("Token loaded from shared preferences and set in HttpClient");
                }
            }

            Console.WriteLine($"API Service initialized with base URL: {baseUrl}");
        }

        // Helper method to detect if we're running in an emulator
        private bool IsRunningOnEmulator()
        {
            // Method 1: Check manufacturer and model
            string manufacturer = Build.Manufacturer.ToLowerInvariant();
            string model = Build.Model.ToLowerInvariant();

            if (manufacturer == "google" && model.Contains("sdk"))
                return true;

            // Method 2: Check product name
            string product = Build.Product.ToLowerInvariant();
            if (product.Contains("sdk") || product.Contains("emulator") || product.Contains("genymotion"))
                return true;

            // Method 3: Check fingerprint
            string fingerprint = Build.Fingerprint.ToLowerInvariant();
            if (fingerprint.Contains("generic") || fingerprint.Contains("vbox") || fingerprint.Contains("test-keys"))
                return true;

            return false;
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            Console.WriteLine($"Attempting login for {email} to {_httpClient.BaseAddress}auth/login");

            // Clear any existing authorization headers
            _httpClient.DefaultRequestHeaders.Authorization = null;

            // Clear any existing tokens
            if (_context != null)
            {
                TokenManager.ClearToken(_context);
            }

            var loginData = new LoginRequest { Email = email, Password = password };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
            {
                try
                {
                    Console.WriteLine("Sending login request...");
                    var response = await _httpClient.PostAsync("auth/login", content, cts.Token);
                    Console.WriteLine($"Received response: {response.StatusCode}");

                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Check if the response is HTML (login page)
                    if (responseContent.Contains("<!DOCTYPE html>") || responseContent.Contains("<html"))
                    {
                        Console.WriteLine("Received HTML instead of JSON authentication response");
                        throw new Exception("The server returned an unexpected response. Please try again later.");
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Response content received successfully");

                        try
                        {
                            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                            // Store token in header for this instance
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

                            // Store token in shared preferences
                            if (_context != null)
                            {
                                TokenManager.SaveToken(
                                    _context,
                                    authResponse.Token,
                                    authResponse.User.UserName,
                                    authResponse.User.Id
                                );
                                Console.WriteLine("Token saved to shared preferences after login");
                            }

                            return authResponse;
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"Failed to parse authentication response: {jsonEx.Message}");
                            Console.WriteLine($"Response content was: {responseContent}");
                            throw new Exception("The server returned data in an unexpected format. Please try again later.");
                        }
                    }

                    // Handle specific status codes
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("Invalid email or password. Please try again.");
                    }

                    Console.WriteLine($"Error response: {responseContent}");
                    throw new Exception($"Login failed: {response.StatusCode} - {response.ReasonPhrase}");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Login request timed out");
                    throw new Exception($"Login request timed out. Please verify your network connection and that the API server is running at {_httpClient.BaseAddress}.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error: {ex.Message}");
                    throw new Exception($"Network error: {ex.Message}. Make sure the API server is running and accessible at {_httpClient.BaseAddress}");
                }
            }
        }


        public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            Console.WriteLine($"Attempting registration for {registerRequest.Email} to {_httpClient.BaseAddress}auth/register");

            // Clear any existing authorization headers
            _httpClient.DefaultRequestHeaders.Authorization = null;

            // Clear any existing tokens
            if (_context != null)
            {
                TokenManager.ClearToken(_context);
            }

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
            {
                try
                {
                    Console.WriteLine("Sending registration request...");
                    var response = await _httpClient.PostAsync("auth/register", content, cts.Token);
                    Console.WriteLine($"Received response: {response.StatusCode}");

                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Check if the response is HTML (error page)
                    if (responseContent.Contains("<!DOCTYPE html>") || responseContent.Contains("<html"))
                    {
                        Console.WriteLine("Received HTML instead of JSON registration response");
                        throw new Exception("The server returned an unexpected response. Please try again later.");
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Registration response received successfully");

                        try
                        {
                            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                            // Store token in header for this instance
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

                            // Store token in shared preferences
                            if (_context != null)
                            {
                                TokenManager.SaveToken(
                                    _context,
                                    authResponse.Token,
                                    authResponse.User.UserName,
                                    authResponse.User.Id
                                );
                                Console.WriteLine("Token saved to shared preferences after registration");
                            }

                            return authResponse;
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"Failed to parse registration response: {jsonEx.Message}");
                            Console.WriteLine($"Response content was: {responseContent}");
                            throw new Exception("The server returned data in an unexpected format. Please try again later.");
                        }
                    }

                    // Try to extract error message from response content
                    string errorMessage = "Registration failed";
                    try
                    {
                        // Try to parse as an ErrorResponse object
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                        if (errorResponse != null)
                        {
                            if (!string.IsNullOrEmpty(errorResponse.Message))
                            {
                                errorMessage = errorResponse.Message;
                            }
                            else if (!string.IsNullOrEmpty(errorResponse.Error))
                            {
                                errorMessage = errorResponse.Error;
                            }
                            else if (errorResponse.Errors != null && errorResponse.Errors.Count > 0)
                            {
                                // Get the first validation error
                                foreach (var error in errorResponse.Errors)
                                {
                                    if (error.Value != null && error.Value.Length > 0)
                                    {
                                        errorMessage = error.Value[0];
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Try parsing as a simpler dictionary as fallback
                            var errorObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                            if (errorObj != null)
                            {
                                if (errorObj.ContainsKey("message"))
                                {
                                    errorMessage = errorObj["message"].ToString();
                                }
                                else if (errorObj.ContainsKey("error"))
                                {
                                    errorMessage = errorObj["error"].ToString();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If we can't parse the error, use the original response
                        errorMessage = $"Registration failed: {response.StatusCode} - {responseContent}";
                    }

                    Console.WriteLine($"Error response: {errorMessage}");
                    throw new Exception(errorMessage);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Registration request timed out");
                    throw new Exception($"Registration request timed out. Please verify your network connection and that the API server is running at {_httpClient.BaseAddress}.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error: {ex.Message}");
                    throw new Exception($"Network error: {ex.Message}. Make sure the API server is running and accessible at {_httpClient.BaseAddress}");
                }
            }
        }

        public async Task<QuizDetail> GetQuizDetailAsync(int quizId)
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync($"quizzes/{quizId}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizDetail>(responseJson);
            }

            throw new Exception($"Failed to get quiz detail: {response.ReasonPhrase}");
        }

        public async Task<QuizResult> SubmitQuizAsync(QuizSubmission submission)
        {
            EnsureAuthenticated();

            try
            {
                // Validate submission
                if (submission == null)
                    throw new ArgumentNullException(nameof(submission), "Quiz submission cannot be null");

                if (submission.QuizId <= 0)
                    throw new ArgumentException("Invalid quiz ID in submission");

                if (submission.Answers == null || submission.Answers.Count == 0)
                    throw new ArgumentException("No answers provided in submission");

                // Log the submission for debugging
                Console.WriteLine($"Submitting quiz ID: {submission.QuizId} with {submission.Answers.Count} answers");

                // Serialize with formatting settings to ensure proper JSON
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                };

                var json = JsonConvert.SerializeObject(submission, jsonSettings);
                Console.WriteLine($"Submission JSON: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Ensure authorization header is set for this specific request
                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization") && _context != null)
                {
                    var token = TokenManager.GetToken(_context);
                    if (!string.IsNullOrEmpty(token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        Console.WriteLine("Re-applying token to request");
                    }
                }

                // Log all headers for debugging
                Console.WriteLine("Request Headers:");
                foreach (var header in _httpClient.DefaultRequestHeaders)
                {
                    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                // Set a timeout for the request
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    var response = await _httpClient.PostAsync("quizzes/take", content, cts.Token);

                    Console.WriteLine($"API response status: {response.StatusCode}");

                    // Read the response content
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Check if the response is actually HTML (login page)
                    if (responseContent.Contains("<!DOCTYPE html>") || responseContent.Contains("<html"))
                    {
                        Console.WriteLine("Received HTML response instead of JSON - Authentication failed");

                        // Clear the token since it's invalid
                        if (_context != null)
                        {
                            TokenManager.ClearToken(_context);
                        }

                        throw new UnauthorizedAccessException("Your session has expired. Please log in again.");
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"API response content: {responseContent}");

                        try
                        {
                            return JsonConvert.DeserializeObject<QuizResult>(responseContent);
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"Failed to parse successful response: {jsonEx.Message}");
                            Console.WriteLine($"Response content was: {responseContent}");
                            throw new Exception("The server returned data in an unexpected format. Please try again later.");
                        }
                    }

                    // Try to get error details from response
                    Console.WriteLine($"API error response: {responseContent}");

                    // Check for specific status codes
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // Clear the token since it's invalid
                        if (_context != null)
                        {
                            TokenManager.ClearToken(_context);
                        }

                        throw new UnauthorizedAccessException("Your session has expired. Please log in again.");
                    }

                    throw new Exception($"Failed to submit quiz. Server returned: {response.StatusCode}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw authentication errors
                throw;
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON serialization error: {jsonEx}");
                throw new Exception($"Error preparing quiz data: {jsonEx.Message}", jsonEx);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("The request timed out. Please check your internet connection and try again.");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx}");
                throw new Exception($"Network error while submitting quiz: {httpEx.Message}", httpEx);
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is ArgumentNullException || ex is UnauthorizedAccessException))
            {
                Console.WriteLine($"Unexpected error in SubmitQuizAsync: {ex}");
                throw new Exception($"Error submitting quiz: {ex.Message}", ex);
            }
        }

        public async Task<QuizResult> GetQuizResultAsync(int attemptId)
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync($"quizzes/result/{attemptId}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizResult>(responseJson);
            }

            throw new Exception($"Failed to get quiz result: {response.ReasonPhrase}");
        }

        public async Task<UserStats> GetUserStatsAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("user/stats");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserStats>(responseJson);
            }

            throw new Exception($"Failed to get user stats: {response.ReasonPhrase}");
        }

        public async Task<UserProfile> GetUserProfileAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("user/profile");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserProfile>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be logged in to access your profile.");
            }

            throw new Exception($"Failed to get user profile: {response.ReasonPhrase}");
        }

        public async Task<UserProfile> UpdateUserProfileAsync(ProfileUpdateRequest updateRequest)
        {
            EnsureAuthenticated();

            var json = JsonConvert.SerializeObject(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("user/profile", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserProfile>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be logged in to update your profile.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update profile: {errorContent}");
        }

        public async Task ChangePasswordAsync(PasswordChangeRequest passwordChangeRequest)
        {
            EnsureAuthenticated();

            var json = JsonConvert.SerializeObject(passwordChangeRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("user/change-password", content);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be logged in to change your password.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();

            // Try to parse the error message
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    throw new Exception(errorResponse.Message);
                }
            }
            catch
            {
                // Fallback to the raw error content
            }

            throw new Exception($"Failed to change password: {errorContent}");
        }

        private bool IsNetworkAvailable()
        {
            if (_context == null)
                return true; // Can't check, assume it's available

            ConnectivityManager connectivityManager = (ConnectivityManager)_context.GetSystemService(Context.ConnectivityService);
            NetworkInfo activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            return activeNetworkInfo != null && activeNetworkInfo.IsConnected;
        }

        private void EnsureAuthenticated()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                // Try to load from shared preferences if context is available
                if (_context != null)
                {
                    string savedToken = TokenManager.GetToken(_context);
                    if (!string.IsNullOrEmpty(savedToken))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
                        Console.WriteLine("Token loaded from shared preferences and set in HttpClient");
                        return;
                    }
                }

                Console.WriteLine("Authentication failed: No token available");
                throw new UnauthorizedAccessException("You must be logged in to access this resource. Please log in again.");
            }
            else
            {
                // Log that we're using an existing token
                Console.WriteLine("Using existing token from HttpClient headers");
            }
        }

        // Admin-specific methods
        public async Task<AdminStats> GetAdminStatsAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("admin/stats");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AdminStats>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You must be an administrator to access these statistics.");
            }

            throw new Exception($"Failed to get admin statistics: {response.ReasonPhrase}");
        }


        // Quiz Administration methods
        public async Task<List<Quiz>> GetQuizzesAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("quizzes");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Quiz>>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You must be logged in to access quizzes.");
            }

            throw new Exception($"Failed to get quizzes: {response.ReasonPhrase}");
        }

        public async Task<QuizAdmin> GetQuizAdminAsync(int quizId)
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync($"admin/quizzes/{quizId}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizAdmin>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You must be an administrator to access this resource.");
            }

            throw new Exception($"Failed to get quiz details: {response.ReasonPhrase}");
        }

        public async Task<QuizAdmin> CreateQuizAsync(QuizAdmin quiz)
        {
            EnsureAuthenticated();

            var json = JsonConvert.SerializeObject(quiz);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("admin/quizzes", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizAdmin>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You must be an administrator to create quizzes.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create quiz: {errorContent}");
        }

        public async Task<QuizAdmin> UpdateQuizAsync(int quizId, QuizAdmin quiz)
        {
            EnsureAuthenticated();

            var json = JsonConvert.SerializeObject(quiz);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"admin/quizzes/{quizId}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizAdmin>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You must be an administrator to update quizzes.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update quiz: {errorContent}");
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            EnsureAuthenticated();

            var response = await _httpClient.DeleteAsync($"admin/quizzes/{quizId}");

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You must be an administrator to delete quizzes.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to delete quiz: {errorContent}");
        }

        // Add/Update these methods in your ApiService class

        // User Administration methods
        public async Task<List<UserProfile>> GetUsersAsync()
        {
            EnsureAuthenticated();
            Console.WriteLine("ApiService: Getting all users");

            var response = await _httpClient.GetAsync("admin/users");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ApiService: Get users response: {responseJson}");
                return JsonConvert.DeserializeObject<List<UserProfile>>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("ApiService: Unauthorized access to users list");
                throw new UnauthorizedAccessException("You must be an administrator to access user list.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ApiService: Error getting users: {errorContent}");
            throw new Exception($"Failed to get users: {errorContent}");
        }

        public async Task<UserProfile> GetUserByIdAsync(string userId)
        {
            EnsureAuthenticated();
            Console.WriteLine($"ApiService: Getting user by ID: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty");
            }

            var response = await _httpClient.GetAsync($"admin/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ApiService: Get user response: {responseJson}");
                return JsonConvert.DeserializeObject<UserProfile>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("ApiService: Unauthorized access to user details");
                throw new UnauthorizedAccessException("You must be an administrator to access user details.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ApiService: Error getting user: {errorContent}");
            throw new Exception($"Failed to get user details: {errorContent}");
        }

        // Modified: Use the auth/register endpoint for user creation
        public async Task<UserProfile> CreateUserAsync(AdminCreateUserRequest request)
        {
            EnsureAuthenticated();
            Console.WriteLine($"ApiService: Creating user with email: {request.Email}, username: {request.UserName}");

            // Convert the admin request to a register request
            var registerRequest = new RegisterRequest
            {
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.Password // Using the same password
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"ApiService: User creation payload: {json}");

            var response = await _httpClient.PostAsync("auth/register", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ApiService: Create user response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // If registration is successful, update the username and roles if needed
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                if (authResponse != null && authResponse.User != null)
                {
                    // If username needs to be updated or roles need to be set
                    if (authResponse.User.UserName != request.UserName || request.Roles.Contains("Administrator"))
                    {
                        try
                        {
                            // Update the username and/or roles
                            await UpdateUserRolesAsync(authResponse.User.Id, request.Roles);

                            // Get the updated user profile
                            return await GetUserByIdAsync(authResponse.User.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ApiService: Error updating new user: {ex.Message}");
                            // Still return the created user even if update fails
                            var userProfile = new UserProfile
                            {
                                Id = authResponse.User.Id,
                                UserName = authResponse.User.UserName,
                                Email = authResponse.User.Email,
                                Roles = new List<string> { "User" }
                            };
                            return userProfile;
                        }
                    }

                    // Convert from AuthResponse.User to UserProfile
                    var profile = new UserProfile
                    {
                        Id = authResponse.User.Id,
                        UserName = authResponse.User.UserName,
                        Email = authResponse.User.Email,
                        Roles = new List<string> { "User" }
                    };

                    return profile;
                }

                throw new Exception("User created but response contained no user data");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("ApiService: Unauthorized access when creating user");
                throw new UnauthorizedAccessException("You must be an administrator to create users.");
            }

            Console.WriteLine($"ApiService: Error creating user: {responseContent}");

            try
            {
                // Try to extract meaningful error from response
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    throw new Exception(errorResponse.Message);
                }
            }
            catch { /* Ignore parsing errors */ }

            throw new Exception($"Failed to create user: {responseContent}");
        }

        // Modified: Update uses the roles PATCH endpoint
        public async Task<UserProfile> UpdateUserAsync(string userId, AdminUpdateUserRequest request)
        {
            EnsureAuthenticated();
            Console.WriteLine($"ApiService: Updating user {userId} with username: {request.UserName}, roles: {string.Join(", ", request.Roles)}");

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty");
            }

            // First, update the roles
            try
            {
                await UpdateUserRolesAsync(userId, request.Roles);
                Console.WriteLine("ApiService: User roles updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApiService: Error updating user roles: {ex.Message}");
                throw new Exception($"Failed to update user roles: {ex.Message}");
            }

            // Return the updated user profile
            return await GetUserByIdAsync(userId);
        }

        // New method for updating user roles
        private async Task UpdateUserRolesAsync(string userId, List<string> roles)
        {
            Console.WriteLine($"ApiService: Updating roles for user {userId}: {string.Join(", ", roles)}");

            var json = JsonConvert.SerializeObject(roles);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"admin/users/{userId}/roles", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ApiService: Update roles response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("You must be an administrator to update user roles.");
                }

                throw new Exception($"Failed to update user roles: {responseContent}");
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            EnsureAuthenticated();
            Console.WriteLine($"ApiService: Deleting user {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty");
            }

            var response = await _httpClient.DeleteAsync($"admin/users/{userId}");

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ApiService: Delete user response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("ApiService: Unauthorized access when deleting user");
                throw new UnauthorizedAccessException("You must be an administrator to delete users.");
            }

            Console.WriteLine($"ApiService: Error deleting user: {responseContent}");
            throw new Exception($"Failed to delete user: {responseContent}");
        }

        // Note: There is no reset password endpoint in the API, so this would need to be implemented
        // on the server side. This is a placeholder implementation that will throw an error.
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            // This doesn't require authentication
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var requestData = new { Email = email };
            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/forgot-password", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Password reset request failed: {errorContent}");
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword, string confirmPassword)
        {
            // This doesn't require authentication
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var requestData = new
            {
                Email = email,
                Code = code,
                Password = newPassword,
                ConfirmPassword = confirmPassword
            };

            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Password reset failed: {errorContent}");
        }

        public async Task<UserDashboardDto> GetUserDashboardAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("user/dashboard");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDashboardDto>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be logged in to access your dashboard.");
            }

            throw new Exception($"Failed to get user dashboard: {response.ReasonPhrase}");
        }

        public async Task<List<QuizAttemptDetailDto>> GetUserHistoryAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("user/history");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<QuizAttemptDetailDto>>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be logged in to access your history.");
            }

            throw new Exception($"Failed to get user history: {response.ReasonPhrase}");
        }

        // Admin-specific methods that will only be called from admin UI
        public async Task<UserProfile> ResetUserPasswordAsync(string userId)
        {
            EnsureAuthenticated();

            Console.WriteLine($"Resetting password for user {userId}");

            var response = await _httpClient.PostAsync($"admin/users/{userId}/reset-password", null);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);

                // Show the temp password via Toast - in production you might want to display this more securely
                Toast.MakeText(_context, $"Temporary password: {result["tempPassword"]}", ToastLength.Long).Show();

                // Return the updated user
                return await GetUserByIdAsync(userId);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be an administrator to reset passwords.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to reset password: {errorContent}");
        }

        public async Task<byte[]> DownloadUserReportAsync()
        {
            EnsureAuthenticated();

            Console.WriteLine("Downloading user report");

            var response = await _httpClient.GetAsync("admin/users/report");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be an administrator to download user reports.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to download user report: {errorContent}");
        }

        // Enhanced update user method that supports all properties
        public async Task<UserProfile> UpdateUserFullAsync(string userId, UpdateUserFullRequest request)
        {
            EnsureAuthenticated();

            Console.WriteLine($"Updating user {userId} with full details");

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"admin/users/{userId}", content);

            if (response.IsSuccessStatusCode)
            {
                // Return the updated user
                return await GetUserByIdAsync(userId);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be an administrator to update users.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update user: {errorContent}");
        }

        // Method to bulk delete users
        public async Task<BulkDeleteResultDto> BulkDeleteUsersAsync(List<string> userIds)
        {
            EnsureAuthenticated();

            Console.WriteLine($"Bulk deleting {userIds.Count} users");

            var json = JsonConvert.SerializeObject(userIds);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("admin/users/bulk-delete", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BulkDeleteResultDto>(responseJson);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be an administrator to delete users.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to bulk delete users: {errorContent}");
        }

        public async Task<bool> DirectResetPasswordAsync(string email, string newPassword)
        {
            // This doesn't require authentication
            _httpClient.DefaultRequestHeaders.Authorization = null;

            Console.WriteLine($"Directly resetting password for {email}");

            // For simplicity, we'll use the ResetPasswordRequest but generate a dummy code
            // In a real implementation, you would create a dedicated endpoint on the server
            var dummyCode = "DIRECT_RESET_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            var requestData = new ResetPasswordRequest
            {
                Email = email,
                Code = dummyCode, // Server will ignore this and generate its own
                Password = newPassword,
                ConfirmPassword = newPassword
            };

            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Use the existing reset endpoint
                var response = await _httpClient.PostAsync("auth/reset-password", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Password reset success: {responseContent}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Password reset error: {errorContent}");

                    // Try to extract a meaningful error message
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            throw new Exception(errorResponse.Message);
                        }
                    }
                    catch (JsonException)
                    {
                        // If we can't parse the JSON, just use the raw content
                    }

                    throw new Exception($"Password reset failed: {errorContent}");
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception("The request timed out. Please check your internet connection and try again.");
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error while resetting password: {httpEx.Message}", httpEx);
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is ArgumentNullException))
            {
                Console.WriteLine($"Unexpected error in DirectResetPasswordAsync: {ex}");
                throw new Exception($"Error resetting password: {ex.Message}", ex);
            }
        }
    }
}