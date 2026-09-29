using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// The lines RACKPROYECTAR prints. Plain text without accents (the Plugin convention). Every blocking report says
    /// that no point was asked and nothing was written, lists every offender in the stable order of the plan, and
    /// uses the remedy row the pure plan selected: it never answers RACKEDITAR to everything.
    /// </summary>
    public static class RackProjectionReport
    {
        private const string Prefix = "RackCad: RACKPROYECTAR ";

        public static IReadOnlyList<string> Snapshot(RackProjectionSnapshot snapshot)
        {
            var lines = new List<string>();
            if (snapshot != null)
            {
                lines.AddRange(snapshot.Notices);
            }

            var reason = snapshot?.Reason;
            switch (snapshot?.Failure)
            {
                case RackProjectionSnapshotFailure.NoSelection:
                    lines.Add(Prefix + "cancelado: no se selecciono nada. No se pidio ningun punto ni se escribio nada.");
                    break;
                case RackProjectionSnapshotFailure.NoRackMembers:
                    lines.Add(Prefix + "no encontro racks en la seleccion. No se pidio ningun punto ni se escribio nada.");
                    break;
                default:
                    lines.Add(Prefix + "no pudo leer el dibujo"
                        + (string.IsNullOrWhiteSpace(reason) ? "." : ": " + reason)
                        + " No se pidio ningun punto ni se escribio nada.");
                    break;
            }

            return lines;
        }

        public static IReadOnlyList<string> Blocked(RackGroupPlacementPlanResult result)
        {
            var lines = new List<string>
            {
                Prefix + "se detuvo en la etapa " + StageName(result.FailedStage)
                    + ". No se pidio ningun punto y no se escribio nada."
            };

            foreach (var diagnostic in result.Diagnostics)
            {
                lines.Add("  - " + Describe(diagnostic));
            }

            lines.AddRange(Warnings(result.Warnings));
            return lines;
        }

        public static IReadOnlyList<string> Warnings(IReadOnlyList<RackProjectionWarning> warnings)
            => warnings.Select(warning => "RackCad: aviso " + WarningName(warning.Code)
                + (string.IsNullOrWhiteSpace(warning.RackId) ? string.Empty : " (rack " + warning.RackId + ")")
                + ": " + warning.Detail).ToList();

        public static IReadOnlyList<string> PointFailed(string diagnostic)
            => new[]
            {
                Prefix + "fallo al pedir un punto"
                    + (string.IsNullOrWhiteSpace(diagnostic) ? "." : ": " + diagnostic)
                    + " No se escribio nada."
            };

        public static IReadOnlyList<string> Cancelled()
            => new[] { Prefix + "cancelado. No se escribio ni se importo nada de los racks." };

        public static IReadOnlyList<string> Prerequisites(
            IReadOnlyList<LibraryPieceRequirement> missing, RackProjectionLibraryObservation observation)
        {
            var cause = observation != null && observation.LibraryFileMissing
                ? "biblioteca no disponible"
                : "faltan bloques de biblioteca en el dibujo";
            var lines = new List<string>
            {
                Prefix + "no escribio nada: " + cause + ". No se creo ninguna definicion ni referencia de rack."
            };

            foreach (var requirement in missing)
            {
                lines.Add("  - pieza " + requirement.PieceId + " (" + requirement.ViewAddress + "): bloque '"
                    + requirement.LibraryKey + "'");
            }

            return lines;
        }

        public static IReadOnlyList<string> Placement(CommonTransformFailure failure)
            => new[]
            {
                Prefix + "no pudo componer la transformacion comun (" + failure + "). No se escribio nada."
            };

        public static IReadOnlyList<string> Materialization(RackProjectionMaterializationResult result)
        {
            var lines = new List<string>
            {
                Prefix + "fallo al escribir y se deshizo la operacion completa (" + result.Failure
                    + (string.IsNullOrWhiteSpace(result.RackId) ? string.Empty : ", rack " + result.RackId) + "): "
                    + (string.IsNullOrWhiteSpace(result.Diagnostic) ? "sin detalle" : result.Diagnostic)
                    + ". No se confirmo ninguna definicion ni referencia."
            };
            return lines;
        }

        public static IReadOnlyList<string> Completed(RackProjectionMaterializationResult result)
            => new[]
            {
                "RackCad: RACKPROYECTAR coloco " + result.References + " vista(s) en " + result.Definitions
                    + " definicion(es) nueva(s). Son vistas enlazadas del mismo rack, no copias: el BOM no cambia"
                    + " (para copiar, RACKDUPLICAR)."
            };

        private static string Describe(RackProjectionDiagnostic diagnostic)
        {
            var subject = string.IsNullOrWhiteSpace(diagnostic.RackId) ? "vista sin identidad" : "rack " + diagnostic.RackId;
            var view = string.IsNullOrWhiteSpace(diagnostic.PhysicalKey) ? string.Empty : ", vista " + diagnostic.PhysicalKey;
            var address = diagnostic.Address.HasValue ? " (" + diagnostic.Address.Value + ")" : string.Empty;
            var piece = diagnostic.Piece == null
                ? string.Empty
                : " [pieza " + diagnostic.Piece.PieceId + ", bloque '" + diagnostic.Piece.LibraryKey + "']";
            var remedy = diagnostic.Remedy == null || string.IsNullOrWhiteSpace(diagnostic.Remedy.Message)
                ? string.Empty
                : " " + diagnostic.Remedy.Message;
            return subject + view + address + ": " + diagnostic.Code + piece + "." + remedy;
        }

        private static string StageName(RackProjectionStage stage)
            => stage.ToString().ToUpperInvariant();

        private static string WarningName(RackProjectionWarningCode code)
        {
            switch (code)
            {
                case RackProjectionWarningCode.Overlap:
                    return "de superposicion";
                case RackProjectionWarningCode.DirectionWindowNearLimit:
                    return "de cercania al limite angular";
                default:
                    return "de pieza visual opcional";
            }
        }
    }
}
