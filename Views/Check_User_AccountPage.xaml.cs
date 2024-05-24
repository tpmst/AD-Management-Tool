using System.Collections.Immutable;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ADTool.Contracts.Services;
using ADTool.Services;
using ADTool.ViewModels;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;

namespace ADTool.Views;

public partial class Check_User_AccountPage : Page
{
    private readonly IADServices _ADservices;
    private readonly ILogService _logService;
    private readonly IPassword_Generator _passwordGenerator;

    private string[] ClearItems = { "" };
    private Dictionary<string, string> UserNames;
    private Dictionary<string, string> CityBranch;
    string[] countries;

    public Check_User_AccountPage(Check_User_AccountViewModel viewModel, IADServices aDservices, ILogService logService, IPassword_Generator password_Generator)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ADservices = aDservices;
        _logService = logService;
        _passwordGenerator = password_Generator;
        loadCityBrach();
    }

    private void loadCityBrach()
    {
        string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
        AppConfigManager.TryLoadCityBranch(AppConfPath, out CityBranch);
        AppConfigManager.TryLoadCountry(AppConfPath, out countries);
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (User_Name_ComboBox.SelectedItem != null)
            {
                string selectedKey = User_Name_ComboBox.SelectedItem.ToString();
                string selectedValue = UserNames[selectedKey]; // Assuming GroupNames is your dictionary
                Check_UserAccount_Textbox.Visibility = Visibility.Visible;
                Check_UserAccount_Textbox.IsEnabled = true;
                User_Name_ComboBox.Visibility = Visibility.Hidden;
                User_Name_ComboBox.IsEnabled = false;


                Check_UserAccount_Textbox.Text = selectedKey;
                User_Name_ComboBox.ItemsSource = ClearItems;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Check_Names(object sender, EventArgs e)
    {
        if (!Check_UserAccount_Textbox.Text.IsNullOrEmpty())
        {
            string searchfilter_groupName = Check_UserAccount_Textbox.Text;
            UserNames = _ADservices.FindUsers(searchfilter_groupName);
            if (UserNames.Keys.Count > 1)
            {
                Check_UserAccount_Textbox.Visibility = Visibility.Hidden;
                Check_UserAccount_Textbox.IsEnabled = false;
                User_Name_ComboBox.Visibility = Visibility.Visible;
                User_Name_ComboBox.IsEnabled = true;
                User_Name_ComboBox.ItemsSource = UserNames.Keys;
            }
            else if (UserNames.Keys.Count == 1)
            {
                Check_UserAccount_Textbox.Text = UserNames.Keys.FirstOrDefault();
            }
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.MessageBox.Show("Password copied to clipboard"); 
            System.Windows.Clipboard.SetText(newpassLabel.Content.ToString());

        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Error copying text to clipboard: " + ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

   


    private void Button_unlock(object sender, RoutedEventArgs e)
    {
        
        string username = Check_UserAccount_Textbox.Text;
        if (username.IsNullOrEmpty())
        {
            System.Windows.MessageBox.Show($"You cant do that operation", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            if (Enable_Disable_Label.Content == "locked")
            {
                MessageBoxResult resutl = System.Windows.MessageBox.Show($"Do you want to unlock {username}", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (resutl == MessageBoxResult.Yes)
                {
                    _ADservices.UnlockUserAccount(username);
                    _logService.createLog("unlockUser", $"inputData(username:{username})");
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"You cant do that operation", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void Button_restetPassword(object sender, RoutedEventArgs e)
    {

        string username = Check_UserAccount_Textbox.Text;
        if (username.IsNullOrEmpty())
        {
            System.Windows.MessageBox.Show($"You cant do that operation", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            if (Enable_Disable_Label.Content == "disabled")
            {
                System.Windows.MessageBox.Show($"You cant do that operation if the account is disabled", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBoxResult resutl = System.Windows.MessageBox.Show($"Do you want to restet the password of {username}", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (resutl == MessageBoxResult.Yes)
                {
                    string newpass = _passwordGenerator.GeneratePassword();
                    _ADservices.ResetPassword(username, newpass);
                    _logService.createLog("resetPassword", $"username:{username}");
                    newpassLabel.Content = newpass;
                    passwordGrid.Visibility = Visibility.Visible;
                    passwordGrid.IsEnabled = true;
                }
            }
        }
    }
    private void Button_ChangeCity(object sender, RoutedEventArgs e)
    {
        if (!CityLabel.Content.ToString().IsNullOrEmpty())
        {
            CityBranch = CityBranch
                .Where(kv => kv.Value != "Unknown")
                .OrderBy(kv => kv.Value) // Sort the values alphabetically
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            cityTextBox.ItemsSource = CityBranch.Values;
            changeButtonSubmit();
            cityTextBox.Visibility = Visibility.Visible;
            CityLabel.Visibility = Visibility.Collapsed;
            cityButton.Visibility = Visibility.Collapsed;
        }
    }

    private void Button_ChangeCountry(object sender, RoutedEventArgs e)
    {
        if (!CountryLabel.Content.ToString().IsNullOrEmpty())
        {
            var sortedCountries = countries.OrderBy(country => country).ToList(); // Sort the countries alphabetically
            CountryTextBox.ItemsSource = sortedCountries;
            changeButtonSubmit();
            CountryTextBox.Visibility = Visibility.Visible;
            CountryLabel.Visibility = Visibility.Collapsed;
            CountryButton.Visibility = Visibility.Collapsed;
        }
    }

    private void Button_Smartphone(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Hello");
        if (!smartphonenumberLabel.Content.ToString().IsNullOrEmpty())
        {
            changeButtonSubmit();
            SmartphoneTextbox.Visibility = Visibility.Visible;
            smartphonenumberLabel.Visibility = Visibility.Collapsed;
            Smartphonebutton.Visibility = Visibility.Collapsed;
        }
    }

    private void Button_changePath(object sender, RoutedEventArgs e)
    {
        if (!ScriptPath.Content.ToString().IsNullOrEmpty())
        {
            changeButtonSubmit();
            LogonTextbox.Visibility = Visibility.Visible;
            ScriptPath.Visibility = Visibility.Collapsed;
            LogonButton.Visibility = Visibility.Collapsed;
        }
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        string country;
        string city;
        string LogonPath;
        string MobileNumber;

        if(!CountryTextBox.Text.IsNullOrEmpty())
        {
            country = CountryTextBox.Text;
        }
        else
        {
            country = CountryLabel.Content.ToString();
        }
        if (!cityTextBox.Text.IsNullOrEmpty())
        {
            city = cityTextBox.Text;
        }
        else
        {
            city = CityLabel.Content.ToString();
        }
        if (!LogonTextbox.Text.IsNullOrEmpty())
        {
            LogonPath = LogonTextbox.Text;
        }
        else
        {
            LogonPath = ScriptPath.Content.ToString();
        }
        if (!SmartphoneTextbox.Text.IsNullOrEmpty())
        {
            MobileNumber = SmartphoneTextbox.Text;
        }
        else
        {
            MobileNumber = smartphonenumberLabel.Content.ToString();
        }
        string username = Check_UserAccount_Textbox.Text;
        _ADservices.UpdateUserAttributes(username, city, country, LogonPath, MobileNumber);

        _logService.createLog("updateCityCountry", $"username:{username}/city:{city}/country:{country}/logonPath:{LogonPath}/mobileNumber:{MobileNumber}");

        cityTextBox.Visibility = Visibility.Collapsed;
        CityLabel.Visibility = Visibility.Visible;
        cityButton.Visibility = Visibility.Visible;

        CountryTextBox.Visibility = Visibility.Collapsed;
        CountryLabel.Visibility = Visibility.Visible;
        CountryButton.Visibility = Visibility.Visible;

        SmartphoneTextbox.Visibility = Visibility.Collapsed;
        smartphonenumberLabel.Visibility = Visibility.Visible;
        Smartphonebutton.Visibility = Visibility.Visible;

        LogonTextbox.Visibility = Visibility.Collapsed;
        ScriptPath.Visibility = Visibility.Visible;
        LogonButton.Visibility = Visibility.Visible;

        Account_Submit.Visibility = Visibility.Visible;
        save_changes.Visibility = Visibility.Collapsed;
        refresh();
    }

    private void changeButtonSubmit()
    {
        if(Account_Submit.Visibility == Visibility.Visible)
        {
            Account_Submit.Visibility = Visibility.Collapsed;
            save_changes.Visibility = Visibility.Visible;
        }
    }

    private void refresh()
    {
        StatusIcon2.Visibility = Visibility.Hidden;
        CityLabel.Content = "";
        CountryLabel.Content = "";
        CompanyLabel.Content = "";
        ManagerLabel.Content = "";
        smartphonenumberLabel.Content = "";
        LastLogin.Content = "";
        ScriptPath.Content = "";
        Createdon.Content = ""; 

        string username = Check_UserAccount_Textbox.Text;
        if (!username.IsNullOrEmpty())
        {
            try
            {
                DateTime? passwordExperation = _ADservices.GetPasswordExpirationDate(username);
                DateTime? lastLoginDate = _ADservices.GetLastLogin(username);
                string CreationDate = _ADservices.GetCreationDate(username);
                (string city, string company, string country, string manager, string phonenumber, string logonpath) = _ADservices.GetUserCityAndCompany(username);
                bool isDisabled = _ADservices.IsAccountDisabled(username);
                if (isDisabled)
                {

                    StatusIcon.Kind = MahApps.Metro.IconPacks.PackIconRemixIconKind.CheckboxBlankCircleLine;
                    StatusIcon2.Visibility = Visibility.Visible;
                    Enable_Disable_Label.Content = "disabled";
                }
                else
                {
                    bool isLocked = _ADservices.IsAccountLocked(username);
                    if (isLocked)
                    {
                        Enable_Disable_Label.Content = "locked";
                        StatusIcon.Kind = MahApps.Metro.IconPacks.PackIconRemixIconKind.Lock2Line;
                    }
                    else
                    {
                        Enable_Disable_Label.Content = "unlocked";
                        StatusIcon.Kind = MahApps.Metro.IconPacks.PackIconRemixIconKind.LockUnlockLine;
                    }
                }
                DateTime currentDateTime = DateTime.Now;
                if (passwordExperation < currentDateTime)
                {
                    PasswordLabel.Content = "expiered";
                }
                else
                {
                    try
                    {
                        string originalString = passwordExperation.ToString();
                        int spaceIndex = originalString.IndexOf(' ');
                        string shortenedString = originalString.Substring(0, spaceIndex);
                        PasswordLabel.Content = shortenedString;
                    }
                    catch { }
                }

                CityLabel.Content = city;
                if (!company.IsNullOrEmpty())
                {
                    try
                    {
                        string originalString2 = company;
                        int spaceIndex2 = originalString2.IndexOf(' ');
                        string shortenedString2 = originalString2.Substring(0, spaceIndex2);
                        CompanyLabel.Content = shortenedString2;
                    }
                    catch
                    {
                        CityLabel.Content = "";

                    }
                }
                if (!manager.IsNullOrEmpty())
                {
                    try
                    {
                        int commaIndex = manager.IndexOf(',');
                        string shotrtenedmanager = manager.Substring(0, commaIndex);
                        string stringWithoutFirstThreeLetters = shotrtenedmanager.Substring(3);
                        int spaceIndex3 = stringWithoutFirstThreeLetters.IndexOf(" ");
                        ManagerLabel.Content = stringWithoutFirstThreeLetters.Substring(spaceIndex3);
                    }
                    catch
                    {

                        ManagerLabel.Content = "";
                    }
                }
                if (!country.IsNullOrEmpty()) { CountryLabel.Content = country; }
                else
                {
                    CountryLabel.Content = "Not available";
                }
                if (!phonenumber.IsNullOrEmpty()) { smartphonenumberLabel.Content = phonenumber; }
                else
                { smartphonenumberLabel.Content = "Not available"; }
                if (!logonpath.IsNullOrEmpty()) { ScriptPath.Content = logonpath; }
                else 
                { ScriptPath.Content = "Not available"; }
                if (!lastLoginDate.ToString().IsNullOrEmpty()) {
                    string originalString = lastLoginDate.ToString();
                    int spaceIndex = originalString.IndexOf(' ');
                    string shortenedString = originalString.Substring(0, spaceIndex);
                    LastLogin.Content = shortenedString;
                }
                else
                { LastLogin.Content = "Not available"; }

                if (!CreationDate.IsNullOrEmpty()) {
                    string originalString = CreationDate;
                    int spaceIndex = originalString.IndexOf(' ');
                    string shortenedString = originalString.Substring(0, spaceIndex);
                    Createdon.Content = shortenedString;
                } 
                else 
                { Createdon.Content = "Not available"; }

                //TODO: confige for all fields in tool
                _logService.createLog("checkUser", $"user:{username}");


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occured: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            System.Windows.MessageBox.Show($"Enter an valid username", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

        refresh();
    }

    
}
