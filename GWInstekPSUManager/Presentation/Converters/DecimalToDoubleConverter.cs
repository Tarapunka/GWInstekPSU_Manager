using System.Globalization;
using System.Windows.Data;

namespace GWInstekPSUManager.Presentation.Converters;

public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Для привязки к интерфейсу (decimal -> double)
        if (value is decimal decimalValue)
            return System.Convert.ToDouble(decimalValue);

        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Для обратной привязки (double -> decimal)
        if (value is double doubleValue)
            return System.Convert.ToDecimal(doubleValue);

        return 0m;
    }
}