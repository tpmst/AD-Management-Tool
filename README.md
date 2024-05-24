# AD-Management-Tool
Manage Users and Devices in your AD 

# Application Functionality Overview

This README provides an overview of the functionalities offered by the application.

### User Account Management
- **Create and Modify User Accounts:** Allows users to create and modify user accounts with various attributes such as city, telephone number, etc.
- **Retrieve User Attributes:** Enables retrieval of user attributes like city, telephone number, etc.

### Group Membership Management
- **Add and Remove Users from Groups:** Facilitates adding and removing users from different groups.
- **Retrieve Group Memberships:** Allows retrieval of group memberships for a given user.

### Device Management
- **Check Device IP Address:** Provides functionality to check the IP address of a device.
- **Retrieve Device Operating System Version:** Enables retrieval of the operating system version of a device.
- **Check Device Status:** Allows checking whether a device is disabled or locked.

### Additional Functionalities
- **Read BitLocker Keys:** Capability to read BitLocker keys for device encryption.
- **Create Shared Mailboxes and Teams Groups:** Allows creation of shared mailboxes and Teams groups.
- **Add Users to Shared Mailboxes:** Facilitates adding users to shared mailboxes.
- **Utilize Azure Automation:** Integration with Azure Automation for task automation.

### Automation Tools
- **AutoHotKey:** Used for automating keystrokes.
- **PowerShell:** Executing commands within the application.

### Integration with Azure Services
- **Graph API:** Utilization of Graph API for interactions with Azure services.

### Security and Compliance
- **256-bit Encryption:** Ensures security by using 256-bit encryption for account data.
- **Audit Logging:** Logs all user actions for auditing purposes.

### User Interface
- **Graphical User Interface (GUI):** Offers a user-friendly interface for ease of use.

### Documentation
- **User Documentation:** Provides comprehensive user documentation for reference.


# Application Setup
This guide will walk you through the process of creating an Azure application and configuring the application client ID in the appsettings.json file for your project.

### Prerequisites
Before you begin, make sure you have the following:
- An Azure account with sufficient permissions to create applications and manage permissions.
- Visual Studio or any text editor to edit the appsettings.json file in your project.

### Step 1: Create Azure Application
1. Log in to the Azure portal (https://portal.azure.com).
2. Navigate to Azure Active Directory.
3. Click on "App registrations" and then "New registration".
4. Enter a name for your application and select the appropriate account type.
5. Under "Redirect URI", select the appropriate type (usually Web) and enter the URL where authentication responses should be sent.
6. Click "Register" to create the application.

### Step 2: Configure Application Permissions
1. Once the application is created, navigate to the "API permissions" tab.
2. Click on "Add a permission" and select "Microsoft Graph".
3. Choose the permissions required for your application:
   - user.read
   - group.read.all
   - sites.readwrite.all
   - files.readwrite.all
4. Click "Add permissions" to grant the selected permissions to your application.

### Step 3: Retrieve Application Client ID
1. After configuring permissions, navigate to the "Overview" tab of your application.
2. Note down the "Application (client) ID". This will be used to configure the application in your project.

### Step 4: Configure Application Client ID
1. Open the appsettings.json file in your project.
2. Find the section where application settings are defined.
3. Replace the placeholder for the "ClientId" with the Application ID obtained in Step 3.
4. Create a Evelation Group and add yourself to it.
5. Change the variable "_apiServiceEvelationSettings" in "MicrosoftGraphService.cs" to the GroupID of your group. 

### Step 5:
1. **Login to SharePoint:** 
   - Log in to your SharePoint account using your credentials.

2. **Create a New Document Library:**
   - Navigate to the site where you want to create the folders.
   - Click on "Site Contents" or "Documents" depending on your SharePoint version.
   - Click on "New" and select "Document library".
   - Name the document library as desired, e.g., "Shared Documents".
   - Click on "Create" to create the document library.

3. **Create Folders:**
   - Open the document library you just created.
   - Click on "New" and select "Folder".
   - Name the folder "Conf" and press Enter to create it.
   - Repeat the above step to create folders for "Logs", "O365", "UAMNewUser", and "UpdateFile".
   - Enter the logfiles for the user using the Application like "username.csv" in the "Logs" Folder

4. **Create Config File:**
   1. Create a file called AppConf.json in the Folder "Conf"
   2. Download the file AppConf.json from the reposetory
   3. Change it so it works for your domain
   4. if there are any fields you dont need let chnage the Parameter to "Unknown"



5. **Connect Sharepoint to Application:**
   - Enter the Site and DriveID of the sharepointsite into the variables in the file MicrosoftGraphService.cs

### Step 6: Save and Test
1. Save the changes to the appsettings.json file.
2. Build and run your project to ensure that the application is configured correctly and able to authenticate with Azure using the provided client ID.


# Contributors
- Tom Stiefel
