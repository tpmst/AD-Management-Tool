using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using ADTool.Core.Contracts.Services;
using ADTool.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ADTool.ViewModels;

public class Enable_AccountViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(UserName))
            {
                searchUsername();
            }
        }

    }

    private string _userName;
    public string UserName
    {
        get { return _userName; }
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName)); // You should raise PropertyChanged here
            }
        }
    }

    private readonly INavigationService _navigationService;
    private readonly IADServices _ADservice;
    private readonly ILogService _logService;
    private ICommand _navigateToDetailCommand;
    private ICommand _seachfilter;

    public ICommand NavigateToDetailCommand => _navigateToDetailCommand ?? (_navigateToDetailCommand = new RelayCommand<DisabledUsers>(NavigateToDetail));
    public ICommand searchUsernameBind => _seachfilter ?? (_seachfilter = new RelayCommand(searchUsername));
    public ObservableCollection<DisabledUsers> Source { get; } = new ObservableCollection<DisabledUsers>();


    private async void searchUsername()
    {
        Source.Clear();
        // Replace this with your actual data
        var data = await GetAllDisabledUsersAsync();
        foreach (var item in data)
        {
            if (item.UserName.Contains(".") || !item.UserName.Contains("Test") || !item.UserName.Contains("svc") || !item.UserName.Contains("test"))
            {
                if (item.extensionAttribute9 != "crutial")
                {
                    if (item.UserName.Contains(UserName))
                    {
                        Source.Add(item);
                    }
                }

            }

        }
    }

    public Enable_AccountViewModel( IADServices aDServices, INavigationService navigationService, ILogService logService)
    {
        _ADservice = aDServices;
        _navigationService = navigationService;
        _logService = logService;
    }
    private async Task<IEnumerable<DisabledUsers>> DisabledUsersAll()
    {
        List<DisabledUsers> build = await _ADservice.GetDisabledUsersWithExtensionAttribute9();
        return build;
    }

    public async Task<IEnumerable<DisabledUsers>> GetAllDisabledUsersAsync()
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
            if (item.UserName.Contains(".") || !item.UserName.Contains("Test") || !item.UserName.Contains("svc") || !item.UserName.Contains("test"))
            {
                if(item.extensionAttribute9 != "crutial")
                {
                    Source.Add(item);
                }
                
            }
            
        }
    }
    public void OnNavigatedFrom()
    {
    }

    public async void Refresh()
    {
        Source.Clear();
        // Replace this with your actual data
        var data = await GetAllDisabledUsersAsync();
        foreach (var item in data)
        {
            if (item.UserName.Contains(".") || !item.UserName.Contains("Test") || !item.UserName.Contains("svc") || !item.UserName.Contains("test"))
            {
                if (item.extensionAttribute9 != "crutial")
                {
                    if (item.UserName.Contains(".")) {
                        Source.Add(item);
                    }
                    
                }

            }

        }
    }

    private void NavigateToDetail(DisabledUsers disabledUsers)
    {
        MessageBoxResult result = MessageBox.Show($"Do you want enable the account {disabledUsers.UserName}?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            _ADservice.EnableAccount(UserName);
            _logService.createLog("enableAccount", $"username:{disabledUsers.UserName}");
            Refresh();
        }
    }
}
