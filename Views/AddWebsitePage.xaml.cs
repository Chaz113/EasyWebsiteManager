using EasyWebsiteManager.Models;
using System.Collections.ObjectModel;

namespace EasyWebsiteManager.Views;

public partial class AddWebsitePage : ContentPage
{
    private bool _updatingCategorySelection;

    public ObservableCollection<WebsiteCategory> Categories { get; }

    public event Action<string, string, string>? WebsiteSaved;

    public AddWebsitePage(
        ObservableCollection<WebsiteCategory> categories)
    {
        InitializeComponent();

        Categories = categories;
        BindingContext = this;
    }

    private void NewCategoryEntry_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (_updatingCategorySelection)
            return;

        if (string.IsNullOrWhiteSpace(e.NewTextValue))
            return;

        _updatingCategorySelection = true;

        CategoryPicker.SelectedItem = null;

        _updatingCategorySelection = false;
    }

    private void CategoryPicker_SelectedIndexChanged(
        object? sender,
        EventArgs e)
    {
        if (_updatingCategorySelection)
            return;

        if (CategoryPicker.SelectedItem == null)
            return;

        if (string.IsNullOrWhiteSpace(NewCategoryEntry.Text))
            return;

        _updatingCategorySelection = true;

        NewCategoryEntry.Text = string.Empty;

        _updatingCategorySelection = false;
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
        var selectedCategory =
            CategoryPicker.SelectedItem as WebsiteCategory;

        var newCategoryName =
            NewCategoryEntry.Text?.Trim();

        var websiteName =
            WebsiteNameEntry.Text?.Trim();

        var url =
            UrlEntry.Text?.Trim();

        if (selectedCategory == null &&
            string.IsNullOrWhiteSpace(newCategoryName))
        {
            await DisplayAlertAsync(
                "Category Required",
                "Select an existing category or enter a new category name.",
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
            !string.IsNullOrWhiteSpace(newCategoryName)
                ? newCategoryName
                : selectedCategory!.Name;

        WebsiteSaved?.Invoke(
            categoryName,
            websiteName,
            url);

        await Navigation.PopModalAsync();
    }
}