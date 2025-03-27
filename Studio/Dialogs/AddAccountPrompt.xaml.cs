using Studio.Services;
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
using System.Windows.Shell;
using Studio.Helpers;
using Studio.Models;
using Wpf.Ui.Controls;
using Studio.Services.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Wpf.Ui.Input;
using Studio.Contracts.Services;
using System.Windows.Media.Animation;

namespace Studio.Controls
{
    /// <summary>
    /// Interaction logic for AddAccountPrompt.xaml
    /// </summary>
    public partial class AddAccountPrompt : ContentDialog, INotifyPropertyChanged
    {
        private readonly IProfileFetchingService _profileFetchingService;
        private readonly BattleNetService _battleNetService;


        public Profile Profile { get; set; }
        public AddAccountPrompt(ContentPresenter contentPresenter)
        {
            InitializeComponent();
            DialogHost = contentPresenter;

            IsPrimaryButtonEnabled = false;
            PrimaryButtonText = "Add Account";

            CloseButtonText = "Cancel";

            DataContext = this;

            _profileFetchingService = ((App)Application.Current).GetService<IProfileFetchingService>();
            _battleNetService = ((App)Application.Current).GetService<BattleNetService>();

        }

        private string _infoText =
            "Do you want to manually enter the BattleTag of the account or try to automatically fetch it by reading the memory of battle.net? (experimental)";
        private bool _isSelectingMode = true;
        private bool _isManualEntry;
        private bool _isMemoryRead;
        private bool _isMemoryReadSuccessful;
        private bool _isAwaitingMemoryRead;
        private string _battleTagInput;
        private bool _isLoading;


        private InfoBarSeverity _infoBarSeverity;
        private string _infoBarTitle;
        private string _infoBarMessage;
        private bool _isInfoBarOpen;

        public string InfoText
        {
            get => _infoText;
            set
            {
                _infoText = value;
                OnPropertyChanged();
                var opacityAnimation = new DoubleAnimation
                {
                    To = 1, // Target Opacity
                    From = 0,
                    Duration = TimeSpan.FromSeconds(2),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                InformationText.BeginAnimation(OpacityProperty, opacityAnimation);
            }
        }

        public InfoBarSeverity InfoBarSeverity
        {
            get => _infoBarSeverity;
            set { _infoBarSeverity = value; OnPropertyChanged(); }
        }

        public string InfoBarTitle
        {
            get => _infoBarTitle;
            set { _infoBarTitle = value; OnPropertyChanged(); }
        }

        public string InfoBarMessage
        {
            get => _infoBarMessage;
            set { _infoBarMessage = value; OnPropertyChanged(); }
        }

        public bool IsInfoBarOpen
        {
            get => _isInfoBarOpen;
            set { _isInfoBarOpen = value; OnPropertyChanged(); }
        }
        public bool IsSelectingMode
        {
            get => _isSelectingMode;
            set { _isSelectingMode = value; OnPropertyChanged(); }
        }

        public bool IsManualEntry
        {
            get => _isManualEntry;
            set { _isManualEntry = value; OnPropertyChanged(); }
        }

        public bool IsMemoryRead
        {
            get => _isMemoryRead;
            set { _isMemoryRead = value; OnPropertyChanged(); }
        }

        public bool IsMemoryReadSuccessful
        {
            get => _isMemoryReadSuccessful;
            set { _isMemoryReadSuccessful = value; OnPropertyChanged(); }
        }
        public bool IsAwaitingMemoryRead
        {
            get => _isAwaitingMemoryRead;
            set { _isAwaitingMemoryRead = value; OnPropertyChanged(); }
        }



        public string BattleTagInput
        {
            get => _battleTagInput;
            set
            {
                _battleTagInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBattleTagValid));
            }
        }

