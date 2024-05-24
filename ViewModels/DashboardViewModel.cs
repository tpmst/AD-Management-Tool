using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

using ADTool.Contracts.Services;
using ADTool.Contracts.ViewModels;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using ADTool.Services;
using ADTool.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.DirectoryServices.ActiveDirectory;
using System.Net.NetworkInformation;
using System.Windows.Media;
using Microsoft.Graph.Models;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Management.Automation.Language;

namespace ADTool.ViewModels;

public class DashboardViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ISampleDataService _sampleDataService;
    private ICommand _navigateToDetailCommand;


    public ISeries[] LatencyGridData { get; set; }
    public Axis[] LatencyGridDataXachse { get; set; }
    public Axis[] LatencyGridDataYachse { get; set; }
    public ISeries[] LatencyGridDataUser { get; set; }
    public Axis[] LatencyGridDataUserXachse { get; set; }
    public Axis[] LatencyGridDataUserYachse { get; set; }
    public ISeries[] LatencyGridDataComps { get; set; }
    public Axis[] LatencyGridDataCompsXachse { get; set; }
    public Axis[] LatencyGridDataCompsYachse { get; set; }

    
    public ICommand NavigateToDetailCommand => _navigateToDetailCommand ?? (_navigateToDetailCommand = new RelayCommand<DashboardSites>(NavigateToDetail));

    public ObservableCollection<DashboardSites> Source { get; } = new ObservableCollection<DashboardSites>();

    public DashboardViewModel(ISampleDataService sampleDataService, INavigationService navigationService)
    {
        _sampleDataService = sampleDataService;
        _navigationService = navigationService;
        CreateCollection();
        CreateCollection2();
    }

    

    private async void CreateCollection()
    {
        Dictionary<string, long> DataDC = new Dictionary<string, long>();
        DashboardDataManager.TryLoadDashboardDClatency(out DataDC);
        if(DataDC != null)
        {
            Dictionary<string, long> ShortenedKeysDataDC = new Dictionary<string, long>();

            foreach (var kvp in DataDC)
            {
                // Split the original key by dot and take the first part
                string shortenedKey = kvp.Key.Split('.')[0];

                // Add to the new dictionary with shortened key
                ShortenedKeysDataDC[shortenedKey] = kvp.Value;
            }
            List<int> numbersList = new List<int>();
            
            foreach (int value in ShortenedKeysDataDC.Values)
            {
                numbersList.Add(value);
            }

            List<double> numbersList2 = new List<double>();
            double numberValue = 1000;
            foreach(var values2 in numbersList)
            {
                numbersList2.Add(numberValue);
            }

            LatencyGridData = new ISeries[] {
                new ColumnSeries<double>
                {
                    IsHoverable = false, // disables the series from the tooltips 
                    Values = numbersList2.ToArray(),
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(30, 30, 30, 100)),
                    IgnoresBarPosition = true
                },

                new ColumnSeries<int>
                {
                    Values = numbersList.ToArray(),
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    IgnoresBarPosition = true
                }

            };

            LatencyGridDataXachse = new Axis[]
            {
                new Axis
        {
            Labels = ShortenedKeysDataDC.Keys.ToArray(),
            LabelsRotation = 0,
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
            SeparatorsAtCenter = false,
            TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
            TicksAtCenter = true,
            ForceStepToMin = true,
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(SKColors.White),
            ShowSeparatorLines = false,
            MinStep = 1
                }
            };
            LatencyGridDataYachse = new Axis[]
            {
                new Axis
        {

            ShowSeparatorLines = false,
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(SKColors.White),
            MinStep = 1
                }
            };
        }

    }

    private async void CreateCollection2()
    {

        int Users;
        int DisabledUserAccounts;
        int disabledUsers;
        int activeUsers;
        int AllComps;
        int _AllDC;
        int _AllServer;
        int _AllTest;
        int _AllLockedUsers;
        int _expieredPasswordUsers;

        DashboardDataManager.TryLoadDashboardDataComp(out AllComps, out Users, out DisabledUserAccounts, out activeUsers, out disabledUsers, out _AllServer, out _AllDC, out _AllTest, out _AllLockedUsers, out _expieredPasswordUsers);

        int Value1 = Users - disabledUsers - activeUsers - _AllLockedUsers;
        int Value2 = disabledUsers;
        int Value3 = activeUsers;
        int AllDeviceDash = AllComps - _AllTest - _AllDC - _AllServer;

        int[] chartValues = { Users, activeUsers, _AllLockedUsers, _expieredPasswordUsers };
        int[] DisabledRed = { DisabledUserAccounts , disabledUsers };

        if (Users != null && activeUsers != null && disabledUsers != null && _AllLockedUsers != null)
        {

            LatencyGridDataUser = new ISeries[] {
                new ColumnSeries<double>
                {
                    IsHoverable = false, // disables the series from the tooltips 
                    Values = new double[] { 4000, 4000, 4000, 4000},
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(30, 30, 30, 100)),
                    IgnoresBarPosition = true
                },

                new ColumnSeries<int>
                {
                    Values = chartValues,
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    IgnoresBarPosition = true
                },
                new ColumnSeries<int>
                {
                    Values = DisabledRed,
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.IndianRed),
                    IgnoresBarPosition = true
                }

            };

            LatencyGridDataUserXachse = new Axis[]
            {
                new Axis
        {
            Labels = new string[] { "All Users", "Employees", "Locked" , "Pass expired"},
            LabelsRotation = 0,
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(SKColors.White),
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
            SeparatorsAtCenter = false,
            TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
            TicksAtCenter = true,
            ShowSeparatorLines = false,
            ForceStepToMin = true,
            MinStep = 1
                }
            };


        }

        int[] ValuesComp = { AllComps, _AllServer, _AllDC, _AllTest };

        if (AllComps != null && _AllServer != null && _AllTest != null && _AllDC != null)
        {

            LatencyGridDataComps = new ISeries[] {
                new ColumnSeries<double>
                {
                    IsHoverable = false, // disables the series from the tooltips 
                    Values = new double[] { 2000, 2000, 2000, 2000},
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(30, 30, 30, 100)),
                    IgnoresBarPosition = true
                },

                new ColumnSeries<int>
                {
                    Values = ValuesComp,
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    IgnoresBarPosition = true
                }

            };

            LatencyGridDataCompsXachse = new Axis[]
            {
                new Axis
        {
            Labels = new string[] { "All", "Test", "Servers", "DCs" },
            LabelsRotation = 0,
            LabelsPaint = new SolidColorPaint(SKColors.White),
            TextSize = 12,
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
            SeparatorsAtCenter = false,
            TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
            TicksAtCenter = true,
            ForceStepToMin = true,
            ShowSeparatorLines = false,
            MinStep = 1
                }
            };
            
        }


    }

    

    public async void OnNavigatedTo(object parameter)
    {
        int SelcedPage1;
        int SelcedPage2;
        int SelcedPage3;

        DashboardDataManager.TryLoadDashboardDataSelectedPage(out SelcedPage1, out SelcedPage2, out SelcedPage3);

        Source.Clear();

        // Replace this with your actual data
        var data = await _sampleDataService.GetContentGridDataAsync();
        foreach (var item in data)
        {
            if(item.SiteID == SelcedPage1 || item.SiteID == SelcedPage2 || item.SiteID == SelcedPage3)
            {
                Source.Add(item);
            }
            
        }
    }

    public void OnNavigatedFrom()
    {

    }

    private void NavigateToDetail(DashboardSites sites)
    {
        _navigationService.NavigateTo($"ADTool.ViewModels.{sites.SiteUrl}");
    }

    
}
