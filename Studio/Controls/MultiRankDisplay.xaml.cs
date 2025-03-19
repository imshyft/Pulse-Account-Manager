using System;
using System.Collections.Generic;
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
using Studio.Models;

namespace Studio.Controls
{
    /// <summary>
    /// Interaction logic for MultiRankDisplay.xaml
    /// </summary>
    public partial class MultiRankDisplay : UserControl
    {
        public MultiRankDisplay()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CareerProperty =
            DependencyProperty.Register(nameof(Career), typeof(RankedCareer), typeof(MultiRankDisplay),
                new PropertyMetadata(new RankedCareer(), OnRankPropertyChanged));

        public RankedCareer Career
        {
            get => (RankedCareer)GetValue(CareerProperty);
            set => SetValue(CareerProperty, value);
        }

        public static readonly DependencyProperty ShowRoleProperty =
            DependencyProperty.Register(nameof(ShowRoles), typeof(bool), typeof(MultiRankDisplay),
                new PropertyMetadata(false, OnShowRolePropertyChanged));

        public bool ShowRoles
        {
            get => (bool)GetValue(ShowRoleProperty);
            set => SetValue(ShowRoleProperty, value);
        }



        private static void OnShowRolePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiRankDisplay control)
            {
                bool showRoles = (bool)e.NewValue;


            }
        }

        private static void OnRankPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiRankDisplay control)
            {
                if (e.NewValue is RankedCareer career)
                {
                    control.TankDisplay.RankValue = career.Tank.CurrentRank;
                    control.SupportDisplay.RankValue = career.Support.CurrentRank;
                    control.DamageDisplay.RankValue = career.Damage.CurrentRank;
                }
            }
        }
    }
}