        public bool IsBattleTagValid
        {
            get
            {
                if (!string.IsNullOrEmpty(BattleTagInput))
                {
                    string[] parts = BattleTagInput.Split("#");
                    if (parts.Length == 2)
                        return true;
                }

                return false;

            }
        }


        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }


        public ICommand SelectManualEntryCommand => new RelayCommand<object>(_ =>
        {
            IsManualEntry = true;
            IsMemoryRead = false;
            IsSelectingMode = false;

            InfoText =
                "Enter the Battle tag of the account you will log into. Once Battle.net is launched, log into the account and then continue.";

        });

        public ICommand SelectMemoryReadCommand => new RelayCommand<object>(async _ =>
        {
            CloseInfoBar();
            IsAwaitingMemoryRead = true;
            IsMemoryRead = true;
            IsManualEntry = false;
            IsSelectingMode = false;



            BattleTag battleTag = _battleNetService.ReadBattleTagFromMemory();

            if (battleTag != null)
            {
                IsMemoryReadSuccessful = true;
                IsAwaitingMemoryRead = false;
                AutoBattleTagResultTextBox.Text = battleTag.ToString();
                InfoText = "Now that we've found the account, wait for us to get the profile data and then confirm";
                IsLoading = true;
                var result = await _profileFetchingService.FetchProfileAsync(battleTag);
                IsLoading = false;
                switch (result.Outcome)
                {
                    case ProfileFetchOutcome.NotFound:
                        ShowError(
                            "Profile could not be found",
                            "We couldn't retrieve the details of the account, but you can still switch to it. Maybe the profile was private. "
                            );
                        break;
                    case ProfileFetchOutcome.Success:
                        ShowSuccess("Profile Found", $"Found the profile at {result.Profile.Battletag}");
                        break;
                    case ProfileFetchOutcome.Error:
                        ShowError("Unexpected Error Occurred", $"You can still swap to it, but we couldn't get the info. Error [{result.ErrorMessage}]");
                        break;
                }

                Profile = result.Profile;
                
                IsPrimaryButtonEnabled = true;
            }
            else
            {
                ShowError("Couldn't read BattleTag", "We couldn't read the BattleTag. Make sure the Battle.net has completely opened and try again or manually enter it.");
            }
        });

        public ICommand LaunchBattleNetCommand => new RelayCommand<object>(async _ =>
        {
            _battleNetService.OpenBattleNetWithEmptyAccount();
            InfoText = "Great! Now while we fetch the account details, log in to the same account through Battle.net, and once its fully loaded press Add Account";
            BattleTag battleTag = new BattleTag(BattleTagInput);
            var result = await _profileFetchingService.FetchProfileAsync(battleTag);

            switch (result.Outcome)
            {
                case ProfileFetchOutcome.NotFound:
                    ShowError(
                        "Profile could not be found",
                        "We couldn't retrieve the details of the account, but you can still switch to it. Maybe the battletag was incorrect or the profile was private. "
                        );
                    break;
                case ProfileFetchOutcome.Success:
                    ShowSuccess("Profile Found", $"Found the profile at {result.Profile.Battletag}. Now just log in through Battle.net to register the email and confirm");
                    break;
                case ProfileFetchOutcome.Error:
                    ShowError("Unexpected Error Occurred", $"You can still swap to it, but we couldn't get the info. Error [{result.ErrorMessage}]");
                    break;
            }

            Profile = result.Profile;
            IsPrimaryButtonEnabled = true;

        });

        public ICommand RetryMemoryReadCommand => new RelayCommand<object>(async _ =>
        {
            SelectMemoryReadCommand.Execute(null);
        });

        public ICommand CancelMemoryReadCommand => new RelayCommand<object>(_ =>
        {
            SelectManualEntryCommand.Execute(null);
        });

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void ShowError(string title, string message)
        {
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = title;
            InfoBarMessage = message;
            IsInfoBarOpen = true;
        }

        public void ShowSuccess(string title, string message)
        {
            InfoBarSeverity = InfoBarSeverity.Success;
            InfoBarTitle = title;
            InfoBarMessage = message;
            IsInfoBarOpen = true;
        }

        public void CloseInfoBar()
        {
            IsInfoBarOpen = false;
        }

        protected override void OnButtonClick(ContentDialogButton button)
        {
            if (button == ContentDialogButton.Primary && !IsPrimaryButtonEnabled)
                return;

            
            base.OnButtonClick(button);
        }
    }
}
