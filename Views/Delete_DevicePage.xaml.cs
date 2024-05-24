using System.IO;
using System.Windows;
using System.Windows.Controls;
using ADTool.Contracts.Services;
using ADTool.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace ADTool.Views;

public partial class Delete_DevicePage : Page
{
    private readonly IADServices _aDServices;
    private readonly ILogService _logService;
    private readonly ISharePointFileService _sharePointFileService;

    private string[] ClearItems = { "" };
    private Dictionary<string, string> ComputerNames;

    public Delete_DevicePage(Delete_DeviceViewModel viewModel, IADServices aDServices, ILogService logService, ISharePointFileService sharePointFileService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _aDServices = aDServices;
        _logService = logService;
        _sharePointFileService = sharePointFileService;
        
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (Computer_Name_ComboBox.SelectedItem != null)
            {
                string selectedKey = Computer_Name_ComboBox.SelectedItem.ToString();
                string selectedValue = ComputerNames[selectedKey]; // Assuming GroupNames is your dictionary
                Input_DeleteDevice.Visibility = Visibility.Visible;
                Input_DeleteDevice.IsEnabled = true;
                Computer_Name_ComboBox.Visibility = Visibility.Hidden;
                Computer_Name_ComboBox.IsEnabled = false;


                Input_DeleteDevice.Text = selectedKey;
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
        if (!Input_DeleteDevice.Text.IsNullOrEmpty())
        {
            string searchfilter_groupName = Input_DeleteDevice.Text;
            ComputerNames = _aDServices.FindComputers(searchfilter_groupName);
            if (ComputerNames.Keys.Count > 1)
            {
                Input_DeleteDevice.Visibility = Visibility.Hidden;
                Input_DeleteDevice.IsEnabled = false;
                Computer_Name_ComboBox.Visibility = Visibility.Visible;
                Computer_Name_ComboBox.IsEnabled = true;
                Computer_Name_ComboBox.ItemsSource = ComputerNames.Keys;

            }
            else if (ComputerNames.Keys.Count == 1)
            {
                Input_DeleteDevice.Text = ComputerNames.Keys.FirstOrDefault();
            }
        }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        
        if (!_aDServices.ExistsCom(Input_DeleteDevice.Text))
        {
            MessageBox.Show("Please enter an valid Computername", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            try
            {
                
                string logData = $"tag:{Input_DeleteDevice.Text}";
                await _logService.createLog("Delete_Device",logData);
                _aDServices.DeleteDevice(Input_DeleteDevice.Text);
                Output_DeleteDevice.Text = "The Device with the name " + Input_DeleteDevice.Text + " has been Disabled";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error accured please contact an Administrator: {ex}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
