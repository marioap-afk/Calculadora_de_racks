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
        }
    }
}
