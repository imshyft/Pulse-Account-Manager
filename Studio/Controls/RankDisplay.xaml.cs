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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Studio.Models;
using Studio.Properties;

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
                new PropertyMetadata(null, OnRankValueChanged));

        public static readonly DependencyProperty ShowWideGroupDecoratorProperty =
            DependencyProperty.Register(nameof(ShowWideGroupDecorator), typeof(bool), typeof(RankDisplay),
                new PropertyMetadata(false, OnShowWideGroupChanged));

        public new static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(RankDisplay),
                new PropertyMetadata(Brushes.Transparent, OnBackgroundChanged));

        public static readonly DependencyProperty RoleProperty =
                DependencyProperty.Register(nameof(Role), typeof(Roles?), typeof(RankDisplay),
                    new PropertyMetadata(null, OnRoleChanged));

        public Rank RankValue
        {
            get => (Rank)GetValue(RankValueProperty);
            set => SetValue(RankValueProperty, value);
        }

        public bool ShowWideGroupDecorator
        {
            get => (bool)GetValue(ShowWideGroupDecoratorProperty);
            set => SetValue(ShowWideGroupDecoratorProperty, value);
        }
        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public Roles? Role
        {
            get => (Roles?)GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }

        private static void OnRankValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        private static void OnShowWideGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RankDisplay control)
            {
                bool showWide = control.ShowWideGroupDecorator;


                // Animate Opacity for Image
                var opacityAnimation = new DoubleAnimation
                {
                    To = showWide ? 1 : 0, // Target Opacity
                    Duration = TimeSpan.FromSeconds(0.1),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                control.WideGroupImage.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);

                // Toggle Visibility after fading out
                if (!showWide)
                {
                    opacityAnimation.Completed += (s, a) => control.WideGroupImage.Visibility = Visibility.Hidden;
                }
                else
                {
                    control.WideGroupImage.Visibility = Visibility.Visible;
                }


                // Animate Border Thickness
                var thicknessAnimation = new ThicknessAnimation
                {
                    To = showWide ? new Thickness(2) : new Thickness(0), // Target Thickness
                    Duration = TimeSpan.FromSeconds(0.3), // Duration of animation
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } // Smooth transition
                };

                control.Border.BeginAnimation(Border.BorderThicknessProperty, thicknessAnimation);
            }
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RankDisplay)d;

            control.Border.Background = (Brush)e.NewValue;
        }

        private static void OnRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RankDisplay)d;

            if (control.Role is null)
            {
                control.Visibility = Visibility.Hidden;
                return;
            }

            switch (control.Role)
            {
                case Roles.Tank:
                    control.RoleImage.Source = (ImageSource)App.Current.FindResource("TankDrawingImage");
                    control.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#303498db");
                    break;
                case Roles.Damage:
                    control.RoleImage.Source = (ImageSource)App.Current.FindResource("DamageDrawingImage");
                    control.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#30c0392b");
                    break;
                case Roles.Support:
                    control.RoleImage.Source = (ImageSource)App.Current.FindResource("SupportDrawingImage");
                    control.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#302ecc71");
                    break; 
                default:
                    control.Visibility = Visibility.Hidden;
                    control.Background = Brushes.Transparent;
                    break;
            }
        }

    }
}
