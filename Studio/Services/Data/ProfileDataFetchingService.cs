using Studio.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using HarfBuzzSharp;
using Studio.Models;
using LiveChartsCore.Kernel;

namespace Studio.Services.Data
{
    public class ProfileDataFetchingService
    {
        public ProfileDataFetchingService()
        {

        }

        public static string BattletagToWebFormat(Battletag battletag) =>
            $"{battletag.Username}-{battletag.Tag}";

        private async Task<ApiResponse> FetchProfileFromApi(Battletag battletag)
        {
            string accountId = BattletagToWebFormat(battletag);
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                Debug.WriteLine($"https://overfast-api.tekrop.fr/players/{accountId}/summary");

                client.BaseAddress = new Uri($"https://overfast-api.tekrop.fr/players/{accountId}/summary");

                HttpResponseMessage response = await client.GetAsync("");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse()
                    {
                        Error = response.ReasonPhrase
                    };

                }


                string result = response.Content.ReadAsStringAsync().Result;
                ApiResponse jsonResponse = JsonHandler.DeserializeApiResponseJson(result);

                return jsonResponse;
            }
        }

        public async Task<ProfileFetchResult> GetUserProfile(Battletag battletag)
        {
            ApiResponse response = await FetchProfileFromApi(battletag);

            UserData userData = new UserData
            {
                Battletag = battletag,
                CustomId = battletag?.Username,
                //Username = username,
                //Tag = tag,

                TimesLaunched = 0,
                TimesSwitched = 0,

                RankedCareer = new RankedCareer(),

            };

            if (!string.IsNullOrEmpty(response.Error))
            {
                return new ProfileFetchResult()
                {
                    Error = response.Error,
                    Profile = userData
                };
            }

            userData.LastUpdate = response.last_updated_at;
            userData.Avatar = response.avatar;

            if (response.competitive.pc.tank != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.tank.division,
                    response.competitive.pc.tank.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.LastUpdate
                };

                userData.RankedCareer.Tank = new Tank()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };

            }
            if (response.competitive.pc.damage != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.damage.division,
                    response.competitive.pc.damage.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.LastUpdate
                };

                userData.RankedCareer.Damage = new Damage()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };

            }
            if (response.competitive.pc.support != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.support.division,
                    response.competitive.pc.support.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.LastUpdate
                };

                userData.RankedCareer.Support = new Support()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };
            }
            return new ProfileFetchResult()
            {
                Error = "",
                Profile = userData
            };
        }
    }
}
