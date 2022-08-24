using System.Linq;
using System.Threading.Tasks;
using API_appusers.DTO.Request;
using DTO_PremierDucts;
using Microsoft.AspNetCore.Mvc;
using WebService.Entities;
using WebService.Service.impl;

namespace WebService.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {


        private readonly UserService userService;


        public UserController(AppuserDBContext appuserDBContext)
        {
            userService = new UserService(appuserDBContext);
        }

        [Authorize]
        [HttpGet("getOnlineUsers")]
        public async Task<ResponseData> GetOnlineUser(string jobday, string supervisorname)
        {
            string token = Request.Headers["Token"].FirstOrDefault()?.Split(" ").Last();
            return await userService.GetOnlineUser(jobday, supervisorname, token);


        }


        [Authorize]
        [HttpGet("getOfflineUsers")]
        public ResponseData GetOfflineUser()
        {


            return userService.GetOfflineUser();

        }

        [HttpGet("getUserForReport")]
        public ResponseData GetUserForReport()
        {

            return userService.GetUserForReport();


        }
        [HttpPost("login")]
        public ResponseData login([FromBody] LoginRequest request)
        {
            return userService.login(request); ;
        }

      
    }
}
