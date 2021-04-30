using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UPOSS.Models;

namespace UPOSS.Services
{
    public class UserAPIService
    {
        HttpClient _request = new HttpClient();

        private string _Path;

        public UserAPIService()
        {
            _Path = "user";
        }


        public async Task<RootUserObject> UserPostAPI(string apiCommand, object param)
        {
            dynamic requestBody = new { command = apiCommand, @params = param };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _request.PostAsync("" + _Path, content);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                RootUserObject responseObj = JsonConvert.DeserializeObject<RootUserObject>(responseString);

                if (responseObj.Data != null)
                {
                    foreach (User user in responseObj.Data)
                    {
                        switch (user.Role.ToString())
                        {
                            case "1":
                                user.Role = "Super Admin"; break;

                            case "2":
                                user.Role = "Admin"; break;

                            case "3":
                                user.Role = "Staff"; break;

                            default:
                                user.Role = "Unknown"; break;
                        }
                    }
                }

                return responseObj;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);

                return new RootUserObject { Status = "error", Msg = e.Message, Data = null };
            }
        }
    }
}
