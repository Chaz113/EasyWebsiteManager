using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class SettingsPage : ContentPage
{
    public event Action? SettingsChanged;
    public event Action? ExportDataRequested;
 
    private void ExportDataButton_Clicked(
    object? sender,
    EventArgs e)
    {
        ExportDataRequested?.Invoke();
    }
    public event Action? ImportDataRequested;
    private void ImportDataButton_Clicked(
    object? sender,
    EventArgs e)
    {
        ImportDataRequested?.Invoke();
    }

    private readonly AppSettings _settings;

    public SettingsPage(AppSettings settings)
    {
        InitializeComponent();

        _settings = settings;

        ConfirmDeleteSwitch.IsToggled =
            _settings.ConfirmDelete;

        DarkModeSwitch.IsToggled =
            _settings.Appearance == "Dark";

        UpdateStatusLabels();

        ConfirmDeleteSwitch.Toggled +=
            ConfirmDeleteSwitch_Toggled;

        DarkModeSwitch.Toggled +=
            DarkModeSwitch_Toggled;
    }

    private void ConfirmDeleteSwitch_Toggled(
        object? sender,
        ToggledEventArgs e)
    {
        _settings.ConfirmDelete = e.Value;

        ConfirmDeleteStatusLabel.Text =
            e.Value ? "ON" : "OFF";

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

        _settings.Appearance = appearance;

        Preferences.Default.Set(
            "Appearance",
            appearance);

        Application.Current!.UserAppTheme =
            e.Value
                ? AppTheme.Dark
                : AppTheme.Light;

#if WINDOWS
        EasyWebsiteManager.WinUI.App.UpdateTitleBarTheme();
#endif

        AppearanceStatusLabel.Text =
            e.Value ? "Dark" : "Light";

        SettingsChanged?.Invoke();
    }
    private async void HelpButton_Clicked(
    object? sender,
    EventArgs e)
    {
        var page = new HelpPage();

        await Navigation.PushModalAsync(page);
    }

    private async void AboutButton_Clicked(
        object? sender,
        EventArgs e)
    {
        var page = new AboutPage();

        await Navigation.PushModalAsync(page);
    }

    private void UpdateStatusLabels()
    {
        ConfirmDeleteStatusLabel.Text =
            _settings.ConfirmDelete
                ? "ON"
                : "OFF";

        AppearanceStatusLabel.Text =
            _settings.Appearance == "Dark"
                ? "Dark"
                : "Light";
    }

    private async void CloseButton_Clicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}