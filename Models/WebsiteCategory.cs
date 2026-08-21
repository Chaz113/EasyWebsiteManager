using System.Collections.ObjectModel;

namespace EasyWebsiteManager.Models;

public class WebsiteCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public bool IsExpanded { get; set; } = true;

    public ObservableCollection<WebsiteItem> Websites { get; set; } = new();
}