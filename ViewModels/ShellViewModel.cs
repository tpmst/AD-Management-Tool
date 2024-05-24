using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using ADTool.Contracts.Services;
using ADTool.Properties;
using ADTool.Views;

using Azure;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADTool.ViewModels;

public class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IUserDataService _userDataService;
    private HamburgerMenuItem _selectedMenuItem;
    private HamburgerMenuItem _selectedOptionsMenuItem;
    private RelayCommand _goBackCommand;
    private ICommand _menuItemInvokedCommand;
    private ICommand _optionsMenuItemInvokedCommand;
    private ICommand _loadedCommand;
    private ICommand _unloadedCommand;

    private ICommand _navigateToDashboard;
    private ICommand _navigateToLAPS;
    private ICommand _navigateToEnableDevice;
    private ICommand _navigateToDeleteDevice;
    private ICommand _navigateToGetBitlocker;
    private ICommand _navigateToEnableUser;
    private ICommand _navigateToCheckUser;
    private ICommand _navigateToAddRemoveGroup;
    private ICommand _navigateToCreateUser;
    private ICommand _navigateToDeleteUser;
    private ICommand _navigateToSharedMailboxUser;
    private ICommand _navigateToOneDrivePro;

    public HamburgerMenuItem SelectedMenuItem
    {
        get { return _selectedMenuItem; }
        set { SetProperty(ref _selectedMenuItem, value); }
    }

    public HamburgerMenuItem SelectedOptionsMenuItem
    {
        get { return _selectedOptionsMenuItem; }
        set { SetProperty(ref _selectedOptionsMenuItem, value); }
    }
        
    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
    {
        new HamburgerMenuGlyphItem() { Label = Resources.ShellDashboardPage, Glyph = "\uE80F", TargetPageType = typeof(DashboardViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Get computerinfo", Glyph = "\uE8D7", TargetPageType = typeof(LAPSViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Delete device", Glyph = "\uE74D", TargetPageType = typeof(Delete_DeviceViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Check account", Glyph = "\uF78B", TargetPageType = typeof(Check_User_AccountViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Delete account", Glyph = "\uE72E", TargetPageType = typeof(Disable_Enable_AccountViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Enable account", Glyph = "\uE785", TargetPageType = typeof(Enable_AccountViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Groupmembership", Glyph = "\uE716", TargetPageType = typeof(Add_Remove_User_GroupViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Bitlocker", Glyph = "\uE730", TargetPageType = typeof(Get_BitlockerViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellNewUserPage, Glyph = "\uE8FA", TargetPageType = typeof(NewUserViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellEnable_DevicePage, Glyph = "\uE8A5", TargetPageType = typeof(Enable_DeviceViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellAdd_SharedMailboxUserPage, Glyph = "\uE8A5", TargetPageType = typeof(Add_SharedMailboxUserViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellCreate_Shared_MailboxPage, Glyph = "\uE8A5", TargetPageType = typeof(Create_Shared_MailboxViewModel) },
    };

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
    {
        new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
        
    };

    public RelayCommand GoBackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(OnGoBack, CanGoBack));

    public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ?? (_optionsMenuItemInvokedCommand = new RelayCommand(OnOptionsMenuItemInvoked));

    public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

    public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

    public ICommand NavigateToDashBoard => _navigateToDashboard ?? (_navigateToDashboard = new RelayCommand(onDashboard));

    public ICommand NavigateToLAPS => _navigateToLAPS ?? (_navigateToLAPS = new RelayCommand(onLAPS));

    public ICommand NavigateToEnableDevice => _navigateToEnableDevice ?? (_navigateToEnableDevice = new RelayCommand(onEnableDevice));
    public ICommand NavigateToDisableDevice => _navigateToDeleteDevice ?? (_navigateToDeleteDevice = new RelayCommand(onDelete_Device));
    public ICommand NavigateToGetBitlocker => _navigateToGetBitlocker ?? (_navigateToGetBitlocker = new RelayCommand(onGet_Bitlocker));
    public ICommand NavigateToEnableUser => _navigateToEnableUser ?? (_navigateToEnableUser = new RelayCommand(onEnalbe_Account));
    public ICommand NavigateToCheckUser => _navigateToCheckUser ?? (_navigateToCheckUser = new RelayCommand(onCheck_User));
    public ICommand NavigateToAddRemoveGroup => _navigateToAddRemoveGroup ?? (_navigateToAddRemoveGroup = new RelayCommand(onAdd_Remove));
    public ICommand NavigateToCreateUser => _navigateToCreateUser ?? (_navigateToCreateUser = new RelayCommand(onNewUser));
    public ICommand NavigateToDeleteUser => _navigateToDeleteUser ?? (_navigateToDeleteUser = new RelayCommand(onDeleteUser));
    public ICommand NavigateToSharedMailboxUser => _navigateToSharedMailboxUser ?? (_navigateToSharedMailboxUser = new RelayCommand(onAddSharedMailboxUser));
    public ICommand NavigateToOneDrivePro => _navigateToOneDrivePro ?? (_navigateToOneDrivePro = new RelayCommand(onOnedriveProvisioning));



    public ShellViewModel(INavigationService navigationService, IUserDataService userDataService)
    {
        _navigationService = navigationService;
        _userDataService = userDataService;
    }

    private void OnLoaded()
    {

        _navigationService.Navigated += OnNavigated;
        _userDataService.UserDataUpdated += OnUserDataUpdated;
            var user = _userDataService.GetUser();
            var userMenuItem = new HamburgerMenuImageItem()
            {
                Thumbnail = user.Photo,
                Label = user.Name,
                Command = new RelayCommand(OnUserItemSelected)
            };

            OptionMenuItems.Insert(0, userMenuItem);
        
    }

    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
        var userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
        if (userMenuItem != null)
        {
            OptionMenuItems.Remove(userMenuItem);
        }
    }

    private void OnUserDataUpdated(object sender, UserViewModel user)
    {
        var userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
        if (userMenuItem != null)
        {
            userMenuItem.Label = user.Name;
            userMenuItem.Thumbnail = user.Photo;
        }
    }


    private bool CanGoBack()
        => _navigationService.CanGoBack;

    private void OnGoBack()
        => _navigationService.GoBack();

    private void onDashboard()
        => _navigationService.NavigateTo("ADTool.ViewModels.DashboardViewModel");

    private void onLAPS()
        => _navigationService.NavigateTo("ADTool.ViewModels.LAPSViewModel");

    private void onEnableDevice()
        => _navigationService.NavigateTo("ADTool.ViewModels.Enable_DeviceViewModel");

    private void onDelete_Device()
        => _navigationService.NavigateTo("ADTool.ViewModels.Delete_DeviceViewModel");

    private void onEnalbe_Account()
        => _navigationService.NavigateTo("ADTool.ViewModels.Enable_AccountViewModel");

    private void onCheck_User()
        => _navigationService.NavigateTo("ADTool.ViewModels.Check_User_AccountViewModel");

    private void onAdd_Remove()
        => _navigationService.NavigateTo("ADTool.ViewModels.Add_Remove_User_GroupViewModel");

    private void onGet_Bitlocker()
      => _navigationService.NavigateTo("ADTool.ViewModels.Get_BitlockerViewModel");

    private void onNewUser()
      => _navigationService.NavigateTo("ADTool.ViewModels.NewUserViewModel");

    private void onDeleteUser()
      => _navigationService.NavigateTo("ADTool.ViewModels.Disable_Enable_AccountViewModel");

    private void onAddSharedMailboxUser()
    => _navigationService.NavigateTo("ADTool.ViewModels.Add_SharedMailboxUserViewModel");

    private void onOnedriveProvisioning()
    => _navigationService.NavigateTo("ADTool.ViewModels.Create_Shared_MailboxViewModel");




    private void OnOptionsMenuItemInvoked()
        => _navigationService.NavigateTo("ADTool.ViewModels.SettingsViewModel");

    private void OnUserItemSelected()
        => NavigateTo(typeof(SettingsViewModel));

    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel != null)
        {
            _navigationService.NavigateTo(targetViewModel.FullName);
        }
    }

    private void OnNavigated(object sender, string viewModelName)
    {
        var item = MenuItems
                    .OfType<HamburgerMenuItem>()
                    .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        if (item != null)
        {
            SelectedMenuItem = item;
        }
        else
        {
            SelectedOptionsMenuItem = OptionMenuItems
                    .OfType<HamburgerMenuItem>()
                    .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        }

        GoBackCommand.NotifyCanExecuteChanged();
    }
}
