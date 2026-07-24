using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// Collapses a shell slot when it has no content: a null slot (or an empty string) → <see cref="Visibility.Collapsed"/>
    /// so an optional slot leaves NO gap; anything else → <see cref="Visibility.Visible"/>. Pure and reusable.
    /// </summary>
    public sealed class ShellSlotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null || (value is string s && string.IsNullOrWhiteSpace(s))
                ? Visibility.Collapsed
                : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
