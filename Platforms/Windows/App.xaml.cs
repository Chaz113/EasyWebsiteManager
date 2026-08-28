using Microsoft.Maui.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace EasyWebsiteManager.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp()
    {
        var app = MauiProgram.CreateMauiApp();

        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(
            "EasyWebsiteManagerTitleBar",
            (handler, view) =>
            {
                UpdateTitleBarTheme();
            });

        return app;
    }

    public static void UpdateTitleBarTheme()
    {
        try
        {
            var mauiApp =
                Microsoft.Maui.Controls.Application.Current;

            if (mauiApp == null ||
                mauiApp.Windows.Count == 0)
            {
                return;
            }

            var mauiWindow =
                mauiApp.Windows[0];

            if (mauiWindow.Handler?.PlatformView
                is not Microsoft.UI.Xaml.Window nativeWindow)
            {
                return;
            }

            var appWindow =
                nativeWindow.AppWindow;

            if (appWindow == null)
                return;

            if (!AppWindowTitleBar.IsCustomizationSupported())
                return;

            var appearance =
                Preferences.Default.Get(
                    "Appearance",
                    "System");

            appWindow.TitleBar.PreferredTheme =
                appearance switch
                {
                    "Dark" =>
                        TitleBarTheme.Dark,

                    "Light" =>
                        TitleBarTheme.Light,

                    _ =>
                        mauiApp.RequestedTheme == AppTheme.Dark
                            ? TitleBarTheme.Dark
                            : TitleBarTheme.Light
                };
        }
        catch
        {
            // If Windows title-bar customization is unavailable,
            // leave the system title bar unchanged.
        }
    }
}