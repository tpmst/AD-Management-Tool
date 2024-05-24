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

public partial class Create_Shared_MailboxPage : Page
{

    private readonly IADServices _ADService;
    private readonly ILogService _LogService;
    private readonly ISharePointFileService _SharePointFileService;

    private static string branch;
    private static string company;
    private static string DateString;
    private static string userName;

    private string[] DisabledSitesItem;
    private string[] DisabledSitesItemTeams;

    private string[] ClearItems = { "" };

    public Create_Shared_MailboxPage(Create_Shared_MailboxViewModel viewModel, ISharePointFileService sharePointFileService, ILogService logService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _SharePointFileService = sharePointFileService;
        _LogService = logService;
        populate_Visiblity_input();
        populate_company_input();
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

    private void populate_Visiblity_input()
    {
        string[] companyOptions = { "Private", "Public", "Organization-wide" };
        company_inputTeams.ItemsSource = companyOptions;
        company_inputTeams.SelectionChanged += (sender, e) =>
        {
            company = company_inputTeams.SelectedItem.ToString();
        };

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
        if (!DisabledSitesTextBox.Text.IsNullOrEmpty())
        {
            string addDisabledSite = DisabledSitesTextBox.Text;
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

            DisabledSitesTextBox.Text = "";
        }
    }

    private void AddDisabledSitesTeams(object Sender, RoutedEventArgs e)
    {
        DisabledSitesLabelTeams.Visibility = Visibility.Hidden;
        DisabledSitesBoxTeams.Visibility = Visibility.Hidden;
        DisabledSitesBoxTeams.IsEnabled = false;
        AddDisabledSitesButtonTeams.Visibility = Visibility.Hidden;
        AddDisabledSitesButtonTeams.IsEnabled = false;
        RemoveDisabledSitesButtonTeams.Visibility = Visibility.Hidden;
        RemoveDisabledSitesButtonTeams.IsEnabled = false;
        AddDisabledSitesLabelTeams.Visibility = Visibility.Visible;
        AddToDisabledSitesListTeams.Visibility = Visibility.Visible;
        AddToDisabledSitesListTeams.IsEnabled = true;
        DisabledSitesTextBoxTeams.Visibility = Visibility.Visible;
        DisabledSitesTextBoxTeams.IsEnabled = true;
    }

    private void DisabledSites_removeTeams(object Sender, RoutedEventArgs e)
    {
        var selectedItemtarget = DisabledSitesBoxTeams.SelectedItem as string;

        List<string> list = DisabledSitesItemTeams.ToList();
        list.Remove(selectedItemtarget);
        DisabledSitesItemTeams = list.ToArray();

        DisabledSitesBoxTeams.ItemsSource = DisabledSitesItemTeams;
    }

    private void AddDisabledSitesToListTeams(object Sender, RoutedEventArgs e)
    {
        if (!DisabledSitesTextBoxTeams.Text.IsNullOrEmpty())
        {
            string addDisabledSite = DisabledSitesTextBoxTeams.Text;
            DisabledSitesItemTeams = DisabledSitesBoxTeams.Items.OfType<string>().ToArray();
            DisabledSitesItemTeams = DisabledSitesItemTeams.Concat(new[] { addDisabledSite }).ToArray();

            DisabledSitesBoxTeams.ItemsSource = DisabledSitesItemTeams;

            DisabledSitesBoxTeams.SelectedIndex = 0;

            DisabledSitesBoxTeams.Visibility = Visibility.Visible;
            DisabledSitesBoxTeams.IsEnabled = true;
            DisabledSitesLabelTeams.Visibility = Visibility.Visible;
            AddDisabledSitesButtonTeams.Visibility = Visibility.Visible;
            AddDisabledSitesButtonTeams.IsEnabled = true;
            RemoveDisabledSitesButtonTeams.Visibility = Visibility.Visible;
            RemoveDisabledSitesButtonTeams.IsEnabled = true;
            DisabledSitesTextBoxTeams.Visibility = Visibility.Hidden;
            AddDisabledSitesLabelTeams.Visibility = Visibility.Hidden;
            DisabledSitesTextBoxTeams.IsEnabled = false;
            AddToDisabledSitesListTeams.Visibility = Visibility.Hidden;
            AddToDisabledSitesListTeams.IsEnabled = false;

            DisabledSitesTextBoxTeams.Text = "";
        }
    }



    private async void CreateSharedMailbox(object sender, RoutedEventArgs e)
    {

        try
        {

            string allItems = string.Join(",", DisabledSitesBox.Items.Cast<string>());

            if (company_input.SelectedItem != null)
            {
                if (Firstname_input.Text != null && Lastname_input.Text != null && company_input.SelectedItem != null && allItems != null && Ticketnumber.Text != null && IsNumeric(Ticketnumber.Text) == true)
                {
                    try
                    {

                    
                        string content = $"{Firstname_input.Text},{Lastname_input.Text},{company_input.SelectedItem}" + ",members{" + allItems + "}" + $",{Ticketnumber.Text}";
                        bool check = await _SharePointFileService.UpdateCreateSharedMailbox(content);
                        if(check)
                        {
                            string logData = $"displayname:{Firstname_input.Text},name:{Lastname_input.Text},member:{allItems},comapny:{company_input.SelectedItem},ticketnumber:{Ticketnumber.Text}";
                            await _LogService.createLog("createSharedMailbox", logData);
                            MessageBox.Show($"The Shared Mailbox will be created today", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"ErrorMessage: Failed to upload File", "Error: 0x00076", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception cex)
                    {
                        MessageBox.Show($"ErrorMessage: {cex.Message}", "Error: 0x00077", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {

                    MessageBox.Show("Please enter valid values!", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            else
            {
                MessageBox.Show("Please select a company", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }catch (Exception ex)
        {
            MessageBox.Show($"ErrorMessage: {ex.Message}", "Error: 0x00070", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CreateTeamsgroup(object sender, RoutedEventArgs e)
    {

        try
        {

            string allItems = string.Join(",", DisabledSitesBoxTeams.Items.Cast<string>());

            
                if (Firstname_inputTeams.Text != null && Lastname_inputTeams.Text != null && UserEmailTeams.Text != null && allItems != null && Description.Text != null && TicketnumberTeams.Text != null && IsNumeric(TicketnumberTeams.Text) == true)
                {
                    try
                    {


                        string content = $"{Firstname_inputTeams.Text},{Lastname_inputTeams.Text},{company_inputTeams.SelectedItem},{UserEmailTeams.Text},members:" + "{" + allItems + "}" + $",{TicketnumberTeams.Text},{Description.Text}";
                        bool check = await _SharePointFileService.UpdateCreateTeamsgroup(content);
                        if (check)
                        {
                            string logData = $"displayname:{Firstname_inputTeams.Text},mailnickname:{Lastname_inputTeams.Text},owner:{UserEmailTeams.Text},members:{allItems},visiblilty:{company_inputTeams.SelectedItem},ticketnumber:{TicketnumberTeams.Text},description:{Description.Text}";
                            await _LogService.createLog("createTeamsgroup", logData);
                            MessageBox.Show($"The Teamsgroup will be created today", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"ErrorMessage: Failed to upload File", "Error: 0x00079", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception cex)
                    {
                        MessageBox.Show($"ErrorMessage: {cex.Message}", "Error: 0x00080", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {

                    MessageBox.Show("Please enter valid values!", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            
            
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ErrorMessage: {ex.Message}", "Error: 0x00078", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
