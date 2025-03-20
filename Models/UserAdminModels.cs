using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    // Models for admin user creation and updating
    public class AdminCreateUserRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AdminUpdateUserRequest
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AdminResetPasswordRequest
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }
}