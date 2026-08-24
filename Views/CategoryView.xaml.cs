using EasyWebsiteManager.Models;
using System.Linq;

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
        };

        await Navigation.PushModalAsync(editPage);
    }
}