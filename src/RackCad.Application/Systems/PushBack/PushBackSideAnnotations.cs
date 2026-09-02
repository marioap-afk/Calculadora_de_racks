using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — las etiquetas A / B como INFORMACION GRAFICA. Reutilizan el pipeline de anotaciones que ya existe
    /// (<see cref="SelectiveAnnotations"/>, el mismo que consumen <see cref="DynamicViewDecorations"/> y las
    /// decoraciones del selectivo): mismo rol <see cref="HeaderBlockRole.Annotation"/>, misma altura de texto
    /// derivada de la escala de anotacion del rack y misma capa. NO se crea un segundo pipeline textual.
    ///
    /// <para>
    /// Son informacion de lectura del plano y NUNCA entran al BOM: el rol Annotation no lo cuenta ningun builder de
    /// materiales, igual que las tarimas de I-41.
    /// </para>
    /// <para>
    /// Se emiten donde el lector necesita saber a que pasillo mira: en PLANTA (los dos costados de la calle) y en
    /// los cortes LATERALES (los dos extremos de la profundidad). Los cortes frontales no las llevan: un corte
    /// frontal ES de un lado, y su titulo ya lo dice.
    /// </para>
    /// </summary>
    public static class PushBackSideAnnotations
    {
        /// <summary>Texto de la etiqueta de un lado.</summary>
        public static string Text(PushBackSide side) => side == PushBackSide.A ? "A" : "B";

        /// <summary>
        /// Las etiquetas del corte LATERAL: una en el extremo exterior de cada lado con ALMACENAMIENTO en las
        /// ranuras que este corte muestra, a la altura del suelo y por debajo de la estructura.
        ///
        /// <para>
        /// I-42 (ronda 8, V1) — antes se rotulaba todo lado DECLARADO. Eso es una propiedad del RACK, no de la
        /// seccion: un corte cuya unica ranura tiene el lado A en blanco salia igualmente con una «A», afirmando un
        /// almacenamiento que ahi no existe. La reticula fisica sigue completa —postes, placas y cabeceras no se
        /// mueven—; lo que la letra representa es el ALMACENAMIENTO de ese lado en lo que se esta mostrando.
        /// </para>
        /// </summary>
        /// <param name="shows">Que ranuras materializa este corte. NULL = el rack entero.</param>
        public static IReadOnlyList<HeaderBlockInstance> Lateral(
            PushBackSystem system, Func<int, bool> shows = null, double sectionHeight = 0.0)
        {
            var result = new List<HeaderBlockInstance>();
            var composite = system?.Composite;
            if (composite == null || system.Structure == null || !system.IsComposite)
            {
                return result;   // un rack de un solo sentido no tiene lados que distinguir
            }

            var scale = system.Structure.AnnotationScale > 0.0 ? system.Structure.AnnotationScale : 1.0;
            var height = SelectiveAnnotations.TextHeightFor(scale);
            var y = -(height + SelectiveAnnotations.Margin * scale);

            var functional = PushBackFunctionalSides.In(system, shows);
            Append(result, PushBackSide.A, composite.SideA, "LATERAL", y, height, functional);
            Append(result, PushBackSide.B, composite.SideB, "LATERAL", y, height, functional);
            return result;
        }

        /// <summary>
        /// Las etiquetas de PLANTA: una por lado con almacenamiento, en su extremo exterior, desplazadas hacia el
        /// pasillo de ese lado. Es la vista donde mas falta hacen, porque en planta los dos sentidos comparten la
        /// misma calle. La planta muestra el rack entero, asi que basta con que el lado almacene en alguna ranura.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> Planta(PushBackSystem system, double transverseY = 0.0)
        {
            var result = new List<HeaderBlockInstance>();
            var composite = system?.Composite;
            if (composite == null || system.Structure == null || !system.IsComposite)
            {
                return result;
            }

            var scale = system.Structure.AnnotationScale > 0.0 ? system.Structure.AnnotationScale : 1.0;
            var height = SelectiveAnnotations.TextHeightFor(scale);
            var y = transverseY - (height + SelectiveAnnotations.Margin * scale);

            var functional = PushBackFunctionalSides.In(system, shows: null);
            Append(result, PushBackSide.A, composite.SideA, "PLANTA", y, height, functional);
            Append(result, PushBackSide.B, composite.SideB, "PLANTA", y, height, functional);
            return result;
        }

        private static void Append(
            ICollection<HeaderBlockInstance> target,
            PushBackSide side,
            PushBackSideSystem view,
            string viewName,
            double y,
            double height,
            IReadOnlyList<PushBackSide> functional)
        {
            // La letra la autoriza el ALMACENAMIENTO de ese lado en lo que la vista muestra, no su declaracion.
            if (view == null || functional == null || !functional.Contains(side))
            {
                return;
            }

            target.Add(SelectiveAnnotations.Label(Text(side), viewName, new Point2D(view.OuterX, y), height));
        }
    }
}
