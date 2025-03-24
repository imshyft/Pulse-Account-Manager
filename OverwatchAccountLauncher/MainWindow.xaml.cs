using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Studio.Models;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;



namespace OverwatchAccountLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        // Create New User And Save Data To File
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BattleTag battleTag = new BattleTag("FstAsFoxGirl#1143");

            string battleTagUrl = battleTag.ToString().Replace("#", "-");
            string url = $"https://overwatch.blizzard.com/en-us/career/{battleTagUrl}";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");

            HttpResponseMessage response = await client.GetAsync(url);

            string htmlContent = await response.Content.ReadAsStringAsync();


            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);


            var document = await context.OpenAsync(req => req.Content(htmlContent));

            string username = document.QuerySelector(".Profile-player--name")?.TextContent ?? "Unknown";

            


            UserData data = new UserData();
            //data.Username = document.QuerySelector(".Profile-player--name")?.TextContent ?? "Unknown";
            data.Avatar = document.QuerySelector<IHtmlImageElement>(".Profile-player--portrait")?.Source ??
                "https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png";
            data.TimesLaunched = 0;
            data.CustomId = battleTag.Username;
            data.RankedCareer = new RankedCareer();

            foreach (IElement roleElement in document.QuerySelectorAll(".Profile-playerSummary--roleWrapper"))
            {
                string role_img = roleElement.QuerySelector(".Profile-playerSummary--role")?.QuerySelector<IHtmlImageElement>("img")?.Source;
                if (role_img == null)
                    continue;

                string roleString = Regex.Match(role_img.Split("/").Last(), @"([^.]+)-")?.Groups[1].Value;
                if (string.IsNullOrEmpty(roleString))
                    continue;

                // account for where its stored as offense but the enum uses 'Damage' instead
                roleString = roleString.Replace("offense", "Damage");

                Role role;
                switch (roleString)
                {
                    case "offense":
                        data.RankedCareer.Damage = new Damage();
                        role = data.RankedCareer.Damage;
                        break;
                    case "tank":
                        data.RankedCareer.Tank = new Tank();
                        role = data.RankedCareer.Tank;
                        break;
                    case "support":
                        data.RankedCareer.Support = new Support();
                        role = data.RankedCareer.Support;
                        break;
                    default:
                        continue;
                }



                var imageElements = roleElement.QuerySelector(".Profile-playerSummary--rankImageWrapper")?.QuerySelectorAll<IHtmlImageElement>("img").ToList();
                string divisionSource = imageElements[0].Source;
                string tierSource = imageElements[1].Source;

                if (string.IsNullOrEmpty(tierSource) || string.IsNullOrEmpty(divisionSource))
                    continue;

                string divisionMatch = Regex.Match(divisionSource, @"_([^_-]+)-")?.Groups[1].Value;
                string tierMatch = Regex.Match(tierSource, @"_(\d+)-")?.Groups[1].Value;

                if (string.IsNullOrEmpty(tierMatch) || string.IsNullOrEmpty(divisionMatch))
                    continue;

                if (!int.TryParse(tierMatch, out int tier))
                    continue;

                Rank currentRank = Rank.RankFromDivision(divisionMatch.Remove(divisionMatch.Length - 4), tier);
                RankMoment rankMoment = new()
                {
                    Rank = currentRank,
                    Date = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                role.PeakRank = rankMoment;
                role.CurrentRank = currentRank;
                role.RankMoments = [rankMoment];

                
            }
        }


        // Switch First Account
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }
    }
}