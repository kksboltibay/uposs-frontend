using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UPOSS.Models;

namespace UPOSS.Services
{
    public class BranchAPIService
    {
        HttpClient _request = new HttpClient();

        private string _Path;
        public BranchAPIService()
        {
            _Path = "branch";
        }

        public async Task<RootBranchObject> BranchPostAPI(string apiCommand, object param)
        {
            dynamic requestBody = new { command = apiCommand, @params = param };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                string currentUsername = Properties.Settings.Default.CurrentUsername;
                string currentBranch = Properties.Settings.Default.CurrentBranch;

                var authToken = Encoding.ASCII.GetBytes($"{currentUsername}:{currentBranch}");
                _request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                var response = await _request.PostAsync("" + _Path, content);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                RootBranchObject responseObj = JsonConvert.DeserializeObject<RootBranchObject>(responseString);

                return responseObj;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);

                return new RootBranchObject { Status = "error", Msg = e.Message, Data = null };
            }
        }
    }
}
