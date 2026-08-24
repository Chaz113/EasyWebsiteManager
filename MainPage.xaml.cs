using EasyWebsiteManager.Models;
using EasyWebsiteManager.Services;
using EasyWebsiteManager.ViewModels;
using EasyWebsiteManager.Views;

namespace EasyWebsiteManager;

public partial class MainPage : ContentPage
{
    private WebsiteCategory? _deletedCategory;
    private WebsiteItem? _deletedWebsite;
    private CancellationTokenSource? _undoCancellation;
    private async void CategoryView_WebsiteNoteChanged(
    WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

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
        }
    }

    private async Task SaveCurrentDataAsync()
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var data = vm.CreateAppData();

        await StorageService.SaveAsync(data);
    }
    private async void CategoryView_WebsiteUpdated(
    WebsiteItem website)
    {
        await SaveCurrentDataAsync();
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
        if (e.Element is CategoryView categoryView &&
     BindingContext is MainViewModel vm)
        {
            categoryView.Settings = vm.Settings;

            categoryView.WebsiteDeleted -= CategoryView_WebsiteDeleted;
            categoryView.WebsiteDeleted += CategoryView_WebsiteDeleted;

            categoryView.WebsiteNoteChanged -= CategoryView_WebsiteNoteChanged;
            categoryView.WebsiteNoteChanged += CategoryView_WebsiteNoteChanged;
            categoryView.WebsiteUpdated -= CategoryView_WebsiteUpdated;
            categoryView.WebsiteUpdated += CategoryView_WebsiteUpdated;
        }
    }

    private async void CategoryView_WebsiteDeleted(
        WebsiteCategory category,
        WebsiteItem website)
    {
        _deletedCategory = category;
        _deletedWebsite = website;

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

    private async void UndoDelete_Clicked(
        object? sender,
        EventArgs e)
    {
        if (_deletedCategory == null ||
            _deletedWebsite == null)
        {
            return;
        }

        _undoCancellation?.Cancel();

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