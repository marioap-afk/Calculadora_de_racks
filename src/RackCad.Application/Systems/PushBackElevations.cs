using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
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
    /// La regla (Owner, tras el rechazo del round 1):
    /// <list type="number">
    /// <item>el larguero POSTERIOR es el ANCLA: conserva la elevación que ya le ajustó el resolver compartido;</item>
    /// <item>su CONTACTO es la arista que <see cref="PushBackLoadBeamGeometry.RearBeamTangencyPointWorld"/> elige por
    /// geometría —la de mayor X en mundo—, nunca un lado fijo del catálogo: con el bloque espejado la arista buena es
    /// la otra;</item>
    /// <item>la subida NOMINAL se mide sobre la LONGITUD COMERCIAL de la cama
    /// (<see cref="PushBackFlowBedGeometry.ResolveBedLength"/>), que es la pieza que se compra y se dibuja, no sobre
    /// la distancia entre contactos;</item>
    /// <item>el punto teórico bajo es el contacto posterior menos esa subida; su elevación de inserción resta el
    /// <c>TROQUEL_CAMA</c> local del larguero bajo;</item>
    /// <item>esa inserción se ajusta al troquel válido más cercano, con la MISMA retícula del resolver
    /// (<see cref="PushBackTroquelGrid"/>).</item>
    /// </list>
    /// La subida final es la RESULTANTE del ajuste, no el objetivo nominal.
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
                var chosen = ChooseLowTroquel(
                    rearContact.Value, lowContactX, lowMate.Value.Y, railLocalMate, gridBase);
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
        /// <item>a igualdad, el más cercano al resultado de la regla anterior (ajustar la subida nominal), para que
        /// la decisión sea estable y no salte entre dos troqueles equivalentes;</item>
        /// <item>a igualdad, el más bajo, que hace la elección determinista.</item>
        /// </list>
        /// </summary>
        private static (double Insertion, double Rotation)? ChooseLowTroquel(
            Point2D rearContact, double lowContactX, double lowMateLocalY, Point2D railLocalMate, double gridBase)
        {
            var pitch = SelectiveRackDefaults.TroquelPaso;
            if (pitch <= 0.0)
            {
                return null;
            }

            // Referencias de desempate: la posición teórica continua y la que daba la regla anterior.
            var theoretical = PushBackBedRotation.TheoreticalExitY(
                rearContact.X, rearContact.Y, lowContactX, railLocalMate.Y) - lowMateLocalY;
            var legacy = PushBackTroquelGrid.Snap(theoretical, gridBase);

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
                var toLegacy = Math.Abs(insertion - legacy);

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
                        front.LoadBeamLevels.Count,
                        front.EndX - front.StartX,
                        LowInsertions(system, catalog, front))),
                systemEnvelope: LowInsertions(system, catalog, null));
        }
    }
}
