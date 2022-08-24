using System;
using System.ComponentModel.DataAnnotations;

namespace API_appusers.DTO.Request
{
    public class LoginRequest
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string password { get; set; }

        public string deviceId { get; set; }
    }
}
