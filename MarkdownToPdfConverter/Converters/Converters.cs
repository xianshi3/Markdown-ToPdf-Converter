using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MarkdownToPdfConverter.Converters
{
    /// <summary>Provides static instances of commonly used Boolean converters.</summary>
    public static class BoolConverters
    {
        /// <summary>Inverts a Boolean value.</summary>
        public static readonly IValueConverter Not = new NotBoolConverter();
        /// <summary>Converts true to 1.0 opacity and false to 0.0.</summary>
        public static readonly IValueConverter BoolToOpacity = new BoolToOpacityConverter();
    }

    /// <summary>Inverts a Boolean value (true → false, false → true).</summary>
    /// <summary>Inverts a Boolean value (true → false, false → true).</summary>
    public class NotBoolConverter : IValueConverter
    {
        /// <summary>Returns the logical NOT of the Boolean value.</summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }

        /// <summary>Returns the logical NOT of the Boolean value (same as Convert).</summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }
    }

    /// <summary>Converts true to 1.0 (visible) and false to 0.0 (hidden).</summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        /// <summary>Returns 1.0 for true, 0.0 for false.</summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? 1.0 : 0.0;
            return 0.0;
        }

        /// <summary>Returns true if the opacity value is greater than 0.5.</summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d) return d > 0.5;
            return false;
        }
    }

    /// <summary>Converts true to italic font style, false to normal.</summary>
    public class BoolToFontStyleConverter : IValueConverter
    {
        /// <summary>Returns Italic when true, Normal otherwise.</summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b) return FontStyle.Italic;
            return FontStyle.Normal;
        }

        /// <summary>Not implemented — one-way converter only.</summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts true to a monospace font family, false to the default font.</summary>
    public class BoolToFontFamilyConverter : IValueConverter
    {
        /// <summary>Returns a monospace font when true, the default font otherwise.</summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b) return new FontFamily("JetBrains Mono, Cascadia Code, Consolas, monospace");
            return FontFamily.Default;
        }

        /// <summary>Not implemented — one-way converter only.</summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
