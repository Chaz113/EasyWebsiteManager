using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyWebsiteManager.Models;

public class WebsiteItem : ObservableObject
{
    private string _name = "";
    private string _url = "";
    private string _textColor = "Default";

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public string TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    public WebsiteNote Note { get; set; } = new();
}