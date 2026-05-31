using System.Globalization;

namespace PracticoMobileApp.Converters
{
    /// <summary>
    /// Converter para mostrar medallas solo en las primeras 3 posiciones.
    /// Usage: IsVisible="{Binding Posicion, Converter={StaticResource IntToBoolConverter}, ConverterParameter='1'}"
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int posicion && parameter is string targetPosition)
            {
                return posicion.ToString() == targetPosition;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
