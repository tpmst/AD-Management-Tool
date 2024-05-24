using ADTool.Contracts.Services;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ADTool.Services
{
    public class DashBoardsServices : IDashBoardService
    {
        private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
        string domainName;

        private readonly IADServices _ADService;
        public DashBoardsServices(IADServices aDServices) 
        {
            _ADService = aDServices;
            AppConfigManager.TryLoaddomain(AppConfPath, out  domainName);
        }
        
        public async void getDashBoardValues()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
                if (!File.Exists(filePath))
                {
                    var (AllComputers, AllTest, AllServer, AllDC) = await _ADService.CountDevicesServersAndDCsInOuAllRegions(domainName);
                    int AllPrebuild = await _ADService.CountDevicesInOuAllRegionsPrebuild();
                    var (AllUsers, DisabledUser, AllActiveUsers, AllDisabledUsers, AllLockedUsers, expieredPasswordUsers) = await _ADService.CountUsersWithEmployeeIDAndLockedUsers();
                    int AllDisabledComps = await _ADService.CountDisabledDevicesInOuAllRegions();
                    List<string> recentChanges = await _ADService.GetLast3SecurityEventLogs();
                    Dictionary<string, long> DClatency = await _ADService.GetLatencyToDomainControllersAsync(domainName);
                    int SelcedPage1;
                    int SelcedPage2;
                    int SelcedPage3;

                    DashboardDataManager.TryLoadDashboardDataSelectedPage(out SelcedPage1, out SelcedPage2, out SelcedPage3);
                    DashboardDataManager.SaveDashboardData(AllComputers, AllUsers, DisabledUser, AllActiveUsers, AllPrebuild, AllDisabledUsers, AllDisabledComps, SelcedPage1, SelcedPage2, SelcedPage3, recentChanges, DClatency, AllTest, AllServer, AllDC, AllLockedUsers, expieredPasswordUsers);
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00088", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        public async Task refreshDashBoardValues()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            if(!File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var (AllComputers, AllTest, AllServer, AllDC) = await _ADService.CountDevicesServersAndDCsInOuAllRegions(domainName);
            int AllPrebuild = await _ADService.CountDevicesInOuAllRegionsPrebuild();
            var (AllUsers, DisabledUser, AllActiveUsers, AllDisabledUsers, AllLockedUsers, expieredPasswordUsers) = await _ADService.CountUsersWithEmployeeIDAndLockedUsers();
            int AllDisabledComps = await _ADService.CountDisabledDevicesInOuAllRegions();
            List<string> recentChanges = await _ADService.GetLast3SecurityEventLogs();
            Dictionary<string, long> DClatency = await _ADService.GetLatencyToDomainControllersAsync(domainName);
            int SelcedPage1;
            int SelcedPage2;
            int SelcedPage3;

            DashboardDataManager.TryLoadDashboardDataSelectedPage(out SelcedPage1, out SelcedPage2, out SelcedPage3);
            DashboardDataManager.SaveDashboardData(AllComputers, AllUsers, DisabledUser, AllActiveUsers, AllPrebuild, AllDisabledUsers, AllDisabledComps, SelcedPage1, SelcedPage2, SelcedPage3, recentChanges, DClatency, AllTest, AllServer, AllDC, AllLockedUsers, expieredPasswordUsers);

        }

    }
}
