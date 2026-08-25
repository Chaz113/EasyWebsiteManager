using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class EditWebsitePage : ContentPage
{
    private readonly WebsiteItem _website;
    private string _selectedTextColor;

    public event Action<WebsiteItem>? WebsiteUpdated;

    public EditWebsitePage(WebsiteItem website)
    {
        InitializeComponent();

        _website = website;
        _selectedTextColor = website.TextColor;

        WebsiteNameEntry.Text = website.Name;
        UrlEntry.Text = website.Url;
    }

    private void BlackColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#000000";

    private void WhiteColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#FFFFFF";

    private void BlueColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#0000CD";

    private void RedColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#FF0000";

    private void GreenColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#00FF00";

    private void OrangeColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#FFA500";

    private void PurpleColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#4B0082";

    private void VioletColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#8F00FF";

    private void LightGreyColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#D3D3D3";

    private void MediumGreyColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#808080";

    private void TealColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#008080";

    private void YellowColor_Clicked(object? sender, EventArgs e)
        => _selectedTextColor = "#FFD700";

    private async void CancelButton_Clicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void SaveButton_Clicked(
        object? sender,
        EventArgs e)
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
        _website.TextColor = _selectedTextColor;

        WebsiteUpdated?.Invoke(_website);

        await Navigation.PopModalAsync();
    }
}