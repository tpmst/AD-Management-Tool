using System.Collections;
using System.Windows.Controls;
using System.Windows.Input;
using ADTool.ViewModels;
using ADTool.Contracts.Services;
using ADTool.Models;
using CommunityToolkit.Mvvm.Input;
using ADTool.Services;
using System.DirectoryServices.ActiveDirectory;
using ADTool.Contracts.ViewModels;
using System.Windows;
using Microsoft.Extensions.Options;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Models;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using static ADTool.Services.EvelationService;
using ADTool.Helpers;
using Microsoft.Graph.Models;

namespace ADTool.Views;

public partial class SettingsPage : Page
{
    private readonly IADServices _ADServices;
    private readonly ISharePointFileService _SharePointFileService;
    private readonly IEvelationService _evelationservice;
    private readonly ISampleDataService _sampleDataService;

    public static string domainController;
    public static int lenght;

    private string[] targetOUItems;
    private string[] companyItems;
    private string[] CountrysItems;
    private string[] branches;
    private string[] ClearItems;
    private string[] DisabledSitesItemToAdd;
    private string[] DisabledSitesItem;
    Dictionary<string, string> CityBranch = new Dictionary<string, string>();
    string[] countries;

    private string standartDC;
    private string LAPSController;
    private int _passwordLength;
    bool Evelationtest;
    string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
    string AppConfTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppConTemp.json");
    public readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;


