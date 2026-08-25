using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class EditCategoryPage : ContentPage
{
    private readonly WebsiteCategory _category;
    private string _selectedTextColor;

    public event Action<WebsiteCategory>? CategoryUpdated;

    public EditCategoryPage(WebsiteCategory category)
    {
        InitializeComponent();

        _category = category;
        _selectedTextColor = category.TextColor;

        CategoryNameEntry.Text = category.Name;
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