namespace EasyWebsiteManager;

public partial class App : Application
{
    private const double DefaultWidth = 760;
    private const double DefaultHeight = 680;

    private const double MinimumWidth = 650;
    private const double MinimumHeight = 550;

    public App()
    {
        InitializeComponent();

        ApplySavedAppearance();
    }

    private void ApplySavedAppearance()
    {
        var appearance =
            Preferences.Default.Get("Appearance", "System");

        UserAppTheme =
            appearance switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        var window = new Window(new MainPage())
        {
            Width =
                Preferences.Default.Get(
                    "WindowWidth",
                    DefaultWidth),

            Height =
                Preferences.Default.Get(
                    "WindowHeight",
                    DefaultHeight),

            X =
                Preferences.Default.Get(
                    "WindowX",
                    100.0),

            Y =
                Preferences.Default.Get(
                    "WindowY",
                    100.0),

            MinimumWidth = MinimumWidth,
            MinimumHeight = MinimumHeight
        };

        window.SizeChanged += Window_SizeChanged;
        window.Destroying += Window_Destroying;

        return window;
    }

    private void Window_SizeChanged(
        object? sender,
        EventArgs e)
    {
        if (sender is Window window)
        {
            SaveWindowBounds(window);
        }
    }

    private void Window_Destroying(
        object? sender,
        EventArgs e)
    {
        if (sender is Window window)
        {
            SaveWindowBounds(window);
        }
    }

    private static void SaveWindowBounds(Window window)
    {
        if (window.Width > 0)
        {
            Preferences.Default.Set(
                "WindowWidth",
                window.Width);
        }

        if (window.Height > 0)
        {
            Preferences.Default.Set(
                "WindowHeight",
                window.Height);
        }

        if (window.X >= 0)
        {
            Preferences.Default.Set(
                "WindowX",
                window.X);
        }

        if (window.Y >= 0)
        {
            Preferences.Default.Set(
                "WindowY",
                window.Y);
        }
    }
}