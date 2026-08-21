using System.Collections.ObjectModel;
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
}