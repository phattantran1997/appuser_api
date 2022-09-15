using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API_appusers.DTO.Request;
using API_appusers.DTO.Response;
using DTO_PremierDucts;
using DTO_PremierDucts.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using WebService.DTO;
using WebService.Entities;
using WebService.Entities.response;
using WebService.Exceptions;

namespace WebService.Service.impl
{
    public class UserService

    {
        private static List<StationAttendes_jobtiming> onlineUserofSupervisor = new List<StationAttendes_jobtiming>();
        private static List<Operator> employees = new List<Operator>();
        public readonly AppuserDBContext appuserDBContext;
        private static List<Operator> operators = new List<Operator>();

        public UserService(AppuserDBContext appuser)
        {
            this.appuserDBContext = appuser;
        }

        //recursiveHierarchy
        public void CheckHierarchy(string managerUsername, int level)

        {

            List<Operator> group = new List<Operator>();

            group = operators.Where(x => x.Manager == managerUsername)
                                                    .ToList();

            foreach (Operator empDetails in group)

            {

                employees.Add(empDetails);


                CheckHierarchy(empDetails.Username, level + 1);
            }


        }

        public List<Operator> GetAllUsersHierarchy(string managerUsername)
        {

            employees.Clear();

            CheckHierarchy(managerUsername, 0);

            if (employees != null && employees.Count > 0)
                return employees;
            else
            {
                throw new ServiceException("Employees cannot be empty");
            }
              
        }

