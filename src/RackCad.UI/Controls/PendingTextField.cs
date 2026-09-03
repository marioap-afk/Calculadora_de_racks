using System;
using System.Windows.Controls;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// La cara no genérica de <see cref="PendingTextField{T}"/>, para que un commit pueda recorrer campos de tipos
    /// distintos en el orden contractual sin conocer sus valores.
    /// </summary>
    internal interface IPendingTextField
    {
        /// <summary>Nombre del campo tal como lo lee el usuario, para los mensajes de error.</summary>
        string Label { get; }

        /// <summary>Hay una edición humana sin resolver.</summary>
        bool IsDirty { get; }

        /// <summary>
        /// Fase 1: valida lo pendiente y lo deja preparado, SIN mutar nada. Un campo limpio devuelve <c>true</c> y no
        /// prepara nada. Devuelve <c>false</c> con el motivo cuando el texto no es aceptable.
        /// </summary>
        bool TryStage(out string error);

        /// <summary>Fase 2: aplica lo que la fase 1 preparó. No hace nada si no había nada preparado.</summary>
        void ApplyStaged();

        /// <summary>Vuelve a mostrar el estado comprometido y da la edición por resuelta.</summary>
        void ShowCommitted();

        /// <summary>Descarte EXPLÍCITO de lo pendiente (auto-reparación / carga).</summary>
        void ResetToCommitted();
    }

    /// <summary>
    /// Un campo de texto que edita un valor PENDIENTE, distinto del valor comprometido (I-43, ADR-0032).
    /// <para>
    /// El control no sabe nada del Selectivo: no conoce el estado, ni los fondos destino, ni ninguna regla de
    /// dominio. Solo distingue lo que ESCRIBIÓ EL USUARIO de lo que le MOSTRÓ el programa, y parte el commit en dos
    /// fases —preparar y aplicar— para que un conjunto de campos pueda validarse entero antes de que ninguno mute
    /// nada. La mutación sigue viviendo en la ventana y en el estado.
    /// </para>
    /// <para>
    /// La distinción entre <see cref="Show"/> y <see cref="ResetToCommitted"/> es deliberada: <c>Show</c> refleja el
    /// valor comprometido y NUNCA debe pisar una edición sin resolver (eso sería perderla en silencio), mientras que
    /// el reset es un descarte explícito — la auto-reparación de un texto inválido, o una carga que reemplaza todo el
    /// estado.
    /// </para>
    /// </summary>
    /// <typeparam name="T">El valor que el texto representa una vez validado.</typeparam>
    internal sealed class PendingTextField<T> : IPendingTextField
    {
        private readonly TextBox box;
        private readonly Func<string, PendingParse<T>> parse;
        private readonly Action<T> apply;
        private readonly Func<string> committedText;
        private bool programmatic;
        private bool staged;
        private T stagedValue;

        /// <param name="box">La caja que el usuario edita.</param>
        /// <param name="label">Nombre del campo tal como lo lee el usuario.</param>
        /// <param name="parse">Valida el texto SIN mutar nada (fase 1).</param>
        /// <param name="apply">Escribe el valor ya validado (fase 2).</param>
        /// <param name="committedText">El texto que corresponde al estado comprometido ahora mismo.</param>
        internal PendingTextField(
            TextBox box,
            string label,
            Func<string, PendingParse<T>> parse,
            Action<T> apply,
            Func<string> committedText)
        {
            this.box = box ?? throw new ArgumentNullException(nameof(box));
            this.parse = parse ?? throw new ArgumentNullException(nameof(parse));
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
            this.committedText = committedText ?? throw new ArgumentNullException(nameof(committedText));
            Label = label;
            this.box.TextChanged += (s, e) => { if (!programmatic) IsDirty = true; };
        }

        public string Label { get; }

        /// <summary>
        /// Hubo una edición humana sin resolver. Retipear el MISMO valor visible cuenta como edición: el usuario
        /// expresó una intención y O-43-02 exige respetarla.
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>El texto tal como está ahora en la caja.</summary>
        internal string Text => box.Text;

        /// <summary>
        /// Refleja el valor COMPROMETIDO. No marca la edición como pendiente.
        /// <para>
        /// Hacer <c>Show</c> sobre un campo con una edición sin resolver descartaría lo tecleado sin que el usuario se
        /// entere; es un defecto de la ventana, no de este control, así que en DEBUG revienta en vez de tragárselo
        /// (INV-14). El caller que de verdad quiere descartar tiene <see cref="ResetToCommitted"/>.
        /// </para>
        /// </summary>
        internal void Show(string text)
        {
#if DEBUG
            // La protección NO puede depender de que el texto difiera. Una edición humana que acaba en el mismo texto
            // comprometido sigue siendo una intención explícita (O-43-02: retipear el mismo valor CUENTA como
            // edición), y con la comparación de textos se descartaba en silencio. INV-14 es incondicional: un campo
            // sin resolver solo sale de ese estado por un commit o por un descarte explícito.
            if (IsDirty)
            {
                throw new InvalidOperationException(
                    "Show() sobre '" + Label + "' con una edición pendiente sin comprometer ni descartar (I-43, INV-14). "
                        + "Compromete el campo antes, o descártalo explícitamente con Reset si eso es lo que quieres.");
            }
#endif
            Write(text);
        }

        /// <summary>Descarte EXPLÍCITO de lo pendiente con un texto dado (carga).</summary>
        internal void Reset(string text) => Write(text);

        public void ResetToCommitted() => Write(committedText());

        public void ShowCommitted() => Show(committedText());

        public bool TryStage(out string error)
        {
            staged = false;
            stagedValue = default;
            error = null;
            if (!IsDirty) return true; // limpio: ni aporta ni bloquea

            var parsed = parse(box.Text);
            if (!parsed.Ok)
            {
                error = parsed.Error;
                return false;
            }

            staged = parsed.HasValue;
            stagedValue = parsed.Value;
            return true;
        }

        public void ApplyStaged()
        {
            if (!staged) return;
            staged = false;
            // La edicion queda RESUELTA en cuanto se aplica: a partir de aqui la caja puede volver a mostrar el
            // estado comprometido sin que eso sea perder nada (es justo lo contrario).
            IsDirty = false;
            apply(stagedValue);
        }

        private void Write(string text)
        {
            programmatic = true;
            try
            {
                box.Text = text ?? string.Empty;
            }
            finally
            {
                programmatic = false;
                IsDirty = false;
            }
        }
    }

    /// <summary>Resultado de la fase 1: nada que hacer, un valor válido, o un error con su motivo.</summary>
    internal readonly struct PendingParse<T>
    {
        private PendingParse(bool has, bool ok, T value, string error)
        {
            HasValue = has;
            Ok = ok;
            Value = value;
            Error = error;
        }

        /// <summary>El texto no representa ningún cambio aplicable, pero tampoco es un error.</summary>
        internal static PendingParse<T> Nothing => new PendingParse<T>(false, true, default, null);

        internal static PendingParse<T> Valid(T value) => new PendingParse<T>(true, true, value, null);

        internal static PendingParse<T> Invalid(string error) => new PendingParse<T>(false, false, default, error);

        /// <summary>Hay un valor aplicable.</summary>
        internal bool HasValue { get; }

        /// <summary>El texto es aceptable.</summary>
        internal bool Ok { get; }

        internal T Value { get; }

        internal string Error { get; }
    }
}
