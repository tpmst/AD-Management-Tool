using ADTool.Contracts.Services;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using ADTool.Models;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Security;
using System.Windows;

namespace ADTool.Services
{
    public class ADServices : IADServices
    {
        private readonly IIdentityService _identityService;
        private readonly IMicrosoftGraphService _MicrosoftGraphService;

        private static string _username;
        private string _password;
        private static string _dom;
        private static string _container;
        private static string _domain;
        private static string _LDAPFilter;
        private static string _UserCredPath;
        private static string _LAPSController;
        private static string _ldapFilterDCAZ01;
        private static string standardDC;
        private string _usernameDomain;
        private string[] traget;
        private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
        public readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;


        public ADServices(IIdentityService identityService, IMicrosoftGraphService microsoftGraphService)
        {
            _identityService = identityService;
            _MicrosoftGraphService = microsoftGraphService;
            loadConf();
        }

        private void loadConf()
        {
            try
            {
                    AppConfigManager.TryLoadAppConADServices(AppConfPath, out standardDC, out _UserCredPath, out _LAPSController, out _dom, out _container, out _domain, out traget);
                    string Paths = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "usercreds.json");
                    UserCredentialManager.TryLoadUserCredentials(Paths, out _username, out _password);
                    _LDAPFilter = $"LDAP://{_container}";
                    _usernameDomain = @$"{_dom}\{_username}";
                    _ldapFilterDCAZ01 = $"LDAP://{_LAPSController}.{_domain}";
            }
            catch(Exception ex)
            {
                MessageBox.Show($"ErrorMessage: {ex.Message}", "Error: 0x00082");
            }
           
        }


