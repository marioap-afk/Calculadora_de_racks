using System;
using System.Globalization;
using System.Windows.Data;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// The label of one family row of <see cref="StructuralSectionPicker"/>'s filter: «Todas» for the sentinel,
    /// the family's own short name otherwise. The wording lives in the Application layer
    /// (<c>StructuralSectionSearch.Label</c>) so the picker, its tests and any future consumer read the same one.
    /// </summary>
    public sealed class SectionFamilyLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => StructuralSectionPicker.FamilyLabel(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException("El filtro de familia no se convierte de vuelta.");
    }
}
