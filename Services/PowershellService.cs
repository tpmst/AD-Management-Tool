using MahApps.Metro.Converters;
using Microsoft.PowerShell;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Windows;

namespace ADTool.Services
{
    class PowershellService
    {
        private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");

        public static string RunPowerShellCommand(string command, string userName, string password)
        {
            try
            {
                string _dom;
                AppConfigManager.TryLoaddom(AppConfPath, out _dom);

                // Create a new ProcessStartInfo
                var newProcessInfo = new System.Diagnostics.ProcessStartInfo();

                newProcessInfo.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
                newProcessInfo.Verb = "runas";
                newProcessInfo.Arguments = $"-Command \"{command}\"";
                newProcessInfo.Domain = _dom.ToLower();
                newProcessInfo.UserName = userName;
                // Create a SecureString for the password
                SecureString securePassword = new SecureString();
                foreach (char c in password)
                {
                    securePassword.AppendChar(c);
                }
                newProcessInfo.Password = securePassword;

                // Enable the UseShellExecute property
                newProcessInfo.UseShellExecute = false;

                // Redirect the standard output
                newProcessInfo.RedirectStandardOutput = true;

                // Create a new Process
                using (var process = new Process())
                {
                    process.StartInfo = newProcessInfo;

                    // Hide the window
                    process.StartInfo.CreateNoWindow = true;

                    // Start the process
                    process.Start();

                    // Read the output
                    string output = process.StandardOutput.ReadToEnd();

                    // Wait for the process to exit
                    process.WaitForExit();

                    // Extract the password from the output
                    string passwordLine = output.Split('\n')[3].Trim();
                    return passwordLine;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the execution
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00068");
                return null;
            }
        }


    }
}
