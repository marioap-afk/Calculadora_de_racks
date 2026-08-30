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
        /// <summary>El pasillo por el que ese lado carga y descarga.</summary>
        EntryExit = 0,

        /// <summary>La cara opuesta de ese lado, que puede necesitar proteccion aunque no se cargue por ella.</summary>
        Rear = 1,
    }

    /// <summary>
    /// I-42 (S1E) — UNA BOTA FISICA YA RESUELTA: quien la pidio, donde va y como va.
    /// </summary>
    public sealed class ResolvedBoot
    {
        /// <summary>El lado que la pidio. Cuando los dos lados nombran la misma cara, se atribuye al DUEÑO del pasillo.</summary>
        public PushBackSide Side { get; set; }

        /// <summary>La cara de ese lado — leida dentro del lado, no en el marco del rack.</summary>
        public BootFace Face { get; set; }

        /// <summary>La linea de postes.</summary>
        public int PostIndex { get; set; }

        /// <summary>La cara FISICA de esa linea: true = la lejana del rack.</summary>
        public bool AtHighEnd { get; set; }

        /// <summary>Su mano, funcion SOLO de la cara fisica (I-42, S1B).</summary>
        public bool Mirrored { get; set; }

        public string PieceId { get; set; }

        /// <summary>El ancla en PLANTA, ya con la placa descontada.</summary>
        public Point2D PlantaAt { get; set; }

        /// <summary>La X de la linea del poste, que es donde la ancla un corte frontal.</summary>
        public double LineX { get; set; }
    }

    /// <summary>
    /// I-42 (S1E, contrato del dueño) — LA RESOLUCION FISICA DE LAS BOTAS, una sola vez para todo el rack.
    ///
    /// <para>
    /// Antes cada vista resolvia la intencion por su cuenta y en su propio marco: la planta leia una eleccion global
    /// como una cara del rack, el corte del lado B leia la MISMA eleccion como su propia entrada, y el BOM contaba
    /// la planta. Medido en un compuesto sin blancos con la general en «Entrada/Salida»: planta 5 piezas en el
    /// pasillo de A, corte de A 5, corte de B 5 mas —que en planta no existian— y BOM 5.
    /// </para>
    /// <para>
    /// Desde S1E la intencion es POR LADO —cada uno con su general y sus postes— y se convierte a caras fisicas UNA
    /// vez, en <see cref="SelectiveSafetySelection.BootFacesAt"/>. Esta autoridad la proyecta a piezas con identidad
    /// suficiente (lado, poste, cara, ancla y mano) y las vistas solo eligen cuales caen en su plano y las
    /// transforman: ninguna vuelve a decidir quien lleva pieza.
    /// </para>
    /// </summary>
    public static class PushBackBootPlan
    {
        private static readonly IReadOnlyList<ResolvedBoot> None = new List<ResolvedBoot>();

        /// <summary>Las botas fisicas del rack, en el marco de la PLANTA.</summary>
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

            var result = new List<ResolvedBoot>();
            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                // La seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se coloca (I-33).
                if (!DynamicFrontActivation.BoundaryExists(structure, postIndex))
                {
                    continue;
                }

                // Un PROTECTOR LATERAL recorre la linea entera y sustituye a las botas de esa linea (I-32). Es una
                // regla de la familia vecina, no de la bota, y se aplica aqui para que las tres vistas y el BOM la
                // vean igual.
                if (guards.Count > 0
                    && DynamicLateralGuardPlan.SideAt(
                        guards[0].Selection, postIndex, layout.PostPositions.Count) != SafetySide.None)
                {
                    continue;
                }

                var faces = element.Selection.BootFacesAt(postIndex);
                if (!faces.Any)
                {
                    continue;
                }

                var depthRange = DynamicDepthGeometry.AtPost(structure, postIndex);
                var rangeStart = structure.Modules
                    .FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = structure.Modules
                    .FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? structure.TotalLength;
                var at = new Point2D(rangeStart - plateMate.X, layout.PostPositions[postIndex] - plateMate.Y);
                var mirrorAxisX = (rangeStart + rangeEnd) / 2.0;

                if (faces.Near)
                {
                    result.Add(Piece(element, postIndex, atHighEnd: false, at, mirrorAxisX, layout, system));
                }

                if (faces.Far)
                {
                    result.Add(Piece(element, postIndex, atHighEnd: true, at, mirrorAxisX, layout, system));
                }
            }

            return result;
        }

        /// <summary>
        /// Las botas que caen en el plano de un corte frontal, que es lo unico que un corte decide. Un compuesto no
        /// tiene ninguna en sus cortes POSTERIORES: ese plano es la interfaz entre los dos lados, y ahi no hay
        /// ancla de bota —las dos que tiene una linea son sus extremos—.
        /// </summary>
        public static IReadOnlyList<ResolvedBoot> AtCut(
            PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
        {
            var plane = CutHighEnd(system, end, side);
            return plane == null
                ? None
                : Resolve(system, catalog).Where(boot => boot.AtHighEnd == plane.Value).ToList();
        }

        /// <summary>
        /// La cara FISICA que ese corte muestra, o NULL si el corte no mira a ninguna cara con ancla de bota. En un
        /// rack de un sentido el corte bajo es la cara cercana y el posterior la lejana; en uno compuesto cada lado
        /// mira a SU pasillo —el de A la cercana, el de B la lejana— y los posteriores miran a la interfaz.
        /// </summary>
        public static bool? CutHighEnd(PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
        {
            if (system != null && system.IsComposite)
            {
                return end == PushBackFrontalEnd.Posterior ? (bool?)null : PushBackDefenseSides.IsFarEnd(side);
            }

            return end == PushBackFrontalEnd.Posterior;
        }

        private static ResolvedBoot Piece(
            SelectiveSafetyPlacement.SafetyElement element,
            int postIndex,
            bool atHighEnd,
            Point2D at,
            double mirrorAxisX,
            DynamicFrontLayout layout,
            PushBackSystem system)
        {
            var x = atHighEnd ? 2.0 * mirrorAxisX - at.X : at.X;
            var owner = Owner(element.Selection, postIndex, atHighEnd, system);
            return new ResolvedBoot
            {
                Side = owner.Side,
                Face = owner.Face,
                PostIndex = postIndex,
                AtHighEnd = atHighEnd,
                Mirrored = SelectiveSafetyEnds.Mirror(farEnd: atHighEnd),
                PieceId = element.PieceId,
                PlantaAt = new Point2D(x, at.Y),
                LineX = layout.PostPositions[postIndex],
            };
        }

        /// <summary>
        /// A quien se le atribuye una cara fisica. Se prefiere el DUEÑO DEL PASILLO —la cara cercana es la entrada
        /// del lado A y la lejana la del lado B—, y solo si ese lado no la pidio se atribuye a la posterior del
        /// contrario. La atribucion es identidad, no pertenencia: la pieza es la misma la pida quien la pida.
        /// </summary>
        private static (PushBackSide Side, BootFace Face) Owner(
            SelectiveSafetySelection selection, int postIndex, bool atHighEnd, PushBackSystem system)
        {
            var aisleSide = atHighEnd ? PushBackSide.B : PushBackSide.A;
            var aisle = aisleSide == PushBackSide.A
                ? selection.BootPlacementAt(postIndex)
                : selection.BootPlacementAtSideB(postIndex);
            if (BootPlacements.IncludesEntryExit(aisle) && (aisleSide == PushBackSide.A || (system?.IsComposite ?? false)))
            {
                return (aisleSide, BootFace.EntryExit);
            }

            var otherSide = aisleSide == PushBackSide.A ? PushBackSide.B : PushBackSide.A;
            return (otherSide, BootFace.Rear);
        }
    }
}
