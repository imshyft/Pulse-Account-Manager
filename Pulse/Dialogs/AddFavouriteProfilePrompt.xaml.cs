using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services;
using Studio.Services.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using Wpf.Ui.Controls;
using Wpf.Ui.Input;

namespace Studio.Dialogs
{
    /// <summary>
    /// Interaction logic for AddFavouriteProfilePrompt.xaml
    /// </summary>
    public partial class AddFavouriteProfilePrompt : ContentDialog, INotifyPropertyChanged
    {
        private readonly IProfileFetchingService _profileFetchingService;
        private readonly BattleNetService _battleNetService;


        public ProfileV2 Profile { get; set; }
        public AddFavouriteProfilePrompt(ContentPresenter contentPresenter)
        {
            InitializeComponent();
            DialogHost = contentPresenter;

            IsPrimaryButtonEnabled = false;
            PrimaryButtonText = "Add Account";

            CloseButtonText = "Cancel";

            DataContext = this;

            _profileFetchingService = ((App)Application.Current).GetService<IProfileFetchingService>();
            _battleNetService = ((App)Application.Current).GetService<BattleNetService>();
            MemoryBattleTags.Clear();

        }

        private string _infoText =
            "Do you want to manually enter the BattleTag of the profile to favourite or choose from your friends of your currently logged in Battle.net account (experimental: reads from memory of Battle.net, may take up to 30s on first scan)";
        private bool _isSelectingMode = true;
        private bool _isManualEntry;
        private bool _isMemoryRead;
        private bool _isMemoryReadSuccessful;
        private bool _isAwaitingMemoryRead;
        private string _battleTagManualInput;
        private bool _isLoading;
        


        private InfoBarSeverity _infoBarSeverity;
        private string _infoBarTitle;
        private string _infoBarMessage;
        private bool _isInfoBarOpen;

        public ObservableCollection<BattleTagV2> MemoryBattleTags { get; private set; } = new();

        public string InfoText
        {
            get => _infoText;
            set 
            { 
                _infoText = value; 
                OnPropertyChanged();
                var opacityAnimation = new DoubleAnimation
                {
                    To = 1,
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

        public string BattleTagManualInput
        {
            get => _battleTagManualInput;
            set
            {
                _battleTagManualInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBattleTagValid));
            }
        }



        public bool IsBattleTagValid
        {
            get
            {
                if (!string.IsNullOrEmpty(BattleTagManualInput))
                {
                    string[] parts = BattleTagManualInput.Split("#");
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
                "Enter the Battle tag of the profile you want to favourite and press Add. Once we have found the account, press confirm";

        });

        public ICommand SelectMemoryReadCommand => new RelayCommand<object>(async _ =>
        {
            CloseInfoBar(); 
            IsMemoryRead = true;
            IsManualEntry = false;
            IsSelectingMode = false;
            IsAwaitingMemoryRead = true;




            BattleTagV2[] battleTags = _battleNetService.ReadFriendsListFromMemory();
            if (battleTags.Length == 0)
            {
                ShowError("Could not find any friends", "if Battle.net wasn't opened, we just launched it so wait for it to load and try again");
                return;
            }
            else
            {
                InfoText =
                    "Your friends have been located from Battle.net. use the search bar to find who to add";
                foreach (var battleTag in battleTags)
                {
                    MemoryBattleTags.Add(battleTag);
                }
                
            }

            IsMemoryReadSuccessful = true;
            IsAwaitingMemoryRead = false;


        });

        public ICommand AddFriendFromMemory => new RelayCommand<object>(async _ => {
            
            string battletagString = AutoBattleNetSuggestBox.Text;

            bool inputIsFriend = false;
            foreach (var btag in MemoryBattleTags)
            {
                if (battletagString == btag.ToString())
                {
                    inputIsFriend = true;
                }
            }

            if (!inputIsFriend)
            {
                ShowError("Input was not valid friend", "The input wasn't one of the friends found. Maybe you left it empty or edited it");
                return;
            }
            IsLoading = true;
            await FetchProfile(battletagString);
            IsMemoryReadSuccessful = false;
            IsLoading = false;
        });

        public ICommand AddFriendManual => new RelayCommand<object>(async _ => {
            IsLoading = true;
            string battletagString = BattleTagManualInput;
            await FetchProfile(battletagString);
            IsManualEntry = false;
            IsLoading = false;
        });

        private async Task FetchProfile(string tag)
        {

            BattleTagV2 battleTag = new BattleTagV2(tag);
            var result = await _profileFetchingService.FetchProfileAsync(battleTag);

            switch (result.Outcome)
            {
                case ProfileFetchOutcome.NotFound:
                    ShowError(
                        "Profile could not be found",
                        "We couldn't find the profile. Maybe the profile is private, or the battletag was incorrect "
                        );
                    break;
                case ProfileFetchOutcome.Success:
                    ShowSuccess("Profile Found", $"Found the profile at {result.Profile.Battletag}");
                    break;
                case ProfileFetchOutcome.Error:
                    ShowError("Unexpected Error Occurred", $"We couldn't get the infornmation from the servers. Error [{result.ErrorMessage}]");
                    break;
            }

            Profile = result.Profile;
            IsPrimaryButtonEnabled = true;

        }

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
