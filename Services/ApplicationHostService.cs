using ADTool.Contracts.Activation;
using ADTool.Contracts.Services;
using ADTool.Contracts.Views;
using ADTool.Core.Contracts.Services;
using ADTool.Helpers;
using ADTool.Models;
using ADTool.ViewModels;
using System.DirectoryServices;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ADTool.Views;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using Microsoft.Graph.Models;
using MessageBox = System.Windows.MessageBox;

namespace ADTool.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IIdentityService _identityService;
    private readonly IUserDataService _userDataService;
    private readonly ISharePointFileService _sharePointFileService;
    private readonly AppConfig _appConfig;
    public readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;
    private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
    private string UserCredsPath;
    private string _dom;

    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private IShellWindow _shellWindow;
    private ILogInWindow _logInWindow;
    private bool _isInitialized;
    

    public ApplicationHostService(IServiceProvider serviceProvider, IEnumerable<IActivationHandler> activationHandlers, INavigationService navigationService, IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService, IIdentityService identityService, IUserDataService userDataService, IOptions<AppConfig> config, ISharePointFileService sharePointFileService)
    {
        try
        {

            _serviceProvider = serviceProvider;
            _activationHandlers = activationHandlers;
            _navigationService = navigationService;
            _themeSelectorService = themeSelectorService;
            _persistAndRestoreService = persistAndRestoreService;
            _identityService = identityService;
            _userDataService = userDataService;
            _appConfig = config.Value;
            _sharePointFileService = sharePointFileService;
        }catch (Exception ex)
        {
            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00087", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Initialize services that you need before app activation
            await InitializeAsync();

            if (!_isInitialized)
            {
                _identityService.InitializeWithAadSingleOrg(_appConfig.IdentityClientId, "ClientID", "  https://domain.com/.auth/login/aad/callback"); //TODO: Enter ClientID and Application CallbackURL
            }

            var silentLoginSuccess = await _identityService.AcquireTokenSilentAsync();
            if (!silentLoginSuccess || !_identityService.IsAuthorized())
            {
                if (!_isInitialized)
                {
                    _logInWindow = _serviceProvider.GetService(typeof(ILogInWindow)) as ILogInWindow;
                    _logInWindow.ShowWindow();
                    await StartupAsync();
                    _isInitialized = true;
                }

                return;
            }

            await HandleActivationAsync();

            // Tasks after activation
            await StartupAsync();
            _isInitialized = true;
        }
        catch (Exception ex) 
        {
            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00089", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _persistAndRestoreService.PersistData();
        await Task.CompletedTask;
        _identityService.LoggedIn -= OnLoggedIn;
        _identityService.LoggedOut -= OnLoggedOut;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _persistAndRestoreService.RestoreData();
            _themeSelectorService.InitializeTheme();
            _userDataService.Initialize();
            _identityService.LoggedIn += OnLoggedIn;
            _identityService.LoggedOut += OnLoggedOut;
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

        if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            await _navigationService.NavigateTo(typeof(DashboardViewModel).FullName);
            await Task.CompletedTask;
        }
    }

    public static bool IsAuthenticated(string ldap, string usr, string pwd)
    {
        bool authenticated = false;

        try
        {
            DirectoryEntry root = new DirectoryEntry(ldap);
            string defaultNamingContext = root.Properties["defaultNamingContext"].Value.ToString();

            // Construct the LDAP path for the Users container in the local Active Directory
            string usersContainerPath = $"LDAP://{defaultNamingContext}";
            DirectoryEntry entry = new DirectoryEntry(usersContainerPath, usr, pwd);
            object nativeObject = entry.NativeObject;
            authenticated = true;
        }
        catch (DirectoryServicesCOMException cex)
        {
            authenticated = false;
            System.Windows.Forms.MessageBox.Show($"ErrorMassage: {cex.Message}", "Error: 0x00064");
        }
        catch (Exception ex)
        {
            authenticated = false;
            System.Windows.Forms.MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00063");
        }
        return authenticated;
    }


    private async void OnLoggedIn(object sender, EventArgs e)
    {
        string Paths = Path.Combine(_localAppData, "usercreds.json");
        string _username, _password;
        string usernameAndDomain;

        UserCredentialManager.TryLoadUserCredentials(Paths, out _username, out _password);

        if(File.Exists(AppConfPath))
        {
            AppConfigManager.TryLoaddom(AppConfPath, out _dom);
            usernameAndDomain = _dom + @"\" + _username;
        }
        else
        {
            CustomMessageBox customMessageBox = new CustomMessageBox("Put in your Domain name like (DOMAIN)");
            customMessageBox.ShowDialog();

            // Access user input after the dialog is closed
            string userInput = customMessageBox.UserInput;
            usernameAndDomain = userInput + @"\" + _username;
        }

        
        

        bool returnAuth = false;
        if (IsAuthenticated("LDAP://RootDSE", usernameAndDomain, _password))
        {
            returnAuth = true;
        }

        if (returnAuth == true)
        {
            await HandleActivationAsync();
            _logInWindow.CloseWindow();
        }
        else
        {

            System.Windows.Forms.MessageBox.Show($"ErrorMassage: Your Username " + _username + " or your Password is incorect", "Error: 0x00065", MessageBoxButtons.OK,MessageBoxIcon.Error) ;
            
        }
    }

    private void OnLoggedOut(object sender, EventArgs e)
    {
        _logInWindow = _serviceProvider.GetService(typeof(ILogInWindow)) as ILogInWindow;
        _logInWindow.ShowWindow();

        _shellWindow.CloseWindow();
        _navigationService.UnsubscribeNavigation();
        AppConfigManager.TryLoadUserCredentialsPath(AppConfPath, out UserCredsPath);
        //string Paths = Path.Combine(_localAppData, UserCredsPath);
        //File.Delete(Paths);
    }

}
