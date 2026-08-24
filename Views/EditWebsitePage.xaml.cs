using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class EditWebsitePage : ContentPage
{
    private readonly WebsiteItem _website;

    public event Action<WebsiteItem>? WebsiteUpdated;

    public EditWebsitePage(WebsiteItem website)
    {
        InitializeComponent();

        _website = website;

        WebsiteNameEntry.Text = website.Name;
        UrlEntry.Text = website.Url;
    }

    private async void CancelButton_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void SaveButton_Clicked(object? sender, EventArgs e)
    {
        var websiteName = WebsiteNameEntry.Text?.Trim();
        var url = UrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(websiteName))
        {
            await DisplayAlertAsync(
                "Website Name Required",
                "Enter a website name.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            await DisplayAlertAsync(
                "URL Required",
                "Enter a website URL.",
                "OK");

            return;
        }

        _website.Name = websiteName;
        _website.Url = url;

        WebsiteUpdated?.Invoke(_website);

        await Navigation.PopModalAsync();
    }
}