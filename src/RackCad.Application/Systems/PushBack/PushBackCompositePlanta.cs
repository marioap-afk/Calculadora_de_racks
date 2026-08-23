using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — la PLANTA de un rack compuesto. Es la vista donde los dos sentidos comparten la misma calle, asi que es
    /// la que mas necesita distinguirlos.
    ///
    /// <para>
    /// Se compone como el lateral: la ESTRUCTURA se dibuja UNA vez, desde el sistema compuesto (cabeceras,
    /// separadores —el central incluido—, postes derivados, placas y seguridad), y encima se monta el contenido de
    /// cada lado, tomado de su planta local y llevado al rack con la MISMA reflexion rigida que usa todo lo demas del
    /// lado B. Ningun poste, cabecera ni placa se dibuja dos veces por el hecho de que el rack tenga dos sentidos.
    /// </para>
    /// <para>
    /// LIMITACION DECLARADA: la planta colapsa los niveles, asi que una ranura en la que TODOS los niveles fueran
    /// camas corridas seguiria mostrando los largueros posteriores de la interfaz. En cuanto un solo nivel de esa
    /// ranura no sea corrido, esos largueros existen de verdad. Se declara en vez de resolverse con una regla que la
    /// planta no puede sostener (no tiene nivel al que preguntar).
    /// </para>
    /// </summary>
    public static class PushBackCompositePlanta
    {
        public static HeaderRunPlan Build(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            var composite = system?.Composite;
            if (structure == null || composite == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            // 1) La estructura, una sola vez. Se retiran los largueros de extremo del dinamico: los aporta cada lado.
            var result = new DynamicSystemPlantaBuilder()
                .BuildPlan(structure, catalog)
                .Flatten()
                .Instances
                .Where(instance => !PushBackPlanComposer.IsDynamicSpecific(instance))
                .ToList();

            // 2) El contenido de cada lado, en su marco, reflejado el del lado B.
            AppendSide(result, system, catalog, composite.SideA, reflected: false, mirrorAxis: 0.0);
            AppendSide(result, system, catalog, composite.SideB, reflected: true,
                mirrorAxis: PushBackMirror.AxisOf(structure));

            // 3) Las etiquetas A/B, por el pipeline de anotaciones que ya existe. Nunca al BOM.
            result.AddRange(PushBackSideAnnotations.Planta(system));

            return HeaderInstanceGrouper.Group(result, "PB_PLANTA_PIEZA");
        }

        /// <summary>
        /// El contenido de un lado: su larguero IN/OUT del pasillo, su larguero posterior de troquel redondo y su
        /// tope. Se toma de la planta LOCAL del lado —el mismo builder de un Push Back de un sentido— y se descartan
        /// sus piezas estructurales, que ya vienen del sistema compuesto.
        /// </summary>
        private static void AppendSide(
            List<HeaderBlockInstance> target,
            PushBackSystem system,
            RackCatalog catalog,
            PushBackSideSystem side,
            bool reflected,
            double mirrorAxis)
        {
            if (side == null || !side.IsPresent || side.Local == null)
            {
                return;
            }

            var highId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var inOutId = string.IsNullOrWhiteSpace(side.Local.Structure?.InOutBeamCatalogId)
                ? DynamicRackDefaults.InOutBeamCatalogId
                : side.Local.Structure.InOutBeamCatalogId;

            var content = new PushBackSystemPlantaBuilder()
                .BuildPlan(side.Local, catalog)
                .Flatten()
                .Instances
                .Where(instance => IsLoadPiece(instance, inOutId, highId))
                .ToList();

            target.AddRange(reflected ? PushBackMirror.Instances(content, mirrorAxis) : content);
        }

        /// <summary>
        /// Lo que pertenece a una CALLE y no a la estructura: el larguero de entrada/salida, el posterior de troquel
        /// redondo y el tope. Se identifica por PieceId/Role, nunca por nombre de grupo.
        /// </summary>
        private static bool IsLoadPiece(HeaderBlockInstance instance, string inOutId, string highId)
            => instance != null
               && (instance.Role == HeaderBlockRole.Tope
                   || string.Equals(instance.PieceId, inOutId, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase));
    }
}
