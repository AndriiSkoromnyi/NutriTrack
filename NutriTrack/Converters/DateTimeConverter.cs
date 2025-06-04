using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NutriTrack.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                if (targetType == typeof(DateTimeOffset?))
                {
                    return new DateTimeOffset(dateTime);
                }
                if (targetType == typeof(DateTimeOffset))
                {
                    return new DateTimeOffset(dateTime);
                }
                if (parameter is string format)
                {
                    return dateTime.ToString(format, culture);
                }
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                if (targetType == typeof(DateTime))
                {
                    return dateTimeOffset.DateTime;
                }
                if (parameter is string format)
                {
                    return dateTimeOffset.DateTime.ToString(format, culture);
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(DateTime))
            {
                if (value is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset.DateTime;
                }
                if (value is string timeString && parameter is string format && format == "HH:mm")
                {
                    if (DateTime.TryParseExact(timeString, format, culture, DateTimeStyles.None, out var parsedTime))
                    {
                        var currentDate = DateTime.Now.Date;
                        return currentDate.Add(parsedTime.TimeOfDay);
                    }
                }
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                if (value is DateTime dateTime)
                {
                    return new DateTimeOffset(dateTime);
                }
            }

            return value;
        }
    }
} 