using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Windows.Input;

using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using ADTool.Core.Contracts.Services;
using ADTool.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Options;

namespace ADTool.ViewModels;

// Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly AppConfig _appConfig;
    private readonly IUserDataService _userDataService;
    private readonly IIdentityService _identityService;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ISystemService _systemService;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly ISharePointFileService _sharePointFileService;
    private AppTheme _theme;
    private string _versionDescription;
    private UserViewModel _user;
    private ICommand _setThemeCommand;
    private ICommand _privacyStatementCommand;
    private ICommand _logOutCommand;

    public AppTheme Theme
    {
        get { return _theme; }
        set { SetProperty(ref _theme, value); }
    }

    public string VersionDescription
    {
        get { return _versionDescription; }
        set { SetProperty(ref _versionDescription, value); }
    }

    public UserViewModel User
    {
        get { return _user; }
        set { SetProperty(ref _user, value); }
    }

    public ICommand SetThemeCommand => _setThemeCommand ?? (_setThemeCommand = new RelayCommand<string>(OnSetTheme));

    public ICommand PrivacyStatementCommand => _privacyStatementCommand ?? (_privacyStatementCommand = new RelayCommand(OnPrivacyStatement));

    public ICommand LogOutCommand => _logOutCommand ?? (_logOutCommand = new RelayCommand(OnLogOut));

    public SettingsViewModel(IOptions<AppConfig> appConfig, IThemeSelectorService themeSelectorService, ISystemService systemService, IApplicationInfoService applicationInfoService, IUserDataService userDataService, IIdentityService identityService, ISharePointFileService sharePointFileService)
    {
        _appConfig = appConfig.Value;
        _themeSelectorService = themeSelectorService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
        _userDataService = userDataService;
        _identityService = identityService;
        _sharePointFileService = sharePointFileService;
    }

    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme = _themeSelectorService.GetCurrentTheme();
        _identityService.LoggedOut += OnLoggedOut;
        _userDataService.UserDataUpdated += OnUserDataUpdated;
        User = _userDataService.GetUser();
    }



    public void OnNavigatedFrom()
    {
        UnregisterEvents();
    }

    private void UnregisterEvents()
    {
        _identityService.LoggedOut -= OnLoggedOut;
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
    }

    private void OnSetTheme(string themeName)
    {
        var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
        _themeSelectorService.SetTheme(theme);
    }

    private void OnPrivacyStatement()
        => _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);

    private async void OnLogOut()
    {
        string credsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.csv");
        if (File.Exists(credsPath))
        {
            File.Delete(credsPath);
        }

        await _identityService.LogoutAsync();
    }

    private void OnUserDataUpdated(object sender, UserViewModel userData)
    {
        User = userData;
    }

    private void OnLoggedOut(object sender, EventArgs e)
    {
        
        UnregisterEvents();
    }
}
