using System.Collections.ObjectModel;
using System.Linq;
using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.ViewModels;

public class MainViewModel
{
    public ObservableCollection<WebsiteCategory> Categories { get; } = new();

    public MainViewModel()
    {
        Categories.Add(new WebsiteCategory { Name = "Retail" });
        Categories.Add(new WebsiteCategory { Name = "Weather" });
        Categories.Add(new WebsiteCategory { Name = "Videos to Watch" });
    }
    public void AddWebsite(string categoryName, string websiteName, string url)
    {
        var category = Categories.FirstOrDefault(c =>
            c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (category == null)
        {
            category = new WebsiteCategory
            {
                Name = categoryName
            };

            Categories.Add(category);

            var sorted = Categories.OrderBy(c => c.Name).ToList();

            Categories.Clear();

            foreach (var item in sorted)
                Categories.Add(item);
        }

        category.Websites.Add(new WebsiteItem
        {
            Name = websiteName,
            Url = url
        });

        // Keep websites in alphabetical order
        var sortedWebsites = category.Websites
            .OrderBy(w => w.Name)
            .ToList();

        category.Websites.Clear();

        foreach (var website in sortedWebsites)
        {
            category.Websites.Add(website);
        }
    }
    public async Task OpenWebsite(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
    }
}