using System;
using System.Globalization;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// Pure authority for the historical generated-view base string. Collision lookup and suffixes remain in Plugin;
    /// library-piece keys never enter this API.
    /// </summary>
    public static class RackViewBaseName
    {
        private const string CantileverPrefix = "RACKCAD_CANTILEVER_";

        public static string SelectiveFrontal(SelectiveRackSystem system, string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Selectivo frontal - {0} frentes - H{1:0.##}",
                    system?.Bays?.Count ?? 0,
                    system?.Height ?? 0.0)
                : rackName.Trim());

        public static string SelectivePlanta(SelectiveRackSystem system, string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Selectivo planta - {0} frentes",
                    system?.Bays?.Count ?? 0)
                : rackName.Trim() + " - planta");

        public static string DynamicLateral(DynamicRackSystem system, string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Sistema dinamico - {0} fondos - L{1:0.##}",
                    system?.PalletsDeep ?? 0,
                    system?.TotalLength ?? 0.0)
                : rackName.Trim());

        public static string DynamicFrontal(DynamicRackSystem system, string rackName, RackFlowEnd end)
        {
            var suffix = end == RackFlowEnd.Entrance ? "frontal entrada" : "frontal salida";
            return Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Dinamico {0} - {1} frentes",
                    suffix,
                    system?.Fronts?.Count ?? 0)
                : rackName.Trim() + " - " + suffix);
        }

        public static string DynamicPlanta(DynamicRackSystem system, string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Dinamico planta - {0} frentes",
                    system?.Fronts?.Count ?? 0)
                : rackName.Trim() + " - planta");

        public static string PushBackLateral(PushBackSystem system, string rackName, int postIndex)
        {
            var section = postIndex >= 0
                ? " - lateral " + (postIndex + 1).ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            return Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Push Back{0} - {1} frentes",
                    section,
                    system?.Fronts?.Count ?? 0)
                : rackName.Trim() + section);
        }

        public static string PushBackFrontal(
            PushBackSystem system,
            string rackName,
            RackPushBackEnd end,
            RackPushBackSide side)
        {
            var suffix = end == RackPushBackEnd.Posterior ? "frontal posterior" : "frontal entrada-salida";
            if (system != null && system.IsComposite)
            {
                suffix += side == RackPushBackSide.B ? " B" : " A";
            }

            return Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Push Back {0} - {1} frentes",
                    suffix,
                    system?.Fronts?.Count ?? 0)
                : rackName.Trim() + " - " + suffix);
        }

        public static string PushBackPlanta(PushBackSystem system, string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Push Back planta - {0} frentes",
                    system?.Fronts?.Count ?? 0)
                : rackName.Trim() + " - planta");

        public static string CabeceraPlanta(string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName) ? "Cabecera planta" : rackName.Trim() + " - planta");

        public static string CabeceraLateral(
            RackCatalog catalog,
            RackFrameConfiguration configuration,
            string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName)) return Standard(rackName.Trim());
            var post = BlockNaming.NormalizeWhitespace(catalog?.DescribeId(configuration?.LeftPost?.PostCatalogId));
            if (string.IsNullOrWhiteSpace(post)) post = "cabecera";
            return Standard(string.Format(
                CultureInfo.InvariantCulture,
                "Cabecera {0} - F{1:0.##} A{2:0.##}",
                post,
                configuration?.Depth ?? 0.0,
                configuration?.Height ?? 0.0));
        }

        public static string Cama(FlowBedConfiguration configuration, string rackName)
            => Standard(string.IsNullOrWhiteSpace(rackName)
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Cama {0} - fondo {1:0.##}",
                    configuration?.BedType == FlowBedType.Pushback ? "pushback" : "dinamica",
                    configuration?.LaneDepth ?? 0.0)
                : rackName.Trim());

        public static string CantileverViewLabel(string baseName, CantileverViewKind kind, int station)
        {
            if (baseName == null) return null;
            switch (kind)
            {
                case CantileverViewKind.Lateral:
                    return baseName + " - lateral " + (station + 1).ToString(CultureInfo.InvariantCulture);
                case CantileverViewKind.Planta:
                    return baseName + " - planta";
                default:
                    return baseName + " - frontal";
            }
        }

        public static string LinkedBase(string baseName)
            => string.IsNullOrWhiteSpace(baseName) ? null : Standard(baseName.Trim());

        public static string LinkedPlanta(string baseName)
            => string.IsNullOrWhiteSpace(baseName) ? null : Standard(baseName.Trim() + " - planta");

        public static string LinkedLateral(string baseName, int zeroBasedIndex)
            => string.IsNullOrWhiteSpace(baseName)
                ? null
                : Standard(baseName.Trim() + " - lateral "
                    + (zeroBasedIndex + 1).ToString(CultureInfo.InvariantCulture));

        public static string LinkedFlowEnd(string baseName, RackFlowEnd end)
            => string.IsNullOrWhiteSpace(baseName)
                ? null
                : Standard(baseName.Trim() + (end == RackFlowEnd.Entrance
                    ? " - frontal entrada"
                    : " - frontal salida"));

        public static string LinkedPushBackCut(
            string baseName,
            RackPushBackEnd end,
            RackPushBackSide side,
            bool includeSide)
        {
            if (string.IsNullOrWhiteSpace(baseName)) return null;
            var suffix = end == RackPushBackEnd.Posterior
                ? " - frontal posterior"
                : " - frontal entrada-salida";
            if (includeSide) suffix += side == RackPushBackSide.B ? " B" : " A";
            return Standard(baseName.Trim() + suffix);
        }

        public static string LinkedSelectiveFrontal(string baseName, int fondo, int fondoCount)
            => string.IsNullOrWhiteSpace(baseName)
                ? null
                : Standard(fondoCount > 1
                    ? baseName.Trim() + " - frente F" + (fondo + 1).ToString(CultureInfo.InvariantCulture)
                    : baseName.Trim());

        public static string CantileverGenerated(CantileverViewKind kind, int station, string requestedName)
        {
            var name = string.IsNullOrWhiteSpace(requestedName) ? "LINEA" : requestedName.Trim();
            var suffix = kind == CantileverViewKind.Lateral && station >= 0
                ? "_E" + (station + 1).ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            return CantileverSanitize(CantileverPrefix + name + "_" + kind.ToString().ToUpperInvariant() + suffix);
        }

        private static string Standard(string value) => BlockNaming.SanitizeBlockName(value);

        private static string CantileverSanitize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                builder.Append(
                    ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z'
                    || ch >= '0' && ch <= '9' || ch == '_' || ch == '-'
                        ? ch
                        : '_');
            }

            return builder.ToString();
        }
    }
}
