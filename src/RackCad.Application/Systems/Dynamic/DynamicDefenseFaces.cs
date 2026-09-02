using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// I-42 (ronda 7D) — LAS DOS CARAS DE ATAQUE de una linea transversal, y si cada una existe.
    ///
    /// <para>
    /// La regla es la que la ronda 6D fijo dentro del constructor de planta y no cambia aqui: una defensa protege una
    /// CARA DE CARGA, o sea el extremo de la profundidad por donde entra el montacargas, y los extremos de la
    /// cobertura de una linea SON caras mientras no caigan en la interfaz con el otro lado. Lo unico nuevo es que
    /// deja de estar escrita dentro del bucle que dibuja: ahora la UI puede hacer la MISMA pregunta que hace el
    /// dibujo, y por construccion no pueden discrepar. Antes la rejilla por poste pintaba el extremo lejano con la
    /// regla de un rack de un solo sentido y mostraba «apagado» donde el rack si llevaba defensa.
    /// </para>
    /// <para>
    /// Es NEUTRAL, como <see cref="DynamicRackSystem.IsInteriorFace"/>: un rack dinamico no declara ningun tramo
    /// interior y las dos caras existen siempre que exista la linea, que es exactamente lo que hacia antes.
    /// </para>
    /// </summary>
    public static class DynamicDefenseFaces
    {
        /// <summary>
        /// I-42 (ronda 7E) — la PIEZA que materializa una cara, o NULL cuando esa cara no lleva ninguna. Es la unica
        /// lectura que los constructores necesitan hacer, y la hacen los tres —lateral, frontal y planta, que es la
        /// que alimenta el BOM—, asi que no pueden discrepar sobre que pieza lleva cada pasillo.
        /// </summary>
        public static string ElementIdFor(SelectiveSafetySelection selection, bool farEnd)
            => selection?.ElementIdForFace(farEnd);

        /// <summary>
        /// La X de mundo donde empieza la cobertura de ALMACENAMIENTO de la linea: su cara CERCANA.
        ///
        /// <para>
        /// I-42 (A1B-D5): se mide sobre <see cref="DynamicDepthGeometry.StorageCoverageAtPost"/>, no sobre la
        /// cobertura estructural. Una cara de ataque protege ALMACENAMIENTO, y un frente en blanco conserva su
        /// estructura sin almacenar nada.
        /// </para>
        /// </summary>
        public static double NearX(DynamicRackSystem system, int postIndex)
        {
            var range = DynamicDepthGeometry.StorageAtPost(system, postIndex);
            return system.Modules.FirstOrDefault(module => module.Index + 1 == range.StartPosition)?.StartX ?? 0.0;
        }

        /// <summary>La X de mundo donde termina la cobertura de ALMACENAMIENTO de la linea: su cara LEJANA.</summary>
        public static double FarX(DynamicRackSystem system, int postIndex)
        {
            var range = DynamicDepthGeometry.StorageAtPost(system, postIndex);
            return system.Modules.FirstOrDefault(module => module.Index + 1 == range.EndPosition)?.EndX
                   ?? system.TotalLength;
        }

        /// <summary>
        /// Si la cara indicada de esa linea es de verdad una cara de ataque: la linea existe, ALMACENA algo por su
        /// lado, y ese extremo mira a un pasillo y no al interior del rack.
        ///
        /// <para>
        /// I-42 (A1B-D5, contrato del dueño): la continuidad ESTRUCTURAL no es cobertura de almacenamiento. Una
        /// linea puede existir —y su poste, y su placa— porque una ranura en blanco conserva su claro, y aun asi no
        /// tener nada que proteger. Antes esa estructura se leia como si almacenara, y de ahi salian una bota
        /// automatica y una defensa automatica del lado equivocado.
        /// </para>
        /// </summary>
        public static bool HasFace(DynamicRackSystem system, int postIndex, bool farEnd)
        {
            if (system == null || !DynamicFrontActivation.BoundaryExists(system, postIndex))
            {
                return false;
            }

            // Hay poste, pero no hay almacenamiento en ninguno de sus claros: no hay cara que proteger.
            if (DynamicDepthGeometry.StorageCoverageAtPost(system, postIndex).IsEmpty)
            {
                return false;
            }

            return !system.IsInteriorFace(farEnd ? FarX(system, postIndex) : NearX(system, postIndex));
        }
    }
}
