using EasyWebsiteManager.Models;
using System.Collections.ObjectModel;

namespace EasyWebsiteManager.Views;

public partial class AddWebsitePage : ContentPage
{
    private bool _updatingCategorySelection;

    private readonly WebsiteCategory? _fixedCategory;

    public ObservableCollection<WebsiteCategory> Categories { get; }

    public event Action<string, string, string>? WebsiteSaved;

    // ---------------------------------------------------------
    // NORMAL + BUTTON
    // ---------------------------------------------------------

    public AddWebsitePage(
        ObservableCollection<WebsiteCategory> categories)
        : this(
            categories,
            null)
    {
    }

    // ---------------------------------------------------------
    // FIXED CATEGORY
    // Used when Add Website is opened from Edit Category.
    // ---------------------------------------------------------

    public AddWebsitePage(
        ObservableCollection<WebsiteCategory> categories,
        WebsiteCategory? fixedCategory)
    {
        InitializeComponent();

        Categories = categories;
        _fixedCategory = fixedCategory;

        BindingContext = this;

        if (_fixedCategory != null)
        {
            CategorySelectionSection.IsVisible = false;
            FixedCategorySection.IsVisible = true;

            FixedCategoryNameLabel.Text =
                _fixedCategory.Name;
        }
        else
        {
            CategorySelectionSection.IsVisible = true;
            FixedCategorySection.IsVisible = false;
        }
    }

    // ---------------------------------------------------------
    // NEW CATEGORY
    // ---------------------------------------------------------

    private void NewCategoryEntry_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (_updatingCategorySelection)
            return;

        if (_fixedCategory != null)
            return;

        if (string.IsNullOrWhiteSpace(e.NewTextValue))
            return;

        _updatingCategorySelection = true;

        CategoryPicker.SelectedItem = null;

        _updatingCategorySelection = false;
    }

    // ---------------------------------------------------------
    // EXISTING CATEGORY
    // ---------------------------------------------------------

    private void CategoryPicker_SelectedIndexChanged(
        object? sender,
        EventArgs e)
    {
        if (_updatingCategorySelection)
            return;

        if (_fixedCategory != null)
            return;

        if (CategoryPicker.SelectedItem == null)
            return;

        if (string.IsNullOrWhiteSpace(NewCategoryEntry.Text))
            return;

        _updatingCategorySelection = true;

        NewCategoryEntry.Text =
            string.Empty;

        _updatingCategorySelection = false;
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
        WebsiteCategory? selectedCategory = null;

        string? newCategoryName = null;

        if (_fixedCategory == null)
        {
            selectedCategory =
                CategoryPicker.SelectedItem
                    as WebsiteCategory;

            newCategoryName =
                NewCategoryEntry.Text?.Trim();
        }

        var websiteName =
            WebsiteNameEntry.Text?.Trim();

        var url =
            UrlEntry.Text?.Trim();

        if (_fixedCategory == null &&
            selectedCategory == null &&
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

        string categoryName;

        if (_fixedCategory != null)
        {
            categoryName =
                _fixedCategory.Name;
        }
        else if (!string.IsNullOrWhiteSpace(newCategoryName))
        {
            categoryName =
                newCategoryName;
        }
        else
        {
            categoryName =
                selectedCategory!.Name;
        }

        WebsiteSaved?.Invoke(
            categoryName,
            websiteName,
            url);

        await Navigation.PopModalAsync();
    }
}