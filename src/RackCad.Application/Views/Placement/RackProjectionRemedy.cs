using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>Remedy families of Proposal V5 section 3.8. One row per frozen situation, never a default.</summary>
    public enum RackProjectionRemedyKind
    {
        None,
        UpdateFromAnotherView,
        UpdateThisView,
        PurgeAndReinsert,
        InsertAnotherView,
        UpdateFromPreferredView,
        UnifyProperties,
        FixDesignInEditor,
        RepairVariable,
        SectionCatalogUnavailable,
        UnreadableByThisBuild,
        FixInXrefSource,
        NotProjectable,
        NoAutomaticRemedy,
        ReviewSelection,
        LibraryBlocksMissing
    }

    /// <summary>A selected remedy: its frozen kind plus the plain message the Plugin prints (no accents).</summary>
    public sealed class RackProjectionRemedy
    {
        public RackProjectionRemedy(RackProjectionRemedyKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }

        public RackProjectionRemedyKind Kind { get; }
        public string Message { get; }

        public static RackProjectionRemedy None { get; } = new RackProjectionRemedy(RackProjectionRemedyKind.None, string.Empty);
    }

    /// <summary>
    /// Chooses the remedy from kind, disposition, availability and authority state. It never answers
    /// "abre con RACKEDITAR" for every failure.
    /// </summary>
    public static class RackProjectionRemedySelector
    {
        public static RackProjectionRemedy For(
            RackProjectionFailureCode code,
            RackSystemKind systemKind,
            bool hasAnotherValidSiblingView,
            bool isXrefDependent = false,
            string detail = null)
        {
            switch (code)
            {
                case RackProjectionFailureCode.BlankRackId:
                    return Remedy(
                        RackProjectionRemedyKind.NoAutomaticRemedy,
                        "Esta vista no tiene identidad de rack: no hay remedio automatico.");

                case RackProjectionFailureCode.UnsupportedMember:
                case RackProjectionFailureCode.MixedSourceTypes:
                case RackProjectionFailureCode.MultipleSourceDefinitions:
                    return Remedy(
                        RackProjectionRemedyKind.ReviewSelection,
                        "Revisa la seleccion: proyecta una sola clase de vista y una definicion por rack.");

                case RackProjectionFailureCode.AuthoredDivergent:
                case RackProjectionFailureCode.AuthoredUnreadable:
                    return Remedy(
                        RackProjectionRemedyKind.UpdateFromPreferredView,
                        "RACKEDITAR y Actualizar desde la vista cuyo diseno quieres conservar.");

                case RackProjectionFailureCode.PropertiesDivergent:
                case RackProjectionFailureCode.PropertiesUnreadable:
                    return Remedy(
                        RackProjectionRemedyKind.UnifyProperties,
                        "Unifica las propiedades con RACKPROPIEDADES; si las muestra en solo lectura, resuelve antes la causa que indica.");

                case RackProjectionFailureCode.ResolveOutputBlocking:
                    return Remedy(
                        RackProjectionRemedyKind.FixDesignInEditor,
                        Append("Corrige el diseno en su editor", detail));

                case RackProjectionFailureCode.ResolveBrokenReference:
                    return Remedy(
                        RackProjectionRemedyKind.RepairVariable,
                        Append("Repara la variable de proyecto con RACKVARIABLES", detail));

                case RackProjectionFailureCode.ResolveDependencyUnavailable:
                    return Remedy(
                        RackProjectionRemedyKind.SectionCatalogUnavailable,
                        Append("Catalogo de secciones no disponible", detail));

                case RackProjectionFailureCode.ResolveUnreadable:
                case RackProjectionFailureCode.ResolveFailed:
                    return Remedy(
                        RackProjectionRemedyKind.UnreadableByThisBuild,
                        "Esta version de RackCad no puede leer el rack.");

                case RackProjectionFailureCode.ResolveLegacyUnsupported:
                    return systemKind == RackSystemKind.PalletFlow
                        ? Remedy(
                            RackProjectionRemedyKind.UpdateThisView,
                            "RACKEDITAR y Actualizar reescribe el rack en la forma vigente, con los valores por defecto de los campos ausentes.")
                        : Remedy(
                            RackProjectionRemedyKind.NotProjectable,
                            "No se puede proyectar este rack legado.");

                case RackProjectionFailureCode.EditPreflightRejected:
                    if (isXrefDependent)
                    {
                        return Remedy(
                            RackProjectionRemedyKind.FixInXrefSource,
                            "Corrigela en el dibujo de origen de la referencia externa y recarga la referencia.");
                    }

                    if (systemKind == RackSystemKind.PushBack || systemKind == RackSystemKind.Cantilever)
                    {
                        return hasAnotherValidSiblingView
                            ? Remedy(
                                RackProjectionRemedyKind.PurgeAndReinsert,
                                "RACKEDITAR abortara mientras exista esta vista: borra sus referencias, purga su definicion y vuelve a insertarla desde otra vista del rack.")
                            : Remedy(
                                RackProjectionRemedyKind.NoAutomaticRemedy,
                                "Es la unica vista del rack y RACKEDITAR la rechaza: no hay remedio automatico.");
                    }

                    return hasAnotherValidSiblingView
                        ? Remedy(
                            RackProjectionRemedyKind.UpdateFromAnotherView,
                            "RACKEDITAR y Actualizar desde otra vista del rack la retira.")
                        : Remedy(
                            RackProjectionRemedyKind.NoAutomaticRemedy,
                            "Es la unica vista del rack: no hay remedio automatico.");

                case RackProjectionFailureCode.SourceAddressUnavailable:
                case RackProjectionFailureCode.TargetAddressUnavailable:
                    return hasAnotherValidSiblingView
                        ? Remedy(
                            RackProjectionRemedyKind.UpdateFromAnotherView,
                            "RACKEDITAR y Actualizar desde otra vista la retira como huerfana.")
                        : Remedy(
                            RackProjectionRemedyKind.InsertAnotherView,
                            "RACKEDITAR e Insertar otra vista del rack: la huerfana se retira al colocarla.");

                case RackProjectionFailureCode.RequiredKeyMissing:
                case RackProjectionFailureCode.RequiredBlockMissing:
                case RackProjectionFailureCode.RequiredLibraryMissing:
                case RackProjectionFailureCode.RequiredUnknownAvailability:
                case RackProjectionFailureCode.UnknownSourceRole:
                    return Remedy(
                        RackProjectionRemedyKind.LibraryBlocksMissing,
                        Append("Faltan bloques de biblioteca para la vista proyectada", detail));

                default:
                    return Remedy(
                        RackProjectionRemedyKind.NotProjectable,
                        Append("Esta vista no se puede proyectar", detail));
            }
        }

        public static RackProjectionRemedy ForPolicy(
            ProjectionPolicyDecision decision,
            RackSystemKind systemKind,
            bool hasAnotherValidSiblingView)
        {
            switch (decision.ReasonCode)
            {
                case ProjectionPolicyReasonCode.UnsupportedVariant:
                    return For(
                        RackProjectionFailureCode.TargetAddressUnavailable, systemKind, hasAnotherValidSiblingView);
                case ProjectionPolicyReasonCode.NotExposedForOperation:
                    return For(RackProjectionFailureCode.TargetNotExposed, systemKind, hasAnotherValidSiblingView);
                case ProjectionPolicyReasonCode.UnavailableForSystem:
                    return For(RackProjectionFailureCode.PairNotExposed, systemKind, hasAnotherValidSiblingView);
                default:
                    return For(RackProjectionFailureCode.TargetAddressUnavailable, systemKind, hasAnotherValidSiblingView);
            }
        }

        private static RackProjectionRemedy Remedy(RackProjectionRemedyKind kind, string message)
            => new RackProjectionRemedy(kind, message);

        private static string Append(string message, string detail)
            => string.IsNullOrWhiteSpace(detail) ? message + "." : message + ": " + detail + ".";
    }
}
