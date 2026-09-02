using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el EJE de una cama fisica ya en coordenadas de RACK: sus dos contactos, su pendiente y su longitud
    /// comercial. Es la lectura neutral que consumen las vistas, el editor y las pruebas para preguntar «donde
    /// empieza y donde acaba esta cama», sin tener que saber en que marco se resolvio.
    /// </summary>
    public readonly struct PushBackRunAxis
    {
        public PushBackRunAxis(
            int slot, int level, Point2D lowContact, Point2D highContact, double slope, double length)
        {
            Slot = slot;
            Level = level;
            LowContact = lowContact;
            HighContact = highContact;
            Slope = slope;
            Length = length;
        }

        public int Slot { get; }
        public int Level { get; }

        /// <summary>Contacto del extremo BAJO (el pasillo por el que se carga), en coordenadas de rack.</summary>
        public Point2D LowContact { get; }

        /// <summary>Contacto del extremo ALTO (donde va el tope), en coordenadas de rack.</summary>
        public Point2D HighContact { get; }

        /// <summary>
        /// Pendiente en coordenadas de rack. Es NEGATIVA cuando la cama fluye hacia -X: cambiar el sentido cambia
        /// fisicamente que extremo es alto, no solo el dibujo.
        /// </summary>
        public double Slope { get; }

        /// <summary>Longitud comercial de la cama. Una corrida tiene UNA sola, la del rack entero.</summary>
        public double Length { get; }

        /// <summary>True cuando la cama avanza hacia +X en coordenadas de rack.</summary>
        public bool FlowsForward => HighContact.X > LowContact.X;
    }

    /// <summary>
    /// I-42 — resuelve el eje de cada cama fisica y lo devuelve en coordenadas de rack. La fisica no se recalcula:
    /// se pregunta a <see cref="PushBackFlowBedGeometry"/> en el marco de la cama y se aplica la MISMA reflexion
    /// rigida que llevara al mundo el riel, los rodillos, la tarima y el tope.
    /// </summary>
    public static class PushBackRunGeometry
    {
        /// <summary>El eje de una cama, o null si su marco no produjo ninguno (celda sin subida, por ejemplo).</summary>
        public static PushBackRunAxis? Axis(PushBackRun run, RackCatalog catalog, double mirrorAxis)
        {
            if (run?.Source == null)
            {
                return null;
            }

            var front = run.Front();
            var local = PushBackFlowBedGeometry.Resolve(run.Source, catalog, front)
                .FirstOrDefault(axis => axis.LevelNumber == run.SourceLevel);
            if (local.LevelNumber != run.SourceLevel)
            {
                return null;
            }

            var length = PushBackCellDepth.BedLength(run.Source, front, run.SourceLevel);
            var low = local.ExitMate;
            var high = local.HighMate;
            var slope = local.Slope;

            if (run.Reflected)
            {
                low = new Point2D(PushBackMirror.X(mirrorAxis, low.X), low.Y);
                high = new Point2D(PushBackMirror.X(mirrorAxis, high.X), high.Y);
                slope = -slope;
            }

            return new PushBackRunAxis(run.Slot, run.Level, low, high, slope, length);
        }

        /// <summary>Los ejes de todas las camas de un rack compuesto, en coordenadas de rack.</summary>
        public static IReadOnlyList<PushBackRunAxis> Axes(PushBackRunSet set, RackCatalog catalog)
        {
            var result = new List<PushBackRunAxis>();
            if (set == null)
            {
                return result;
            }

            foreach (var run in set.Runs)
            {
                var axis = Axis(run, catalog, set.MirrorAxis);
                if (axis.HasValue)
                {
                    result.Add(axis.Value);
                }
            }

            return result;
        }
    }
}
