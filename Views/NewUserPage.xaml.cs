using System.Windows.Controls;
using ADTool.Services;
using ADTool.ViewModels;
using System.IO;
using System.Windows;
using ADTool.Contracts.Services;
using Microsoft.IdentityModel.Tokens;
using ADTool.Helpers;
using System.Text.RegularExpressions;

namespace ADTool.Views;



public partial class NewUserPage : Page
{

    private readonly IADServices _ADService;
    private readonly ILogService _LogService;
    private readonly ISharePointFileService _SharePointFileService;

    private static string branch;
    private static string company;
    private static string DateString;
    private static string userName;

    private static string _username;
    private static string _password;

    private const Dictionary<string, List<string>> ouByRegion = null;
    private Dictionary<string, string> UserNames;
    private string[] ClearItems = { "" };

    public NewUserPage(NewUserViewModel viewModel, IADServices aDServices, ILogService logService, ISharePointFileService sharePointFileService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ADService = aDServices;
        _LogService = logService;
        _SharePointFileService = sharePointFileService;
        UserCredentialManager.TryLoadUserCredentials(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.json"), out _username, out _password);
        populate_branch_input();
        populate_company_input();
        populate_cityinput();
        getDate();
    }

    private void populate_branch_input()
    {
        Dictionary<string, List<string>> ouByRegion;

        string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");

        AppConfigManager.TryLoadOUByRegion(AppConfPath, out ouByRegion);

        branch_input.Items.Clear();
        region_Input.Items.Clear();

        foreach (string region in ouByRegion.Keys)
        {
            region_Input.Items.Add(region);
        }

        region_Input.SelectionChanged += (sender, e) =>
        {
            branch_input.Items.Clear();

            string selectedRegion = region_Input.SelectedItem.ToString();

            if (ouByRegion.ContainsKey(selectedRegion))
            {
                foreach (string allbranch in ouByRegion[selectedRegion])
                {
                    branch_input.Items.Add(allbranch);
                }

                branch_input.Items.Remove(branch_input.Items[0].ToString());


            }
        };
    }

    private void populate_cityinput()
    {
        Dictionary<string, string> CityBranch;
        string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
        AppConfigManager.TryLoadCityBranch(AppConfPath, out CityBranch);
        
        city_input.Items.Clear();
        city_input.ItemsSource = CityBranch.Values;

    }


    private void populate_company_input()
    {
        string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
        string[] companyOptions;
        AppConfigManager.TryLoadCompany(AppConfPath, out companyOptions);
        company_input.ItemsSource = companyOptions;
        company_input.SelectionChanged += (sender, e) =>
        {
            company = company_input.SelectedItem.ToString();
        };

    }

    private void getDate()
    {
        DueDate_input.SelectedDateChanged += (sender, e) =>
        {
            DateTime dueDate = DueDate_input.SelectedDate.GetValueOrDefault();
            DateString = dueDate.ToShortDateString();
        };
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (Submitter_ComboBox.SelectedItem != null)
            {
                string selectedKey = Submitter_ComboBox.SelectedItem.ToString();
                string selectedValue = UserNames[selectedKey]; // Assuming GroupNames is your dictionary
                Submitter_input.Visibility = Visibility.Visible;
                Submitter_input.IsEnabled = true;
                Submitter_ComboBox.Visibility = Visibility.Hidden;
                Submitter_ComboBox.IsEnabled = false;


                Submitter_input.Text = selectedKey;
                Submitter_ComboBox.ItemsSource = ClearItems;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Check_Names(object sender, EventArgs e)
    {
        try
        {
            if (!Submitter_input.Text.IsNullOrEmpty())
            {
                string searchfilter_groupName = Submitter_input.Text;
                UserNames = _ADService.FindUsers(searchfilter_groupName);
                if (UserNames.Keys.Count > 1)
                {
                    Submitter_input.Visibility = Visibility.Hidden;
                    Submitter_input.IsEnabled = false;
                    Submitter_ComboBox.Visibility = Visibility.Visible;
                    Submitter_ComboBox.IsEnabled = true;
                    Submitter_ComboBox.ItemsSource = UserNames.Keys;
                }
                else if (UserNames.Keys.Count == 1)
                {
                    Submitter_input.Text = UserNames.Keys.FirstOrDefault();
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Check_Selection_Changed(object sender, RoutedEventArgs e)
    {
        if(Device_input.IsChecked == true)
        {
            DeviceGrid.Visibility = Visibility.Visible;
            DeviceGrid.IsEnabled = true;
            TotalBorder.Width = 770;
            shadowBorder.Width = 770;
            shadowBorder.Margin = new Thickness(0, 10, 10, 0);
            Total.Margin = new Thickness(20,20,0,0);
        }
        if(Device_input.IsChecked == false)
        {
            DeviceGrid.Visibility = Visibility.Collapsed;
            DeviceGrid.IsEnabled = false;
            TotalBorder.Width = 520;
            shadowBorder.Width = 520;
            shadowBorder.Margin = new Thickness(0, 10, 260, 0);
            Total.Margin = new Thickness(170, 20, 0, 0);
        }
    }

    private bool IsNumeric(string text)
    {
        // Regular expression pattern to match numbers
        string pattern = @"^[0-9]+$";

        // Create a Regex object
        Regex regex = new Regex(pattern);

        // Use the IsMatch method to check if the text matches the pattern
        return regex.IsMatch(text);
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Firstname_input.Text.IsNullOrEmpty() || Lastname_input.Text.IsNullOrEmpty() || company_input.SelectedItem == null || region_Input.SelectedItem == null || branch_input.SelectedItem == null || city_input.SelectedItem == null || EmployeeID_input.Text.IsNullOrEmpty() || Ticketnumber_input.Text.IsNullOrEmpty() || Submitter_input.Text.IsNullOrEmpty() || DueDate_input.SelectedDate == null)
            {
                MessageBox.Show("All fields must be filled", "Attenion", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string Tickemp = $"{Ticketnumber_input.Text}{EmployeeID_input.Text}";
                if (IsNumeric(Tickemp))
                {
                    string device;
                    string deviceBrand = "";
                    string deviceCondition = "";
                    if (Device_input.IsChecked == true)
                    {
                        device = "y";
                        if (DeivceNew.IsChecked == true)
                        {
                            deviceCondition = "y";
                        }
                        if (DeivceOld.IsChecked == true)
                        {
                            deviceCondition = "n";
                        }
                        if (DeivceDell.IsChecked == true)
                        {
                            deviceBrand = "Dell";
                        }
                        if (DeivceLeonvo.IsChecked == true)
                        {
                            deviceBrand = "Lenovo";
                        }
                    }
                    else
                    {
                        device = "n";
                    }

                    string SubmitterEmail = await _ADService.GetUserEmailFromLocalAD(Submitter_input.Text);

                    string content = $"{_username},{Firstname_input.Text},{Lastname_input.Text},{company_input.SelectedItem},{region_Input.SelectedItem},{branch_input.SelectedItem},{city_input.SelectedItem},{EmployeeID_input.Text},{Ticketnumber_input.Text},{Submitter_input.Text},{SubmitterEmail},{DueDate_input.Text},{device},{deviceBrand},{deviceCondition},{Devicemodel_input.Text},{DeviceTag_input.Text}";
                    try
                    {
                        bool check = await _SharePointFileService.UpdateNewUserDetails(content);
                        if (check)
                        {
                            MessageBox.Show($"The user {Firstname_input.Text}.{Lastname_input.Text} will be created today", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            string logData = $"fristName:{Firstname_input.Text},lastName:{Lastname_input.Text},company:{company_input.SelectedItem},region:{region_Input.SelectedItem},branch:{branch_input.SelectedItem},city:{city_input.SelectedItem},employeeID:{EmployeeID_input.Text},ticketnumber:{Ticketnumber_input.Text},submitter:{Submitter_input.Text},submitterEmail:{SubmitterEmail},dueDate:{DueDate_input.Text},getsDevice:{device},deviceCondition:{deviceCondition}deviceBrand:{deviceBrand},deviceModel:{Devicemodel_input.Text},deviceTag:{DeviceTag_input.Text}";
                            await _LogService.createLog("Delete_User", logData);
                        }
                        else
                        {
                            MessageBox.Show($"Something went Wrong:" +
                                $"The user {Firstname_input.Text}.{Lastname_input.Text} won´t created today", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00072", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("There are Letters in your Ticketnumberfiled or in your EmployeeID field", "Attenion", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }catch(Exception ex)
        {
            System.Windows.MessageBox.Show($"ErrorMassage: {ex.Message}", "Error: 0x00071", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

