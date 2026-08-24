using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>Cuanto importa un diagnostico del rack compuesto. Misma escala que el resto del repo.</summary>
    public enum PushBackCompositeSeverity
    {
        /// <summary>Merece saberse; el resultado es lo que dice ser.</summary>
        Info = 0,

        /// <summary>El resultado es utilizable pero hay algo que el usuario tiene que ver.</summary>
        Warning = 1,

        /// <summary>La celda no es construible. No se dibuja y no se cuenta.</summary>
        Blocking = 2
    }

    /// <summary>
    /// I-42 — una cosa que el resolver compuesto quiere que el editor muestre. <see cref="Code"/> es el token
    /// estable: las pruebas se apoyan en el y nunca en el mensaje, que es prosa en espanol y puede reescribirse.
    /// </summary>
    public sealed class PushBackCompositeDiagnostic
    {
        public PushBackCompositeDiagnostic(
            PushBackCompositeSeverity severity, string code, string message, int frontIndex = -1, int levelNumber = -1)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Un diagnostico necesita su codigo estable.", nameof(code));
            }

            Severity = severity;
            Code = code;
            Message = message ?? string.Empty;
            FrontIndex = frontIndex;
            LevelNumber = levelNumber;
        }

        public PushBackCompositeSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }

        /// <summary>Ranura transversal a la que apunta, o -1 si es del rack.</summary>
        public int FrontIndex { get; }

        /// <summary>Nivel (1-based) al que apunta, o -1 si es del rack o de una ranura entera.</summary>
        public int LevelNumber { get; }

        public bool IsBlocking => Severity == PushBackCompositeSeverity.Blocking;

        public override string ToString() => "[" + Severity + "] " + Code + " — " + Message;
    }

    /// <summary>Los tokens estables del rack compuesto.</summary>
    public static class PushBackCompositeCodes
    {
        /// <summary>La demanda de la celda no cabe en la estructura efectiva: RequiredBedLength &gt; AvailableBedSpan.</summary>
        public const string CellDoesNotFit = "PB42_CELL_DOES_NOT_FIT";

        /// <summary>El override manual de estructura es MENOR que la propuesta derivada actual.</summary>
        public const string StructureOverrideBelowProposal = "PB42_STRUCTURE_OVERRIDE_BELOW_PROPOSAL";

        /// <summary>La topologia almacenada no puede aplicarse porque el nivel solo existe en un lado.</summary>
        public const string TopologyDegraded = "PB42_TOPOLOGY_DEGRADED";

        /// <summary>Se pidio separador central sin hueco donde ponerlo.</summary>
        public const string CentralSeparatorWithoutGap = "PB42_CENTRAL_SEPARATOR_WITHOUT_GAP";

        /// <summary>El hueco declarado no es una longitud fisica valida (negativo o no finito).</summary>
        public const string GapInvalid = "PB42_GAP_INVALID";

        /// <summary>El override manual de estructura no es un valor construible. NO equivale a restaurar.</summary>
        public const string StructureOverrideInvalid = "PB42_STRUCTURE_OVERRIDE_INVALID";
    }

    /// <summary>
    /// I-42 — LA lectura de diagnosticos de un rack compuesto YA RESUELTO. No decide nada ni corrige nada: traduce lo
    /// que el resolver dejo escrito en las celdas y en los lados a mensajes que el editor puede mostrar con su
    /// severidad. Es la unica fuente del editor, de modo que la UI no vuelve a razonar sobre geometria.
    /// </summary>
    public static class PushBackCompositeDiagnostics
    {
        /// <summary>
        /// Los diagnosticos de una INTENCION, antes de resolver nada. Existen porque una entrada invalida no puede
        /// convertirse en silencio en otro valor: el editor la conserva, la declara y bloquea.
        /// </summary>
        public static IReadOnlyList<PushBackCompositeDiagnostic> EvaluateIntent(
            double gap, bool centralSeparator, int? overrideA, int? overrideB)
        {
            var result = new List<PushBackCompositeDiagnostic>();
            if (double.IsNaN(gap) || double.IsInfinity(gap) || gap < 0.0)
            {
                result.Add(new PushBackCompositeDiagnostic(
                    PushBackCompositeSeverity.Blocking,
                    PushBackCompositeCodes.GapInvalid,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "El hueco ({0:0.##}\") no es una separacion fisica valida: debe ser cero o positivo. "
                        + "Corrigelo; no se interpreta como otro valor.",
                        gap)));
            }
            else if (centralSeparator && gap <= 0.0)
            {
                result.Add(new PushBackCompositeDiagnostic(
                    PushBackCompositeSeverity.Blocking,
                    PushBackCompositeCodes.CentralSeparatorWithoutGap,
                    "El separador central necesita un hueco mayor que cero: sin el no hay donde colocarlo."));
            }

            AppendInvalidOverride(result, overrideA, "A");
            AppendInvalidOverride(result, overrideB, "B");
            return result;
        }

        private static void AppendInvalidOverride(
            ICollection<PushBackCompositeDiagnostic> target, int? value, string label)
        {
            if (!value.HasValue || value.Value >= PushBackCellDepth.MinimumPalletsDeep)
            {
                return;
            }

            target.Add(new PushBackCompositeDiagnostic(
                PushBackCompositeSeverity.Blocking,
                PushBackCompositeCodes.StructureOverrideInvalid,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Lado {0}: la estructura manual ({1}) no es construible; el minimo fisico es {2} fondos. "
                    + "Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.",
                    label,
                    value.Value,
                    PushBackCellDepth.MinimumPalletsDeep)));
        }

        public static IReadOnlyList<PushBackCompositeDiagnostic> Evaluate(PushBackSystem system)
        {
            var result = new List<PushBackCompositeDiagnostic>();
            var composite = system?.Composite;
            if (composite == null)
            {
                return result;
            }

            AppendStructure(result, composite.SideA, "A");
            AppendStructure(result, composite.SideB, "B");

            foreach (var cell in composite.Cells)
            {
                if (cell == null || cell.IsValid)
                {
                    continue;
                }

                // El motivo ya nombra el LADO, el frente, el nivel y las dos magnitudes reales: lo escribe la
                // autoridad de capacidad, que es quien las midio. Aqui no se vuelve a redactar.
                result.Add(new PushBackCompositeDiagnostic(
                    PushBackCompositeSeverity.Blocking,
                    PushBackCompositeCodes.CellDoesNotFit,
                    cell.DisabledReason,
                    cell.FrontIndex,
                    cell.LevelNumber));
            }

            return result;
        }

        /// <summary>
        /// El aviso de un override de estructura por debajo de la propuesta. Es WARNING y no bloqueo: la estructura
        /// que el usuario pidio se respeta, y lo que se bloquea —celda por celda— es lo que no cabe en ella. Subirla
        /// en silencio seria construir un rack que nadie pidio.
        /// </summary>
        private static void AppendStructure(
            ICollection<PushBackCompositeDiagnostic> target, PushBackSideSystem side, string label)
        {
            if (side == null || !side.IsPresent || !side.StructureOverride.HasValue)
            {
                return;
            }

            if (side.StructureOverride.Value < PushBackCellDepth.MinimumPalletsDeep)
            {
                // Un valor manual INVALIDO no es «sin override»: eso es null, y solo lo escribe una restauracion
                // explicita. Se conserva la intencion y se bloquea, en vez de resolverla como otro valor.
                target.Add(new PushBackCompositeDiagnostic(
                    PushBackCompositeSeverity.Blocking,
                    PushBackCompositeCodes.StructureOverrideInvalid,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Lado {0}: la estructura manual ({1}) no es construible; el minimo fisico es {2} fondos. "
                        + "Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.",
                        label,
                        side.StructureOverride.Value,
                        PushBackCellDepth.MinimumPalletsDeep)));
                return;
            }

            if (side.StructureOverride.Value >= side.ProposedStructure)
            {
                return;
            }

            target.Add(new PushBackCompositeDiagnostic(
                PushBackCompositeSeverity.Warning,
                PushBackCompositeCodes.StructureOverrideBelowProposal,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Lado {0}: la estructura manual ({1} fondos) es menor que la propuesta derivada ({2}). "
                    + "Las celdas que no quepan quedaran bloqueadas; restaura la estructura para volver a la propuesta.",
                    label,
                    side.StructureOverride.Value,
                    side.ProposedStructure)));
        }
    }
}
