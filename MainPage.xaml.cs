using EasyWebsiteManager.Models;
using EasyWebsiteManager.ViewModels;
using EasyWebsiteManager.Views;

namespace EasyWebsiteManager;

public partial class MainPage : ContentPage
{
    private WebsiteCategory? _deletedCategory;
    private WebsiteItem? _deletedWebsite;
    private CancellationTokenSource? _undoCancellation;

    public MainPage()
    {
        InitializeComponent();

        var vm = new MainViewModel();
        BindingContext = vm;

        CategoryListView.ChildAdded += CategoryListView_ChildAdded;
    }

    private async void AddButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var addPage = new AddWebsitePage(vm.Categories);

        addPage.WebsiteSaved += (category, website, url) =>
        {
            vm.AddWebsite(category, website, url);
        };

        await Navigation.PushModalAsync(addPage);
    }

    private void CategoryListView_ChildAdded(object? sender, ElementEventArgs e)
    {
        if (e.Element is CategoryView categoryView)
        {
            categoryView.WebsiteDeleted -= CategoryView_WebsiteDeleted;
            categoryView.WebsiteDeleted += CategoryView_WebsiteDeleted;
        }
    }

    private async void CategoryView_WebsiteDeleted(
        WebsiteCategory category,
        WebsiteItem website)
    {
        _deletedCategory = category;
        _deletedWebsite = website;

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

    private void UndoDelete_Clicked(object? sender, EventArgs e)
    {
        if (_deletedCategory == null || _deletedWebsite == null)
            return;

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

        UndoBanner.IsVisible = false;

        _deletedCategory = null;
        _deletedWebsite = null;
    }
}