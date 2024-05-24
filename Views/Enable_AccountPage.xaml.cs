using System.Windows;
using System.Windows.Controls;
using ADTool.Contracts.Services;
using ADTool.Services;
using ADTool.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace ADTool.Views;

public partial class Enable_AccountPage : Page
{
    public Enable_AccountPage(Enable_AccountViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
