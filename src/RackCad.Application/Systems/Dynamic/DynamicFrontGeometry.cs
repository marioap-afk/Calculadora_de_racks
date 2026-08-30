using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>Shared transverse post grid for the dynamic frontal and planta views.</summary>
    public sealed class DynamicFrontLayout
    {
        public DynamicFrontLayout(IReadOnlyList<double> postPositions, IReadOnlyList<double> troquelPositions)
        {
            PostPositions = postPositions;
            TroquelPositions = troquelPositions;
        }

        public IReadOnlyList<double> PostPositions { get; }
        public IReadOnlyList<double> TroquelPositions { get; }
        public double TotalWidth => PostPositions.Count == 0 ? 0.0 : PostPositions[PostPositions.Count - 1];
    }

    /// <summary>
    /// Resolves dynamic front widths and their post grid. One lane has BFR = pallet front + 2 in; the complete
    /// IN/OUT cut is the sum of its lane BFR widths + 6 in, unless the front carries an explicit override. Post
    /// spacing then adds the catalog-driven hook/profile offsets at both ends of the IN/OUT beam.
    /// </summary>
    public static class DynamicFrontGeometry
    {
        public static double AutoBeamLength(double palletFront, int palletCount, double tolerance)
        {
            var count = Math.Max(1, palletCount);
            return Bfr(palletFront) * count + DynamicRackDefaults.InOutBeamLengthAllowance;
        }

        public static double Bfr(double palletFront)
            => Math.Max(0.0, palletFront) + DynamicRackDefaults.BfrAllowance;

        public static IReadOnlyList<DynamicRackFront> Resolve(
            IEnumerable<DynamicRackFrontDesign> designs,
            double palletFront,
            double tolerance,
            int defaultLoadLevels = DynamicRackDefaults.DefaultLoadLevels,
            int defaultPalletsDeep = DynamicRackDefaults.DefaultPalletsDeep)
        {
            var source = designs?.Where(front => front != null).ToList() ?? new List<DynamicRackFrontDesign>();
            if (source.Count == 0)
            {
                source.Add(new DynamicRackFrontDesign { PalletCount = DynamicRackDefaults.DefaultPalletsWide });
            }

            // The Activo/En blanco intent is carried through VERBATIM. An all-blank set is NOT normalized here (I-33):
            // the editor prevents reaching it, and the canonical check rejects it with a visible error at the resolver
            // and at RackDesignValidation. Silently reactivating a front here would hide the caller's mistake and make
            // this a second, divergent guard.
            var result = new List<DynamicRackFront>(source.Count);
            for (var index = 0; index < source.Count; index++)
            {
                var design = source[index];
                var count = Math.Max(1, design.PalletCount);
                var beamLength = design.BeamLengthOverride.HasValue && design.BeamLengthOverride.Value > 0.0
                    ? design.BeamLengthOverride.Value
                    : AutoBeamLength(palletFront, count, tolerance);
                var resolved = new DynamicRackFront
                {
                    Index = index,
                    IsActive = design.IsActive,
                    PalletCount = count,
                    LoadLevels = design.LoadLevels.HasValue && design.LoadLevels.Value > 0
                        ? design.LoadLevels.Value
                        : Math.Max(1, defaultLoadLevels),
                    PalletsDeep = design.PalletsDeep.HasValue && design.PalletsDeep.Value >= 2
                        ? design.PalletsDeep.Value
                        : Math.Max(2, defaultPalletsDeep),
                    DepthStartPosition = design.DepthStartPosition.HasValue && design.DepthStartPosition.Value > 0
                        ? design.DepthStartPosition.Value
                        : 1,
                    Bfr = Bfr(palletFront),
                    BeamLength = beamLength,
                    BeamLengthOverride = design.BeamLengthOverride
                };

                // I-42: los TRAMOS viajan verbatim. Sin ellos el frente ocupa su rango continuo de siempre, que es
                // lo que hace cualquier diseño anterior a la iniciativa.
                foreach (var segment in design.DepthSegments)
                {
                    resolved.DepthSegments.Add(segment);
                }

                result.Add(resolved);
            }

            return result;
        }

        /// <summary>
        /// The load levels one front actually carries. This is the funnel every load-bearing consumer goes through —
        /// frontal beams, lateral placements and the bed axes — so a BLANK front (I-33) answers an empty list here once
        /// and disappears from all of them, while its own dormant elevations stay on the front for the height rule.
        /// </summary>
        public static IReadOnlyList<DynamicLoadBeamLevel> LoadBeamLevels(
            DynamicRackSystem system,
            DynamicRackFront front)
        {
            if (DynamicFrontActivation.IsBlank(front))
            {
                return Array.Empty<DynamicLoadBeamLevel>();
            }

            if (front?.LoadBeamLevels?.Count > 0)
            {
                return front.LoadBeamLevels.ToList();
            }

            if (system?.LoadBeamLevels == null)
            {
                return Array.Empty<DynamicLoadBeamLevel>();
            }

            return front == null
                ? system.LoadBeamLevels.ToList()
                : system.LoadBeamLevels.Take(Math.Max(1, front.LoadLevels)).ToList();
        }

        public static DynamicFrontLayout Compute(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null || system.Fronts.Count == 0)
            {
                return new DynamicFrontLayout(Array.Empty<double>(), Array.Empty<double>());
            }

            var postId = PostId(system, catalog);
            var peralte = PostPeralte(system, catalog, postId);
            var parameters = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte };
            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                RackEmbedView.Frontal);
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, parameters).X;
            var posts = new List<double> { 0.0 };
            var troqueles = new List<double> { troquel };
            foreach (var front in system.Fronts)
            {
                var envelope = DynamicRackLevelGeometry.Envelope(system, front);
                var inicioEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                    envelope.InOutBeamCatalogId,
                    SelectiveRackDefaults.BeamProfileStartPoint,
                    RackEmbedView.Frontal);
                var inicio = SelectivePostGeometry.Resolve(inicioEntry, new Dictionary<string, double>
                {
                    [SelectiveRackDefaults.PeralteParam] = envelope.InOutBeamDepth
                }).X;
                posts.Add(posts[posts.Count - 1] + front.BeamLength + 2.0 * (troquel + inicio));
                troqueles.Add(troquel);
            }

            return new DynamicFrontLayout(posts, troqueles);
        }

        public static string PostId(DynamicRackSystem system, RackCatalog catalog)
            => system?.Modules.FirstOrDefault(module => module.IsHeader
                    && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId
               ?? catalog?.Defaults?.Post;

        public static string PlateId(DynamicRackSystem system, RackCatalog catalog)
            => system?.Modules.FirstOrDefault(module => module.IsHeader
                    && module.AssociatedFrameConfiguration?.LeftBasePlate != null)?
                .AssociatedFrameConfiguration.LeftBasePlate.PlateCatalogId
               ?? catalog?.Defaults?.BasePlate;

        public static double Height(DynamicRackSystem system)
            => system?.Modules.Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                   .Select(module => module.AssociatedFrameConfiguration.Height)
                   .DefaultIfEmpty(0.0)
                   .Max()
               ?? 0.0;

        /// <summary>Height of one transverse post: the tallest of the fronts that share it. This mirrors the
        /// selective-rack contract and prevents an unrelated taller front from inflating every post.</summary>
        public static double PostHeight(DynamicRackSystem system, int postIndex)
        {
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return 0.0;
            }

            var height = 0.0;
            if (postIndex > 0)
            {
                height = Math.Max(height, system.Fronts[postIndex - 1]?.Height ?? 0.0);
            }

            if (postIndex < system.Fronts.Count)
            {
                height = Math.Max(height, system.Fronts[postIndex]?.Height ?? 0.0);
            }

            return height > 0.0 ? height : Height(system);
        }

        /// <summary>
        /// I-42 (ronda 6D) — LA ALTURA DE UNA LINEA EN UNA POSICION DE PROFUNDIDAD. Una cabecera vive en una linea
        /// transversal Y en una posicion longitudinal, y en un Push Back compuesto esa segunda coordenada decide a
        /// que lado sirve: la primera mitad de la profundidad es de A y la segunda de B, con demandas
        /// independientes. Sin zonas declaradas —el Dinamico, y todo rack de un solo sentido— responde exactamente
        /// <see cref="PostHeight"/>, que es lo que se hacia antes de esta ronda.
        /// </summary>
        public static double PostHeightAt(DynamicRackSystem system, int postIndex, double x)
        {
            if (system != null && postIndex >= 0)
            {
                foreach (var zone in system.HeaderHeightZones)
                {
                    if (zone == null || x < zone.StartX - 1e-6 || x > zone.EndX + 1e-6)
                    {
                        continue;
                    }

                    if (postIndex < zone.HeightByLine.Count && zone.HeightByLine[postIndex] > 0.0)
                    {
                        return zone.HeightByLine[postIndex];
                    }
                }
            }

            return PostHeight(system, postIndex);
        }

        /// <summary>
        /// Resolves the header configuration physically present at one transverse post line. Calculated headers
        /// inherit the tallest adjacent front; manually edited headers remain authoritative. Lateral drawing and
        /// BOM consume this same rule so a tall front changes only its two adjacent header sections and quantities.
        /// </summary>
        public static RackFrameConfiguration HeaderConfigurationAtPost(
            DynamicRackSystem system,
            DynamicRackModule module,
            RackCatalog catalog,
            int postIndex)
        {
            var configuration = module?.AssociatedFrameConfiguration;

            // I-40 — la LINEA fisica manda sobre el modulo. Esta funcion es el UNICO sitio que decide que
            // configuracion usa una cabecera en una linea, y la consumen la geometria lateral, el BOM y el preview,
            // asi que el override llega a los tres por construccion (AGENTS: la regla vive en una sola funcion).
            // Sin overrides —cualquier rack anterior, y todo el Dinamico— esta busqueda no encuentra nada y el
            // comportamiento es identico al de siempre.
            var line = LineOverride(system, module, postIndex);
            if (line != null)
            {
                return line;
            }

            if (configuration == null || !module.UseCalculatedHeaderConfiguration)
            {
                return configuration;
            }

            // I-42 (ronda 6D): la altura se pregunta en la POSICION de esta cabecera, no solo en su linea. En un
            // rack de un solo sentido las dos preguntas dan lo mismo.
            var height = PostHeightAt(system, postIndex, 0.5 * (module.StartX + module.EndX));
            if (height <= 0.0)
            {
                return configuration;
            }

            var postId = configuration.LeftPost?.PostCatalogId ?? PostId(system, catalog);
            return new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default,
                postId,
                height,
                module.Length,
                system.PostPeralte);
        }

        /// <summary>
        /// The per-LINE cabecera configuration of a module, or null when that line uses the module's own. Matching is
        /// exact on <c>PostIndex</c> + <c>ModuleId</c>, the same identity the module reconciliation uses.
        /// </summary>
        public static RackFrameConfiguration LineOverride(
            DynamicRackSystem system,
            DynamicRackModule module,
            int postIndex)
        {
            if (system == null || module == null || system.HeaderLineOverrides.Count == 0)
            {
                return null;
            }

            foreach (var candidate in system.HeaderLineOverrides)
            {
                if (candidate?.Header != null
                    && candidate.PostIndex == postIndex
                    && string.Equals(candidate.ModuleId, module.ModuleId, StringComparison.Ordinal))
                {
                    return candidate.Header;
                }
            }

            return null;
        }

        /// <summary>
        /// The LONGITUD the derived posts of one physical line take (I-40). The line's own value wins; otherwise the
        /// rack-wide one; otherwise <paramref name="inheritedHeight"/>, which is the cabecera height the derived post
        /// has always inherited. Single authority: the lateral geometry and the BOM both read it.
        /// </summary>
        public static double DerivedPostHeightAtPost(
            DynamicRackSystem system,
            int postIndex,
            double inheritedHeight)
        {
            if (system != null)
            {
                foreach (var candidate in system.DerivedPostLineOverrides)
                {
                    if (candidate != null && candidate.PostIndex == postIndex && candidate.Height > 0.0)
                    {
                        return candidate.Height;
                    }
                }

                if (system.DerivedPostHeight.HasValue && system.DerivedPostHeight.Value > 0.0)
                {
                    return system.DerivedPostHeight.Value;
                }
            }

            return inheritedHeight;
        }

        /// <summary>
        /// The cabecera one physical LINE presents to the FRONTAL or the REAR view (I-40, decision del Owner).
        ///
        /// <para>
        /// La frontal y la posterior son VISTAS DE CORTE, no envolventes: la frontal corta por la PRIMERA cabecera
        /// longitudinal del rack —la del extremo bajo, la del pasillo— y la posterior por la ULTIMA. Mirando el rack
        /// de frente se tiene delante esa cabecera y ninguna otra, asi que una cabecera mas alta en OTRA posicion
        /// longitudinal no puede dominar la vista.
        /// </para>
        ///
        /// <para>
        /// Los dos ejes NO se mezclan: <paramref name="end"/> elige QUE cabecera se tiene delante (eje longitudinal)
        /// y <paramref name="postIndex"/> elige de QUE linea es el poste que se dibuja (eje transversal). La
        /// configuracion sale de <see cref="HeaderConfigurationAtPost"/>, la misma autoridad que gobiernan las
        /// personalizaciones por linea y que consumen la geometria lateral y el BOM.
        /// </para>
        /// </summary>
        public static double HeaderHeightAtPost(
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex,
            DynamicRackEnd end)
            => HeaderHeightAtPost(system, catalog, postIndex, end, double.NegativeInfinity, double.PositiveInfinity);

        /// <summary>
        /// I-42 (ronda 6D) — el mismo corte, restringido a un TRAMO de profundidad. Un rack compuesto lo necesita
        /// porque su frontal es de UN LADO, y ese lado ocupa solo la mitad de la profundidad: buscar la cabecera del
        /// extremo sobre el rack entero devolveria la del otro lado. Con el tramo abierto —el valor por defecto— el
        /// comportamiento es exactamente el anterior.
        /// </summary>
        public static double HeaderHeightAtPost(
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex,
            DynamicRackEnd end,
            double minX,
            double maxX)
        {
            var configuration = HeaderConfigurationAtCut(system, catalog, postIndex, end, minX, maxX);
            if (configuration != null && configuration.Height > 0.0)
            {
                return configuration.Height;
            }

            return double.IsInfinity(minX) || double.IsInfinity(maxX)
                ? PostHeight(system, postIndex)
                : PostHeightAt(system, postIndex, 0.5 * (minX + maxX));
        }

        /// <summary>
        /// The configuration of the cabecera this CUT shows on that line: the FIRST header module of the line's
        /// depth range for the low end (frontal), the LAST for the high end (posterior). Null when the line carries
        /// no cabecera with a configuration.
        /// </summary>
        public static RackFrameConfiguration HeaderConfigurationAtCut(
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex,
            DynamicRackEnd end)
            => HeaderConfigurationAtCut(
                system, catalog, postIndex, end, double.NegativeInfinity, double.PositiveInfinity);

        /// <summary>El mismo corte, limitado a las cabeceras cuyo centro cae en <c>[minX, maxX]</c> (I-42, 6D).</summary>
        public static RackFrameConfiguration HeaderConfigurationAtCut(
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex,
            DynamicRackEnd end,
            double minX,
            double maxX)
        {
            if (system == null)
            {
                return null;
            }

            var range = DynamicDepthGeometry.CoverageAtPost(system, postIndex);
            var headers = system.Modules
                .Where(module => module != null && module.IsHeader && range.Contains(module.Index + 1))
                .Where(module =>
                {
                    var centre = 0.5 * (module.StartX + module.EndX);
                    return centre >= minX - 1e-6 && centre <= maxX + 1e-6;
                })
                .OrderBy(module => module.Index)
                .ToList();

            if (headers.Count == 0)
            {
                return null;
            }

            // Exit = extremo BAJO (entrada/salida, el del pasillo) ⇒ la PRIMERA cabecera longitudinal.
            // Entrance = extremo ALTO (posterior) ⇒ la ULTIMA.
            var module = end == DynamicRackEnd.Entrance ? headers[headers.Count - 1] : headers[0];
            return HeaderConfigurationAtPost(system, module, catalog, postIndex);
        }

        /// <summary>Number of load levels visible at one post section: the tallest adjacent front owns the cut.</summary>
        public static int LoadLevelsAtPost(DynamicRackSystem system, int postIndex)
        {
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return 0;
            }

            // Blank fronts contribute zero, so a post between an active and a blank front keeps its active neighbour's
            // cut and a post surrounded only by blank fronts carries none (I-33). The rack-wide fallback is reserved
            // for a legacy rack with no resolved fronts at all, where there is no neighbour to ask.
            var levels = LoadLevelsAtPost(DynamicFrontActivation.EffectiveLevelsPerFront(system), postIndex);
            if (levels > 0)
            {
                return levels;
            }

            return system.Fronts.Count == 0 ? system.LoadBeamLevels.Count : 0;
        }

        /// <summary>
        /// The same "tallest adjacent front owns the cut" rule, stated over a plain per-FRONT level count so a caller
        /// with no resolved system — an editor dialog, for instance — cannot invent a second, divergent rule. A rack of
        /// N fronts has N+1 posts: each end post sees only its single neighbour, every interior post sees two.
        /// </summary>
        public static int LoadLevelsAtPost(IReadOnlyList<int> levelsPerFront, int postIndex)
        {
            if (levelsPerFront == null || postIndex < 0 || postIndex > levelsPerFront.Count)
            {
                return 0;
            }

            var levels = 0;
            if (postIndex > 0)
            {
                levels = Math.Max(levels, levelsPerFront[postIndex - 1]);
            }

            if (postIndex < levelsPerFront.Count)
            {
                levels = Math.Max(levels, levelsPerFront[postIndex]);
            }

            return levels;
        }

        /// <summary>The level count of EVERY post (N fronts produce N+1 posts), by the adjacent-front rule above.</summary>
        public static IReadOnlyList<int> LoadLevelsPerPost(IReadOnlyList<int> levelsPerFront)
        {
            var fronts = levelsPerFront ?? new List<int>();
            var result = new List<int>(fronts.Count + 1);
            for (var post = 0; post <= fronts.Count; post++)
            {
                result.Add(Math.Max(1, LoadLevelsAtPost(fronts, post)));
            }

            return result;
        }

        /// <summary>Fronts physically adjacent to one post, used by section-only level and peralte rules.</summary>
        public static IReadOnlyList<DynamicRackFront> AdjacentFronts(DynamicRackSystem system, int postIndex)
        {
            var result = new List<DynamicRackFront>();
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return result;
            }

            if (postIndex > 0 && system.Fronts[postIndex - 1] != null)
            {
                result.Add(system.Fronts[postIndex - 1]);
            }

            if (postIndex < system.Fronts.Count && system.Fronts[postIndex] != null)
            {
                result.Add(system.Fronts[postIndex]);
            }

            return result;
        }

        public static double PostPeralte(DynamicRackSystem system, RackCatalog catalog, string postId = null)
        {
            if (system?.PostPeralte > 0.0)
            {
                return system.PostPeralte;
            }

            var configuration = system?.Modules.FirstOrDefault(module => module.IsHeader
                && module.AssociatedFrameConfiguration != null)?.AssociatedFrameConfiguration;
            if (configuration?.PostPeralte > 0.0)
            {
                return configuration.PostPeralte;
            }

            var resolvedPostId = string.IsNullOrWhiteSpace(postId) ? PostId(system, catalog) : postId;
            var width = catalog?.PostProfiles?.FirstOrDefault(profile => string.Equals(
                profile?.Id,
                resolvedPostId,
                StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : 3.0;
        }

        /// <summary>Internal view names without taking a persistence dependency from Domain.</summary>
        private static class RackEmbedView
        {
            public const string Frontal = "FRONTAL";
        }
    }
}
