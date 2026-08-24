using EasyWebsiteManager.Models;
using System.Linq;
using EasyWebsiteManager.Services;

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