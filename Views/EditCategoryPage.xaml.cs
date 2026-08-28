using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class EditCategoryPage : ContentPage
{
    private readonly WebsiteCategory _category;
    private string _selectedTextColor;

    public event Action<WebsiteCategory>? CategoryUpdated;

    public event Action<WebsiteCategory>? AddWebsiteRequested;

    public EditCategoryPage(
        WebsiteCategory category)
    {
        InitializeComponent();

        _category = category;
        _selectedTextColor = category.TextColor;

        CategoryNameEntry.Text = category.Name;

        HighlightCurrentColor();
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
            button.BorderColor =
                App.Current?.RequestedTheme == AppTheme.Dark
                    ? Colors.Gray
                    : Colors.DarkGray;

            button.BorderWidth = 1;
            button.Scale = 1.0;
        }

        selectedButton.BorderColor =
            App.Current?.RequestedTheme == AppTheme.Dark
                ? Colors.White
                : Colors.Black;

        selectedButton.BorderWidth = 3;
        selectedButton.Scale = 1.10;
    }

    private void HighlightCurrentColor()
    {
        switch (_selectedTextColor.ToUpperInvariant())
        {
            case "#000000":
                SelectColor(
                    BlackColorButton,
                    "#000000");
                break;

            case "#FFFFFF":
                SelectColor(
                    WhiteColorButton,
                    "#FFFFFF");
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
        }
    }

    private void BlackColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#000000");
    }

    private void WhiteColor_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
            SelectColor(button, "#FFFFFF");
    }

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

    // ---------------------------------------------------------
    // ADD WEBSITE
    // ---------------------------------------------------------

    private void AddWebsiteButton_Clicked(
        object? sender,
        EventArgs e)
    {
        AddWebsiteRequested?.Invoke(_category);
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
        var categoryName =
            CategoryNameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            await DisplayAlertAsync(
                "Category Name Required",
                "Enter a category name.",
                "OK");

            return;
        }

        _category.Name = categoryName;
        _category.TextColor = _selectedTextColor;

        CategoryUpdated?.Invoke(_category);

        await Navigation.PopModalAsync();
    }
}