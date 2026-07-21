using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Single source of truth for the dynamic-block parameters Application applies to each catalog block, so
    /// the library manifest expects EXACTLY the grips the producers set — never a divergent hand-kept string
    /// list. Two sources, combined per <b>exact</b> (pieceId, view):
    /// <list type="number">
    ///   <item>the connection-layout slide params (<c>paramX</c>/<c>paramY</c>) of that same piece and view;</item>
    ///   <item>the role grips the builders write to <see cref="Headers.HeaderBlockInstance.DynamicParameters"/>
    ///         (LONGITUD of the rail/posts/separators, PERALTE, ALTURA of the pallet, SAQUE, …).</item>
    /// </list>
    /// The parameter NAMES come from the same domain constants the builders use
    /// (<see cref="SelectiveRackDefaults"/> / <see cref="SelectiveSafetyDefaults"/>), so a rename cannot make the
    /// manifest disagree with the producers. The per-(role, view) requirements MIRROR the builders (see the
    /// Systems/Headers builders); a regression guard cross-checks a real builder's output against this table.
    /// Pure: catalog data only, no geometry, no product-rule change.
    /// </summary>
    public static class CatalogBlockParameters
    {
        private const string Frontal = "FRONTAL";
        private const string Lateral = "LATERAL";
        private const string Planta = "PLANTA";

        /// <summary>Role code of the rail row in <c>flow-bed-profiles.csv</c> (the only flow-bed piece with a LONGITUD grip).</summary>
        private const string RailRole = "RIEL";

        /// <summary>The dynamic parameter names Application applies to <paramref name="pieceId"/>'s block in
        /// <paramref name="view"/>. Case-insensitive set; empty when the block carries no dynamic parameters.</summary>
        public static ISet<string> ExpectedParameters(RackCatalog catalog, string pieceId, string view)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (catalog == null || string.IsNullOrWhiteSpace(pieceId) || string.IsNullOrWhiteSpace(view))
            {
                return result;
            }

            var piece = pieceId.Trim();
            var viewName = view.Trim();

            // 1. Layout slide params for THIS exact piece+view only (never a different view of the same piece).
            foreach (var row in catalog.ConnectionLayout ?? Enumerable.Empty<ConnectionLayoutEntry>())
            {
                if (row == null
                    || !string.Equals((row.PieceId ?? string.Empty).Trim(), piece, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals((row.View ?? string.Empty).Trim(), viewName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(row.ParamX))
                {
                    result.Add(row.ParamX.Trim());
                }

                if (!string.IsNullOrWhiteSpace(row.ParamY))
                {
                    result.Add(row.ParamY.Trim());
                }
            }

            // 2. Role grips the builders apply that the layout does not carry (LONGITUD, ALTURA, SAQUE, ...).
            AddRoleGrips(result, Classify(catalog, piece), viewName);

            return result;
        }

        private enum ParamRole
        {
            None,
            Post,
            Plate,
            Beam,
            Truss,
            Separator,
            Rail,
            Pallet,
            Tope,
            Parrilla,

            /// <summary>Desviador / guía / defensa: a single stretched LONGITUD in every view drawn.</summary>
            StretchLongitud
        }

        private static void AddRoleGrips(ISet<string> result, ParamRole role, string view)
        {
            var length = SelectiveRackDefaults.LengthParam;      // LONGITUD
            var peralte = SelectiveRackDefaults.PeralteParam;     // PERALTE
            var alto = SelectiveRackDefaults.PalletAltoParam;     // ALTURA
            var saque = SelectiveSafetyDefaults.SaqueParam;       // SAQUE

            switch (role)
            {
                case ParamRole.Post:
                    if (IsFrontal(view) || IsLateral(view)) result.Add(length);
                    if (IsFrontal(view) || IsPlanta(view)) result.Add(peralte);
                    break;
                case ParamRole.Plate:
                    result.Add(peralte);
                    break;
                case ParamRole.Beam:
                    if (IsFrontal(view) || IsPlanta(view)) result.Add(length);
                    result.Add(peralte);
                    break;
                case ParamRole.Truss:
                    if (IsLateral(view) || IsPlanta(view)) result.Add(length);
                    if (IsPlanta(view)) result.Add(peralte);
                    break;
                case ParamRole.Separator:
                    if (IsLateral(view) || IsPlanta(view)) result.Add(length);
                    break;
                case ParamRole.Rail:
                    if (IsLateral(view)) result.Add(length);
                    break;
                case ParamRole.Pallet:
                    if (IsFrontal(view) || IsLateral(view))
                    {
                        result.Add(length);
                        result.Add(alto);
                    }

                    break;
                case ParamRole.Tope:
                    result.Add(saque);
                    if (IsFrontal(view) || IsPlanta(view)) result.Add(length);
                    break;
                case ParamRole.Parrilla:
                    if (IsFrontal(view)) result.Add(SelectiveSafetyDefaults.ParrillaFrenteParam);
                    if (IsLateral(view)) result.Add(SelectiveSafetyDefaults.ParrillaFondoParam);
                    break;
                case ParamRole.StretchLongitud:
                    result.Add(length);
                    break;
            }
        }

        private static ParamRole Classify(RackCatalog catalog, string pieceId)
        {
            if (string.Equals(pieceId, SelectiveRackDefaults.PalletPieceId, StringComparison.OrdinalIgnoreCase))
            {
                return ParamRole.Pallet;
            }

            if (IsRailPiece(catalog, pieceId)) return ParamRole.Rail;
            if (ContainsId(catalog.PostProfiles, pieceId)) return ParamRole.Post;
            if (ContainsId(catalog.TrussProfiles, pieceId)) return ParamRole.Truss;
            if (ContainsId(catalog.SpacerProfiles, pieceId)) return ParamRole.Separator;
            if (ContainsId(catalog.BeamProfiles, pieceId)) return ParamRole.Beam;
            if (ContainsId(catalog.BasePlates, pieceId)) return ParamRole.Plate;

            return ClassifySafety(catalog, pieceId);
        }

        private static ParamRole ClassifySafety(RackCatalog catalog, string pieceId)
        {
            var safety = (catalog.SafetyElements ?? Enumerable.Empty<SafetyElementCatalogEntry>())
                .FirstOrDefault(entry => entry != null
                    && string.Equals((entry.Id ?? string.Empty).Trim(), pieceId, StringComparison.OrdinalIgnoreCase));

            if (safety == null)
            {
                return ParamRole.None;
            }

            var type = safety.Type ?? string.Empty;

            if (SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.TopeType)) return ParamRole.Tope;
            if (SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.ParrillaType)) return ParamRole.Parrilla;
            if (SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.DesviadorType)
                || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.GuiaType)
                || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.DefensaType))
            {
                return ParamRole.StretchLongitud;
            }

            // BOTA / LATERAL protectors are fixed blocks: Application applies no dynamic parameter.
            return ParamRole.None;
        }

        private static bool IsRailPiece(RackCatalog catalog, string pieceId)
            => (catalog.FlowBedProfiles ?? Enumerable.Empty<FlowBedComponentCatalogEntry>())
                .Any(component => component != null
                    && string.Equals((component.Id ?? string.Empty).Trim(), pieceId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals((component.Role ?? string.Empty).Trim(), RailRole, StringComparison.OrdinalIgnoreCase));

        private static bool ContainsId<TEntry>(IEnumerable<TEntry> entries, string id) where TEntry : CatalogEntryBase
            => (entries ?? Enumerable.Empty<TEntry>())
                .Any(entry => entry != null && string.Equals((entry.Id ?? string.Empty).Trim(), id, StringComparison.OrdinalIgnoreCase));

        private static bool IsFrontal(string view) => string.Equals(view, Frontal, StringComparison.OrdinalIgnoreCase);

        private static bool IsLateral(string view) => string.Equals(view, Lateral, StringComparison.OrdinalIgnoreCase);

        private static bool IsPlanta(string view) => string.Equals(view, Planta, StringComparison.OrdinalIgnoreCase);
    }
}
