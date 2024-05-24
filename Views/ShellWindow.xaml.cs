using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ADTool.Contracts.Services;
using ADTool.Contracts.Views;
using ADTool.Core.Contracts.Services;
using ADTool.Services;
using ADTool.ViewModels;
using AutoHotkey.Interop;
using MahApps.Metro.Controls;
using Microsoft.IdentityModel.Tokens;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ADTool.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    private readonly IDashBoardService _boardService;
    private readonly IADServices _services;
    private readonly IPageService _pageService;
    private readonly INavigationService _navigationService;
    private readonly ISharePointFileService _sharePointFileService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUpdateService _updateService;

    private readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;
    private string _username;
    private string _password;
    private int Timer = 7200000;

    private ILogInWindow _logInWindow;

    private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
    private static string tempAppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppContemp.json");
    private int i;

    public ShellWindow(ShellViewModel viewModel, IUpdateService updateService , IDashBoardService boardService, IServiceProvider serviceProvider,IADServices services, IPageService pageService, INavigationService navigationService, ISharePointFileService sharePointFileService)
    {
        InitializeComponent();
        
        

        DataContext = viewModel;
        _boardService = boardService;
        _services = services;
        _serviceProvider = serviceProvider;
        _pageService = pageService;
        _navigationService = navigationService;
        _sharePointFileService = sharePointFileService;
        _updateService = updateService;
        string Paths = Path.Combine(_localAppData, "usercreds.json");
        UserCredentialManager.TryLoadUserCredentials(Paths, out _username, out _password);
        Task.Run(() => CheckLocked());
        GetConf();
        initiakizePicture();
        Task.Run(() => CheckInput());  
        HotkeyService.HotkeyPassword();
        HotkeyService.HotkeyLocalAdmin();
        this.MouseLeftButtonDown += ChceckClickMouse;
    }
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
    private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        WindowInteropHelper helper = new WindowInteropHelper(this);
        SendMessage(helper.Handle, 161, 2, 0);
    }
    private async void btnClose_Click(object sender, RoutedEventArgs e)
    {
        string UserCredsPath;
        //TODO: Enable on Start
        string credsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.json");
        if (File.Exists(credsPath))
        {
            //File.Delete(credsPath);
        }
        AppConfigManager.TryLoadUserCredentialsPath(AppConfPath, out UserCredsPath);
        string Paths = Path.Combine(_localAppData, UserCredsPath);
        //TODO: Delete if puplished
        //File.Delete(Paths);

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            System.Environment.Exit(0);

        });
    }
    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Normal)
            this.WindowState = WindowState.Maximized;
        else this.WindowState = WindowState.Normal;
    }

    private void initiakizePicture()
    {
        string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output-onlinepngtools.png");

        // Create a BitmapImage and set its source to the image file
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new System.Uri(imagePath, System.UriKind.RelativeOrAbsolute);
        bitmapImage.EndInit();

        // Set the Source property of the Image control to the BitmapImage
        imageLogo.Source = bitmapImage;
    }

    private void navigate_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationService.CanGoBack)
        {
            List<string> page = _navigationService.GetOpendPage();
            _navigationService.GoBack();
            if (page.Last() == LAPSViewModel.Name)
            {
                LAPSViewModel.IsChecked = true;
            }
            if (page.Last() == DashBoardViewModel.Name)
            {
                DashBoardViewModel.IsChecked = true;
            }
            if (page.Last() == Delete_DeviceViewModel.Name)
            {
                Delete_DeviceViewModel.IsChecked = true;
            }
            if(page.Last() == Enable_DeviceViewModel.Name)
            {
                Enable_DeviceViewModel.IsChecked= true;
            }
            if (page.Last() == Get_BitlockerViewModel.Name)
            {
                Get_BitlockerViewModel.IsChecked = true;
            }
            if (page.Last() == Enable_AccountViewModel.Name)
            {
                Enable_AccountViewModel.IsChecked = true;
            }
            if (page.Last() == Check_User_AccountViewModel.Name)
            {
                Check_User_AccountViewModel.IsChecked = true;
            }
            if (page.Last() == Add_Remove_User_GroupViewModel.Name)
            {
                Add_Remove_User_GroupViewModel.IsChecked = true;
            }
            if (page.Last() == NewUserViewModel.Name)
            {
                NewUserViewModel.IsChecked = true;
            }
            if (page.Last() == Disable_Enable_AccountViewModel.Name)
            {
                Disable_Enable_AccountViewModel.IsChecked = true;
            }
            if (page.Last() == Add_SharedMailboxUserViewModel.Name)
            {
                Add_SharedMailboxUserViewModel.IsChecked = true;
            }
            if (page.Last() == Create_Shared_MailboxViewModel.Name)
            {
                Create_Shared_MailboxViewModel.IsChecked = true;
            }
            if (page.Last() == SettingsViewModel.Name)
            {
                SettingsViewModel.IsChecked = true;
            }
            if(page.Last().IsNullOrEmpty())
            {
                DashBoardViewModel.IsChecked = true;
            }
        }
    }

    public void CheckIfSessionIsRunning()
    {
        // Choose a unique name for your mutex

        string appName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
        Process[] processes = Process.GetProcessesByName(appName);

        if (processes.Length > 1)
        {
            // More than one instance is running. Close the second instance.
            foreach (var process in processes)
            {
                if (process.Id != Process.GetCurrentProcess().Id)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Failed to close the second instance: " + ex.Message);
                    }
                }
            }
        }
        else
        {
            // This is the first instance. Your program's logic goes here.
            
        }
    }


