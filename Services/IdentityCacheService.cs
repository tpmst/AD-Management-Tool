using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using ADTool.Core.Contracts.Services;

namespace ADTool.Services;

public class IdentityCacheService : IIdentityCacheService
{
    public static readonly string _msalCacheFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Assembly.GetExecutingAssembly().GetName().Name}";
    public static readonly string _msalCacheFileName = ".msalcache.bin3";
    private readonly object _fileLock = new object();

    public byte[] ReadMsalToken()
    {
        lock (_fileLock)
        {
            var filePath = Path.Combine(_msalCacheFilePath, _msalCacheFileName);
            if (File.Exists(filePath))
            {
                var encryptedData = File.ReadAllBytes(filePath);
                return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            }

            return default;
        }
    }

    public void SaveMsalToken(byte[] token)
    {
        lock (_fileLock)
        {
            if (!Directory.Exists(_msalCacheFilePath))
            {
                Directory.CreateDirectory(_msalCacheFilePath);
            }

            var encryptedData = ProtectedData.Protect(token, null, DataProtectionScope.CurrentUser);
            var filePath = Path.Combine(_msalCacheFilePath, _msalCacheFileName);
            File.WriteAllBytes(filePath, encryptedData);
        }
    }
}
