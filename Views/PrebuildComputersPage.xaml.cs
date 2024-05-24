using System.Windows.Controls;

using ADTool.ViewModels;

namespace ADTool.Views;

public partial class PrebuildComputersPage : Page
{
    public PrebuildComputersPage(PrebuildComputersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
