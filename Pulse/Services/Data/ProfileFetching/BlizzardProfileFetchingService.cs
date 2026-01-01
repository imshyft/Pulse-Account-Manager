using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using HarfBuzzSharp;
using LiveChartsCore.Kernel;
using Newtonsoft.Json;
using Studio.Contracts.Services;
using Studio.Helpers;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;


namespace Studio.Services.Data
{
    public class BlizzardProfileFetchingService : IProfileFetchingService
    {
        public BlizzardProfileFetchingService()
        {

        }

        public async Task<ProfileFetchResult> FetchProfileAsync(BattleTagV2 battleTag)
        {
            ProfileV2 profile = new();
            profile.Battletag = battleTag;
            profile.CustomName = battleTag.Username;
            profile.Snapshots = [];

            return await SyncProfile(profile);

        }

        public async Task<ProfileFetchResult> UpdateProfileAsync(ProfileV2 profile)
        {
            return await SyncProfile(profile);
        }

        private static async Task<ProfileFetchResult> SyncProfile(ProfileV2 profile)
        {
            string errorMessage = "";

            string battleTagUrl = profile.Battletag.FullTag.Replace("#", "-");
            string url = $"https://overwatch.blizzard.com/en-us/career/{battleTagUrl}";


            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");

            HttpResponseMessage response = await client.GetAsync(url);
            
            // create failed result
            if (response.StatusCode != HttpStatusCode.OK)
            {
                errorMessage = response.ReasonPhrase ?? "unexplained error.";

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ProfileFetchResult()
                    {
                        Profile = profile,
                        Outcome = ProfileFetchOutcome.NotFound,
                        ErrorMessage = errorMessage
                    };
                }


                return new ProfileFetchResult()
                {
                    Profile = profile,
                    Outcome = ProfileFetchOutcome.Error,
                    ErrorMessage = errorMessage
                };
            }

            string htmlContent = await response.Content.ReadAsStringAsync();

            // initialising angle-sharp document
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(htmlContent));


            if (document.QuerySelector(".error-contain") is not null)
            {
                return new ProfileFetchResult()
                {
                    Profile = profile,
                    Outcome = ProfileFetchOutcome.NotFound,
                };
            }

            profile.AvatarURL = document.QuerySelector<IHtmlImageElement>(".Profile-player--portrait")?.Source ??
                "https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png";
            

            ProfileSnapshotV2 snapshot = GetSnapshot(document);
            profile.Snapshots.Add(snapshot);

            return new ProfileFetchResult()
            {
                Profile = profile,
                Outcome = ProfileFetchOutcome.Success,
            };
        }

        private static ProfileSnapshotV2 GetSnapshot(IDocument document)
        {
            int unixNow = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var snapshot = new ProfileSnapshotV2()
            {
                Timestamp = unixNow,
            };

            // getting roles for each rank
            foreach (IElement roleElement in document.QuerySelectorAll(".Profile-playerSummary--roleWrapper"))
            {
                string? role_img = roleElement.QuerySelector(".Profile-playerSummary--role")?.QuerySelector<IHtmlImageElement>("img")?.Source;
                if (role_img == null)
                    continue;

                string? roleString = Regex.Match(role_img.Split("/").Last(), @"([^.]+)-")?.Groups[1].Value;
                if (string.IsNullOrEmpty(roleString))
                    continue;


                RoleV2 role;
                switch (roleString)
                {
                    case "offense":
                        role = snapshot.Damage;
                        break;
                    case "tank":
                        role = snapshot.Tank;
                        break;
                    case "support":
                        role = snapshot.Support;
                        break;
                    default:
                        continue;
                }

                // rank image sources
                var imageElements = roleElement.QuerySelector(".Profile-playerSummary--rankImageWrapper")?.QuerySelectorAll<IHtmlImageElement>("img").ToList();
                string? divisionSource = imageElements[0]?.Source;
                string? tierSource = imageElements[1]?.Source;

                if (string.IsNullOrEmpty(tierSource) || string.IsNullOrEmpty(divisionSource))
                    continue;

                // rank and division string names
                var divisionMatch = Regex.Match(divisionSource, @"_([^_-]+)-");
                var tierMatch = Regex.Match(tierSource, @"_(\d+)-");

                if (!tierMatch.Success || !divisionMatch.Success)
                    continue;

                string tierString = tierMatch.Groups[1].Value;
                string divisionString = divisionMatch.Groups[1].Value;

                if (string.IsNullOrEmpty(tierString) || string.IsNullOrEmpty(divisionString))
                    continue;

                if (!int.TryParse(tierString, out int tier))
                    continue;

                // remove 'Tier' from ending of division string
                divisionString = divisionString.Remove(divisionString.Length - 4);

                if (!Enum.TryParse(divisionString, true, out Division division))
                    throw new ArgumentException("Division was not an accepted string");

                if (tier < 1 || tier > 5)
                    throw new ArgumentException("Tier must be between 1 and 5");
                RankV2 currentRank = new RankV2(tier, division);

                role.Rank = currentRank;
            }

            return snapshot;
        }

    }


}
