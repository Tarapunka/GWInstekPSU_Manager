using System.Globalization;
using System.Windows.Data;

namespace GWInstekPSUManager.Presentation.Converters;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;

        return value; // Если значение не bool, возвращаем как есть (или можно выбросить исключение)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;

        return value;
    }
}