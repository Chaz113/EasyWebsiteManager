using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class SettingsPage : ContentPage
{
    public event Action? SettingsChanged;
    public event Action? ExportDataRequested;
    public event Action? ImportDataRequested;

    private readonly AppSettings _settings;

    public SettingsPage(
        AppSettings settings)
    {
        InitializeComponent();

        _settings = settings;

        ConfirmDeleteSwitch.IsToggled =
            _settings.ConfirmDelete;

        SetAppearanceControls();

        UpdateConfirmDeleteStatus();

        ConfirmDeleteSwitch.Toggled +=
            ConfirmDeleteSwitch_Toggled;

        DarkModeSwitch.Toggled +=
            DarkModeSwitch_Toggled;
    }

    private void ExportDataButton_Clicked(
        object? sender,
        EventArgs e)
    {
        ExportDataRequested?.Invoke();
    }

    private void ImportDataButton_Clicked(
        object? sender,
        EventArgs e)
    {
        ImportDataRequested?.Invoke();
    }

    private void ConfirmDeleteSwitch_Toggled(
        object? sender,
        ToggledEventArgs e)
    {
        _settings.ConfirmDelete =
            e.Value;

        ConfirmDeleteStatusLabel.Text =
            e.Value
                ? "ON"
                : "OFF";

        SettingsChanged?.Invoke();
    }

    private void DarkModeSwitch_Toggled(
        object? sender,
        ToggledEventArgs e)
    {
        var appearance =
            e.Value
                ? "Dark"
                : "Light";

        _settings.Appearance =
            appearance;

        Preferences.Default.Set(
            "Appearance",
            appearance);

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme =
                e.Value
                    ? AppTheme.Dark
                    : AppTheme.Light;
        }

#if WINDOWS
        EasyWebsiteManager.WinUI.App.UpdateTitleBarTheme();
#endif

        AppearanceStatusLabel.Text =
            appearance;

        SettingsChanged?.Invoke();
    }

    private void SetAppearanceControls()
    {
        var appearance =
            _settings.Appearance;

        bool isDark;

        if (appearance == "Dark")
        {
            isDark = true;
        }
        else if (appearance == "Light")
        {
            isDark = false;
        }
        else
        {
            // "System" follows the theme currently resolved
            // by the operating system.
            isDark =
                Application.Current?.RequestedTheme ==
                AppTheme.Dark;
        }

        DarkModeSwitch.IsToggled =
            isDark;

        AppearanceStatusLabel.Text =
            isDark
                ? "Dark"
                : "Light";
    }

    private void UpdateConfirmDeleteStatus()
    {
        ConfirmDeleteStatusLabel.Text =
            _settings.ConfirmDelete
                ? "ON"
                : "OFF";
    }

    private async void HelpButton_Clicked(
        object? sender,
        EventArgs e)
    {
        var page =
            new HelpPage();

        await Navigation.PushModalAsync(
            page);
    }

    private async void AboutButton_Clicked(
        object? sender,
        EventArgs e)
    {
        var page =
            new AboutPage();

        await Navigation.PushModalAsync(
            page);
    }

    private async void CloseButton_Clicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}