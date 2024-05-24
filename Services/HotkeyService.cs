using AutoHotkey.Interop;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ADTool.Services
{
    public class HotkeyService
    {
        private const ushort KEY_PRESSED = 0x8000;

        static bool automationRunning = false;
        static AutoHotkeyEngine ahk;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        
        [STAThread]
        public static void HotkeyPassword()
        {

            // Create the AutoHotkeyEngine instance
            ahk = new AutoHotkeyEngine();

            // Start a separate thread to listen for key presses
            Thread keyPressThread = new Thread(ToggleAutomation);
            keyPressThread.Start();

            // Keep the program running
            Console.ReadLine();
        }

        static void ToggleAutomation()
        {
            while (true)
            {
                int passwordButton;
                AutoCopyConfManager.TryLoadButtonPassword(out passwordButton);
                if ((GetAsyncKeyState(passwordButton) & KEY_PRESSED) != 0)
                {
                    if (!automationRunning)
                    {
                        automationRunning = true;
                        PerformAutomation();
                        automationRunning = false;
                    }
                }

                Thread.Sleep(100);
            }
        }

        static void PerformAutomation()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Construct the script file path relative to the base directory
            string scriptPath = Path.Combine(baseDirectory, "Password.ahk"); // Update with your script's filename
            string scriptContent = File.ReadAllText(scriptPath);
            ahk.ExecRaw(scriptContent);
        }

        public static void HotkeyLocalAdmin()
        {

            // Create the AutoHotkeyEngine instance
            ahk = new AutoHotkeyEngine();

            // Start a separate thread to listen for key presses
            Thread keyPressThread = new Thread(ToggleAutomationLocalAdmin);
            keyPressThread.Start();

            // Keep the program running
            Console.ReadLine();
        }

        static void ToggleAutomationLocalAdmin()
        {
            while (true)
            {
                int localAdminButton;
                AutoCopyConfManager.TryLoadButtonLocalAdmin(out localAdminButton);
                if ((GetAsyncKeyState(localAdminButton) & KEY_PRESSED) != 0)
                {
                    if (!automationRunning)
                    {
                        automationRunning = true;
                        PerformAutomationLocalAdmin();
                        automationRunning = false;
                    }
                }

                Thread.Sleep(100);
            }
        }

        static void PerformAutomationLocalAdmin()
        {
            string password;
            AutoCopyConfManager.TryLoadLastPassword(out password);
            if (password.IsNullOrEmpty()) 
            {
                MessageBox.Show("There is no LAPS selected. Please go to to the site Computerinfo to check a LAPS","Attention",MessageBoxButton.OK,MessageBoxImage.Question);
            }
            else
            {
                ahk.ExecRaw("send, .\\");
                ahk.ExecRaw("send, +administrator");
                ahk.ExecRaw("send {Tab}");
                ahk.ExecRaw($"send, {password}");
            }
        }



    }

    public class AutoCopyConf
    {
        public string password { get; set; }
        public int buttonPassword { get; set; }
        public int buttonlocalAdmin { get; set; }
    }

    public static class AutoCopyConfManager
    {
        

        

        public static void SaveAutoCopyConf(string _password, int _buttonPassword, int _buttonlocalAdmin)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoCopy.json");
            var credentials = new AutoCopyConf
            {
                password = _password,
                buttonPassword = _buttonPassword,
                buttonlocalAdmin = _buttonlocalAdmin
            };

            string serializedCredentials = JsonConvert.SerializeObject(credentials);
            byte[] encryptedData = ProtectData(serializedCredentials);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, encryptedData);
        }

        public static bool TryLoadLastPassword(out string _password)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoCopy.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AutoCopyConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        _password = appConfig.password;
                        return true;
                    }
                }
            }
            _password = null;
            return false;
        }

        public static bool TryLoadButtonPassword(out int _buttonPassword)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoCopy.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AutoCopyConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        _buttonPassword = appConfig.buttonPassword;
                        return true;
                    }
                }
            }
            _buttonPassword = 0;
            return false;
        }

        public static bool TryLoadButtonLocalAdmin(out int _buttonLocalAdmin)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoCopy.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AutoCopyConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        _buttonLocalAdmin = appConfig.buttonlocalAdmin;
                        return true;
                    }
                }
            }
            _buttonLocalAdmin = 0;
            return false;
        }

        private static byte[] ProtectData(string data)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
            byte[] protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return protectedBytes;
        }

        private static string UnprotectData(byte[] protectedData)
        {
            try
            {
                byte[] bytes = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                string data = System.Text.Encoding.UTF8.GetString(bytes);
                return data;
            }
            catch
            {
                return null;
            }
        }
    }
}
