using EasyWebsiteManager.Models;
using EasyWebsiteManager.Services;

namespace EasyWebsiteManager.Views;

public partial class CategoryView : ContentView
{
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
        if (Window?.Page is Page page)
        {
            await DialogService.ShowMessageAsync(
    page,
    "Edit Website",
    "The edit window is our next feature.");
        }
    }
}