private async Task CheckLocked()
    {
        string capturedUsername = _username; // Capture the username value

        Thread.Sleep(10000);

        await Task.Run(() =>
        {
            while (true)
            {
                _boardService.refreshDashBoardValues();
                bool check = _services.IsAccountDisabledOrLocked(capturedUsername); // Use the captured value
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // Update the UI or perform any UI-related actions here based on the 'check' result
                    if (check)
                    {
                        string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.json");
                        //File.Delete(credPath);

                        _logInWindow = _serviceProvider.GetService(typeof(ILogInWindow)) as ILogInWindow;
                        _logInWindow.ShowWindow();

                        Window currentWindow = System.Windows.Application.Current.MainWindow;
                        currentWindow.Close();
                    }

                    //CheckIfSessionIsRunning();

                    _boardService.refreshDashBoardValues();
                });
                Thread.Sleep(300000);
            }
        });
    }

    private void ChceckClickMouse(object sender, EventArgs e)
    {
        Timer = 7200000;
    }

    private async Task CheckInput()
    {
        string capturedUsername = _username; // Capture the username value

        await Task.Run(() =>
        {
            
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    while (Timer != 0)
                    { 
                        if(Timer == 0)
                        {
                            string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usercreds.json");
                            //File.Delete(credPath);

                            _logInWindow = _serviceProvider.GetService(typeof(ILogInWindow)) as ILogInWindow;
                            _logInWindow.ShowWindow();

                            Window currentWindow = System.Windows.Application.Current.MainWindow;
                            currentWindow.Close();
                        }
                        else
                        {
                            Timer = Timer - 1;
                        }
                    }
                });
                
            
        });
    }




    private async void GetConf()
    {
        try
        {
            string conf = await _sharePointFileService.GetFileBody();
            File.Delete(tempAppConfPath);
            File.WriteAllText(tempAppConfPath, conf);

            string standartDC;
            string LAPSController;
            string dom;
            string container;
            string domain;

            int _passwordLength;
            string _passwordComplexity;
            string[] _targetOU;
            string _logPath;
            string[] _company;
            Dictionary<string, List<string>> _OUsByRegion;
            Dictionary<string, string> _CityBranch;
            string[] _countries;
            bool _SavePointLogs;
            string EvelationPath;
            string UserCredsPath;
            int NewAppVersion;
            int OldAppVersion;
            string[] DisabledSites;

            AppConfigManager.TryLoadAppConSettingAll(tempAppConfPath, out standartDC, out LAPSController, out dom, out container, out domain, out _passwordLength, out _passwordComplexity, out _targetOU, out _logPath, out _company, out _OUsByRegion, out _CityBranch, out _countries, out _SavePointLogs, out EvelationPath, out NewAppVersion, out UserCredsPath, out DisabledSites);

            if (File.Exists(AppConfPath))
            {
                AppConfigManager.TryLoadVersionfile(AppConfPath, out OldAppVersion);

                if (NewAppVersion != OldAppVersion)
                {
                    File.Delete(tempAppConfPath);
                    AppConfigManager.SaveAppconfig(tempAppConfPath, standartDC, _passwordLength, _passwordComplexity, domain, container, dom, _targetOU, _logPath, UserCredsPath, _company, _OUsByRegion, _CityBranch, _countries, _SavePointLogs, EvelationPath, NewAppVersion, LAPSController, DisabledSites);

                    MessageBox.Show("The Application is about to update!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    bool updated = await _updateService.UpdateApplication(NewAppVersion, OldAppVersion);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Environment.Exit(0);

                        });
                }
            }
            else
            {
                AppConfigManager.SaveAppconfig(AppConfPath, standartDC, _passwordLength, _passwordComplexity, domain, container, dom, _targetOU, _logPath, UserCredsPath, _company, _OUsByRegion, _CityBranch, _countries, _SavePointLogs, EvelationPath, NewAppVersion, LAPSController, DisabledSites);
                MessageBox.Show("The Application is about to restart to creat your Profile!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Environment.Exit(0);

                });
            }
        }catch(Exception ex)
        {
            MessageBox.Show($"ErrorMessage: {ex.Message}", "Error: 0x00083");
        }
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();

   
}
