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
            return new BillOfMaterials(components);
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
