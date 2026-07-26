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
        public PushBackCellElevation(int levelNumber, double lowInsertion, double rearInsertion, Point2D lowContact, Point2D rearContact)
        {
            LevelNumber = levelNumber;
            LowInsertion = lowInsertion;
            RearInsertion = rearInsertion;
            LowContact = lowContact;
            RearContact = rearContact;
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

        /// <summary>Subida REAL de la cama de esta celda, la que sale del ajuste al troquel.</summary>
        public double ResultingRise => RearContact.Y - LowContact.Y;
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

            // La longitud COMERCIAL de la cama es la que fija la subida nominal.
            var bedLength = PushBackFlowBedGeometry.ResolveBedLength(system, front);
            if (bedLength <= 0.0)
            {
                return result;
            }

            var nominalRise = PushBackBedSlope.Rise(bedLength);
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

                var lowInsertion = PushBackTroquelGrid.Snap(rearContact.Value.Y - nominalRise - lowMate.Value.Y, gridBase);
                var lowContact = PushBackLoadBeamGeometry.BedTangencyPointWorld(
                    lowMate.Value, low.X, lowInsertion, low.MirroredX);

                result[level] = new PushBackCellElevation(level, lowInsertion, rear.Y, lowContact, rearContact.Value);
            }

            return result;
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
