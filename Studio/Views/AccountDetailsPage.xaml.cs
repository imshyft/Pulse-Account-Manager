using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using OpenTK.Audio.OpenAL.Extensions.Creative.EFX;
using SkiaSharp;
using Studio.Contracts.Views;
using Studio.Helpers;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ICartesianAxis[] YAxes { get; set; }


        // TODO: Web graph showing stats, scrolling zooms through time
        public static ObservableCollection<double> Vals { get; set; } = new ObservableCollection<double> { 5, 2, 1, 4, 3 };
        public ISeries[] S { get; set; }
            = new ISeries[]
            {
                new PolarLineSeries<double>
                {
                    Values = Vals,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.2,
                    Stroke = null,
                    IsClosed = true,
                },
            };
        public double TangentAngle { get; set; } =
            LiveCharts.TangentAngle;

        public PolarAxis[] RadiusAxes { get; set; }
            = new PolarAxis[]
            {
                        new PolarAxis
                        {
                            TextSize = 10,
                            LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                            LabelsBackground = new LvcColor(0, 0, 0, 0),
                            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
                            CustomSeparators = [1, 3, 5],
                            MaxLimit = 5,
                            MinLimit = 0
                            
                        }
            };

        public PolarAxis[] AngleAxes { get; set; }
            = new PolarAxis[]
            {
                new PolarAxis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
                    {
                        SKTypeface = SKTypeface.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Fonts/Hakuna-Sans.otf")).Stream)
                    },
                    TextSize = 20,
                    LabelsBackground = new LvcColor(0, 0, 0, 0),
                    Labels = ["Damage", "Healing", "Solo Kills", "Deaths", "Elims"],
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 1,
                        PathEffect = new DashEffect([3, 3])
                    },
                    LabelsRotation = LiveCharts.TangentAngle,
                }
            };

        public AccountDetailsPage(Profile profile)
        {
            InitializeComponent();
            LiveCharts.Configure(c => c.AddLiveChartsRenderSettings());
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
            
            //Chart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#DD2f2c3d"));
            //Chart.TooltipTextPaint = new SolidColorPaint(SKColors.White);
            Series = series.ToArray();
            XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(20), date => date.ToString("M/d")) };

            var uri = new Uri("pack://application:,,,/Resources/Fonts/Roboto-Regular-Ranks.ttf");
            YAxes = new Axis[]
            {
                new Axis
                {
                    MinStep = 500,
                    LabelsPaint = new SolidColorPaint(SKColors.White)
                    {
                        SKTypeface = SKTypeface.FromStream(Application.GetResourceStream(uri).Stream)
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
                    TextSize = 20,
                    CustomSeparators = new double[] { 0, 1500, 2000, 2500, 3000, 3500, 3500, 4000, 4500, 5000},
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    },
                    
                }
            };
            //RankDisplay.Career = Profile.RankedCareer;
            
            //polar.Tooltip = 

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
            for (int i = 0; i < Vals.Count; i++)
            {
                Vals[i] += e.Delta / 1000.0;
            }
        }
    }

    //public class CustomTooltip : SKDefaultTooltip
    //{
    //    public override void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
    //    {
            
    //        chart.GetPointsAt(chart. )
    //        base.Show(foundPoints, chart);
    //    }
    //}
}
