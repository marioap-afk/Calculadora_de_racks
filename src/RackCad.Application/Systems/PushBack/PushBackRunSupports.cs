using System;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>El papel que una cama tiene EN UN PLANO DE CORTE concreto.</summary>
    public enum PushBackSupportRole
    {
        /// <summary>La cama no tiene ningun apoyo en ese plano: no se dibuja nada de ella.</summary>
        None = 0,

        /// <summary>Ahi empieza: el extremo por donde se carga y descarga.</summary>
        Low = 1,

        /// <summary>Ahi solo pasa: un apoyo intermedio, sin principio ni final.</summary>
        Intermediate = 2,

        /// <summary>Ahi termina: el extremo alto, el unico que admite tope.</summary>
        High = 3,
    }

    /// <summary>
    /// I-42 (ronda 8B) — LA AUTORIDAD DE CORTE: que apoyo de una cama coincide con un plano de corte.
    ///
    /// <para>
    /// Una vista NO decide su contenido a partir de como se llama. «Frontal» y «Posterior» dicen DONDE esta el
    /// plano, no que papel tiene la pieza que aparece alli: una cara exterior puede mostrar el extremo BAJO de las
    /// camas de su lado y a la vez el ALTO de una corrida que termina ahi, y una cara interior puede mostrar un
    /// apoyo INTERMEDIO de una corrida que solo la atraviesa — o nada, si la cama termino antes.
    /// </para>
    ///
    /// <para>
    /// La regla es la que el constructor del larguero intermedio ya aplicaba en el lateral, ahora dicha una sola vez
    /// y consultable por las cuatro vistas: una cama ocupa desde el arranque de su frente hasta la X de su larguero
    /// posterior (<see cref="PushBackCellDepth.RearX"/>), y en cada frontera de ese tramo tiene un apoyo. La
    /// frontera inicial es su BAJO, la final su ALTO, y las de en medio INTERMEDIOS.
    /// </para>
    ///
    /// <para>
    /// La tolerancia compara coordenadas que YA representan la misma identidad —la frontera de un modulo—, nunca
    /// elige que apoyo es. Y cuando dos lineas fisicas distintas comparten X porque el hueco es cero, quien
    /// desempata es el LADO al que pertenece el extremo, no la coordenada.
    /// </para>
    /// </summary>
    public static class PushBackRunSupports
    {
        /// <summary>Por debajo de una milesima de pulgada dos fronteras son la misma. Reutiliza la del span.</summary>
        public const double Tolerance = PushBackBedSpan.Tolerance;

        /// <summary>
        /// La X de mundo de la LINEA FISICA que un corte representa: el pasillo del lado
        /// (<see cref="PushBackFrontalEnd.EntradaSalida"/>) o su linea terminal interior
        /// (<see cref="PushBackFrontalEnd.Posterior"/>).
        /// </summary>
        public static double? CutX(PushBackSystem system, PushBackSide side, PushBackFrontalEnd end)
        {
            var view = system?.Composite?.Of(side);
            if (view == null || !view.IsPresent)
            {
                return null;
            }

            return end == PushBackFrontalEnd.EntradaSalida ? view.OuterX : view.InnerX;
        }

        /// <summary>
        /// Las dos fronteras de una cama, en X de MUNDO: donde empieza (su bajo) y donde termina (su alto). Salen de
        /// las autoridades que ya existen —el arranque del frente y <see cref="PushBackCellDepth.RearX"/>— y se
        /// llevan al mundo con la misma reflexion rigida que su contenido.
        /// </summary>
        public static (double LowX, double HighX)? BoundariesOf(PushBackRunSet runs, PushBackRun run)
        {
            var front = run?.Front();
            if (front == null || run.Source?.Structure == null)
            {
                return null;
            }

            var lowLocal = front.StartX;
            var highLocal = PushBackCellDepth.RearX(run.Source, front, run.SourceLevel);
            return run.Reflected
                ? (PushBackMirror.X(runs.MirrorAxis, lowLocal), PushBackMirror.X(runs.MirrorAxis, highLocal))
                : (lowLocal, highLocal);
        }

        /// <summary>
        /// EL PAPEL de <paramref name="run"/> en el corte <paramref name="side"/>/<paramref name="end"/>.
        ///
        /// <para>
        /// El extremo BAJO se muestra en el corte del lado que lo posee; el ALTO en el del lado que lo posee —que
        /// puede ser una cara exterior, si la cama termina en el pasillo del otro lado—; y un apoyo intermedio en
        /// cualquiera de las dos lineas interiores que la cama atraviese, porque son dos lineas fisicas distintas y
        /// la cama se apoya en las dos.
        /// </para>
        /// </summary>
        public static PushBackSupportRole At(
            PushBackSystem system,
            PushBackRunSet runs,
            PushBackRun run,
            PushBackSide side,
            PushBackFrontalEnd end)
        {
            var cut = CutX(system, side, end);
            var boundaries = BoundariesOf(runs, run);
            if (cut == null || boundaries == null)
            {
                return PushBackSupportRole.None;
            }

            var cutX = cut.Value;
            var (lowX, highX) = boundaries.Value;

            // El BAJO: siempre en el pasillo de su lado. El lado desempata cuando dos lineas comparten X.
            if (Math.Abs(cutX - lowX) <= Tolerance)
            {
                return run.LowSide == side ? PushBackSupportRole.Low : PushBackSupportRole.None;
            }

            // El ALTO: en la linea de su lado alto, sea esta interior o exterior.
            if (Math.Abs(cutX - highX) <= Tolerance)
            {
                return run.HighSide == side ? PushBackSupportRole.High : PushBackSupportRole.None;
            }

            var lo = Math.Min(lowX, highX);
            var hi = Math.Max(lowX, highX);
            return cutX > lo + Tolerance && cutX < hi - Tolerance
                ? PushBackSupportRole.Intermediate
                : PushBackSupportRole.None;
        }

        /// <summary>
        /// El TOPE pertenece EXCLUSIVAMENTE al extremo alto: solo puede existir donde el corte coincide con el, y
        /// nunca se busca por su cuenta. Esto es lo que impide que aparezca en una cara que la cama solo atraviesa,
        /// o en una que ya dejo atras.
        /// </summary>
        public static bool TopeAt(
            PushBackSystem system,
            PushBackRunSet runs,
            PushBackRun run,
            PushBackSide side,
            PushBackFrontalEnd end)
        {
            if (At(system, runs, run, side, end) != PushBackSupportRole.High)
            {
                return false;
            }

            var tope = run.Source?.RearTope ?? new PushBackRearTopeConfig();
            return tope.Draws(run.SourceFrontIndex, run.SourceLevel - 1);
        }
    }
}
