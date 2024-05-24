using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Diagnostics;
using ADTool.Contracts.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.DirectoryServices;

namespace ADTool.Services
{
    public class EvelationService : IEvelationService
    {
        private readonly IIdentityService _identityService;
        private readonly IMicrosoftGraphService _MicrosoftGraphService;

        

        public EvelationService(IIdentityService identityService, IMicrosoftGraphService microsoftGraphService)
        {
            
            _identityService = identityService;
            _MicrosoftGraphService = microsoftGraphService;
        }

        public async Task<bool> ElevationSettingsPage()
        {
            bool isElevated = true;
            try
            {
                // Get the access token for Microsoft Graph using the identity service
                string accessToken = await _identityService.GetAccessTokenForGraphAsync();

                // Retrieve the user information and elevation settings from Microsoft Graph
                User user = await _MicrosoftGraphService.GetUserInfoAsync(accessToken);
                Members members = await _MicrosoftGraphService.GetEvelationHigh(accessToken);

                if (members != null && members.value != null)
                {
                    string userId = user.Id;
                    foreach (Member member in members.value)
                    {
                        string id = member.id;
                        if (id != null && id == userId)
                        {
                            // User is elevated
                            isElevated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00066", MessageBoxButton.OK, MessageBoxImage.Error);
                isElevated = false;
            }

            return isElevated;
        }

        public async Task<bool> ElevationApplicationTab()
        {
            bool isElevated = false;
            try
            {
                // Get the access token for Microsoft Graph using the identity service
                string accessToken = await _identityService.GetAccessTokenForGraphAsync();

                // Retrieve the user information and elevation settings from Microsoft Graph
                User user = await _MicrosoftGraphService.GetUserInfoAsync(accessToken);
                Members members = await _MicrosoftGraphService.GetEvelationMinior(accessToken);

                if (members != null && members.value != null)
                {
                    string userId = user.Id;
                    foreach (Member member in members.value)
                    {
                        string id = member.id;
                        if (id != null && id == userId)
                        {
                            // User is elevated
                            isElevated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00067", MessageBoxButton.OK, MessageBoxImage.Error);
                isElevated = false;
            }

            return isElevated;
        }

        


    }
}
