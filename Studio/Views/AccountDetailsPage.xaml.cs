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
    
    // TODO : like all of the ui
    public partial class AccountDetailsPage : Page
    {
        public UserData Profile { get; set; }
        public ISeries[] Series { get; set; }
        public ICartesianAxis[] XAxes { get; set; }
        public AccountDetailsPage(UserData profile)
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnLoaded;

            Profile = profile;

            List<DateTimePoint> tankDateTimePoints = GetDateTimePoints(Profile.RankedCareer.Tank);
            List<DateTimePoint> damageDateTimePoints = GetDateTimePoints(Profile.RankedCareer.Damage);
            List<DateTimePoint> supportDateTimePoints = GetDateTimePoints(Profile.RankedCareer.Support);

            LineSeries<DateTimePoint> lineFormat = new()
            {
                Fill = null,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.CornflowerBlue,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeThickness = 6,
                },
            };

            ISeries[] series =
            {
                new RankLineSeries(tankDateTimePoints, SKColors.LightSkyBlue),
                new RankLineSeries(damageDateTimePoints, SKColors.IndianRed),
                new RankLineSeries(supportDateTimePoints, SKColors.LightGreen)
            };
            Series = series;

            XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(20), date => date.ToString("M/d")) };

            RankDisplay.Career = Profile.RankedCareer;

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


    }
}
