using ADTool.Contracts.Services;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using ADTool.Core.Services;
using ADTool.Services;
using ADTool.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.ComponentModel;

namespace ADTool.ViewModels;

public class Add_Remove_User_GroupViewModel : ObservableObject, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
                OnPropertyChanged(); // You should raise PropertyChanged here
            }
        }
    }


    private readonly INavigationService _navigationService;
    private readonly ISampleDataService _sampleDataService;
    private readonly ILogService _logService;
    private readonly IADServices _ADservice;
    private ICommand _navigateToDetailCommand;
    private ICommand _submit;

    public ICommand NavigateToDetailCommand => _navigateToDetailCommand ?? (_navigateToDetailCommand = new RelayCommand<Groups>(NavigateToDetail));
    public ICommand submit => _submit ?? (_submit = new RelayCommand(submitButton));
    public ObservableCollection<Groups> Source { get; } = new ObservableCollection<Groups>();



    public Add_Remove_User_GroupViewModel(IADServices aDservice, ILogService logService)
    {
        _ADservice = aDservice;
        _logService = logService;
    }



    private async Task<IEnumerable<Groups>> GroupsAll()
    {
        List<Groups> build = await _ADservice.GetAllUsersGroupAsync(UserName);
        return build;
    }

    public async Task<IEnumerable<Groups>> GetContentGroupsAsync()
    {
        var sites = GroupsAll();
        return await await Task.FromResult(sites);
    }

    private void submitButton()
        => ButtonClick();

    private async void ButtonClick()
    {
        Source.Clear();
        // Replace this with your actual data
        var data = await GetContentGroupsAsync();
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }
    public void OnNavigatedFrom()
    {
    }

    private void NavigateToDetail(Groups group)
    {
        MessageBoxResult resutl = System.Windows.MessageBox.Show($"Do you want to remove the groupmembership of {UserName} in {group.Gropname}", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (resutl == MessageBoxResult.Yes)
        {
            bool removeConf = _ADservice.RemoveUserFromGroup(UserName, group.dsName);
            if (removeConf)
            {
                ButtonClick();
                _logService.createLog("removeGroup", $"username:{UserName}/group:{group.Gropname}");
            }

            MessageBox.Show($"The group {group.Gropname} has been added to {UserName}", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Information);
            Source.Remove(group);
            ButtonClick();
        }
    }
}
