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
    /// <summary>
    /// Que lados alcanza una edicion. NO es un lado fisico: <see cref="Both"/> es una operacion del editor y no
    /// existe en el dominio, en el archivo ni en el dibujo.
    /// </summary>
    public enum PushBackSideSelection
    {
        A = 0,
        B = 1,
        Both = 2
    }

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

        /// <summary>
        /// Que lados alcanza una edicion: uno, el otro, o LOS DOS.
        ///
        /// <para>
        /// «Ambos» es una operacion de EDICION, no un tercer lado: no existe en el dominio ni en el archivo, no posee
        /// ninguna pieza y no aparece en el dibujo. Solo dice que una misma intencion —niveles, fondo base, alto del
        /// primer nivel, fondo por celda, tarima— se escribe en A y en B a la vez, que es como el dueño configura un
        /// rack simetrico sin repetirlo todo dos veces.
        /// </para>
        /// </summary>
        public PushBackSideSelection ActiveSelection { get; private set; } = PushBackSideSelection.A;

        /// <summary>Los estados que una edicion debe alcanzar con la seleccion vigente.</summary>
        public IReadOnlyList<PushBackEditorState> EditTargets()
        {
            if (ActiveSelection != PushBackSideSelection.Both || !SideBPresent)
            {
                return new[] { Active };
            }

            return new[] { SideA, SideB };
        }

        /// <summary>Los LADOS que una edicion alcanza, en el mismo orden que <see cref="EditTargets"/>.</summary>
        public IReadOnlyList<PushBackSide> EditSides()
            => ActiveSelection == PushBackSideSelection.Both && SideBPresent
                ? new[] { PushBackSide.A, PushBackSide.B }
                : new[] { ActiveSide };

        /// <summary>
        /// Fija la seleccion de edicion. Con «Ambos», la matriz sigue siendo la del lado A —hace falta UNA para
        /// seleccionar celdas— y el lado B recibe la MISMA seleccion, de modo que un alcance significa lo mismo en
        /// los dos. Sin lado B declarado, «Ambos» no existe y se cae al lado A.
        /// </summary>
        public void SetActiveSelection(PushBackSideSelection selection)
        {
            if (!SideBPresent)
            {
                ActiveSelection = PushBackSideSelection.A;
                ActiveSide = PushBackSide.A;
                return;
            }

            ActiveSelection = selection;
            ActiveSide = selection == PushBackSideSelection.B ? PushBackSide.B : PushBackSide.A;
            if (selection == PushBackSideSelection.Both)
            {
                MirrorSelection();
            }
        }

        /// <summary>
        /// Lleva la seleccion de celda del lado activo al otro, acotada a lo que ese otro lado tiene. Es lo que hace
        /// que «esta celda» signifique lo mismo en los dos cuando se editan a la vez.
        /// </summary>
        public void MirrorSelection()
        {
            var source = Active.Structure;
            var target = (ActiveSide == PushBackSide.A ? SideB : SideA).Structure;
            if (target.Count == 0)
            {
                return;
            }

            var front = Math.Max(0, Math.Min(source.SelectedFrontIndex, target.Count - 1));
            var level = Math.Max(0, Math.Min(source.SelectedLevelIndex, Math.Max(1, target.Fronts[front].LoadLevels) - 1));
            target.ToggleCell(front, level, extendSelection: false);
        }

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
            if (!present)
            {
                ActiveSide = PushBackSide.A;
                ActiveSelection = PushBackSideSelection.A;
            }

            // La topologia POR DEFECTO depende de cuantos sentidos tiene el rack, y esa es la misma regla que ya
            // aplica al cargar: un rack de un sentido es Solo A, y uno de dos son camas encontradas. Sin volver a
            // evaluarla aqui, un rack NUEVO al que se le declara el lado B se quedaba en Solo A: B aportaba
            // estructura y ni una sola cama, que es exactamente el «solo quedan las cabeceras» que el dueño vio.
            //
            // Solo se cambia el default de CADA MODO por el del otro. Una eleccion explicita distinta —una corrida,
            // por ejemplo, o Solo B— se respeta: no es el default de nadie, asi que nadie la pisa.
            if (present && DefaultTopology == PushBackCellTopology.SoloA)
            {
                DefaultTopology = PushBackCellTopology.Encontradas;
            }
            else if (!present && DefaultTopology == PushBackCellTopology.Encontradas)
            {
                DefaultTopology = PushBackCellTopology.SoloA;
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

        /// <summary>
        /// Retirar una ranura solo es legal si sigue existiendo EN ALGUN SITIO: una ranura ausente en los dos lados
        /// no es una ranura, es una linea de postes sin nada, y un lado sin ninguna ranura no es un lado. Devuelve
        /// null cuando la operacion es legal, y el motivo cuando no lo es. No corrige nada: responde.
        /// </summary>
        public string WhySlotCannotBeRemoved(PushBackSide side, int slot)
        {
            if (slot < 0 || slot >= SlotCount)
            {
                return "Esa ranura no existe.";
            }

            var other = side == PushBackSide.A ? PushBackSide.B : PushBackSide.A;
            if (!SideBPresent)
            {
                return "El rack es de un solo sentido: retirar una ranura equivale a quitar un frente.";
            }

            if (!IsSlotPresent(other, slot))
            {
                return "La ranura " + (slot + 1) + " solo existe en este lado: retirarla la dejaria sin ningun lado. "
                       + "Declarala en el otro lado primero, o reduce el numero de frentes.";
            }

            var remaining = 0;
            for (var index = 0; index < SlotCount; index++)
            {
                if (index != slot && IsSlotPresent(side, index))
                {
                    remaining++;
                }
            }

            return remaining > 0
                ? null
                : "Es la ultima ranura de este lado: un lado sin ranuras no existe. Apaga «Rack de dos sentidos» "
                  + "si lo que quieres es un rack de un solo sentido.";
        }

        /// <summary>
        /// El numero de ranuras transversales del RACK. La retícula es UNA —los dos lados comparten las mismas
        /// lineas de postes—, asi que el conteo es del rack y no de un lado: crecer por un lado y no por el otro
        /// dejaba media estructura sin contenido a partir de la primera ranura, que es exactamente lo que el dueño
        /// vio. La asimetria A/B se expresa con PRESENCIA (<see cref="SetSlotPresent"/>), que es su autoridad.
        /// </summary>
        public void SetSlotCount(int requested)
        {
            var count = Math.Max(1, requested);
            SideA.SetFrontCount(count);
            if (SideBPresent)
            {
                SideB.SetFrontCount(count);
            }

            // Una ranura nueva nace PRESENTE en los dos lados; retirarla es una decision explicita.
            Trim(presentA, count);
            Trim(presentB, count);
        }

        /// <summary>
        /// Iguala la retícula de los dos lados SIN destruir nada: crece hasta el mayor de los dos y declara AUSENTES
        /// las ranuras que el lado corto no tenia. Es lo que hace falta cuando un lado B dormante —o uno cargado de
        /// un diseño— vuelve con mas o menos ranuras que el rack: recortarlo perderia su configuracion, y darle las
        /// ranuras del otro como presentes inventaria bahias que nadie pidio.
        /// </summary>
        public void AlignSlotGrid()
        {
            if (!SideBPresent)
            {
                return;
            }

            var beforeA = SideA.Structure.Count;
            var beforeB = SideB.Structure.Count;
            var count = Math.Max(1, Math.Max(beforeA, beforeB));

            SideA.SetFrontCount(count);
            SideB.SetFrontCount(count);

            for (var slot = beforeA; slot < count; slot++)
            {
                SetSlotPresent(PushBackSide.A, slot, false);
            }

            for (var slot = beforeB; slot < count; slot++)
            {
                SetSlotPresent(PushBackSide.B, slot, false);
            }

            Trim(presentA, count);
            Trim(presentB, count);
        }

        private static void Trim(List<bool> list, int count)
        {
            while (list.Count > count)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        // ---- Interfaz central --------------------------------------------------------------------------------

        /// <summary>
        /// Fija el hueco CONSERVANDO lo que el usuario escribio. Un valor negativo NO se convierte en cero: se guarda
        /// tal cual y el editor lo bloquea con su diagnostico. Corregir una entrada invalida en silencio es
        /// exactamente lo que hace que un rack acabe siendo distinto del que se pidio.
        /// </summary>
        public void SetGap(double gap) => Gap = gap;

        /// <summary>True cuando el hueco almacenado es una separacion fisica valida.</summary>
        public bool GapIsValid => !double.IsNaN(Gap) && !double.IsInfinity(Gap) && Gap >= 0.0;

        /// <summary>Fija el separador central. Solo se materializa si hay hueco donde ponerlo.</summary>
        public void SetCentralSeparator(bool value) => CentralSeparator = value;

        /// <summary>True cuando se pidio separador central y no hay hueco: el editor lo avisa antes de resolver.</summary>
        public bool CentralSeparatorWithoutGap => CentralSeparator && GapIsValid && Gap <= 0.0;

        // ---- Estructura efectiva por lado --------------------------------------------------------------------

        /// <summary>
        /// Fija el override manual de la estructura de un lado, CONSERVANDO el valor tal cual se escribio.
        ///
        /// <para>
        /// Un valor por debajo del minimo fisico NO se convierte en null: null significa RESTAURAR, y restaurar solo
        /// ocurre por accion explicita del usuario (<see cref="RestoreStructure"/>). Confundir «valor invalido» con
        /// «restaurar» tiraba el ajuste manual sin decir nada.
        /// </para>
        /// </summary>
        public void SetStructureOverride(PushBackSide side, int? positions)
        {
            if (side == PushBackSide.A)
            {
                StructureOverrideA = positions;
            }
            else
            {
                StructureOverrideB = positions;
            }
        }

        /// <summary>True cuando el override almacenado de ese lado existe y NO es construible.</summary>
        public bool StructureOverrideIsInvalid(PushBackSide side)
        {
            var value = StructureOverride(side);
            return value.HasValue && value.Value < PushBackCellDepth.MinimumPalletsDeep;
        }

        /// <summary>
        /// Los diagnosticos de la INTENCION actual, antes de resolver: hueco invalido, separador sin hueco y
        /// overrides manuales no construibles. Es lo que el editor consulta para bloquear sin corregir nada.
        /// </summary>
        public IReadOnlyList<PushBackCompositeDiagnostic> IntentDiagnostics()
            => PushBackCompositeDiagnostics.EvaluateIntent(
                Gap, CentralSeparator, StructureOverrideA, StructureOverrideB);

        /// <summary>True cuando la intencion actual tiene algo que impide resolver.</summary>
        public bool HasBlockingIntent() => IntentDiagnostics().Any(diagnostic => diagnostic.IsBlocking);

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
            var written = 0;
            foreach (var target in Targets(scope))
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
            if (existing == null)
            {
                if (topology == DefaultTopology && direction == DefaultDirection)
                {
                    return;
                }

                topologies.Add(new PushBackTopologyCell
                {
                    Frente = slot, Level = level, Topology = topology, Direction = direction
                });
                return;
            }

            existing.Topology = topology;
            existing.Direction = direction;
            Prune(existing);
        }

        // ---- I-42: el FONDO PROPIO de la cama corrida -----------------------------------------------------------

        /// <summary>
        /// El fondo propio de la cama corrida de una celda, o null si hereda el de una corrida por defecto.
        ///
        /// <para>
        /// Es una autoridad DISTINTA del fondo de A y del de B: cambiarla no toca ninguno de los dos, y cambiar
        /// cualquiera de los dos no la toca a ella. Los tres conviven, y el que gobierna depende de la topologia
        /// vigente de la celda — por eso cambiar de topologia es reversible y no pierde nada.
        /// </para>
        /// </summary>
        public int? CorridaDepthAt(int slot, int level) => Stored(slot, level)?.CorridaDepth;

        /// <summary>Escribe el fondo propio de la cama corrida de una celda. Null lo retira.</summary>
        public void SetCorridaDepth(int slot, int level, int? depth)
        {
            var existing = Stored(slot, level);
            if (existing == null)
            {
                if (!depth.HasValue)
                {
                    return;
                }

                topologies.Add(new PushBackTopologyCell
                {
                    Frente = slot,
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
        /// Escribe el fondo de la cama corrida sobre el ALCANCE, con el MISMO resolutor de alcances y la MISMA
        /// seleccion multiple que el resto del editor. Solo escribe donde hay una corrida: en una celda que no lo es
        /// el valor no significaria nada, y escribirlo en silencio seria inventar configuracion. Devuelve cuantas
        /// celdas se escribieron; el llamante compara con <see cref="CorridaTargets"/> para decir lo que quedo fuera.
        /// </summary>
        public int ApplyCorridaDepth(int? depth, DynamicRackCellScope scope)
        {
            var written = 0;
            foreach (var target in Targets(scope))
            {
                if (TopologyAt(target.FrontIndex, target.LevelIndex) != PushBackCellTopology.Corrida)
                {
                    continue;
                }

                SetCorridaDepth(target.FrontIndex, target.LevelIndex, depth);
                written++;
            }

            return written;
        }

        /// <summary>Cuantas celdas del alcance son corridas y cuantas hay en total. Para explicar, no para decidir.</summary>
        public (int Corridas, int Total) CorridaTargets(DynamicRackCellScope scope)
        {
            var total = 0;
            var corridas = 0;
            foreach (var target in Targets(scope))
            {
                total++;
                if (TopologyAt(target.FrontIndex, target.LevelIndex) == PushBackCellTopology.Corrida)
                {
                    corridas++;
                }
            }

            return (corridas, total);
        }

        /// <summary>
        /// True cuando el alcance mezcla celdas corridas con celdas que no lo son. El campo de fondo significa una
        /// cosa distinta en cada una, asi que la ventana lo dice en vez de aplicar una de las dos a ciegas.
        /// </summary>
        public bool ScopeMixesCorrida(DynamicRackCellScope scope)
        {
            var targets = CorridaTargets(scope);
            return targets.Corridas > 0 && targets.Corridas < targets.Total;
        }

        private IReadOnlyList<DynamicRackCellAddress> Targets(DynamicRackCellScope scope)
        {
            var matrix = Active.Structure;
            return DynamicRackCellScopeResolver.Targets(
                matrix.LevelCounts(),
                matrix.SelectedFrontIndex,
                matrix.SelectedLevelIndex,
                scope,
                matrix.SelectedCells());
        }

        /// <summary>
        /// Retira una entrada que ya no dice nada. El fondo de corrida MANTIENE viva la entrada aunque la topologia
        /// vuelva al default: es configuracion dormante, y perderla obligaria a volver a escribirla.
        /// </summary>
        private void Prune(PushBackTopologyCell cell)
        {
            if (cell.Topology == DefaultTopology
                && cell.Direction == DefaultDirection
                && !cell.CorridaDepth.HasValue)
            {
                topologies.Remove(cell);
            }
        }

        private PushBackTopologyCell Stored(int slot, int level)
            => topologies.FirstOrDefault(cell => cell != null && cell.Frente == slot && cell.Level == level);

        // ---- I-42: los TOPES de los dos lados, visibles y editables por celda ----------------------------------

        /// <summary>
        /// Que topes puede MATERIALIZAR fisicamente una celda, segun su topologia. No decide lo que el usuario quiso:
        /// dice lo que el rack puede construir, que es otra cosa. La intencion del lado que no aplica queda DORMANTE
        /// y vuelve intacta en cuanto la topologia la admite otra vez.
        ///
        /// <para>
        /// Un tope vive en el extremo ALTO de una cama. Con camas encontradas hay DOS extremos altos —uno por lado— y
        /// por tanto dos topes independientes. Con una sola cama solo hay UNO, y esta en el lado alto de esa cama:
        /// en Solo A es A, en Solo B es B, y en una corrida es el lado hacia el que fluye.
        /// </para>
        /// </summary>
        public (bool A, bool B) TopeApplicability(int slot, int level)
        {
            var hasA = IsSlotPresent(PushBackSide.A, slot) && level < LevelsOf(PushBackSide.A, slot);
            var hasB = IsSlotPresent(PushBackSide.B, slot) && level < LevelsOf(PushBackSide.B, slot);
            if (!hasA && !hasB)
            {
                return (false, false);
            }

            switch (Degrade(TopologyAt(slot, level), hasA, hasB))
            {
                case PushBackCellTopology.SoloA:
                    return (true, false);
                case PushBackCellTopology.SoloB:
                    return (false, true);
                case PushBackCellTopology.Corrida:
                    return DirectionAt(slot, level) == PushBackRunDirection.AToB ? (false, true) : (true, false);
                default:
                    return (hasA, hasB);
            }
        }

        /// <summary>La topologia que una celda puede REALMENTE tener con los lados que existen en ella.</summary>
        private static PushBackCellTopology Degrade(PushBackCellTopology requested, bool hasA, bool hasB)
        {
            if (hasA && hasB)
            {
                return requested;
            }

            return hasA ? PushBackCellTopology.SoloA : PushBackCellTopology.SoloB;
        }

        private int LevelsOf(PushBackSide side, int slot)
        {
            var matrix = Of(side).Structure;
            return slot >= 0 && slot < matrix.Count ? Math.Max(1, matrix.Fronts[slot].LoadLevels) : 0;
        }

        /// <summary>La INTENCION de tope de una celda en un lado. Es lo almacenado, se materialice o no.</summary>
        public bool RearTopeAt(PushBackSide side, int slot, int level) => Of(side).Cell(slot, level).RearTopeEnabled;

        /// <summary>
        /// Escribe la intencion de tope de un lado sobre el ALCANCE, con el mismo resolutor de alcances y la misma
        /// seleccion multiple que el resto del editor. Escribe la INTENCION incluso donde la topologia no la
        /// materializa hoy: es exactamente lo que la mantiene viva para cuando vuelva a aplicar. Devuelve cuantas
        /// celdas se escribieron.
        /// </summary>
        public int ApplyRearTope(PushBackSide side, bool enabled, DynamicRackCellScope scope)
        {
            var target = Of(side);
            var written = 0;
            foreach (var address in Targets(scope))
            {
                if (address.FrontIndex >= target.Structure.Count
                    || address.LevelIndex >= LevelsOf(side, address.FrontIndex))
                {
                    continue;
                }

                target.Cell(address.FrontIndex, address.LevelIndex).RearTopeEnabled = enabled;
                written++;
            }

            return written;
        }

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
                    Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction,
                    CorridaDepth = cell.CorridaDepth
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

            Gap = composite.Gap;
            CentralSeparator = composite.CentralSeparator;
            StructureOverrideA = composite.StructureOverrideA;
            StructureOverrideB = composite.StructureOverrideB;
            DefaultTopology = composite.DefaultTopology;
            DefaultDirection = composite.DefaultDirection;

            // La PRESENCIA del lado A vuelve del archivo. El diseño DECLARA las ranuras ausentes en vez de borrarlas
            // —borrarlas desplazaria los indices—, asi que aqui hay que volver a leerlas: sin esto, un rack asimetrico
            // «A = 3 y B = 4» se reabria con las cuatro ranuras en los dos lados.
            presentA.Clear();
            foreach (var slot in composite.AbsentSlotsA)
            {
                SetSlotPresent(PushBackSide.A, slot, false);
            }
            foreach (var cell in composite.Topologies)
            {
                if (cell != null)
                {
                    topologies.Add(new PushBackTopologyCell
                    {
                        Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction,
                    CorridaDepth = cell.CorridaDepth
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
                    Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction,
                    CorridaDepth = cell.CorridaDepth
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
                    Frente = cell.Frente, Level = cell.Level, Topology = cell.Topology, Direction = cell.Direction,
                    CorridaDepth = cell.CorridaDepth
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
