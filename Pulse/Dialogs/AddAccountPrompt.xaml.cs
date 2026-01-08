using Studio.Contracts.Services;
using Studio.Helpers;
using Studio.Models;
using Studio.Models.Legacy;
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
using System.Windows.Shell;
using Wpf.Ui.Controls;
using Wpf.Ui.Input;

namespace Studio.Controls
{
    /// <summary>
    /// Interaction logic for AddAccountPrompt.xaml
    /// </summary>
    public partial class AddAccountPrompt : ContentDialog, INotifyPropertyChanged
    {
        private readonly IProfileFetchingService _profileFetchingService;
        private readonly BattleNetService _battleNetService;

        private CancellationTokenSource _memoryReadToken = new CancellationTokenSource();

        public ObservableCollection<BattleTagV2> MemoryBattletags { get; private set; } = [];

        public ProfileV2 Profile { get; set; }
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

            IsMemoryReadSuccessful = false;

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

        public bool IsAwaitingMemoryRead
        {
            get => _isAwaitingMemoryRead;
            set { _isAwaitingMemoryRead = value; OnPropertyChanged(); }
        }
        
        public bool IsMemoryReadSuccessful
        {
            get => _isMemoryReadSuccessful;
            set { _isMemoryReadSuccessful = value; OnPropertyChanged(); }
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
                    return BattleTagV2.IsBattleTagValid(BattleTagInput);
                }

                return false;

            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        /*
         * FLOW
         * SelectManualEntryCommand, SelectMemoryReadCommand ->
         * 
         */
        public ICommand SelectManualEntryCommand => new RelayCommand<object>(_ =>
        {
            IsManualEntry = true;
            IsMemoryRead = false;
            IsSelectingMode = false;

            InfoText =
                "Enter the Battle tag of the account you will log into. " +
                "Then choose whether to save the email (locally!) of the current account signed into battle net, or sign in to a different account.";

        });

        public ICommand SelectMemoryReadCommand => new RelayCommand<object>(async _ =>
        {
            CloseInfoBar();
            IsAwaitingMemoryRead = true;
            IsMemoryRead = true;
            IsManualEntry = false;
            IsSelectingMode = false;

            InfoText = "Please wait while the recent battletags are read from the Battle.Net executable, which you can then seach for";

            IsLoading = true;

            _memoryReadToken = new CancellationTokenSource();
            BattleTagV2[] battleTags = await _battleNetService.ReadBattleTagsFromMemory(_memoryReadToken.Token);

            IsLoading = false;
            IsAwaitingMemoryRead = false;

            if (_memoryReadToken.IsCancellationRequested)
                return;

            if (battleTags.Length == 0)
            {
                ShowError("Couldn't read BattleTag", "We couldn't read the BattleTag. Make sure the Battle.net has completely opened and try again or manually enter it.");
                return;
            }
            else
            {
                AccountNameAutoSuggestBox.OriginalItemsSource = battleTags;

                IsMemoryReadSuccessful = true;
                if (battleTags.Length == 1)
                {
                    AccountNameAutoSuggestBox.Text = battleTags[0].ToString();
                }
                InfoText = "Start typing to search recent accounts or enter a different battletag. " +
                "Then choose whether to save the email (locally!) of the current account signed into battle net, or sign in to a different account.";

            }
        });

        public ICommand LaunchBattleNetCommand => new RelayCommand<object>(async _ =>
        {
            _battleNetService.OpenBattleNetWithEmptyAccount();
            InfoText = "Great! Now while we fetch the account details, log in to the same account through Battle.net, and once its fully loaded press Add Account";
            await FetchProfile(BattleTagInput);
        });

        public ICommand GetCurrentAccountCommand => new RelayCommand<object>(async _ =>
        {
            _battleNetService.GetBattleNetAccount();
            InfoText = "Great! Wait while the account details are attempted to be fetched, and then confirm by pressing 'Add Account'";
            await FetchProfile(BattleTagInput);
            
        });

        private async Task FetchProfile(string battletag)
        {
            IsLoading = true;
            BattleTagV2 battleTag = new BattleTagV2(battletag);
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
            IsLoading = false;
            IsPrimaryButtonEnabled = true;
        }

        public ICommand RetryMemoryReadCommand => new RelayCommand<object>(async _ =>
        {
            _memoryReadToken.Cancel();
            SelectMemoryReadCommand.Execute(null);
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
