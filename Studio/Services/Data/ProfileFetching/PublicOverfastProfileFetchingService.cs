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
using Newtonsoft.Json;
using Studio.Contracts.Services;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp;
using System.Text.RegularExpressions;

namespace Studio.Services.Data
{
    public class PublicOverfastProfileFetchingService : IProfileFetchingService
    {
        public PublicOverfastProfileFetchingService()
        {

        }

        private static string BattletagToWebFormat(BattleTag battletag) =>
            $"{battletag.Username}-{battletag.Tag}";


        private async Task<ApiResponse> FetchProfileFromOverfastAsync(BattleTag battletag)
        {
            string accountId = BattletagToWebFormat(battletag);
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");
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
                ApiResponse jsonResponse = JsonConvert.DeserializeObject<ApiResponse>(result);

                return jsonResponse;
            }
        }

        public async Task<ProfileFetchResult> FetchProfileAsync(BattleTag battletag)
        {
            ApiResponse response = await FetchProfileFromOverfastAsync(battletag);

            Profile userData = new Profile
            {
                Battletag = battletag,
                CustomId = battletag?.Username,

                TimesLaunched = 0,
                TimesSwitched = 0,

                RankedCareer = new RankedCareer(),

            };

            if (!string.IsNullOrEmpty(response.Error))
            {
                if (response.Error == "Not Found")
                    return new ProfileFetchResult()
                    {
                        ErrorMessage = response.Error,
                        Outcome = ProfileFetchOutcome.NotFound,
                        Profile = userData
                    };
                else
                    return new ProfileFetchResult()
                    {
                        ErrorMessage = response.Error,
                        Outcome = ProfileFetchOutcome.Error,
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
                Outcome = ProfileFetchOutcome.Success,
                Profile = userData
            };
        }
    }
    public class OverfastFetchResult
    {
        public Profile Profile { get; set; }
        public string Error { get; set; }
    }
}
