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
        /// <summary>
        /// Si el lado B se declaro alguna vez. Distingue un lado RECIEN CREADO —que nace entero en blanco— de uno
        /// DORMANTE, que vuelve con lo que el usuario le dejo. No es presencia: la presencia se deriva de «En
        /// blanco».
        /// </summary>
        private bool sideBEverDeclared;

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

            // I-42 (ronda post-5a73b92) — LA CELDA SELECCIONADA ES DEL RACK, no del lado.
            //
            // Cada lado guarda su propia matriz, y con ella su propia celda primaria. Al cambiar de lado el cursor
            // saltaba a la celda que ESE lado tenia seleccionada la ultima vez —normalmente la 1—, asi que el
            // usuario elegia el frente 3 mirando el rack, cambiaba al lado B para declararlo y la casilla de
            // presencia escribia en el frente 1. Eso es lo que hacia parecer que «solo F1 puede hacerse compuesto».
            //
            // La celda es una posicion FISICA del rack: el frente y el nivel existen en los dos lados. Se lleva al
            // lado que se va a editar ANTES de cambiar, acotada a lo que ese lado tiene.
            MirrorSelection();
            ActiveSelection = selection;
            ActiveSide = selection == PushBackSideSelection.B ? PushBackSide.B : PushBackSide.A;
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
            var declaring = present && !SideBPresent;
            SideBPresent = present;
            if (!present)
            {
                ActiveSide = PushBackSide.A;
                ActiveSelection = PushBackSideSelection.A;
            }

            if (declaring)
            {
                DeclareSideBCapability();
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

        /// <summary>
        /// I-42 (ronda post-5a73b92) — lo que ocurre al declarar la CAPACIDAD del lado B, y solo eso.
        ///
        /// <para>
        /// Declarar el lado B NO es declarar su presencia en ningun frente. Son tres estados distintos y esta
        /// iniciativa los separa: <b>capacidad del rack</b> («existe el lado B como posibilidad»),
        /// <b>presencia en el frente</b> («este frente tiene fisicamente lado B») y <b>topologia de la celda</b>
        /// (Solo A / Solo B / Encontradas / Corrida). Antes activar el modo compuesto mutaba todos los frentes de A
        /// a la vez: es la regresion que el dueño vio.
        /// </para>
        /// <para>
        /// Se hace AQUI y no en la ventana para que el modelo no dependa del orden en que se toquen los controles.
        /// Tres cosas, en este orden:
        /// </para>
        /// <list type="number">
        /// <item>un lado B que nunca existio se INICIALIZA con los defaults del producto, los mismos que uso el
        /// lado A. Sin esto B nacia con el default del Dinamico y los dos lados arrancaban en troqueles distintos
        /// con la misma intencion a la vista;</item>
        /// <item>la retícula transversal se iguala: es UNA sola y los indices de ranura significan lo mismo;</item>
        /// <item>el lado B nace AUSENTE en todas las ranuras. La presencia la declara el usuario, frente a frente.</item>
        /// </list>
        /// <para>
        /// Un lado B que vuelve de estar DORMANTE conserva su presencia y su configuracion: retirar el lado nunca
        /// destruyo nada, y volver a declararlo tampoco.
        /// </para>
        /// </summary>
        private void DeclareSideBCapability()
        {
            // «Nunca configurado» lo dice una marca propia: el estado del lado B nace con un frente por
            // construccion, asi que contarlos no distingue un lado recien creado de uno dormante.
            var dormant = sideBEverDeclared;
            sideBEverDeclared = true;
            if (!dormant)
            {
                SideB.LoadNew();
            }

            AlignSlotGrid();
            if (dormant)
            {
                // I-42 (A3-NP1, contrato del dueño) — DORMIR NO ES BORRAR. La igualacion del «Alto 1er nivel» es
                // una regla de NACIMIENTO —los dos lados tienen que arrancar en el mismo troquel cuando el lado B
                // se crea a partir de A—, no una sincronizacion permanente. Corria antes de esta guarda, asi que
                // reactivar un lado B ya configurado le escribia el valor de A encima: medido, con A = 6" y B = 14"
                // authored, apagar el lado B conservaba sus 14" —tambien a traves de dos recalculos— y volver a
                // declararlo lo dejaba en 6", con la intencion del usuario perdida sin decirlo. La retícula
                // transversal SI se iguala siempre: es una sola y sus indices significan lo mismo en los dos lados.
                return;   // vuelve de estar dormante: su presencia y su configuracion son las que el usuario dejo
            }

            AlignSideBFirstLevel();

            // El lado B nace ENTERO en blanco: la capacidad esta declarada y todavia no la usa ningun frente.
            // Se escribe directo sobre la matriz porque la guarda de «al menos un frente activo» es del rack, y el
            // rack lo sostiene el lado A.
            var matrix = SideB.Structure;
            for (var slot = 0; slot < matrix.Count; slot++)
            {
                matrix.Fronts[slot].IsActive = false;
            }
        }

        /// <summary>
        /// El lado B arranca con la MISMA intencion de «Alto 1er nivel» que el lado A.
        ///
        /// <para>
        /// No es una copia de la configuracion de A —ni celdas, ni overrides, ni niveles—: es la unica magnitud
        /// donde dos convenciones distintas producen una sorpresa FISICA. Con la misma intencion a la vista, los
        /// dos lados tienen que arrancar en el mismo troquel; que uno venga de un documento existente y el otro se
        /// acabe de crear no autoriza medio paso de diferencia.
        /// </para>
        /// </summary>
        private void AlignSideBFirstLevel()
        {
            var reference = SideA.Structure.Count > 0
                ? SideA.Structure.Fronts[0].FirstLevelHeight
                : (double?)null;
            if (!reference.HasValue)
            {
                return;
            }

            for (var slot = 0; slot < SideB.Structure.Count; slot++)
            {
                var own = slot < SideA.Structure.Count
                    ? SideA.Structure.Fronts[slot].FirstLevelHeight
                    : reference.Value;
                SideB.Structure.Fronts[slot].FirstLevelHeight = own;
            }
        }

        /// <summary>
        /// Si la ranura existe en el lado. Una ranura ausente no aporta celda, cama, larguero ni tope.
        ///
        /// <para>
        /// I-42 (ronda post-82e918b) — es DERIVADA de «En blanco», que es la unica intencion visible. Un frente en
        /// blanco (I-33) conserva su claro y su estructura pero no lleva ninguna carga, que es exactamente lo que
        /// significa «este frente no tiene lado B». Antes existian dos controles para la misma decision —«En
        /// blanco» y «Frente presente en este lado»— y competian: uno quitaba cabeceras donde no debia y el otro
        /// no. Ahora hay una sola autoridad y la presencia se lee de ella.
        /// </para>
        /// </summary>
        public bool IsSlotPresent(PushBackSide side, int slot)
        {
            if (side == PushBackSide.B && !SideBPresent)
            {
                return false;
            }

            var matrix = Of(side).Structure;
            return slot >= 0 && slot < matrix.Count && matrix.IsActive(slot);
        }

        /// <summary>
        /// Declara o retira una ranura de un lado. Es lo que expresa el caso «A=3 y B=4»: la cuarta ranura existe
        /// solo en B. La configuracion de la ranura retirada queda DORMANTE en su lado.
        /// </summary>
        public bool SetSlotPresent(PushBackSide side, int slot, bool present)
        {
            if (slot < 0 || slot >= Of(side).Structure.Count)
            {
                return false;
            }

            // Escribe la UNICA intencion: «En blanco». La presencia por lado no tiene almacenamiento propio.
            // El lado B puede quedarse ENTERO en blanco —capacidad declarada y sin usar— mientras A sostenga el
            // rack; esa excepcion solo la puede conceder quien conoce los dos lados.
            return Of(side).SetActive(slot, present, allowAllBlank: OtherSideHasAnyFront(side));
        }

        /// <summary>Si el OTRO lado sostiene el rack por si solo: entonces este puede quedarse sin frentes.</summary>
        private bool OtherSideHasAnyFront(PushBackSide side)
        {
            var other = side == PushBackSide.A ? PushBackSide.B : PushBackSide.A;
            if (other == PushBackSide.B && !SideBPresent)
            {
                return false;
            }

            var matrix = Of(other).Structure;
            for (var slot = 0; slot < matrix.Count; slot++)
            {
                if (matrix.IsActive(slot))
                {
                    return true;
                }
            }

            return false;
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
            var beforeA = SideA.Structure.Count;
            var beforeB = SideB.Structure.Count;

            SideA.SetFrontCount(count);
            if (SideBPresent)
            {
                SideB.SetFrontCount(count);
            }

            // I-42 (A1/H5) — la RETICULA es compartida; el ALMACENAMIENTO no. Anadir un frente en un lado amplia la
            // rejilla de los dos —tienen que compartir posiciones— pero solo declara bahia en el lado que lo pidio:
            // el otro recibe la ranura AUSENTE. Antes las ranuras nuevas nacian activas en los dos, asi que crecer
            // por A convertia el lado B en almacenamiento sin que nadie lo decidiera.
            var dormant = ActiveSide == PushBackSide.B ? PushBackSide.A : PushBackSide.B;
            var before = dormant == PushBackSide.A ? beforeA : beforeB;
            for (var slot = before; slot < count; slot++)
            {
                SetSlotPresent(dormant, slot, false);
            }
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

        /// <summary>
        /// La topologia EFECTIVA de una celda: la que el rack puede construir de verdad con los lados que esa celda
        /// tiene.
        ///
        /// <para>
        /// I-42 (ronda post-5a73b92): activar el modo compuesto es una CAPACIDAD del rack, no un permiso para mutar
        /// cada celda. Un frente sin lado B sigue siendo Solo A por mucho que el default del rack sea «encontradas»,
        /// y esta es la respuesta que ve toda la ventana. La INTENCION guardada no se toca —vive en
        /// <see cref="StoredTopologyAt"/> y vuelve a mandar en cuanto el frente reciba su lado B—, exactamente como
        /// la intencion de tope.
        /// </para>
        /// </summary>
        public PushBackCellTopology TopologyAt(int slot, int level)
        {
            var hasA = IsSlotPresent(PushBackSide.A, slot) && level < LevelsOf(PushBackSide.A, slot);
            var hasB = IsSlotPresent(PushBackSide.B, slot) && level < LevelsOf(PushBackSide.B, slot);
            return hasA || hasB
                ? Degrade(StoredTopologyAt(slot, level), hasA, hasB)
                : StoredTopologyAt(slot, level);
        }

        /// <summary>
        /// La topologia GUARDADA de una celda: su entrada si la hay, y si no el default del rack. Es la INTENCION,
        /// se pueda construir hoy o no, y es lo que se persiste.
        /// </summary>
        public PushBackCellTopology StoredTopologyAt(int slot, int level)
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
            var surface = TopeSurface(slot, level);
            return (surface.AppliesToA, surface.AppliesToB);
        }

        /// <summary>
        /// ERROR 10 (I-42) — todo lo que la UI necesita para que el usuario PREDIGA el resultado en planta sin
        /// conocer la implementacion: la topologia que la celda tiene de verdad, que lado es efectivo, cual queda
        /// como intencion dormante y DONDE aterriza fisicamente el tope.
        ///
        /// <para>
        /// Las tres cosas son distintas y la superficie las separa a proposito. La INTENCION es lo que el usuario
        /// guardo por lado (<see cref="RearTopeAt"/>) y no se borra nunca. La APLICABILIDAD es lo que la topologia
        /// admite hoy. Y la MATERIALIZACION es la pieza, que solo existe donde las dos coinciden.
        /// </para>
        /// </summary>
        public PushBackTopeSurface TopeSurface(int slot, int level)
        {
            var hasA = IsSlotPresent(PushBackSide.A, slot) && level < LevelsOf(PushBackSide.A, slot);
            var hasB = IsSlotPresent(PushBackSide.B, slot) && level < LevelsOf(PushBackSide.B, slot);
            if (!hasA && !hasB)
            {
                return default;
            }

            var topology = Degrade(TopologyAt(slot, level), hasA, hasB);
            var direction = DirectionAt(slot, level);
            switch (topology)
            {
                case PushBackCellTopology.SoloA:
                    // Una cama de un lado descarga en SU pasillo, asi que su extremo alto mira al centro del rack.
                    return new PushBackTopeSurface(topology, direction, true, false, atInterface: true);
                case PushBackCellTopology.SoloB:
                    return new PushBackTopeSurface(topology, direction, false, true, atInterface: true);
                case PushBackCellTopology.Corrida:
                    // La corrida cruza el rack: su unico extremo alto es el EXTERIOR del lado hacia el que fluye.
                    return direction == PushBackRunDirection.AToB
                        ? new PushBackTopeSurface(topology, direction, false, true, atInterface: false)
                        : new PushBackTopeSurface(topology, direction, true, false, atInterface: false);
                default:
                    return new PushBackTopeSurface(topology, direction, hasA, hasB, atInterface: true);
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
            foreach (var slot in composite.AbsentSlotsA)
            {
                SetSlotPresent(PushBackSide.A, slot, false);
            }

            foreach (var slot in composite.AbsentSlotsB)
            {
                SetSlotPresent(PushBackSide.B, slot, false);
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

            // I-42 (ronda post-5a73b92) — CAPACIDAD sin PRESENCIA no es un lado.
            //
            // Mientras el usuario no declare el lado B en ningun frente, el rack es fisicamente el de un solo
            // sentido: misma longitud, mismas cabeceras, misma seguridad, mismo BOM. Devolver un lado «presente» y
            // vacio lo alargaba y le añadia una segunda cara de carga por una capacidad que todavia no tiene
            // ninguna cama. La capacidad sigue declarada en el editor —la seccion compuesta permanece abierta— y en
            // cuanto un frente reciba su lado B el rack pasa a compuesto.
            var declared = false;
            for (var slot = 0; slot < SlotCount && !declared; slot++)
            {
                declared = IsSlotPresent(PushBackSide.B, slot);
            }

            if (!declared)
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
                RearTope = SideB.RearTopeConfig(),
                DefensePieceId = SideB.DefensePieceId
            };

            var fronts = SideB.BuildEnvelopeFrontDesigns();
            for (var slot = 0; slot < fronts.Count; slot++)
            {
                // I-42 (correccion aislada 2B) — una ranura EN BLANCO viaja COMPLETA y se declara aparte, en
                // AbsentSlotsB, exactamente como el lado A. Escribirla como entrada nula perdia su declaracion
                // fisica —ancho, BFR, override de larguero—, asi que ponerla en blanco encogia la bahia y movia
                // todas las lineas posteriores. Ahora solo queda DORMANTE, y al quitar el blanco reaparece.
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
        public IReadOnlyList<int> AbsentSlotsOfA() => AbsentSlotsOf(PushBackSide.A, SideA.Structure.Count);

        /// <summary>Las ranuras EN BLANCO del lado B: la misma declaracion, para el mismo fin.</summary>
        public IReadOnlyList<int> AbsentSlotsOfB() => AbsentSlotsOf(PushBackSide.B, SideB.Structure.Count);

        private IReadOnlyList<int> AbsentSlotsOf(PushBackSide side, int slots)
        {
            var result = new List<int>();
            for (var slot = 0; slot < slots; slot++)
            {
                if (!IsSlotPresent(side, slot))
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
                sideBEverDeclared);

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

            sideBEverDeclared = snapshot.SideBEverDeclared;
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
            bool sideBEverDeclared)
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
            SideBEverDeclared = sideBEverDeclared;
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
        /// <summary>Si el lado B se declaro alguna vez: distingue un lado recien creado de uno dormante.</summary>
        public bool SideBEverDeclared { get; }
    }
}
