using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using Microsoft.Extensions.Azure;
using Microsoft.Graph.Models.Security;
using Microsoft.Graph.Security.Cases.EdiscoveryCases.Item.Searches.Item.LastEstimateStatisticsOperation;
using Microsoft.IdentityModel.Tokens;
using static ADTool.Services.EvelationService;

namespace ADTool.Services;

public class NavigationService : INavigationService
{
    private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
    private static string Usercreds = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.json");

    private readonly IPageService _pageService;
    private readonly IEvelationService _elevatorService;
    private readonly IADServices _adServices;
    private Frame _frame;
    private object _lastParameterUsed;

    public List<string> lastVisitedPageKey = new List<string> { "ADTool.ViewModels.DashboardViewModel" };
    bool Evelationtest;

    public event EventHandler<string> Navigated;

    public bool CanGoBack => _frame.CanGoBack;

    public NavigationService(IPageService pageService, IEvelationService elevatorService, IADServices adServices)
    {
        _pageService = pageService;
        _elevatorService = elevatorService;
        _adServices = adServices;
    }

    public void Initialize(Frame shellFrame)
    {
        if (_frame == null)
        {
            _frame = shellFrame;
            _frame.Navigated += OnNavigated;
        }
    }

    public List<string> GetOpendPage()
    {
        return lastVisitedPageKey;
    }

    public void UnsubscribeNavigation()
    {
        _frame.Navigated -= OnNavigated;
        _frame = null;
    }

    public void GoBack()
    {
        if (_frame.CanGoBack)
        {
            var vmBeforeNavigation = _frame.GetDataContext();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
            lastVisitedPageKey.Remove(lastVisitedPageKey.Last());
        }
    }

    public async Task<bool> NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        string pagekeyshort = null;

        string[] DisabledPages;

        AppConfigManager.TryLoadDisabledSites(AppConfPath, out DisabledPages);

        string originalString = pageKey;

        int lastIndex = originalString.LastIndexOf('.');

        if (lastIndex >= 0 && lastIndex < originalString.Length - 1)
        {
            pagekeyshort = originalString.Substring(lastIndex + 1);
        }
        if(pagekeyshort != null) { lastVisitedPageKey.Add(pagekeyshort); }


        int lastDotIndex = pageKey.LastIndexOf('.');

        if (lastDotIndex >= 0)
        {
            string pageKeyShort = pageKey.Substring(lastDotIndex + 1);
        }
        if (!DisabledPages.IsNullOrEmpty())
        { 
            if (DisabledPages.Contains(pagekeyshort))
            {
                Evelationtest = await _elevatorService.ElevationApplicationTab();
                if (Evelationtest)
                {
                    MessageBox.Show("This site is currently in maintenance!", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
        }

        string username;
        string password;

        UserCredentialManager.TryLoadUserCredentials(Usercreds, out username, out password);

        if(pagekeyshort == "Create_Shared_MailboxViewModel" || pagekeyshort == "Add_SharedMailboxUserViewModel")
        {
            bool sharedMailboxElevation = _adServices.IsElevatedforSharedMailboxes(username, "TotalToolAccessO365");

            if (!sharedMailboxElevation)
            {
                MessageBox.Show("You dont have permission to visit this site", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        

        if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            _frame.Tag = clearNavigation;
            var page = _pageService.GetPage(pageKey);
            var navigated = _frame.Navigate(page, parameter);
            if (navigated)
            {
                _lastParameterUsed = parameter;
                var dataContext = _frame.GetDataContext();
                if (dataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }
            return navigated;
        }

        return false;
    }

    public void CleanNavigation()
        => _frame.CleanNavigation();

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.CleanNavigation();
            }

            var dataContext = frame.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }

            Navigated?.Invoke(sender, dataContext.GetType().FullName);
        }
    }
}
