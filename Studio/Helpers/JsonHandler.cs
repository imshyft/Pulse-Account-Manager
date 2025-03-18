using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Studio.Core.Models;

namespace OverwatchAccountLauncher.Classes
{
    class JsonHandler
    {
        private static readonly string[] _divs = { "bronze", "silver", "gold", "platinum", "diamond", "master", "grandmaster", "champion" };
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
            File.WriteAllText(filepath, JsonSerializer.Serialize(data));
        }

        public static UserData CreateUserData(string username, string tag, string? email)
        {

            ApiResponse response = Api.ApiRequest($"{username}-{tag}").Result;
            UserData userData = new UserData
            {
                CustomID = $"{username}#{tag}",
                Username = username,
                Tag = tag,

                Times_launched = 0,
                Times_switched = 0,

                RankedCareer = new RankedCareer(),

            };

            if (email != null)
                userData.Email = email;


            //userData.Rank_history.Tank = new Dictionary<int, int>();
            //userData.Rank_history.Damage = new Dictionary<int, int>();
            //userData.Rank_history.Support = new Dictionary<int, int>();

            //userData.Rank_history.Highest = new _HighestRoles();
            //userData.Rank_history.Highest.Tank = new _Role();
            //userData.Rank_history.Highest.Damage = new _Role();
            //userData.Rank_history.Highest.Support = new _Role();

            //userData.Rank_history.Highest.Tank.Rating = 0;
            //userData.Rank_history.Highest.Tank.Date = 0;
            //userData.Rank_history.Highest.Damage.Rating = 0;
            //userData.Rank_history.Highest.Damage.Date = 0;
            //userData.Rank_history.Highest.Support.Rating = 0;
            //userData.Rank_history.Highest.Support.Date = 0;

            if (response == null)
            {
                return userData;
            }

            userData.Last_update = response.last_updated_at;
            userData.Avatar = response.avatar;

            if (response.competitive.pc.tank != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.tank.division,
                    response.competitive.pc.tank.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.Last_update
                };

                userData.RankedCareer.Tank = new Tank()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };
                //int sr = RankToInt(response.competitive.pc.tank.division, response.competitive.pc.tank.tier);
                //userData.Rank_history.Tank[response.last_updated_at] = sr;
                //userData.Rank_history.Highest.Tank.Rating = sr;
                //userData.Rank_history.Current.Tank = sr;
                //userData.Rank_history.Highest.Tank.Date = userData.Last_update;
            }
            if (response.competitive.pc.damage != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.damage.division,
                    response.competitive.pc.damage.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.Last_update
                };

                userData.RankedCareer.Damage = new Damage()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };
                //int sr = RankToInt(response.competitive.pc.damage.division, response.competitive.pc.damage.tier);
                //userData.Rank_history.Damage[response.last_updated_at] = sr;
                //userData.Rank_history.Highest.Damage.Rating = sr;
                //userData.Rank_history.Current.Damage = sr;
                //userData.Rank_history.Highest.Damage.Date = userData.Last_update;
            }
            if (response.competitive.pc.support != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.support.division,
                    response.competitive.pc.support.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.Last_update
                };

                userData.RankedCareer.Support = new Support()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };
                //int sr = RankToInt(response.competitive.pc.support.division, response.competitive.pc.support.tier);
                //userData.Rank_history.Support[response.last_updated_at] = sr;
                //userData.Rank_history.Highest.Support.Rating = sr;
                //userData.Rank_history.Current.Support = sr;
                //userData.Rank_history.Highest.Support.Date = userData.Last_update;
            }
            return userData;
        }

        public static int RankToInt(string division, int tier)
        {
            return _divisions[division] + (500 - tier * 100);
        }

        public static string IntToRank(int sr)
        {
            double flr = ((double)sr - 1000) / 500;
            int x = (int)(flr - flr % 1);
            return $"{_divs[x]} {5 - (sr % 500) / 100}";
        }
    }



}
