using EasyWebsiteManager.Models;
using EasyWebsiteManager.Services;
using EasyWebsiteManager.ViewModels;
using EasyWebsiteManager.Views;

namespace EasyWebsiteManager;

public partial class MainPage : ContentPage
{
    private WebsiteCategory? _deletedCategory;
    private WebsiteItem? _deletedWebsite;
    private WebsiteCategory? _deletedWholeCategory;

    private CancellationTokenSource? _undoCancellation;
    private bool _dataLoaded;

    public MainPage()
    {
        InitializeComponent();

        var vm = new MainViewModel();
        BindingContext = vm;

        CategoryListView.ChildAdded += CategoryListView_ChildAdded;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_dataLoaded)
            return;

        _dataLoaded = true;

        if (BindingContext is not MainViewModel vm)
            return;

        var data = await StorageService.LoadAsync();

        if (data != null)
        {
            vm.LoadAppData(data);

            // Persist any one-time migrations immediately.
            await SaveCurrentDataAsync();
        }
    }

    private async Task SaveCurrentDataAsync()
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var data = vm.CreateAppData();

        await StorageService.SaveAsync(data);
    }

    private async void AddButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var addPage = new AddWebsitePage(vm.Categories);

        addPage.WebsiteSaved += async (category, website, url) =>
        {
            vm.AddWebsite(category, website, url);

            await SaveCurrentDataAsync();
        };

        await Navigation.PushModalAsync(addPage);
    }

    private async void SettingsButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var settingsPage = new SettingsPage(vm.Settings);

        settingsPage.SettingsChanged += async () =>
        {
            await SaveCurrentDataAsync();
        };

        await Navigation.PushModalAsync(settingsPage);
    }

    private void CategoryListView_ChildAdded(
        object? sender,
        ElementEventArgs e)
    {
        if (e.Element is not CategoryView categoryView)
            return;

        if (BindingContext is not MainViewModel vm)
            return;

        categoryView.Settings = vm.Settings;

        categoryView.WebsiteDeleted -= CategoryView_WebsiteDeleted;
        categoryView.WebsiteDeleted += CategoryView_WebsiteDeleted;

        categoryView.WebsiteNoteChanged -= CategoryView_WebsiteNoteChanged;
        categoryView.WebsiteNoteChanged += CategoryView_WebsiteNoteChanged;

        categoryView.WebsiteUpdated -= CategoryView_WebsiteUpdated;
        categoryView.WebsiteUpdated += CategoryView_WebsiteUpdated;

        categoryView.CategoryEditRequested -= CategoryView_CategoryEditRequested;
        categoryView.CategoryEditRequested += CategoryView_CategoryEditRequested;

        categoryView.CategoryDeleteRequested -= CategoryView_CategoryDeleteRequested;
        categoryView.CategoryDeleteRequested += CategoryView_CategoryDeleteRequested;
    }

    private async void CategoryView_WebsiteDeleted(
        WebsiteCategory category,
        WebsiteItem website)
    {
        _deletedCategory = category;
        _deletedWebsite = website;
        _deletedWholeCategory = null;

        await SaveCurrentDataAsync();

        UndoMessageLabel.Text = $"{website.Name} deleted";
        UndoBanner.IsVisible = true;

        _undoCancellation?.Cancel();
        _undoCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(5),
                _undoCancellation.Token);

            UndoBanner.IsVisible = false;

            _deletedCategory = null;
            _deletedWebsite = null;
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void CategoryView_WebsiteNoteChanged(
        WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

    private async void CategoryView_WebsiteUpdated(
        WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

    private async void CategoryView_CategoryEditRequested(
       WebsiteCategory category)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var editPage = new EditCategoryPage(category);

        editPage.CategoryUpdated += async updatedCategory =>
        {
            vm.ResortCategories();

            await SaveCurrentDataAsync();
        };

        await Navigation.PushModalAsync(editPage);
    }
    private void HomeButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        vm.SearchText = string.Empty;
    }
    private async void CategoryView_CategoryDeleteRequested(
        WebsiteCategory category)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        if (vm.Settings.ConfirmDelete)
        {
            if (Window?.Page is not Page page)
                return;

            var confirmed = await DialogService.ConfirmAsync(
                page,
                "Delete Category",
                $"Delete {category.Name} and all websites inside it?");

            if (!confirmed)
                return;
        }

        if (!vm.DeleteCategory(category))
            return;

        _deletedWholeCategory = category;

        _deletedCategory = null;
        _deletedWebsite = null;

        await SaveCurrentDataAsync();

        UndoMessageLabel.Text =
            $"{category.Name} category deleted";

        UndoBanner.IsVisible = true;

        _undoCancellation?.Cancel();
        _undoCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(5),
                _undoCancellation.Token);

            UndoBanner.IsVisible = false;
            _deletedWholeCategory = null;
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void UndoDelete_Clicked(
        object? sender,
        EventArgs e)
    {
        _undoCancellation?.Cancel();

        if (_deletedWholeCategory != null)
        {
            if (BindingContext is MainViewModel vm)
            {
                vm.RestoreCategory(_deletedWholeCategory);

                await SaveCurrentDataAsync();
            }

            _deletedWholeCategory = null;
            UndoBanner.IsVisible = false;

            return;
        }

        if (_deletedCategory == null ||
            _deletedWebsite == null)
        {
            return;
        }

        _deletedCategory.Websites.Add(_deletedWebsite);

        var sorted = _deletedCategory.Websites
            .OrderBy(w => w.Name)
            .ToList();

        _deletedCategory.Websites.Clear();

        foreach (var website in sorted)
        {
            _deletedCategory.Websites.Add(website);
        }

        await SaveCurrentDataAsync();

        UndoBanner.IsVisible = false;

        _deletedCategory = null;
        _deletedWebsite = null;
    }
}