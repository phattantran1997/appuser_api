using System;
namespace API_appusers.DTO.Response
{
    public class LoginResponse
    {
        public string username { get; set; }
        public string token { get; set; }
        public LoginResponse()
        {
        }
    }
}
