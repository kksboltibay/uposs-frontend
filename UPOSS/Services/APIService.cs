using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UPOSS.Models;

namespace UPOSS.Services
{
    public class APIService
    {
        HttpClient _request = new HttpClient();
        private string _url;

        public APIService()
        {
            _url = "http://localhost:5000/api/v1/";
        }

        public async Task<dynamic> PostAPI(string apiCommand, object param, string _path)
        {
            dynamic requestBody = new { command = apiCommand, @params = param, path = _path };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                // current credential
                string currentUsername = Properties.Settings.Default.CurrentUsername;
                string currentBranch = Properties.Settings.Default.CurrentBranch;

                var authToken = Encoding.ASCII.GetBytes($"{currentUsername}:{currentBranch}");
                _request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                // post request
                var response = await _request.PostAsync(_url + _path, content);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                // return respond base on model
                dynamic responseObj;
                switch (_path)
                {
                    case "user":
                        responseObj = JsonConvert.DeserializeObject<RootUserObject>(responseString);

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
                        break;

                    case "branch":
                        responseObj = JsonConvert.DeserializeObject<RootBranchObject>(responseString); break;

                    case "product":
                        responseObj = JsonConvert.DeserializeObject<RootProductObject>(responseString); break;

                    //case "user":
                    //responseObj = JsonConvert.DeserializeObject<RootBranchObject>(responseString); break;

                    default:
                        responseObj = new { Status = "error", Msg = "API Service error, please contact IT support", Data = "" }; break;
                }

                return responseObj;
            }
            catch (HttpRequestException e)
            {
                System.Diagnostics.Trace.WriteLine("\nException Caught!");
                System.Diagnostics.Trace.WriteLine("Message :{0} ", e.Message);

                switch (_path)
                {
                    case "user":
                        return new RootUserObject { Status = "error", Msg = e.Message, Data = null };

                    case "branch":
                        return new RootBranchObject { Status = "error", Msg = e.Message, Data = null };

                    case "product":
                        return new RootProductObject { Status = "error", Msg = e.Message, Data = null };

                    //case "user":
                    //responseObj = JsonConvert.DeserializeObject<RootBranchObject>(responseString); break;

                    default:
                        return new { Status = "error", Msg = e.Message, Data = "" };
                }
            }
        }
    }
}
