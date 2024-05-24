using System.IO;
using System;
using Microsoft.Graph.Models;
using System.Net;
using System.Security;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using System.Windows;
using Azure;
using Newtonsoft.Json;
using ADTool.Contracts.Services;
using ADTool.Core.Services;
using Microsoft.Graph.Identity.ApiConnectors.Item.UploadClientCertificate;
using Azure.Core;
using ControlzEx.Standard;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ADTool.Services
{
    public class SharePointFileService : ISharePointFileService
    {
        private readonly IIdentityService _identityService;
        private readonly IMicrosoftGraphService _MicrosoftGraphService;

        private readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;
        private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");

        public SharePointFileService(IIdentityService identityService, IMicrosoftGraphService microsoftGraphService)
        {
            _identityService = identityService;
            _MicrosoftGraphService = microsoftGraphService;
        }

        public async Task<string> GetFileBody()
        {
            try
            {
                // Get the access token for Microsoft Graph using the identity service
                string accessToken = await _identityService.GetAccessTokenForGraphAsync();
                if (accessToken == null)
                {
                    return null;
                }
                else
                {
                    // Retrieve the file ID for the specified file name from SharePoint
                    FileId responseHeader = await _MicrosoftGraphService.GetFileIDFromSharepoint(accessToken, "AppConf.json");
                    // Get the body/content of the file from SharePoint
                    string body = await _MicrosoftGraphService.GetFileFromSharepoint(accessToken, responseHeader.id);
                    return body;
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00008", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public byte[] ConvertStringToByteArray(string inputString)
        {
            // Choose the appropriate encoding (e.g., UTF8, ASCII, Unicode, etc.)
            Encoding encoding = Encoding.UTF8;

            // Convert the string to a byte array using the chosen encoding
            byte[] byteArray = encoding.GetBytes(inputString);

            return byteArray;
        }

        public async Task<bool> UpdateNewUserDetails(string contetnt)
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
                    // Retrieve the file ID for the specified file name from SharePoint
                    byte[] newContent = ConvertStringToByteArray(contetnt);

                    return await _MicrosoftGraphService.AddLineToFileOnSharePointGraph(accessToken, newContent, "UAMNewUser", "NewUserDetails.csv");
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00009", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateDeleteUserDetails(string contetnt)
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
                    
                    byte[] newContent = ConvertStringToByteArray(contetnt);

                    return await _MicrosoftGraphService.AddLineToFileOnSharePointGraph(accessToken, newContent, "UAMNewUser", "DeleteUserDetails.csv");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00010", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateCreateSharedMailbox(string contetnt)
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
                    byte[] newContent = ConvertStringToByteArray(contetnt);


                    bool check = await _MicrosoftGraphService.AddLineToFileOnSharePointGraph(accessToken, newContent, "O365", "CreateSharedMailboxDetails.csv");
                    return check;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x000073", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateCreateTeamsgroup(string contetnt)
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
                    
                    byte[] newContent = ConvertStringToByteArray(contetnt);

                    // Create a byte array containing the newline character

                    return await _MicrosoftGraphService.AddLineToFileOnSharePointGraph(accessToken, newContent, "O365", "CreateTeamsGroupDetails.csv");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x000073", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        public async Task<bool> UpdateLogFile(string contetnt, string username)
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

                    byte[] newContent = ConvertStringToByteArray(contetnt);

                    // Create a byte array containing the newline character

                    return await _MicrosoftGraphService.AddLineToFileOnSharePointGraph(accessToken, newContent, "Logs", $"{username}.csv");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x000073", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task UploadConfig(byte[] fileBytes)
        {
            try
            {
                // Get the access token for Microsoft Graph using the identity service
                string accessToken = await _identityService.GetAccessTokenForGraphAsync();
                if (accessToken != null)
                {
                    // Upload the file to SharePoint using the access token
                    bool test = await _MicrosoftGraphService.UploadFileToSharePointConfig(accessToken, fileBytes);

                    if (test == false)
                    {
                        // Display an error message if the config upload fails
                        MessageBox.Show($"ErrorMassage: Config upload failed", "Error: 0x00011", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00012", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        
    }

}        

