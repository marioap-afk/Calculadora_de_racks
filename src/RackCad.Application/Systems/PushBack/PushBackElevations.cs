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

        /// <summary>
        /// Elevación de inserción del larguero de ENTRADA/SALIDA: el ANCLA, tal como la resolvió el nivel desde el
        /// datum del producto. No se corrige.
        /// </summary>
        public double LowInsertion { get; }

        /// <summary>
        /// Elevación de inserción del larguero POSTERIOR: DERIVADA del ancla baja y resuelta sobre la retícula de
        /// troqueles.
        /// </summary>
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
    /// La regla vigente es la decisión FINAL del dueño, y <b>retira</b> la de I-32 («verticalmente gobierna el
    /// ALTO»). Hay UNA sola política, en un solo sentido:
    /// <list type="number">
    /// <item>el larguero de ENTRADA —el BAJO— es el ANCLA y <b>no se mueve</b>: conserva exactamente el troquel que
    /// su nivel le dio, medido desde el datum del producto. Es la altura que el usuario pidió, y ni la topología ni
    /// el fondo de la cama pueden hundirla;</item>
    /// <item>el larguero POSTERIOR se <b>DERIVA</b>: se enumeran los troqueles de la retícula y se elige el que
    /// mejor cumple, en este orden — (a) menor error de pendiente contra 7/192, (b) más cercano al ALTO teórico
    /// (<c>bajo + subida nominal sobre la longitud real de la cama</c>), (c) el de menor elevación;</item>
    /// <item>su CONTACTO es la arista que <see cref="PushBackLoadBeamGeometry.RearBeamTangencyPointWorld"/> elige por
    /// geometría —la de mayor X en mundo—, nunca un lado fijo del catálogo: con el bloque espejado la arista buena es
    /// la otra;</item>
    /// <item>la cama es ASIMÉTRICA: abajo hace <b>mate por <c>TROQUEL_IN</c></b> sobre el <c>TROQUEL_CAMA</c> del
    /// larguero In/Out, mientras que los intermedios y el posterior son tangentes a la <b>línea del ORIGEN</b> del
    /// bloque. Las dos rectas son paralelas y están separadas por la componente perpendicular del mate;</item>
    /// <item>la ROTACIÓN que concilia las dos la resuelve <see cref="PushBackBedRotation"/>, y es una sola para todo
    /// el bloque;</item>
    /// <item>los DOS largueros de extremo quedan sobre troqueles válidos de esa misma retícula
    /// (<see cref="PushBackTroquelGrid"/>).</item>
    /// </list>
    /// La pendiente final es la RESULTANTE de esa selección, no el objetivo nominal: 7/192 es el objetivo al que se
    /// acerca lo más posible, no un valor que se imponga sacando un larguero de su troquel.
    ///
    /// <para>
    /// No queda viva ninguna ruta que fije el ALTO o elija el BAJO. Quien quiera la elevación del larguero posterior
    /// pregunta por <see cref="HighInsertions"/>; leer <c>EntranceElevation</c> del resolver para dibujarlo sería
    /// una SEGUNDA autoridad vertical, que es justo lo que esta regla elimina.
    /// </para>
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

            // I-41 (PB-015): las colocaciones ya llegan con la X del larguero posterior resuelta POR CELDA, así que
            // todo lo que sigue —la subida nominal, el contacto posterior y la elección del troquel bajo— se calcula
            // sobre el fondo efectivo de cada nivel sin que ninguna fórmula cambie.
            var placements = PushBackPlacements.Resolve(system, front);

            foreach (var level in placements.Select(p => p.LevelNumber).Distinct())
            {
                var low = placements.FirstOrDefault(p => p.LevelNumber == level && !p.IsEntrance);
                var rear = placements.FirstOrDefault(p => p.LevelNumber == level && p.IsEntrance);
                if (low == null || rear == null)
                {
                    continue;
                }

                // La subida NOMINAL sobre la longitud comercial: ya no elige el troquel, pero sigue siendo la
                // referencia del tercer desempate. Se calcula con la fórmula ANTERIOR, no reconstruyéndola desde la
                // posición teórica asimétrica — son cantidades distintas y confundirlas volvería a hacer inútil el
                // desempate. I-41 la mide sobre la cama de ESTA celda: con fondos distintos por nivel, una sola
                // subida nominal por frente desempataría contra una longitud que ese nivel no tiene.
                var nominalRise = PushBackBedSlope.Rise(PushBackCellDepth.BedLength(system, front, level));

                var lowBeamId = string.IsNullOrWhiteSpace(low.BeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : low.BeamCatalogId;
                var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, lowBeamId);
                if (!lowMate.HasValue)
                {
                    continue;
                }

                // El larguero BAJO es el ANCLA: conserva EXACTAMENTE la elevacion que el nivel le dio (su troquel
                // resuelto, medido desde el datum del producto). No se mueve para mejorar la pendiente.
                var lowInsertion = low.Y;
                var lowContact = PushBackLoadBeamGeometry.BedTangencyPointWorld(
                    lowMate.Value, low.X, lowInsertion, low.MirroredX);
                var exitMate = new Point2D(lowContact.X, lowInsertion + lowMate.Value.Y);

                // El ALTO se DERIVA: la subida nominal sobre la longitud real da el objetivo teorico, y de ahi se
                // elige el troquel fisicamente valido con el criterio del dueño.
                var chosen = ChooseHighTroquel(
                    catalog, rearBeamId, exitMate, lowContact.Y + nominalRise,
                    rear.X, rear.MirroredX, railLocalMate, gridBase);
                if (!chosen.HasValue)
                {
                    continue;
                }

                result[level] = new PushBackCellElevation(
                    level, lowInsertion, chosen.Value.Insertion, lowContact, chosen.Value.Contact,
                    chosen.Value.Rotation);
            }

            return result;
        }

        /// <summary>
        /// Elige el troquel del larguero POSTERIOR (decision final del dueño: la autoridad vertical es el BAJO).
        ///
        /// <para>
        /// El larguero de entrada es el ancla y NO se mueve: conserva el troquel que su nivel le dio. Lo que se
        /// elige es el ALTO, enumerando los troqueles fisicamente validos por encima del bajo —sin ellos no hay
        /// subida y por tanto no hay cama— y quedandose con el que mejor cumple, en este orden:
        /// </para>
        /// <list type="number">
        /// <item>MENOR error respecto de la pendiente nominal;</item>
        /// <item>si empatan, el mas cercano al ALTO teorico (<c>bajo + subida nominal</c>);</item>
        /// <item>si siguen empatando, el de MENOR elevacion.</item>
        /// </list>
        /// <para>
        /// La regla anterior hacia lo contrario —fijaba el alto y elegia el bajo— y por eso una cama mas larga
        /// hundia su larguero de entrada por debajo de la altura que el usuario habia pedido. Su tercer desempate
        /// era la cercania al resultado PRE-I-32; pertenecia a aquella autoridad y desaparece con ella.
        /// </para>
        /// </summary>
        private static (double Insertion, double Rotation, Point2D Contact)? ChooseHighTroquel(
            RackCatalog catalog,
            string rearBeamId,
            Point2D exitMate,
            double theoreticalHighY,
            double rearX,
            bool rearMirroredX,
            Point2D railLocalMate,
            double gridBase)
        {
            var pitch = SelectiveRackDefaults.TroquelPaso;
            if (pitch <= 0.0)
            {
                return null;
            }

            (double Insertion, double Rotation, Point2D Contact)? best = null;
            var bestError = double.MaxValue;
            var bestToTheoretical = double.MaxValue;

            // El CONTACTO del larguero posterior no está a la altura de su inserción: la arista la elige la geometría
            // del bloque y queda desplazada una cantidad FIJA (el bloque solo se traslada verticalmente). Se mide esa
            // separación una vez —no se estima— para que el barrido arranque en el primer troquel que puede llegar a
            // tener subida, sea cual sea el perfil del catálogo. Empezar en el troquel más próximo al mate de salida
            // dejaría fuera candidatos válidos que están por debajo de él.
            var probe = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                catalog, rearBeamId, rearX, gridBase, rearMirroredX);
            if (!probe.HasValue)
            {
                return null;   // sin arista medida no hay contacto: no se inventa uno
            }

            var contactOffset = probe.Value.Y - gridBase;
            var first = PushBackTroquelGrid.Snap(exitMate.Y - contactOffset, gridBase) - pitch;
            var ceiling = theoreticalHighY - contactOffset
                + Math.Max(4.0 * pitch, Math.Abs(theoreticalHighY - exitMate.Y));

            for (var insertion = first; insertion <= ceiling + pitch; insertion += pitch)
            {
                var contact = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                    catalog, rearBeamId, rearX, insertion, rearMirroredX);
                if (!contact.HasValue)
                {
                    return null;   // sin arista medida no hay contacto: no se inventa uno
                }

                if (contact.Value.Y <= exitMate.Y)
                {
                    continue;   // sin subida no hay cama
                }

                var rotation = PushBackBedRotation.Solve(exitMate, contact.Value, railLocalMate);
                if (!rotation.HasValue)
                {
                    continue;
                }

                var error = PushBackBedRotation.SlopeError(rotation.Value);
                var toTheoretical = Math.Abs(contact.Value.Y - theoreticalHighY);

                // Tercer desempate —el ALTO de MENOR elevacion— sale gratis: se enumera de abajo arriba y solo se
                // reemplaza al mejorar ESTRICTAMENTE, asi que ante un empate perfecto gana el primero, que es el mas bajo.
                var better = error < bestError - 1e-12
                    || (Math.Abs(error - bestError) <= 1e-12 && toTheoretical < bestToTheoretical - 1e-12);

                if (best == null || better)
                {
                    best = (insertion, rotation.Value, contact.Value);
                    bestError = error;
                    bestToTheoretical = toTheoretical;
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
        /// Las inserciones del larguero POSTERIOR por nivel. Desde la decision final del dueño es el extremo que se
        /// DERIVA, asi que su colocacion tiene que leerse de aqui: si el dibujo siguiera usando la elevacion del
        /// resolver compartido, la cama y su larguero alto quedarian en troqueles distintos.
        /// </summary>
        public static IReadOnlyDictionary<int, double> HighInsertions(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => Resolve(system, catalog, front)
                .ToDictionary(entry => entry.Key, entry => entry.Value.RearInsertion);

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
            => BuildContext(system, catalog, low: true);

        /// <summary>
        /// El mismo contexto para el extremo ALTO. Existe porque desde la decisión final del dueño el larguero
        /// posterior ya NO es el ancla: se DERIVA, y por tanto su elevación tampoco se puede leer del resolver
        /// compartido. El corte frontal posterior lo consume igual que el bajo consume <see cref="Context"/>, de
        /// modo que la misma pieza física sale a la misma altura en el lateral y en el frontal.
        /// </summary>
        public static RackLevelElevations HighContext(PushBackSystem system, RackCatalog catalog)
            => BuildContext(system, catalog, low: false);

        private static RackLevelElevations BuildContext(PushBackSystem system, RackCatalog catalog, bool low)
        {
            var fronts = system?.Structure?.Fronts;
            if (fronts == null)
            {
                return null;
            }

            IReadOnlyDictionary<int, double> At(DynamicRackFront front)
                => low ? LowInsertions(system, catalog, front) : HighInsertions(system, catalog, front);

            return RackLevelElevations.From(
                fronts
                    .Where(front => front != null)
                    .Select(front => new RackFrontLevelElevations(
                        front.Index,
                        // A blank front keeps its elevations DORMANT on the front, so the authority reports the
                        // EFFECTIVE count — zero — instead of claiming levels that nothing places (I-33).
                        DynamicFrontActivation.EffectiveLoadLevels(front),
                        front.EndX - front.StartX,
                        At(front))),
                systemEnvelope: At(null));
        }
    }
}
