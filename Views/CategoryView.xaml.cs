using EasyWebsiteManager.Models;
using System.Linq;
using EasyWebsiteManager.Services;

namespace EasyWebsiteManager.Views;

public partial class CategoryView : ContentView
{
    private void EditCategory_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is not WebsiteCategory category)
            return;

        CategoryEditRequested?.Invoke(category);
    }

    private void DeleteCategory_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is not WebsiteCategory category)
            return;

        CategoryDeleteRequested?.Invoke(category);
    }
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
    private async void WebsiteName_Tapped(object? sender, TappedEventArgs e)
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
            // Immediate visual feedback
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

            await Browser.Default.OpenAsync(
                url,
                BrowserLaunchMode.SystemPreferred);
        }
        finally
        {
            // Restore the user's normal website appearance
            label.Opacity = originalOpacity;
            label.Scale = originalScale;
        }
    }

    private async void NotepadButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not WebsiteItem website)
            return;

        var notepadPage = new NotepadPage(website);

        notepadPage.NoteSaved += updatedWebsite =>
        {
            WebsiteNoteChanged?.Invoke(updatedWebsite);
        };

        await Navigation.PushModalAsync(notepadPage);
    }


    private async void DeleteWebsite_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not WebsiteItem website)
            return;

        if (BindingContext is not WebsiteCategory category)
            return;

        bool confirmDelete = Settings?.ConfirmDelete ?? true;

        if (confirmDelete)
        {
            if (Window?.Page is not Page page)
                return;

            var confirmed = await DialogService.ConfirmAsync(
                page,
                "Delete Website",
                $"Delete {website.Name}?");

            if (!confirmed)
                return;
        }

        if (category.Websites.Remove(website))
        {
            WebsiteDeleted?.Invoke(category, website);
        }
    }

    public CategoryView()
    {
        InitializeComponent();
    }

    private void CategoryTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is WebsiteCategory category)
        {
            category.IsExpanded = !category.IsExpanded;
        }
    }
    private async void EditWebsite_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not WebsiteItem website)
            return;

        if (BindingContext is not WebsiteCategory category)
            return;

        var editPage = new EditWebsitePage(website);

        editPage.WebsiteUpdated += updatedWebsite =>
        {
            var sortedWebsites = category.Websites
                .OrderBy(w => w.Name)
                .ToList();

            category.Websites.Clear();

            foreach (var item in sortedWebsites)
            {
                category.Websites.Add(item);
            }

            WebsiteUpdated?.Invoke(updatedWebsite);
        };

        await Navigation.PushModalAsync(editPage);
    }
   
    
}