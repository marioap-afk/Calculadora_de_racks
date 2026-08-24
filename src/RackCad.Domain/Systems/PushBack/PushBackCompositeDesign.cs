using System.Collections.Generic;

namespace RackCad.Domain.Systems.PushBack
{
    /// <summary>
    /// I-42 — la intencion COMPUESTA de un Push Back: lo que pertenece a la INTERFAZ entre los dos lados y a la
    /// estructura fisica compartida, no a ninguno de ellos.
    ///
    /// <list type="bullet">
    /// <item><see cref="Gap"/> — la separacion FISICA entre la ultima linea de postes de A y la primera de B. Es una
    /// longitud real del rack, no un desplazamiento visual, ni un fondo ficticio, ni la posicion de una tarima. Puede
    /// ser cero, y aun asi los dos extremos fisicos siguen existiendo.</item>
    /// <item><see cref="CentralSeparator"/> — si ese hueco lleva el separador de cabecera que ya existe en
    /// Dinamico/Push Back. Cuando existe pertenece a la INTERFAZ, no a una cama, y se cuenta UNA vez.</item>
    /// <item><see cref="Topologies"/> — la topologia y el sentido de cada celda (ranura x nivel).</item>
    /// <item>los OVERRIDES de estructura por lado — la estructura efectiva editable.</item>
    /// </list>
    /// </summary>
    public sealed class PushBackCompositeDesign
    {
        /// <summary>Separacion fisica (in) entre la linea terminal de A y la inicial de B. Nunca negativa.</summary>
        public double Gap { get; set; }

        /// <summary>Si el hueco lleva el separador central (la MISMA pieza de catalogo que ya usa el rack).</summary>
        public bool CentralSeparator { get; set; }

        /// <summary>
        /// Override MANUAL de la estructura del lado A, en posiciones de fondo. Null = seguir la propuesta derivada
        /// ACTUAL, que es justo lo que significa «restaurar estructura».
        /// </summary>
        public int? StructureOverrideA { get; set; }

        /// <summary>Override MANUAL de la estructura del lado B, en posiciones de fondo. Null = seguir la propuesta.</summary>
        public int? StructureOverrideB { get; set; }

        /// <summary>
        /// I-42 — las ranuras transversales que NO existen en el lado A. El lado B expresa su ausencia con una
        /// entrada nula en su propia lista; el lado A no puede, porque sus frentes son los del diseno legacy y
        /// quitarlos desplazaria los indices de las ranuras siguientes. Su configuracion queda DORMANTE.
        /// </summary>
        public IList<int> AbsentSlotsA { get; } = new List<int>();

        /// <summary>True cuando la ranura no existe en el lado A.</summary>
        public bool IsSlotAbsentInA(int slot) => AbsentSlotsA.Contains(slot);

        /// <summary>
        /// Topologia por celda. Solo se persisten las celdas que se APARTAN del valor por defecto
        /// (<see cref="DefaultTopology"/>), igual que <see cref="PushBackRearTopeConfig.OffCells"/> solo persiste
        /// desactivaciones: una lista positiva completa nunca llega al archivo.
        /// </summary>
        public IList<PushBackTopologyCell> Topologies { get; } = new List<PushBackTopologyCell>();

        /// <summary>
        /// La topologia que hereda una celda sin entrada propia. <see cref="PushBackCellTopology.Encontradas"/> es el
        /// default de un rack compuesto: es la unica que no destruye configuracion de ninguno de los dos lados.
        /// </summary>
        public PushBackCellTopology DefaultTopology { get; set; } = PushBackCellTopology.Encontradas;

        /// <summary>El sentido que hereda una corrida sin entrada propia.</summary>
        public PushBackRunDirection DefaultDirection { get; set; } = PushBackRunDirection.AToB;

        /// <summary>La entrada almacenada de una celda, o null si hereda los valores por defecto.</summary>
        public PushBackTopologyCell CellAt(int front, int level)
        {
            foreach (var cell in Topologies)
            {
                if (cell != null && cell.Frente == front && cell.Level == level)
                {
                    return cell;
                }
            }

            return null;
        }

        /// <summary>La topologia EFECTIVA de una celda: su entrada si la hay, y si no el default del rack.</summary>
        public PushBackCellTopology TopologyAt(int front, int level)
            => CellAt(front, level)?.Topology ?? DefaultTopology;

