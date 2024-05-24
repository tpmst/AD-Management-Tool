using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ADTool.ViewModels;

public class PrebuildComputersViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ISampleDataService _sampleDataService;
    private readonly ILogService _logService;
    private readonly IADServices _ADservice;
    private ICommand _navigateToDetailCommand;

    public ICommand NavigateToDetailCommand => _navigateToDetailCommand ?? (_navigateToDetailCommand = new RelayCommand<PrebuildComputers>(NavigateToDetail));

    public ObservableCollection<PrebuildComputers> Source { get; } = new ObservableCollection<PrebuildComputers>();

    public PrebuildComputersViewModel(ISampleDataService sampleDataService, INavigationService navigationService, IADServices aDservice, ILogService logService)
    {
        _sampleDataService = sampleDataService;
        _navigationService = navigationService;
        _ADservice = aDservice;
        _logService = logService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();
        // Replace this with your actual data
        var data = await _sampleDataService.GetContentPrebuildComputersAsync();
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    

    public void OnNavigatedFrom()
    {
    }

    private void NavigateToDetail(PrebuildComputers comp)
    {
        string compName = comp.ComputerName;
        MessageBoxResult result = MessageBox.Show($"Do you want to move {compName} to the Computers OU?","Confirmation",MessageBoxButton.YesNo,MessageBoxImage.Question);
        if(result == MessageBoxResult.Yes)
        {
            bool confirm = _ADservice.MoveComputerToOu(compName);
            if(confirm)
            {
                Source.Remove(comp);
                _logService.createLog("movePrebuild", $"tag:{compName}");
            }
        }
    }
}
