using System.Linq;
using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class EditWebsitePage : ContentPage
{
    private readonly WebsiteItem _website;
    private string _selectedTextColor;

    public event Action<WebsiteItem>? WebsiteUpdated;

    public EditWebsitePage(
        WebsiteItem website)
    {
        InitializeComponent();

        _website = website;
        _selectedTextColor = website.TextColor;

        WebsiteNameEntry.Text = website.Name;
        UrlEntry.Text = website.Url;

        HighlightCurrentColor();
    }

    private bool IsDarkTheme =>
        App.Current?.RequestedTheme == AppTheme.Dark;

    private void SelectThemeDefaultColor()
    {
        if (IsDarkTheme)
            SelectColor(WhiteColorButton, "#FFFFFF");
        else
            SelectColor(BlackColorButton, "#000000");
    }

    private void SelectColor(
        Button selectedButton,
        string color)
    {
        _selectedTextColor = color;

        var colorButtons = new[]
        {
            BlackColorButton,
            WhiteColorButton,
            BlueColorButton,
            RedColorButton,
            GreenColorButton,
            OrangeColorButton,
            PurpleColorButton,
            VioletColorButton,
            LightGreyColorButton,
            MediumGreyColorButton,
            TealColorButton,
            YellowColorButton
        };

        foreach (var button in colorButtons)
        {
            // The checkmark, not the border, indicates selection.
            button.Text = string.Empty;
            button.BorderColor = IsDarkTheme ? Colors.Gray : Colors.DarkGray;
            button.BorderWidth = 1;
            button.Scale = 1.0;
        }

        // Contrast is based on the swatch, not on the application theme.
        // White is legible on dark swatches; black on lighter swatches.
        selectedButton.Text = "✓";
        selectedButton.TextColor = color switch
        {
            "#000000" or "#0000CD" or "#FF0000" or "#4B0082"
                or "#8F00FF" or "#008080" => Colors.White,
            _ => Colors.Black
        };
        selectedButton.FontAttributes = FontAttributes.Bold;
        selectedButton.FontSize = 20;
        selectedButton.BorderWidth = 1;
        selectedButton.Scale = 1.0;
    }

    private void HighlightCurrentColor()
    {
        var color =
            _selectedTextColor?.ToUpperInvariant()
            ?? "#000000";

        switch (color)
        {
            /*
             * Existing black and white values are both
             * treated as the theme-aware Default.
             */
            case "#000000":
            case "#FFFFFF":
                SelectThemeDefaultColor();
                break;

            case "#0000CD":
                SelectColor(
                    BlueColorButton,
                    "#0000CD");
                break;

            case "#FF0000":
                SelectColor(
                    RedColorButton,
                    "#FF0000");
                break;

            case "#00FF00":
                SelectColor(
                    GreenColorButton,
                    "#00FF00");
                break;

            case "#FFA500":
                SelectColor(
                    OrangeColorButton,
                    "#FFA500");
                break;

            case "#4B0082":
                SelectColor(
                    PurpleColorButton,
                    "#4B0082");
                break;

            case "#8F00FF":
                SelectColor(
                    VioletColorButton,
                    "#8F00FF");
                break;

            case "#D3D3D3":
                SelectColor(
                    LightGreyColorButton,
                    "#D3D3D3");
                break;

            case "#808080":
                SelectColor(
                    MediumGreyColorButton,
                    "#808080");
                break;

            case "#008080":
                SelectColor(
                    TealColorButton,
                    "#008080");
                break;

            case "#FFD700":
                SelectColor(
                    YellowColorButton,
                    "#FFD700");
                break;

            default:
                SelectThemeDefaultColor();
                break;
        }
    }

    // ---------------------------------------------------------
    // DEFAULT TEXT COLOR
    // ---------------------------------------------------------

    private void BlackColor_Clicked(object? sender, EventArgs e)
    {
        SelectThemeDefaultColor();
    }

    private void WhiteColor_Clicked(object? sender, EventArgs e)
    {
        SelectThemeDefaultColor();
    }

    // ---------------------------------------------------------
    // FIXED COLORS
    // ---------------------------------------------------------

    private void BlueColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#0000CD");
    }

    private void RedColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#FF0000");
    }

    private void GreenColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#00FF00");
    }

    private void OrangeColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#FFA500");
    }

    private void PurpleColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#4B0082");
    }

    private void VioletColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#8F00FF");
    }

    private void LightGreyColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#D3D3D3");
    }

    private void MediumGreyColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#808080");
    }

    private void TealColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#008080");
    }

    private void YellowColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#FFD700");
    }

    private static bool WebsiteUrlIsValid(string address)
    {
        var candidate = address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        address.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? address
            : "https://" + address;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return false;

        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host) || !host.Contains('.') ||
            host.StartsWith('.') || host.EndsWith('.'))
            return false;

        var labels = host.Split('.');
        return labels.All(label => label.Length > 0 && label.Length <= 63 &&
            label[0] != '-' && label[^1] != '-' &&
            label.All(c => char.IsLetterOrDigit(c) || c == '-'));
    }

    // ---------------------------------------------------------
    // CANCEL
    // ---------------------------------------------------------

    private async void CancelButton_Clicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    // ---------------------------------------------------------
    // SAVE
    // ---------------------------------------------------------

    private async void SaveButton_Clicked(
        object? sender,
        EventArgs e)
    {
        var websiteName =
            WebsiteNameEntry.Text?.Trim();

        var url =
            UrlEntry.Text?.Trim();

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

        if (!WebsiteUrlIsValid(url))
        {
            await DisplayAlertAsync(
                "Invalid Website Address",
                "The website address is invalid. Please correct the URL and try again.",
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