namespace EasyWebsiteManager.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();

        VersionLabel.Text =
            $"Version {AppInfo.Current.VersionString}";
    }

    private async void CloseButton_Clicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}