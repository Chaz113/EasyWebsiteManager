using EasyWebsiteManager.Models;
using System.Collections.ObjectModel;

namespace EasyWebsiteManager.Views;

public partial class AddWebsitePage : ContentPage
{
    public ObservableCollection<WebsiteCategory> Categories { get; }

    public event Action<string, string, string>? WebsiteSaved;

    public AddWebsitePage(ObservableCollection<WebsiteCategory> categories)
    {
        InitializeComponent();

        Categories = categories;
        BindingContext = this;
    }

    private async void CancelButton_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void SaveButton_Clicked(object? sender, EventArgs e)
    {
        var selectedCategory = CategoryPicker.SelectedItem as WebsiteCategory;
        var newCategoryName = NewCategoryEntry.Text?.Trim();
        var websiteName = WebsiteNameEntry.Text?.Trim();
        var url = UrlEntry.Text?.Trim();

        if (selectedCategory == null &&
            string.IsNullOrWhiteSpace(newCategoryName))
        {
            await DisplayAlertAsync(
                "Category Required",
                "Select an existing category or enter a new category.",
                "OK");

            return;
        }

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

        var categoryName =
            selectedCategory?.Name ?? newCategoryName!;

        WebsiteSaved?.Invoke(
            categoryName,
            websiteName,
            url);

        await Navigation.PopModalAsync();
    }
    private void NewCategoryCheckBox_CheckedChanged(
    object? sender,
    CheckedChangedEventArgs e)
    {
        CategoryPicker.IsVisible = !e.Value;
        NewCategoryEntry.IsVisible = e.Value;

        if (e.Value)
        {
            CategoryPicker.SelectedItem = null;
            NewCategoryEntry.Focus();
        }
        else
        {
            NewCategoryEntry.Text = string.Empty;
        }
    }
}