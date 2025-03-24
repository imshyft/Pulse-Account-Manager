using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services;
using Studio.Services.Data;
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
using Wpf.Ui.Controls;

namespace Studio.Dialogs
{
    /// <summary>
    /// Interaction logic for AddFavouriteProfilePrompt.xaml
    /// </summary>
    public partial class AddFavouriteProfilePrompt : ContentDialog
    {
        public bool IsBattleTagValid { get; set; }

        private readonly IProfileFetchingService _profileFetchingService;
        private readonly BattleNetService _battleNetService;


        public Profile Profile { get; set; }
        public AddFavouriteProfilePrompt(ContentPresenter contentPresenter)
        {
            InitializeComponent();
            DialogHost = contentPresenter;

            IsPrimaryButtonEnabled = false;
            PrimaryButtonText = "Add Favourite";

            CloseButtonText = "Cancel";

            DataContext = this;

            _profileFetchingService = ((App)Application.Current).GetService<IProfileFetchingService>();
            _battleNetService = ((App)Application.Current).GetService<BattleNetService>();
        }

        private void OnBattleTagInputTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = BattletagInputBox.Text;
            if (text != "")
            {
                string[] parts = text.Split("#");
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out _))
                    {
                        IsBattleTagValid = true;
                        LaunchBnetButton.IsEnabled = true;

                        Debug.WriteLine("Battletag Valid !");
                        InformationBar.IsOpen = false;
                        return;
                    }

                }
            }

            IsBattleTagValid = false;
            InformationBar.IsOpen = true;
            LaunchBnetButton.IsEnabled = false;
        }

        private async void OnLaunchBattlenetButtonClick(object sender, RoutedEventArgs e)
        {

            if (!IsBattleTagValid)
                return;

            BattletagInputBox.Visibility = Visibility.Collapsed;
            LaunchBnetButton.Visibility = Visibility.Collapsed;

            LaunchingBnetProgressBar.Visibility = Visibility.Visible;


            InformationText.Text =
                "Great! Now wait for us to find the account, and confirm";

            BattleTag battletag = new BattleTag(BattletagInputBox.Text);


            var result = await _profileFetchingService.GetUserProfile(battletag);

            LaunchingBnetProgressBar.Visibility = Visibility.Collapsed;
            InformationBar.IsOpen = true;

            switch (result.Error)
            {
                case "Not Found":
                    InformationBar.Title = "Profile Could not be found";
                    InformationBar.Severity = InfoBarSeverity.Warning;
                    InformationBar.Message =
                        "We couldn't retrieve the details of the account. Could it be a Private Profile?";
                    break;
                case "":
                    InformationBar.Title = "Profile found";
                    InformationBar.Severity = InfoBarSeverity.Success;
                    InformationBar.Message = $"Successfully found the profile at {result.Profile.Battletag}!";
                    IsPrimaryButtonEnabled = true;
                    break;
                default:
                    InformationBar.Title = "Couldn't contact the API";
                    InformationBar.Severity = InfoBarSeverity.Error;
                    InformationBar.Message =
                        $"We couldn't contact the servers at the moment. Try connecting again later. \nError Message: [{result.Error}]";
                    break;
            }

            InformationBar.IsOpen = true;

            Profile = result.Profile;

        }


    }
}
