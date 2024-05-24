using System.Windows.Controls;

using ADTool.Contracts.Services;
using ADTool.ViewModels;
using ADTool.Views;

using CommunityToolkit.Mvvm.ComponentModel;

namespace ADTool.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();
    private readonly IServiceProvider _serviceProvider;

    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Configure<LAPSViewModel, LAPSPage>();
        Configure<Delete_DeviceViewModel, Delete_DevicePage>();
        Configure<Check_User_AccountViewModel, Check_User_AccountPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<Disable_Enable_AccountViewModel, Disable_Enable_AccountPage>();
        Configure<Enable_AccountViewModel, Enable_AccountPage>();
        Configure<Get_BitlockerViewModel, Get_BitlockerPage>();
        Configure<Add_Remove_User_GroupViewModel, Add_Remove_User_GroupPage>();
        Configure<DashboardViewModel, DashboardPage>();
        Configure<PrebuildComputersViewModel, PrebuildComputersPage>();
        Configure<NewUserViewModel, NewUserPage>();
        Configure<Enable_DeviceViewModel, Enable_DevicePage>();
        Configure<Add_SharedMailboxUserViewModel, Add_SharedMailboxUserPage>();
        Configure<Create_Shared_MailboxViewModel, Create_Shared_MailboxPage>();
    }

    public Type GetPageType(string key)
    {
        Type pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }



    public Page GetPage(string key)
    {
        var pageType = GetPageType(key);
        return _serviceProvider.GetService(pageType) as Page;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
