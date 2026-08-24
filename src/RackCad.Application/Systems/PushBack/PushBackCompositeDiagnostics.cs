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
    }

    /// <summary>
    /// I-42 — LA lectura de diagnosticos de un rack compuesto YA RESUELTO. No decide nada ni corrige nada: traduce lo
    /// que el resolver dejo escrito en las celdas y en los lados a mensajes que el editor puede mostrar con su
    /// severidad. Es la unica fuente del editor, de modo que la UI no vuelve a razonar sobre geometria.
    /// </summary>
    public static class PushBackCompositeDiagnostics
    {
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

            if ((system.Composite.CentralSeparator == false) && composite.Gap <= 0.0)
            {
                // Nada que decir: sin hueco y sin separador el rack esta bien. El aviso solo aparece cuando el
                // usuario PIDIO separador y no hay hueco, y eso lo detecta el editor antes de resolver.
            }

            foreach (var cell in composite.Cells)
            {
                if (cell == null || cell.IsValid)
                {
                    continue;
                }

                result.Add(new PushBackCompositeDiagnostic(
                    PushBackCompositeSeverity.Blocking,
                    PushBackCompositeCodes.CellDoesNotFit,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Frente {0}, nivel {1}: {2}",
                        cell.FrontIndex + 1,
                        cell.LevelNumber,
                        cell.DisabledReason),
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
