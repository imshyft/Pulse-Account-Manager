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

namespace OverwatchAccountLauncher.Classes
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
    }

    class JsonHandler
    {
        private static readonly Dictionary<string, int> _divisions = new Dictionary<string, int> { { "bronze", 1000 }, { "silver", 1500 }, { "gold", 2000 }, { "platinum", 2500 }, { "diamond", 3000 }, { "master", 3500 }, { "grandmaster", 4000 }, { "champion", 4500 } };
        public static ApiResponse DeserializeApiResponseJson(string json)
        {
            return JsonSerializer.Deserialize<ApiResponse>(json)!;
        }

        public static UserData DeserializeUserDataJson(string json)
        {
            return JsonSerializer.Deserialize<UserData>(json)!;
        }

        public static string LoadJsonFromFile(string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                string json = reader.ReadToEnd();
                return json;
            }
        }

        public static void WriteUserDataToFile(UserData data, string filepath)
        {
            File.WriteAllText(@filepath, JsonSerializer.Serialize<UserData>(data));
        }

        public static UserData CreateUserData(string username, int tag, string email)
        {
            
            ApiResponse response = Api.ApiRequest($"{username}-{tag}").Result;
            if (response == null)
            {
                return null;
            }

            UserData userData = new UserData();

            userData.CustomID = $"{username}#{tag}";
            userData.Username = username;
            Debug.WriteLine(userData.Username);
            userData.Tag = tag;
            userData.Email = email;

            userData.Last_update = response.last_updated_at;
            userData.Avatar = response.avatar;
            userData.Times_launched = 0;
            userData.Times_switched = 0;

            userData.Rank_history = new RankHistory();
            userData.Rank_history.Tank = new Dictionary<int, int>();
            userData.Rank_history.Damage = new Dictionary<int, int>();
            userData.Rank_history.Support = new Dictionary<int, int>();

            userData.Rank_history.Highest = new HighestRoles();
            userData.Rank_history.Highest.Tank = new Role();
            userData.Rank_history.Highest.Damage = new Role();
            userData.Rank_history.Highest.Support = new Role();

            userData.Rank_history.Highest.Tank.Rating = 0;
            userData.Rank_history.Highest.Tank.Date = 0;
            userData.Rank_history.Highest.Damage.Rating = 0;
            userData.Rank_history.Highest.Damage.Date = 0;
            userData.Rank_history.Highest.Support.Rating = 0;
            userData.Rank_history.Highest.Support.Date = 0;

            if (response.competitive.pc.tank != null)
            {
                int sr = RankToInt(response.competitive.pc.tank.division, response.competitive.pc.tank.tier);
                userData.Rank_history.Tank[response.last_updated_at] = sr;
                userData.Rank_history.Highest.Tank.Rating = sr;
                userData.Rank_history.Highest.Tank.Date = userData.Last_update;
            }
            if (response.competitive.pc.damage != null)
            {
                int sr = RankToInt(response.competitive.pc.damage.division, response.competitive.pc.damage.tier);
                userData.Rank_history.Damage[response.last_updated_at] = sr;
                userData.Rank_history.Highest.Damage.Rating = sr;
                userData.Rank_history.Highest.Damage.Date = userData.Last_update;
            }
            if (response.competitive.pc.support != null)
            {
                int sr = RankToInt(response.competitive.pc.support.division, response.competitive.pc.support.tier);
                userData.Rank_history.Support[response.last_updated_at] = sr;
                userData.Rank_history.Highest.Support.Rating = sr;
                userData.Rank_history.Highest.Support.Date = userData.Last_update;
            }
            return userData;
        }

        public static int RankToInt(string division, int tier)
        {
            return _divisions[division] + (500 - tier * 100);
        }
    }


    // Our user data format

    public class UserData
    {
        public string CustomID { get; set; }
        public string Username { get; set; }
        public int Tag { get; set; }
        public string Email { get; set; }
        public string Avatar {  get; set; }
        public int Last_update { get; set; }
        public int Times_switched {  get; set; }
        public int Times_launched {  get; set; }
        public RankHistory Rank_history { get; set; }


    }

    public class RankHistory
    {
        public Dictionary<int, int> Tank { get; set; }
        public Dictionary<int, int> Damage { get; set; }
        public Dictionary<int, int> Support { get; set; }
        public HighestRoles Highest { get; set; }

    }

    public class HighestRoles
    {
        public Role Tank { get; set; }
        public Role Damage { get; set; }
        public Role Support { get; set; }
    }

    public class Role
    {
        public int Rating { get; set; }
        public int Date { get; set; }
    }

    // Overfast Api Response

    public class ApiResponse
    {
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
