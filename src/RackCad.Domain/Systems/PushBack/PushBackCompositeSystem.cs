using System.Collections.Generic;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Domain.Systems.PushBack
{
    /// <summary>
    /// I-42 — la mitad RESUELTA de un lado: su proyeccion sobre la retícula transversal compartida, su configuracion
    /// Push Back por ranura, su rejilla de topes y su estructura propuesta/efectiva.
    ///
    /// <para>
    /// Es una VISTA del rack, no un rack: sus frentes son proyecciones del lado sobre la MISMA estructura fisica que
    /// el otro lado usa. Nada de lo que hay aqui es propietario de un poste, una cabecera, una placa o un separador —
    /// esos pertenecen a la estructura compartida y se cuentan una sola vez.
    /// </para>
    /// </summary>
    public sealed class PushBackSideSystem
    {
        public PushBackSide Side { get; set; }

        /// <summary>False = el lado no existe: no aporta celda, cama, larguero, tope ni demanda de estructura.</summary>
        public bool IsPresent { get; set; }

        /// <summary>
        /// Proyeccion del lado por ranura transversal (alineada por indice con la retícula compartida). Una entrada
        /// NULA significa que la ranura no existe en este lado — el caso «A=3 y B=4».
        /// </summary>
        public IList<DynamicRackFront> Fronts { get; } = new List<DynamicRackFront>();

        /// <summary>Valores Push Back resueltos por ranura (peralte posterior, fondo efectivo y tarima por nivel).</summary>
        public IList<PushBackResolvedFront> ResolvedFronts { get; } = new List<PushBackResolvedFront>();

        /// <summary>Rejilla de topes del lado (activa por defecto).</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

        /// <summary>I-42 (ronda 7E) — el tipo de defensa resuelto de este lado.</summary>
        public string DefensePieceId { get; set; }

        /// <summary>
        /// Estructura PROPUESTA: las posiciones de fondo que la demanda de las celdas de este lado exige como minimo.
        /// Es derivada — se recalcula siempre — y nunca es autoridad inmutable.
        /// </summary>
        public int ProposedStructure { get; set; }

        /// <summary>El override manual almacenado, o null si el lado sigue la propuesta.</summary>
        public int? StructureOverride { get; set; }

        /// <summary>Estructura EFECTIVA del lado: <c>Override ?? Propuesta</c>. Es la que se construye.</summary>
        public int EffectiveStructure { get; set; }

        /// <summary>Primera posicion de fondo (1-based) que ocupa este lado en la secuencia compartida.</summary>
        public int FirstPosition { get; set; }

        /// <summary>Ultima posicion de fondo (1-based) que ocupa este lado en la secuencia compartida.</summary>
        public int LastPosition { get; set; }

        /// <summary>X del extremo EXTERIOR del lado (su pasillo): el extremo BAJO de sus camas propias.</summary>
        public double OuterX { get; set; }

        /// <summary>X del extremo INTERIOR del lado: su linea de postes terminal, la que mira al gap.</summary>
        public double InnerX { get; set; }

        /// <summary>
        /// El sistema Push Back del lado EN SU MARCO LOCAL: su sub-estructura arranca en 0 por su extremo exterior y
        /// su flujo avanza hacia +X. Es lo que permite que TODA la fisica ya validada de Push Back —elevaciones,
        /// rotacion de la cama, tangencias, tope posterior— se reutilice tal cual para los dos lados: el lado B no
        /// tiene reglas propias, tiene un marco propio. El paso a mundo es una REFLEXION rigida del conjunto (la
        /// cama y todo lo que va montado sobre ella comparten esa misma transformacion), no un espejo de decoracion.
        /// </summary>
        public PushBackSystem Local { get; set; }

        /// <summary>
        /// Indice del frente dentro de <see cref="Local"/> para cada ranura transversal, o -1 si la ranura no existe
        /// en este lado. Es el puente entre la numeracion COMPARTIDA de ranuras y la local del lado.
        /// </summary>
        public IList<int> LocalIndexBySlot { get; } = new List<int>();

        /// <summary>El frente LOCAL de una ranura, o null si la ranura no existe en este lado.</summary>
        public DynamicRackFront LocalFront(int frontIndex)
        {
            if (frontIndex < 0 || frontIndex >= LocalIndexBySlot.Count || Local?.Structure == null)
            {
                return null;
            }

            var index = LocalIndexBySlot[frontIndex];
            return index >= 0 && index < Local.Structure.Fronts.Count ? Local.Structure.Fronts[index] : null;
        }

        /// <summary>La proyeccion de la ranura <paramref name="frontIndex"/> en este lado, o null si no existe.</summary>
        public DynamicRackFront Front(int frontIndex)
            => frontIndex >= 0 && frontIndex < Fronts.Count ? Fronts[frontIndex] : null;

        /// <summary>Los valores Push Back resueltos de la ranura, o null si la ranura no existe en este lado.</summary>
        public PushBackResolvedFront Resolved(int frontIndex)
            => frontIndex >= 0 && frontIndex < ResolvedFronts.Count ? ResolvedFronts[frontIndex] : null;
    }

    /// <summary>
    /// I-42 — UNA CAMA FISICA de una celda, con las dos magnitudes que deciden si es construible.
    ///
    /// <para>
    /// Existe porque una celda puede contener MAS DE UNA cama: «encontradas» son dos camas independientes, cada una
    /// con su propia demanda y su propia estructura disponible. Medirlas juntas —comparando la demanda de una contra
    /// el espacio de la otra— produce errores de capacidad que no existen, y es exactamente lo que hacia falta
    /// corregir: con A de 4 fondos y B de 8, la cama de B pedia su longitud y se comparaba contra la estructura de A.
    /// </para>
    /// </summary>
    public sealed class PushBackCellBed
    {
        /// <summary>Lado del extremo BAJO (por donde se carga).</summary>
        public PushBackSide LowSide { get; set; }

        /// <summary>Lado del extremo ALTO (donde va el tope). Coincide con el bajo salvo en una corrida.</summary>
        public PushBackSide HighSide { get; set; }

        /// <summary>Fondos que la cama DECLARA. Es su demanda, y no se deriva de ninguna estructura.</summary>
        public int DemandPositions { get; set; }

        /// <summary>
        /// Longitud MINIMA que exige la demanda de esta cama con la regla fisica vigente. No depende del hueco, ni
        /// de la longitud total del rack, ni de la estructura sobrante, ni del ajuste manual vigente.
        /// </summary>
        public double RequiredBedLength { get; set; }

        /// <summary>
        /// Longitud FISICA realmente utilizada por la cama: el primer apoyo valido, desde su ancla ALTA hacia el
        /// bajo, cuya distancia satisface la demanda. Es la que se dibuja y la que se cotiza.
        /// <para>
        /// Se cumple <c>RequiredBedLength &lt;= ResolvedBedLength &lt;= AvailableBedSpan</c>: ni una longitud
        /// flotante entre dos apoyos, ni todo el espacio disponible cuando con menos basta.
        /// </para>
        /// </summary>
        public double ResolvedBedLength { get; set; }

        /// <summary>
        /// Longitud fisicamente DISPONIBLE entre los apoyos de la estructura efectiva para esta cama. Es CAPACIDAD
        /// MAXIMA, no longitud obligatoria.
        /// </summary>
        public double AvailableBedSpan { get; set; }

        /// <summary>Razon por la que la cama no es construible, o null si lo es.</summary>
        public string DisabledReason { get; set; }

        public bool IsValid => string.IsNullOrEmpty(DisabledReason);
    }

    /// <summary>
    /// I-42 — una CELDA compuesta resuelta: la ranura transversal x nivel con su topologia, su sentido y las camas
    /// fisicas que contiene.
    /// </summary>
    public sealed class PushBackResolvedCell
    {
        public int FrontIndex { get; set; }

        /// <summary>Numero de nivel, 1-based (como en las colocaciones y en los ejes de cama).</summary>
        public int LevelNumber { get; set; }

        public PushBackCellTopology Topology { get; set; }

        /// <summary>Sentido de la corrida. En cualquier otra topologia no significa nada y no se consulta.</summary>
        public PushBackRunDirection Direction { get; set; }

        /// <summary>
        /// Las camas FISICAS de la celda: una en solo A / solo B / corrida, DOS en encontradas. Cada una lleva su
        /// propia capacidad, porque cada una es una pieza distinta.
        /// </summary>
        public IList<PushBackCellBed> Beds { get; } = new List<PushBackCellBed>();

        /// <summary>La cama cuyo extremo BAJO esta en el lado indicado, o null.</summary>
        public PushBackCellBed BedFrom(PushBackSide side)
        {
            foreach (var bed in Beds)
            {
                if (bed != null && bed.LowSide == side)
                {
                    return bed;
                }
            }

            return null;
        }

        /// <summary>La mayor demanda de las camas de la celda (lectura para la UI).</summary>
        public double RequiredBedLength
        {
            get
            {
                var value = 0.0;
                foreach (var bed in Beds)
                {
                    if (bed != null && bed.RequiredBedLength > value)
                    {
                        value = bed.RequiredBedLength;
                    }
                }

                return value;
            }
        }

        /// <summary>La capacidad de la cama mas exigida (lectura para la UI).</summary>
        public double AvailableBedSpan
        {
            get
            {
                PushBackCellBed worst = null;
                foreach (var bed in Beds)
                {
                    if (bed != null && (worst == null || bed.RequiredBedLength > worst.RequiredBedLength))
                    {
                        worst = bed;
                    }
                }

                return worst?.AvailableBedSpan ?? 0.0;
            }
        }

        /// <summary>Razon por la que la celda no es construible, o null. Nunca se corrige en silencio.</summary>
        public string DisabledReason { get; set; }

        public bool IsValid => string.IsNullOrEmpty(DisabledReason);
    }

    /// <summary>
    /// I-42 — la parte COMPUESTA de un Push Back resuelto: los dos lados, la interfaz central y la rejilla de celdas
    /// con su topologia. NULL en <see cref="PushBackSystem.Composite"/> significa «rack de un solo sentido», que es
    /// el legacy.
    /// </summary>
    public sealed class PushBackCompositeSystem
    {
        /// <summary>Separacion fisica resuelta (in) entre la linea terminal de A y la inicial de B.</summary>
        public double Gap { get; set; }

        /// <summary>Si el hueco lleva el separador central. Pertenece a la interfaz y se cuenta UNA sola vez.</summary>
        public bool CentralSeparator { get; set; }

        /// <summary>X donde acaba la estructura de A (su linea de postes terminal).</summary>
        public double GapStartX { get; set; }

        /// <summary>X donde empieza la estructura de B (su linea de postes inicial). Igual a <see cref="GapStartX"/> si el gap es 0.</summary>
        public double GapEndX { get; set; }

        /// <summary>Posicion de fondo (1-based) que ocupa el hueco en la secuencia de modulos, o 0 si no hay hueco.</summary>
        public int GapPosition { get; set; }

        public PushBackSideSystem SideA { get; set; } = new PushBackSideSystem { Side = PushBackSide.A };
        public PushBackSideSystem SideB { get; set; } = new PushBackSideSystem { Side = PushBackSide.B };

        /// <summary>Las celdas resueltas del rack compuesto, con su topologia y su viabilidad geometrica.</summary>
        public IList<PushBackResolvedCell> Cells { get; } = new List<PushBackResolvedCell>();

        public PushBackSideSystem Of(PushBackSide side) => side == PushBackSide.A ? SideA : SideB;

        /// <summary>La celda resuelta en (ranura, nivel 1-based), o null si no existe.</summary>
        public PushBackResolvedCell Cell(int frontIndex, int levelNumber)
        {
            foreach (var cell in Cells)
            {
                if (cell != null && cell.FrontIndex == frontIndex && cell.LevelNumber == levelNumber)
                {
                    return cell;
                }
            }

            return null;
        }
    }
}
