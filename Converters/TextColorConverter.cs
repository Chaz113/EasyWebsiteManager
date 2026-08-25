using System.Globalization;

namespace EasyWebsiteManager.Converters;

public class TextColorConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var colorValue = value as string;

        if (string.IsNullOrWhiteSpace(colorValue) ||
            colorValue.Equals(
                "Default",
                StringComparison.OrdinalIgnoreCase))
        {
            return Application.Current?.RequestedTheme == AppTheme.Dark
                ? Colors.White
                : Colors.Black;
        }

        try
        {
            return Color.FromArgb(colorValue);
        }
        catch
        {
            return Application.Current?.RequestedTheme == AppTheme.Dark
                ? Colors.White
                : Colors.Black;
        }
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}