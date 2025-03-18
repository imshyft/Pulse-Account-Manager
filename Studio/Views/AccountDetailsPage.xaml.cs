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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveChartsCore;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Studio.Contracts.Views;
using Studio.Core.Models;


namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for AccountDetailsPage.xaml
    /// </summary>
    public partial class AccountDetailsPage : Page
    {
        public UserData Profile { get; set; }
        public ISeries[] Series { get; set; }
        public AccountDetailsPage(UserData profile)
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnLoaded;

            Profile = profile;

            ISeries[] series =
            {
                
                //new StackedAreaSeries<int>(Profile.TankRankHistory) {Name = "Tank", Fill = new SolidColorPaint(SKColors.LightSkyBlue)},
                //new StackedAreaSeries<int>(Profile.DamageRankHistory) {Name = "Damage", Fill = new SolidColorPaint(SKColors.IndianRed)},
                //new StackedAreaSeries<int>(Profile.SupportRankHistory) {Name = "Support", Fill = new SolidColorPaint(SKColors.MediumSeaGreen)}
            };
            Series = series;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            return;
        }

    }
}
