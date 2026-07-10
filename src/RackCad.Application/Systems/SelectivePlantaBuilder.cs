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

            var frenteYs = SelectivePostGeometry.Compute(system, catalog).PostXs; // frente positions, read as Y here
            var depth = system.PalletDepth > 0.0 ? system.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();

            var groups = new List<PlantaGroupBuilder>();
            var shared = new Dictionary<string, PlantaGroupBuilder>(StringComparer.Ordinal);

            // One cabecera-planta per frame, stacked at its frente Y (fondo runs along X inside each frame).
            for (var i = 0; i < frenteYs.Count; i++)
            {
                var custom = i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;

                // Each frame draws with ITS post's peralte (per-post override, else the run default) so the planta
                // grows the post/celosía/plate exactly like the frontal (matches SelectiveFrontalBuilder).
                var framePeralte = SelectivePostGeometry.PostPeralteAt(system, i);

                PlantaGroupBuilder group;
                if (custom != null && custom.Height > 0.0)
                {
                    // Custom cabecera: never shared, even with an identical twin — editing one must not move both.
                    group = NewGroup(groups, custom, catalog, framePeralte, system.DrawBasePlate);
                }
                else
                {
                    var height = SelectivePostGeometry.PostHeight(system, i);
                    var resolvedHeight = height > 0.0 ? height : system.Height;
                    var key = GroupKey(resolvedHeight, framePeralte, depth);
                    if (!shared.TryGetValue(key, out group))
                    {
                        var cabecera = factory.Build(template, system.PostId, resolvedHeight, depth);
                        group = NewGroup(groups, cabecera, catalog, framePeralte, system.DrawBasePlate);
                        shared[key] = group;
                    }
                }

                group.Placements.Add(new HeaderPlacement(0.0, mirrored: false, insertionY: frenteYs[i]));
            }

            AddLargueros(loose, system, catalog, frenteYs, depth);
            AddAnnotations(loose, system, frenteYs);
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
        /// Per bay, a FRONT larguero at the front post (X=0) and a BACK one at the back post (X=fondo), running along Y
        /// from the frame's larguero troquel with LONGITUD = the beam length. The troquel slides with the post peralte
        /// (its PLANTA Y-slope) and the beam's INICIO_PERFIL (planta) sets the ménsula overhang along Y.
        /// </summary>
        private static void AddLargueros(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, RackCatalog catalog, IReadOnlyList<double> frenteYs, double depth)
        {
            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveRackDefaults.PostBeamPoint, PlantaView);

            for (var i = 0; i < system.Bays.Count && i + 1 < frenteYs.Count; i++)
            {
                var bay = system.Bays[i];
                if (bay.Levels.Count == 0)
                {
                    continue;
                }

                var level = bay.Levels[0]; // in plan the levels collapse onto one line — one front + one back per bay
                var block = catalog?.Blocks.FindBlock(level.BeamId, PlantaView)?.BlockName;
                if (string.IsNullOrWhiteSpace(block))
                {
                    continue;
                }

                // The beam is placed at its troquel exactly like the frontal (its origin = the hook); the ménsula
                // overhang (INICIO_PERFIL) is already baked into the frame spacing by SelectivePostGeometry, so it is
                // NOT subtracted here. The troquel slides with THIS frame's post peralte along Y (its PLANTA Y-slope).
                var postParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = SelectivePostGeometry.PostPeralteAt(system, i) };
                var troquel = SelectivePostGeometry.Resolve(troquelEntry, postParams);
                var mateY = frenteYs[i] + troquel.Y;

                // Front post at X=0, back post at X=fondo; LONGITUD runs along Y (= the beam length).
                AddLarguero(instances, level.BeamId, block, new Point2D(troquel.X, mateY), bay.BeamLength, level.BeamPeralte, mirrored: false);
                AddLarguero(instances, level.BeamId, block, new Point2D(depth - troquel.X, mateY), bay.BeamLength, level.BeamPeralte, mirrored: true);
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
