using EasyWebsiteManager.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace EasyWebsiteManager.Views;

public partial class AddWebsitePage : ContentPage
{
    private bool _updatingCategorySelection;
    private WebsiteCategory? _selectedCategory;

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

    private static Color GetPlaceholderColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#858585")
            : Color.FromArgb("#C4C4C4");

    private static Color GetSelectedTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#F0F0F0")
            : Color.FromArgb("#222222");

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

        _selectedCategory = null;
        SelectedCategoryLabel.Text = "Select";
        SelectedCategoryLabel.TextColor = GetPlaceholderColor();
        CategoryDropdownList.IsVisible = false;

        _updatingCategorySelection = false;
    }

    // ---------------------------------------------------------
    // EXISTING CATEGORY
    // ---------------------------------------------------------

    private void CategoryDropdown_Tapped(object? sender, TappedEventArgs e)
    {
        if (_fixedCategory != null)
            return;

        CategoryDropdownList.IsVisible = !CategoryDropdownList.IsVisible;
    }

    private void CategoryOption_Tapped(object? sender, TappedEventArgs e)
    {
        if (_updatingCategorySelection || _fixedCategory != null)
            return;

        if (sender is not BindableObject option ||
            option.BindingContext is not WebsiteCategory category)
            return;

        _updatingCategorySelection = true;
        try
        {
            _selectedCategory = category;
            SelectedCategoryLabel.Text = category.Name;
            SelectedCategoryLabel.TextColor = GetSelectedTextColor();
            NewCategoryEntry.Text = string.Empty;
            CategoryDropdownList.IsVisible = false;
        }
        finally
        {
            _updatingCategorySelection = false;
        }
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
        WebsiteCategory? selectedCategory = null;

        string? newCategoryName = null;

        if (_fixedCategory == null)
        {
            selectedCategory = _selectedCategory;

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

        if (!WebsiteUrlIsValid(url))
        {
            await DisplayAlertAsync(
                "Invalid Website Address",
                "The website address is invalid. Please correct the URL and try again.",
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