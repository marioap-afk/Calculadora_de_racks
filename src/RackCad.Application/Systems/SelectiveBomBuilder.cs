using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Bill of materials for a RESOLVED selective rack, expressed by COMPONENT: every frame position (and medio-frente
    /// intermediate post) is a <b>cabecera</b> (a full frame — 2 posts + base plates + celosía, via <see cref="BomBuilder"/>,
    /// the same module the standalone cabecera uses), and every load beam is a <b>larguero</b> component (its profile +
    /// 2 ménsulas). Each component expands to its per-unit pieces; <see cref="BillOfMaterials"/> flattens ×quantity for the
    /// piece total. Cabecera frames already include front AND back posts, so they are counted once per frame; largueros
    /// double for the front + back of each bay. Every fondo contributes its OWN content (doble profundidad). Pure; no AutoCAD.
    /// </summary>
    public static class SelectiveBomBuilder
    {
        public const string Post = "Poste";
        public const string BasePlate = "Placa base";
        public const string Beam = "Larguero";
        public const string Mensula = "Ménsula";
        public const string Safety = "Seguridad";
        public const string Separador = "Separador";
        public const string Tope = "Tope";
        public const string Parrilla = "Parrilla";

        /// <summary>The component BOM of a resolved system: cabeceras (per frame) + largueros (per beam), each with its pieces.</summary>
        public static BillOfMaterials Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            if (system == null || system.Bays.Count == 0)
            {
                return new BillOfMaterials(new List<BomComponent>());
            }

            var components = new List<BomComponent>();
            AddCabeceraComponents(components, system, catalog);
            AddLargueroComponents(components, system, catalog);
            AddSafetyComponents(components, system, catalog);
            AddDesviadorComponents(components, system, catalog);
            AddSeparadorComponents(components, system, catalog);
            AddTopeComponents(components, system, catalog);
            AddParrillaComponents(components, system, catalog);
            return new BillOfMaterials(components);
        }

        // ---- Safety accessories: one component per element (the bota itself IS the component), counted from the drawing ----

        private static void AddSafetyComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            if (system.SafetySelections == null || system.SafetySelections.Count == 0)
            {
                return;
            }

            // Count what the drawing actually places, by piece id — "en base al dibujo sería el BOM". Botas are a
            // SYSTEM-level element (front + back of the whole depth, not per fondo), so count them from the PLANTA, which
            // carries that placement. An element that draws nothing yet (no block/rule) falls back to its manual quantity.
            var drawn = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var drawnLongitud = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var instance in new SelectivePlantaBuilder().Build(system, catalog))
            {
                if (instance.Role != HeaderBlockRole.Safety || string.IsNullOrWhiteSpace(instance.PieceId))
                {
                    continue;
                }

                drawn[instance.PieceId] = drawn.TryGetValue(instance.PieceId, out var count) ? count + 1 : 1;
                if (instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var lp) && lp > 0.0)
                {
                    drawnLongitud[instance.PieceId] = lp; // the LONGITUD the lateral was drawn with (= the fondo)
                }
            }

            // Each safety element is its OWN component named after the piece (not wrapped under a generic node).
            foreach (var selection in system.SafetySelections)
            {
                if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
                {
                    continue;
                }

                var element = catalog?.SafetyElements?.FirstOrDefault(s => string.Equals(s?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));

                // Grid-driven families are counted by their pure placement plans, not from the collapsed PLANTA view.
                if (element != null
                    && (SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyPlacement.TopeType)
                        || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyPlacement.ParrillaType)
                        || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DesviadorType)))
                {
                    continue;
                }

                // A DRAWABLE element (bota/lateral) is counted ONLY from the drawing — 0 if it draws nothing (e.g. a bota
                // fully replaced by laterales). The manual quantity is the fallback ONLY for elements with no draw rule yet.
                var drawable = element != null
                    && (SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyPlacement.BotaType)
                        || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyPlacement.LateralType));
                var quantity = drawable
                    ? (drawn.TryGetValue(selection.ElementId, out var drawnCount) ? drawnCount : 0)
                    : selection.Quantity;
                if (quantity <= 0)
                {
                    continue;
                }

                var label = element?.Label ?? selection.ElementId;

                // A protector lateral reports a LENGTH = its drawn LONGITUD (the fondo) + the guide overhang; a bota has none.
                var length = 0.0;
                var isLateral = element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyPlacement.LateralType);
                if (isLateral && drawnLongitud.TryGetValue(selection.ElementId, out var longitud))
                {
                    length = longitud + SelectiveSafetyPlacement.LateralLengthAllowance;
                }

                components.Add(new BomComponent
                {
                    Category = Safety,
                    ProfileId = selection.ElementId,
                    Description = label,
                    Length = length,
                    Quantity = quantity,
                    Pieces = new List<BomLine>
                    {
                        new BomLine { Category = Safety, ProfileId = selection.ElementId, Description = label, Length = length, Quantity = 1 }
                    }
                });
            }
        }

        private static void AddDesviadorComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system?.SafetySelections,
                catalog?.SafetyElements,
                SelectiveSafetyDefaults.DesviadorType);
            if (selection == null)
            {
                return;
            }

            var plan = SelectiveDesviadorPlan.Build(system, catalog, selection);
            if (plan.PhysicalQuantity <= 0)
            {
                return;
            }

            var label = catalog?.SafetyElements?.FirstOrDefault(s => string.Equals(s?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase))?.Label
                        ?? selection.ElementId;
            components.Add(new BomComponent
            {
                Category = Safety,
                ProfileId = selection.ElementId,
                Description = label,
                Length = plan.Longitud,
                Quantity = plan.PhysicalQuantity,
                Pieces = new List<BomLine>
                {
                    new BomLine
                    {
                        Category = Safety,
                        ProfileId = selection.ElementId,
                        Description = label,
                        Length = plan.Longitud,
                        Quantity = 1
                    }
                }
            });
        }

        // ---- Separadores (doble profundidad): the drawn spacer beams, one component per distinct length ----

        private static void AddSeparadorComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            // Count from the LATERAL cortes — that view carries the full vertical stack (the planta collapses it to one
            // line per frente). Group by LONGITUD (the fondo gap): different gaps are different cut lengths.
            var byLength = new Dictionary<double, int>();
            var order = new List<double>();
            foreach (var corte in new SelectiveLateralBuilder().Cortes(system, catalog))
            {
                foreach (var instance in corte.Largueros)
                {
                    if (instance.Role != HeaderBlockRole.Separator)
                    {
                        continue;
                    }

                    var length = Round(Param(instance, SelectiveRackDefaults.LengthParam));
                    if (!byLength.ContainsKey(length))
                    {
                        byLength[length] = 0;
                        order.Add(length);
                    }

                    byLength[length]++;
                }
            }

            var id = DynamicRackDefaults.SeparatorCatalogId;
            var profile = catalog?.SpacerProfiles?.FirstOrDefault(s => string.Equals(s?.Id, id, StringComparison.OrdinalIgnoreCase));
            var label = profile?.Label ?? id;
            foreach (var length in order.OrderBy(l => l))
            {
                var quantity = byLength[length];
                components.Add(new BomComponent
                {
                    Category = Separador,
                    ProfileId = id,
                    Description = label,
                    Length = length,
                    Quantity = quantity,
                    Pieces = new List<BomLine>
                    {
                        new BomLine { Category = Separador, ProfileId = id, Description = label, Length = length, Quantity = 1 }
                    }
                });
            }
        }

        // ---- Larguero topes: one per (bay, level) at the central fondo's back; grouped by length ----

        private static void AddTopeComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            var topes = SelectiveSafetyPlacement.EnabledOfType(system, catalog, SelectiveLateralBuilder.LateralView, SelectiveSafetyPlacement.TopeType);
            if (topes.Count == 0)
            {
                return;
            }

            // One tope per larguero (bay × level, per loaded tramo of a medio-frente bay) at the CENTRAL fondo — counted
            // from the model, matching the per-tramo drawing (the lateral shows each end-on, the planta splits by tramo).
            var selection = topes[0].Selection;
            var fondoCount = SelectiveDepthLayout.Count(system);
            var troquelXs = SelectiveDepthLayout.MasterGrid(system, catalog).TroquelXs;
            var offCells = SelectiveSafetyGrid.OffCellKeys(selection.TopeOffCells);
            var byLength = new Dictionary<double, int>();
            var order = new List<double>();
            foreach (var spot in SelectiveSafetyPlacement.TopeSpots(selection, fondoCount))
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, spot.Fondo);
                for (var b = 0; b < bays.Count; b++)
                {
                    var frente = b;
                    TallyByTramo(bays[b], troquelXs, b, catalog, lvl => !offCells.Contains((frente, lvl)), SelectiveSafetyPlacement.TopeLengthAllowance, byLength, order);
                }
            }

            if (byLength.Count == 0)
            {
                return;
            }

            var id = topes[0].PieceId;
            var label = catalog?.SafetyElements?.FirstOrDefault(s => string.Equals(s?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
            foreach (var length in order.OrderBy(l => l))
            {
                var quantity = byLength[length];
                components.Add(new BomComponent
                {
                    Category = Tope,
                    ProfileId = id,
                    Description = label,
                    Length = length,
                    Quantity = quantity,
                    Pieces = new List<BomLine>
                    {
                        new BomLine { Category = Tope, ProfileId = id, Description = label, Length = length, Quantity = 1 }
                    }
                });
            }
        }

        // ---- Parrillas (decks): one per (frente, level) grid-ON cell on every fondo, grouped by the frente width ----

        private static void AddParrillaComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            if (system.SafetySelections == null)
            {
                return;
            }

            SelectiveSafetySelection selection = null;
            foreach (var sel in system.SafetySelections)
            {
                if (sel == null || string.IsNullOrWhiteSpace(sel.ElementId))
                {
                    continue;
                }

                var el = catalog?.SafetyElements?.FirstOrDefault(s => string.Equals(s?.Id, sel.ElementId, StringComparison.OrdinalIgnoreCase));
                if (el != null && SelectiveSafetyDefaults.IsType(el.Type, SelectiveSafetyPlacement.ParrillaType))
                {
                    selection = sel;
                    break;
                }
            }

            if (selection == null)
            {
                return;
            }

            // ONE deck per tarima (the level's frente/count, or the manual override width), on every fondo's grid-ON
            // cells, grouped by the deck FRENTE — exactly the rows SelectiveFrontalBuilder draws, so the BOM agrees.
            var byLength = new Dictionary<double, int>();
            var order = new List<double>();
            var troquelXs = SelectiveDepthLayout.MasterGrid(system, catalog).TroquelXs;
            var overrideFrente = selection.ParrillaFrente;
            var overrideCount = selection.ParrillaCantidad;
            var offCells = SelectiveSafetyGrid.OffCellKeys(selection.ParrillaOffCells);
            var fondoCount = SelectiveDepthLayout.Count(system);

            void AddRow(double span, SelectiveLevel level, bool fitToSpan)
            {
                var (frente, count) = SelectiveFrontalBuilder.ParrillaRow(span, level.PalletFrente, level.PalletCount, overrideFrente, overrideCount, fitToSpan);
                if (frente <= 0.0 || count <= 0)
                {
                    return;
                }

                var key = Round(frente);
                if (!byLength.ContainsKey(key))
                {
                    byLength[key] = 0;
                    order.Add(key);
                }

                byLength[key] += count;
            }

            for (var k = 0; k < fondoCount; k++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                for (var b = 0; b < bays.Count; b++)
                {
                    var bay = bays[b];
                    if (bay.BeamLength <= 0.0 || bay.Levels.Count == 0)
                    {
                        continue;
                    }

                    var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);
                    var troquelX = b >= 0 && b < troquelXs.Count ? troquelXs[b] : 0.0;
                    var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);
                    for (var lvl = 0; lvl < bay.Levels.Count; lvl++)
                    {
                        if (offCells.Contains((b, lvl)))
                        {
                            continue;
                        }

                        var level = bay.Levels[lvl];
                        if (tramos == null)
                        {
                            AddRow(bay.BeamLength, level, false);
                            continue;
                        }

                        foreach (var tramo in tramos)
                        {
                            if (tramo.Loaded) AddRow(tramo.Length, level, true);
                        }
                    }
                }
            }

            if (byLength.Count == 0)
            {
                return;
            }

            var id = selection.ElementId;
            var label = catalog?.SafetyElements?.FirstOrDefault(s => string.Equals(s?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
            foreach (var length in order.OrderBy(l => l))
            {
                components.Add(new BomComponent
                {
                    Category = Parrilla,
                    ProfileId = id,
                    Description = label,
                    Length = length,
                    Quantity = byLength[length],
                    Pieces = new List<BomLine>
                    {
                        new BomLine { Category = Parrilla, ProfileId = id, Description = label, Length = length, Quantity = 1 }
                    }
                });
            }
        }

        /// <summary>Tally, for one bay, ONE unit per (level that <paramref name="levelOn"/> accepts × loaded tramo) into
        /// <paramref name="byLength"/>, keyed by the piece length (the tramo length in a medio-frente bay, else the whole
        /// bay) + <paramref name="allowance"/>. Shared by the tope and parrilla BOM so both agree with the per-tramo
        /// drawing of a split bay. <paramref name="troquelXs"/> are the shared master-grid post troqueles.</summary>
        private static void TallyByTramo(SelectiveBay bay, IReadOnlyList<double> troquelXs, int bayIndex, RackCatalog catalog, Func<int, bool> levelOn, double allowance, Dictionary<double, int> byLength, List<double> order)
        {
            if (bay.BeamLength <= 0.0 || bay.Levels.Count == 0)
            {
                return;
            }

            var onLevels = 0;
            for (var lvl = 0; lvl < bay.Levels.Count; lvl++)
            {
                if (levelOn(lvl)) onLevels++;
            }

            if (onLevels == 0)
            {
                return;
            }

            void Add(double pieceLength)
            {
                var key = Round(pieceLength);
                if (!byLength.ContainsKey(key))
                {
                    byLength[key] = 0;
                    order.Add(key);
                }

                byLength[key] += onLevels; // one piece per accepted level of this tramo (the vertical stack)
            }

            var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);
            var troquelX = bayIndex >= 0 && bayIndex < troquelXs.Count ? troquelXs[bayIndex] : 0.0;
            var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);
            if (tramos == null)
            {
                Add(bay.BeamLength + allowance);
                return;
            }

            foreach (var tramo in tramos)
            {
                if (tramo.Loaded) Add(tramo.Length + allowance);
            }
        }

        // ---- Cabeceras: one component per distinct frame (grouped by its exact piece recipe) ----

        private static void AddCabeceraComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            // Each selective frame IS a cabecera; reuse the same component grouping the standalone/dynamic cabecera BOM uses.
            components.AddRange(BomBuilder.Components(EnumerateCabeceras(system, catalog), catalog));
        }

        /// <summary>Every cabecera (frame) in the system: per fondo, one per frame position (custom on fondo 0, else a
        /// standard frame at that fondo's height/depth) plus one per medio-frente tramo boundary. Mirrors the planta.</summary>
        private static IEnumerable<RackFrameConfiguration> EnumerateCabeceras(SelectiveRackSystem system, RackCatalog catalog)
        {
            var factory = new RackFrameConfigurationFactory(catalog);
            var memberBuilder = new BracingPanelMemberBuilder(); // materializes the celosía into Members so the BOM counts it
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var fondoCount = SelectiveDepthLayout.Count(system);
            var troquelXs = SelectiveDepthLayout.MasterGrid(system, catalog).TroquelXs; // for the medio-frente tramo layout

            for (var k = 0; k < fondoCount; k++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                var depthK = SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
                var fallbackK = FondoFallbackHeight(bays, system.Height);

                // Shared posts (frame positions): a fondo with C bays has C+1 posts.
                for (var i = 0; i < bays.Count + 1; i++)
                {
                    var custom = k == 0 && i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                    if (custom != null && custom.Height > 0.0)
                    {
                        // A custom cabecera restored from persistence carries Horizontals + BracingPanels but an EMPTY
                        // Members list (derived data is regenerated on load), so materialize its celosía too — otherwise
                        // its travesaños/diagonales are silently dropped from the BOM (unlike a standard cabecera).
                        if (custom.Members == null || custom.Members.Count == 0)
                        {
                            memberBuilder.RefreshPhysicalModel(custom);
                        }

                        yield return custom;
                    }
                    else
                    {
                        var height = SelectivePostGeometry.PostHeight(bays, i, fallbackK);
                        if (height > 0.0)
                        {
                            yield return StandardCabecera(factory, memberBuilder, template, system.PostId, height, depthK);
                        }
                    }
                }

                // Medio frente: each tramo boundary plants an intermediate cabecera (same height/depth as its bay).
                for (var i = 0; i < bays.Count && i < troquelXs.Count; i++)
                {
                    var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bays[i], SelectiveRackDefaults.View);
                    var tramos = SelectiveMedioFrente.Resolve(bays[i], troquelXs[i], inicioX);
                    if (tramos == null)
                    {
                        continue;
                    }

                    var height = bays[i].Height > 0.0 ? bays[i].Height : system.Height;
                    for (var t = 1; t < tramos.Count; t++)
                    {
                        yield return StandardCabecera(factory, memberBuilder, template, system.PostId, height, depthK);
                    }
                }
            }
        }

        /// <summary>A standard frame with its celosía materialized into Members (a fresh config, so it's safe to mutate),
        /// so <see cref="BomBuilder"/> counts the horizontals + diagonals — not just the posts + plates.</summary>
        private static RackFrameConfiguration StandardCabecera(
            RackFrameConfigurationFactory factory, BracingPanelMemberBuilder memberBuilder,
            RackFrameTemplate template, string postId, double height, double depth)
        {
            var cabecera = factory.Build(template, postId, height, depth);
            memberBuilder.RefreshPhysicalModel(cabecera);
            return cabecera;
        }

        // ---- Largueros: one component per distinct beam (profile + 2 ménsulas), doubled for front + back ----

        private static void AddLargueroComponents(List<BomComponent> components, SelectiveRackSystem system, RackCatalog catalog)
        {
            var frontalBuilder = new SelectiveFrontalBuilder();
            var quantities = new Dictionary<(string Id, double Length, double Peralte), int>();
            var order = new List<(string, double, double)>();

            var fondoCount = SelectiveDepthLayout.Count(system);
            for (var k = 0; k < fondoCount; k++)
            {
                foreach (var instance in frontalBuilder.Build(SelectiveDepthLayout.FondoSystemView(system, k), catalog))
                {
                    if (instance.Role != HeaderBlockRole.Beam)
                    {
                        continue;
                    }

                    var key = (instance.PieceId, Round(Param(instance, SelectiveRackDefaults.LengthParam)), Round(Param(instance, SelectiveRackDefaults.PeralteParam)));
                    if (!quantities.ContainsKey(key))
                    {
                        quantities[key] = 0;
                        order.Add(key);
                    }

                    quantities[key] += 2; // the frontal shows the front larguero; ×2 for the back one of the bay
                }
            }

            foreach (var key in order.OrderBy(k => k.Item2))
            {
                var (beamId, length, peralte) = key;
                // Reuse the single "one larguero = perfil + 2 ménsulas" recipe (also used by the standalone larguero editor).
                components.Add(LargueroBomBuilder.Component(catalog, beamId, length, peralte, mensulaOverride: null, quantity: quantities[key]));
            }
        }

        private static double FondoFallbackHeight(IList<SelectiveBay> bays, double systemHeight)
        {
            var h = 0.0;
            if (bays != null)
            {
                foreach (var bay in bays)
                {
                    if (bay.Height > h) h = bay.Height;
                }
            }

            return h > 0.0 ? h : systemHeight;
        }

        private static double Param(HeaderBlockInstance instance, string name)
            => instance.DynamicParameters.TryGetValue(name, out var value) ? value : 0.0;

        private static double Round(double value) => Math.Round(value, 2);
    }
}
