using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Places one intermediate support beam (<c>LARGUERO_ESCALON_INFINITO</c>) at every internal post and load level,
    /// tangent to the PUSH BACK bed axis (whose high mate is the rear TROQUEL_REDONDO beam, so the tangent line differs
    /// from the dynamic one). It REUSES the dynamic structural helpers (<see cref="DynamicIntermediateBeamGeometry.Supports"/>,
    /// peralte/beam-id lookups) but consumes <see cref="PushBackFlowBedGeometry"/> for the axis. No dynamic code is altered.
    ///
    /// <para>
    /// I-42 — un intermedio pertenece a UNA CAMA, no a la estructura: sostiene su riel, sigue su pendiente y vive en
    /// su marco. Por eso el rack compuesto lo construye por cama (<see cref="BuildFor"/>) y no una vez sobre la
    /// estructura compartida: hacerlo sobre la compuesta resolvia ejes que no son los de ninguna cama real, y por eso
    /// faltaban intermedios y otros aparecian a elevaciones equivocadas.
    /// </para>
    /// </summary>
    public sealed class PushBackIntermediateBeamLateralBuilder
    {
        public const string GroupPrefix = "PB_LARGUERO_INTERMEDIO";

        public HeaderRunPlan Build(PushBackSystem system, RackCatalog catalog, int postIndex = -1, int levelCount = int.MaxValue)
        {
            var flat = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return HeaderInstanceGrouper.Group(flat, GroupPrefix);
            }

            var fronts = postIndex >= 0
                ? DynamicFrontGeometry.AdjacentFronts(structure, postIndex).OrderByDescending(front => front.PalletsDeep).ToList()
                : new List<DynamicRackFront> { null };
            var added = new HashSet<string>();
            foreach (var front in fronts)
            {
                Append(flat, added, system, catalog, front, null, postIndex, levelCount);
            }

            return HeaderInstanceGrouper.Group(flat, GroupPrefix);
        }

        /// <summary>
        /// I-42 — los intermedios de UNA cama: los de <paramref name="front"/> en el marco de
        /// <paramref name="system"/>, restringidos a <paramref name="levels"/>. Devuelve instancias sueltas para que
        /// el compositor las refleje junto con el resto de la cama, con la MISMA transformacion.
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> BuildFor(
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front,
            IReadOnlyCollection<int> levels,
            int postIndex = -1)
        {
            var flat = new List<HeaderBlockInstance>();
            if (system?.Structure == null || front == null)
            {
                return flat;
            }

            Append(flat, new HashSet<string>(), system, catalog, front, levels, postIndex, int.MaxValue);
            return flat;
        }

        private static void Append(
            ICollection<HeaderBlockInstance> flat,
            ISet<string> added,
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front,
            IReadOnlyCollection<int> levels,
            int postIndex,
            int levelCount)
        {
            var structure = system.Structure;
            var postId = structure.Modules
                .FirstOrDefault(module => module.IsHeader && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
            var finPoste = CatalogLookup.Local(catalog, postId, "FIN_POSTE", DynamicRackDefaults.IntermediateBeamView);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .Where(axis => axis.LevelNumber <= levelCount)
                .Where(axis => levels == null || levels.Contains(axis.LevelNumber))
                .ToList();

            foreach (var support in DynamicIntermediateBeamGeometry.Supports(structure, finPoste, front))
            {
                foreach (var axis in axes)
                {
                    // I-41 (PB-015): un soporte que cae DETRAS del larguero posterior de esta celda no sostiene
                    // nada — esa cama termina antes. I-42 anade la otra mitad de la misma regla: un soporte que cae
                    // POR DELANTE del larguero bajo tampoco sostiene nada, porque una cama corrida corta no llega
                    // hasta el extremo exterior del lado bajo.
                    if (front != null)
                    {
                        var rearX = PushBackCellDepth.RearX(system, front, axis.LevelNumber);
                        if (support.PostAxisX >= rearX - 1e-6 || support.PostAxisX <= front.StartX + 1e-6)
                        {
                            continue;
                        }
                    }

                    var beamId = postIndex >= 0
                        ? DynamicIntermediateBeamGeometry.BeamIdAtPost(structure, postIndex, axis.LevelNumber)
                        : structure.Fronts
                            .Where(candidate => candidate.LoadLevels >= axis.LevelNumber)
                            .OrderByDescending(candidate => DynamicIntermediateBeamGeometry.PeralteAt(candidate, axis.LevelNumber))
                            .Select(candidate => DynamicIntermediateBeamGeometry.BeamIdAt(candidate, axis.LevelNumber))
                            .FirstOrDefault() ?? DynamicRackDefaults.IntermediateBeamCatalogId;
                    var block = CatalogLookup.Block(catalog, beamId, DynamicRackDefaults.IntermediateBeamView);
                    var leftEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                        beamId, DynamicRackDefaults.IntermediateBeamLeftBedMatePoint, DynamicRackDefaults.IntermediateBeamView);
                    var rightEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                        beamId, DynamicRackDefaults.IntermediateBeamRightBedMatePoint, DynamicRackDefaults.IntermediateBeamView);
                    if (string.IsNullOrWhiteSpace(block) || leftEntry == null || rightEntry == null)
                    {
                        continue;
                    }

                    var leftMate = new Point2D(leftEntry.LocalX, leftEntry.LocalY);
                    var rightMate = new Point2D(rightEntry.LocalX, rightEntry.LocalY);
                    var mate = support.Mirrored ? rightMate : leftMate;
                    var key = support.PostAxisX.ToString("0.####", CultureInfo.InvariantCulture)
                              + "|" + axis.LevelNumber + "|" + beamId;
                    if (!added.Add(key))
                    {
                        continue;
                    }

                    flat.Add(Make(
                        axis,
                        support.PostAxisX,
                        mate,
                        support.Mirrored,
                        beamId,
                        block,
                        postIndex >= 0
                            ? DynamicIntermediateBeamGeometry.PeralteAtPost(structure, postIndex, axis.LevelNumber)
                            : DynamicIntermediateBeamGeometry.PeralteAt(structure, axis.LevelNumber)));
                }
            }
        }

        private static HeaderBlockInstance Make(
            PushBackFlowBedAxis axis,
            double postAxisX,
            Point2D localBedMate,
            bool mirrored,
            string beamId,
            string block,
            double peralte)
        {
            var contactX = postAxisX + (mirrored ? -localBedMate.X : localBedMate.X);
            var insertion = new Point2D(postAxisX, axis.RailOriginYAt(contactX) - localBedMate.Y);

            var result = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = beamId,
                BlockName = block,
                View = DynamicRackDefaults.IntermediateBeamView,
                Insertion = insertion,
                ConnectionAnchor = insertion,
                MirroredX = mirrored
            };
            result.DynamicParameters[SelectiveRackDefaults.PeralteParam] = peralte;
            return result;
        }
    }
}
