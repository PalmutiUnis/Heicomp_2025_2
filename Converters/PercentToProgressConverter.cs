using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiApp1.Converters
{
    public class PercentToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            double percent = System.Convert.ToDouble(value);
            return percent / 100.0; // converte 52 → 0.52
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
