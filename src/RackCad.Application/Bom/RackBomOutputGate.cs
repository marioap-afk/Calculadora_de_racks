using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Bom
{
    /// <summary>El veredicto de si un rack puede producir salida final, y el motivo cuando no.</summary>
    public sealed class RackBomOutputVerdict
    {
        private RackBomOutputVerdict(bool allowed, string reason)
        {
            Allowed = allowed;
            Reason = reason;
        }

        /// <summary>True cuando ese rack puede cotizarse y dibujarse.</summary>
        public bool Allowed { get; }

        /// <summary>El motivo por el que NO puede, ya redactado por la autoridad que lo midio. Null si puede.</summary>
        public string Reason { get; }

        public static readonly RackBomOutputVerdict Allow = new RackBomOutputVerdict(true, null);

        public static RackBomOutputVerdict Deny(string reason)
            => new RackBomOutputVerdict(false, string.IsNullOrWhiteSpace(reason) ? "diseño bloqueado" : reason.Trim());
    }

    /// <summary>
    /// I-42 (A1C/H11, contrato del dueño) — UN DISEÑO BLOQUEADO NO PRODUCE SALIDA, Y EL USUARIO SABE POR QUE.
    ///
    /// <para>
    /// El editor ya no deja insertar, actualizar ni cotizar un rack con un diagnostico BLOQUEANTE. Los comandos de
    /// AutoCAD recorrian los mismos diseños sin ese filtro, asi que un rack cuya celda no cabe —su cama pide mas
    /// longitud de la que su estructura tiene— entraba en el BOM del dibujo como si fuera correcto. Y un rack que no
    /// se podia interpretar se saltaba en silencio: el total salia mas corto sin decir nada.
    /// </para>
    /// <para>
    /// <b>No hay una segunda regla.</b> El veredicto lo da la MISMA autoridad que usa el editor
    /// (<see cref="PushBackCompositeDiagnostics"/>) y el motivo es el que ella redacta. Aqui solo se traduce a la
    /// pregunta que un comando necesita hacer: ¿puede este rack salir?
    /// </para>
    /// </summary>
    public static class RackBomOutputGate
    {
        /// <summary>El veredicto de un sistema Push Back ya resuelto.</summary>
        public static RackBomOutputVerdict For(PushBackSystem system)
        {
            if (system == null)
            {
                return RackBomOutputVerdict.Allow;   // no es asunto de esta puerta: el llamador ya lo reporta
            }

            var blocking = PushBackCompositeDiagnostics.Evaluate(system)
                .FirstOrDefault(diagnostic => diagnostic != null && diagnostic.IsBlocking);
            return blocking == null ? RackBomOutputVerdict.Allow : RackBomOutputVerdict.Deny(blocking.Message);
        }

        /// <summary>
        /// El mensaje con el que un comando ABORTA por racks bloqueados. Nombra cada rack y su motivo: un total del
        /// que falta un rack no puede parecer completo.
        /// </summary>
        public static string DescribeBlocked(IEnumerable<KeyValuePair<string, string>> blocked)
        {
            var entries = (blocked ?? Enumerable.Empty<KeyValuePair<string, string>>()).ToList();
            if (entries.Count == 0)
            {
                return string.Empty;
            }

            var header = string.Format(
                CultureInfo.CurrentCulture,
                entries.Count == 1
                    ? "RackCad: no se genera el listado. {0} rack tiene un problema que impide cotizarlo:"
                    : "RackCad: no se genera el listado. {0} racks tienen problemas que impiden cotizarlos:",
                entries.Count);

            return entries.Aggregate(
                header,
                (text, entry) => text + Environment.NewLine
                    + "  - " + (string.IsNullOrWhiteSpace(entry.Key) ? "(sin nombre)" : entry.Key.Trim())
                    + ": " + entry.Value);
        }

        /// <summary>El aviso de un rack cuyo diseño no se pudo interpretar. Saltarlo en silencio no es una opcion.</summary>
        public static string DescribeUnreadable(string rackName)
            => "RackCad: el rack «"
               + (string.IsNullOrWhiteSpace(rackName) ? "(sin nombre)" : rackName.Trim())
               + "» no se pudo interpretar y queda FUERA del listado.";
    }
}
