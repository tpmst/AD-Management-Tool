using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADTool.Core.Models;
using ADTool.Models;
using ADTool.Services;
using static ADTool.Services.ADServices;

namespace ADTool.Contracts.Services
{
    public interface IADServices
    {

        bool IsElevatedforSharedMailboxes(string username, string groupName);

        string ConvertDNtoGUID(string objectDN);
        Dictionary<string, string> FindComputers(string searchFilterComputerName);

        Dictionary<string, string> FindUsers(string searchFilterUserName);

        Dictionary<string, string> FindGroups(string searchFilterGroupName);

        bool AddUserToGroup(string username, string groupName);

        bool RemoveUserFromGroup(string username, string groupName);

        bool ResetPassword(string userName,string newPassword);

        string GetLAPS(string computerName);

        string GetExtensionAttribute(string username);

        bool SetExtensionAttribute(string username);

        void UnlockUserAccount(string userName);

        Task<List<DisabledUsers>> GetDisabledUsersWithExtensionAttribute9();
        Task<List<Groups>> GetAllUsersGroupAsync(string username);
        bool ExistsCom(string compName);

        Task<List<DisabledDevcies>> GetDisabledComputersWithExtensionAttribute9();
        string GetOSVersion(string computerName);
        void UpdateUserAttributes(string username, string city, string country, string logonPath, string mobileNumber);

        bool IsUserInLocalAD(string username);

        bool IsGroupInLocalAD(string groupName);

        bool DisableAccount(string username, string resetPassword);

        void EnableAccount(string userName);

        void DeleteDevice(string computerName);

        Dictionary<string, List<string>> GetAllSubOUsByRegion();

        BitlockerKey GetBitLockerRecoveryKeyFromAAD(string BitlockerKeyID);
        void EnableDevice(string hostname);

        Task<(int employeeCount, int disabledUser, int activeUsersWithEmployeeIDCount, int disabledUserCount, int lockedUserCount, int expieredPasswordUsers)> CountUsersWithEmployeeIDAndLockedUsers();
        Task<(int deviceCount, int testDeviceCount, int serverCount, int dcCount)> CountDevicesServersAndDCsInOuAllRegions(string domainName);
        bool ExistsComputerInOU(string computerName, string ouDistinguishedName);
        Task<int> CountDisabledDevicesInOuAllRegions();
        Task<List<string>> GetLast3SecurityEventLogs();

        Task<int> CountDevicesInOuAllRegionsPrebuild();

        Task<List<PrebuildComputers>> GetComputersInOuAllRegionsPrebuilt();

        bool IsDeviceDisabledOrLocked(string deviceName);

        Task<string> GetIPAddress(string hostname);

        bool MoveComputerToOu(string computerName);

        bool IsAccountDisabledOrLocked(string username);

        Task<string> GetUserEmailFromLocalAD(string username);

        //check User
        bool IsAccountLocked(string username);
        bool IsAccountDisabled(string username);
        (string City, string Company, string Country, string Manager, string PhoneNumber, string LogonPath) GetUserCityAndCompany(string username);
        DateTime? GetPasswordExpirationDate(string username);
        DateTime? GetLastLogin(string username);
        string GetCreationDate(string username);

        Task<Dictionary<string, long>> GetLatencyToDomainControllersAsync(string domainName);
    }
}
