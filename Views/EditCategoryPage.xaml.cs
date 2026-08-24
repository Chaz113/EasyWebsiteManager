using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class EditCategoryPage : ContentPage
{
    private readonly WebsiteCategory _category;

    public event Action<WebsiteCategory>? CategoryUpdated;

    public EditCategoryPage(WebsiteCategory category)
    {
        InitializeComponent();

        _category = category;

        CategoryNameEntry.Text = category.Name;
    }

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

        CategoryUpdated?.Invoke(_category);

        await Navigation.PopModalAsync();
    }
}