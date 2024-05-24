using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ADTool.Contracts.Services;
using ADTool.Core.Models;
using ADTool.Services;
using ADTool.ViewModels;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.Serialization;

namespace ADTool.Views;

public partial class DashboardPage : Page
{

    private readonly IDashBoardService _dashboardService;
    private readonly INavigationService _navigationService;
    private readonly IADServices _services;


    public DashboardPage(DashboardViewModel viewModel, IDashBoardService dashboardService, INavigationService navigationService, IADServices aDServices)
    {
        InitializeComponent();
        DataContext = viewModel;
        Setup_Counts_Count();
        _dashboardService = dashboardService;
        _navigationService = navigationService;
        _services = aDServices;

    }

    

    public async void MovePageDisabeldComps(object sender, EventArgs e)
    {
        await _navigationService.NavigateTo($"ADTool.ViewModels.Enable_DeviceViewModel");
        
    }

    public async void MovePagePrebuild(object sender, EventArgs e)
    {
        List<PrebuildComputers> prebuild = await _services.GetComputersInOuAllRegionsPrebuilt();
        PrebuildComputersManager.SavePrebuildComputers(prebuild);
        if(prebuild.Count > 0)
        {
            await _navigationService.NavigateTo($"ADTool.ViewModels.PrebuildComputersViewModel");
        }
        
    }

    public void Setup_Counts_Count()
    {
        try
        {
            int _AllComputers;
            int _AllUsers;
            int _AllActiveUsers;
            int _AllDisabledComps;
            int _AllDisabledUsers;
            int _AllPrebuild;
            DashboardDataManager.TryLoadDashboardData(out _AllComputers, out _AllUsers, out _AllActiveUsers,out _AllDisabledUsers,out _AllDisabledComps, out _AllPrebuild);
            Computer_Disabled_Count_TextBox.Content = _AllDisabledComps.ToString();
            Computer_Prebuild_Count_TextBox.Content = _AllPrebuild.ToString();
        }
        catch(Exception ex)
        {
            MessageBox.Show($"An error occured while loading your Dashboard: {ex.Message}","Attention",MessageBoxButton.OK);
        }
    }

    public async void refreshDashboard(object sender, EventArgs e)
    {
        InitializeComponent();
        RefreshIcon.Spin = true;
        RefreshIcon.Foreground = System.Windows.Media.Brushes.Red;
        await _dashboardService.refreshDashBoardValues();
        Setup_Counts_Count();
        RefreshIcon.Foreground = System.Windows.Media.Brushes.LimeGreen;
        RefreshIcon.Spin = false;
    }
}
