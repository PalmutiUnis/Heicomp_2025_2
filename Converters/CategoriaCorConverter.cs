using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiApp1.Converters
{
    public class CategoriaCorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var categoria = (value?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
            // Default palette; adjust as desired
            return categoria switch
            {
                "docente" => Color.FromArgb("#2563EB"),          // blue
                "administrativo" => Color.FromArgb("#10B981"),   // green
                "dois cargos" => Color.FromArgb("#F59E0B"),      // amber
                _ => Color.FromArgb("#6B7280")                    // gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
