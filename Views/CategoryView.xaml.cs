using EasyWebsiteManager.Models;

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
}