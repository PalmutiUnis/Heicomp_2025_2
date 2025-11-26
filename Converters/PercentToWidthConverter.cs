using System.Globalization;

namespace MauiApp1.Converters // Use seu namespace correto (Heicomp_2025_2 se for o caso)
{
    public class PercentToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && decimal.TryParse(value.ToString(), out decimal percent))
            {
                // DIMINUÍMOS O MULTIPLICADOR PARA 2.2
                // Isso impede que a barra "Branca" (52%) estoure a tela do celular.
                return (double)(percent * 2.2m);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}