        public  async Task<ResponseData> GetOnlineUser(string jobday, string supervisorname, string token)
        {

            ResponseData responseData = new ResponseData();

            if(String.IsNullOrEmpty(token))
            {
                responseData.Code = ERROR_CODE.APPUSER_CANNOT_GET_TOKEN_REQUEST;
                responseData.Data = "TOKEN IS NULL OR EMPTY";
                return responseData;
            }

            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                // Pass the handler to httpclient(from you are calling api)
                HttpClient client = new HttpClient(clientHandler);

                client.DefaultRequestHeaders.Add("Token", token);


                //get all user
                operators = await appuserDBContext.Operators.ToListAsync();

                List<Operator> allUserFromSupervisor = new List<Operator>();
                if (operators != null && operators.Count > 0)
                {
                    allUserFromSupervisor = GetAllUsersHierarchy(supervisorname);
                }

                if (allUserFromSupervisor != null && allUserFromSupervisor.Count > 0)
                {


                    //get data from StationAttendes in database jobtimings for check online user.
                    String url1 = Startup.StaticConfig.GetSection("URLForPremierAPI").Value + "/user/getAllOnlineUser";
                    var response = client.GetAsync(url1).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync(); //Returns the response as JSON string
                        var allUserOnlineList = JsonConvert.DeserializeObject<List<StationAttendes_jobtiming>>(content.Result); //Converts JSON string to dynamic


                        //get user online from sup
                        onlineUserofSupervisor = (from useritem in allUserOnlineList
                                                  join employee in allUserFromSupervisor on useritem.username equals employee.Username into temptable
                                                  from tempItem in temptable.DefaultIfEmpty()
                                                  orderby useritem.stationNo
                                                  where tempItem != null
                                                  select new StationAttendes_jobtiming
                                                  {
                                                      name = tempItem.Name,
                                                      username = tempItem.Username
                                                  }).ToList();
                        if (onlineUserofSupervisor.Count > 0)
                        {
                            //call API from PREMIER - API
                            String url2 = Startup.StaticConfig.GetSection("URLForPremierAPI").Value + "/jobtiming/users/current";

                            GetLastestJobTimingsRequest request = new GetLastestJobTimingsRequest();
                            request.jobday = jobday;
                            request.users = onlineUserofSupervisor.Select(x => x.username).ToList();

                            var json = JsonConvert.SerializeObject(request);
                            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                            List<UserOnline> list_jobday = new List<UserOnline>();
                            var pos = await client.PostAsync(url2, stringContent);

                            if (pos.IsSuccessStatusCode)
                            {

                                var x = pos.Content.ReadAsAsync<ResponseData>().Result;
                                List<GetCurrentJobTimingsResponse> data =  JsonConvert.DeserializeObject<List<GetCurrentJobTimingsResponse>>(x.Data.ToString());
                                responseData.Code = ERROR_CODE.SUCCESS;
                                responseData.Data = data;

                            }
                            else
                            {
                                responseData.Code = ERROR_CODE.FAIL;
                                responseData.Data = pos.Content.ToString();
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                responseData.Code = ERROR_CODE.FAIL;
                responseData.Data = e.Message.ToString();
                return responseData;
            }

            return responseData;

        }


        public ResponseData GetOfflineUser()
        {
            ResponseData responseData = new ResponseData();
            try
            {
              

                var response = (from employee in employees

                            join user in onlineUserofSupervisor on employee.Username equals user.username into temp
                            from sub in temp.DefaultIfEmpty()
                            where sub == null
                            select new
                            {
                                Username = sub == null ? employee.Username : sub.username,
                                employee.Name,
                                employee.Company,
                                employee.Country
                            }).ToList();

                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = response;
            }
            catch
            {

                responseData.Code = ERROR_CODE.APPUSER_CANNOT_GET_OFFLINE_USER;
                responseData.Data = "CANNOT GET OFFLINE USER";
            }
          

            return responseData;


        }
        public ResponseData login(LoginRequest loginRequest)
        {

            ResponseData responseData = new ResponseData();
            Operator user = appuserDBContext.Operators.Where(i => i.Username == loginRequest.username &&
            i.Password.Equals(StringUtils.MD5Hash(loginRequest.password))).ToList().FirstOrDefault();

            if (user != null)
            {
                LoginResponse loginResponse = new LoginResponse();
                loginResponse.username = loginRequest.username;
                string new_token = generateJwtToken(user);

                User_Token currentToken = appuserDBContext.Token.Where(token => token.device.Equals(loginRequest.deviceId)).FirstOrDefault();
                if(currentToken == null)
                {
                    try
                    {
                        //save new record into user_token
                        appuserDBContext.Token.Add(new User_Token
                        {

                            device = loginRequest.deviceId,
                            token = new_token,
                            user = user.Username
                        });

                    }
                    catch
                    {
                        responseData.Code = ERROR_CODE.APPUSER_CANNOT_SAVE_TOKEN;
                        responseData.Data = "Cannot save new Token data";
                        return responseData;
                    }

                }
                else
                {
                    currentToken.token = new_token;
                }
                appuserDBContext.SaveChanges();
                loginResponse.token = new_token;

                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = loginResponse; 
                
            }
            else
            {
                responseData.Code = ERROR_CODE.APPUSER_WRONG_PASSWORD_USERNAME;
                responseData.Data = "Wrong username or password";
            }
            return responseData;
        }

        public ResponseData GetUserForReport()
        {
            ResponseData responseData = new ResponseData();
            try
            {
                string factory = Startup.StaticConfig.GetSection("Factory").Value;
                string company = Startup.StaticConfig.GetSection("Company").Value;
                string country = Startup.StaticConfig.GetSection("Country").Value;

                List<Operator> operators= appuserDBContext.Operators.Where(i => i.Factory == factory &&
                i.Company == company && i.Country == country && i.position == "Production" && i.Status == "active"
                && i.Role != "admin").ToList();

                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = operators;

            }
            catch
            {
                responseData.Code = ERROR_CODE.APPUSER_CANNOT_GET_USER_FOR_REPORT;
                responseData.Data = "CANNOT GET USER FOR REPORT";

            }

            return responseData;

        }

        private string generateJwtToken(Operator user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Startup.StaticConfig.GetSection("AppSettings:Secret").Value.ToString()));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("username", user.Username.ToString()),
                    new Claim("name", user.Name.ToString()),
                    new Claim("role", user.Role.ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(hmac.Key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }


}
