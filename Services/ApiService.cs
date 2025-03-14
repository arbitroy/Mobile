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

namespace Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly Context _context;

        // Special IP address for Android Emulator
        private const string EmulatorBaseUrl = "http://x.x.x.x:5063/api/";

        // Your computer's actual local network IP address
        // This should match what you see in your ipconfig output for the WiFi adapter
        private const string PhysicalDeviceBaseUrl = "http://x.x.x.x:5063/api/";

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

        public async Task<List<Quiz>> GetQuizzesAsync()
        {
            EnsureAuthenticated();

            var response = await _httpClient.GetAsync("quizzes");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Quiz>>(responseJson);
            }

            throw new Exception($"Failed to get quizzes: {response.ReasonPhrase}");
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
    }
}