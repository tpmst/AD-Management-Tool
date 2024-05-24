using System.Windows;
using System.Windows.Controls;
using ADTool.Contracts.Services;
using ADTool.Services;
using ADTool.ViewModels;
using Microsoft.IdentityModel.Tokens;
using System.Windows.Media;

namespace ADTool.Views;

public partial class LAPSPage : Page
{
    private readonly IADServices _aDServices;
    private readonly ILogService _logService;

    private string[] ClearItems = {""};
    int passwordButton;
    int adminButton;
    private Dictionary<string, string> ComputerNames;

    public LAPSPage(LAPSViewModel viewModel, IADServices aDServices, ILogService logService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _aDServices = aDServices;
        _logService = logService;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Clipboard.SetText(Output_LAPS.Content.ToString());

        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Error copying text to clipboard: " + ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Copy_Click_ipaddress(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Clipboard.SetText(Output_ipaddress.Content.ToString());

        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Error copying text to clipboard: " + ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (Computer_Name_ComboBox.SelectedItem != null)
            {
                string selectedKey = Computer_Name_ComboBox.SelectedItem.ToString();
                string selectedValue = ComputerNames[selectedKey]; // Assuming GroupNames is your dictionary
                Input_compName.Visibility = Visibility.Visible;
                Input_compName.IsEnabled = true;
                Computer_Name_ComboBox.Visibility = Visibility.Hidden;
                Computer_Name_ComboBox.IsEnabled = false;


                Input_compName.Text = selectedKey;
                Computer_Name_ComboBox.ItemsSource = ClearItems;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Check_Names(object sender, EventArgs e)
    {
        if (!Input_compName.Text.IsNullOrEmpty())
        {
            string searchfilter_groupName = Input_compName.Text;
            ComputerNames = _aDServices.FindComputers(searchfilter_groupName);
            if (ComputerNames.Keys.Count > 1)
            {
                Input_compName.Visibility = Visibility.Hidden;
                Input_compName.IsEnabled = false;
                Computer_Name_ComboBox.Visibility = Visibility.Visible;
                Computer_Name_ComboBox.IsEnabled = true;
                Computer_Name_ComboBox.ItemsSource = ComputerNames.Keys;
            }
            else if (ComputerNames.Keys.Count == 1)
            {
                Input_compName.Text = ComputerNames.Keys.FirstOrDefault();
            }
        }
    }

    private void Button_ConnectTeamviewer(object sender, System.Windows.RoutedEventArgs e)
    {

    }
    private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        
        if (Input_compName.Text.IsNullOrEmpty() )
        {
            System.Windows.MessageBox.Show("Please enter a valid Computername", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            if(_aDServices.ExistsCom(Input_compName.Text) == true)
            {
                    try
                    {
                        
                        string ipaddress = await _aDServices.GetIPAddress(Input_compName.Text);
                        if (ipaddress.IsNullOrEmpty())
                        {
                            Output_ipaddress.Content = "Not available";
                            onlineLabel.Content = "offline";
                        }
                        else
                        {
                            Output_ipaddress.Content = ipaddress;
                            onlineLabel.Content = "online";
                        }
                        
                        
                        Output_LAPS.Content = _aDServices.GetLAPS(Input_compName.Text);
                        Output_compName.Content = Input_compName.Text;
                        
                    try
                    {
                        
                        AutoCopyConfManager.TryLoadButtonLocalAdmin(out adminButton);
                        AutoCopyConfManager.TryLoadButtonPassword(out passwordButton);
                    }
                    catch
                    {
                        MessageBox.Show("Hotkeys couldn´t be loaded", "Error 0x00090", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                        
                        if (Output_LAPS.Content.ToString().IsNullOrEmpty())
                        {
                            AutoCopyConfManager.SaveAutoCopyConf("HiobHiob63450", passwordButton, adminButton);
                        }
                        else
                        {
                            AutoCopyConfManager.SaveAutoCopyConf(Output_LAPS.Content.ToString(), passwordButton, adminButton);
                        }
                        if (Output_LAPS.Content.ToString().IsNullOrEmpty())
                        {
                            Output_LAPS.Content = "There is no LAPS in AD so try the defaultpassword";
                        }
                        else
                        {
                            CopyButton.Visibility = Visibility.Visible;
                            CopyButton.IsEnabled = true;
                            CopyButtonIPaddress.Visibility = Visibility.Visible;
                            CopyButtonIPaddress.IsEnabled = true;
                        }
                        osLabel.Content = _aDServices.GetOSVersion(Input_compName.Text);
                        bool enabled = _aDServices.IsDeviceDisabledOrLocked(Input_compName.Text);
                        if (!enabled)
                        {
                            CompIcon.Kind = MahApps.Metro.IconPacks.PackIconRemixIconKind.CheckboxCircleLine;
                            SolidColorBrush brush = new SolidColorBrush(Colors.LimeGreen);
                            CompIcon.Foreground = brush;
                            Enable_Disable_Label.Content = "enabled";
                        }
                        else
                        {
                            CompIcon.Kind = MahApps.Metro.IconPacks.PackIconRemixIconKind.CloseCircleLine;
                            SolidColorBrush brush = new SolidColorBrush(Colors.Red);
                            CompIcon.Foreground = brush;
                            Enable_Disable_Label.Content = "disabled";
                        }

                        string logData = $"tag:{Input_compName.Text}";
                        _logService.createLog("Get_LAPS", logData);


                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Soemthing went Wrong please contact an Administrator: {ex}", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                
                

            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a valid Computername", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


    }

}
