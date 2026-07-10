using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    /// <summary>
    /// Converts the frame enums (Cara, Dirección, Patrón de celosía, Estado) to their Spanish label for display in
    /// the combos, so the UI never shows the raw English enum name. One-way: the bound SelectedItem stays the enum
    /// value; only what the user reads is translated. Wired via implicit (DataType) DataTemplates in the window.
    /// </summary>
    public sealed class EnumDisplayConverter : IValueConverter
    {
        private static readonly Dictionary<object, string> Labels = new Dictionary<object, string>
        {
            // Cara (FrameSide)
            { FrameSide.Front, "Frente" },
            { FrameSide.Back, "Atrás" },
            { FrameSide.Both, "Ambas" },

            // Dirección (DiagonalDirection)
            { DiagonalDirection.AutoAlternating, "Auto (alterna)" },
            { DiagonalDirection.UpRight, "Sube a la derecha" },
            { DiagonalDirection.UpLeft, "Sube a la izquierda" },

            // Patrón de celosía (BracingPattern)
            { BracingPattern.NoBracing, "Sin celosía" },
            { BracingPattern.SingleDiagonal, "Diagonal simple" },
            { BracingPattern.DoubleDiagonal, "Diagonal doble" },
            { BracingPattern.XBracing, "Celosía en X" },
            { BracingPattern.KBracing, "Celosía en K" },
            { BracingPattern.Custom, "Personalizado" },

            // Estado del componente (FrameComponentState)
            { FrameComponentState.Standard, "Estándar" },
            { FrameComponentState.Modified, "Modificado" },
            { FrameComponentState.Manual, "Manual" },
            { FrameComponentState.Rule, "Regla" },
            { FrameComponentState.Exception, "Excepción" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null && Labels.TryGetValue(value, out var label) ? label : value?.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
