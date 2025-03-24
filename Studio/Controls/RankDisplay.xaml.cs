using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Studio.Models;

namespace Studio.Controls
{
    /// <summary>
    /// Interaction logic for RankDisplay.xaml
    /// </summary>
    public partial class RankDisplay : UserControl
    {
        public RankDisplay()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RankValueProperty =
            DependencyProperty.Register(nameof(RankValue), typeof(Rank), typeof(RankDisplay),
                new PropertyMetadata(new Rank(), OnRankPropertyChanged));

        public Rank RankValue
        {
            get => (Rank)GetValue(RankValueProperty);
            set => SetValue(RankValueProperty, value);
        }

        private static void OnRankPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RankDisplay control)
            {
                Rank rank = control.RankValue;

                if (rank == null)
                {
                    control.DivisionImage.Source = null;
                    control.TierImage.Source = null;
                    return;
                }


                string rankDivision = Enum.GetName(rank.Division);
                string divisionImagePath = $"/Resources/RankIcons/Divisions/{rankDivision}.png";
                control.DivisionImage.Source = new BitmapImage(new Uri(divisionImagePath, UriKind.Relative));

                string tierImagePath = $"/Resources/RankIcons/Tiers/{rank.Tier}.png";
                control.TierImage.Source = new BitmapImage(new Uri(tierImagePath, UriKind.Relative));

            }
        }

    }
}
