using System;
using System.ComponentModel;
using System.Globalization;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.Systems.Cantilever
{
    /// <summary>
    /// Una fila de la tabla avanzada de paneles: el tramo tal y como se escribe y se lee en pantalla.
    ///
    /// <para><b>Lleva TEXTO y no números.</b> Un campo numérico que se rehúsa a contener «12,» mientras el
    /// usuario está escribiendo «12.5» es un campo que pelea con quien lo usa. Aquí se acepta lo que se teclea
    /// y se convierte al confirmar; si no convierte, se dice por qué, en la misma ventana, en vez de revertir
    /// la celda en silencio.</para>
    ///
    /// <para><b>La altura es de sólo lectura</b>, y no por comodidad: es derivada de las dos cotas. Poder
    /// escribirla la convertiría en una tercera autoridad sobre el mismo hecho, y habría que decidir cuál de
    /// las tres gana cuando no coincidan.</para>
    /// </summary>
    public sealed class CantileverPanelSegmentRow : INotifyPropertyChanged
    {
        private string y1;
        private string y2;
        private bool braced;
        private string height;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>El ordinal que se lee en el plano: base UNO, porque nadie cuenta tramos desde cero.</summary>
        public int Number { get; private set; }

        public string Y1
        {
            get => y1;
            set
            {
                y1 = value;
                Raise(nameof(Y1));
            }
        }

        public string Y2
        {
            get => y2;
            set
            {
                y2 = value;
                Raise(nameof(Y2));
            }
        }

        /// <summary>La altura, DERIVADA y sólo de lectura.</summary>
        public string Height
        {
            get => height;
            private set
            {
                height = value;
                Raise(nameof(Height));
            }
        }

        /// <summary>Si el tramo lleva la X de dos tensores. Apagado es un hueco, que es un tramo igual.</summary>
        public bool Braced
        {
            get => braced;
            set
            {
                braced = value;
                Raise(nameof(Braced));
            }
        }

        private string Format { get; set; } = "0.####";

        public static CantileverPanelSegmentRow From(
            int index, CantileverPanelSegmentDesign segment, string format)
        {
            var text = string.IsNullOrWhiteSpace(format) ? "0.####" : format;

            return new CantileverPanelSegmentRow
            {
                Number = index + 1,
                Format = text,
                y1 = segment.StartElevation.ToString(text, CultureInfo.InvariantCulture),
                y2 = segment.EndElevation.ToString(text, CultureInfo.InvariantCulture),
                height = segment.Height.ToString(text, CultureInfo.InvariantCulture),
                braced = segment.BracingMode == CantileverPanelBracingMode.CrossBraced
            };
        }

        /// <summary>
        /// El tramo que esta fila describe, o el motivo por el que no describe ninguno.
        ///
        /// Sólo comprueba que las dos cotas SEAN números. Que suban, que no se solapen y que quepan en la
        /// columna lo comprueba la validación al resolver, sobre la lista entera: una fila no puede saber si
        /// pisa a su vecina, y fingir que sí llevaría a dos validaciones que un día no dirían lo mismo.
        /// </summary>
        public bool TryToDesign(out CantileverPanelSegmentDesign segment, out string reason)
        {
            segment = null;
            reason = null;

            if (!TryNumber(Y1, out var start))
            {
                reason = "El tramo " + Number + " tiene una cota inferior que no es un numero: '" + Y1 + "'.";
                return false;
            }

            if (!TryNumber(Y2, out var end))
            {
                reason = "El tramo " + Number + " tiene una cota superior que no es un numero: '" + Y2 + "'.";
                return false;
            }

            segment = new CantileverPanelSegmentDesign
            {
                StartElevation = start,
                EndElevation = end,
                BracingMode = Braced
                    ? CantileverPanelBracingMode.CrossBraced
                    : CantileverPanelBracingMode.None
            };

            Height = (end - start).ToString(Format, CultureInfo.InvariantCulture);

            return true;
        }

        /// <summary>
        /// Acepta el punto Y la coma decimal.
        ///
        /// El teclado de quien dibuja no siempre es el de la cultura invariante, y rechazar «12,5» por no ser
        /// «12.5» sería rechazar un número que la persona escribió bien.
        /// </summary>
        private static bool TryNumber(string text, out double value)
        {
            value = 0.0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalised = text.Trim().Replace(',', '.');

            return double.TryParse(
                       normalised, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private void Raise(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
