using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyWebsiteManager.Models;

public class WebsiteCategory : ObservableObject
{
    private string _name = "";
    private bool _isExpanded = true;
    private string _textColor = "Default";

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public string TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    public ObservableCollection<WebsiteItem> Websites { get; set; } = new();
}