using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace ADTool
{
    public class AppConf
    {
        public string standardDomainController {get; set;}
        public int passwordLength { get; set;}
        public string passwordComplexity { get; set;}
        public string domain {get ; set;}
        public string container { get ; set;}
        public string dom { get  ; set;}
        public string[] targetOU { get ; set;}
        public string logPath { get ; set;}
        public string UserCredPath { get ; set;}
        public string[] company { get ; set;}
        public string LAPSControlelr { get ; set;}
        public Dictionary<string, List<string>> OUsByRegion { get; set;}
        public bool SavePointLogs { get; set;}
        public Dictionary<string, string> CityBranch { get; set;}
        public string[] Country { get; set;}
        public string EvelationPath { get; set;}
        public int AppVersion { get; set;}
        public string[] DisabledSites { get; set; }
    }

    public static class AppConfigManager
    {
        public static void SaveAppconfig(string filePath,
        string standardDC,
        int _passwordLength,
        string _passwordComplexity,
        string _domain,
        string _container,
        string _dom,
        string[] _targetOU,
        string _logPath,
        string _userCredsPath,
        string[] _company,
        Dictionary<string, List<string>> _OUsByRegion,
        Dictionary<string, string> _CityBranch,
        string[] _Country,
        bool _SavePointLogs,
        string EvelationPath,
        int appversion,
        string _LAPSController,
        string[] _DisabledSite)
        {
            var credentials = new AppConf
            {
                standardDomainController = standardDC,
                passwordLength = _passwordLength,
                passwordComplexity = _passwordComplexity,
                domain = _domain,
                container = _container,
                dom = _dom,
                targetOU = _targetOU,
                logPath = _logPath,
                UserCredPath = _userCredsPath,
                company = _company,
                OUsByRegion = _OUsByRegion,
                CityBranch = _CityBranch,
                Country = _Country,
                SavePointLogs = _SavePointLogs,
                EvelationPath = EvelationPath,
                AppVersion = appversion,
                LAPSControlelr = _LAPSController,
                DisabledSites = _DisabledSite

            };

            string serializedCredentials = JsonConvert.SerializeObject(credentials);
            byte[] encryptedData = ProtectData(serializedCredentials);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
                File.WriteAllBytes(filePath, encryptedData);
        }
            public static void SaveAppconfigSettinsPage(string filePath,
            string standardDC,
            int _passwordLength,
            string _passwordComplexity,
            string _domain,
            string _container,
            string _dom,
            string[] _targetOU,
            string _logPath,
            string[] _company,
            string UserCreds,
            string EvelationPath,
            Dictionary<string, List<string>> _OUsByRegion,
            bool _SavePointLogs,
            Dictionary<string, string> _CityBranch,
            string[] _Country,
            string _LAPSController,
            string[] _DisabledSites)
            {
                var credentials = new AppConf
                {
                    standardDomainController = standardDC,
                    passwordLength = _passwordLength,
                    passwordComplexity = _passwordComplexity,
                    domain = _domain,
                    container = _container,
                    dom = _dom,
                    targetOU = _targetOU,
                    logPath = _logPath,
                    company = _company,
                    UserCredPath = UserCreds,
                    OUsByRegion = _OUsByRegion,
                    CityBranch = _CityBranch,
                    Country = _Country,
                    SavePointLogs = _SavePointLogs,
                    EvelationPath = EvelationPath,
                    LAPSControlelr = _LAPSController,
                    DisabledSites = _DisabledSites

                };

                string serializedCredentials = JsonConvert.SerializeObject(credentials);
                //byte[] encryptedData = ProtectData(serializedCredentials);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, serializedCredentials);
            }

        public static void SaveAppconfigSettinsPageENC(string filePath,
            string standardDC,
            int _passwordLength,
            string _passwordComplexity,
            string _domain,
            string _container,
            string _dom,
            string[] _targetOU,
            string _logPath,
            string[] _company,
            string UserCreds,
            string EvelationPath,
            Dictionary<string, List<string>> _OUsByRegion,
            bool _SavePointLogs,
            Dictionary<string, string> _CityBranch,
            string[] _Country,
            string _LAPSController,
            string[] _DisabledPages)
        {
            var credentials = new AppConf
            {
                standardDomainController = standardDC,
                passwordLength = _passwordLength,
                passwordComplexity = _passwordComplexity,
                domain = _domain,
                container = _container,
                dom = _dom,
                targetOU = _targetOU,
                logPath = _logPath,
                company = _company,
                UserCredPath = UserCreds,
                OUsByRegion = _OUsByRegion,
                CityBranch = _CityBranch,
                Country = _Country,
                SavePointLogs = _SavePointLogs,
                EvelationPath = EvelationPath,
                LAPSControlelr = _LAPSController,
                DisabledSites = _DisabledPages

            };

            string serializedCredentials = JsonConvert.SerializeObject(credentials);
            byte[] encryptedData = ProtectData(serializedCredentials);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, encryptedData);
        }




        public static bool TryLoadAppConSettingAll(string filePath, out string standartDC, out string LAPSController, out string dom, out string container, out string domain,
                out int _passwordLength,
                out string _passwordComplexity,
                out string[] _targetOU,
                out string _logPath,
                out string[] _company,
                out Dictionary<string, List<string>> _OUsByRegion,
                out Dictionary<string, string> _CityBranch,
                out string[] _Country,
                out bool _SavePointLogs,
                out string EvelationPath,
                out int AppVersion,
                out string UserCredPath,
                out string[] _DisabledSites)
                {
                    if (File.Exists(filePath))
                    {
                        /*
                        byte[] encryptedData = File.ReadAllBytes(filePath);
                        string serializedCredentials = UnprotectData(encryptedData);
                        */
                        string serializedCredentials = File.ReadAllText(filePath);

                        if (!string.IsNullOrEmpty(serializedCredentials))
                        {
                            var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                            if (appConfig != null)
                            {
                                standartDC = appConfig.standardDomainController;
                                LAPSController = appConfig.LAPSControlelr;
                                dom = appConfig.dom;
                                container = appConfig.container;
                                domain = appConfig.domain;
                                _passwordLength = appConfig.passwordLength;
                                _passwordComplexity = appConfig.passwordComplexity;
                                _targetOU = appConfig.targetOU;
                                _logPath = appConfig.logPath;
                                _company = appConfig.company;
                                _OUsByRegion = appConfig.OUsByRegion;
                                _CityBranch = appConfig.CityBranch;
                                _Country = appConfig.Country;
                                _SavePointLogs = appConfig.SavePointLogs;
                                EvelationPath = appConfig.EvelationPath;
                                AppVersion = appConfig.AppVersion;  
                                UserCredPath = appConfig.UserCredPath;
                                _DisabledSites = appConfig.DisabledSites;
                                return true;
                            }
                        }
                    }

                    standartDC = null;
                    LAPSController = null;
                    dom = null;
                    container = null;
                    domain = null;
                    _targetOU = null;
                    _logPath = null;
                    _company = null;
                    _passwordComplexity = null;
                    _passwordLength = 0;
            _OUsByRegion = null;
            _CityBranch = null;
            _Country = null;
            _SavePointLogs = false;
            EvelationPath = null;
            UserCredPath = null;
            AppVersion = 0;
            _DisabledSites = null;
                    return false;
                }



        public static bool TryLoadCityBranch(string filePath, out Dictionary<string, string> CityBranch)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        CityBranch = appConfig.CityBranch;
                        return true;
                    }
                }
            }
            CityBranch = null;
            return false;
        }

        public static bool TryLoadDisabledSites(string filePath, out string[] DisabledSites)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        DisabledSites = appConfig.DisabledSites;
                        return true;
                    }
                }
            }
            DisabledSites = null;
            return false;
        }

        public static bool TryLoadCountry(string filePath, out string[] Country)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        Country = appConfig.Country;
                        return true;
                    }
                }
            }
            Country = null;
            return false;
        }

        public static bool TryLoadLogDestination(string filePath, out bool _SavePointLogs)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        _SavePointLogs = appConfig.SavePointLogs;
                        return true;
                    }
                }
            }
            _SavePointLogs = false;
            return false;
        }


        public static bool TryLoadAppConADServices(string filePath, out string standartDC, out string userCredsPath, out string LAPSController, out string dom, out string container, out string domain, out string[] targetOUs)
        {
            if (File.Exists(filePath))
            {
                
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);
                

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        standartDC = appConfig.standardDomainController;
                        userCredsPath = appConfig.UserCredPath;
                        LAPSController = appConfig.LAPSControlelr;
                        dom = appConfig.dom;
                        container = appConfig.container;
                        domain = appConfig.domain;
                        targetOUs = appConfig.targetOU;
                        return true;
                    }
                }
            }

            standartDC = null;
            userCredsPath = null;
            LAPSController = null;
            dom = null;
            container = null;
            domain = null;
            targetOUs = null;
            return false;
        }

        public static bool TryLoadAppConSettingPage(string filePath, out string standartDC, out string LAPSController, out string dom, out string container, out string domain,
        out int _passwordLength,
        out string _passwordComplexity,
        out string[] _targetOU,
        out string _logPath,
        out string[] _company)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        standartDC = appConfig.standardDomainController;
                        LAPSController = appConfig.LAPSControlelr;
                        dom = appConfig.dom;
                        container = appConfig.container;
                        domain = appConfig.domain;
                        _passwordLength = appConfig.passwordLength;
                        _passwordComplexity = appConfig.passwordComplexity;
                        _targetOU = appConfig.targetOU;
                        _logPath = appConfig.logPath;
                        _company = appConfig.company;
                        return true;
                    }
                }
            }

            standartDC = null;
            LAPSController = null;
            dom = null;
            container = null;
            domain = null;
            _targetOU = null;
            _logPath = null;
            _company = null;
            _passwordComplexity = null;
            _passwordLength = 0;
            return false;
        }

        public static bool TryLoadVersionfile(string filePath, out int AppVersion)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        AppVersion = appConfig.AppVersion;
                        return true;
                    }
                }
            }
            AppVersion = 0;
            return false;
        }

        public static bool TryLoadUserCredentialsPath(string filePath, out string userCredPath)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        userCredPath = appConfig.UserCredPath;
                        return true;
                    }
                }
            }
            userCredPath = null;
            return false;
        }

        public static bool TryLoadUserEvelationPath(string filePath, out string EvelationPath)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        EvelationPath = appConfig.EvelationPath;
                        return true;
                    }
                }
            }
            EvelationPath= null;
            return false;
        }

        public static bool TryLoadPasswordConfig(string filePath, out int passwordLength, out string passwordComplexity)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        passwordComplexity = appConfig.passwordComplexity;
                        passwordLength = appConfig.passwordLength;
                        return true;
                    }
                }
            }
            passwordLength = 0;
            passwordComplexity = null;
            return false;
        }

        public static bool TryLoadOUByRegion(string filePath, out Dictionary<string, List<string>> OUsByRegion)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        OUsByRegion = appConfig.OUsByRegion;
                        return true;
                    }
                }
            }
            OUsByRegion = null;
            return false;
        }

        public static bool TryLoadtargetOUs(string filePath, out string[] targetOU)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        targetOU = appConfig.targetOU;
                        return true;
                    }
                }
            }
            targetOU = null;
            return false;
        }

        public static bool TryLoadCompany(string filePath, out string[] companys)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        companys = appConfig.company;
                        return true;
                    }
                }
            }
            companys = null;
            return false;
        }

        public static bool TryLoadLogService(string filePath, out string userCredPath, out string LogPath)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        userCredPath = appConfig.UserCredPath;
                        LogPath = appConfig.logPath;
                        return true;
                    }
                }
            }
            userCredPath = null;
            LogPath = null;
            return false;
        }

        public static bool TryLoaddomain(string filePath, out string domain)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        domain = appConfig.domain;
                        return true;
                    }
                }
            }
            domain = null;
            return false;
        }

        public static bool TryLoaddom(string filePath, out string dom)
        {
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string serializedCredentials = UnprotectData(encryptedData);

                if (!string.IsNullOrEmpty(serializedCredentials))
                {
                    var appConfig = JsonConvert.DeserializeObject<AppConf>(serializedCredentials);
                    if (appConfig != null)
                    {
                        dom = appConfig.dom;
                        return true;
                    }
                }
            }
            dom = null;
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
