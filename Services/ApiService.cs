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
        private const string EmulatorBaseUrl = "http://10.0.2.2:5063/api/";

        // Your computer's actual local network IP address
        // This should match what you see in your ipconfig output for the WiFi adapter
        private const string PhysicalDeviceBaseUrl = "http://192.168.1.15:5063/api/";

        private string _token;

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
            try
            {
                var loginData = new LoginRequest { Email = email, Password = password };
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/login", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login Response Status: {response.StatusCode}");
                Console.WriteLine($"Login Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    SetAuthToken(authResponse.Token);
                    return authResponse;
                }
                else
                {
                    throw new Exception($"Login failed: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Exception: {ex.Message}");
                throw;
            }
        }

        public void SetAuthToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }



        public async Task<List<Quiz>> GetQuizzesAsync()
        {
            // Add detailed logging
            Console.WriteLine($"Getting Quizzes - Current Token: {_token}");
            Console.WriteLine($"Base Address: {_httpClient.BaseAddress}");

            // Ensure the Authorization header is set each time
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }

            try
            {
                var response = await _httpClient.GetAsync("quizzes");

                // Log full response details
                Console.WriteLine($"Response Status: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Quiz>>(responseContent);
                }

                // More detailed error handling
                throw new Exception($"Failed to get quizzes: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetQuizzesAsync Exception: {ex.Message}");
                throw;
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
            
            var json = JsonConvert.SerializeObject(submission);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("quizzes/take", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizResult>(responseJson);
            }
            
            throw new Exception($"Failed to submit quiz: {response.ReasonPhrase}");
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
            if (string.IsNullOrEmpty(_token))
            {
                throw new Exception("You must be logged in to access this resource");
            }
            // Optional: Add additional token validation if needed
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}