    public SettingsPage(SettingsViewModel viewModel, IADServices aDServices, ISharePointFileService sharePointFileService, IEvelationService evelationService, ISampleDataService sampleDataService)
    {
        try
        {
            InitializeComponent();
            DataContext = viewModel;
            _ADServices = aDServices;
            _SharePointFileService = sharePointFileService;
            _evelationservice = evelationService;
            _sampleDataService = sampleDataService;
            Checkevelation();
            AppConfigManager.TryLoadCityBranch(AppConfPath, out CityBranch);
            AppConfigManager.TryLoadCountry(AppConfPath, out countries);
            AppConfigManager.TryLoadAppConSettingPage(AppConfPath, out string standartDC, out string LAPSController, out string dom, out string container, out string domain,
            out int _passwordLength,
            out string _passwordComplexity,
            out string[] _targetOU,
            out string _logPath,
            out string[] _company);
            AppConfigManager.TryLoadDisabledSites(AppConfPath, out DisabledSitesItem);
            PopulateComboBoxDomainController();
            PopulateDashboardItems();
            SetSlidervalue();
            PopulateBranch();
            PopulateComboboxCountrys();
            populateFButtons();
            populateDisabledSitesComboBox();
            targetOUItems = _targetOU;
            targetOUsBox.ItemsSource = targetOUItems;
            targetOUsBox.SelectedIndex = 0;
            companyItems = _company;
            CountrysItems = countries;
            companyBox.ItemsSource = companyItems;
            companyBox.SelectedIndex = 0;
            Domain_input.Text = domain;
            Container_Input.Text = container;
            DomainUsers.Text = dom;
            LogPath_input.Text = _logPath;
            PasswordChar.Text = _passwordComplexity;
            PasswordLength.Value = _passwordLength;
            DCSlection.SelectedItem = standartDC;
        }catch(Exception ex)
        {
            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00086", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    //Standart Sitefunctions

    private async void Checkevelation()
    {
        Evelationtest = await _evelationservice.ElevationSettingsPage();


        if (Evelationtest)
        {
            SettingsGrid.Visibility = Visibility.Visible;
            SettingsGrid.IsEnabled = true;
        }
    }

    public async void saveSettingsUser(object sender, EventArgs e)
    {
        try
        {


            if (Select_Item_1.SelectedItem != null && Select_Item_2.SelectedItem != null && Select_Item_3.SelectedItem != null)
            {
                Dictionary<int, string> comboValues = await ComboValues();

                int selectedPage1 = comboValues.FirstOrDefault(x => x.Value == Select_Item_1.SelectedItem.ToString()).Key;
                int selectedPage2 = comboValues.FirstOrDefault(x => x.Value == Select_Item_2.SelectedItem.ToString()).Key;
                int selectedPage3 = comboValues.FirstOrDefault(x => x.Value == Select_Item_3.SelectedItem.ToString()).Key;

                int _AllComputers;
                int _AllUsers;
                int _DisabledUsers;
                int _AllActiveUsers;
                int _AllDisbaledUsers;
                int _AllDisabledComps;
                int _AllPrebuild;
                List<string> ChangeLogs;
                Dictionary<string, long> DCLatency;
                int _AllDC;
                int _AllServer;
                int _AllTest;
                int _AllLockedUsers;
                int _expieredPasswordUsers;

                string _lastPass;
                AutoCopyConfManager.TryLoadLastPassword(out _lastPass);
                AutoCopyConfManager.SaveAutoCopyConf(_lastPass, keyDictionary[Select_Item_Key.SelectedItem.ToString()], keyDictionary[Select_Item_KeylocalAdmin.SelectedItem.ToString()]);

                DashboardDataManager.TryLoadDashboardDClatency(out DCLatency);
                DashboardDataManager.TryLoadDashboardChangeLogs(out ChangeLogs);
                DashboardDataManager.TryLoadDashboardDataComp(out _AllComputers, out _AllUsers, out _DisabledUsers, out _AllActiveUsers, out _AllDisbaledUsers, out _AllServer, out _AllDC, out _AllTest, out _AllLockedUsers, out _expieredPasswordUsers);
                DashboardDataManager.TryLoadDashboardData(out _AllComputers, out _AllUsers, out _AllActiveUsers, out _AllDisbaledUsers, out _AllDisabledComps, out _AllPrebuild);
                DashboardDataManager.SaveDashboardData(_AllComputers, _AllUsers, _DisabledUsers, _AllActiveUsers, _AllPrebuild, _AllDisbaledUsers, _AllDisabledComps, selectedPage1, selectedPage2, selectedPage3, ChangeLogs, DCLatency, _AllTest, _AllServer, _AllDC, _AllLockedUsers, _expieredPasswordUsers);
            }
        }
        catch(Exception ex)
        {
            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00085", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void saveSettings(object sender, RoutedEventArgs e)
    {
        try
        {

            Dictionary<string, List<string>> ouByRegion = new Dictionary<string, List<string>>();

            if (Sharepoint.IsChecked == true)
            {
                _SavePointLogs = true;
            }
            if (SharepointQnap.IsChecked == true)
            {
                _SavePointLogs = false;
            }
            ouByRegion = _ADServices.GetAllSubOUsByRegion();
            try
            {
                AppConfigManager.SaveAppconfigSettinsPageENC(AppConfPath, domainController, lenght, PasswordChar.Text, Domain_input.Text, Container_Input.Text, DomainUsers.Text, targetOUsBox.Items.OfType<string>().ToArray(), LogPath_input.Text, companyBox.Items.OfType<string>().ToArray(), "usercreds.json", "evelation.json", ouByRegion, _SavePointLogs, CityBranch, countries, LAPSController, DisabledSitesBox.Items.OfType<string>().ToArray());
                AppConfigManager.SaveAppconfigSettinsPage(AppConfTempPath, domainController, lenght, PasswordChar.Text, Domain_input.Text, Container_Input.Text, DomainUsers.Text, targetOUsBox.Items.OfType<string>().ToArray(), LogPath_input.Text, companyBox.Items.OfType<string>().ToArray(), "usercreds.json", "evelation.json", ouByRegion, _SavePointLogs, CityBranch, countries, LAPSController, DisabledSitesBox.Items.OfType<string>().ToArray());

                byte[] content = File.ReadAllBytes(AppConfTempPath);
                File.Delete(AppConfTempPath);

                await _SharePointFileService.UploadConfig(content);
            }
            catch(Exception cex)
            {
                MessageBox.Show($"ErrorMassage: {cex.Message}", "Error: 0x00084", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00069", MessageBoxButton.OK, MessageBoxImage.Error);
        }


    }

    //Location Functions

    private void PopulateBranch()
    {
        Branch_ComboBox.ItemsSource = CityBranch.Keys;
        Branch_ComboBox.SelectedIndex = 0;
    }

    private void AddCompany(object Sender, RoutedEventArgs e)
    {
        CompanyLabel.Visibility = Visibility.Hidden;
        companyBox.Visibility = Visibility.Hidden;
        companyBox.IsEnabled = false;
        AddCompanyButton.Visibility = Visibility.Hidden;
        AddCompanyButton.IsEnabled = false;
        RemoveCompanyButton.Visibility = Visibility.Hidden;
        RemoveCompanyButton.IsEnabled = false;
        AddCompanyLabel.Visibility = Visibility.Visible;
        AddToCompanyList.Visibility = Visibility.Visible;
        AddToCompanyList.IsEnabled = true;
        CompanyTextBox.Visibility = Visibility.Visible;
        CompanyTextBox.IsEnabled = true;

    }

    private void AddCompanyToList(object Sender, RoutedEventArgs e)
    {
        if (!CompanyTextBox.Text.IsNullOrEmpty())
        {
            var itemToAdd = CompanyTextBox.Text;

            companyItems = companyItems.Concat(new[] { itemToAdd }).ToArray();

            companyBox.ItemsSource = companyItems;

            companyBox.SelectedIndex = 0;

            CompanyLabel.Visibility = Visibility.Visible;
            companyBox.Visibility = Visibility.Visible;
            companyBox.IsEnabled = true;
            AddCompanyButton.Visibility = Visibility.Visible;
            AddCompanyButton.IsEnabled = true;
            RemoveCompanyButton.Visibility = Visibility.Visible;
            RemoveCompanyButton.IsEnabled = true;
            AddCompanyLabel.Visibility = Visibility.Hidden;
            AddToCompanyList.Visibility = Visibility.Hidden;
            AddToCompanyList.IsEnabled = false;
            CompanyTextBox.Visibility = Visibility.Hidden;
            CompanyTextBox.IsEnabled = false;
        }
    }

    private void company_remove(object Sender, RoutedEventArgs e)
    {
        var selectedItemCompany = companyBox.SelectedItem as string;

        List<string> list = companyItems.ToList();
        list.Remove(selectedItemCompany);
        companyItems = list.ToArray();

        companyBox.ItemsSource = companyItems;
    }

    private void Branch_Selection(object sender, SelectionChangedEventArgs e)
    {
        if (Branch_ComboBox.SelectedItem == null)
        {
            Branch_ComboBox.SelectedIndex = 0;
        }
        else
        {
            Branch_Name_ComboBox.Text = CityBranch[Branch_ComboBox.SelectedItem.ToString()];
        }


    }

    private void RemoveBranch(object sender, EventArgs e)
    {
        CityBranch.Remove(Branch_ComboBox.SelectedItem.ToString());
        branches = CityBranch.Keys.ToArray();
        Branch_ComboBox.ItemsSource = ClearItems;
        Branch_ComboBox.ItemsSource = branches;
    }

    private void AddBranchToListClick(object sender, EventArgs e)
    {
        if (!NewBranch_TextBox.Text.IsNullOrEmpty())
        {



            CityBranch.Add(NewBranch_TextBox.Text, null);
            branches = CityBranch.Keys.ToArray();
            Branch_ComboBox.ItemsSource = ClearItems;
            Branch_ComboBox.ItemsSource = branches;



            Branch_ComboBox.Visibility = Visibility.Visible;
            Branch_ComboBox.IsEnabled = true;
            Branch.Visibility = Visibility.Visible;
            Branch.IsEnabled = true;
            AddBranchButton.Visibility = Visibility.Visible;
            RemoveBranchButton.Visibility = Visibility.Visible;
            AddBranchButton.IsEnabled = true;
            RemoveBranchButton.IsEnabled = true;

            // Hide controls that are currently visible
            NewBranch_TextBox.Visibility = Visibility.Collapsed;
            AddBranchToList.Visibility = Visibility.Collapsed;
            NewBranch.Visibility = Visibility.Collapsed;
            NewBranch.IsEnabled = false;
            AddBranchToList.IsEnabled = false;
            NewBranch_TextBox.IsEnabled = false;
        }
    }

    private void AddBranch(object sender, EventArgs e)
    {
        Branch_ComboBox.Visibility = Visibility.Collapsed;
        Branch_ComboBox.IsEnabled = false;
        Branch.Visibility = Visibility.Collapsed;
        Branch.IsEnabled = false;
        AddBranchButton.Visibility = Visibility.Collapsed;
        RemoveBranchButton.Visibility = Visibility.Collapsed;
        AddBranchButton.IsEnabled = false;
        RemoveBranchButton.IsEnabled = false;
        NewBranch_TextBox.Visibility = Visibility.Visible;
        AddBranchToList.Visibility = Visibility.Visible;
        NewBranch.Visibility = Visibility.Visible;
        NewBranch.IsEnabled = true;
        AddBranchToList.IsEnabled = true;
        NewBranch_TextBox.IsEnabled = true;
    }

    private void AddCountrys(object Sender, RoutedEventArgs e)
    {
        CountrysLabel.Visibility = Visibility.Hidden;
        CountrysBox.Visibility = Visibility.Hidden;
        CountrysBox.IsEnabled = false;
        AddCountrysButton.Visibility = Visibility.Hidden;
        AddCountrysButton.IsEnabled = false;
        RemoveCountrysButton.Visibility = Visibility.Hidden;
        RemoveCountrysButton.IsEnabled = false;
        AddCountrysLabel.Visibility = Visibility.Visible;
        AddToCountrysList.Visibility = Visibility.Visible;
        AddToCountrysList.IsEnabled = true;
        CountrysTextBox.Visibility = Visibility.Visible;
        CountrysTextBox.IsEnabled = true;

    }

    private void AddCountrysToList(object Sender, RoutedEventArgs e)
    {
        if (!CountrysTextBox.Text.IsNullOrEmpty())
        {
            string itemToAdd = CountrysTextBox.Text;
            if (!countries.Contains(itemToAdd))
            {
                CountrysItems = CountrysItems.Concat(new[] { itemToAdd }).ToArray();

                CountrysBox.ItemsSource = CountrysItems;

                CountrysBox.SelectedIndex = 0;

                CountrysLabel.Visibility = Visibility.Visible;
                CountrysBox.Visibility = Visibility.Visible;
                CountrysBox.IsEnabled = true;
                AddCountrysButton.Visibility = Visibility.Visible;
                AddCountrysButton.IsEnabled = true;
                RemoveCountrysButton.Visibility = Visibility.Visible;
                RemoveCountrysButton.IsEnabled = true;
                AddCountrysLabel.Visibility = Visibility.Hidden;
                AddToCountrysList.Visibility = Visibility.Hidden;
                AddToCountrysList.IsEnabled = false;
                CountrysTextBox.Visibility = Visibility.Hidden;
                CountrysTextBox.IsEnabled = false;
            }
        }
    }

    private void Countrys_remove(object Sender, RoutedEventArgs e)
    {
        var selectedItemCountrys = CountrysBox.SelectedItem as string;

        List<string> list = countries.ToList();
        list.Remove(selectedItemCountrys);
        CountrysItems = list.ToArray();

        CountrysBox.ItemsSource = CountrysItems;
    }

    private void SaveBranchName(object sender, EventArgs e)
    {
        CityBranch[Branch_ComboBox.SelectedItem.ToString()] = Branch_Name_ComboBox.Text;
    }

    //Domain Functions

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Get the selected item
        object proveSelct = DCSlection.SelectedItem;

        // Perform actions based on the selected item
        if (proveSelct != null)
        {
            domainController = proveSelct.ToString();
            // Do something with the selected option
        }
        else
        {
            domainController = standartDC;
        }
    }

    private string[] EnumerateDomainControllers()
    {
        List<string> domainControllerList = new List<string>();
        System.DirectoryServices.ActiveDirectory.Domain domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain();

        foreach (DomainController dc in domain.DomainControllers)
        {
            domainControllerList.Add(dc.Name);
        }

        return domainControllerList.ToArray();
    }

    private void PopulateComboboxCountrys()
    {
        CountrysBox.ItemsSource = countries;
    }

    private void PopulateComboBoxDomainController()
    {
        string[] options = EnumerateDomainControllers();// Retrieve options from another function
        DCSlection.ItemsSource = options;
        // Set the ArrayList as the ItemsSource of the ComboBox
        if (DCSlection.SelectedItem == null)
        {
            if (domainController == null)
            {
                DCSlection.SelectedItem = standartDC;
            }
            else
            {
                DCSlection.SelectedItem = domainController;
            }
        }
    }

    private void AddTargetOU(object Sender, RoutedEventArgs e)
    {
        targetOUsBox.Visibility = Visibility.Hidden;
        targetOUsBox.IsEnabled = false;
        TargetOULabel.Visibility = Visibility.Hidden;
        AddButtonTargetOU.Visibility = Visibility.Hidden;
        AddButtonTargetOU.IsEnabled = false;
        RemoveButtonTargetOU.Visibility = Visibility.Hidden;
        RemoveButtonTargetOU.IsEnabled = false;
        AddTargetOULabel.Visibility = Visibility.Visible;
        targetOUsTextBox.Visibility = Visibility.Visible;
        targetOUsTextBox.IsEnabled = true;
        AddToTargetOUList.Visibility = Visibility.Visible;
        AddToTargetOUList.IsEnabled = true;
    }

    private void AddTargerOUToList(object Sender, RoutedEventArgs e)
    {
        if (!targetOUsTextBox.Text.IsNullOrEmpty())
        {
            var addTargetOU = targetOUsTextBox.Text;

            targetOUItems = targetOUItems.Concat(new[] { addTargetOU }).ToArray();

            targetOUsBox.ItemsSource = targetOUItems;

            targetOUsBox.SelectedIndex = 0;

            targetOUsBox.Visibility = Visibility.Visible;
            targetOUsBox.IsEnabled = true;
            TargetOULabel.Visibility = Visibility.Visible;
            AddButtonTargetOU.Visibility = Visibility.Visible;
            AddButtonTargetOU.IsEnabled = true;
            RemoveButtonTargetOU.Visibility = Visibility.Visible;
            RemoveButtonTargetOU.IsEnabled = true;
            targetOUsTextBox.Visibility = Visibility.Hidden;
            AddTargetOULabel.Visibility = Visibility.Hidden;
            targetOUsTextBox.IsEnabled = false;
            AddToTargetOUList.Visibility = Visibility.Hidden;
            AddToTargetOUList.IsEnabled = false;
        }
    }

    private void tragetOU_remove(object Sender, RoutedEventArgs e)
    {
        var selectedItemtarget = targetOUsBox.SelectedItem as string;

        List<string> list = targetOUItems.ToList();
        list.Remove(selectedItemtarget);
        targetOUItems = list.ToArray();

        targetOUsBox.ItemsSource = targetOUItems;
    }

    private void SetSlidervalue()
    {
        PasswordLength.Minimum = 0;
        PasswordLength.Maximum = 100;
        if (lenght == 0)
        {
            PasswordLength.Value = _passwordLength;
        }
        else
        {
            PasswordLength.Value = lenght;
        }
    }

    public bool _SavePointLogs;


    //Dashboard Functions

    public async Task<Dictionary<int, string>> ComboValues()
    {
        Dictionary<int, string> Values = new Dictionary<int, string>();

        var data = await _sampleDataService.GetContentGridDataAsync();
        foreach (var item in data)
        {
            Values.Add(item.SiteID, item.SiteName);
        }
        return Values;
    }

    private async void PopulateDashboardItems()
    {
        Dictionary<int, string> comboValues = await ComboValues();

        Select_Item_1.ItemsSource = comboValues.Values;
        Select_Item_2.ItemsSource = comboValues.Values;
        Select_Item_3.ItemsSource = comboValues.Values;

        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashboardData.json");
        if (File.Exists(filePath))
        {
            int SelcedPage1;
            int SelcedPage2;
            int SelcedPage3;

            DashboardDataManager.TryLoadDashboardDataSelectedPage(out SelcedPage1, out SelcedPage2, out SelcedPage3);
            Select_Item_1.SelectedIndex = SelcedPage1 -1;
            Select_Item_2.SelectedIndex = SelcedPage2 -1;
            Select_Item_3.SelectedIndex = SelcedPage3 -1;

        }
    }

    private void populateFButtons()
    {
        Select_Item_KeylocalAdmin.ItemsSource = keyDictionary.Keys;
        Select_Item_Key.ItemsSource = keyDictionary.Keys;

        int _buttonPass;
        int _buttonlocal;
        AutoCopyConfManager.TryLoadButtonPassword(out _buttonPass);
        AutoCopyConfManager.TryLoadButtonLocalAdmin(out _buttonlocal);

        // Find the corresponding key in the dictionary based on the loaded value
        string selectedKeyPass = keyDictionary.FirstOrDefault(x => x.Value == _buttonPass).Key;
        string selectedKeyLocal = keyDictionary.FirstOrDefault(x => x.Value == _buttonlocal).Key;

        Select_Item_Key.SelectedItem = selectedKeyPass;
        Select_Item_KeylocalAdmin.SelectedItem = selectedKeyLocal;
    }

    Dictionary<string, int> keyDictionary = new Dictionary<string, int>
    {
    {"Disabled", 0},
    { "VK_BACK", 0x8 },
    { "VK_TAB", 0x9 },
    { "VK_CLEAR", 0xC },
    { "VK_RETURN", 0xD },
    { "VK_SHIFT", 0x10 },
    { "VK_CONTROL", 0x11 },
    { "VK_MENU", 0x12 },
    { "VK_PAUSE", 0x13 },
    { "VK_CAPITAL", 0x14 },
    { "VK_F1", 0x70 },
    { "VK_F2", 0x71 },
    { "VK_F3", 0x72 },
    { "VK_F4", 0x73 },
    { "VK_F5", 0x74 },
    { "VK_F6", 0x75 },
    { "VK_F7", 0x76 },
    { "VK_F8", 0x77 },
    { "VK_F9", 0x78 },
    { "VK_F10", 0x79 },
    { "VK_F11", 0x7A },
    { "VK_F12", 0x7B },
    };

    //Sitesettings Functions

        //Populate Functions

    private void populateDisabledSitesComboBox()
    {

        if (DisabledSitesItem.IsNullOrEmpty())
        {
            DisabledSitesBox.Items.Add("None");
        }
        else
        {
            DisabledSitesBox.ItemsSource = DisabledSitesItem;
        }
        
        string[] DisabledSitesItemToAdd2 = {
            "None",
            "LAPSViewModel",
            "Delete_DeviceViewModel",
            "Check_User_AccountViewModel",
            "Disable_Enable_AccountViewModel",
            "Enable_AccountViewModel",
            "Add_Remove_User_GroupViewModel",
            "Get_BitlockerViewModel",
            "Enable_DeviceViewModel",
            "NewUserViewModel",
            "Add_SharedMailboxUserViewModel",
            "Create_Shared_MailboxViewModel"

        };
        DisabledSitesItemToAdd = DisabledSitesItemToAdd2;
        DisabledSitesTextBox.ItemsSource = DisabledSitesItemToAdd;

    }

    //Disabled Buttons

    private void AddDisabledSites(object Sender, RoutedEventArgs e)
    {
        DisabledSitesLabel.Visibility = Visibility.Hidden;
        DisabledSitesBox.Visibility = Visibility.Hidden;
        DisabledSitesBox.IsEnabled = false;
        AddDisabledSitesButton.Visibility = Visibility.Hidden;
        AddDisabledSitesButton.IsEnabled = false;
        RemoveDisabledSitesButton.Visibility = Visibility.Hidden;
        RemoveDisabledSitesButton.IsEnabled = false;
        AddDisabledSitesLabel.Visibility = Visibility.Visible;
        AddToDisabledSitesList.Visibility = Visibility.Visible;
        AddToDisabledSitesList.IsEnabled = true;
        DisabledSitesTextBox.Visibility = Visibility.Visible;
        DisabledSitesTextBox.IsEnabled = true;
    }

    private void DisabledSites_remove(object Sender, RoutedEventArgs e)
    {
        var selectedItemtarget = DisabledSitesBox.SelectedItem as string;

        List<string> list = DisabledSitesItem.ToList();
        list.Remove(selectedItemtarget);
        DisabledSitesItem = list.ToArray();

        DisabledSitesBox.ItemsSource = DisabledSitesItem;
    }

    private void AddDisabledSitesToList(object Sender, RoutedEventArgs e)
    {
        if (!DisabledSitesTextBox.SelectedItem.ToString().IsNullOrEmpty())
        {
            string addDisabledSite = DisabledSitesTextBox.SelectedItem.ToString();
            DisabledSitesItem = DisabledSitesBox.Items.OfType<string>().ToArray();
            DisabledSitesItem = DisabledSitesItem.Concat(new[] { addDisabledSite }).ToArray();

            DisabledSitesBox.ItemsSource = DisabledSitesItem;

            DisabledSitesBox.SelectedIndex = 0;

            DisabledSitesBox.Visibility = Visibility.Visible;
            DisabledSitesBox.IsEnabled = true;
            DisabledSitesLabel.Visibility = Visibility.Visible;
            AddDisabledSitesButton.Visibility = Visibility.Visible;
            AddDisabledSitesButton.IsEnabled = true;
            RemoveDisabledSitesButton.Visibility = Visibility.Visible;
            RemoveDisabledSitesButton.IsEnabled = true;
            DisabledSitesTextBox.Visibility = Visibility.Hidden;
            AddDisabledSitesLabel.Visibility = Visibility.Hidden;
            DisabledSitesTextBox.IsEnabled = false;
            AddToDisabledSitesList.Visibility = Visibility.Hidden;
            AddToDisabledSitesList.IsEnabled = false;
        }
    }
}
