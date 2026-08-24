using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyWebsiteManager.Models;

public class WebsiteNote : ObservableObject
{
    private string _text = "";
    private string _backgroundColor = "#F5EFE1";

    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value))
            {
                OnPropertyChanged(nameof(HasNote));
            }
        }
    }

    public string BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    public bool HasNote =>
        !string.IsNullOrWhiteSpace(Text);
}