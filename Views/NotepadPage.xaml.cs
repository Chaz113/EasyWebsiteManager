using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Views;

public partial class NotepadPage : ContentPage
{
    private readonly WebsiteItem _website;
    private string _selectedColor;
    private bool _cancelled;
    private bool _savedExplicitly;

    public event Action<WebsiteItem>? NoteSaved;

    public NotepadPage(WebsiteItem website)
    {
        InitializeComponent();

        _website = website;
        _selectedColor = website.Note.BackgroundColor;

        NoteEditor.Text = website.Note.Text;

        ApplyColor(_selectedColor);
    }

    private void ApplyColor(string color)
    {
        _selectedColor = color;
        NoteCard.BackgroundColor = Color.FromArgb(color);
    }

    private void WhiteColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#FFFFFF");

    private void IvoryColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#F5EFE1");

    private void CreamyGreyColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#DDDBDD");

    private void MilkyOliveColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#C5E1B0");

    private void MilkyGrapeColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#B8C3F6");

    private void CreamyRoseColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#E9CACB");

    private void MilkyMauveColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#EDD2FF");

    private void MilkyBronzeColor_Clicked(object? sender, EventArgs e)
        => ApplyColor("#F1D1B0");

    private void ClearButton_Clicked(object? sender, EventArgs e)
    {
        NoteEditor.Text = string.Empty;
    }

    private async void CancelButton_Clicked(object? sender, EventArgs e)
    {
        _cancelled = true;

        await Navigation.PopModalAsync();
    }

    private async void SaveButton_Clicked(object? sender, EventArgs e)
    {
        SaveNote();

        _savedExplicitly = true;

        await Navigation.PopModalAsync();
    }

    private void SaveNote()
    {
        _website.Note.Text = NoteEditor.Text?.Trim() ?? "";
        _website.Note.BackgroundColor = _selectedColor;

        NoteSaved?.Invoke(_website);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_cancelled || _savedExplicitly)
            return;

        SaveNote();
    }
}