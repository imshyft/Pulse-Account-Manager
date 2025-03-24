using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Studio.Contracts.Views;
using Studio.Models;
using Visual = LiveChartsCore.VisualElements.Visual;


namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for AccountDetailsPage.xaml
    /// </summary>
    
    public partial class AccountDetailsPage : Page
    {
        public Profile Profile { get; set; }
        public ISeries[] Series { get; set; }
        public ICartesianAxis[] XAxes { get; set; }
        public AccountDetailsPage(Profile profile)
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnLoaded;

            Profile = profile;

            var series = new List<ISeries>();

            if (Profile.RankedCareer.Tank != null)
                series.Add(new RankLineSeries(GetDateTimePoints(Profile.RankedCareer.Tank), SKColors.LightSkyBlue));

            if (Profile.RankedCareer.Damage != null)
                series.Add(new RankLineSeries(GetDateTimePoints(Profile.RankedCareer.Damage), SKColors.IndianRed));

            if (Profile.RankedCareer.Support != null)
                series.Add(new RankLineSeries(GetDateTimePoints(Profile.RankedCareer.Support), SKColors.LightGreen));


            Series = series.ToArray();

            XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(20), date => date.ToString("M/d")) };

            //RankDisplay.Career = Profile.RankedCareer;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            return;
        }

        private List<DateTimePoint> GetDateTimePoints(Role role)
        {
            return role.RankMoments
                .Select(m => new DateTimePoint()
                {
                    DateTime = DateTimeOffset.FromUnixTimeSeconds(m.Date).DateTime,
                    Value = m.Rank.SkillRating
                })
                .ToList();
        }

        private class RankLineSeries : LineSeries<DateTimePoint>
        {
            public RankLineSeries(List<DateTimePoint> values, SKColor strokeColor)
            {
                Values = values;
                Fill = null;
                Stroke = new SolidColorPaint
                {
                    Color = strokeColor,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeThickness = 4,
                };
                GeometryStroke = new SolidColorPaint
                {
                    Color = strokeColor,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeThickness = 5,
                };
            }
        }


        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {

            NavigationService?.Navigate(new AccountListPage());

        }
    }
}
