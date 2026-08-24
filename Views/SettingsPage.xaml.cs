using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class SettingsPage : ContentPage
{
    public event Action? SettingsChanged;
    private readonly AppSettings _settings;

    public SettingsPage(AppSettings settings)
    {
        InitializeComponent();

        _settings = settings;

        ConfirmDeleteSwitch.IsToggled = _settings.ConfirmDelete;
        ConfirmDeleteSwitch.Toggled += ConfirmDeleteSwitch_Toggled;
    }

    private void ConfirmDeleteSwitch_Toggled(object? sender, ToggledEventArgs e)
    {
        _settings.ConfirmDelete = e.Value;

        SettingsChanged?.Invoke();
    }

    private async void CloseButton_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}