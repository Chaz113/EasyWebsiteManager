using System.Collections.ObjectModel;
using System.Linq;
using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.ViewModels;

public class MainViewModel
{
    private string _searchText = "";

    public AppSettings Settings { get; } = new();

    public ObservableCollection<WebsiteCategory> Categories { get; } = new();

    public ObservableCollection<WebsiteCategory> FilteredCategories { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            ApplySearch();
        }
    }

    public MainViewModel()
    {
        Categories.Add(new WebsiteCategory { Name = "Retail" });
        Categories.Add(new WebsiteCategory { Name = "Weather" });
        Categories.Add(new WebsiteCategory { Name = "Videos to Watch" });

        ApplySearch();
    }

    public void AddWebsite(
        string categoryName,
        string websiteName,
        string url)
    {
        var category = Categories.FirstOrDefault(c =>
            c.Name.Equals(
                categoryName,
                StringComparison.OrdinalIgnoreCase));

        if (category == null)
        {
            category = new WebsiteCategory
            {
                Name = categoryName
            };

            Categories.Add(category);

            ResortCategories();
        }

        category.Websites.Add(new WebsiteItem
        {
            Name = websiteName,
            Url = url,
            TextColor = category.TextColor
        });

        var sortedWebsites = category.Websites
            .OrderBy(w => w.Name)
            .ToList();

        category.Websites.Clear();

        foreach (var website in sortedWebsites)
        {
            category.Websites.Add(website);
        }

        ApplySearch();
    }

    public bool DeleteCategory(WebsiteCategory category)
    {
        var removed = Categories.Remove(category);

        if (removed)
        {
            ApplySearch();
        }

        return removed;
    }

    public void RestoreCategory(WebsiteCategory category)
    {
        if (Categories.Contains(category))
            return;

        Categories.Add(category);

        ResortCategories();
    }

    public void ResortCategories()
    {
        var sortedCategories = Categories
            .OrderBy(c => c.Name)
            .ToList();

        Categories.Clear();

        foreach (var category in sortedCategories)
        {
            Categories.Add(category);
        }

        ApplySearch();
    }

    public AppData CreateAppData()
    {
        return new AppData
        {
            Categories = Categories.ToList(),
            Settings = Settings
        };
    }

    public void LoadAppData(AppData data)
    {
        // One-time migration:
        // old default white text becomes theme-aware "Default".
        if (!data.Settings.TextColorDefaultsMigrated)
        {
            foreach (var category in data.Categories)
            {
                if (category.TextColor.Equals(
                    "#FFFFFF",
                    StringComparison.OrdinalIgnoreCase))
                {
                    category.TextColor = "Default";
                }

                foreach (var website in category.Websites)
                {
                    if (website.TextColor.Equals(
                        "#FFFFFF",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        website.TextColor = "Default";
                    }
                }
            }

            data.Settings.TextColorDefaultsMigrated = true;
        }

        Categories.Clear();

        foreach (var category in data.Categories
            .OrderBy(c => c.Name))
        {
            var sortedWebsites = category.Websites
                .OrderBy(w => w.Name)
                .ToList();

            category.Websites.Clear();

            foreach (var website in sortedWebsites)
            {
                category.Websites.Add(website);
            }

            Categories.Add(category);
        }

        Settings.ConfirmDelete =
            data.Settings.ConfirmDelete;

        Settings.Appearance =
            string.IsNullOrWhiteSpace(data.Settings.Appearance)
                ? "System"
                : data.Settings.Appearance;

        Settings.TextColorDefaultsMigrated =
            data.Settings.TextColorDefaultsMigrated;

        ApplySearch();
    }

    private void ApplySearch()
    {
        FilteredCategories.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var category in Categories)
            {
                FilteredCategories.Add(category);
            }

            return;
        }

        var search = SearchText.Trim();

        foreach (var category in Categories)
        {
            if (category.Name.Contains(
                search,
                StringComparison.OrdinalIgnoreCase))
            {
                FilteredCategories.Add(category);
                continue;
            }

            var matchingWebsites = category.Websites
                .Where(w =>
                    w.Name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.Name)
                .ToList();

            if (matchingWebsites.Count == 0)
                continue;

            var filteredCategory = new WebsiteCategory
            {
                Id = category.Id,
                Name = category.Name,
                IsExpanded = true,
                TextColor = category.TextColor
            };

            foreach (var website in matchingWebsites)
            {
                filteredCategory.Websites.Add(website);
            }

            FilteredCategories.Add(filteredCategory);
        }
    }

    public async Task OpenWebsite(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

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
}