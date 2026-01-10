using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using Studio.Contracts.Services;
using Studio.Helpers;
using Studio.Models;
using Studio.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Controls;


namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for AccountDetailsPage.xaml
    /// </summary>
    public partial class AccountDetailsPage : Page
    {
        private readonly SnackbarService _snackbarService;
        private readonly BattleNetService _battleNetService;

        public ProfileV2 Profile { get; set; }


        public ISeries[] RankSeries { get; set; }
        public ICartesianAxis[] RankXAxes { get; set; }
        public ICartesianAxis[] RankYAxes { get; set; }


        public static ObservableCollection<double> Vals { get; set; } = new ObservableCollection<double> { 0, 0, 0, 0 };
        public PolarLineSeries<float>[] StatGraphSeries { get; set; }


        public PolarAxis[] RadiusAxes { get; set; }
        public PolarAxis[] AngleAxes { get; set; }

        static readonly Dictionary<Roles, SKColor> _roleColors = new()
        {
            {Roles.Tank, new SKColor(52, 152, 219) },
            {Roles.Damage, new SKColor(192, 57, 43) },
            {Roles.Support, new SKColor(46, 204, 113) },
        };

        public AccountDetailsPage(ProfileV2 profile)
        {
            InitializeComponent();
            _battleNetService = ((App)Application.Current).GetService<BattleNetService>();
            _snackbarService = ((App)Application.Current).GetService<SnackbarService>();

            DataContext = this;

            Profile = profile;

            SizeChanged += OnPageSizeChanged;
            InitRankGraph();
            InitStatGraph();
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // polar graph margin from -60..-30 based on width to account for margin gap being too large
            // on small window sizes
            double margin = Math.Clamp(-60 + ((StatPolarGraph.ActualWidth - 250.0) / 5.0), -60, -30);
            StatPolarGraph.Margin = new Thickness(margin);
        }

        private void InitRankGraph()
        {
            var uri = new Uri("pack://application:,,,/Resources/Fonts/Roboto-Regular-Ranks.ttf");
            var font = SKTypeface.FromStream(Application.GetResourceStream(uri).Stream);
            RankChart.TooltipTextPaint = new SolidColorPaint(SKColors.White)
            {
                SKTypeface = font,
            };
            RankChart.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(30, 30, 50, 200));
            RankYAxes = new Axis[]
            {
                new() {
                    MinStep = 500,
                    LabelsDensity = 0.09f,
                    LabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKTypeface = font,
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

            RankXAxes = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("M/d"))
            {
                //MinStep = timespan.TotalSeconds / 7.0,
                LabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke),
            }];

            if (Profile.Snapshots.Count == 0)
                return;

            var points = GetDateTimePoints(Profile.Snapshots);

            var series = new List<ISeries>();

            if (Profile.LatestSnapshot.Tank != null)
                series.Add(new RankLineSeries(points[Roles.Tank], _roleColors[Roles.Tank]));

            if (Profile.LatestSnapshot.Damage != null)
                series.Add(new RankLineSeries(points[Roles.Damage], _roleColors[Roles.Damage]));

            if (Profile.LatestSnapshot.Support != null)
                series.Add(new RankLineSeries(points[Roles.Support], _roleColors[Roles.Support]));

            RankSeries = series.ToArray();
            var oldestSnapshot = Profile.Snapshots.OrderByDescending(s => s.Timestamp).Last();
            var timespan = TimeSpan.FromSeconds(Math.Max(Profile.LatestSnapshot.Timestamp - oldestSnapshot.Timestamp, TimeSpan.FromDays(10).TotalSeconds));

            RankXAxes[0].UnitWidth = (timespan / 7.0).Ticks;
            RankXAxes[0].MinLimit = null;

        }


        private void InitStatGraph()
        {
            StatPolarGraph.TooltipTextPaint = new SolidColorPaint(SKColors.White);
            StatPolarGraph.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(30, 30, 50));


            AngleAxes = [
                new PolarAxis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
                    {
                        SKTypeface = SKTypeface.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Fonts/Hakuna-Sans.otf")).Stream)
                    },
                    TextSize = 15,
                    LabelsBackground = new LvcColor(0, 0, 0, 0),
                    Labels = ["Healing", "Elims", "Damage", "Deaths"],
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
                    CustomSeparators = [1],
                    MaxLimit = 1,
                    MinLimit = 0

                }
            ];


            StatGraphSeries = [
                new StatChartSeries(Profile.LatestSnapshot?.Tank),
                new StatChartSeries(Profile.LatestSnapshot?.Damage),
                new StatChartSeries(Profile.LatestSnapshot?.Support)
            ];


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
                YToolTipLabelFormatter = point =>
                {
                    int sr = (int)point.Model.Value;
                    var (tier, division) = RankV2.CalculateDiv(sr);
                    string icon = Char.ConvertFromUtf32(1 + Array.IndexOf(Enum.GetValues<Division>(), division));
                    return $"{icon} {tier}";
                };
                
            }
        }

        private class StatChartSeries : PolarLineSeries<float>
        {
            StatisticType[] statTypes = [
                            StatisticType.Healing,
                            StatisticType.Elims,
                            StatisticType.Damage,
                            StatisticType.Deaths,
                        ];
            public StatChartSeries(RoleV2 role)
            {
                if (role?.Stats == null)
                {
                    Values = [10, 10, 10, 10];
                    Fill = null;
                }
                else
                {
                    var stats = StatisticScaler.ScaleStatistics(role.Stats);
                    Values = statTypes.Select(s => role.Stats[s]).ToList();
                    //Values = role.Stats.Where(x => statTypes.Contains(x.Key)).Select(x => x.Value).ToList();
                    Mapping = (height, rad) => new Coordinate(rad, stats[statTypes[rad]]);
                    Fill = new SolidColorPaint(_roleColors[role.Type].WithAlpha(50));

                }
                GeometryFill = null;
                GeometryStroke = null;
                LineSmoothness = 0.2;
                Stroke = null;
                GeometrySize = 20; // hover radius
                IsClosed = true;
                RadiusToolTipLabelFormatter = point => point.Model.ToString();

            }
        }


        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void polar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //_snapshotIndex = (float)Math.Clamp(_snapshotIndex + e.Delta / 100.0, 0, Profile.Snapshots.Count - 1);

            //DisplayStats(snapshot);
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            await TryLaunchAccount(Profile, true);
        }

        // TODO: fix repeated code
        private async Task TryLaunchAccount(ProfileV2 profile, bool tryLaunchGame = false)
        {
            if (profile == null)
                return;
            if (profile.Email == null)
            {
                _ = _snackbarService.GetSnackbarPresenter().ImmediatelyDisplay(new Snackbar(_snackbarService.GetSnackbarPresenter())
                {
                    AllowDrop = false,
                    Appearance = ControlAppearance.Danger,
                    Title = "Couldn't Launch Account",
                    Content = "No email is associated with this account",
                    Icon = new SymbolIcon(SymbolRegular.ErrorCircle12),
                    Opacity = 0.9
                });
                return;
            }

            _ = _snackbarService.GetSnackbarPresenter().ImmediatelyDisplay(new Snackbar(_snackbarService.GetSnackbarPresenter())
            {
                AllowDrop = false,
                Appearance = ControlAppearance.Success,
                Title = "Switching Account!",
                Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35),
                Opacity = 0.9
            });

            _battleNetService.OpenBattleNetWithAccount(profile.Email);
            if (!tryLaunchGame)
                return;

            bool result = await _battleNetService.WaitForMainWindow();
            if (result)
            {
                _ = _snackbarService.GetSnackbarPresenter().ImmediatelyDisplay(new Snackbar(_snackbarService.GetSnackbarPresenter())
                {
                    AllowDrop = false,
                    Appearance = ControlAppearance.Success,
                    Title = "Launching Game!",
                    Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35),
                    Opacity = 0.9
                });

                _battleNetService.OpenBattleNet(true);
            }
            else
            {
                _ = _snackbarService.GetSnackbarPresenter().ImmediatelyDisplay(new Snackbar(_snackbarService.GetSnackbarPresenter())
                {
                    AllowDrop = false,
                    Appearance = ControlAppearance.Danger,
                    Title = "Couldn't Launch Overwatch",
                    Content = "Timed out waiting for Battle.net to load",
                    Icon = new SymbolIcon(SymbolRegular.ErrorCircle12),
                    Opacity = 0.9
                });
            }
        }

        private void OnHomeButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AccountListPage());
        }


    }

}
