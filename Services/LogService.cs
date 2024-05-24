using ADTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ADTool.Contracts.Services;
using Microsoft.Graph.Models;

namespace ADTool.Services
{
    public class LogService : ILogService
    {

        private readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;
        private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");

        private readonly ISharePointFileService _sharePointFileService;

        public LogService(ISharePointFileService sharePointFileService)
        {
            _sharePointFileService = sharePointFileService;
        }

        public async Task createLog(string Action, string inputData)
        {
            bool _SavePointLogs;
            string userCredPath;
            string QnapLogPath;
            AppConfigManager.TryLoadLogService(AppConfPath, out userCredPath, out QnapLogPath);
            AppConfigManager.TryLoadLogDestination(AppConfPath, out _SavePointLogs);
            // Get the paths for user credentials and Qnap log
            string Paths = Path.Combine(_localAppData, userCredPath);

            string _username, _password;
            UserCredentialManager.TryLoadUserCredentials(Paths, out _username, out _password);
            DateTime currentDateTime = DateTime.Now;
            string lineToAdd = $"{_username},{currentDateTime},{Action},inputData({inputData})";
            try
            {
                bool check = await _sharePointFileService.UpdateLogFile(lineToAdd, _username);
                if (!check)
                {
                    MessageBox.Show($"ErrorMassage: Your Logs cant be uploaded to Sharepoint contact an Administrator", "Error: 0x00015", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00018", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

    }
}
