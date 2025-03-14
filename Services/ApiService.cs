using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Mobile.Models;

namespace Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://192.168.1.15:5063/api/";
        private string _token;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetAuthToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var loginData = new LoginRequest { Email = email, Password = password };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                SetAuthToken(authResponse.Token);
                return authResponse;
            }
            
            throw new Exception($"Login failed: {response.ReasonPhrase}");
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

        private void EnsureAuthenticated()
        {
            if (string.IsNullOrEmpty(_token))
            {
                throw new Exception("You must be logged in to access this resource");
            }
        }
    }
}