using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Graph.Models;

namespace ADTool.Services
{
    public class DashboardData
    {
        public int AllComputers { get; set; }
        public int AllUsers { get; set; }
        public int DisabeledUsers { get; set; }
        public int AllActiveUsers { get; set; }

        public int AllLockedUsers { get; set; }
        public int expieredPasswordUsers { get; set; }
        public int AllPrebuild { get; set; }

        public int AllDCs { get; set; }
        public int AllServer { get; set; }

        public int AllTestDevices { get; set; }

        public int AllDisabledUsers { get; set; }

        public int AllDisabledComputers { get; set; }

        public int SelectedPages1 { get; set; }
        public int SelectedPages2 { get; set; }
        public int SelectedPages3 { get; set; }

        public List<string> ChangeLogs { get; set; }

        public Dictionary<string, long> DClatency { get; set; }
    }

    public class DashboardDataManager
    {
        public static void SaveDashboardData(int _AllCopmputers, int _AllUsers, int _DisabledUsers, int _AllAktiveUsers, int _AllPrebuild, int _AllDisabledUsers, int _AllDisabledComps, int _SelectedPage1, int _SelectedPage2, int _SelectedPage3, List<string> _ChangeLogs, Dictionary<string, long> _DClatency, int _AllTest, int _AllServer, int _AllDC, int _AllLockedUsers, int _expieredPasswordUsers)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            var data = new DashboardData()
            {
                AllComputers = _AllCopmputers,
                AllPrebuild = _AllPrebuild,
                AllUsers = _AllUsers,
                DisabeledUsers = _DisabledUsers,
                AllActiveUsers = _AllAktiveUsers,
                AllDisabledComputers = _AllDisabledComps,
                AllDisabledUsers = _AllDisabledUsers,
                SelectedPages1 = _SelectedPage1,
                SelectedPages2 = _SelectedPage2,
                SelectedPages3 = _SelectedPage3,
                ChangeLogs = _ChangeLogs,
                DClatency = _DClatency,
                AllDCs = _AllDC,
                AllServer = _AllServer,
                AllTestDevices = _AllTest,
                AllLockedUsers = _AllLockedUsers,
                expieredPasswordUsers = _expieredPasswordUsers

            };

            string serializedCredentials = JsonConvert.SerializeObject(data);
            byte[] encryptedData = ProtectData(serializedCredentials);

            File.WriteAllBytes(filePath, encryptedData);
        }

        public static bool TryLoadDashboardChangeLogs(out List<string> _ChangeLogs)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedData = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedData))
                {
                    var data = JsonConvert.DeserializeObject<DashboardData>(serializedData);
                    if (data != null)
                    {
                        _ChangeLogs = data.ChangeLogs;
                        return true;
                    }
                }
            }

            _ChangeLogs = null;
            return false;
        }
        public static bool TryLoadDashboardDClatency(out Dictionary<string, long> _DClatency)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedData = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedData))
                {
                    var data = JsonConvert.DeserializeObject<DashboardData>(serializedData);
                    if (data != null)
                    {
                        _DClatency = data.DClatency;
                        return true;
                    }
                }
            }

            _DClatency = null;
            return false;
        }

        public static bool TryLoadDashboardDataSelectedPage(out int _SelectedPage1, out int _SelectedPage2, out int _SelectedPage3)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedData = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedData))
                {
                    var data = JsonConvert.DeserializeObject<DashboardData>(serializedData);
                    if (data != null)
                    {
                        _SelectedPage1 = data.SelectedPages1;
                        _SelectedPage2 = data.SelectedPages2;
                        _SelectedPage3 = data.SelectedPages3;
                        return true;
                    }
                }
            }

            _SelectedPage1 = 0;
            _SelectedPage2 = 0;
            _SelectedPage3 = 0;
            return false;
        }


        public static bool TryLoadDashboardDataComp(out int _AllCopmputers, out int _AllUsers, out int _DisabledUsers, out int _AllAktiveUsers, out int _AllDisabledUsers, out int _AllServer, out int _AllDCs, out int _AllTest, out int _AllLockedUsers, out int _expieredPasswordUsers)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedData = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedData))
                {
                    var data = JsonConvert.DeserializeObject<DashboardData>(serializedData);
                    if (data != null)
                    {
                        _AllAktiveUsers = data.AllActiveUsers;
                        _AllUsers = data.AllUsers;
                        _DisabledUsers = data.DisabeledUsers;
                        _AllCopmputers = data.AllComputers;
                        _AllDisabledUsers = data.AllDisabledUsers;
                        _AllTest = data.AllTestDevices;
                        _AllServer = data.AllServer;
                        _AllDCs = data.AllDCs;
                        _AllLockedUsers = data.AllLockedUsers;
                        _expieredPasswordUsers = data.expieredPasswordUsers;

                        return true;
                    }
                }
            }

            _AllCopmputers = 0;
            _AllAktiveUsers = 0;
            _AllUsers = 0;
            _DisabledUsers = 0;
            _AllDisabledUsers = 0;
            _AllDCs = 0;
            _AllServer = 0;
            _AllTest = 0;
            _AllLockedUsers = 0;
            _expieredPasswordUsers = 0;
            return false;
        }

        public static bool TryLoadDashboardData(out int _AllCopmputers, out int _AllUsers, out int _AllAktiveUsers, out int _AllDisabledUsers, out int _AllDisabeldComps, out int _AllPrebuild)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedData = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedData))
                {
                    var data = JsonConvert.DeserializeObject<DashboardData>(serializedData);
                    if (data != null)
                    {
                        _AllAktiveUsers = data.AllActiveUsers;
                        _AllUsers = data.AllUsers;
                        _AllCopmputers = data.AllComputers;
                        _AllDisabeldComps = data.AllDisabledComputers;
                        _AllDisabledUsers = data.AllDisabledUsers;
                        _AllPrebuild = data.AllPrebuild;
                        return true;
                    }
                }
            }

            _AllCopmputers = 0;
            _AllAktiveUsers = 0;
            _AllUsers = 0;
            _AllDisabledUsers = 0;
            _AllDisabeldComps = 0;
            _AllPrebuild = 0;
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
