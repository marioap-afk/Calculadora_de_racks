using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>La cara de un LADO a la que pertenece una bota. Se lee DENTRO del lado, nunca en el marco del rack.</summary>
    public enum BootFace
    {
        /// <summary>El pasillo por el que ese lado carga y descarga: su cara EXTERIOR.</summary>
        EntryExit = 0,

        /// <summary>La cara opuesta de ese lado. En un rack compuesto es su cara INTERIOR, la que da a la interfaz.</summary>
        Rear = 1,
    }

    /// <summary>
    /// I-42 (S1F) — UNA BOTA FISICA YA RESUELTA. Su identidad es <b>lado × cara × linea de postes</b>, y sobrevive
    /// entera hasta despues de resolver la pertenencia: la geometria y la reflexion vienen DESPUES y no la tocan.
    /// </summary>
    public sealed class ResolvedBoot
    {
        /// <summary>El lado al que pertenece. Nunca se deduce de una coordenada.</summary>
        public PushBackSide Side { get; set; }

        /// <summary>La cara de ese lado — leida dentro del lado, no en el marco del rack.</summary>
        public BootFace Face { get; set; }

        /// <summary>La linea de postes.</summary>
        public int PostIndex { get; set; }

        /// <summary>La X del PLANO fisico que protege: el exterior de su lado o su interior.</summary>
        public double FaceX { get; set; }

        /// <summary>Su mano: hacia donde mira la pieza. Es funcion de la cara, no al reves.</summary>
        public bool Mirrored { get; set; }

        public string PieceId { get; set; }

        /// <summary>El ancla en PLANTA, ya con la placa descontada.</summary>
        public Point2D PlantaAt { get; set; }

        /// <summary>La X de la linea del poste, que es donde la ancla un corte frontal.</summary>
        public double LineX { get; set; }

        /// <summary>La identidad, legible. Dos botas con la misma identidad son la misma pieza; ninguna otra lo es.</summary>
        public string Identity => FormattableString.Invariant($"{Side}|{Face}|P{PostIndex}");
    }

    /// <summary>
    /// I-42 (S1F, contrato del dueño) — LA RESOLUCION FISICA DE LAS BOTAS, una sola vez para todo el rack.
    ///
    /// <para>
    /// Un Push Back compuesto tiene CUATRO caras identificables por linea de postes: el exterior de A, el interior
    /// de A, el interior de B y el exterior de B. Las dos interiores pertenecen a lados distintos —A termina en su
    /// linea y B empieza en la suya—, asi que <b>Posterior(A) no es Posterior(B)</b> y, desde luego,
    /// <b>Posterior(A) no es Entrada/Salida(B)</b>. Con hueco CERO sus proyecciones se acercan hasta tocarse y
    /// siguen siendo dos piezas: el lado rompe el empate de coordenadas, como ya se cerro para la interfaz.
    /// </para>
    /// <para>
    /// <b>Lo que S1F corrige.</b> S1E convertia (lado, cara) a un unico eje global cercano/lejano ANTES de tener
    /// geometria, y ahi se perdian dos identidades: medido, con «A = Posterior» la bota aparecia en X=792.39 —el
    /// pasillo de B— y con «B = Posterior» en X=-0.39 —el de A—; con «Ambas» en los dos lados solo salian dos
    /// piezas por linea en vez de cuatro, porque las dos interiores no tenian ancla.
    /// </para>
    /// <para>
    /// El orden es: intencion (lado + poste + cara) → identidad fisica → ancla → transformacion de vista. La planta,
    /// los cuatro cortes, el lateral y el BOM consumen ESTA resolucion; ninguno vuelve a decidir quien lleva pieza
    /// ni deduplica por coordenada.
    /// </para>
    /// </summary>
    public static class PushBackBootPlan
    {
        private static readonly IReadOnlyList<ResolvedBoot> None = new List<ResolvedBoot>();

        /// <summary>Las botas fisicas del rack, con su identidad y su ancla de PLANTA.</summary>
        public static IReadOnlyList<ResolvedBoot> Resolve(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null || catalog == null)
            {
                return None;
            }

            var boots = SelectiveSafetyPlacement.EnabledOfType(
                structure.SafetySelections, catalog, "PLANTA", SelectiveSafetyPlacement.BotaType);
            var element = boots.FirstOrDefault();
            if (element?.Selection == null)
            {
                return None;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            if (layout?.PostPositions == null)
            {
                return None;
            }

            var plateId = DynamicFrontGeometry.PlateId(structure, catalog);
            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, "PLANTA");
            var guards = SelectiveSafetyPlacement.EnabledOfType(
                structure.SafetySelections, catalog, "PLANTA", SelectiveSafetyPlacement.LateralType,
                allowEmptySide: true);
            var sides = system.IsComposite
                ? new[] { PushBackSide.A, PushBackSide.B }
                : new[] { PushBackSide.A };

            var result = new List<ResolvedBoot>();
            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                // La seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se coloca (I-33).
                if (!DynamicFrontActivation.BoundaryExists(structure, postIndex))
                {
                    continue;
                }

                // Un PROTECTOR LATERAL recorre la linea entera y sustituye a las botas de esa linea (I-32). Es una
                // regla de la familia vecina, no de la bota, y se aplica aqui para que todas las vistas y el BOM la
                // vean igual.
                if (guards.Count > 0
                    && DynamicLateralGuardPlan.SideAt(
                        guards[0].Selection, postIndex, layout.PostPositions.Count) != SafetySide.None)
                {
                    continue;
                }

                foreach (var side in sides)
                {
                    var placement = side == PushBackSide.A
                        ? element.Selection.BootPlacementAt(postIndex)
                        : element.Selection.BootPlacementAtSideB(postIndex);
                    if (BootPlacements.IncludesEntryExit(placement))
                    {
                        result.Add(Piece(element, structure, layout, plateMate, postIndex, side, BootFace.EntryExit));
                    }

                    if (BootPlacements.IncludesRear(placement))
                    {
                        result.Add(Piece(element, structure, layout, plateMate, postIndex, side, BootFace.Rear));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Las botas que caen en el plano de un corte, que es lo unico que un corte decide. Un corte se identifica
        /// por su LADO y su EXTREMO, y la pieza ya llega con los suyos: no hay nada que reinterpretar.
        /// </summary>
        public static IReadOnlyList<ResolvedBoot> AtCut(
            PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
        {
            var cutSide = system != null && system.IsComposite ? side : PushBackSide.A;
            var face = end == PushBackFrontalEnd.Posterior ? BootFace.Rear : BootFace.EntryExit;
            return Resolve(system, catalog)
                .Where(boot => boot.Side == cutSide && boot.Face == face)
                .ToList();
        }

        /// <summary>True cuando esa instancia es una bota, segun el catalogo.</summary>
        public static bool IsBoot(HeaderBlockInstance instance, RackCatalog catalog)
            => instance != null
               && !string.IsNullOrWhiteSpace(instance.PieceId)
               && (catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>()).Any(entry =>
                   entry != null
                   && SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)
                   && string.Equals(entry.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// El mismo plan SIN sus botas. Las vistas retiran las que traiga el pipeline compartido y reponen las de
        /// esta autoridad: mientras haya dos sitios que las coloquen, hay dos respuestas posibles.
        /// </summary>
        public static HeaderRunPlan Without(HeaderRunPlan plan, RackCatalog catalog)
        {
            if (plan == null)
            {
                return null;
            }

            var groups = plan.Headers
                .Select(group => group?.Instances == null || !group.Instances.Any(i => IsBoot(i, catalog))
                    ? group
                    : new HeaderGroup(
                        group.Name,
                        group.Instances.Where(i => !IsBoot(i, catalog)).ToList(),
                        group.Placements))
                .Where(group => group != null && (group.Instances == null || group.Instances.Count > 0))
                .ToList();
            return new HeaderRunPlan(groups, plan.LooseInstances.Where(i => !IsBoot(i, catalog)).ToList());
        }

        /// <summary>La instancia de dibujo de una bota resuelta, en la vista y el ancla que esa vista le da.</summary>
        public static HeaderBlockInstance Instance(ResolvedBoot boot, string block, string view, Point2D at)
            => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = boot.PieceId,
                BlockName = block,
                View = view,
                Insertion = at,
                ConnectionAnchor = at,
                MirroredX = boot.Mirrored,
                MirroredY = false,
            };

        private static ResolvedBoot Piece(
            SelectiveSafetyPlacement.SafetyElement element,
            DynamicRackSystem structure,
            DynamicFrontLayout layout,
            Point2D plateMate,
            int postIndex,
            PushBackSide side,
            BootFace face)
        {
            // LA MANO la decide la cara: una bota mira hacia fuera de lo que protege. Las dos caras exteriores miran
            // en sentidos opuestos, y las dos interiores tambien —y al reves que las suyas—.
            var mirrored = (side == PushBackSide.A) == (face == BootFace.Rear);
            var faceX = FaceX(structure, layout, postIndex, side, face);
            return new ResolvedBoot
            {
                Side = side,
                Face = face,
                PostIndex = postIndex,
                FaceX = faceX,
                Mirrored = mirrored,
                PieceId = element.PieceId,
                PlantaAt = new Point2D(
                    mirrored ? faceX + plateMate.X : faceX - plateMate.X,
                    layout.PostPositions[postIndex] - plateMate.Y),
                LineX = layout.PostPositions[postIndex],
            };
        }

        /// <summary>
        /// La X del plano que esa cara protege. El EXTERIOR de un lado es el extremo de la cobertura de esa linea
        /// por su parte —el bajo para A, el alto para B—; el INTERIOR es su borde de la interfaz, que la estructura
        /// declara (ronda 6D). Un rack de un solo sentido no tiene interfaz: su posterior es el extremo alto.
        /// </summary>
        public static double FaceX(
            DynamicRackSystem structure, DynamicFrontLayout layout, int postIndex, PushBackSide side, BootFace face)
        {
            var depthRange = DynamicDepthGeometry.AtPost(structure, postIndex);
            var rangeStart = structure.Modules
                .FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
            var rangeEnd = structure.Modules
                .FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? structure.TotalLength;

            if (face == BootFace.EntryExit)
            {
                return side == PushBackSide.A ? rangeStart : rangeEnd;
            }

            if (side == PushBackSide.A)
            {
                return structure.InteriorFaceStartX ?? rangeEnd;
            }

            return structure.InteriorFaceEndX ?? rangeStart;
        }
    }
}
