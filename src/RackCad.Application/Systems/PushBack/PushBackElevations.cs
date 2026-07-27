using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>Las dos elevaciones resueltas de una celda Push Back, con sus contactos físicos.</summary>
    public readonly struct PushBackCellElevation
    {
        public PushBackCellElevation(
            int levelNumber, double lowInsertion, double rearInsertion,
            Point2D lowContact, Point2D rearContact, double rotationRadians)
        {
            LevelNumber = levelNumber;
            LowInsertion = lowInsertion;
            RearInsertion = rearInsertion;
            LowContact = lowContact;
            RearContact = rearContact;
            RotationRadians = rotationRadians;
        }

        public int LevelNumber { get; }

        /// <summary>Elevación de inserción del larguero de ENTRADA/SALIDA: derivada del posterior y ajustada a troquel.</summary>
        public double LowInsertion { get; }

        /// <summary>Elevación de inserción del larguero POSTERIOR: el ancla, tal como la ajustó el resolver.</summary>
        public double RearInsertion { get; }

        /// <summary>Contacto físico del larguero bajo con la cama (su <c>TROQUEL_CAMA</c> transformado).</summary>
        public Point2D LowContact { get; }

        /// <summary>Contacto físico del larguero posterior: la arista que la geometría elige, no un lado fijo.</summary>
        public Point2D RearContact { get; }

        /// <summary>
        /// La ROTACIÓN del bloque completo de la cama, resuelta por <see cref="PushBackBedRotation"/> para que el
        /// contacto posterior caiga sobre la línea del ORIGEN mientras el <c>TROQUEL_IN</c> se mantiene sobre el
        /// contacto bajo. Es el dato explícito que la autoridad entrega; nadie debe volver a derivarla de los dos
        /// contactos, porque no pertenecen a la misma recta.
        /// </summary>
        public double RotationRadians { get; }

        /// <summary>La PENDIENTE resultante de esa rotación: la que se compara con el objetivo de 7/192.</summary>
        public double ResultingSlope => Math.Tan(RotationRadians);

        /// <summary>
        /// Diferencia de elevación entre los dos CONTACTOS. Es una magnitud real, pero <b>no</b> es la subida de la
        /// cama: los dos contactos viven en rectas paralelas distintas, separadas por el mate local. Para la
        /// pendiente, <see cref="ResultingSlope"/>.
        /// </summary>
        public double MateElevationDelta => RearContact.Y - LowContact.Y;
    }

    /// <summary>
    /// PB-004 (I-32) — LA autoridad de elevaciones de Push Back, resuelta por FRENTE y NIVEL. Ninguna fórmula se
    /// copia en un builder: todo lo que dependa de dónde está un larguero de extremo pregunta aquí.
    ///
    /// Quiénes la consumen y por qué vía:
    /// <list type="bullet">
    /// <item>la CAMA y los LARGUEROS bajos, directamente, por <see cref="Resolve"/> / <see cref="LowInsertions"/>;</item>
    /// <item>el corte LATERAL, los dos cortes FRONTALES, el DESVIADOR bajo y las COTAS y ETIQUETAS, a través del
    /// contexto neutral que devuelve <see cref="Context"/> y que los builders compartidos reciben como último
    /// parámetro opcional.</item>
    /// </list>
    ///
    /// El extremo POSTERIOR no pasa por aquí en ninguna vista: su larguero es el ancla y conserva la elevación del
    /// resolver, así que lo que cuelgue de él la sigue leyendo de ahí.
    ///
    /// La regla vigente (aclaración final del Owner):
    /// <list type="number">
    /// <item>el larguero POSTERIOR es el ANCLA y <b>no se mueve</b>: conserva el troquel que ya le ajustó el
    /// resolver compartido;</item>
    /// <item>su CONTACTO es la arista que <see cref="PushBackLoadBeamGeometry.RearBeamTangencyPointWorld"/> elige por
    /// geometría —la de mayor X en mundo—, nunca un lado fijo del catálogo: con el bloque espejado la arista buena es
    /// la otra;</item>
    /// <item>la cama es ASIMÉTRICA: abajo hace <b>mate por <c>TROQUEL_IN</c></b> sobre el <c>TROQUEL_CAMA</c> del
    /// larguero In/Out, mientras que los intermedios y el posterior son tangentes a la <b>línea del ORIGEN</b> del
    /// bloque. Las dos rectas son paralelas y están separadas por la componente perpendicular del mate;</item>
    /// <item>la ROTACIÓN que concilia las dos la resuelve <see cref="PushBackBedRotation"/>, y es una sola para todo
    /// el bloque;</item>
    /// <item>el troquel del larguero BAJO se elige <b>minimizando el error de pendiente contra 7/192</b> sobre TODO
    /// el rango válido de la retícula de 2", de modo que el mínimo es global;</item>
    /// <item>los DOS largueros de extremo quedan sobre troqueles válidos de esa misma retícula
    /// (<see cref="PushBackTroquelGrid"/>).</item>
    /// </list>
    /// La pendiente final es la RESULTANTE de esa selección, no el objetivo nominal: 7/192 es el objetivo al que se
    /// acerca lo más posible, no un valor que se imponga sacando un larguero de su troquel.
    ///
    /// No toca <see cref="DynamicLoadBeamLevel"/> ni el sistema Dinámico: los builders compartidos reciben estas
    /// elevaciones por un parámetro OPCIONAL cuyo valor por defecto (null) deja su comportamiento intacto, y el
    /// Dinámico no lo pasa nunca.
    /// </summary>
    public static class PushBackElevations
    {
        /// <summary>Las elevaciones resueltas de cada nivel de un frente, indexadas por número de nivel.</summary>
        public static IReadOnlyDictionary<int, PushBackCellElevation> Resolve(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
        {
            var result = new Dictionary<int, PushBackCellElevation>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var rearBeamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

            // El mate local del riel: su componente perpendicular es lo que separa las dos rectas paralelas.
            var railLocalMate = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            var gridBase = PushBackTroquelGrid.Base(structure, catalog);

            // La subida NOMINAL sobre la longitud comercial: ya no elige el troquel, pero sigue siendo la
            // referencia del tercer desempate. Se calcula con la fórmula ANTERIOR, no reconstruyéndola desde la
            // posición teórica asimétrica — son cantidades distintas y confundirlas volvería a hacer inútil el
            // desempate.
            var nominalRise = PushBackBedSlope.Rise(PushBackFlowBedGeometry.ResolveBedLength(system, front));
            var placements = DynamicLoadBeamGeometry.Placements(structure, front);

            foreach (var level in placements.Select(p => p.LevelNumber).Distinct())
            {
                var low = placements.FirstOrDefault(p => p.LevelNumber == level && !p.IsEntrance);
                var rear = placements.FirstOrDefault(p => p.LevelNumber == level && p.IsEntrance);
                if (low == null || rear == null)
                {
                    continue;
                }

                // El contacto posterior lo elige la GEOMETRÍA entre las dos aristas medidas del bloque.
                var rearContact = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                    catalog, rearBeamId, rear.X, rear.Y, rear.MirroredX);
                if (!rearContact.HasValue)
                {
                    continue;   // sin arista medida no hay contacto: no se inventa uno
                }

                var lowBeamId = string.IsNullOrWhiteSpace(low.BeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : low.BeamCatalogId;
                var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, lowBeamId);
                if (!lowMate.HasValue)
                {
                    continue;
                }

                var lowContactX = PushBackLoadBeamGeometry
                    .BedTangencyPointWorld(lowMate.Value, low.X, 0.0, low.MirroredX).X;
                // El resultado EXACTO de la regla anterior para esta celda: tercer desempate.
                var legacyInsertion = PushBackTroquelGrid.Snap(
                    rearContact.Value.Y - nominalRise - lowMate.Value.Y, gridBase);
                var chosen = ChooseLowTroquel(
                    rearContact.Value, lowContactX, lowMate.Value.Y, railLocalMate, gridBase, legacyInsertion);
                if (!chosen.HasValue)
                {
                    continue;
                }

                var lowInsertion = chosen.Value.Insertion;
                var lowContact = PushBackLoadBeamGeometry.BedTangencyPointWorld(
                    lowMate.Value, low.X, lowInsertion, low.MirroredX);

                result[level] = new PushBackCellElevation(
                    level, lowInsertion, rear.Y, lowContact, rearContact.Value, chosen.Value.Rotation);
            }

            return result;
        }

        /// <summary>
        /// Elige el troquel del larguero de ENTRADA/SALIDA (aclaración final del Owner, I-32).
        ///
        /// El larguero POSTERIOR es el ancla y no se mueve: conserva su troquel resuelto. Lo que se elige es la
        /// posición del BAJO, y el criterio ya no es «ajustar una subida nominal» sino <b>minimizar el error de
        /// PENDIENTE contra el objetivo de 7/192</b>, sobre las posiciones válidas de la retícula de 2".
        ///
        /// Se recorre TODO el rango físicamente válido —desde el primer troquel de la retícula hasta que la subida
        /// se anula—, no una ventana alrededor de una estimación: así el mínimo es GLOBAL y no depende de dónde se
        /// empiece a buscar. El error es monótono a cada lado del cruce con el objetivo, así que el barrido completo
        /// encuentra el óptimo y no un mínimo local.
        ///
        /// <b>Desempate</b>, en este orden exacto:
        /// <list type="number">
        /// <item>menor error de pendiente;</item>
        /// <item>a igualdad, el candidato más cercano a la posición teórica CONTINUA —la que daría exactamente
        /// 7/192 si la retícula no existiera—;</item>
        /// <item>a igualdad, el más cercano al resultado <b>real</b> de la regla anterior —ajustar la subida
        /// nominal medida sobre la longitud comercial—, que llega calculado en <paramref name="legacyInsertion"/>,
        /// para que la decisión sea estable y no salte entre dos troqueles equivalentes;</item>
        /// <item>a igualdad, el más bajo, que hace la elección determinista.</item>
        /// </list>
        /// </summary>
        /// <param name="legacyInsertion">
        /// El resultado real de la regla anterior para esta celda, calculado por el llamador — que es quien tiene el
        /// sistema y el frente— como <c>Snap(rearContact.Y − Rise(ResolveBedLength) − lowMateLocalY)</c>. No se
        /// reconstruye aquí desde la posición teórica asimétrica: esa es OTRA cantidad, y usarla dejaría el tercer
        /// desempate sin contenido propio.
        /// </param>
        private static (double Insertion, double Rotation)? ChooseLowTroquel(
            Point2D rearContact, double lowContactX, double lowMateLocalY, Point2D railLocalMate, double gridBase,
            double legacyInsertion)
        {
            var pitch = SelectiveRackDefaults.TroquelPaso;
            if (pitch <= 0.0)
            {
                return null;
            }

            // Segundo desempate: la posición teórica CONTINUA de la geometría asimétrica.
            var theoretical = PushBackBedRotation.TheoreticalExitY(
                rearContact.X, rearContact.Y, lowContactX, railLocalMate.Y) - lowMateLocalY;

            (double Insertion, double Rotation)? best = null;
            var bestError = double.MaxValue;
            var bestToTheoretical = double.MaxValue;
            var bestToLegacy = double.MaxValue;

            for (var insertion = PushBackTroquelGrid.Snap(gridBase, gridBase); ; insertion += pitch)
            {
                var exitMate = new Point2D(lowContactX, insertion + lowMateLocalY);
                if (exitMate.Y >= rearContact.Y)
                {
                    break;   // sin subida no hay cama
                }

                var rotation = PushBackBedRotation.Solve(exitMate, rearContact, railLocalMate);
                if (!rotation.HasValue)
                {
                    continue;
                }

                var error = PushBackBedRotation.SlopeError(rotation.Value);
                var toTheoretical = Math.Abs(insertion - theoretical);
                var toLegacy = Math.Abs(insertion - legacyInsertion);

                var better = error < bestError - 1e-12
                    || (Math.Abs(error - bestError) <= 1e-12
                        && (toTheoretical < bestToTheoretical - 1e-12
                            || (Math.Abs(toTheoretical - bestToTheoretical) <= 1e-12
                                && toLegacy < bestToLegacy - 1e-12)));

                if (best == null || better)
                {
                    best = (insertion, rotation.Value);
                    bestError = error;
                    bestToTheoretical = toTheoretical;
                    bestToLegacy = toLegacy;
                }
            }

            return best;
        }

        /// <summary>
        /// Solo las elevaciones de inserción del larguero BAJO, en la forma que consumen los builders compartidos por
        /// su parámetro opcional. Null nunca: un diccionario vacío deja el comportamiento por defecto.
        /// </summary>
        public static IReadOnlyDictionary<int, double> LowInsertions(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => Resolve(system, catalog, front)
                .ToDictionary(entry => entry.Key, entry => entry.Value.LowInsertion);

        /// <summary>
        /// El CONTEXTO que consumen las vistas compartidas: las elevaciones bajas de cada frente, envueltas en un
        /// tipo neutral que no sabe nada de Push Back. Se construye una sola vez por dibujo y se pasa como último
        /// parámetro opcional; los builders no vuelven a preguntar nada.
        ///
        /// Cada frente aporta además los dos datos que necesita la regla de proyección —su número de niveles y su
        /// profundidad—, para que <see cref="RackLevelElevations.AtPost"/> y
        /// <see cref="RackLevelElevations.AtProjectedSystem"/> elijan frente EXACTAMENTE como lo hace el resolver.
        /// Aquí no se decide nada: solo se entregan los datos con los que esa regla se aplica.
        ///
        /// Y aparte de los frentes se entrega el mapa de la ENVOLVENTE, resuelto con <c>front: null</c>: el rack
        /// entero, con la longitud de cama del sistema completo. No es el de ningún frente y no se puede deducir de
        /// ellos — con un frente que gana por niveles y otro más profundo, proyección y envolvente caen en troqueles
        /// distintos. Es lo que dibuja el lateral no seccionado.
        ///
        /// Devuelve <c>null</c> cuando no hay ninguna elevación que aportar; los builders lo tratan como «sin
        /// override» y se quedan con la elevación del resolver.
        /// </summary>
        public static RackLevelElevations Context(PushBackSystem system, RackCatalog catalog)
        {
            var fronts = system?.Structure?.Fronts;
            if (fronts == null)
            {
                return null;
            }

            return RackLevelElevations.From(
                fronts
                    .Where(front => front != null)
                    .Select(front => new RackFrontLevelElevations(
                        front.Index,
                        // A blank front keeps its elevations DORMANT on the front, so the authority reports the
                        // EFFECTIVE count — zero — instead of claiming levels that nothing places (I-33).
                        DynamicFrontActivation.EffectiveLoadLevels(front),
                        front.EndX - front.StartX,
                        LowInsertions(system, catalog, front))),
                systemEnvelope: LowInsertions(system, catalog, null));
        }
    }
}
