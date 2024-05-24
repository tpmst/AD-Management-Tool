using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Services;
using System.Diagnostics;
using ADTool.Contracts.Services;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace ADTool.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IIdentityService _identityService;
        private readonly IMicrosoftGraphService _MicrosoftGraphService;

        private readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string downloadPath = "Update.7z";

        public UpdateService(IIdentityService identityService, IMicrosoftGraphService microsoftGraphService)
        {
            _identityService = identityService;
            _MicrosoftGraphService = microsoftGraphService;
        }

        static void GetPowershellScript()
        {
            // Get the base directory of the application
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Specify the name of your PowerShell script
            string scriptName = "updateAgent.ps1";

            // Combine the base directory and script name to get the full path
            string scriptPath = Path.Combine(baseDirectory, scriptName);

            // Check if the script file exists
            if (File.Exists(scriptPath))
            {
                // Run PowerShell with the script
                RunPowerShellScript(scriptPath);
            }
            else
            {
                MessageBox.Show("Your Application failed to update", "Error: 0x00005", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        static async void RunPowerShellScript(string scriptPath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(scriptPath)
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    using (StreamWriter sw = process.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine($"& '{scriptPath}'");
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Your Application failed to update", "Error: 0x00006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> UpdateApplication(int latestVersion, int currentVersion)
        {

            if (latestVersion.CompareTo(currentVersion) > 0)
            {

                bool downloaded = await DownloadUpdate();
                if(downloaded)
                {
                    // Replace the current executable

                    GetPowershellScript();

                    return true;
                }
                else
                {
                    MessageBox.Show("Your Application failed to update", "Error: 0x00004", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }


                
            }

            return false;
        }

        public async Task<bool> DownloadUpdate()
        {
            try
            {
                string accessToken = await _identityService.GetAccessTokenForGraphAsync();
                if (accessToken == null)
                {
                    return false;
                }
                else
                {
                    FileId responseHeader = await _MicrosoftGraphService.GetFileIDFromSharepointUpadteZIP(accessToken);
                    if(responseHeader.id != null)
                    {
                        string updatePath = Path.Combine(_localAppData, downloadPath);

                        bool downloaded = await _MicrosoftGraphService.DownloadFileFromSharepoint(accessToken, responseHeader.id, updatePath);
                        if(downloaded)
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Your Application failed to update", "Error: 0x00001", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Your Application failed to update", "Error: 0x00002", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }catch (Exception ex)
            {
                MessageBox.Show("Your Application failed to update", "Error: 0x00003", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}
