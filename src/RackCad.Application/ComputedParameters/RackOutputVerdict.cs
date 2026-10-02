using System;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Resultado de la puerta de salida (I-63 D-27).</summary>
    public enum RackOutputVerdictKind
    {
        Allow = 1,
        Deny = 2,
        Undetermined = 3,
    }

    /// <summary>Razones cerradas de un veredicto <c>Undetermined</c> (D-27).</summary>
    public enum RackOutputUndeterminedReason
    {
        DesignUnreadable = 1,
        CatalogUnavailable = 2,
        ResolveFailed = 3,
    }

    /// <summary><c>Allow | Deny(motivo) | Undetermined(razon)</c>. El motivo del <c>Deny</c> lo redacta <see cref="RackBomOutputGate"/>.</summary>
    public sealed class RackOutputDecision
    {
        private RackOutputDecision(
            RackOutputVerdictKind kind, string denyReason, RackOutputUndeterminedReason? undeterminedReason)
        {
            Kind = kind;
            DenyReason = denyReason;
            UndeterminedReason = undeterminedReason;
        }

        public RackOutputVerdictKind Kind { get; }

        /// <summary>El motivo de bloqueo. Solo con <see cref="RackOutputVerdictKind.Deny"/>.</summary>
        public string DenyReason { get; }

        /// <summary>La razon de indeterminacion. Solo con <see cref="RackOutputVerdictKind.Undetermined"/>.</summary>
        public RackOutputUndeterminedReason? UndeterminedReason { get; }

        public static RackOutputDecision Allow { get; } = new RackOutputDecision(RackOutputVerdictKind.Allow, null, null);

        public static RackOutputDecision Deny(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Un Deny lleva su motivo.", nameof(reason));
            }

            return new RackOutputDecision(RackOutputVerdictKind.Deny, reason, null);
        }

        public static RackOutputDecision Undetermined(RackOutputUndeterminedReason reason)
            => new RackOutputDecision(RackOutputVerdictKind.Undetermined, null, reason);

        public override string ToString()
            => Kind == RackOutputVerdictKind.Deny ? "Deny(" + DenyReason + ")"
                : Kind == RackOutputVerdictKind.Undetermined ? "Undetermined(" + UndeterminedReason + ")"
                : Kind.ToString();
    }

    /// <summary>
    /// La autoridad UNICA de la puerta de salida (I-63 D-27, E6): es la unica composicion de
    /// <c>PushBackResolver</c> y <see cref="RackBomOutputGate"/> para decidir si un rack puede cotizarse. La consumen
    /// la poblacion de I-63 (<see cref="Evaluate"/>) y el handler de Push Back (<see cref="HandlerBlockedReason"/>).
    /// </summary>
    public static class RackOutputVerdict
    {
        /// <summary>
        /// <c>RackOutputVerdict(kindToken, designJson, CatalogInput)</c>. Solo Push Back lee el diseno; los demas kinds
        /// son <c>Allow</c> sin leer nada. Una excepcion del <i>store</i> es «ilegible» (D-26); una del resolver o de
        /// la puerta es <c>ResolveFailed</c>.
        /// </summary>
        public static RackOutputDecision Evaluate(string kindToken, string designJson, RackCatalogInput catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (!string.Equals(kindToken, RackEmbedDocument.KindPushBack, StringComparison.Ordinal))
            {
                return RackOutputDecision.Allow;
            }

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(designJson);
            }
            catch (Exception)
            {
                return RackOutputDecision.Undetermined(RackOutputUndeterminedReason.DesignUnreadable);
            }

            if (project?.PushBackDesign == null)
            {
                return RackOutputDecision.Undetermined(RackOutputUndeterminedReason.DesignUnreadable);
            }

            if (!catalog.IsLoaded)
            {
                return RackOutputDecision.Undetermined(RackOutputUndeterminedReason.CatalogUnavailable);
            }

            try
            {
                var system = new PushBackResolver(catalog.Catalog).Resolve(project.PushBackDesign);
                var reason = RackBomOutputGate.For(system).Reason;
                return reason == null ? RackOutputDecision.Allow : RackOutputDecision.Deny(reason);
            }
            catch (Exception)
            {
                return RackOutputDecision.Undetermined(RackOutputUndeterminedReason.ResolveFailed);
            }
        }

        /// <summary>
        /// La via del handler (D-27): normaliza <c>catalog == null</c> a un catalogo VACIO cargado, como hacia
        /// <c>PushBackResolver</c> con <c>catalog ?? new RackCatalog()</c>, y corresponde <c>Deny(m) → m</c>,
        /// <c>Allow → null</c> y <c>Undetermined → null</c> (<i>fail-open</i>). Nunca produce <c>LoadFailed</c>.
        /// </summary>
        public static string HandlerBlockedReason(string kindToken, string designJson, RackCatalog catalog)
        {
            var decision = Evaluate(kindToken, designJson, RackCatalogInput.Loaded(catalog ?? new RackCatalog()));
            return decision.Kind == RackOutputVerdictKind.Deny ? decision.DenyReason : null;
        }
    }
}
