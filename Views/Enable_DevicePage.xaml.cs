using System.Windows.Controls;

using ADTool.ViewModels;

namespace ADTool.Views;

public partial class Enable_DevicePage : Page
{
    public Enable_DevicePage(Enable_DeviceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
