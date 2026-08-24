using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el estado del editor de un Push Back COMPUESTO. Es un COORDINADOR, no un modelo nuevo: contiene DOS
    /// <see cref="PushBackEditorState"/> —uno por lado, exactamente el que ya conducia el editor de un sentido— mas
    /// la intencion de la INTERFAZ (hueco, separador central, topologia por celda y overrides de estructura).
    ///
    /// <para>
    /// Esa es la forma del UX acordado: un selector <b>Lado A / Lado B</b> y, debajo, la MISMA matriz Frente x Nivel
    /// del lado activo. No hay matriz tridimensional, no hay un segundo modelo de seleccion y los cinco alcances
    /// (<see cref="DynamicRackCellScope"/>) siguen siendo los mismos, aplicados dentro del lado activo.
    /// </para>
    /// <para>
    /// Nada de lo que se edita en un lado toca al otro, y cambiar de lado o de topologia NO destruye configuracion:
    /// la del lado que deja de dibujar queda DORMANTE en su propio estado y reaparece intacta.
    /// </para>
    /// </summary>
    public sealed class PushBackCompositeEditorState
    {
        private readonly List<PushBackTopologyCell> topologies = new List<PushBackTopologyCell>();
        private readonly List<bool> presentA = new List<bool>();
        private readonly List<bool> presentB = new List<bool>();

        public PushBackCompositeEditorState()
            : this(new PushBackEditorState(), new PushBackEditorState())
        {
        }

        public PushBackCompositeEditorState(PushBackEditorState sideA, PushBackEditorState sideB)
        {
            SideA = sideA ?? new PushBackEditorState();
            SideB = sideB ?? new PushBackEditorState();
        }

        /// <summary>El estado del lado A. Es EL MISMO tipo que conduce un Push Back de un solo sentido.</summary>
        public PushBackEditorState SideA { get; }

        /// <summary>El estado del lado B. Vive aunque el lado este ausente: por eso su configuracion no se pierde.</summary>
        public PushBackEditorState SideB { get; }

        /// <summary>El lado que la matriz esta editando ahora mismo.</summary>
        public PushBackSide ActiveSide { get; private set; } = PushBackSide.A;

        /// <summary>Si el rack tiene lado B. False = Push Back de un solo sentido, el legacy.</summary>
        public bool SideBPresent { get; private set; }

        /// <summary>Separacion fisica (in) entre la linea terminal de A y la inicial de B. Nunca negativa.</summary>
        public double Gap { get; private set; }

        /// <summary>Si el hueco lleva el separador central (la MISMA pieza del rack).</summary>
        public bool CentralSeparator { get; private set; }

        /// <summary>Override manual de la estructura del lado A, o null si sigue la propuesta.</summary>
        public int? StructureOverrideA { get; private set; }

        /// <summary>Override manual de la estructura del lado B, o null si sigue la propuesta.</summary>
        public int? StructureOverrideB { get; private set; }

        /// <summary>La topologia que hereda una celda sin entrada propia.</summary>
        public PushBackCellTopology DefaultTopology { get; private set; } = PushBackCellTopology.Encontradas;

        /// <summary>El sentido que hereda una corrida sin entrada propia.</summary>
        public PushBackRunDirection DefaultDirection { get; private set; } = PushBackRunDirection.AToB;

        /// <summary>El estado del lado ACTIVO: al que van la matriz, la seleccion y los alcances.</summary>
        public PushBackEditorState Active => ActiveSide == PushBackSide.A ? SideA : SideB;

        /// <summary>El estado de un lado concreto.</summary>
        public PushBackEditorState Of(PushBackSide side) => side == PushBackSide.A ? SideA : SideB;

        /// <summary>Ranuras transversales FISICAS: la mayor demanda de los dos lados.</summary>
        public int SlotCount => Math.Max(SideA.Structure.Count, SideBPresent ? SideB.Structure.Count : 0);

        // ---- Lado activo y presencia ------------------------------------------------------------------------

        /// <summary>
        /// Cambia el lado que la matriz edita. NO toca ninguna configuracion: la del lado que se abandona queda
        /// intacta en su propio estado, con su seleccion y su celda primaria.
        /// </summary>
        public void SetActiveSide(PushBackSide side)
        {
            if (side == PushBackSide.B && !SideBPresent)
            {
                return;   // no se puede editar un lado que el rack no tiene
            }

            ActiveSide = side;
        }

        /// <summary>
        /// Declara o retira el lado B. Retirarlo NO borra su configuracion: el rack vuelve a ser de un solo sentido y
        /// el lado B queda dormante, listo para reaparecer tal cual estaba.
        /// </summary>
        public void SetSideBPresent(bool present)
        {
            SideBPresent = present;
            if (!present && ActiveSide == PushBackSide.B)
            {
                ActiveSide = PushBackSide.A;
            }
        }

        /// <summary>Si la ranura existe en el lado. Una ranura ausente no aporta celda, cama, larguero ni tope.</summary>
        public bool IsSlotPresent(PushBackSide side, int slot)
        {
            var list = side == PushBackSide.A ? presentA : presentB;
            if (side == PushBackSide.B && !SideBPresent)
            {
                return false;
            }

            if (slot < 0 || slot >= Of(side).Structure.Count)
            {
                return false;
            }

            return slot >= list.Count || list[slot];
        }

        /// <summary>
        /// Declara o retira una ranura de un lado. Es lo que expresa el caso «A=3 y B=4»: la cuarta ranura existe
        /// solo en B. La configuracion de la ranura retirada queda DORMANTE en su lado.
        /// </summary>
        public void SetSlotPresent(PushBackSide side, int slot, bool present)
        {
            if (slot < 0)
            {
                return;
            }

            var list = side == PushBackSide.A ? presentA : presentB;
            while (list.Count <= slot)
            {
                list.Add(true);
            }

            list[slot] = present;
        }

        // ---- Interfaz central --------------------------------------------------------------------------------

        /// <summary>Fija el hueco. Es una longitud fisica real; un valor negativo se lee como cero.</summary>
        public void SetGap(double gap) => Gap = gap > 0.0 ? gap : 0.0;

        /// <summary>Fija el separador central. Solo se materializa si hay hueco donde ponerlo.</summary>
        public void SetCentralSeparator(bool value) => CentralSeparator = value;

        /// <summary>True cuando se pidio separador central y no hay hueco: el editor lo avisa antes de resolver.</summary>
        public bool CentralSeparatorWithoutGap => CentralSeparator && Gap <= 0.0;

        // ---- Estructura efectiva por lado --------------------------------------------------------------------

        /// <summary>
        /// Fija el override manual de la estructura de un lado. Null es la RESTAURACION: el lado vuelve a seguir la
        /// propuesta derivada ACTUAL, no la que hubiera cuando se escribio el override.
        /// </summary>
        public void SetStructureOverride(PushBackSide side, int? positions)
        {
            var value = positions.HasValue && positions.Value >= PushBackCellDepth.MinimumPalletsDeep
                ? positions
                : null;
            if (side == PushBackSide.A)
            {
                StructureOverrideA = value;
            }
            else
            {
                StructureOverrideB = value;
            }
        }

        /// <summary>Restaurar la estructura de un lado es exactamente eliminar su override manual.</summary>
        public void RestoreStructure(PushBackSide side) => SetStructureOverride(side, null);

        /// <summary>El override almacenado de un lado.</summary>
        public int? StructureOverride(PushBackSide side)
            => side == PushBackSide.A ? StructureOverrideA : StructureOverrideB;

        // ---- Topologia por celda -----------------------------------------------------------------------------

        /// <summary>Los valores por defecto del rack. Escribirlos hace que las celdas sin entrada propia los hereden.</summary>
        public void SetDefaults(PushBackCellTopology topology, PushBackRunDirection direction)
        {
            DefaultTopology = topology;
            DefaultDirection = direction;
        }

        /// <summary>La topologia efectiva de una celda: su entrada si la hay, y si no el default del rack.</summary>
        public PushBackCellTopology TopologyAt(int slot, int level)
            => Stored(slot, level)?.Topology ?? DefaultTopology;

        /// <summary>El sentido efectivo de una celda corrida.</summary>
        public PushBackRunDirection DirectionAt(int slot, int level)
            => Stored(slot, level)?.Direction ?? DefaultDirection;

        /// <summary>
        /// Escribe la topologia y el sentido de las celdas del ALCANCE, resuelto sobre la matriz del lado ACTIVO con
        /// el MISMO <see cref="DynamicRackCellScopeResolver"/> y la MISMA seleccion multiple que el resto del editor.
        /// No hay un segundo modelo de seleccion. Devuelve cuantas celdas se escribieron.
        /// <para>
        /// La topologia es del RACK, no de un lado —una corrida pertenece a los dos—, pero se EDITA desde el lado
        /// activo porque es ahi donde el usuario esta mirando la celda.
        /// </para>
        /// </summary>
        public int ApplyTopology(
            PushBackCellTopology topology, PushBackRunDirection direction, DynamicRackCellScope scope)
        {
            var matrix = Active.Structure;
            var targets = DynamicRackCellScopeResolver.Targets(
                matrix.LevelCounts(),
                matrix.SelectedFrontIndex,
                matrix.SelectedLevelIndex,
                scope,
                matrix.SelectedCells());

            var written = 0;
            foreach (var target in targets)
            {
                SetCell(target.FrontIndex, target.LevelIndex, topology, direction);
                written++;
            }

            return written;
        }

        /// <summary>Escribe una celda. Escribir el valor por defecto BORRA la entrada: el archivo no acumula ruido.</summary>
        public void SetCell(int slot, int level, PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var existing = Stored(slot, level);
            if (topology == DefaultTopology && direction == DefaultDirection)
            {
                if (existing != null)
                {
                    topologies.Remove(existing);
                }

                return;
            }

            if (existing == null)
            {
                topologies.Add(new PushBackTopologyCell
                {
                    Frente = slot, Level = level, Topology = topology, Direction = direction
                });
                return;
            }

            existing.Topology = topology;
            existing.Direction = direction;
        }

        private PushBackTopologyCell Stored(int slot, int level)
            => topologies.FirstOrDefault(cell => cell != null && cell.Frente == slot && cell.Level == level);

        // ---- Proyeccion al dominio ----------------------------------------------------------------------------

        /// <summary>La intencion de interfaz que el ensamblador escribe en el diseno.</summary>
        public PushBackCompositeDesign BuildComposite()
        {
            var composite = new PushBackCompositeDesign
            {
                Gap = Gap,
                CentralSeparator = CentralSeparator,
                StructureOverrideA = StructureOverrideA,
                StructureOverrideB = StructureOverrideB,
                DefaultTopology = DefaultTopology,
                DefaultDirection = DefaultDirection
            };

            foreach (var cell in topologies)
            {
                composite.Topologies.Add(new PushBackTopologyCell
                {
                    Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction
                });
            }

            return composite;
        }

        /// <summary>Recupera la intencion de interfaz de un diseno cargado.</summary>
        public void LoadComposite(PushBackCompositeDesign composite)
        {
            topologies.Clear();
            if (composite == null)
            {
                Gap = 0.0;
                CentralSeparator = false;
                StructureOverrideA = null;
                StructureOverrideB = null;
                DefaultTopology = SideBPresent ? PushBackCellTopology.Encontradas : PushBackCellTopology.SoloA;
                DefaultDirection = PushBackRunDirection.AToB;
                return;
            }

            Gap = composite.Gap > 0.0 ? composite.Gap : 0.0;
            CentralSeparator = composite.CentralSeparator;
            StructureOverrideA = composite.StructureOverrideA;
            StructureOverrideB = composite.StructureOverrideB;
            DefaultTopology = composite.DefaultTopology;
            DefaultDirection = composite.DefaultDirection;
            foreach (var cell in composite.Topologies)
            {
                if (cell != null)
                {
                    topologies.Add(new PushBackTopologyCell
                    {
                        Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction
                    });
                }
            }
        }

        /// <summary>La configuracion funcional del lado B tal como la persiste el dominio.</summary>
        public PushBackSideDesign BuildSideB()
        {
            if (!SideBPresent)
            {
                return null;
            }

            var matrix = SideB.Structure;
            var side = new PushBackSideDesign
            {
                IsPresent = true,
                LoadLevels = Math.Max(1, matrix.MaxLoadLevels()),
                FirstLevelHeight = matrix.Count > 0
                    ? matrix.Fronts[0].FirstLevelHeight
                    : PushBackDefaults.DefaultFirstLevelHeight,
                LegacyHighEndBeamPeralte = PushBackDefaults.HighEndBeamDefaultPeralte,
                RearTope = SideB.RearTopeConfig()
            };

            var fronts = SideB.BuildEnvelopeFrontDesigns();
            for (var slot = 0; slot < fronts.Count; slot++)
            {
                // Una ranura ausente viaja como entrada NULA: es lo que dice «esta ranura no existe en este lado»
                // sin destruir la configuracion que el lado guarda para ella.
                if (!IsSlotPresent(PushBackSide.B, slot))
                {
                    side.Fronts.Add(null);
                    side.FrontConfigs.Add(null);
                    continue;
                }

                side.Fronts.Add(fronts[slot]);
                var levels = Math.Max(1, matrix.Fronts[slot].LoadLevels);
                var config = new PushBackFrontConfig
                {
                    DefaultPalletsDeep = Math.Max(
                        PushBackCellDepth.MinimumPalletsDeep, matrix.Fronts[slot].PalletsDeep)
                };
                for (var level = 0; level < levels; level++)
                {
                    var cell = SideB.Cell(slot, level);
                    config.HighEndBeamPeraltes.Add(cell.HighEndBeamPeralte);
                    config.PalletsDeepOverrides.Add(
                        cell.PalletsDeepOverride.HasValue
                        && cell.PalletsDeepOverride.Value >= PushBackCellDepth.MinimumPalletsDeep
                            ? cell.PalletsDeepOverride
                            : null);
                    config.DrawPallets.Add(cell.DrawPallet ? true : (bool?)null);
                }

                side.FrontConfigs.Add(config);
            }

            return side;
        }

        /// <summary>Las ranuras del lado A que el ensamblador debe RETIRAR del diseno legacy por estar ausentes.</summary>
        public IReadOnlyList<int> AbsentSlotsOfA()
        {
            var result = new List<int>();
            for (var slot = 0; slot < SideA.Structure.Count; slot++)
            {
                if (!IsSlotPresent(PushBackSide.A, slot))
                {
                    result.Add(slot);
                }
            }

            return result;
        }

        // ---- Snapshot / rollback -------------------------------------------------------------------------------

        /// <summary>Copia profunda del estado COMPLETO para deshacer: los dos lados, la interfaz y la presencia.</summary>
        public PushBackCompositeEditorSnapshot Snapshot()
            => new PushBackCompositeEditorSnapshot(
                SideA.Snapshot(),
                SideB.Snapshot(),
                ActiveSide,
                SideBPresent,
                Gap,
                CentralSeparator,
                StructureOverrideA,
                StructureOverrideB,
                DefaultTopology,
                DefaultDirection,
                topologies.Select(cell => new PushBackTopologyCell
                {
                    Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction
                }).ToList(),
                presentA.ToList(),
                presentB.ToList());

        /// <summary>Restaura el estado completo desde una copia.</summary>
        public void Restore(PushBackCompositeEditorSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            SideA.Restore(snapshot.SideA);
            SideB.Restore(snapshot.SideB);
            ActiveSide = snapshot.ActiveSide;
            SideBPresent = snapshot.SideBPresent;
            Gap = snapshot.Gap;
            CentralSeparator = snapshot.CentralSeparator;
            StructureOverrideA = snapshot.StructureOverrideA;
            StructureOverrideB = snapshot.StructureOverrideB;
            DefaultTopology = snapshot.DefaultTopology;
            DefaultDirection = snapshot.DefaultDirection;
            topologies.Clear();
            foreach (var cell in snapshot.Topologies)
            {
                topologies.Add(new PushBackTopologyCell
                {
                    Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction
                });
            }

            presentA.Clear();
            presentA.AddRange(snapshot.PresentA);
            presentB.Clear();
            presentB.AddRange(snapshot.PresentB);
        }
    }

    /// <summary>Copia profunda del estado compuesto, para el rollback transaccional del editor.</summary>
    public sealed class PushBackCompositeEditorSnapshot
    {
        public PushBackCompositeEditorSnapshot(
            PushBackEditorSnapshot sideA,
            PushBackEditorSnapshot sideB,
            PushBackSide activeSide,
            bool sideBPresent,
            double gap,
            bool centralSeparator,
            int? structureOverrideA,
            int? structureOverrideB,
            PushBackCellTopology defaultTopology,
            PushBackRunDirection defaultDirection,
            IReadOnlyList<PushBackTopologyCell> topologies,
            IReadOnlyList<bool> presentA,
            IReadOnlyList<bool> presentB)
        {
            SideA = sideA;
            SideB = sideB;
            ActiveSide = activeSide;
            SideBPresent = sideBPresent;
            Gap = gap;
            CentralSeparator = centralSeparator;
            StructureOverrideA = structureOverrideA;
            StructureOverrideB = structureOverrideB;
            DefaultTopology = defaultTopology;
            DefaultDirection = defaultDirection;
            Topologies = topologies ?? new List<PushBackTopologyCell>();
            PresentA = presentA ?? new List<bool>();
            PresentB = presentB ?? new List<bool>();
        }

        public PushBackEditorSnapshot SideA { get; }
        public PushBackEditorSnapshot SideB { get; }
        public PushBackSide ActiveSide { get; }
        public bool SideBPresent { get; }
        public double Gap { get; }
        public bool CentralSeparator { get; }
        public int? StructureOverrideA { get; }
        public int? StructureOverrideB { get; }
        public PushBackCellTopology DefaultTopology { get; }
        public PushBackRunDirection DefaultDirection { get; }
        public IReadOnlyList<PushBackTopologyCell> Topologies { get; }
        public IReadOnlyList<bool> PresentA { get; }
        public IReadOnlyList<bool> PresentB { get; }
    }
}
