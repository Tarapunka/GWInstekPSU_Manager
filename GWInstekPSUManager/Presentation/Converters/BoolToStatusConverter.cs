using System;
using System.Globalization;
using System.Windows.Data;

namespace GWInstekPSUManager.Presentation.Converters;

[ValueConversion(typeof(bool), typeof(string))]
public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Подключено" : "Отключено";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}