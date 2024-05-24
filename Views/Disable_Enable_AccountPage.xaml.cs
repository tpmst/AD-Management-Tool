using System.ComponentModel.Design;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using ADTool.Contracts.Services;
using ADTool.Models;
using ADTool.Services;
using ADTool.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;

namespace ADTool.Views;

public class SubOUsByRegion
{
    public Dictionary<string, List<string>> OUsByRegion { get; set; }

    public SubOUsByRegion()
    {
        OUsByRegion = new Dictionary<string, List<string>>();
    }
}

public partial class Disable_Enable_AccountPage : Page
{
    private readonly IADServices _ADService;
    private readonly IPassword_Generator _PasswordGenerator;
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

    public Disable_Enable_AccountPage(Disable_Enable_AccountViewModel viewModel, IADServices aDServices, IPassword_Generator password_Generator, ILogService logService, ISharePointFileService sharePointFileService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ADService = aDServices;
        _PasswordGenerator = password_Generator;
        _LogService = logService;
        UserCredentialManager.TryLoadUserCredentials(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.json"), out _username, out _password);
        populate_branch_input();
        populate_company_input();
        getDate();
        _SharePointFileService = sharePointFileService;
        
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

        branch_input.SelectionChanged += (sender, e) =>
        {
            if (branch_input.SelectedItem != null)
            {
                branch = branch_input.SelectedItem.ToString();
            }
        };
        region_Input.SelectedIndex = 0;
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
            if (Firstname_input.Text.IsNullOrEmpty() || Lastname_input.Text.IsNullOrEmpty() || region_Input.SelectedItem == null || branch_input.SelectedItem == null || Manager_input.Text.IsNullOrEmpty() || Ticketnumber_input.Text.IsNullOrEmpty() || Submitter_input.Text.IsNullOrEmpty() || DueDate_input.SelectedDate == null)
            {
                MessageBox.Show("All fields must be filled", "Attenion", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (IsNumeric(Ticketnumber_input.Text))
                {
                    string deviceBrand = "";
                    string OoO = "";
                    string Groupmembership = string.Empty;

                    if (DeivceDell.IsChecked == true)
                    {
                        deviceBrand = "Dell";
                    }
                    if (DeivceLeonvo.IsChecked == true)
                    {
                        deviceBrand = "Lenovo";
                    }
                    if (OoO_input.IsChecked == true)
                    {
                        OoO = "y";
                    }
                    else
                    {
                        OoO = "n";
                    }
                    if (Groupmebership_input.IsChecked == true)
                    {
                        Groupmembership = "y";
                    }
                    else
                    {
                        Groupmembership = "n";
                    }



                    string SubmitterEmail = await _ADService.GetUserEmailFromLocalAD(Submitter_input.Text);

                    string content = $"{_username},{Firstname_input.Text},{Lastname_input.Text},{company_input.SelectedItem},{region_Input.SelectedItem},{branch_input.SelectedItem},{Manager_input.Text},{Ticketnumber_input.Text},{Submitter_input.Text},{SubmitterEmail},{DueDate_input.Text},{deviceBrand},{Devicemodel_input.Text},{DeviceTag_input.Text},{OoO},{OoO_username_input.Text},{Groupmembership}";
                    try
                    {
                        string userName = $"{Firstname_input.Text}.{Lastname_input.Text}";
                        string userlower = userName.ToLower();
                        string newpass = _PasswordGenerator.GenerateSecurePass(200);

                        //TODO: Activeate for usage
                        MessageBoxResult result = MessageBox.Show("Do you want to crucally disable the account. This means, that the user cant be enabled anymore!", "Attention", MessageBoxButton.YesNo,MessageBoxImage.Stop);
                        if (result == MessageBoxResult.Yes)
                        {
                            MessageBoxResult result2 = MessageBox.Show("Are you sure to take this action?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                            if (result2 == MessageBoxResult.Yes)
                            {
                                _ADService.SetExtensionAttribute(userlower);
                            }
                        }
                        _ADService.DisableAccount(userlower, newpass);
                        bool check = await _SharePointFileService.UpdateDeleteUserDetails(content);
                        if (check)
                        {
                            MessageBox.Show($"The user {Firstname_input.Text}.{Lastname_input.Text} will be deleted today", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            string logData = $"fristName:{Firstname_input.Text},lastName:{Lastname_input.Text},company:{company_input.SelectedItem},region:{region_Input.SelectedItem},branch:{branch_input.SelectedItem},manager:{Manager_input.Text},ticketnumber:{Ticketnumber_input.Text},submitter:{Submitter_input.Text},submitterEmail:{SubmitterEmail},dueDate:{DueDate_input.Text},deviceBrand:{deviceBrand},deviceModel:{Devicemodel_input.Text},deviceTag:{DeviceTag_input.Text},OoOConfirmation:{OoO},OoOUsername:{OoO_username_input.Text},gruopsConfirmation:{Groupmembership}";
                            await _LogService.createLog("Delete_User", logData);
                        }
                        else
                        {
                            MessageBox.Show($"Something went Wrong:" +
                                $"The user {Firstname_input.Text}.{Lastname_input.Text} won´t deleted today", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("There are Letters in your Ticketnumberfield", "Attenion", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


}
