using ADTool.Contracts.Views;
using ADTool.ViewModels;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MahApps.Metro.Controls;
using ADTool.Services;
using System.Windows;
using System.IO;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Forms;
using ADTool.Class;
using System.Windows.Controls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ADTool.Views;

public partial class LogInWindow : MetroWindow, ILogInWindow
{
    public readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;

    public LogInWindow(LogInViewModel viewModel)
    {
        
        InitializeComponent();
        DataContext = viewModel;
        Password_Text_Box.PasswordChanged += Password_Text_Box_PasswordChanged;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
    private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        WindowInteropHelper helper = new WindowInteropHelper(this);
        SendMessage(helper.Handle, 161, 2, 0);
    }
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {


            App.Current.Shutdown();

        });
    }

    void opacity()
    {
        DataContext = new WindowBlureffect(this, AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT) { BlurOpacity = 100 };
    }

   
        private void Password_Text_Box_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Password_Text_Box.Password))
        {
            LogIn.IsEnabled = false;
        }
        else
        {
            LogIn.IsEnabled = true;
            try
            {

                string Paths = Path.Combine(_localAppData, "usercreds.json");

                UserCredentialManager.SaveCredentials(Paths, Username_Text_Box.Text, Password_Text_Box.Password);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Login failure", "Error: 0x00007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

        public void ShowWindow()
                => Show();

            public void CloseWindow()
                => Close();
}
