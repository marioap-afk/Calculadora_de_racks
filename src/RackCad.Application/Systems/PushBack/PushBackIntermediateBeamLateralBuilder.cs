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

        /// <summary>
        /// I-44 — QUIEN decide que larguero es este: el perfil y el peralte de cada instancia. Lo fija el LLAMADOR,
        /// no la forma de sus argumentos.
        ///
        /// <para>
        /// La distincion no puede leerse de <c>postIndex</c> ni de <c>front</c>: <see cref="Build"/> pasa los dos
        /// cuando proyecta un corte por poste, y <see cref="BuildFor"/> tambien recibe un <c>postIndex</c> real
        /// desde el dibujo de un rack compuesto. Deducirla de los argumentos es exactamente el defecto que I-44
        /// corrige — el BOM materializaba camas fisicas y cobraba la envolvente del rack.
        /// </para>
        /// </summary>
        private enum BeamAuthority
        {
            /// <summary>
            /// Lo VISIBLE en un corte lateral: varios frentes se superponen en la proyeccion y el que se ve es el de
            /// mayor peralte, que tapa a los de detras. La envolvente vive en
            /// <see cref="DynamicIntermediateBeamGeometry"/> y esta semantica no cambia.
            /// </summary>
            Projection,

            /// <summary>
            /// La pieza que EXISTE en una cama concreta: su perfil y su peralte son los de esa celda (frente x
            /// nivel) y de ninguna otra. Ningun frente vecino puede convertir un intermedio de 3.5" en uno de 6".
            /// </summary>
            PhysicalBed
        }

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
                Append(flat, added, system, catalog, front, null, postIndex, levelCount, BeamAuthority.Projection);
            }

            return HeaderInstanceGrouper.Group(flat, GroupPrefix);
        }

        /// <summary>
        /// I-42 — los intermedios de UNA cama: los de <paramref name="front"/> en el marco de
        /// <paramref name="system"/>, restringidos a <paramref name="levels"/>. Devuelve instancias sueltas para que
        /// el compositor las refleje junto con el resto de la cama, con la MISMA transformacion.
        ///
        /// <para>
        /// I-44 — estas son piezas FISICAS, asi que su perfil y su peralte son los de la celda que las pide
        /// (<see cref="BeamAuthority.PhysicalBed"/>). <paramref name="postIndex"/> sigue participando en la
        /// COLOCACION, pero no decide ninguno de los dos: un dibujo de rack compuesto pasa un poste real y aun asi
        /// cada cama debe llevar su propio larguero.
        /// </para>
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

            Append(
                flat, new HashSet<string>(), system, catalog, front, levels, postIndex, int.MaxValue,
                BeamAuthority.PhysicalBed);
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
            int levelCount,
            BeamAuthority authority)
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
                    //
                    // I-42 (correccion aislada 5D) — la pregunta se hace sobre la FRONTERA del apoyo, no sobre donde
                    // se dibuja. En un poste derivado y REFORZADO el eje dibujado cae una FIN_POSTE antes de su
                    // frontera, asi que el apoyo de la frontera donde ACABA la cama se colaba por delante del
                    // larguero alto: dos largueros dibujados para un solo apoyo fisico. Un refuerzo cambia el
                    // POSTE; no anade un segundo apoyo funcional a la cama.
                    if (front != null)
                    {
                        var rearX = PushBackCellDepth.RearX(system, front, axis.LevelNumber);
                        if (support.BoundaryX >= rearX - 1e-6 || support.BoundaryX <= front.StartX + 1e-6)
                        {
                            continue;
                        }
                    }

                    // I-44 — el perfil y el peralte SALEN JUNTOS de la misma autoridad, nunca uno de cada una.
                    //
                    // En una cama fisica los da la celda concreta, leidos de UNA sola operacion
                    // (DynamicRackLevelGeometry.At es el unico accesor que devuelve los dos del MISMO nivel
                    // resuelto): asi no puede volver a ocurrir que el id venga del frente de mayor peralte y el
                    // peralte de la envolvente del rack. En una proyeccion los da la envolvente, exactamente como
                    // antes — un corte lateral muestra el larguero que se VE, que es el que tapa a los de detras.
                    string beamId;
                    double peralte;
                    if (authority == BeamAuthority.PhysicalBed)
                    {
                        var cell = DynamicRackLevelGeometry.At(structure, front, axis.LevelNumber);
                        beamId = string.IsNullOrWhiteSpace(cell.IntermediateBeamCatalogId)
                            ? DynamicRackDefaults.IntermediateBeamCatalogId
                            : cell.IntermediateBeamCatalogId;
                        peralte = cell.IntermediateBeamDepth;
                    }
                    else if (postIndex >= 0)
                    {
                        beamId = DynamicIntermediateBeamGeometry.BeamIdAtPost(structure, postIndex, axis.LevelNumber);
                        peralte = DynamicIntermediateBeamGeometry.PeralteAtPost(structure, postIndex, axis.LevelNumber);
                    }
                    else
                    {
                        beamId = structure.Fronts
                            .Where(candidate => candidate.LoadLevels >= axis.LevelNumber)
                            .OrderByDescending(candidate => DynamicIntermediateBeamGeometry.PeralteAt(candidate, axis.LevelNumber))
                            .Select(candidate => DynamicIntermediateBeamGeometry.BeamIdAt(candidate, axis.LevelNumber))
                            .FirstOrDefault() ?? DynamicRackDefaults.IntermediateBeamCatalogId;
                        peralte = DynamicIntermediateBeamGeometry.PeralteAt(structure, axis.LevelNumber);
                    }

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

                    flat.Add(Make(axis, support.PostAxisX, mate, support.Mirrored, beamId, block, peralte));
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
