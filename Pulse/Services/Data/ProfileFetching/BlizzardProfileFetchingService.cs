using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using HarfBuzzSharp;
using LiveChartsCore.Kernel;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL;
using Studio.Contracts.Services;
using Studio.Helpers;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;


namespace Studio.Services.Data
{
    public class BlizzardProfileFetchingService : IProfileFetchingService
    {
        private IBrowsingContext _context;
        private Dictionary<string, Roles> _heroRoles = null;

        public BlizzardProfileFetchingService()
        {
            var config = Configuration.Default.WithDefaultLoader();
            _context = BrowsingContext.New(config);
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

        private async Task<ProfileFetchResult> SyncProfile(ProfileV2 profile)
        {
            string errorMessage = "";

            string battleTagUrl = profile.Battletag.FullTag.Replace("#", "-");
            string url = $"https://overwatch.blizzard.com/en-us/career/{battleTagUrl}";

            HttpResponseMessage response = await FetchURL(url);
            
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

            var document = await _context.OpenAsync(req => req.Content(htmlContent));


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

            // ensure hero roles are found before creating snapshot
            if (_heroRoles == null)
            {
                try
                {
                    _heroRoles = await GetHeroRoles();
                }
                catch (Exception ex)
                {
                    return new ProfileFetchResult()
                    {
                        Profile = profile,
                        Outcome = ProfileFetchOutcome.Error,
                        ErrorMessage = "Failed to fetch list of heroes"
                    };
                }
            }

            ProfileSnapshotV2 snapshot = GetSnapshot(document);
            profile.Snapshots.Add(snapshot);

            return new ProfileFetchResult()
            {
                Profile = profile,
                Outcome = ProfileFetchOutcome.Success,
            };
        }

        private ProfileSnapshotV2 GetSnapshot(IDocument document)
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

            // getting stats per role (competitive only)

            var competitiveStatsGroup = document.QuerySelector("blz-section.competitive-view");
            // match ids to heroes by options dropdown (skip first option 'all heroes')
            var playedHeroOptionElems = competitiveStatsGroup
                .QuerySelector("select[data-dropdown-id='hero-dropdown']")
                .QuerySelectorAll("option")
                .Skip(1);

            // initialise stats collection
            Dictionary<Roles, Dictionary<StatisticType, float>> roleStats = new();
            Dictionary<Roles, int> roleMatchesPlayed = new(); // used to calculate w/r

            foreach (var role in Enum.GetValues<Roles>())
            {
                roleMatchesPlayed[role] = 0;
                roleStats[role] = new();
                foreach (var stat in Enum.GetValues<StatisticType>())
                {
                    roleStats[role][stat] = 0f;
                }
            }


            foreach (var opt in playedHeroOptionElems)
            {
                string id = opt.GetAttribute("value");
                string heroName = opt.TextContent.ToLower();

                if (_heroRoles.ContainsKey(heroName))
                {
                    var role = _heroRoles[heroName];
                    roleMatchesPlayed[role] += AppendStatsFromCollection(competitiveStatsGroup, id, roleStats[role]);
                }
                
            }

            foreach (var role in Enum.GetValues<Roles>())
            {
                FormatStatsToAverages(roleStats[role], roleMatchesPlayed[role]);
                snapshot[role].Stats = roleStats[role];
            }

            return snapshot;
        }

        private static void FormatStatsToAverages(Dictionary<StatisticType, float> stats, float matchesPlayed)
        {
            float secondsPlayed = stats[StatisticType.TimePlayed];
            float tenMinutesPlayed = (float)Math.Max(0.01, secondsPlayed / 60 / 10);

            foreach (var stat in Enum.GetValues<StatisticType>())
            {
                if (stat != StatisticType.TimePlayed && stat != StatisticType.Winrate)
                {
                    stats[stat] /= tenMinutesPlayed;
                }
                if (stat == StatisticType.Winrate)
                    stats[stat] /= matchesPlayed;
            }
        }

        private int AppendStatsFromCollection(IElement statsCollectionsGroup, string id, Dictionary<StatisticType, float> stats)
        {
            static TimeSpan ParseFromExtendedHours(string input)
            {
                string[] parts = input.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || parts.Length > 3)
                    throw new FormatException("Input must be in 'mm:ss' or 'hh:mm:ss' format.");

                if (!int.TryParse(parts[parts.Length - 1], out int seconds) || seconds < 0)
                    throw new FormatException($"Invalid seconds. Must be a non-negative integer.");

                if (!int.TryParse(parts[parts.Length - 2], out int minutes) || minutes < 0)
                    throw new FormatException($"Invalid minutes. Must be a non-negative integer.");

                int hours = 0;
                if (parts.Length == 3)
                {
                    if (!int.TryParse(parts[parts.Length - 3], out hours) || hours < 0)
                        throw new FormatException($"Invalid hours. Must be a non-negative integer.");

                }

                return new TimeSpan(hours, minutes, seconds);

            }
            var collection = statsCollectionsGroup.QuerySelector($".stats-container.option-{id}");

            int matchesPlayed = 0;
            const System.Globalization.NumberStyles numberStyles = System.Globalization.NumberStyles.AllowThousands;
            foreach (var elem in collection.QuerySelectorAll(".stat-item"))
            {
                string statName = elem.Children[0].TextContent;
                string statValue = elem.Children[1].TextContent;
                switch (statName)
                {
                    case "Time Played":
                        stats[StatisticType.TimePlayed] += (int)ParseFromExtendedHours(statValue).TotalSeconds;
                        break;
                    case "Eliminations":
                        stats[StatisticType.Elims] += int.Parse(statValue, numberStyles);
                        break;
                    case "Hero Damage Done":
                        stats[StatisticType.Damage] += float.Parse(statValue, numberStyles);
                        break;
                    case "Healing Done":
                        stats[StatisticType.Healing] += int.Parse(statValue, numberStyles);
                        break;
                    case "Deaths":
                        stats[StatisticType.Deaths] += int.Parse(statValue, numberStyles);
                        break;
                    case "Hero Wins":
                        stats[StatisticType.Winrate] += int.Parse(statValue, numberStyles);
                        break;
                    case "Games Played":
                        matchesPlayed = int.Parse(statValue, numberStyles);
                        break;
                }
            }

            return matchesPlayed;

        }

        private async Task<Dictionary<string, Roles>?> GetHeroRoles()
        {
            var heroesPage = await FetchURL("https://overwatch.blizzard.com/en-us/heroes/");

            if (!heroesPage.IsSuccessStatusCode)
                throw new Exception();

            Dictionary<string, Roles> results = new();

            string htmlContent = await heroesPage.Content.ReadAsStringAsync();
            var document = await _context.OpenAsync(req => req.Content(htmlContent));

            var heroCardsCollection = document.QuerySelector(".heroCards");
            var heroCards = heroCardsCollection.QuerySelectorAll("a[slot='gallery-items']");
            foreach ( var card in heroCards )
            {
                string heroName = card.TextContent.ToLower();
                string dataRole = card.GetAttribute("data-role");
                switch (dataRole)
                {
                    case "tank":
                        results[heroName] = Roles.Tank;
                        break;
                    case "damage":
                        results[heroName] = Roles.Damage;
                        break;
                    case "support":
                        results[heroName] = Roles.Support;
                        break;
                }
            }

            return results;

        }

        private static async Task<HttpResponseMessage> FetchURL(string url)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");

            return await client.GetAsync(url);
        }
    }


}
