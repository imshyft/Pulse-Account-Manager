using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Studio.Helpers;
using Studio.Models;

namespace Studio.Helpers
{
    class Api
    {
        public async static Task<ApiResponse> ApiRequest(string accountid)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                Debug.WriteLine($"https://overfast-api.tekrop.fr/players/{accountid}/summary");
                client.BaseAddress = new Uri($"https://overfast-api.tekrop.fr/players/{accountid}/summary");
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    ApiResponse json_response = JsonHandler.DeserializeApiResponseJson(result);
                    return json_response;
                }
                return null;
            }
        }

        public async static Task<dynamic> GetAccountStats(string accountid)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                Debug.WriteLine($"https://overfast-api.tekrop.fr/players/%7Baccountid%7D/stats/summary?gamemode=competitive");
                client.BaseAddress = new Uri($"https://overfast-api.tekrop.fr/players/%7Baccountid%7D/stats/summary?gamemode=competitive");
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic json_response = JsonConvert.DeserializeObject<dynamic>(result);
                    return json_response.roles;
                }
                return null;
            }
        }

    }

    // Overfast Api Response


    public class ProfileFetchResult
    {
        public UserData Profile { get; set; }
        public string Error { get; set; }
    }

    public enum ProfileFetchOutcome
    {
        Success,
        NotFound,
        Failure
    }

    public class ApiResponse
    {
        public string Error { get; set; } = "";
        public string username { get; set; }
        public string avatar { get; set; }
        public string namecard { get; set; }
        public string title { get; set; }
        public Endorsement endorsement { get; set; }
        public CompetitiveHistory competitive { get; set; }
        public int last_updated_at { get; set; }
    }

    public class CompetitiveHistory
    {
        public Platform pc { get; set; }
        public Platform console { get; set; }
    }

    public class Platform
    {
        public int season { get; set; }
        public CompRole? tank { get; set; }
        public CompRole? damage { get; set; }
        public CompRole? support { get; set; }
        public CompRole? open { get; set; }
    }

    public class CompRole
    {
        public string division { get; set; }
        public int tier { get; set; }
        public string role_icon { get; set; }
        public string rank_icon { get; set; }
        public string tier_icon { get; set; }
    }


    public class Endorsement
    {
        public int level { get; set; }
        public string frame { get; set; }
    }
}
