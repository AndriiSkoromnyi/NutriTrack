using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace NutriTrack.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGoalMet)
            {
                return isGoalMet ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Orange);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 