using EasyWebsiteManager.Models;
using EasyWebsiteManager.Services;
using System.Linq;

namespace EasyWebsiteManager.Views;

public partial class CategoryView : ContentView
{
    public static readonly BindableProperty SettingsProperty =
        BindableProperty.Create(
            nameof(Settings),
            typeof(AppSettings),
            typeof(CategoryView));

    public AppSettings? Settings
    {
        get => (AppSettings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public event Action<WebsiteCategory, WebsiteItem>? WebsiteDeleted;
    public event Action<WebsiteItem>? WebsiteNoteChanged;
    public event Action<WebsiteItem>? WebsiteUpdated;

    public event Action<WebsiteCategory>? CategoryEditRequested;
    public event Action<WebsiteCategory>? CategoryDeleteRequested;

    private bool _websiteDeleteInProgress;

    public CategoryView()
    {
        InitializeComponent();

#if WINDOWS
        // On Windows, the header-level gesture can also react to clicks on
        // its child ImageButtons. Limit expand/collapse to the name and chevron.
        // Android and other platforms retain their original header gesture.
        CategoryHeaderGrid.GestureRecognizers.Clear();
        CategoryNameLabel.GestureRecognizers.Add(
            new TapGestureRecognizer { Command = new Command(ToggleCategoryExpansion) });
        CategoryChevron.GestureRecognizers.Add(
            new TapGestureRecognizer { Command = new Command(ToggleCategoryExpansion) });
#endif
    }

#if WINDOWS
    private void ToggleCategoryExpansion()
    {
        if (BindingContext is WebsiteCategory category)
            category.IsExpanded = !category.IsExpanded;
    }
#endif

    // ---------------------------------------------------------
    // TAP / CLICK FEEDBACK
    // ---------------------------------------------------------

    private static async Task ShowTapFeedback(VisualElement element)
    {
        var originalOpacity = element.Opacity;
        var originalScale = element.Scale;

        try
        {
            element.Opacity = 0.60;
            element.Scale = 0.94;

            await Task.Delay(90);
        }
        finally
        {
            element.Opacity = originalOpacity;
            element.Scale = originalScale;
        }
    }

    // ---------------------------------------------------------
    // CATEGORY EXPAND / COLLAPSE
    // ---------------------------------------------------------

    private void CategoryTapped(
        object? sender,
        TappedEventArgs e)
    {
        if (BindingContext is WebsiteCategory category)
        {
            category.IsExpanded = !category.IsExpanded;
        }
    }

    // ---------------------------------------------------------
    // WEBSITE OPEN
    // ---------------------------------------------------------

    private async void WebsiteName_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        if (sender is not Label label)
            return;

        if (label.BindingContext is not WebsiteItem website)
            return;

        if (string.IsNullOrWhiteSpace(website.Url))
            return;

        var originalOpacity = label.Opacity;
        var originalScale = label.Scale;

        try
        {
            // Brief feedback that the website click was registered.
            label.Opacity = 0.55;
            label.Scale = 0.98;

            await Task.Delay(120);

            var url = website.Url.Trim();

            if (!url.StartsWith(
                    "http://",
                    StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith(
                    "https://",
                    StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            if (!WebsiteUrlIsValid(url))
            {
                await ShowInvalidWebsiteAddressAsync();
                return;
            }

            try
            {
                await Browser.Default.OpenAsync(
                    url,
                    BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception)
            {
                await ShowInvalidWebsiteAddressAsync();
            }
        }
        finally
        {
            label.Opacity = originalOpacity;
            label.Scale = originalScale;
        }
    }

    private static bool WebsiteUrlIsValid(string address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) ||
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

    private async Task ShowInvalidWebsiteAddressAsync()
    {
        if (Window?.Page is Page page)
        {
            await page.DisplayAlertAsync(
                "Unable to Open Website",
                "The website address is invalid or could not be opened. Please edit the URL and try again.",
                "OK");
        }
    }

    // ---------------------------------------------------------
    // EDIT WEBSITE
    // ---------------------------------------------------------


    private async void EditWebsite_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not ImageButton button)
            return;

        await ShowTapFeedback(button);

        if (button.BindingContext is not WebsiteItem website)
            return;

        if (BindingContext is not WebsiteCategory)
            return;

        var editPage = new EditWebsitePage(website);

        editPage.WebsiteUpdated += updatedWebsite =>
        {
            // Do not clear or rebuild category.Websites.
            // That operation caused the Android disposed-button crash.
            WebsiteUpdated?.Invoke(updatedWebsite);
        };

        await Navigation.PushModalAsync(editPage);
    }


    // ---------------------------------------------------------
    // NOTEPAD
    // ---------------------------------------------------------

    private async void NotepadButton_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not ImageButton button)
            return;

        await ShowTapFeedback(button);

        if (button.BindingContext is not WebsiteItem website)
            return;

        var notepadPage = new NotepadPage(website);

        notepadPage.NoteSaved += updatedWebsite =>
        {
            WebsiteNoteChanged?.Invoke(updatedWebsite);
        };

        await Navigation.PushModalAsync(notepadPage);
    }

    // ---------------------------------------------------------
    // DELETE WEBSITE
    // ---------------------------------------------------------

    private async void DeleteWebsite_Clicked(object? sender, EventArgs e)
    {
        if (_websiteDeleteInProgress || sender is not ImageButton button)
            return;

        _websiteDeleteInProgress = true;
        WebsiteItem? website = button.BindingContext as WebsiteItem;
        WebsiteCategory? category = BindingContext as WebsiteCategory;
        bool removed = false;

        try
        {
            if (website is null || category is null)
                return;

            button.IsEnabled = false;
            if (!OperatingSystem.IsAndroid())
                await ShowTapFeedback(button);

            if (Settings?.ConfirmDelete ?? true)
            {
                if (Window?.Page is not Page page)
                    return;

                if (!await DialogService.ConfirmAsync(
                        page, "Delete Website", $"Delete {website.Name}?"))
                    return;
            }

            try
            {
                removed = category.Websites.Remove(website);
            }
            catch (ObjectDisposedException ex) when (OperatingSystem.IsAndroid())
            {
                // MAUI BindableLayout can throw while disconnecting a removed
                // Android MaterialButton. ObservableCollection may already have
                // completed the removal before the UI exception propagates.
                System.Diagnostics.Debug.WriteLine(
                    $"Android website removal UI cleanup exception: {ex}");
                removed = !category.Websites.Contains(website);
                if (!removed)
                    throw;
            }

            if (removed)
                WebsiteDeleted?.Invoke(category, website);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Website deletion failed: {ex}");
            if (Window?.Page is Page page)
                await page.DisplayAlertAsync(
                    "Delete Failed",
                    "The website could not be deleted. Please try again.",
                    "OK");
        }
        finally
        {
            // Do not touch a button that was removed from the Android layout.
            // The surviving controls are restored only if deletion was canceled.
            if (!removed)
            {
                try { button.IsEnabled = true; }
                catch (ObjectDisposedException) { }
            }
            _websiteDeleteInProgress = false;
        }
    }

    // ---------------------------------------------------------
    // EDIT CATEGORY
    // ---------------------------------------------------------

    private async void EditCategory_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is ImageButton button)
        {
            await ShowTapFeedback(button);
        }

        if (BindingContext is not WebsiteCategory category)
            return;

        CategoryEditRequested?.Invoke(category);
    }

    // ---------------------------------------------------------
    // DELETE CATEGORY
    // ---------------------------------------------------------

    private async void DeleteCategory_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is ImageButton button)
        {
            await ShowTapFeedback(button);
        }

        if (BindingContext is not WebsiteCategory category)
            return;

        /*
         * MainPage handles category deletion,
         * including confirmation, persistence and Undo.
         */
        CategoryDeleteRequested?.Invoke(category);
    }
}