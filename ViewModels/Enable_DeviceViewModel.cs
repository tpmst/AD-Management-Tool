using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using ADTool.Core.Contracts.Services;
using ADTool.Models;
using ADTool.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ADTool.ViewModels;

public class Enable_DeviceViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
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

    public ICommand NavigateToDetailCommand => _navigateToDetailCommand ?? (_navigateToDetailCommand = new RelayCommand<DisabledDevcies>(NavigateToDetail));
    public ICommand searchUsernameBind => _seachfilter ?? (_seachfilter = new RelayCommand(searchUsername));
    public ObservableCollection<DisabledDevcies> Source { get; } = new ObservableCollection<DisabledDevcies>();


    private async void searchUsername()
    {
        Source.Clear();
        // Replace this with your actual data
        var data = await GetAllDisabledUsersAsync();
        foreach (var item in data)
        {
            if (item.extensionAttribute9 != "crutial")
            {
                string upperUserName = UserName.ToUpper();
                if (!item.Hostname.Contains("DCV") || !item.Hostname.Contains("-")|| !item.Hostname.Contains("CONT")|| !item.Hostname.Contains("cont")|| !item.Hostname.Contains("manag")|| !item.Hostname.Contains("Domain"))
                {
                    if (item.Hostname.Contains(upperUserName))
                    {
                        Source.Add(item);
                    }

                }
                
            }
        }
    }

    public Enable_DeviceViewModel(IADServices aDServices, INavigationService navigationService, ILogService logService)
    {
        _ADservice = aDServices;
        _navigationService = navigationService;
        _logService = logService;
    }

    private async Task<IEnumerable<DisabledDevcies>> DisabledUsersAll()
    {
        List<DisabledDevcies> build = await _ADservice.GetDisabledComputersWithExtensionAttribute9();
        return build;
    }

    public async Task<IEnumerable<DisabledDevcies>> GetAllDisabledUsersAsync()
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
            if (!item.Hostname.Contains("DCV"))
            {
                if (item.extensionAttribute9 != "crutial")
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
            if (item.extensionAttribute9 != "crutial")
            {
                Source.Add(item);
            }
        }
    }

    private void NavigateToDetail(DisabledDevcies disabledUsers)
    {
        MessageBoxResult result = MessageBox.Show($"Do you want enable the device {disabledUsers.Hostname}?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            _ADservice.EnableDevice(disabledUsers.Hostname);
            _logService.createLog("enableDevice", $"servicetag:{disabledUsers.Hostname}");
            Refresh();
        }
    }
}