        public bool IsElevatedforSharedMailboxes(string username, string groupName)
        {
            try
            {
                // Assuming _LDAPFilter, _usernameDomain, and _password are defined elsewhere in your code
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(root))
                    {
                        // Set the filter to find the user by username
                        searcher.Filter = $"(&(objectClass=user)(sAMAccountName={username}))";

                        // Retrieve only the memberOf property
                        searcher.PropertiesToLoad.Add("memberOf");

                        SearchResult result = searcher.FindOne();

                        if (result != null)
                        {
                            // Check if the user is a member of the specified group
                            if (result.Properties.Contains("memberOf"))
                            {
                                foreach (string group in result.Properties["memberOf"])
                                {
                                    if (group.Contains(groupName))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMessage: {ex.Message}", "Error: 0x00081");
            }

            return false;
        }


        public bool IsUserInLocalAD(string username)
        {
            bool isInLocalAD = false;

            // Connect to the Active Directory using the LDAP filter, username domain, and password
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher search = new DirectorySearcher(entry))
                {
                    try
                    {
                        // Set the search filter to find a user with a matching samAccountName
                        search.Filter = $"(&(objectCategory=user)(samAccountName={username}))";
                        search.PropertiesToLoad.Add("samAccountName");

                        SearchResult result = search.FindOne();

                        if (result != null)
                        {
                            // User found in the local AD
                            isInLocalAD = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Display an error message if an exception occurs
                        MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00021", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            return isInLocalAD;
        }

        public bool IsGroupInLocalAD(string groupName)
        {
            bool isGroupInLocalAD = false;

            // Connect to the Active Directory using the LDAP filter, username domain, and password
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher search = new DirectorySearcher(entry))
                {
                    try
                    {
                        // Set the search filter to find a group with a matching name
                        search.Filter = $"(&(objectCategory=group)(cn={groupName}))";
                        search.PropertiesToLoad.Add("cn");

                        SearchResult result = search.FindOne();

                        if (result != null)
                        {
                            // Group found in the local AD
                            isGroupInLocalAD = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Display an error message if an exception occurs
                        MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00022", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            return isGroupInLocalAD;
        }


        public async Task<string> GetUserEmailFromLocalAD(string username)
        {
            string emailAddress = null;

            // Connect to the Active Directory using the LDAP filter, username domain, and password
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher search = new DirectorySearcher(entry))
                {
                    try
                    {
                        // Set the search filter to find a user with a matching samAccountName
                        search.Filter = $"(&(objectCategory=user)(samAccountName={username}))";
                        search.PropertiesToLoad.Add("mail");

                        SearchResult result = search.FindOne();

                        if (result != null)
                        {
                            // User found in the local AD, retrieve the email address
                            emailAddress = result.Properties["mail"][0]?.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Display an error message if an exception occurs
                        MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00023", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            return emailAddress;
        }


        public bool ExistsCom(string computerName)
        {
            bool found = false;

            // Connect to the Active Directory using the LDAP filter, username domain, and password
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password))
            {
                using (DirectorySearcher search = new DirectorySearcher(entry))
                {
                    // Set the search filter to find a computer with a matching name
                    search.Filter = $"(&(objectCategory=computer)(name={computerName}))";
                    search.PropertiesToLoad.Add("name");

                    SearchResult result = search.FindOne();

                    if (result != null)
                    {
                        // Computer found in the local AD
                        found = true;
                    }
                    return found;
                }
            }
        }

        public bool ExistsComputerInOU(string computerName, string ouDistinguishedName)
        {
            bool found = false;

            // Construct the LDAP path for the target OU
            string ouPath = $"LDAP://{ouDistinguishedName}";

            // Connect to the Active Directory using the LDAP path, username domain, and password
            using (DirectoryEntry ouEntry = new DirectoryEntry(ouPath, _usernameDomain, _password))
            {
                using (DirectorySearcher search = new DirectorySearcher(ouEntry))
                {
                    // Set the search filter to find a computer with a matching name
                    search.Filter = $"(&(objectCategory=computer)(name={computerName}))";
                    search.PropertiesToLoad.Add("name");

                    SearchResult result = search.FindOne();

                    if (result != null)
                    {
                        // Computer found in the specified OU
                        found = true;
                    }

                    return found;
                }
            }
        }


        public bool IsAccountDisabledOrLocked(string username)
        {
            bool found = false;
            try
            {
                    // Create a DirectoryEntry for the user
                    using (DirectoryEntry userEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                    {
                        // Set the search filter to find the user by their username
                        using (DirectorySearcher searcher = new DirectorySearcher(userEntry))
                        {
                            searcher.Filter = $"(&(objectClass=user)(samAccountName={username}))";
                            searcher.PropertiesToLoad.Add("userAccountControl");

                            SearchResult result = searcher.FindOne();

                            if (result != null)
                            {
                                int userAccountControl = (int)result.Properties["userAccountControl"][0];

                                // Check if the account is disabled or locked
                                if ((userAccountControl & 0x2) == 0x2 || (userAccountControl & 0x16) == 0x16)
                                {
                                    // Account is either disabled or locked
                                    found = true;
                                }
                            }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                found = true;
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00024");

            }

            // Account is not disabled or locked, or an error occurred
            return found;
        }


        public bool IsAccountLocked(string username)
        {
            bool isLocked = false;
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _dom.ToLower(), _usernameDomain, _password))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

                    if (user != null)
                    {
                        // Check if the user is locked out
                        isLocked = user.IsAccountLockedOut();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the operation
                // You may want to log the exception for debugging
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00062");
            }

            return isLocked;
        }


        public bool IsAccountDisabled(string username)
        {
            bool isDisabled = false;
            try
            {
                // Create a DirectoryEntry object for the root of the local Active Directory
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    
                        // Set the search filter to find the user by their username
                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            searcher.Filter = $"(&(objectClass=user)(samAccountName={username}))";
                            searcher.PropertiesToLoad.Add("userAccountControl");

                            SearchResult result = searcher.FindOne();

                            if (result != null)
                            {
                                int userAccountControl = (int)result.Properties["userAccountControl"][0];

                                // Check if the account is disabled (ACCOUNTDISABLE flag)
                                if ((userAccountControl & 0x2) == 0x2)
                                {
                                    // Account is disabled
                                    isDisabled = true;
                                }
                            }
                        }
                }
            }
            catch(Exception ex)
            {
                //isDisabled = true;
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00025");
            }

            return isDisabled;
        }

        public (string City, string Company, string Country, string Manager, string PhoneNumber, string LogonPath) GetUserCityAndCompany(string username)
        {
            string city = string.Empty;
            string company = string.Empty;
            string Country = string.Empty;
            string Manager = string.Empty;
            string PhoneNumber = string.Empty;
            string LogonPath = string.Empty;

            try
            {
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    

                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            searcher.Filter = $"(&(objectClass=user)(samAccountName={username}))";
                            searcher.PropertiesToLoad.Add("l"); // City property
                            searcher.PropertiesToLoad.Add("company"); // Company property
                            searcher.PropertiesToLoad.Add("co"); // Country property
                            searcher.PropertiesToLoad.Add("manager"); // manager property
                            searcher.PropertiesToLoad.Add("telephoneNumber"); // telephoneNumber property
                            searcher.PropertiesToLoad.Add("scriptPath"); // scriptPath property

                            SearchResult result = searcher.FindOne();
                            
                            //Setting results to a variable

                            if (result != null)
                            {
                                if (result.Properties.Contains("l"))
                                {
                                    city = result.Properties["l"][0].ToString();
                                }

                                if (result.Properties.Contains("telephoneNumber"))
                                {
                                    PhoneNumber = result.Properties["telephoneNumber"][0].ToString();
                                }

                                if (result.Properties.Contains("scriptPath"))
                                {
                                    LogonPath = result.Properties["scriptPath"][0].ToString();
                                }

                                if (result.Properties.Contains("company"))
                                {
                                    company = result.Properties["company"][0].ToString();
                                }

                                if (result.Properties.Contains("co"))
                                {
                                    Country = result.Properties["co"][0].ToString();
                                }

                                if (result.Properties.Contains("manager"))
                                {
                                    Manager = result.Properties["manager"][0].ToString();
                                }
                            }
                        }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00026");
            }

            return (city, company, Country, Manager, PhoneNumber, LogonPath);
        }

        public DateTime? GetPasswordExpirationDate(string username)
        {
            DateTime? expirationDate = null;

            try
            {
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    
                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            searcher.Filter = $"(&(objectClass=user)(samAccountName={username}))";
                            searcher.PropertiesToLoad.Add("pwdLastSet");

                            SearchResult result = searcher.FindOne();

                            if (result != null && result.Properties.Contains("pwdLastSet"))
                            {
                                long pwdLastSet = (long)result.Properties["pwdLastSet"][0];
                                expirationDate = DateTime.FromFileTimeUtc(pwdLastSet).AddDays(90); // Assuming password policy is 90 days
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00027");
            }

            return expirationDate;
        }

        public DateTime? GetLastLogin(string username)
        {
            DateTime? expirationDate = null;

            try
            {
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    
                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            searcher.Filter = $"(&(objectClass=user)(samAccountName={username}))";
                            searcher.PropertiesToLoad.Add("lastLogon");

                            SearchResult result = searcher.FindOne();

                            if (result != null && result.Properties.Contains("lastLogon"))
                            {
                                long pwdLastSet = (long)result.Properties["lastLogon"][0];
                                expirationDate = DateTime.FromFileTimeUtc(pwdLastSet).AddDays(0); // Assuming password policy is 90 days
                            }
                        }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00028");
            }

            return expirationDate;
        }

        public string GetCreationDate(string username)
        {
            string whenCreated = null;
            try
            {
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    
                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            searcher.Filter = $"(&(objectClass=user)(samAccountName={username}))";
                            searcher.PropertiesToLoad.Add("whenCreated");

                            SearchResult result = searcher.FindOne();

                            if (result != null && result.Properties.Contains("whenCreated"))
                            {
                                whenCreated = result.Properties["whenCreated"][0].ToString();
                            }
                        }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00029");
            }

            return whenCreated;
        }


        public async Task<string> GetIPAddress(string hostname)
        {
            string ipAddress = null;
            try
            {
                // Get the IP addresses associated with the given hostname
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(hostname);

                // Get the first IP address as a string
                ipAddress = addresses.FirstOrDefault()?.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00030");
            }
            return ipAddress;
        }

        public bool IsDeviceDisabledOrLocked(string deviceName)
        {
            bool found = false;
            try
            {
                // Create a DirectoryEntry object for the root of the local Active Directory
                using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                        // Set the search filter to find the device by its name
                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            searcher.Filter = $"(&(objectClass=computer)(name={deviceName}))";
                            searcher.PropertiesToLoad.Add("userAccountControl");

                            SearchResult result = searcher.FindOne();

                            if (result != null)
                            {
                                int userAccountControl = (int)result.Properties["userAccountControl"][0];

                                // Check if the device is disabled or locked
                                if ((userAccountControl & 0x2) == 0x2 || (userAccountControl & 0x16) == 0x16)
                                {
                                    // Device is either disabled or locked
                                    found = true;
                                }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                found = true;
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00031");
            }

            // Device is not disabled or locked, or an error occurred
            return found;
        }


        public string ConvertDNtoGUID(string objectDN)
        {
            // Removed logic to check existence first
            // Create a DirectoryEntry object using the provided object DN
            DirectoryEntry directoryObject = new DirectoryEntry(objectDN);
            return directoryObject.Guid.ToString();
        }

        public Dictionary<string, string> FindComputers(string searchFilterComputerName)
        {
            Dictionary<string, string> computerNames = new Dictionary<string, string>();

            try
            {
                // Connect to the Active Directory using the LDAP filter, username domain, and password
                using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        // Set the search filter to find computer objects with a matching name
                        searcher.Filter = $"(&(objectCategory=computer)(sAMAccountName=*{searchFilterComputerName}*))";
                        searcher.PropertiesToLoad.Add("sAMAccountName");
                        searcher.PropertiesToLoad.Add("distinguishedName");

                        SearchResultCollection results = searcher.FindAll();

                        if (results.Count > 0)
                        {
                            // Iterate through the search results and add computer names and DNs to the dictionary
                            foreach (SearchResult result in results)
                            {
                                string computerName = result.Properties["sAMAccountName"][0].ToString();
                                computerName = computerName.Substring(0, computerName.Length - 1);
                                string dn = result.Properties["distinguishedName"][0].ToString();
                                computerNames.Add(computerName, dn);
                            }
                        }
                        else
                        {
                            // No computers found with the specified name
                            MessageBox.Show($"There is no computer matching the name '{searchFilterComputerName}' in AD.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00032", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return computerNames;
        }


        public Dictionary<string, string> FindUsers(string searchFilterUserName)
        {
            Dictionary<string, string> userNames = new Dictionary<string, string>();

            try
            {
                // Connect to the Active Directory using the LDAP filter, username domain, and password
                using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        // Set the search filter to find users with a matching username
                        searcher.Filter = $"(&(objectCategory=user)(sAMAccountName=*{searchFilterUserName}*))";
                        searcher.PropertiesToLoad.Add("sAMAccountName");
                        searcher.PropertiesToLoad.Add("distinguishedName");

                        SearchResultCollection results = searcher.FindAll();

                        if (results.Count > 0)
                        {
                            // Iterate through the search results and add user names and DNs to the dictionary
                            foreach (SearchResult result in results)
                            {
                                string userName = result.Properties["sAMAccountName"][0].ToString();
                                string dn = result.Properties["distinguishedName"][0].ToString();
                                if (!dn.Contains("Certainty"))
                                {
                                    userNames.Add(userName, dn);
                                }
                            }
                        }
                        else
                        {
                            // No users found with the specified name
                            MessageBox.Show($"There is no user matching the name '{searchFilterUserName}' in AD.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00033", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return userNames;
        }


        public Dictionary<string, string> FindGroups(string searchFilterGroupName)
        {
            Dictionary<string, string> groupNames = new Dictionary<string, string>();

            try
            {
                // Connect to the Active Directory using the LDAP filter, username domain, and password
                using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        // Set the search filter to find groups with a matching name
                        searcher.Filter = $"(&(objectCategory=group)(cn=*{searchFilterGroupName}*))";
                        searcher.PropertiesToLoad.Add("cn");
                        searcher.PropertiesToLoad.Add("distinguishedName");

                        SearchResultCollection results = searcher.FindAll();

                        if (results.Count > 0)
                        {
                            // Iterate through the search results and add group names and DNs to the dictionary
                            foreach (SearchResult result in results)
                            {
                                string groupName = result.Properties["cn"][0].ToString();
                                string dn = result.Properties["distinguishedName"][0].ToString();
                                groupNames.Add(groupName, dn);
                            }
                        }
                        else
                        {
                            // No groups found with the specified name
                            MessageBox.Show($"There is no group matching the name '{searchFilterGroupName}' in AD.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00034", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return groupNames;
        }

        public bool AddUserToGroup(string username, string groupName)
        {
            bool UserAddedToGroup = false;

            try
            {
                // Connect to the group in the Active Directory using the group's LDAP path
                using (DirectoryEntry groupEntry = new DirectoryEntry($"LDAP://{groupName}", _usernameDomain, _password))
                {
                    using (DirectoryEntry userEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password))
                    {
                        using (DirectorySearcher userSearcher = new DirectorySearcher(userEntry))
                        {
                            // Set the search filter to find the user with a matching samAccountName
                            userSearcher.Filter = $"(&(objectCategory=user)(samAccountName={username}))";
                            SearchResult userResult = userSearcher.FindOne();

                            if (userResult != null)
                            {
                                // Get the directory entry for the found user
                                DirectoryEntry foundUser = userResult.GetDirectoryEntry();
                                // Add the user's distinguishedName to the "member" property of the group
                                groupEntry.Properties["member"].Add(foundUser.Properties["distinguishedName"].Value);
                                // Save the changes to the group
                                groupEntry.CommitChanges();
                                UserAddedToGroup = true;
                            }
                            else
                            {
                                // User not found
                                MessageBox.Show($"User '{username}' not found.", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00035", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return UserAddedToGroup;
        }


        public bool RemoveUserFromGroup(string username, string groupName)
        {
            bool RemovedFromGroup = false;
            try
            {
                // Connect to the group in the Active Directory using the group's LDAP path
                using (DirectoryEntry groupEntry = new DirectoryEntry($"LDAP://{groupName}", _usernameDomain, _password))
                {
                    using (DirectoryEntry userEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password))
                    {
                        using (DirectorySearcher userSearcher = new DirectorySearcher(userEntry))
                        {
                            // Set the search filter to find the user with a matching samAccountName
                            userSearcher.Filter = $"(&(objectCategory=user)(samAccountName={username}))";
                            SearchResult userResult = userSearcher.FindOne();

                            if (userResult != null)
                            {
                                // Get the directory entry for the found user
                                DirectoryEntry foundUser = userResult.GetDirectoryEntry();
                                // Remove the user's distinguishedName from the "member" property of the group
                                groupEntry.Properties["member"].Remove(foundUser.Properties["distinguishedName"].Value);
                                // Save the changes to the group
                                groupEntry.CommitChanges();
                                RemovedFromGroup = true;
                            }
                            else
                            {
                                // User not found
                                MessageBox.Show($"User '{username}' not found.", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00036", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return RemovedFromGroup;
        }

        //Only Powershell
        public static PSCredential CreatePSCredential(string username, string password)
        {
            // Convert the password to a SecureString
            SecureString securePassword = ConvertToSecureString(password);
            // Create a PSCredential object using the username and SecureString password
            PSCredential psCredential = new PSCredential(username, securePassword);
            return psCredential;
        }

        private static SecureString ConvertToSecureString(string password)
        {
            // Convert a string to a SecureString
            SecureString secureString = new SecureString();
            foreach (char c in password)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }


        public string GetLAPS(string computerName)
        {
            string password = string.Empty;
            try
            {
                //PSCredential pSCredential = CreatePSCredential(_username, _password);
                //Enter OU for Devices like "OU=Computers,DC=domain,DC=local"
                if (ExistsComputerInOU(computerName, "OU=Computers,OU=All Regions,DC=domain,DC=local") == true)
                {



                    string command = $"Get-LapsADPassword {computerName} -AsPlainText | Select-Object Password";

                    string result = PowershellService.RunPowerShellCommand(command, _username, _password);
                    if (result.IsNullOrEmpty())
                    {
                        password = string.Empty;
                    }
                    else
                    {
                        password = result;
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00037", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return password;
        }

        public bool ResetPassword(string userName, string newPassword)
        {
            bool checkIfReset = false;
            try
            {
                // Connect to the local Active Directory
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = $"(&(objectCategory=user)(sAMAccountName={userName}))";
                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult != null)
                {
                    // Retrieve the user's directory entry
                    DirectoryEntry userEntry = searchResult.GetDirectoryEntry();

                    // Reset the user's password
                    userEntry.Invoke("SetPassword", newPassword);
                    userEntry.CommitChanges();

                    MessageBox.Show($"The new password is: {newPassword}", "Password chnaged", MessageBoxButton.OK, MessageBoxImage.Information);

                    checkIfReset = true;
                }
                else
                {
                    MessageBox.Show($"ErrorMassage: User has not been found", "Error: 0x00039", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00038", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return checkIfReset;
        }


        public void UnlockUserAccount(string userName)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _domain, _container, ContextOptions.Negotiate, _usernameDomain, _password))
            {
                try
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);

                    if (user != null)
                    {
                        if (user.IsAccountLockedOut())
                        {
                            user.UnlockAccount();
                            user.Save();
                        }
                        else
                        {
                            MessageBox.Show("User is already unlocked");
                        }
                    }
                    else
                    {
                        MessageBox.Show("User account not found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00040");
                }
            }
        }

        public string GetOSVersion(string computerName)
        {
            string osVersion = null;
            try
            {
                // Connect to the Active Directory
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = $"(&(objectCategory=computer)(sAMAccountName={computerName}$))";
                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult != null)
                {
                    try
                    {
                        DirectoryEntry computerEntry = searchResult.GetDirectoryEntry();

                        // Check if the operatingSystemVersion attribute exists in the Properties collection
                        if (computerEntry.Properties.Contains("operatingSystemVersion"))
                        {
                            osVersion = computerEntry.Properties["operatingSystemVersion"][0].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An Error occurred: {ex}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00041", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return osVersion;
        }

        public string GetExtensionAttribute(string username)
        {
            string extensionValue = null;
            try
            {
                // Connect to the Active Directory
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = $"(&(objectCategory=user)(samaccountname={username}))";
                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult != null)
                {
                    try
                    {
                        DirectoryEntry userEntry = searchResult.GetDirectoryEntry();

                        // Check if the extension attribute exists in the Properties collection
                        if (userEntry.Properties.Contains("extensionAttribute9"))
                        {
                            extensionValue = userEntry.Properties["extensionAttribute9"][0].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An Error occurred: {ex}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00042", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return extensionValue;
        }


        public bool SetExtensionAttribute(string username)
        {
            bool check = false;
            try
            {
                // Connect to the Active Directory
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = $"(&(objectCategory=user)(samaccountname={username}))";
                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult != null)
                {
                    try
                    {
                        DirectoryEntry userEntry = searchResult.GetDirectoryEntry();

                        // Set the extension attribute value
                        userEntry.Properties["extensionAttribute9"].Value = "crutial";
                        userEntry.CommitChanges();

                        check = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00043", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00044", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return check;
        }

        public bool DisableAccount(string username, string resetPassword)
        {
            bool check = false;
            try
            {
                // Connect to the Active Directory
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = $"(&(objectCategory=user)(samaccountname={username}))";
                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult != null)
                {
                    try
                    {

                        DirectoryEntry userEntry = searchResult.GetDirectoryEntry();
                        //resets Password
                        userEntry.Invoke("SetPassword", resetPassword);
                       

                        check = true;


                        // Disable the user account
                        int userAccountControl = (int)userEntry.Properties["userAccountControl"].Value;
                        userEntry.Properties["userAccountControl"].Value = userAccountControl | 0x2; // Disable account flag
                        userEntry.CommitChanges();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00045", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00046", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return check;
        }


        public void EnableAccount(string username)
        {
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(&(objectCategory=user)(sAMAccountName={username}))";
                    searcher.PropertiesToLoad.Add("userAccountControl");

                    SearchResult result = searcher.FindOne();

                    if (result != null)
                    {
                        try
                        {
                            DirectoryEntry userEntry = result.GetDirectoryEntry();
                            // Get the current value of userAccountControl
                            int userAccountControl = (int)userEntry.Properties["userAccountControl"].Value;
                            // Enable the user account by removing the "Disabled" flag
                            userEntry.Properties["userAccountControl"].Value = userAccountControl & ~0x2; // Remove the disabled account flag
                            // Update extensionAttribute13 with leavingDate
                            userEntry.Properties["extensionAttribute13"].Value = "<not set>";
                            // Update extensionAttribute14 with terminationDate
                            userEntry.Properties["extensionAttribute14"].Value = "<not set>";
                            //sets description
                            userEntry.Properties["description"].Value = $"Enabled by: {_username}";
                            userEntry.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00047", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        public void EnableDevice(string hostname)
        {
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    // Set the filter to find the disabled device by hostname
                    searcher.Filter = $"(&(objectCategory=computer)(cn={hostname}))";
                    searcher.PropertiesToLoad.Add("userAccountControl");

                    SearchResult result = searcher.FindOne();

                    if (result != null)
                    {
                        try
                        {
                            DirectoryEntry deviceEntry = result.GetDirectoryEntry();
                            // Get the current value of userAccountControl
                            int userAccountControl = (int)deviceEntry.Properties["userAccountControl"].Value;
                            // Enable the device by removing the "Disabled" flag
                            deviceEntry.Properties["userAccountControl"].Value = userAccountControl & ~0x2; // Remove the disabled account flag
                                                                                                            // You may need to set other attributes as needed for devices, such as extensionAttribute13 and extensionAttribute14
                                                                                                            // Update extensionAttribute13 with a value if needed
                            deviceEntry.Properties["extensionAttribute13"].Value = "<not set>";
                            // Update extensionAttribute14 with a value if needed
                            deviceEntry.Properties["extensionAttribute14"].Value = "<not set>";
                            // Set the description if needed
                            deviceEntry.Properties["description"].Value = $"Enabled by: {_username}";
                            deviceEntry.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00048", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }


        public void DeleteDevice(string computerName)
        {
            using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    // Set the search filter to find the computer with a matching name
                    searcher.Filter = $"(&(objectCategory=computer)(name={computerName}))";
                    searcher.PropertiesToLoad.Add("distinguishedName");

                    SearchResult result = searcher.FindOne();

                    if (result != null && result.Properties.Contains("distinguishedName"))
                    {
                        // Get the directory entry for the device
                        DirectoryEntry deviceEntry = result.GetDirectoryEntry();
                        // Delete the device and all its child objects
                        deviceEntry.DeleteTree();
                        deviceEntry.CommitChanges();
                    }
                }
            }
        }

        public void UpdateUserAttributes(string username, string city, string country, string logonPath, string mobileNumber)
{
    try
    {
        using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
        {
            
            
                using (DirectorySearcher searcher = new DirectorySearcher(root))
                {
                    searcher.Filter = $"(&(objectClass=user)(sAMAccountName={username}))";
                    searcher.PropertiesToLoad.Add("distinguishedName");

                    SearchResult result = searcher.FindOne();

                    if (result != null)
                    {
                        string userDistinguishedName = result.Properties["distinguishedName"][0].ToString();
                        
                        using (DirectoryEntry userEntry = new DirectoryEntry($"LDAP://{userDistinguishedName}", _usernameDomain, _password, AuthenticationTypes.Secure))
                        {
                            // Update City attribute
                            if (!string.IsNullOrEmpty(city))
                            {
                                userEntry.Properties["l"].Value = city;
                            }
                            if (!string.IsNullOrEmpty(logonPath))
                            {
                                userEntry.Properties["scriptPath"].Value = logonPath;
                            }
                            if (!string.IsNullOrEmpty(mobileNumber))
                            {
                                userEntry.Properties["telephoneNumber"].Value = mobileNumber;
                            }

                            // Update Country attribute
                            if (!string.IsNullOrEmpty(country))
                            {
                                userEntry.Properties["co"].Value = country;
                            }

                            userEntry.CommitChanges();
                        }
                    }
                }
            
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00049");
    }
}


        public BitlockerKey GetBitLockerRecoveryKeyFromAAD(string BitlockerKeyID)
        {
            BitlockerKey bitlockerKey = new BitlockerKey();
            try
            {
                // Get the access token for Microsoft Graph using the identity service
                string accessToken = _identityService.GetAccessTokenForGraphAsync().ToString();
                if (accessToken != null)
                {
                    // Retrieve the BitLocker key from Microsoft Graph using the access token and BitLocker key ID
                    _MicrosoftGraphService.GetBitLockerKey(accessToken, BitlockerKeyID);
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00050", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return bitlockerKey;
        }


        public Dictionary<string, List<string>> GetAllSubOUsByRegion()
        {
            Dictionary<string, List<string>> ouByRegion = new Dictionary<string, List<string>>();
            string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
            string[] targetOUs;
            AppConfigManager.TryLoadtargetOUs(AppConfPath, out targetOUs);

            try
            {
                // Connect to the Active Directory using the LDAP filter
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectCategory=organizationalUnit)(ou=*))";
                SearchResultCollection searchResults = directorySearcher.FindAll();

                foreach (SearchResult searchResult in searchResults)
                {
                    DirectoryEntry ouEntry = searchResult.GetDirectoryEntry();
                    string ouName = ouEntry.Name.Substring(3); // Remove "OU=" prefix

                    if (Array.Exists(targetOUs, ou => ou == ouName))
                    {
                        string region = ouName;
                        if (!ouByRegion.ContainsKey(region))
                        {
                            ouByRegion[region] = new List<string>();
                        }

                        // Search for sub-OUs
                        DirectorySearcher subDirectorySearcher = new DirectorySearcher(ouEntry);
                        subDirectorySearcher.Filter = "(&(objectCategory=organizationalUnit)(ou=*))";
                        SearchResultCollection subSearchResults = subDirectorySearcher.FindAll();

                        foreach (SearchResult subSearchResult in subSearchResults)
                        {
                            DirectoryEntry subOuEntry = subSearchResult.GetDirectoryEntry();
                            string subOuName = subOuEntry.Name.Substring(3); // Remove "OU=" prefix
                            ouByRegion[region].Add(subOuName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00051");
            }

            return ouByRegion;
        }


        public async Task<(int employeeCount, int disabledUser, int activeUsersWithEmployeeIDCount, int disabledUserCount, int lockedUserCount, int expieredPasswordUsers)> CountUsersWithEmployeeIDAndLockedUsers()
        {
            int employeeCount = 0;
            int activeUsersWithEmployeeIDCount = 0;
            int disabledUserCount = 0;
            int lockedUserCount = 0;
            int disabledUser = 0;
            int expieredPasswordUsers = 0;

            await Task.Run(() =>
            {
                try
                {
                    // Create a DirectoryEntry object for the root of the local Active Directory
                    using (DirectoryEntry root = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                    {
                        
                        
                            // Create a DirectorySearcher to search for user objects
                            using (DirectorySearcher searcher = new DirectorySearcher(root))
                            {
                                // Set the filter to find user objects with the Employee ID attribute
                                searcher.Filter = "(&(objectClass=user))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of user objects in the result collection
                                employeeCount = results.Count;
                            }
                        

                        
                            // Create a DirectorySearcher to search for user objects with an expired password
                            using (DirectorySearcher searcher = new DirectorySearcher(root))
                            {
                                // Set the filter to find user objects with an expired password
                                searcher.Filter = "(&(objectClass=user)(userAccountControl:1.2.840.113556.1.4.803:=65536))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of user objects with an expired password in the result collection
                                expieredPasswordUsers = results.Count;
                            }
                        


                            // Create a DirectorySearcher to search for disabled user objects with an employeeID
                            using (DirectorySearcher searcher = new DirectorySearcher(root))
                            {
                                // Set the filter to find disabled user objects (objectClass=user and userAccountControl:1.2.840.113556.1.4.803:=2) with an employeeID
                                searcher.Filter = "(&(objectClass=user)(userAccountControl:1.2.840.113556.1.4.803:=2))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of disabled user objects with an employeeID in the result collection
                                disabledUser = results.Count;
                            }
                        

                        // Now, let's count active users with Employee ID
                       
                            // Create a DirectorySearcher to search for active user objects with Employee ID attribute
                            using (DirectorySearcher searcher = new DirectorySearcher(root))
                            {
                                // Set the filter to find active user objects (objectClass=user and not userAccountControl contains "ACCOUNTDISABLE" flag)
                                // and with Employee ID attribute (employeeID=*)
                                searcher.Filter = "(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2)(employeeID=*))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of active user objects with Employee ID attribute in the result collection
                                activeUsersWithEmployeeIDCount = results.Count;
                            }
                        

                        // Now, let's count disabled users with Employee ID
                       
                            // Create a DirectorySearcher to search for disabled user objects with an employeeID
                            using (DirectorySearcher searcher = new DirectorySearcher(root))
                            {
                                // Set the filter to find disabled user objects (objectClass=user and userAccountControl:1.2.840.113556.1.4.803:=2) with an employeeID
                                searcher.Filter = "(&(objectClass=user)(userAccountControl:1.2.840.113556.1.4.803:=2)(employeeID=*))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of disabled user objects with an employeeID in the result collection
                                disabledUserCount = results.Count;
                            }
                        

                        // Now, let's count locked users
                        
                            // Create a DirectorySearcher to search for locked user objects
                            using (DirectorySearcher searcher = new DirectorySearcher(root))
                            {
                                // Set the filter to find locked user objects (objectClass=user and userAccountControl:1.2.840.113556.1.4.804:=16)
                                searcher.Filter = "(&(objectClass=user)(userAccountControl:1.2.840.113556.1.4.804:=16)(employeeID=*))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of locked user objects in the result collection
                                lockedUserCount = results.Count;
                           }
                        
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the operation
                    MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00052");
                }
            });

            return (employeeCount, disabledUser, activeUsersWithEmployeeIDCount, disabledUserCount, lockedUserCount, expieredPasswordUsers);
        }




        public async Task<(int deviceCount, int testDeviceCount, int serverCount, int dcCount)> CountDevicesServersAndDCsInOuAllRegions(string domainName)
        {
            int deviceCount = 0;
            int testDeviceCount = 0;
            int serverCount = 0;
            int dcCount = 0;

            await Task.Run(() =>
            {
                try
                {
                    // Create a DirectoryEntry object for the root of the local Active Directory
                    using (DirectoryEntry root = new DirectoryEntry("LDAP://RootDSE", _usernameDomain, _password, AuthenticationTypes.Secure))
                    {
                        string defaultNamingContext = root.Properties["defaultNamingContext"].Value.ToString();

                        // Construct the LDAP path for the Computers container in the local Active Directory
                        string computersContainerPath = $"OU=All Regions,{defaultNamingContext}";

                        // Append the additional OU "All Regions" to the path
                        string allRegionsPath = $"LDAP://OU=Computers,{computersContainerPath}";

                        using (DirectoryEntry allRegionsContainer = new DirectoryEntry(allRegionsPath))
                        {
                            // Create a DirectorySearcher to search for computer objects in the "All Regions" OU
                            using (DirectorySearcher searcher = new DirectorySearcher(allRegionsContainer))
                            {
                                // Set the filter to find all computer objects (objectClass=computer)
                                searcher.Filter = "(&(objectClass=computer))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of computer objects in the result collection
                                deviceCount = results.Count;

                                // Now, let's count test devices
                                foreach (SearchResult result in results)
                                {
                                    string computerName = result.Properties["name"][0].ToString();

                                    // Check if the computer name contains "TEST"
                                    if (computerName.Contains("TEST", StringComparison.OrdinalIgnoreCase))
                                    {
                                        testDeviceCount++;
                                    }
                                }
                            }
                        }

                        // Now, let's count servers and DCs in the same OU
                        using (DirectorySearcher serverSearcher = new DirectorySearcher(defaultNamingContext))
                        {
                            // Set the filter to find server objects (objectClass=computer and operatingSystem=*Server*)
                            serverSearcher.Filter = "(&(objectClass=computer)(operatingSystem=*Server*))";

                            // Set the PageSize property to handle large result sets more efficiently
                            serverSearcher.PageSize = 500;

                            // Perform the search and retrieve the result collection
                            SearchResultCollection serverResults = serverSearcher.FindAll();

                            // Count the number of server objects in the result collection
                            serverCount = serverResults.Count;
                        }

                        // Now, let's count Domain Controllers (DCs) in the same OU
                        DomainControllerCollection dcc = DomainController.FindAll(new DirectoryContext(DirectoryContextType.Domain, domainName));
                        foreach(DomainController dc in dcc)
                        {
                            dcCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the operation
                    MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00053");
                }
            });

            return (deviceCount, testDeviceCount, serverCount, dcCount);
        }


        public async Task<int> CountDisabledDevicesInOuAllRegions()
        {
            int disabledDeviceCount = 0;
            await Task.Run(() =>
            {
                try
                {
                    // Create a DirectoryEntry object for the root of the local Active Directory
                    using (DirectoryEntry root = new DirectoryEntry("LDAP://RootDSE", _usernameDomain, _password, AuthenticationTypes.Secure))
                    {
                        string defaultNamingContext = root.Properties["defaultNamingContext"].Value.ToString();

                        // Construct the LDAP path for the Computers container in the local Active Directory
                        string computersContainerPath = $"OU=All Regions,{defaultNamingContext}";

                        // Append the additional OU "All Regions" to the path
                        string allRegionsPath = $"LDAP://OU=Computers,{computersContainerPath}";

                        using (DirectoryEntry allRegionsContainer = new DirectoryEntry(allRegionsPath, _usernameDomain, _password, AuthenticationTypes.Secure))
                        {
                            // Create a DirectorySearcher to search for disabled computer objects in the "All Regions" OU
                            using (DirectorySearcher searcher = new DirectorySearcher(allRegionsContainer))
                            {
                                // Set the filter to find disabled computer objects (objectClass=computer and userAccountControl:1.2.840.113556.1.4.803:=2)
                                searcher.Filter = "(&(objectClass=computer)(userAccountControl:1.2.840.113556.1.4.803:=2))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of disabled computer objects in the result collection
                                disabledDeviceCount = results.Count;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the operation
                    MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00054");
                }
            });
            return disabledDeviceCount;
        }


        public async Task<int> CountDevicesInOuAllRegionsPrebuild()
        {
            int deviceCount = 0;
            await Task.Run(() =>
            {
                try
                {
                    // Create a DirectoryEntry object for the root of the local Active Directory
                    using (DirectoryEntry root = new DirectoryEntry("LDAP://RootDSE", _usernameDomain, _password, AuthenticationTypes.Secure))
                    {
                        string defaultNamingContext = root.Properties["defaultNamingContext"].Value.ToString();

                        // Construct the LDAP path for the Computers container in the local Active Directory
                        string computersContainerPath = $"OU=All Regions,{defaultNamingContext}";

                        // Append the additional OU "All Regions" to the path
                        string allRegionsPath = $"LDAP://OU=Computers Prebuilt,OU=Computers,{computersContainerPath}";

                        using (DirectoryEntry allRegionsContainer = new DirectoryEntry(allRegionsPath))
                        {
                            // Create a DirectorySearcher to search for computer objects in the "All Regions" OU
                            using (DirectorySearcher searcher = new DirectorySearcher(allRegionsContainer))
                            {
                                // Set the filter to find computer objects (objectClass=computer)
                                searcher.Filter = "(&(objectClass=computer))";

                                // Set the PageSize property to handle large result sets more efficiently
                                searcher.PageSize = 500;

                                // Perform the search and retrieve the result collection
                                SearchResultCollection results = searcher.FindAll();

                                // Count the number of computer objects in the result collection
                                deviceCount = results.Count;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the operation
                    MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00055");
                }


            });
            return deviceCount;
        }

        



        public async Task<List<string>> GetLast3SecurityEventLogs()
        {
            List<string> last3Changes = new List<string>();
            await Task.Run(() =>
            {
                try
                {
                    
                    //{
                        // Get the last 3 entries from the event log
                        //int totalEntries = eventLog.Entries.Count;
                        //int startIndex = Math.Max(totalEntries - 3, 0);

                        //for (int i = startIndex; i < totalEntries; i++)
                        //{
                            //EventLogEntry entry = eventLog.Entries[i];
                            //last3Changes.Add(entry.Message);

                        //}
                    //}
                }
                catch (Exception ex)
                {
                    last3Changes = null;

                }
            });

            return last3Changes;
        }

        public async Task<List<PrebuildComputers>> GetComputersInOuAllRegionsPrebuilt()
        {
            List<PrebuildComputers> devices = new List<PrebuildComputers>();
            try
            {
                // Create a DirectoryEntry object for the specified LDAP path
                //TODO: Enter where new generate Device are in your Domain Like "LDAP://OU=Computers Prebuilt,OU=Computers,DC=domain,DC=local"
                using (DirectoryEntry entry = new DirectoryEntry("LDAP://OU=Computers Prebuilt,OU=Computers,OU=All Regions,DC=domain,DC=local", _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        // Set the search filter to find computer objects with a matching name
                        searcher.Filter = $"(&(objectCategory=computer))";

                        // Set the properties to load
                        searcher.PropertiesToLoad.AddRange(new string[] { "sAMAccountName", "displayName", "whenCreated", "description" , "distinguishedName" });

                        // Perform the search and retrieve the result collection
                        SearchResultCollection results = searcher.FindAll();
                        if (results.Count > 0)
                        {
                            // Process each device object in the result collection
                            foreach (SearchResult result in results)
                            {
                                PrebuildComputers deviceInfo = new PrebuildComputers();

                                if (result.Properties["sAMAccountName"].Count > 0) {
                                    string computerName = result.Properties["sAMAccountName"][0].ToString();
                                    computerName = computerName.Substring(0, computerName.Length - 1);
                                    deviceInfo.ComputerName = computerName;
                                }

                                if(result.Properties["distinguishedName"].Count > 0)
                                    deviceInfo.CreationDate = result.Properties["distinguishedName"][0].ToString();

                                if (result.Properties["whenCreated"].Count > 0) {
                                    deviceInfo.CreationDate = result.Properties["whenCreated"][0].ToString();

                                    bool MoveNeeded = IsDateTime48HoursInThePast(result.Properties["whenCreated"][0].ToString());
                                    if (MoveNeeded)
                                    {
                                        deviceInfo.NeedMove = "Red";
                                    }
                                    else
                                    {
                                        deviceInfo.NeedMove = "LimeGreen";
                                    }

                                }
                                if (result.Properties["description"].Count > 0)
                                    deviceInfo.Description = result.Properties["description"][0].ToString();

                                

                                devices.Add(deviceInfo);
                            }
                        }
                        else
                        {
                            // No computers found with the specified name
                            MessageBox.Show($"The Prebuild OU is empty", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00056");
            }

            return devices;
        }

        public async Task<List<DisabledDevcies>> GetDisabledComputersWithExtensionAttribute9()
        {
            List<DisabledDevcies> disabledDevicesList = new List<DisabledDevcies>();

            try
            {
                // Connect to the Active Directory
                //Enter OU for Domain like "LDAP://DC=domain,DC=local"
                DirectoryEntry directoryEntry = new DirectoryEntry("LDAP://DC=domain,DC=local", _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);

                // Set the search filter to find disabled computer accounts
                directorySearcher.Filter = "(&(objectCategory=computer)(userAccountControl:1.2.840.113556.1.4.803:=2))";

                // Set the properties to load, including extensionAttribute9
                directorySearcher.PropertiesToLoad.AddRange(new[] { "cn", "extensionAttribute9" });

                // Perform the search and retrieve the result collection
                SearchResultCollection results = directorySearcher.FindAll();

                foreach (SearchResult result in results)
                {
                    if (result.Properties.Contains("cn"))
                    {
                        string hostname = result.Properties["cn"][0].ToString();
                        string extensionAttribute9 = result.Properties.Contains("extensionAttribute9")
                            ? result.Properties["extensionAttribute9"][0].ToString()
                            : string.Empty;

                        DisabledDevcies device = new DisabledDevcies
                        {
                            Hostname = hostname,
                            extensionAttribute9 = extensionAttribute9
                        };

                        disabledDevicesList.Add(device);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00057", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return disabledDevicesList;
        }


        public async Task<List<DisabledUsers>> GetDisabledUsersWithExtensionAttribute9()
        {
            List<DisabledUsers> disabledUsersList = new List<DisabledUsers>();

            try
            {
                // Connect to the Active Directory
                DirectoryEntry directoryEntry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);

                // Set the search filter to find disabled user accounts
                directorySearcher.Filter = "(&(objectCategory=user)(userAccountControl:1.2.840.113556.1.4.803:=2))";

                // Set the properties to load, including extensionAttribute9
                directorySearcher.PropertiesToLoad.AddRange(new[] { "samaccountname", "extensionAttribute9" });

                // Perform the search and retrieve the result collection
                SearchResultCollection results = directorySearcher.FindAll();

                foreach (SearchResult result in results)
                {
                    if (result.Properties.Contains("samaccountname"))
                    {
                        string userName = result.Properties["samaccountname"][0].ToString();
                        string extensionAttribute9 = result.Properties.Contains("extensionAttribute9")
                            ? result.Properties["extensionAttribute9"][0].ToString()
                            : string.Empty;

                        DisabledUsers user = new DisabledUsers
                        {
                            UserName = userName,
                            extensionAttribute9 = extensionAttribute9
                        };

                        disabledUsersList.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00058", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return disabledUsersList;
        }


        public async Task<List<Groups>> GetAllUsersGroupAsync(string username)
        {
            List<Groups> usergroup = new List<Groups>();

            try
            {
                // Create a DirectoryEntry object for the specified LDAP path
                using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password, AuthenticationTypes.Secure))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        // Set the search filter to find user objects with a matching name
                        searcher.Filter = $"(&(objectCategory=user)(sAMAccountName={username}))";

                        // Set the properties to load
                        searcher.PropertiesToLoad.AddRange(new string[] { "sAMAccountName", "displayName", "memberOf" });

                        // Perform the search and retrieve the result collection
                        SearchResultCollection results = searcher.FindAll();
                        if (results.Count > 0)
                        {
                            // Process each user object in the result collection
                            foreach (SearchResult result in results)
                            {
                                string[] memberOf = result.Properties["memberOf"] != null
                                    ? result.Properties["memberOf"].Cast<string>().ToArray()
                                    : new string[0];


                                foreach (string groupDistinguishedName in memberOf)
                                {
                                    using (DirectoryEntry groupEntry = new DirectoryEntry($"LDAP://{groupDistinguishedName}", _usernameDomain, _password, AuthenticationTypes.Secure))
                                    {
                                        // Create a new Groups object for each group
                                        Groups groups = new Groups();
                                        // Get the group name from the distinguished name
                                        groups.dsName = groupDistinguishedName;
                                        groups.Gropname = groupEntry.Name.Substring(3);

                                        usergroup.Add(groups);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // No users found with the specified name
                            MessageBox.Show($"No user found with the specified name", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00059");
            }

            return usergroup;
        }


        public static bool IsDateTime48HoursInThePast(string dateTimeString)
        {
            // Parse the string date and time into a DateTime object
            if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
            {
                // Get the current date and time
                DateTime currentDateTime = DateTime.Now;

                // Subtract 48 hours from the current date and time
                DateTime targetDateTime = currentDateTime.AddHours(-48);

                // Compare the target date and time with the parsed date and time
                return dateTime <= targetDateTime;
            }

            // If parsing fails, return false (or handle the error as needed)
            return false;
        }



        public bool MoveComputerToOu(string computerName)
        {
            bool movedSuccessfully = false;

            // Construct the LDAP path for the destination OU
            //Enter OU for Computers like "LDAP://OU=Computers,OU=All Regions,DC=domain,DC=local"
            string destinationOuPath = "LDAP://OU=Computers,OU=All Regions,DC=domain,DC=local";

            try
            {
                // Connect to the Active Directory using the LDAP filter, username domain, and password
                using (DirectoryEntry entry = new DirectoryEntry(_LDAPFilter, _usernameDomain, _password))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        // Set the search filter to find a computer with a matching name
                        search.Filter = $"(&(objectCategory=computer)(name={computerName}))";
                        search.PropertiesToLoad.Add("distinguishedName");
                        search.PropertiesToLoad.Add("description");

                        SearchResult result = search.FindOne();

                        if (result != null)
                        {
                            // Get the distinguished name and description of the computer
                            string distinguishedName = result.Properties["distinguishedName"][0].ToString();
                            string description = result.Properties["description"]?.Count > 0 ? result.Properties["description"][0].ToString() : null;

                            // Create a DirectoryEntry object for the destination OU
                            using (DirectoryEntry destinationOu = new DirectoryEntry(destinationOuPath, _usernameDomain, _password))
                            {
                                // Move the computer object to the destination OU
                                DirectoryEntry computer = new DirectoryEntry($"LDAP://{distinguishedName}", _usernameDomain, _password);
                                computer.MoveTo(destinationOu);

                                // Remove the description from the computer object
                                if (!string.IsNullOrEmpty(description))
                                {
                                    computer.Properties["description"].Clear();
                                    computer.CommitChanges();
                                }

                                // Commit the changes
                                computer.CommitChanges();
                                movedSuccessfully = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the operation
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00060");
            }

            return movedSuccessfully;
        }

        public async Task<Dictionary<string, long>> GetLatencyToDomainControllersAsync(string domainName)
        {
            Ping pingSender = new Ping();
            List<string> domainControllerNames = new List<string>();
            List<long> latencies = new List<long>();
            Dictionary<string, long> latencyDictionary = new Dictionary<string, long>();

            try
            {
                // Get a list of domain controllers in the domain
                DomainControllerCollection dcc = DomainController.FindAll(new DirectoryContext(DirectoryContextType.Domain, domainName));

                foreach (DomainController dc in dcc)
                {
                    string dcHostName = dc.Name;

                    try
                    {
                        PingReply reply = await pingSender.SendPingAsync(dcHostName);

                        if (reply.Status == IPStatus.Success)
                        {
                            domainControllerNames.Add(dcHostName);
                            latencies.Add(reply.RoundtripTime);
                        }
                    }
                    catch (PingException)
                    {
                        // Handle ping failure if needed
                    }
                }

                // Create a dictionary from the two lists
                for (int i = 0; i < domainControllerNames.Count; i++)
                {
                    latencyDictionary.Add(domainControllerNames[i], latencies[i]);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00061");
            }

            return latencyDictionary;
        }





    }

}

