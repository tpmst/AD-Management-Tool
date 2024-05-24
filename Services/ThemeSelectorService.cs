using System.Windows;

using ADTool.Contracts.Services;
using ADTool.Models;

using ControlzEx.Theming;

using MahApps.Metro.Theming;

namespace ADTool.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string HcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
    private const string HcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";
    private const string HcRedTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Amber.xaml";

    public ThemeSelectorService()
    {
    }

    public void InitializeTheme()
    {
        // We've added basic high contrast dictionaries for Dark and Light themes
        // Please complete these themes following the docs on https://mahapps.com/docs/themes/thememanager#creating-custom-themes
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcDarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcLightTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        //ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcRedTheme), MahAppsLibraryThemeProvider.DefaultInstance));

        var theme = GetCurrentTheme();
        SetTheme(theme);
    }

    public void SetTheme(AppTheme theme)
    {
        if (theme == AppTheme.Default)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
        }
        else
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
            ThemeManager.Current.SyncTheme();
            ThemeManager.Current.ChangeTheme(Application.Current, $"{theme}.Blue", SystemParameters.HighContrast);
        }

        App.Current.Properties["Theme"] = theme.ToString();
    }

    public AppTheme GetCurrentTheme()
    {
        if (App.Current.Properties.Contains("Theme"))
        {
            var themeName = App.Current.Properties["Theme"].ToString();
            Enum.TryParse(themeName, out AppTheme theme);
            return theme;
        }

        return AppTheme.Default;
    }
}
