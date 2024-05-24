using System.Windows;
using System.Windows.Controls;
using ADTool.Contracts.Services;
using ADTool.Core.Models;
using ADTool.ViewModels;

namespace ADTool.Views;

public partial class Get_BitlockerPage : Page
{
    private readonly IADServices _ADServices;
    private readonly ILogService _LogService;


    public Get_BitlockerPage(Get_BitlockerViewModel viewModel, IADServices aDServices, ILogService logService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ADServices = aDServices;
        _LogService = logService;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        string BitlockerKeyID = Input_BitLockerKey.Text;
        
            try
            {
            BitlockerKey bitlockerKey = _ADServices.GetBitLockerRecoveryKeyFromAAD(BitlockerKeyID);
            Output_BitLockerKey.Text = $"The BitlockerKey: {bitlockerKey.key}";

            string logData = $"bitlockerKeyId:{BitlockerKeyID}";
            _LogService.createLog("Get_Bitlocker", logData);
        }
            catch(Exception ex)
            {
                MessageBox.Show($"An error occured please contact an Administrator: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        

    }
}
