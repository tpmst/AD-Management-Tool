
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.IO;
    using System.Security.Cryptography;
using ADTool.Contracts.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

    namespace ADTool.Services
    {
        public class UserCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public static class UserCredentialManager
        {
            public static void SaveCredentials(string filePath, string username, string password)
            {
                var credentials = new UserCredentials
                {
                    Username = username,
                    Password = password,
                    
                };

                string serializedCredentials = JsonConvert.SerializeObject(credentials);
                byte[] encryptedData = ProtectData(serializedCredentials);

                File.WriteAllBytes(filePath, encryptedData);
            }

            public static bool TryLoadUserCredentials(string filePath, out string username, out string password)
            {
                if (File.Exists(filePath))
                {
                    byte[] encryptedData = File.ReadAllBytes(filePath);
                    string serializedCredentials = UnprotectData(encryptedData);

                    if (!string.IsNullOrEmpty(serializedCredentials))
                    {
                        var credentials = JsonConvert.DeserializeObject<UserCredentials>(serializedCredentials);
                        if (credentials != null)
                        {
                            username = credentials.Username;
                            password = credentials.Password;
                            return true;
                        }
                    }
                }

                username = null;
                password = null;
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


