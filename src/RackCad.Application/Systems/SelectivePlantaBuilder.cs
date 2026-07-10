using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the PLANTA (top-down) view of a whole selective run as loose block instances. Orientation (per the
    /// catalog's PLANTA points): <b>X = fondo</b> (each frame's cabecera spans X: front post at 0, back at fondo) and
    /// <b>Y = frente</b> (the largueros run along Y, LONGITUD = the beam length). One cabecera-planta per post is
    /// stacked at its frente position (<see cref="SelectivePostGeometry"/>'s post Xs, read here as Ys), and per bay a
    /// FRONT + BACK larguero connects consecutive frames along Y. Pure — no AutoCAD.
    /// </summary>
    public sealed class SelectivePlantaBuilder
    {
        private const string PlantaView = "PLANTA";

        private readonly PlantaHeaderLayoutBuilder frameBuilder = new PlantaHeaderLayoutBuilder();

        public IReadOnlyList<HeaderBlockInstance> Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            var instances = new List<HeaderBlockInstance>();
            if (system == null || system.Bays.Count == 0)
            {
                return instances;
            }

            // Built from the catalog the caller already loaded (a field initializer would trigger its own load).
            var factory = new RackFrameConfigurationFactory(catalog);

            var frenteYs = SelectivePostGeometry.Compute(system, catalog).PostXs; // frente positions, read as Y here
            var depth = system.PalletDepth > 0.0 ? system.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();

            // One cabecera-planta per frame, stacked at its frente Y (fondo runs along X inside each frame).
            for (var i = 0; i < frenteYs.Count; i++)
            {
                var custom = i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                RackFrameConfiguration cabecera;
                if (custom != null && custom.Height > 0.0)
                {
                    cabecera = custom;
                }
                else
                {
                    var height = SelectivePostGeometry.PostHeight(system, i);
                    cabecera = factory.Build(template, system.PostId, height > 0.0 ? height : system.Height, depth);
                }

                // Each frame draws with ITS post's peralte (per-post override, else the run default) so the planta
                // grows the post/celosía/plate exactly like the frontal (matches SelectiveFrontalBuilder).
                var framePeralte = SelectivePostGeometry.PostPeralteAt(system, i);
                foreach (var instance in frameBuilder.Build(cabecera, catalog, new Point2D(0.0, frenteYs[i]), framePeralte))
                {
                    if (!system.DrawBasePlate && instance.Role == HeaderBlockRole.BasePlate)
                    {
                        continue; // "Dibujar placa base" toggle off
                    }

                    instances.Add(instance);
                }
            }

            AddLargueros(instances, system, catalog, frenteYs, depth);
            return instances;
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
