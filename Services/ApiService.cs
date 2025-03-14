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

namespace Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://192.168.1.15:5063/api/"; // This should be changed to your actual API URL
        private string _token;
        private readonly Context _context;

        public ApiService(Context context = null)
        {
            _context = context;
            var handler = new HttpClientHandler
            {
                // Allow self-signed certificates if needed
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new System.Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Set reasonable timeouts
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public void SetAuthToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            if (!IsNetworkAvailable())
            {
                throw new Exception("No internet connection available. Please check your network settings.");
            }

            var loginData = new LoginRequest { Email = email, Password = password };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Create a cancellation token that will timeout after 15 seconds
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
            {
                try
                {
                    var response = await _httpClient.PostAsync("auth/login", content, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                        SetAuthToken(authResponse.Token);
                        return authResponse;
                    }

                    throw new Exception($"Login failed: {response.StatusCode} - {response.ReasonPhrase}");
                }
                catch (TaskCanceledException)
                {
                    throw new Exception("Login request timed out. Please try again or check your connection.");
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception($"Network error: {ex.Message}");
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
        }
    }
}