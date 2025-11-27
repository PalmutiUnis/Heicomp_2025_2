using System.Globalization;

// ATENÇÃO: Mude 'MauiApp1' para o nome do seu projeto se for diferente
namespace MauiApp1.Converters
{
    public class PercentToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal percent)
            {
                // Multiplica a porcentagem por 2 para definir a altura.
                // Ex: 92.7% vira ~185px de altura.
                return (double)(percent * 2m);
            }
            // Tenta converter se vier como string ou double
            if (value != null && decimal.TryParse(value.ToString(), out decimal result))
            {
                return (double)(result * 2m);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}