        /// <summary>El sentido EFECTIVO de una celda corrida: su entrada si la hay, y si no el default del rack.</summary>
        public PushBackRunDirection DirectionAt(int front, int level)
            => CellAt(front, level)?.Direction ?? DefaultDirection;

        /// <summary>
        /// I-42 — el FONDO PROPIO de la cama corrida de una celda, o null si nunca se fijo.
        ///
        /// <para>
        /// Es una AUTORIDAD PROPIA, no la suma de los fondos de A y B: una corrida de 10 fondos es una cama con
        /// demanda de 10, y eso no obliga a repartir 5 y 5 entre los dos lados ni convierte una estructura 5 + 8 en
        /// una demanda de 13. Los fondos de A y de B siguen existiendo, intactos y DORMANTES, y reaparecen en cuanto
        /// la celda deja de ser corrida.
        /// </para>
        /// </summary>
        public int? CorridaDepthAt(int front, int level) => CellAt(front, level)?.CorridaDepth;

        /// <summary>
        /// Fija el fondo propio de la cama corrida de una celda. Null lo retira, de modo que la celda vuelve a
        /// heredar el fondo por defecto de una corrida (la capacidad de la estructura).
        /// </summary>
        public void SetCorridaDepth(int front, int level, int? depth)
        {
            var existing = CellAt(front, level);
            if (existing == null)
            {
                if (!depth.HasValue)
                {
                    return;
                }

                Topologies.Add(new PushBackTopologyCell
                {
                    Frente = front,
                    Level = level,
                    Topology = DefaultTopology,
                    Direction = DefaultDirection,
                    CorridaDepth = depth
                });
                return;
            }

            existing.CorridaDepth = depth;
            Prune(existing);
        }

        /// <summary>
        /// Fija topologia y sentido de una celda. Escribir el valor por defecto BORRA la entrada, para que el archivo
        /// no acumule intencion que el usuario no expreso.
        /// </summary>
        public void SetCell(int front, int level, PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var existing = CellAt(front, level);
            if (existing == null)
            {
                if (topology == DefaultTopology && direction == DefaultDirection)
                {
                    return;
                }

                Topologies.Add(new PushBackTopologyCell
                {
                    Frente = front, Level = level, Topology = topology, Direction = direction
                });
                return;
            }

            existing.Topology = topology;
            existing.Direction = direction;
            Prune(existing);
        }

        /// <summary>
        /// Retira una entrada que ya no dice nada: topologia y sentido por defecto y sin fondo propio de corrida.
        /// El fondo de corrida MANTIENE viva la entrada aunque la topologia vuelva al default — es configuracion
        /// dormante, y perderla obligaria al usuario a volver a escribirla al reactivar la corrida.
        /// </summary>
        private void Prune(PushBackTopologyCell cell)
        {
            if (cell.Topology == DefaultTopology
                && cell.Direction == DefaultDirection
                && !cell.CorridaDepth.HasValue)
            {
                Topologies.Remove(cell);
            }
        }

        public PushBackCompositeDesign DeepCopy()
        {
            var copy = new PushBackCompositeDesign
            {
                Gap = Gap,
                CentralSeparator = CentralSeparator,
                StructureOverrideA = StructureOverrideA,
                StructureOverrideB = StructureOverrideB,
                DefaultTopology = DefaultTopology,
                DefaultDirection = DefaultDirection
            };

            foreach (var slot in AbsentSlotsA)
            {
                copy.AbsentSlotsA.Add(slot);
            }

            foreach (var cell in Topologies)
            {
                if (cell != null)
                {
                    copy.Topologies.Add(new PushBackTopologyCell
                    {
                        Frente = cell.Frente,
                        Level = cell.Level,
                        Topology = cell.Topology,
                        Direction = cell.Direction,
                        CorridaDepth = cell.CorridaDepth
                    });
                }
            }

            return copy;
        }
    }

    /// <summary>I-42 — la topologia y el sentido almacenados de UNA celda (ranura transversal x nivel, 0-based).</summary>
    public sealed class PushBackTopologyCell
    {
        public int Frente { get; set; }
        public int Level { get; set; }
        public PushBackCellTopology Topology { get; set; }
        public PushBackRunDirection Direction { get; set; }

        /// <summary>
        /// I-42 — el fondo PROPIO de la cama corrida de esta celda. Null = hereda el fondo por defecto de una
        /// corrida. Sobrevive a un cambio de topologia: es configuracion dormante, como los fondos de A y de B.
        /// </summary>
        public int? CorridaDepth { get; set; }
    }
}
