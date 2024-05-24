using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using ADTool.Contracts.Services;
using ADTool.Contracts.Views;
using ADTool.Core.Contracts.Services;
using ADTool.Core.Services;
using ADTool.Models;
using ADTool.Services;
using ADTool.ViewModels;
using ADTool.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

namespace ADTool;

// For more information about application lifecycle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview

// WPF UI elements use language en-US by default.
// If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
// Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
public partial class App : Application
{

    private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");
    private readonly string _localAppData = AppDomain.CurrentDomain.BaseDirectory;
    private string UserCredsPath;

    private IHost _host;


    public T GetService<T>()
        where T : class
        => _host.Services.GetService(typeof(T)) as T;

    public App()
    {
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        bool exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;

        if (exists)
        {
            System.Windows.MessageBox.Show("Your new opend session will be close. " +
                "" +
                "You can run only one at the Time","Attention",MessageBoxButton.OK,MessageBoxImage.Error);
            Application.Current.Shutdown();

        }
        else
        {

            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
            _host = Host.CreateDefaultBuilder(e.Args)
                    .ConfigureAppConfiguration(c =>
                    {
                        c.SetBasePath(appLocation);
                    })
                    .ConfigureServices(ConfigureServices)
                    .Build();

            await _host.StartAsync();
        }
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<AllOUs, AllOUs>();
        services.AddSingleton<IADServices, ADServices>();
        services.AddSingleton<IEvelationService, EvelationService>();
        


        // App Host
        services.AddHostedService<ApplicationHostService>();
        services.AddSingleton<IIdentityCacheService, IdentityCacheService>();
        services.AddHttpClient("msgraph", client =>
        {
            client.BaseAddress = new System.Uri("https://graph.microsoft.com/v1.0/");
        });

        // Activation Handlers


        // Core Services

        services.AddSingleton<IMicrosoftGraphService, MicrosoftGraphService>();
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ISharePointFileService, SharePointFileService>();
        services.AddSingleton<IADServices, ADServices>();


        // Services
        services.AddSingleton<ISampleDataService, SampleDataService>();
        //services.AddSingleton<IADServices, ADServices>();
        services.AddSingleton<IPassword_Generator, Password_Generator>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDashBoardService, DashBoardsServices>();
        services.AddSingleton<IUpdateService, UpdateService>();
        
        


        // Views and ViewModels
        services.AddTransient<IShellWindow, ShellWindow>();
        services.AddTransient<ShellViewModel>();

        services.AddTransient<LAPSViewModel>();
        services.AddTransient<LAPSPage>();

        services.AddTransient<Delete_DeviceViewModel>();
        services.AddTransient<Delete_DevicePage>();

        services.AddTransient<Check_User_AccountViewModel>();
        services.AddTransient<Check_User_AccountPage>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        services.AddSingleton<IUserDataService, UserDataService>();
        services.AddTransient<ILogInWindow, LogInWindow>();
        services.AddTransient<LogInViewModel>();
        services.AddScoped<LogInWindow>();
        services.AddScoped<Check_User_AccountPage>();

        services.AddTransient<Disable_Enable_AccountViewModel>();
        services.AddTransient<Disable_Enable_AccountPage>();

        services.AddTransient<Enable_AccountViewModel>();
        services.AddTransient<Enable_AccountPage>();

        services.AddTransient<Get_BitlockerViewModel>();
        services.AddTransient<Get_BitlockerPage>();

        services.AddTransient<Add_Remove_User_GroupViewModel>();
        services.AddTransient<Add_Remove_User_GroupPage>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardPage>();

        services.AddTransient<PrebuildComputersViewModel>();
        services.AddTransient<PrebuildComputersPage>();

        services.AddTransient<NewUserViewModel>();
        services.AddTransient<NewUserPage>();


        services.AddTransient<Enable_DeviceViewModel>();
        services.AddTransient<Enable_DevicePage>();

        services.AddTransient<Add_SharedMailboxUserViewModel>();
        services.AddTransient<Add_SharedMailboxUserPage>();

        services.AddTransient<Create_Shared_MailboxViewModel>();
        services.AddTransient<Create_Shared_MailboxPage>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }


    private async void OnExit(object sender, ExitEventArgs e)
    {
        AppConfigManager.TryLoadUserCredentialsPath(AppConfPath, out UserCredsPath);
        string Paths = Path.Combine(_localAppData, UserCredsPath);
        //TODO: Delete if puplished
        //File.Delete(Paths);
        await _host.StopAsync();
        _host.Dispose();
        _host = null;
        Process.GetCurrentProcess().Kill();
        Application.Current.Shutdown();
        
    }


    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        MessageBox.Show(e.Exception.Message);
    }
}

