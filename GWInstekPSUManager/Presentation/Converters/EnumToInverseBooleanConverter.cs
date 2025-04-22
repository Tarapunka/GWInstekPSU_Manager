using System.Globalization;
using System.Windows.Data;

namespace GWInstekPSUManager.Presentation.Converters;

public class EnumToInverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return true;

        string checkValue = parameter.ToString();
        string enumValue = value.ToString();

        return !enumValue.Equals(checkValue, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}