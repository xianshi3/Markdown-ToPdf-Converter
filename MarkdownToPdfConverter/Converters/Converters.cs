using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using MarkdownToPdfConverter.Services;

namespace MarkdownToPdfConverter.Converters
{
    public class BlockTypeToFontSizeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PreviewBlock block)
            {
                return block.FontSize;
            }
            return 14.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BlockTypeToForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PreviewBlock block)
            {
                var colorStr = block.ForegroundColor;
                return new SolidColorBrush(ParseColor(colorStr));
            }
            return new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static uint ParseColor(string hex)
        {
            if (hex.StartsWith("#") && hex.Length == 7)
            {
                return uint.Parse(hex.Substring(1), System.Globalization.NumberStyles.HexNumber);
            }
            return 0xFFE6EDF3;
        }
    }

    public class BlockTypeToFontWeightConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PreviewBlock block)
            {
                return block.FontWeight == "Bold" ? FontWeight.Bold : FontWeight.Normal;
            }
            return FontWeight.Normal;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}