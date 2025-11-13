using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using MauiApp1.Services;

namespace MauiApp1.Converters
{
    public class CategoriaTotalSumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<CategoriaTotalDto> list)
            {
                var totalRow = list.FirstOrDefault(x => string.Equals(x.Categoria, "Total", StringComparison.OrdinalIgnoreCase));
                var total = totalRow?.Total ?? list.Sum(x => x.Total);
                return total;
            }
            if (value is IEnumerable enumerable)
            {
                // fallback: count items if not strongly typed
                int count = 0;
                foreach (var _ in enumerable) count++;
                return count;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
