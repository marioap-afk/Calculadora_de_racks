using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 (ronda 6C) — LA DEMANDA FISICA DE ALTURA DE UNA CABECERA, resuelta sobre las CAMAS REALES.
    ///
    /// <para>
    /// La estructura de un rack compuesto es una sola: A + hueco + B invertido. Sus frentes se resuelven con esa
    /// profundidad SINTETICA, y el resolver dinamico deriva de ella la elevacion de entrada del ultimo nivel, que es
    /// la que gobierna la altura de la cabecera. Pero ninguna cama recorre esa profundidad: en camas ENCONTRADAS hay
    /// dos camas de cinco fondos, no una de once. Medido: la entrada del nivel alto pasaba de 86.6053 —lo que un
    /// rack simple de cinco fondos resuelve— a 96.6053, y la cabecera de 120" a 132". Un pie comercial de mas, por
    /// una cama que no existe.
    /// </para>
    /// <para>
    /// Aqui NO hay una formula nueva. La regla de cabecera es la de siempre
    /// (<see cref="DynamicHeaderHeightCalculator"/>): la elevacion de entrada del ultimo nivel, mas el peralte de su
    /// larguero, mas un tercio del espacio libre, redondeado al pie comercial. Lo unico que cambia es el INPUT: la
    /// elevacion sale de la cama REAL y no de la profundidad compuesta.
    /// </para>
    /// <para>
    /// Una CORRIDA si atraviesa fisicamente los dos lados, y su demanda sale de su propia cama: por eso la regla
    /// pregunta por cama y no por topologia.
    /// </para>
    /// </summary>
    public static class PushBackHeaderHeight
    {
        /// <summary>
        /// La altura de cabecera que UNA cama exige. Es
        /// <see cref="DynamicHeaderHeightCalculator.CalculateResolved(DynamicRackFront)"/> aplicado al frente de esa
        /// cama —en su propio marco, con su propia profundidad— y con una salvaguarda: si el larguero ALTO que la
        /// cama dibuja de verdad esta por encima de la entrada que su frente resolvio, manda el larguero. Las dos son
        /// elevaciones de la MISMA pieza fisica y la cabecera tiene que contener la mas alta.
        ///
        /// <para>
        /// En un rack de un solo sentido la entrada resuelta es siempre la mayor, asi que la salvaguarda no cambia
        /// nada: la altura sigue siendo exactamente la de siempre.
        /// </para>
        /// </summary>
        public static double Requirement(PushBackRun run, RackCatalog catalog)
        {
            var front = run?.Front();
            if (front == null || front.LoadBeamLevels == null || front.LoadBeamLevels.Count == 0)
            {
                return 0.0;
            }

            var ordered = front.LoadBeamLevels.OrderBy(level => level.LevelNumber).ToList();
            var top = ordered[ordered.Count - 1];
            var topCell = DynamicRackLevelGeometry.At(null, front, top.LevelNumber);
            var clearSpace = topCell.Pallet.Height + topCell.ClearHeight;

            var elevation = top.EntranceElevation;
            var drawn = PushBackElevations.HighInsertions(run.Source, catalog, front);
            if (drawn != null
                && drawn.TryGetValue(top.LevelNumber, out var drawnHigh)
                && drawnHigh > elevation)
            {
                elevation = drawnHigh;
            }

            return DynamicHeaderHeightCalculator.RoundUpToCommercialFoot(
                elevation + topCell.InOutBeamDepth + clearSpace * DynamicHeaderHeightCalculator.TopFinishFraction);
        }

        /// <summary>
        /// La demanda de cada FRENTE de la estructura compuesta: el maximo de las camas que lo usan. Un frente sin
        /// camas —una ranura en blanco por los dos lados— devuelve 0: no aporta demanda, y la linea contigua toma la
        /// del frente que si carga (<see cref="DynamicFrontGeometry.PostHeight"/> ya hace ese maximo por linea).
        ///
        /// <para>
        /// Una cama pertenece a la ranura <see cref="PushBackRun.Slot"/>, que en la estructura compuesta ES el indice
        /// del frente: la retícula transversal es una sola y el indice significa lo mismo en todas partes.
        /// </para>
        /// </summary>
        public static IReadOnlyList<double> ByFront(PushBackSystem system, RackCatalog catalog)
        {
            var fronts = system?.Structure?.Fronts;
            var result = new List<double>();
            if (fronts == null)
            {
                return result;
            }

            for (var index = 0; index < fronts.Count; index++)
            {
                result.Add(0.0);
            }

            foreach (var run in PushBackRuns.Resolve(system).Runs)
            {
                if (run.Slot < 0 || run.Slot >= result.Count)
                {
                    continue;
                }

                result[run.Slot] = Math.Max(result[run.Slot], Requirement(run, catalog));
            }

            return result;
        }

        /// <summary>
        /// I-42 (ronda 6D) — LA DEMANDA POR LADO Y POR LINEA. Una cabecera vive en una linea transversal Y en una
        /// posicion longitudinal, y esa segunda coordenada decide a que lado sirve: la primera mitad de la
        /// profundidad es de A y la segunda de B. Sus demandas son INDEPENDIENTES — subir los niveles de A no puede
        /// alargar un poste que pertenece solo a B.
        ///
        /// <para>
        /// Una cama contribuye a la linea de su izquierda y a la de su derecha, y al lado o LADOS a los que
        /// pertenece: una cama por lado aporta al suyo, una CORRIDA aporta a los dos porque fisicamente los
        /// atraviesa. Por eso la regla no pregunta por topologia.
        /// </para>
        /// </summary>
        public static IReadOnlyList<DynamicHeaderHeightZone> Zones(PushBackSystem system, RackCatalog catalog)
        {
            var result = new List<DynamicHeaderHeightZone>();
            var structure = system?.Structure;
            if (structure?.Fronts == null || !system.IsComposite)
            {
                return result;
            }

            var runs = PushBackRuns.Resolve(system);
            var lines = structure.Fronts.Count + 1;
            var bySide = new Dictionary<PushBackSide, double[]>
            {
                [PushBackSide.A] = new double[lines],
                [PushBackSide.B] = new double[lines]
            };

            foreach (var run in runs.Runs)
            {
                var requirement = Requirement(run, catalog);
                foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
                {
                    if (run.LowSide != side && run.HighSide != side)
                    {
                        continue;
                    }

                    foreach (var line in new[] { run.Slot, run.Slot + 1 })
                    {
                        if (line >= 0 && line < lines)
                        {
                            bySide[side][line] = Math.Max(bySide[side][line], requirement);
                        }
                    }
                }
            }

            // El TRAMO de cada lado ya lo publica el compuesto: su extremo EXTERIOR (su pasillo) y su extremo
            // INTERIOR (la linea que mira al hueco). Entre los dos queda el hueco, que no es de nadie. No se parte
            // por la mitad: con profundidades distintas la mitad no cae donde acaba A.
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var view = system.Composite?.Of(side);
                if (view == null || !view.IsPresent)
                {
                    continue;
                }

                var zone = NewZone(
                    Math.Min(view.OuterX, view.InnerX),
                    Math.Max(view.OuterX, view.InnerX),
                    bySide[side]);
                if (zone.EndX > zone.StartX)
                {
                    result.Add(zone);
                }
            }

            return result;
        }

        private static DynamicHeaderHeightZone NewZone(double startX, double endX, IReadOnlyList<double> heights)
        {
            var zone = new DynamicHeaderHeightZone { StartX = startX, EndX = endX };
            foreach (var height in heights)
            {
                zone.HeightByLine.Add(height);
            }

            return zone;
        }

        /// <summary>
        /// Escribe la demanda fisica en los frentes de la estructura compuesta, que es de donde la leen —por LINEA—
        /// el corte lateral, los dos cortes frontales y el BOM desde la ronda 6B. Una sola autoridad, un solo sitio.
        ///
        /// <para>
        /// Un OVERRIDE manual de altura (I-40) manda y no se toca: la propuesta derivada es lo que se corrige aqui,
        /// y el efectivo sigue siendo <c>override ?? propuesta</c>. Restore borra el override y vuelve a ESTA
        /// propuesta, ya recalculada sobre las camas actuales.
        /// </para>
        /// </summary>
        public static void Apply(PushBackSystem system, RackCatalog catalog, double? manualOverride)
        {
            if (system?.Structure?.Fronts == null || manualOverride.HasValue)
            {
                return;
            }

            var required = ByFront(system, catalog);
            for (var index = 0; index < system.Structure.Fronts.Count && index < required.Count; index++)
            {
                var front = system.Structure.Fronts[index];
                if (front != null)
                {
                    front.Height = required[index];
                }
            }

            // I-42 (ronda 6D): y la demanda POR LADO, en zonas de profundidad. `front.Height` sigue siendo la
            // envolvente de la linea —la respuesta cuando no se sabe en que posicion se pregunta—; la zona es la
            // respuesta precisa, y es la que consumen la cabecera, sus separadores, sus postes derivados y el BOM.
            system.Structure.HeaderHeightZones.Clear();
            foreach (var zone in Zones(system, catalog))
            {
                system.Structure.HeaderHeightZones.Add(zone);
            }
        }
    }
}
