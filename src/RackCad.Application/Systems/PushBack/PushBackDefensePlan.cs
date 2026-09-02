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
    /// <summary>
    /// I-42 (A1B-D2) — UNA DEFENSA DE MONTACARGAS FISICA YA RESUELTA. Su identidad es <b>lado × linea de postes</b>,
    /// y lleva consigo la PIEZA de su lado: el tipo es de quien la pidio, no de quien la mira.
    /// </summary>
    public sealed class ResolvedDefense
    {
        /// <summary>El lado al que pertenece. Nunca se deduce de una coordenada ni de un espejo.</summary>
        public PushBackSide Side { get; set; }

        /// <summary>La linea transversal de postes a la que se atornilla.</summary>
        public int PostLine { get; set; }

        /// <summary>La pieza de ESE lado. «Ninguno» no llega hasta aqui: no es una pieza fisica.</summary>
        public string PieceId { get; set; }

        /// <summary>La longitud resuelta de esa cara, que es lo que el dibujo materializa.</summary>
        public double Length { get; set; }

        /// <summary>La X de su linea de postes, que es donde la ancla un corte frontal.</summary>
        public double LineX { get; set; }

        /// <summary>La identidad fisica: dos defensas con la misma identidad son la MISMA pieza.</summary>
        public string Identity => FormattableString.Invariant($"{Side}|L{PostLine}");
    }

    /// <summary>
    /// I-42 (A1B-D2, contrato del dueño) — LA RESOLUCION FISICA DE LA DEFENSA DE MONTACARGAS.
    ///
    /// <para>
    /// La configuracion es <b>lado × poste fisico</b>, con TIPO independiente por lado, y esta autoridad la
    /// convierte en piezas: para cada linea y cada lado, la pieza que ESE lado declaro —<see cref="DynamicDefenseFaces"/>
    /// resuelve la cara— si esa linea tiene de verdad cara de ataque de ese lado y si su longitud resuelta es
    /// positiva. «Ninguno» no produce ninguna pieza.
    /// </para>
    /// <para>
    /// <b>Lo que corrige.</b> Los cuatro cortes de un rack compuesto se construyen sobre el sistema LOCAL de cada
    /// lado, y ese local no lleva las caras declaradas: al preguntar por su pieza caia al id general de la seleccion
    /// y dibujaba defensa igual. Medido: con A = pieza y B = «Ninguno», el corte de entrada de B dibujaba sus tres
    /// defensas; con A = «Ninguno» y B = pieza, las dibujaba el de A. La planta y el BOM ya resolvian bien —tres
    /// piezas, en el pasillo correcto—, asi que el defecto vivia solo en la proyeccion de los cortes.
    /// </para>
    /// </summary>
    public static class PushBackDefensePlan
    {
        private static readonly IReadOnlyList<ResolvedDefense> None = new List<ResolvedDefense>();

        /// <summary>Las defensas fisicas del rack, con su lado, su linea y la pieza de ese lado.</summary>
        public static IReadOnlyList<ResolvedDefense> Resolve(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null || catalog == null)
            {
                return None;
            }

            var selection = SelectiveSafetyFamilies.SelectedOfType(
                structure.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            if (selection == null)
            {
                return None;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            if (layout?.PostPositions == null)
            {
                return None;
            }

            var sides = system.IsComposite
                ? new[] { PushBackSide.A, PushBackSide.B }
                : new[] { PushBackSide.A };
            var postCount = layout.PostPositions.Count;
            var result = new List<ResolvedDefense>();

            for (var postIndex = 0; postIndex < postCount; postIndex++)
            {
                // La seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se coloca (I-33).
                if (!DynamicFrontActivation.BoundaryExists(structure, postIndex))
                {
                    continue;
                }

                var setting = DynamicForkliftDefensePlan.ForSelection(selection, postIndex, postCount);
                foreach (var side in sides)
                {
                    // PERTENENCIA: esa linea lleva defensa de este lado solo si ESE extremo mira a un pasillo. Es la
                    // misma pregunta que hace el dibujo, no una copia (I-42 ronda 7D).
                    if (!PushBackDefenseSides.HasFace(structure, postIndex, side))
                    {
                        continue;
                    }

                    // EL TIPO ES DEL LADO: cada cara declara su pieza, y «Ninguno» no materializa ninguna.
                    var piece = DynamicDefenseFaces.ElementIdFor(
                        selection, farEnd: PushBackDefenseSides.IsFarEnd(side));
                    if (string.IsNullOrWhiteSpace(piece))
                    {
                        continue;
                    }

                    // Y la LONGITUD resuelta de esa cara dice si esa linea la lleva de verdad.
                    var length = PushBackDefenseSides.Resolved(setting, side);
                    if (length <= 0.0)
                    {
                        continue;
                    }

                    result.Add(new ResolvedDefense
                    {
                        Side = side,
                        PostLine = postIndex,
                        PieceId = piece,
                        Length = length,
                        LineX = layout.PostPositions[postIndex],
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Las defensas que caen en el plano de un corte. Una defensa protege el pasillo de CARGA de su lado, asi
        /// que sale en el corte de entrada/salida de ese lado y en ninguno de los otros tres.
        /// </summary>
        public static IReadOnlyList<ResolvedDefense> AtCut(
            PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
        {
            if (end == PushBackFrontalEnd.Posterior)
            {
                return None;
            }

            var cutSide = system != null && system.IsComposite ? side : PushBackSide.A;
            return Resolve(system, catalog).Where(defense => defense.Side == cutSide).ToList();
        }

        /// <summary>True cuando esa instancia es una defensa de montacargas, segun el catalogo.</summary>
        public static bool IsDefense(HeaderBlockInstance instance, RackCatalog catalog)
            => instance != null
               && !string.IsNullOrWhiteSpace(instance.PieceId)
               && SelectiveSafetyFamilies
                   .VariantsOfType(catalog?.SafetyElements, SelectiveSafetyDefaults.DefensaType)
                   .Any(entry => string.Equals(entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));

        /// <summary>El mismo plan SIN sus defensas: lo que una vista retira antes de reponer las resueltas.</summary>
        public static HeaderRunPlan Without(HeaderRunPlan plan, RackCatalog catalog)
        {
            if (plan == null)
            {
                return null;
            }

            var groups = plan.Headers
                .Select(group => group?.Instances == null || !group.Instances.Any(i => IsDefense(i, catalog))
                    ? group
                    : new HeaderGroup(
                        group.Name,
                        group.Instances.Where(i => !IsDefense(i, catalog)).ToList(),
                        group.Placements))
                .Where(group => group != null && (group.Instances == null || group.Instances.Count > 0))
                .ToList();
            return new HeaderRunPlan(groups, plan.LooseInstances.Where(i => !IsDefense(i, catalog)).ToList());
        }

        /// <summary>La instancia de dibujo de una defensa resuelta en un corte FRONTAL.</summary>
        public static HeaderBlockInstance Instance(
            ResolvedDefense defense, RackCatalog catalog, string plateId, PushBackSide side)
        {
            var block = CatalogLookup.Block(catalog, defense.PieceId, View);
            if (string.IsNullOrWhiteSpace(block))
            {
                return null;
            }

            // La MANO y el sentido del vuelo los decide el pasillo: el de B mira al otro lado, como el corte espejo
            // que lo dibuja. Es orientacion, no pertenencia.
            var farEnd = PushBackDefenseSides.IsFarEnd(side);
            var offset = CatalogLookup.Local(catalog, defense.PieceId, DynamicForkliftDefensePlan.PostOriginPoint, View);
            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, View);
            var at = new Point2D(
                defense.LineX + (farEnd ? -1.0 : 1.0) * offset.X,
                -plateMate.Y + offset.Y);
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = defense.PieceId,
                BlockName = block,
                View = View,
                Insertion = at,
                ConnectionAnchor = at,
                MirroredX = farEnd,
                MirroredY = false,
            };
            if (defense.Length > 0.0)
            {
                instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = defense.Length;
            }

            return instance;
        }

        private const string View = "FRONTAL";
    }
}
