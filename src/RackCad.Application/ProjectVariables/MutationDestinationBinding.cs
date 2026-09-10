using System;
using System.Collections.Generic;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Whether the plan's destinations still match what the drawing has.</summary>
    public enum DestinationBindingOutcome
    {
        Bound = 1,

        /// <summary>They do not, and the difference cannot be resolved without guessing. Nothing is written.</summary>
        Abort = 2,
    }

    /// <summary>The answer, with the reason the user has to read when it is no.</summary>
    public sealed class DestinationBindingResult
    {
        private DestinationBindingResult(DestinationBindingOutcome outcome, string error)
        {
            Outcome = outcome;
            Error = error;
        }

        public DestinationBindingOutcome Outcome { get; }

        /// <summary>Null when bound. Names the rack and the definition that could not be reconciled.</summary>
        public string Error { get; }

        public bool IsBound => Outcome == DestinationBindingOutcome.Bound;

        public static DestinationBindingResult Bound()
            => new DestinationBindingResult(DestinationBindingOutcome.Bound, null);

        public static DestinationBindingResult Abort(string error)
            => new DestinationBindingResult(DestinationBindingOutcome.Abort, error);
    }

    /// <summary>
    /// Checks that the views a plan names are EXACTLY the views the drawing has for that rack, before a single
    /// one is written.
    ///
    /// <para>
    /// A plan is built against a photograph of the drawing; the write happens later. The two directions of
    /// drift are not the same failure, and neither of them is recoverable by doing what can be done:
    /// </para>
    /// <list type="bullet">
    /// <item>a view the plan names and the drawing no longer has ⇒ the propagation would be partial;</item>
    /// <item>a view the drawing has and the plan does not name ⇒ that sibling keeps the OLD value, and the
    /// rack ends up divergent — the precise defect ID22A exists to prevent.</item>
    /// </list>
    /// <para>
    /// So the answer is all or nothing. It is PURE — it compares identifiers, never blocks — which is what
    /// lets the promise be proven in the Core suite instead of asserted about a drawing nobody can load.
    /// </para>
    /// </summary>
    public static class MutationDestinationBinding
    {
        /// <summary>
        /// <paramref name="presentDefinitionIds"/> is what the drawing says it has for THIS rack right now.
        /// Whoever measured it names the definitions physically; this decides what the difference means.
        /// </summary>
        public static DestinationBindingResult Bind(
            string rackId,
            IReadOnlyList<ProjectVariableScanEntry> destinations,
            IReadOnlyList<string> presentDefinitionIds)
        {
            if (destinations == null || destinations.Count == 0)
            {
                return DestinationBindingResult.Abort(
                    "El plan no nombra ninguna vista del rack " + rackId + ", así que no hay nada que escribir " +
                    "y no hay forma de saber si el rack quedaría propagado. La operación se cancela.");
            }

            var planned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var destination in destinations)
            {
                if (destination == null || string.IsNullOrWhiteSpace(destination.DefinitionId))
                {
                    return DestinationBindingResult.Abort(
                        "El plan del rack " + rackId + " incluye una vista sin identificador de definición, " +
                        "así que no se puede localizar en el dibujo. La operación se cancela sin tocar nada.");
                }

                if (!planned.Add(destination.DefinitionId))
                {
                    return DestinationBindingResult.Abort(
                        "El plan del rack " + rackId + " nombra dos veces la definición '" +
                        destination.DefinitionId + "'. Escribirla dos veces no es redundante: es escribirla " +
                        "una vez de más. La operación se cancela.");
                }
            }

            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (presentDefinitionIds != null)
            {
                foreach (var definitionId in presentDefinitionIds)
                {
                    if (string.IsNullOrWhiteSpace(definitionId))
                    {
                        return DestinationBindingResult.Abort(
                            "El dibujo tiene una definición del rack " + rackId + " que no sabe nombrar, así " +
                            "que no se puede demostrar que el plan la cubra. La operación se cancela.");
                    }

                    present.Add(definitionId);
                }
            }

            foreach (var definitionId in planned)
            {
                if (!present.Contains(definitionId))
                {
                    return DestinationBindingResult.Abort(
                        "La vista '" + definitionId + "' del rack " + rackId + " ya no está en el dibujo, así " +
                        "que la propagación quedaría a medias. La operación se cancela sin tocar nada.");
                }
            }

            foreach (var definitionId in present)
            {
                if (!planned.Contains(definitionId))
                {
                    // La dirección que parece inofensiva y no lo es: esa vista no se reescribiría y se
                    // quedaría mostrando el valor anterior.
                    return DestinationBindingResult.Abort(
                        "El dibujo tiene una vista del rack " + rackId + " —la definición '" + definitionId +
                        "'— que el plan no contempla, así que se quedaría con el valor anterior. La operación " +
                        "se cancela sin tocar nada.");
                }
            }

            return DestinationBindingResult.Bound();
        }
    }
}
