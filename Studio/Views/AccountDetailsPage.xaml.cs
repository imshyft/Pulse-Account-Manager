using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using Studio.Helpers;
using Studio.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for AccountDetailsPage.xaml
    /// </summary>
    public partial class AccountDetailsPage : Page
    {
        public ProfileV2 Profile { get; set; }
        private float _snapshotIndex;


        public ISeries[] RankSeries { get; set; }
        public ICartesianAxis[] RankXAxes { get; set; }
        public ICartesianAxis[] RankYAxes { get; set; }


        // TODO: Web graph showing stats
        public static ObservableCollection<double> Vals { get; set; } = new ObservableCollection<double> { 0, 0, 0, 0 };
        public PolarLineSeries<double>[] StatGraphSeries { get; set; }


        public PolarAxis[] RadiusAxes { get; set; }
        public PolarAxis[] AngleAxes { get; set; }

        public double CurrentRegionLow => double.NaN;
        public double CurrentRegion { get; set; } = 1748864068 + TimeSpan.FromDays(10).TotalSeconds;

        private readonly Dictionary<Roles, SKColor> _roleColors = new()
        {
            {Roles.Tank, new SKColor(52, 152, 219) },
            {Roles.Damage, new SKColor(192, 57, 43) },
            {Roles.Support, new SKColor(46, 204, 113) },
        };

        public AccountDetailsPage(ProfileV2 profile)
        {
            InitializeComponent();
            //LiveCharts.Configure(c => c.AddLiveChartsRenderSettings());
            DataContext = this;
            Loaded += OnLoaded;

            Profile = profile;
            _snapshotIndex = Profile.Snapshots.Count - 1;

            RankChart.Background = Brushes.Transparent;
            StatPolarGraph.Background = Brushes.Transparent;

            if (Profile.Snapshots.Count == 0)
                return;

            InitRankGraph();
            InitStatGraph();
        }

        private void InitRankGraph()
        {
            var points = GetDateTimePoints(Profile.Snapshots);

            var series = new List<ISeries>();

            if (Profile.LatestSnapshot.Tank != null)
                series.Add(new RankLineSeries(points[Roles.Tank], _roleColors[Roles.Tank]));

            if (Profile.LatestSnapshot.Damage != null)
                series.Add(new RankLineSeries(points[Roles.Damage], _roleColors[Roles.Damage]));

            if (Profile.LatestSnapshot.Support != null)
                series.Add(new RankLineSeries(points[Roles.Support], _roleColors[Roles.Support]));

            //Chart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#DD2f2c3d"));
            //Chart.TooltipTextPaint = new SolidColorPaint(SKColors.White);
            RankSeries = series.ToArray();
            var oldestSnapshot = Profile.Snapshots.OrderByDescending(s => s.Timestamp).Last();
            var timespan = TimeSpan.FromSeconds(Profile.LatestSnapshot.Timestamp - oldestSnapshot.Timestamp);

            CurrentRegion = Profile.LatestSnapshot.Timestamp;

            RankXAxes = [new DateTimeAxis(timespan / 7.0, date => date.ToString("M/d"))
            {
                //MinStep = timespan.TotalSeconds / 7.0,
                LabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
            }];

            var uri = new Uri("pack://application:,,,/Resources/Fonts/Roboto-Regular-Ranks.ttf");
            RankYAxes = new Axis[]
            {
                new() {
                    MinStep = 500,
                    LabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKTypeface = SKTypeface.FromStream(Application.GetResourceStream(uri).Stream),
                    },
                    // Unicode 1 - 8 are rank symbols, map rank values to them
                    // TODO: Make graph tooltips not replace values with rank symbols
                    Labeler = (value) =>
                    {
                        int idx = Array.IndexOf(Enum.GetValues<Division>(), (Division)value);
                        if (idx != -1)
                            return Char.ConvertFromUtf32(1 + idx);
                        else if (value == 5000) // show separator line but not val for 5k
                            return "";
                        else
                            return Convert.ToString(value);
                    },
                    TextSize = 23,
                    CustomSeparators = [0, 1500, 2000, 2500, 3000, 3500, 3500, 4000, 4500, 5000],
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect([3, 3])
                    },

                }
            };
        }
        
        private void DisplayStats(ProfileSnapshotV2 snapshot)
        {

            foreach (Roles role in Enum.GetValues(typeof(Roles)))
            {
                if (snapshot[role].Stats == null)
                {
                    StatGraphSeries[(int)role].Values = [0, 0, 0, 0];
                    continue;
                }
                var stats = StatisticScaler.ScaleStatistics(snapshot[role].Stats);
                StatGraphSeries[(int)role].Values = [
                        stats.GetValueOrDefault(StatisticType.Damage, 0),
                        stats.GetValueOrDefault(StatisticType.Healing, 0),
                        stats.GetValueOrDefault(StatisticType.Deaths, 0),
                        stats.GetValueOrDefault(StatisticType.Elims, 0),
                    ];

            }
        }

        private void InitStatGraph()
        {
            StatGraphSeries = [
                new PolarLineSeries<double>
                {
                    Values = [],
                    Fill = new SolidColorPaint(new SKColor(52, 152, 219, 50)),
                    GeometryStroke = null,
                    GeometryFill = null,
                    LineSmoothness = 0.2,
                    Stroke = null,
                    IsClosed = true,
                },
                new PolarLineSeries<double>
                {
                    Values = [],
                    Fill = new SolidColorPaint(new SKColor(192, 57, 43, 50)),
                    GeometryStroke = null,
                    GeometryFill = null,
                    LineSmoothness = 0.2,
                    Stroke = null,
                    IsClosed = true,
                },
                new PolarLineSeries<double>
                {
                    Values = [],
                    Fill = new SolidColorPaint(new SKColor(46, 204, 113, 50)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.2,
                    Stroke = null,
                    IsClosed = true,
                },
            ];

            var snapshot = Profile.LatestSnapshot;
            DisplayStats(snapshot);


            AngleAxes = [
                new PolarAxis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
                    {
                        SKTypeface = SKTypeface.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Fonts/Hakuna-Sans.otf")).Stream)
                    },
                    TextSize = 20,
                    LabelsBackground = new LvcColor(0, 0, 0, 0),
                    Labels = ["Damage", "Healing", "Deaths", "Elims"],
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 1,
                        PathEffect = new DashEffect([3, 3])
                    },
                    LabelsRotation = LiveCharts.TangentAngle,
                }
            ];

            RadiusAxes = [
                new PolarAxis
                {
                    TextSize = 10,
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    LabelsBackground = new LvcColor(0, 0, 0, 0),
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
                    CustomSeparators = [1, 3, 5],
                    MaxLimit = 1,
                    MinLimit = 0

                }
            ];

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            return;
        }

        private Dictionary<Roles, List<DateTimePoint>> GetDateTimePoints(List<ProfileSnapshotV2> snapshots)
        {
            var result = new Dictionary<Roles, List<DateTimePoint>>()
            {
                {Roles.Tank, [] },
                {Roles.Damage, [] },
                {Roles.Support, [] },
            };

            foreach (var snapshot in snapshots)
            {
                int time = snapshot.Timestamp;
                if (snapshot.Tank.Rank != null)
                    result[Roles.Tank].Add(new DateTimePoint()
                    {
                        DateTime = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
                        Value = snapshot.Tank.Rank.SkillRating
                    });
                if (snapshot.Damage.Rank != null)
                    result[Roles.Damage].Add(new DateTimePoint()
                    {
                        DateTime = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
                        Value = snapshot.Damage.Rank.SkillRating
                    });

                if (snapshot.Support.Rank != null)
                    result[Roles.Support].Add(new DateTimePoint()
                    {
                        DateTime = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
                        Value = snapshot.Support.Rank.SkillRating
                    });
            }
            //return role.RankMoments
            //    .Select(m => new DateTimePoint()
            //    {
            //        DateTime = DateTimeOffset.FromUnixTimeSeconds(m.Date).DateTime,
            //        Value = m.Rank.SkillRating
            //    })
            //    .ToList();
            return result;

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
                    StrokeThickness = 2,
                };
                GeometryFill = new SolidColorPaint
                {
                    Color = strokeColor,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeThickness = 2,
                    
                };
                GeometryStroke = null;
                YToolTipLabelFormatter = point => point.Model.Value.ToString();
            }
        }




        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AccountListPage());
        }

        private void polar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //_snapshotIndex = (float)Math.Clamp(_snapshotIndex + e.Delta / 100.0, 0, Profile.Snapshots.Count - 1);

            //DisplayStats(snapshot);
        }
    }

}
