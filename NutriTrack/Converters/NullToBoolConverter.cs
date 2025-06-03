using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NutriTrack.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        // Если value == null, возвращаем false, иначе true
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        // Обратное преобразование не требуется
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}