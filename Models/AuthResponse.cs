using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public System.DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    // New models related to authentication
    public class ForgotPasswordRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("resetCode")]
        public string ResetCode { get; set; }

        [JsonProperty("resetUrl")]
        public string ResetUrl { get; set; }
    }

    public class ResetPasswordResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}