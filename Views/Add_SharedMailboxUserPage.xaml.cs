using System.Windows.Controls;

using ADTool.ViewModels;

namespace ADTool.Views;

public partial class Add_SharedMailboxUserPage : Page
{
    public Add_SharedMailboxUserPage(Add_SharedMailboxUserViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
