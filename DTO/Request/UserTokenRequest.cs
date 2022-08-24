using System;
namespace API_appusers.DTO.Request
{
    public class UserTokenRequest
    {
        public string token { get; set; }
        public string deviceId { get; set; }
        public string username { get; set; }
    }
}
