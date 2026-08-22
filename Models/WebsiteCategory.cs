using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyWebsiteManager.Models;

public class WebsiteCategory : ObservableObject
{
    private string name = "";
    private bool isExpanded = true;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set => SetProperty(ref isExpanded, value);
    }

    public ObservableCollection<WebsiteItem> Websites { get; } = new();
}