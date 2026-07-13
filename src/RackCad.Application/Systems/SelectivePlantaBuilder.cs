using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the PLANTA (top-down) view of a whole selective run. Orientation (per the catalog's PLANTA points):
    /// <b>X = fondo</b> (each frame's cabecera spans X: front post at 0, back at fondo) and <b>Y = frente</b> (the
    /// largueros run along Y, LONGITUD = the beam length). One cabecera-planta per post is stacked at its frente
    /// position (<see cref="SelectivePostGeometry"/>'s post Xs, read here as Ys), and per bay a FRONT + BACK
    /// larguero connects consecutive frames along Y. <see cref="BuildPlan"/> groups identical frames into shared
    /// nested definitions (the ARRAY pattern the dynamic lateral uses) so a long run draws each distinct frame
    /// once; <see cref="Build"/> flattens that plan into loose instances. Pure — no AutoCAD.
    /// </summary>
    public sealed class SelectivePlantaBuilder
    {
        private const string PlantaView = "PLANTA";

        private readonly PlantaHeaderLayoutBuilder frameBuilder = new PlantaHeaderLayoutBuilder();

        /// <summary>
        /// The planta as a structured plan: each distinct cabecera-planta frame is one <see cref="HeaderGroup"/>
        /// (its pieces built ONCE at Y=0) placed at every frente Y where it repeats; largueros and annotations
        /// stay loose. Standard posts share a group when their resolved height/peralte/fondo match; a post with a
        /// custom cabecera always gets its OWN group (edits may diverge it from its twins later).
        /// </summary>
        public DynamicSystemPlan BuildPlan(SelectiveRackSystem system, RackCatalog catalog)
        {
            var loose = new List<HeaderBlockInstance>();
            if (system == null || system.Bays.Count == 0)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), loose);
            }

            // Built from the catalog the caller already loaded (a field initializer would trigger its own load).
            var factory = new RackFrameConfigurationFactory(catalog);

            var layout = SelectiveDepthLayout.MasterGrid(system, catalog); // longest fondo defines the frente grid
            var frenteYs = layout.PostXs; // master frente positions, read as Y here
            var offsets = SelectiveDepthLayout.Offsets(system); // one X per fondo (doble profundidad), per-fondo depth
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();

            var groups = new List<PlantaGroupBuilder>();
            var shared = new Dictionary<string, PlantaGroupBuilder>(StringComparer.Ordinal);

            // Enabled botas (planta blocks) + the default plate whose PLANTA mate places them at the front-post base.
            // Loose (not grouped): a per-post side may differ, and the frames stay shared for the ARRAY pattern.
            var botas = SelectiveSafetyPlacement.EnabledBotas(system, catalog, PlantaView);
            var defaultPlateId = catalog?.Defaults?.BasePlate;

            // The mirrored (Right) bota reflects about the center of the system's TOTAL fondo (depth) span: frontmost
            // post at X=0 (offsets[0]) to the backmost post across all fondos. So a multi-fondo rack mirrors correctly.
            var backmostDepth = 0.0;
            for (var k = 0; k < offsets.Count; k++)
            {
                var back = offsets[k] + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
                if (back > backmostDepth) backmostDepth = back;
            }

            var depthCenterX = (offsets[0] + backmostDepth) / 2.0;

            // Each fondo's own height fallback (a fondo with only level-less bays still needs a post height).
            var fondoFallbacks = new double[offsets.Count];
            for (var k = 0; k < offsets.Count; k++)
            {
                fondoFallbacks[k] = FondoHeightFallback(SelectiveDepthLayout.BaysOfFondo(system, k), system.Height);
            }

            // One cabecera-planta per (frame, fondo), stacked at its frente Y (fondo runs along X). Each fondo can have
            // its OWN depth AND its OWN frente count (a corner layout): a shorter fondo places fewer frames — its posts
            // are a prefix of the master grid, so it just stops early. Height/peralte are per fondo too.
            for (var i = 0; i < frenteYs.Count; i++)
            {
                var custom = i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                var framePeralte = SelectivePostGeometry.PostPeralteAt(system, i);

                for (var k = 0; k < offsets.Count; k++)
                {
                    var baysK = SelectiveDepthLayout.BaysOfFondo(system, k);
                    if (i >= baysK.Count + 1)
                    {
                        continue; // fondo k doesn't reach this frente post (it has fewer frentes)
                    }

                    var depthK = SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
                    var height = SelectivePostGeometry.PostHeight(baysK, i, fondoFallbacks[k]);
                    var resolvedHeight = height > 0.0 ? height : system.Height;

                    PlantaGroupBuilder group;
                    if (k == 0 && custom != null && custom.Height > 0.0)
                    {
                        // Custom cabecera (fondo 0 only): never shared, even with an identical twin — editing one must not move both.
                        group = NewGroup(groups, custom, catalog, framePeralte, system.DrawBasePlate);
                    }
                    else
                    {
                        var key = GroupKey(resolvedHeight, framePeralte, depthK);
                        if (!shared.TryGetValue(key, out group))
                        {
                            var cabecera = factory.Build(template, system.PostId, resolvedHeight, depthK);
                            group = NewGroup(groups, cabecera, catalog, framePeralte, system.DrawBasePlate);
                            shared[key] = group;
                        }
                    }

                    group.Placements.Add(new HeaderPlacement(offsets[k], mirrored: false, insertionY: frenteYs[i]));

                    // Bota at this fondo's front-post base (X = offset), on the side post i resolves to; the mirrored
                    // (Right) copy reflects about the system's total-fondo center (depthCenterX).
                    SelectiveSafetyPlacement.AppendAtPost(loose, catalog, PlantaView, botas, new Point2D(offsets[k], frenteYs[i]), defaultPlateId, i, depthCenterX);
                }
            }

            // Medio frente: an intermediate cabecera-planta at EVERY tramo boundary (per fondo) — like the frontal's
            // intermediate posts but along Y. Reuses the frame groups (same footprint).
            for (var k = 0; k < offsets.Count; k++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                var depthK = SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
                for (var i = 0; i < bays.Count && i < frenteYs.Count; i++)
                {
                    var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bays[i], SelectiveRackDefaults.View);
                    var tramos = SelectiveMedioFrente.Resolve(bays[i], layout.TroquelXs[i], inicioX);
                    if (tramos == null)
                    {
                        continue; // not split (or the tramos don't fit) → no intermediate cabecera
                    }

                    var framePeralte = SelectivePostGeometry.PostPeralteAt(system, i);
                    var h = bays[i].Height > 0.0 ? bays[i].Height : system.Height;
                    var key = GroupKey(h, framePeralte, depthK);
                    if (!shared.TryGetValue(key, out var group))
                    {
                        var cabecera = factory.Build(template, system.PostId, h, depthK);
                        group = NewGroup(groups, cabecera, catalog, framePeralte, system.DrawBasePlate);
                        shared[key] = group;
                    }

                    // One intermediate cabecera per tramo boundary = the left post of every tramo except the first.
                    for (var t = 1; t < tramos.Count; t++)
                    {
                        group.Placements.Add(new HeaderPlacement(offsets[k], mirrored: false, insertionY: frenteYs[i] + tramos[t].StartOffset));
                    }
                }
            }

            AddLargueros(loose, system, catalog, frenteYs, offsets, layout.TroquelXs);
            AddAnnotations(loose, system, frenteYs);

            var fondoDepths = new List<double>(offsets.Count);
            for (var k = 0; k < offsets.Count; k++) fondoDepths.Add(SelectiveDepthLayout.CabeceraDepthOfFondo(system, k));

            // Governing larguero cut length per master frente (the widest bay across fondos — it set the post pitch),
            // so the planta cotas measure the cut, not the post-to-post spacing.
            var beamLengths = new List<double>();
            for (var i = 0; i + 1 < frenteYs.Count; i++)
            {
                var maxLen = 0.0;
                for (var k = 0; k < offsets.Count; k++)
                {
                    var baysK = SelectiveDepthLayout.BaysOfFondo(system, k);
                    if (i < baysK.Count && baysK[i].BeamLength > maxLen) maxLen = baysK[i].BeamLength;
                }

                beamLengths.Add(maxLen);
            }

            SelectiveDimensions.AddPlanta(loose, system, PlantaView, frenteYs, offsets, fondoDepths, beamLengths);

            return new DynamicSystemPlan(groups.Select(g => g.ToGroup()).ToList(), loose);
        }

        /// <summary>
        /// The planta as a flat instance list: <see cref="BuildPlan"/> expanded (each group's pieces translated to
        /// every placement's frente Y) plus the loose largueros/annotations — the exact instances the pre-plan
        /// builder emitted, in the same per-post order. Kept for the preview/BOM consumers and the tests.
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            var plan = BuildPlan(system, catalog);
            var instances = new List<HeaderBlockInstance>();

            // Expand placements in ascending frente Y (= post order) so the flat list keeps the historical
            // ordering: post 0's frame first, …, then largueros, then annotations.
            var placed = new List<KeyValuePair<HeaderPlacement, HeaderGroup>>();
            foreach (var group in plan.Headers)
            {
                foreach (var placement in group.Placements)
                {
                    placed.Add(new KeyValuePair<HeaderPlacement, HeaderGroup>(placement, group));
                }
            }

            foreach (var pair in placed.OrderBy(p => p.Key.InsertionY))
            {
                foreach (var instance in pair.Value.Instances)
                {
                    instances.Add(Translated(instance, pair.Key));
                }
            }

            instances.AddRange(plan.LooseInstances);
            return instances;
        }

        /// <summary>A group instance shifted to its placement (planta placements only translate — never mirror).</summary>
        private static HeaderBlockInstance Translated(HeaderBlockInstance source, HeaderPlacement placement)
        {
            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                RotationRadians = source.RotationRadians,
                MirroredX = source.MirroredX,
                Text = source.Text,
                TextHeight = source.TextHeight,
                ConnectionAnchor = new Point2D(source.ConnectionAnchor.X + placement.InsertionX, source.ConnectionAnchor.Y + placement.InsertionY),
                Insertion = new Point2D(source.Insertion.X + placement.InsertionX, source.Insertion.Y + placement.InsertionY)
            };

            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }

        /// <summary>Standard frames share a definition when the drawing-relevant inputs match: the resolved post
        /// height (rounded so float noise can't split identical posts), the post peralte and the fondo.</summary>
        private static string GroupKey(double resolvedHeight, double framePeralte, double depth)
            => string.Format(CultureInfo.InvariantCulture, "{0:R}|{1:R}|{2:R}", Math.Round(resolvedHeight, 4), framePeralte, depth);

        /// <summary>A fondo's post-height fallback: the tallest of its bays, else the run height (for a level-less fondo).</summary>
        private static double FondoHeightFallback(IList<SelectiveBay> bays, double systemHeight)
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

        /// <summary>Builds a frame's pieces ONCE at the local origin (Y=0) and registers the group.</summary>
        private PlantaGroupBuilder NewGroup(List<PlantaGroupBuilder> groups, RackFrameConfiguration cabecera, RackCatalog catalog, double framePeralte, bool drawBasePlate)
        {
            var pieces = new List<HeaderBlockInstance>();
            foreach (var instance in frameBuilder.Build(cabecera, catalog, new Point2D(0.0, 0.0), framePeralte))
            {
                if (!drawBasePlate && instance.Role == HeaderBlockRole.BasePlate)
                {
                    continue; // "Dibujar placa base" toggle off
                }

                pieces.Add(instance);
            }

            var name = "PLANTA_CAB_" + (groups.Count + 1).ToString(CultureInfo.InvariantCulture);
            var group = new PlantaGroupBuilder(name, pieces);
            groups.Add(group);
            return group;
        }

        private sealed class PlantaGroupBuilder
        {
            public PlantaGroupBuilder(string name, IReadOnlyList<HeaderBlockInstance> instances)
            {
                Name = name;
                Instances = instances;
                Placements = new List<HeaderPlacement>();
            }

            public string Name { get; }
            public IReadOnlyList<HeaderBlockInstance> Instances { get; }
            public List<HeaderPlacement> Placements { get; }

            public HeaderGroup ToGroup() => new HeaderGroup(Name, Instances, Placements);
        }

        /// <summary>Planta text labels: a number per frente (bay) at its mid-Y on the left, and the rack name on top.
        /// Levels aren't numbered here — in the top-down view they overlap (they only read in frontal/lateral).</summary>
        private static void AddAnnotations(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, IReadOnlyList<double> frenteYs)
        {
            var h = SelectiveAnnotations.TextHeightFor(system.AnnotationScale);
            var gap = h + SelectiveAnnotations.Margin;

            if (system.NumberFronts)
            {
                for (var i = 0; i + 1 < frenteYs.Count; i++) // one per bay (between consecutive frames)
                {
                    var centerY = (frenteYs[i] + frenteYs[i + 1]) / 2.0;
                    instances.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(i + 1), PlantaView, new Point2D(-gap, centerY), h));
                }
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name) && frenteYs.Count > 0)
            {
                var topY = frenteYs[frenteYs.Count - 1] + gap;
                instances.Add(SelectiveAnnotations.Label(system.Name.Trim(), PlantaView, new Point2D(0.0, topY), h * 1.5));
            }
        }

        /// <summary>
        /// Per bay and per fondo, a FRONT larguero at that fondo's front post (X=offset) and a BACK one at its back
        /// post (X=offset+fondo), running along Y from the frame's larguero troquel with LONGITUD = the beam length.
        /// The troquel slides with the post peralte (its PLANTA Y-slope). EACH fondo uses ITS OWN levels: a fondo whose
        /// bay has no levels (an empty frente / column) draws no larguero there. A "medio frente" bay draws a front+back
        /// larguero per LOADED tramo, each at that tramo's left post along Y. A single fondo keeps the historical
        /// front-at-0 / back-at-fondo pair.
        /// </summary>
        private static void AddLargueros(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, RackCatalog catalog, IReadOnlyList<double> frenteYs, IReadOnlyList<double> offsets, IReadOnlyList<double> troquelXs)
        {
            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveRackDefaults.PostBeamPoint, PlantaView);

            // Iterate the MASTER frente count (frenteYs = longest fondo). A fondo shorter than the master simply has no
            // bay at frente i (the inner `i >= bays.Count` guard skips it), so a corner layout draws each row's real span.
            for (var i = 0; i + 1 < frenteYs.Count; i++)
            {
                // The troquel/mate is a horizontal property (shared grid, per-frame peralte) — compute once per bay.
                // The ménsula overhang (INICIO_PERFIL) is already baked into the frame spacing by SelectivePostGeometry,
                // so it is NOT subtracted here.
                var postParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = SelectivePostGeometry.PostPeralteAt(system, i) };
                var troquel = SelectivePostGeometry.Resolve(troquelEntry, postParams);
                var mateY = frenteYs[i] + troquel.Y;

                for (var k = 0; k < offsets.Count; k++)
                {
                    var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                    if (i >= bays.Count || bays[i].Levels.Count == 0)
                    {
                        continue; // empty frente (column) in this fondo
                    }

                    var bay = bays[i];
                    var level = bay.Levels[0]; // in plan the levels collapse onto one line — one front + one back per bay
                    var block = catalog?.Blocks.FindBlock(level.BeamId, PlantaView)?.BlockName;
                    if (string.IsNullOrWhiteSpace(block))
                    {
                        continue;
                    }

                    // This fondo: front post at X=offset, back post at X=offset+ITS fondo (depth); LONGITUD runs along Y.
                    var offset = offsets[k];
                    var depthK = SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
                    var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);
                    var tramos = SelectiveMedioFrente.Resolve(bay, troquelXs[i], inicioX);

                    if (tramos == null)
                    {
                        // Full bay: one front + one back larguero spanning the bay.
                        AddLarguero(instances, level.BeamId, block, new Point2D(offset + troquel.X, mateY), bay.BeamLength, level.BeamPeralte, mirrored: false);
                        AddLarguero(instances, level.BeamId, block, new Point2D(offset + depthK - troquel.X, mateY), bay.BeamLength, level.BeamPeralte, mirrored: true);
                        continue;
                    }

                    // Split bay: a front + back larguero per LOADED tramo, each at its left post's troquel along Y.
                    foreach (var tramo in tramos)
                    {
                        if (!tramo.Loaded) continue;
                        var tramoY = mateY + tramo.StartOffset;
                        AddLarguero(instances, level.BeamId, block, new Point2D(offset + troquel.X, tramoY), tramo.Length, level.BeamPeralte, mirrored: false);
                        AddLarguero(instances, level.BeamId, block, new Point2D(offset + depthK - troquel.X, tramoY), tramo.Length, level.BeamPeralte, mirrored: true);
                    }
                }
            }
        }

        private static void AddLarguero(ICollection<HeaderBlockInstance> instances, string beamId, string block, Point2D insertion, double longitud, double peralte, bool mirrored)
        {
            var beam = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = beamId,
                BlockName = block,
                View = PlantaView,
                MirroredX = mirrored,
                Insertion = insertion,
                ConnectionAnchor = insertion
            };
            beam.DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud;
            beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = peralte;
            instances.Add(beam);
        }
    }
}
