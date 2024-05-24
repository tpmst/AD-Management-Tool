using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using ADTool.Models;
using ADTool.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ADTool.ViewModels;

public class Add_SharedMailboxUserViewModel : ObservableObject, INavigationAware
{
    private readonly ILogService _logService;
    private readonly IMicrosoftGraphService _MicrosoftGraphService;
    private readonly IIdentityService _identityService;
    private ICommand _navigateToDetailCommand;

    public ICommand NavigateToDetailCommand => _navigateToDetailCommand ?? (_navigateToDetailCommand = new RelayCommand<SharedMailbox>(NavigateToDetail));
    public ObservableCollection<SharedMailbox> Source { get; } = new ObservableCollection<SharedMailbox>();

    public Add_SharedMailboxUserViewModel(ILogService logService, IMicrosoftGraphService microsoftGraphService, IIdentityService identityService)
    {
        
        _logService = logService;
        _MicrosoftGraphService = microsoftGraphService;
        _identityService = identityService;
    }

    private async Task<IEnumerable<SharedMailbox>> DisabledUsersAll()
    {
        string accesstoken = await _identityService.GetAccessTokenForGraphAsync();
        List<SharedMailbox> build = await _MicrosoftGraphService.GetAllSharedMailboxes(accesstoken);
        return build;
    }

    public async Task<IEnumerable<SharedMailbox>> GetAllDisabledUsersAsync()
    {
        var sites = DisabledUsersAll();
        return await await Task.FromResult(sites);
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();
        // Replace this with your actual data
        var data = await GetAllDisabledUsersAsync();
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    private void NavigateToDetail(SharedMailbox disabledUsers)
    {
        MessageBoxResult result = MessageBox.Show($"Do you want enable the account {disabledUsers.DisplayName}?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            _logService.createLog("addUserToSharedMailbox", $"mailboxName:{disabledUsers.DisplayName}");
        }
    }

    public void OnNavigatedFrom()
    {
        throw new NotImplementedException();
    }
}
