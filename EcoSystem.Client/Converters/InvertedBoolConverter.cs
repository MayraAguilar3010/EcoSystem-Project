using System.Globalization;

namespace EcoSystem.Client.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool boolValue && !boolValue;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool boolValue && !boolValue;
}
