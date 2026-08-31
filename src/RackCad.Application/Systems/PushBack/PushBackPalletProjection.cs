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
    /// I-42 (A1B-D3) — UNA FILA DE TARIMAS YA RESUELTA para un plano de corte: la cama fisica que la sostiene, la
    /// celda cuya intencion la pide, y el papel que esa cama tiene en ese plano.
    ///
    /// <para>
    /// Su identidad es <b>cama fisica x plano de corte</b>. Una corrida es UNA cama aunque la miren los dos lados,
    /// asi que no puede producir dos filas por existir dos sistemas locales; dos camas encontradas son DOS camas
    /// aunque su proyeccion coincida, asi que no se colapsan por caer en la misma coordenada.
    /// </para>
    /// </summary>
    public sealed class ResolvedPalletRow
    {
        /// <summary>El lado del corte que la muestra.</summary>
        public PushBackSide Side { get; set; }

        /// <summary>El extremo del corte que la muestra.</summary>
        public PushBackFrontalEnd End { get; set; }

        /// <summary>Ranura transversal compartida de la celda (0-based).</summary>
        public int Slot { get; set; }

        /// <summary>Nivel de la celda compuesta (1-based).</summary>
        public int Level { get; set; }

        /// <summary>El papel de la cama en ESE plano. Nunca <see cref="PushBackSupportRole.None"/>: eso no se dibuja.</summary>
        public PushBackSupportRole Role { get; set; }

        /// <summary>La identidad de la CAMA que la sostiene. Dos filas de la misma cama en el mismo plano son una.</summary>
        public string RunIdentity { get; set; }

        /// <summary>Indice del frente en el sistema LOCAL del lado: la columna donde este corte la dibuja.</summary>
        public int LocalFrontIndex { get; set; }

        /// <summary>Calles de la celda: una tarima por calle.</summary>
        public int Lanes { get; set; }

        /// <summary>X del arranque del larguero en el marco del corte.</summary>
        public double AnchorX { get; set; }

        /// <summary>Longitud del larguero de la celda, sobre la que se reparten las calles.</summary>
        public double BeamLength { get; set; }

        /// <summary>Ancho de calle (BFR) de la celda.</summary>
        public double Bfr { get; set; }

        /// <summary>Frente de la tarima de ESA celda.</summary>
        public double PalletFront { get; set; }

        /// <summary>Alto de la tarima de ESA celda.</summary>
        public double PalletHeight { get; set; }

        /// <summary>Y de la superficie de apoyo —los rodillos— de la cama EN EL PLANO de este corte.</summary>
        public double SupportY { get; set; }

        /// <summary>Identidad completa de la proyeccion: una cama en un plano.</summary>
        public string Identity => FormattableString.Invariant($"{RunIdentity}|{Side}/{End}");
    }

    /// <summary>
    /// I-42 (A1B-D3, contrato del dueño) — LA PROYECCION FISICA DE LAS TARIMAS en los cortes de un rack compuesto.
    ///
    /// <para>
    /// <b>La regla.</b> Una celda proyecta tarima en un plano si —y solo si— la CAMA que la sirve tiene un apoyo en
    /// ese plano (<see cref="PushBackRunSupports.At"/> distinto de <see cref="PushBackSupportRole.None"/>) y la celda
    /// pide dibujarla. Los tres papeles con apoyo la muestran, porque los tres significan que la cama corta ese
    /// plano: en el BAJO se ve la primera posicion, en un INTERMEDIO la que lo cruza, en el ALTO la ultima. NONE
    /// significa que ahi no hay cama —ni antes de empezar, ni despues de terminar— y ahi no se dibuja nada.
    /// </para>
    /// <para>
    /// <b>Lo que corrige.</b> Un corte compuesto se arma en tres pasadas sobre el sistema LOCAL de su lado, y las
    /// tarimas se agregaban DENTRO de cada pasada recorriendo otra vez todas las celdas del local, sin el filtro de
    /// celdas por papel y sin preguntar por la cama. Medido antes del cambio, con 2 ranuras x 2 niveles: los CUATRO
    /// cortes de los seis escenarios dibujaban las mismas 8 tarimas —dos filas por celda, la del extremo bajo local
    /// y la del posterior local—, incluso en cortes donde ninguna cama tiene apoyo: el lado B de un rack Solo A, el
    /// lado A de un Solo B, y la cara interior de un lado cuya cama termina antes. Una corrida, que es UNA cama,
    /// aportaba fila por cada uno de los dos locales.
    /// </para>
    /// <para>
    /// <b>Que autoridad da que.</b> La existencia y el papel, <see cref="PushBackRunSupports"/>; la cama y su marco,
    /// <see cref="PushBackRuns"/>; la altura de apoyo, la del propio eje de la cama
    /// (<see cref="PushBackTarimaPlacement.SupportYAt"/>) evaluada en el plano del corte; la intencion, la celda
    /// (<c>DrawPallet</c>, fondo, BFR y alto de tarima), intacta desde I-41. Lo unico que aporta el corte es su
    /// marco transversal —donde cae la columna—, que es lo que el resto del corte ya usa; no se asume que los dos
    /// lados tengan la misma retícula.
    /// </para>
    /// </summary>
    public static class PushBackPalletProjection
    {
        private static readonly IReadOnlyList<ResolvedPalletRow> None = new List<ResolvedPalletRow>();

        /// <summary>La identidad de una CAMA fisica: su ranura, su nivel y sus dos extremos.</summary>
        public static string RunIdentityOf(PushBackRun run)
            => run == null
                ? string.Empty
                : FormattableString.Invariant($"S{run.Slot}|N{run.Level}|{run.Topology}|{run.LowSide}->{run.HighSide}");

        /// <summary>Las filas de tarimas que un corte compuesto materializa.</summary>
        public static IReadOnlyList<ResolvedPalletRow> Resolve(
            PushBackSystem system, RackCatalog catalog, PushBackSide side, PushBackFrontalEnd end)
        {
            var view = system?.Composite?.Of(side);
            if (view == null || !view.IsPresent || view.Local?.Structure == null || catalog == null)
            {
                return None;
            }

            var cut = PushBackRunSupports.CutX(system, side, end);
            if (cut == null)
            {
                return None;
            }

            var runs = PushBackRuns.Resolve(system);
            var localStructure = view.Local.Structure;
            var layout = DynamicFrontGeometry.Compute(localStructure, catalog);
            var columns = Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count);
            var supportLocalY = PushBackTarimaPlacement.SupportLocalY(catalog);
            var result = new List<ResolvedPalletRow>();

            foreach (var run in runs.Runs)
            {
                // EL GATE: el papel de la cama en ESTE plano. Sin apoyo no hay tarima, y no se busca el corte
                // siguiente ni se sustituye por «posterior = alto».
                var role = PushBackRunSupports.At(system, runs, run, side, end);
                if (role == PushBackSupportRole.None)
                {
                    continue;
                }

                var localIndex = LocalIndex(view, run.Slot);
                if (localIndex < 0 || localIndex >= columns || localIndex >= localStructure.Fronts.Count)
                {
                    continue;   // ese lado no dibuja esa ranura: no tiene columna donde ponerla
                }

                // LA INTENCION (I-41), intacta: la celda de la cama dice si su tarima se dibuja.
                if (!run.Source.DrawPalletAt(run.SourceFrontIndex, run.SourceLevel - 1))
                {
                    continue;
                }

                var front = run.Front();
                if (front == null)
                {
                    continue;
                }

                var axis = PushBackFlowBedGeometry.Resolve(run.Source, catalog, front)
                    .FirstOrDefault(candidate => candidate.LevelNumber == run.SourceLevel);
                if (axis.LevelNumber != run.SourceLevel)
                {
                    continue;   // sin eje de cama no hay superficie de apoyo, y no se inventa una
                }

                var cell = DynamicRackLevelGeometry.At(run.Source.Structure, front, run.SourceLevel);
                var palletFront = cell?.Pallet?.Front ?? 0.0;
                var palletHeight = cell?.Pallet?.Height ?? 0.0;
                if (palletFront <= 0.0 || palletHeight <= 0.0)
                {
                    continue;
                }

                // DONDE se mide la altura de apoyo, en el marco de la cama: en un extremo, SU contacto —que es el
                // ancla que I-41 valido, y no la linea de postes, de la que la cama arranca un poco adentro—; en un
                // apoyo intermedio, el plano del corte, que es el unico punto que ese papel nombra. El plano se
                // lleva al marco de la cama con la MISMA reflexion rigida que la lleva a ella al mundo; la Y no se
                // refleja, porque el espejo es en X.
                var planeX = run.Reflected ? PushBackMirror.X(runs.MirrorAxis, cut.Value) : cut.Value;
                var sourceX = role == PushBackSupportRole.Low
                    ? axis.ExitMate.X
                    : role == PushBackSupportRole.High ? axis.HighMate.X : planeX;

                result.Add(new ResolvedPalletRow
                {
                    Side = side,
                    End = end,
                    Slot = run.Slot,
                    Level = run.Level,
                    Role = role,
                    RunIdentity = RunIdentityOf(run),
                    LocalFrontIndex = localIndex,
                    Lanes = Math.Max(1, front.PalletCount),
                    AnchorX = layout.PostPositions[localIndex] + layout.TroquelPositions[localIndex],
                    BeamLength = PushBackLoadBeamGeometry.CellBeamLength(
                        run.Source.Structure, front, run.SourceLevel),
                    Bfr = cell?.Bfr ?? 0.0,
                    PalletFront = palletFront,
                    PalletHeight = palletHeight,
                    SupportY = PushBackTarimaPlacement.SupportYAt(axis, supportLocalY, sourceX),
                });
            }

            return result;
        }

        /// <summary>Las instancias de dibujo de unas filas resueltas, con la MISMA regla de reparto de I-41.</summary>
        public static IReadOnlyList<HeaderBlockInstance> Instances(
            IEnumerable<ResolvedPalletRow> rows, RackCatalog catalog)
            => (rows ?? Enumerable.Empty<ResolvedPalletRow>())
                .SelectMany(row => PushBackTarimaPlacement.FrontalRow(
                    catalog, row.Lanes, row.AnchorX, row.BeamLength, row.Bfr, row.SupportY,
                    row.PalletFront, row.PalletHeight))
                .ToList();

        /// <summary>True cuando esa instancia es una tarima (referencia visual, nunca BOM).</summary>
        public static bool IsPallet(HeaderBlockInstance instance)
            => instance != null && instance.Role == HeaderBlockRole.Pallet;

        /// <summary>El mismo plan SIN sus tarimas: lo que un corte compuesto retira antes de reponer las resueltas.</summary>
        public static HeaderRunPlan Without(HeaderRunPlan plan)
        {
            if (plan == null)
            {
                return null;
            }

            var groups = plan.Headers
                .Select(group => group?.Instances == null || !group.Instances.Any(IsPallet)
                    ? group
                    : new HeaderGroup(
                        group.Name,
                        group.Instances.Where(instance => !IsPallet(instance)).ToList(),
                        group.Placements))
                .Where(group => group != null && (group.Instances == null || group.Instances.Count > 0))
                .ToList();
            return new HeaderRunPlan(groups, plan.LooseInstances.Where(instance => !IsPallet(instance)).ToList());
        }

        private static int LocalIndex(PushBackSideSystem view, int slot)
            => slot >= 0 && slot < view.LocalIndexBySlot.Count ? view.LocalIndexBySlot[slot] : -1;
    }
}
