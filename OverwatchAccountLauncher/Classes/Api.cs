using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OverwatchAccountLauncher.Classes
{
    class Api
    {
        public static void ApiRequest(string accountid)
        {
            var client = new WebClient();
            string JsonString = client.DownloadString($"https://overfast-api.tekrop.fr/players/{accountid}/summary");
            ApiResponse response = JsonSerializer.Deserialize<ApiResponse>(JsonString)!;
            Console.WriteLine(response.username);
            Console.WriteLine(response.endorsement.level);
        }
    }


    public class ApiResponse
    {
        public string username;
        public string avatar;
        public string namecard;
        public string title;
        public Endorsement endorsement;
        public CompetitiveHistory competitive;
        public int last_updated_at;
    }

    public class CompetitiveHistory
    {
        public Platform pc;
        public Platform console;
    }

    public class Platform
    {
        public int season;
        public CompRole tank;
        public CompRole damage;
        public CompRole support;
        public CompRole open;
    }

    public class CompRole
    {
        public string division;
        public int tier;
        public string role_icon;
        public string rank_icon;
        public string tier_icon;
    }


    public class Endorsement
    {
        public int level;
        public string frame;
    }
}
