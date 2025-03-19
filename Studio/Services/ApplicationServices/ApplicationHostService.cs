using Microsoft.Extensions.Hosting;
using Studio.Contracts.Activation;
using Studio.Contracts.Services;
using Studio.Contracts.Views;
using Studio.Services.Storage;
using Studio.Views;

namespace Studio.Services.ApplicationServices;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly UserProfileDataService _userProfileDataService;
    private readonly FavouriteProfileDataService _favouriteProfileDataService;
    private readonly ConfigStorageService _persistAndRestoreService;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private IShellWindow _shellWindow;
    private bool _isInitialized;

    public ApplicationHostService(IServiceProvider serviceProvider, 
        IEnumerable<IActivationHandler> activationHandlers, 
        INavigationService navigationService, 
        ConfigStorageService persistAndRestoreService,
        UserProfileDataService userProfileDataService,
        FavouriteProfileDataService favouriteProfileDataService)
    {
        _serviceProvider = serviceProvider;
        _activationHandlers = activationHandlers;
        _navigationService = navigationService;
        _persistAndRestoreService = persistAndRestoreService;

        _userProfileDataService = userProfileDataService;
        _favouriteProfileDataService = favouriteProfileDataService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize services that you need before app activation
        await InitializeAsync();

        await HandleActivationAsync();

        // Tasks after activation
        await StartupAsync();
        _isInitialized = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _persistAndRestoreService.PersistData();
        await Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _persistAndRestoreService.RestoreData();
            _userProfileDataService.LoadProfilesFromDisk();
            _favouriteProfileDataService.LoadProfilesFromDisk();
            await Task.CompletedTask;
        }
    }

    private async Task StartupAsync()
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private async Task HandleActivationAsync()
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle());

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync();
        }

        await Task.CompletedTask;

        if (System.Windows.Application.Current.Windows.OfType<IShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainPage));
            await Task.CompletedTask;
        }
    }
}
