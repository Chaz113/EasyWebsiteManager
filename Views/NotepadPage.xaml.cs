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

        // A soft-pewter outline identifies the selected pastel swatch.
        // No text or checkmark is displayed inside any color.
        foreach (var swatch in GetColorButtons())
        {
            swatch.Text = string.Empty;
            swatch.BorderWidth = 1;
            swatch.BorderColor = Color.FromArgb("#888888");
        }

        foreach (var (swatch, swatchColor) in GetColorOptions())
        {
            if (!string.Equals(color, swatchColor, StringComparison.OrdinalIgnoreCase))
                continue;

            swatch.BorderWidth = 3;
            swatch.BorderColor = Color.FromArgb("#7A858A");
            break;
        }
    }


    private Button[] GetColorButtons() =>
    [
        WhiteColorButton,
        IvoryColorButton,
        CreamyGreyColorButton,
        MilkyOliveColorButton,
        MilkyGrapeColorButton,
        CreamyRoseColorButton,
        MilkyMauveColorButton,
        MilkyBronzeColorButton
    ];

    private (Button Button, string Color)[] GetColorOptions() =>
    [
        (WhiteColorButton, "#FFFFFF"),
        (IvoryColorButton, "#F5EFE1"),
        (CreamyGreyColorButton, "#DDDBDD"),
        (MilkyOliveColorButton, "#C5E1B0"),
        (MilkyGrapeColorButton, "#B8C3F6"),
        (CreamyRoseColorButton, "#E9CACB"),
        (MilkyMauveColorButton, "#EDD2FF"),
        (MilkyBronzeColorButton, "#F1D1B0")
    ];

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