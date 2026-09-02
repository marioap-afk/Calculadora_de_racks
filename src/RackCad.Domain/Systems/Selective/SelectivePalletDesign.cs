using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Domain.Systems.Selective
{
    /// <summary>
    /// The pallet-driven DESIGN of a selective rack: what the advanced editor edits. The user no longer types
    /// beam length / separation / height directly — they describe the pallets (frente, alto, count) per cell of
    /// a bays × levels matrix, and <c>SelectiveGeometryResolver</c> derives the geometry:
    /// <list type="bullet">
    /// <item>larguero LONGITUD = Frente*Count + Tolerance*(Count+1) (the widest level governs the bay),</item>
    /// <item>level separation = roundUpTroquel(roundUpEven(Alto + Clearance) + beam peralte),</item>
    /// <item>post height = roundUpFoot(topLevelY + topPalletAlto/3).</item>
    /// </list>
    /// </summary>
    public sealed class SelectivePalletDesign
    {
        /// <summary>Catalog id of the post used for the cabeceras.</summary>
        public string PostId { get; set; }

        /// <summary>Peralte of the post (drives the larguero troquel X via the parametric mate).</summary>
        public double PostPeralte { get; set; }

        /// <summary>Horizontal tolerance per gap between/around pallets (in). Editable; default 4".</summary>
        public double PalletTolerance { get; set; } = 4.0;

        /// <summary>Vertical clearance ("holgura") above a pallet inside its clear opening (in). Editable; default 6".</summary>
        public double VerticalClearance { get; set; } = 6.0;

        /// <summary>How far a "larguero a piso" sits above the lowest troquel (in), so its ménsula clears the base plate. Editable; default 4".</summary>
        public double FloorBeamRise { get; set; } = 4.0;

        /// <summary>Pallet depth / fondo (in): the depth of the cabeceras in the LATERAL view. Editable.</summary>
        public double PalletDepth { get; set; } = SelectiveRackDefaults.DefaultPalletDepth;

        /// <summary>
        /// Number of cabecera-lines in DEPTH: 1 = single fondo (sencillo); 2/3/4 = doble/triple/cuádruple
        /// profundidad (espalda con espalda). Each extra fondo repeats the whole depth structure (cabecera +
        /// front/back largueros) offset by <see cref="PalletDepth"/> + the gap. Only the LATERAL and PLANTA views
        /// (and the BOM) change; the FRONTAL elevation is identical. Editable; default 1.
        /// </summary>
        public int DepthCount { get; set; } = 1;

        /// <summary>
        /// Separations (in) between consecutive fondos — one value per gap (<see cref="DepthCount"/> - 1 gaps),
        /// front to back. A gap with no value (or a short list) falls back to the last given value, else
        /// <see cref="SelectiveRackDefaults.DefaultSeparator"/>. The same value drives the physical separador blocks in
        /// lateral/planta and their BOM component; the frontal intentionally shows only the gap. Editable per gap.
        /// </summary>
        public IList<double> SeparatorLengths { get; } = new List<double>();

        /// <summary>
        /// Per-fondo pallet depth (in) for fondos 1..N-1: entry <c>k-1</c> is fondo <c>k</c>'s own fondo. A value &lt;= 0
        /// (or a short list) means that fondo inherits fondo 0's <see cref="PalletDepth"/>. Lets each back-to-back line
        /// carry its own depth (one side deeper than the other). Fondo 0's depth is <see cref="PalletDepth"/>.
        /// </summary>
        public IList<double> ExtraFondoDepths { get; } = new List<double>();

        /// <summary>
        /// Optional CUSTOM cabecera (frame) depth per fondo (in), index <c>k</c> = fondo <c>k</c> (fondo 0 included). A
        /// value &lt;= 0 (or a short list) leaves that fondo's cabecera depth DERIVED by the rule (pallet depth −
        /// <see cref="SelectiveRackDefaults.CabeceraFondoAllowance"/>). Lets a line override the tarima − 6 rule.
        /// </summary>
        public IList<double> CabeceraFondoOverrides { get; } = new List<double>();

        /// <summary>The bays of fondo 0 (the primary/front fondo), left to right. Each carries its own column of level cells.</summary>
        public IList<SelectiveBayDesign> Bays { get; } = new List<SelectiveBayDesign>();

        /// <summary>
        /// Per-fondo level matrices for fondos 1..N-1 (a doble-profundidad rack where each side faces a different
        /// aisle and can carry its OWN levels/heights). Entry <c>k-1</c> is fondo <c>k</c>'s bays; a missing or empty
        /// entry means that fondo inherits fondo 0's <see cref="Bays"/>. The horizontal grid (bay widths / post
        /// positions) is defined by fondo 0 and shared, so the posts of every fondo align — only the vertical (levels)
        /// varies here. A fondo's bay with no levels is an empty frente (e.g. a building column). Frente count follows
        /// fondo 0. Empty = every fondo shares fondo 0's matrix (the plain doble-profundidad case).
        /// </summary>
        public IList<IList<SelectiveBayDesign>> ExtraFondoBays { get; } = new List<IList<SelectiveBayDesign>>();

        /// <summary>
        /// Optional per-post "cabecera" (frame), one entry per post position (N frentes → N+1 posts). A null
        /// entry (or a short list) means that post uses the run defaults. The frontal draw uses each cabecera's
        /// base plate (id + peralte); lateral/planta render the full cabecera. In the frontal a post is this cabecera
        /// seen edge-on.
        /// </summary>
        public IList<RackFrameConfiguration> PostCabeceras { get; } = new List<RackFrameConfiguration>();

        /// <summary>
        /// Per-post custom cabeceras of the fondos AFTER fondo 0 (I-43): entry <c>k-1</c> is fondo <c>k</c>, and inside
        /// it one entry per post of THAT fondo. A null entry, a short row or a missing row all mean "standard cabecera",
        /// so a design written before this existed leaves every extra fondo standard — which is exactly what those
        /// drawings showed, since a custom cabecera only ever applied to fondo 0.
        /// <para>
        /// <see cref="PostCabeceras"/> stays fondo 0's row, so the legacy shape keeps its legacy meaning and the frontal
        /// keeps reading the master fondo. Read the pair through the single authority
        /// (<c>SelectiveCabeceraAuthority</c>) instead of indexing either list, so nothing re-derives the fallback.
        /// </para>
        /// </summary>
        public IList<IList<RackFrameConfiguration>> ExtraFondoPostCabeceras { get; } = new List<IList<RackFrameConfiguration>>();

        /// <summary>
        /// Optional per-post PERALTE override, one entry per post position (N frentes → N+1 posts). An entry
        /// &lt;= 0 (or a short list) means that post inherits <see cref="PostPeralte"/>. Lets each post carry its
        /// own peralte in the frontal/planta; the larguero spacing adapts to each post's troquel.
        /// </summary>
        public IList<double> PostPeraltes { get; } = new List<double>();

        // ---- Annotation / drawing toggles ----

        /// <summary>Draw the base plates. Default true; turning it off omits the plate blocks in frontal/planta.</summary>
        public bool DrawBasePlate { get; set; } = true;

        /// <summary>Number the frentes in the generated annotations.</summary>
        public bool NumberFronts { get; set; }

        /// <summary>Number the load levels in frontal/lateral annotations.</summary>
        public bool NumberLevels { get; set; }

        /// <summary>Draw the rack name as visible text in the generated views.</summary>
        public bool DrawRackName { get; set; }

        /// <summary>Draw the pallets (tarimas) as a VISUAL reference on the load levels (and the floor). Default off;
        /// the block is the catalog "TARIMA" piece and never enters the BOM.</summary>
        public bool DrawPallets { get; set; }

        /// <summary>Multiplier on the annotation text height (1 = default 6"). Scales the frente/level/name labels AND the dimensions.</summary>
        public double AnnotationScale { get; set; } = 1.0;

        /// <summary>How much automatic dimensioning to draw per view (None = off). Scaled by <see cref="AnnotationScale"/>.</summary>
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;

        /// <summary>Name of the AutoCAD dimension style to use for the cotas; null/empty = automatic (the drawing's
        /// current style, sized to <see cref="AnnotationScale"/>). A chosen style is respected as-is.</summary>
        public string DimensionStyle { get; set; }

        /// <summary>Safety accessories chosen for this rack. Implemented families drive their view blocks and BOM;
        /// unknown/future families retain a manual BOM quantity until their placement rule exists.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();
    }

    /// <summary>Which side(s) of a post a drawable safety accessory (e.g. a bota) sits on. None = not drawn.</summary>
    public enum SafetySide
    {
        None = 0,
        Left = 1,
        Right = 2,
        Both = 3
    }

    /// <summary>One safety accessory chosen for a rack: its catalog id, a manual quantity (BOM fallback for elements
    /// with no drawing rule yet), and — for a DRAWABLE element (bota) — the <see cref="Side"/> it sits on at each post,
    /// with optional <see cref="PostSides"/> exceptions for specific posts.</summary>
    public sealed class SelectiveSafetySelection
    {
        public string ElementId { get; set; }
        public int Quantity { get; set; }

        /// <summary>Default side for a drawable element, applied to every post unless overridden in <see cref="PostSides"/>.</summary>
        public SafetySide Side { get; set; } = SafetySide.Both;

        /// <summary>
        /// PB-009 (I-32) — this selection only exists at the LOW (entrance/exit) end of the rack, so the ADAPTIVE
        /// defaults must never place a piece at the far end. False (Selectivo, Dinámico) is the historical behaviour.
        ///
        /// It is DERIVED, not persisted: the Push Back authority re-imposes it at every boundary it owns, so a
        /// document can never carry a stale value and no DTO changes.
        /// </summary>
        public bool LowEndOnly { get; set; }

        /// <summary>
        /// I-42 (S1) — el lado que el USUARIO eligio, antes de que la restriccion de extremo de un sistema lo
        /// colapse. Es DERIVADO y no se persiste, como <see cref="LowEndOnly"/>: lo rellena la autoridad del
        /// sistema en el mismo sitio donde impone su restriccion, y NULL —todo sistema que no restrinja nada—
        /// significa «el de <see cref="Side"/>», que es lo que se leia siempre.
        ///
        /// <para>
        /// Existe porque el colapso destruye informacion que UNA familia si necesita: el PROTECTOR LATERAL lee
        /// Izquierda/Derecha como ORIENTACION en su sitio (contrato validado en I-32) y le basta el lado colapsado,
        /// pero la BOTA lo lee como UBICACION FISICA —que cara de ataque proteger— y con el colapso las tres
        /// opciones daban exactamente lo mismo.
        /// </para>
        /// </summary>
        public SafetySide? AuthoredSide { get; set; }

        /// <summary>
        /// I-42 (S1D, contrato del dueño) — LA COLOCACION EFECTIVA de la bota en un poste, con la precedencia final:
        ///
        /// <list type="number">
        /// <item>la decision PROPIA del poste, que gana siempre;</item>
        /// <item>el BLANCO, que retira la cara de su lado de lo que ese poste HEREDE;</item>
        /// <item>lo heredado: la general si alguien la eligio, y si no el automatico del sistema.</item>
        /// </list>
        ///
        /// <para>
        /// El orden importa y es el contrato: ni el blanco ni la general pueden bloquear una decision explicita
        /// —el poste fisico existe y puede necesitar proteccion—, y la general es un DEFECTO, nunca un interruptor
        /// de la familia. El fallback final es el lado historico: <c>Izquierda</c> es la cara de entrada/salida y
        /// <c>Derecha</c> la posterior, que es exactamente lo que esas etiquetas querian decir.
        /// </para>
        /// </summary>
        public BootPlacement BootPlacementAt(int postIndex) => PlacementAt(Bota, postIndex);

        /// <summary>La colocacion efectiva del LADO B en ese poste. Un rack de un solo lado no la usa.</summary>
        public BootPlacement BootPlacementAtSideB(int postIndex) => PlacementAt(BotaB, postIndex);

        /// <summary>
        /// I-42 (S1G) — EL TIPO DE PIEZA que un lado materializa: el que ESE lado eligio o, si nunca se eligio ahi,
        /// el del documento —<see cref="ElementId"/>—, que es el unico que existia antes de esta ronda. Devuelve el
        /// id de «ninguno» tal cual: quien materializa decide que hacer con el, y la intencion sigue guardada.
        /// </summary>
        public string BootPieceOf(SelectiveBotaConfig config)
            => config == null || string.IsNullOrWhiteSpace(config.PieceId) ? ElementId : config.PieceId;

        /// <summary>
        /// I-42 (S1E) — LAS CARAS FISICAS que hay que proteger en ese poste: la union de lo que piden los dos
        /// lados, DEDUPLICADA.
        ///
        /// <para>
        /// Es el punto donde el lado deja de importar y empieza la geometria. «Entrada/Salida» se lee dentro del
        /// lado: la de A es la cara CERCANA del rack y la de B la LEJANA, y sus posteriores son las contrarias. Dos
        /// intenciones distintas pueden nombrar la misma cara fisica —la posterior de A y la entrada de B— y eso es
        /// UNA pieza, no dos.
        /// </para>
        /// <para>
        /// Esta es la UNICA resolucion de pertenencia: la planta, los cortes y el BOM la consumen, ninguno la
        /// vuelve a calcular en su propio marco.
        /// </para>
        /// </summary>
        public BootFaces BootFacesAt(int postIndex)
        {
            var a = PlacementAt(Bota, postIndex);
            var b = PlacementAt(BotaB, postIndex);
            return new BootFaces(
                near: BootPlacements.IncludesEntryExit(a) || BootPlacements.IncludesRear(b),
                far: BootPlacements.IncludesRear(a) || BootPlacements.IncludesEntryExit(b));
        }

        /// <summary>
        /// La resolucion de UN lado, con la precedencia del contrato: decision propia del poste, blanco de ese
        /// lado, general de ese lado. Lo unico que se añade aqui es lo que la configuracion de un lado no puede
        /// saber por si sola: la MATRIZ HISTORICA por poste y la intencion global de un documento anterior.
        /// </summary>
        private BootPlacement PlacementAt(SelectiveBotaConfig config, int postIndex)
        {
            var sideA = ReferenceEquals(config, Bota);
            if (config == null)
            {
                return BootPlacement.None;
            }

            if (config.HasOwnAt(postIndex))
            {
                return config.At(postIndex).Value;
            }

            if (sideA)
            {
                // Legacy: un poste con entrada propia en la matriz historica es tan EXPLICITO como el de arriba
                // —nadie lo colapsa, igual que en ChosenSide—, y esa matriz es del lado unico/A.
                foreach (var over in PostSides)
                {
                    if (over != null && over.PostIndex == postIndex)
                    {
                        return BootPlacements.From(over.Side);
                    }
                }
            }

            if (config.IsBlankAt(postIndex))
            {
                return BootPlacement.None;
            }

            if (config.Placement.HasValue)
            {
                return config.Placement.Value;
            }

            return sideA ? InheritedSideA(config) : InheritedSideB(config);
        }

        /// <summary>
        /// Lo que hereda el lado A (o el unico lado): el automatico del sistema, salvo que el documento traiga una
        /// intencion GLOBAL anterior a S1E —el lado historico—, que es una decision del usuario y manda. El unico
        /// valor que significa «no lo he tocado» es el que siembra un rack nuevo, «Ambas».
        /// </summary>
        private BootPlacement InheritedSideA(SelectiveBotaConfig config)
        {
            if (BootSidesDeclared)
            {
                return config.Automatic ?? BootPlacement.None;
            }

            var legacy = AuthoredSide ?? Side;
            return legacy == SafetySide.Both
                ? config.Automatic ?? BootPlacements.From(legacy)
                : BootPlacements.From(legacy);
        }

        /// <summary>
        /// Lo que hereda el lado B: su automatico. Con una intencion GLOBAL anterior a S1E el lado B no hereda
        /// nada —esa intencion ya se resolvio entera sobre el lado A, que es como se dibujaba—, asi que un
        /// documento antiguo produce exactamente las mismas piezas que producia.
        /// </summary>
        private BootPlacement InheritedSideB(SelectiveBotaConfig config)
            => !BootSidesDeclared && HasGlobalLegacyIntent
                ? BootPlacement.None
                : config.Automatic ?? BootPlacement.None;

        /// <summary>True cuando el documento trae la unica configuracion global que existia antes de S1E.</summary>
        private bool HasGlobalLegacyIntent
            => Bota.Placement.HasValue || (AuthoredSide ?? Side) != SafetySide.Both;



        /// <summary>
        /// I-42 (S1E, contrato del dueño) — LA CONFIGURACION DE BOTAS DEL LADO B de un rack compuesto.
        ///
        /// <para>
        /// Un Push Back compuesto tiene DOS lados fisicos, y cada uno tiene su propia intencion: su general y sus
        /// postes. <see cref="Bota"/> es la del lado A —y la unica de un rack de un solo sentido, que es como se
        /// guardo siempre—; esta es la de B. VACIA —todo documento anterior y todo sistema de un solo lado—
        /// significa «B no pide nada», y entonces el rack dibuja exactamente lo que dibujaba.
        /// </para>
        /// <para>
        /// LADO y CARA son ejes distintos: «Entrada/Salida» se lee DENTRO del lado —la de A es la cara cercana del
        /// rack y la de B la lejana—, asi que ninguna vista tiene que reinterpretar la eleccion en su marco. La
        /// conversion a caras fisicas se hace UNA vez, en <see cref="BootFacesAt"/>.
        /// </para>
        /// </summary>
        public SelectiveBotaConfig BotaB { get; set; } = new SelectiveBotaConfig();

        /// <summary>
        /// I-42 (S1E) — true cuando la intencion de botas de este documento esta declarada POR LADO.
        ///
        /// <para>
        /// Es el discriminante entre las dos eras, y hace falta porque significan cosas distintas: en un documento
        /// ANTERIOR hay una sola configuracion, que era del rack entero y se resolvia como la del lado A —el lado B
        /// no pedia nada—; en uno de S1E cada lado tiene la suya, y una general vacia significa «este lado hereda su
        /// automatico», no «este lado no existe». Sin el, abrir un documento antiguo cambiaria lo que dibuja.
        /// </para>
        /// </summary>
        public bool BootSidesDeclared { get; set; }

        /// <summary>
        /// I-42 (S1D, contrato del dueño) — True cuando ALGO de esta seleccion pide una pieza: el lado general, la
        /// matriz historica por poste o la decision de bota de algun poste.
        ///
        /// <para>
        /// El selector general es un DEFECTO, no un interruptor de la familia: con la general en «Ninguno» un poste
        /// con eleccion propia sigue llevando su bota, y es esta pregunta la que lo mantiene vivo hasta que la
        /// resolucion por poste decide. Con nadie pidiendo nada la familia no llega al dibujo, como siempre.
        /// </para>
        /// </summary>
        public bool DrawsSomewhere()
        {
            if (Side != SafetySide.None)
            {
                return true;
            }

            foreach (var over in PostSides)
            {
                if (over != null && over.Side != SafetySide.None)
                {
                    return true;
                }
            }

            return Bota.AsksForSomething() || BotaB.AsksForSomething();
        }

        /// <summary>
        /// True cuando ESE poste tiene una decision propia de bota en CUALQUIERA de los dos lados (o en la matriz
        /// historica). Es lo que permite que una eleccion del usuario se coloque aunque el automatico no la pusiera.
        /// </summary>
        public bool HasOwnBootPlacement(int postIndex)
        {
            if (Bota.HasOwnAt(postIndex) || BotaB.HasOwnAt(postIndex))
            {
                return true;
            }

            foreach (var over in PostSides)
            {
                if (over != null && over.PostIndex == postIndex)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// I-42 (A1/H8) — las entradas de <see cref="PostSides"/> que escribio una regla DERIVADA —los pasillos de
        /// carga que el rack tiene ahora—, con el valor que el usuario tenia ahi antes.
        ///
        /// <para>
        /// `PostSides` guarda intencion del usuario Y la lee el dibujo, asi que una regla derivada que escribe ahi
        /// deja rastro persistido: al degradar un compuesto a un solo sentido quedaba un «Derecha» rancio que
        /// mandaba el desviador al extremo alto. Esta lista es DERIVADA y no se persiste; la persistencia la usa
        /// para guardar lo que el usuario habia decidido.
        /// </para>
        /// </summary>
        public IList<DerivedAisleEntry> DerivedAisles { get; } = new List<DerivedAisleEntry>();

        /// <summary>El lado que el usuario eligio, o el vigente si el sistema no lo restringio.</summary>
        public SafetySide ChosenSide(int postIndex)
        {
            foreach (var over in PostSides)
            {
                if (over != null && over.PostIndex == postIndex)
                {
                    return over.Side;   // una entrada POR POSTE es siempre del usuario: nadie la colapsa
                }
            }

            return AuthoredSide ?? Side;
        }

        /// <summary>
        /// I-42 (ronda 7E) — la pieza de la cara CERCANA de cada linea, cuando el sistema declara una por extremo.
        /// NULL —el valor de todo sistema que no la rellene y de todo documento anterior— significa «la de
        /// <see cref="ElementId"/>», que es el comportamiento historico.
        /// </summary>
        public SafetyFacePiece NearFace { get; set; }

        /// <summary>La pieza de la cara LEJANA. Ver <see cref="NearFace"/>.</summary>
        public SafetyFacePiece FarFace { get; set; }

        /// <summary>El id que materializa una cara, o NULL cuando esa cara no lleva ninguna pieza.</summary>
        public string ElementIdForFace(bool farEnd)
            => SafetyFacePiece.Resolve(farEnd ? FarFace : NearFace, ElementId);

        /// <summary>
        /// I-42 — el sistema tiene cara de carga en los DOS extremos longitudinales (un Push Back compuesto son dos
        /// Push Back opuestos). False es el comportamiento historico de todos los sistemas.
        ///
        /// <para>
        /// Es un eje PROPIO, y tiene que serlo. Expresar «dos pasillos» escribiendo <see cref="Side"/> destruiria la
        /// PERTENENCIA: las reglas adaptativas —la del protector lateral, por ejemplo— solo se aplican cuando el
        /// usuario no ha elegido lado, asi que fijar el lado las apaga y la pieza aparece en TODOS los postes, dos
        /// veces. Pertenencia, orientacion y extremo son tres cosas distintas y ninguna puede usarse para decir otra.
        /// </para>
        /// <para>
        /// Como <see cref="LowEndOnly"/>, es DERIVADO y no se persiste: la autoridad de Push Back lo vuelve a imponer
        /// en cada limite que posee, asi que ningun documento puede traerlo rancio y ningun DTO cambia.
        /// </para>
        /// </summary>
        public bool BothEndsAreLoadFaces { get; set; }

        /// <summary>
        /// I-42 (ronda post-5a73b92) — las LINEAS de postes que de verdad tienen esa segunda cara de carga.
        ///
        /// <para>
        /// VACIA significa «todas», que es el comportamiento anterior y el de cualquier sistema que no la rellene.
        /// Existe porque un rack compuesto PARCIAL tiene la segunda cara solo donde hay lado B: convertir todas las
        /// lineas del rack en caras de carga porque UN frente sea compuesto ponia botas y protectores en frentes que
        /// siguen siendo de un solo sentido. Es DERIVADA y no se persiste, igual que
        /// <see cref="BothEndsAreLoadFaces"/> y que <c>LowEndOnly</c>.
        /// </para>
        /// </summary>
        public IList<int> SecondLoadFacePosts { get; } = new List<int>();

        /// <summary>
        /// Si la LINEA indicada tiene la segunda cara de carga. Es la pregunta que hacen las reglas de copias: la
        /// pertenencia sigue siendo la del usuario o la adaptativa, y esto solo decide cuantas CARAS materializa.
        /// </summary>
        public bool HasSecondLoadFaceAt(int postIndex)
            => BothEndsAreLoadFaces
               && (SecondLoadFacePosts.Count == 0 || SecondLoadFacePosts.Contains(postIndex));

        /// <summary>
        /// PB-002 (I-32) — the DESVIADOR off-cells of this selection are keyed by <b>POST</b>, not by front.
        ///
        /// A rack of N fronts has N+1 posts and the desviador grid has one column per POST, so a post index cannot be
        /// used as a front index. The dynamic system historically collapsed the two with a <c>Math.Min</c> onto the
        /// last front, which silently merged the last two columns; false keeps exactly that reading.
        ///
        /// Like <see cref="LowEndOnly"/> it is DERIVED, not persisted: the Push Back authority re-imposes it at every
        /// boundary it owns, so no stored value can go stale and no DTO changes.
        /// </summary>
        public bool DesviadorCellsAreByPost { get; set; }

        /// <summary>Per-post overrides (post index → side); a post not listed uses <see cref="Side"/>.</summary>
        public IList<SafetyPostSide> PostSides { get; } = new List<SafetyPostSide>();

        /// <summary>The side for post <paramref name="postIndex"/>: its override if present, else the default <see cref="Side"/>.</summary>
        public SafetySide SideForPost(int postIndex)
        {
            foreach (var over in PostSides)
            {
                if (over != null && over.PostIndex == postIndex) return over.Side;
            }

            return Side;
        }

        // ---- Per-family safety configuration (I-22, E7): each family owns a sealed config with its OWN DeepCopy
        // and persistence mapping; the flat accessors below delegate to it, so existing consumers stay unchanged. ----

        private SelectiveTopeConfig tope = new SelectiveTopeConfig();
        private SelectiveDesviadorConfig desviador = new SelectiveDesviadorConfig();
        private SelectiveDefensaConfig defensa = new SelectiveDefensaConfig();
        private SelectiveGuiaConfig guia = new SelectiveGuiaConfig();
        private SelectiveParrillaConfig parrilla = new SelectiveParrillaConfig();
        private SelectiveBotaConfig bota = new SelectiveBotaConfig();

        /// <summary>TOPE (larguero tope) configuration. Never null.</summary>
        public SelectiveTopeConfig Tope { get => tope; set => tope = value ?? new SelectiveTopeConfig(); }

        /// <summary>DESVIADOR configuration. Never null.</summary>
        public SelectiveDesviadorConfig Desviador { get => desviador; set => desviador = value ?? new SelectiveDesviadorConfig(); }

        /// <summary>DEFENSA configuration. Never null.</summary>
        public SelectiveDefensaConfig Defensa { get => defensa; set => defensa = value ?? new SelectiveDefensaConfig(); }

        /// <summary>GUIA (entrance guide) configuration. Never null.</summary>
        public SelectiveGuiaConfig Guia { get => guia; set => guia = value ?? new SelectiveGuiaConfig(); }

        /// <summary>PARRILLA (deck) configuration. Never null.</summary>
        public SelectiveParrillaConfig Parrilla { get => parrilla; set => parrilla = value ?? new SelectiveParrillaConfig(); }

        /// <summary>I-42 (S1B) — la configuracion de la familia BOTA: su colocacion general y sus overrides por poste.</summary>
        public SelectiveBotaConfig Bota { get => bota; set => bota = value ?? new SelectiveBotaConfig(); }

        // ---- Flat accessors delegating to the per-family configs (compatibility surface for existing consumers) ----

        /// <summary>TOPE: shared central tope vs one per fondo. Delegates to <see cref="Tope"/>.</summary>
        public bool TopeShared { get => Tope.Shared; set => Tope.Shared = value; }

        /// <summary>TOPE: which fondo carries the tope; &lt; 0 = automatic central. Delegates to <see cref="Tope"/>.</summary>
        public int TopeFondo { get => Tope.Fondo; set => Tope.Fondo = value; }

        /// <summary>TOPE: the block SAQUE (stick-out), inches. Delegates to <see cref="Tope"/>.</summary>
        public double TopeSaque { get => Tope.Saque; set => Tope.Saque = value; }

        /// <summary>TOPE: also draw it in the FRONTAL view. Delegates to <see cref="Tope"/>.</summary>
        public bool TopeFrontal { get => Tope.Frontal; set => Tope.Frontal = value; }

        /// <summary>TOPE: the (frente, level) cells with NO tope. Delegates to <see cref="Tope"/>.</summary>
        public IList<SelectiveGridCell> TopeOffCells => Tope.OffCells;

        /// <summary>TOPE: true if a tope is drawn at (frente, level). Delegates to <see cref="Tope"/>.</summary>
        public bool TopeAt(int frente, int level) => Tope.At(frente, level);

        /// <summary>DESVIADOR: dynamic LONGITUD (in). Delegates to <see cref="Desviador"/>.</summary>
        public double DesviadorLongitud { get => Desviador.Longitud; set => Desviador.Longitud = value; }

        /// <summary>DESVIADOR: first load-level height above the first TROQUEL_LARGUERO (in). Delegates to <see cref="Desviador"/>.</summary>
        public double DesviadorPrimerNivelAltura { get => Desviador.PrimerNivelAltura; set => Desviador.PrimerNivelAltura = value; }

        /// <summary>DESVIADOR: disabled (post, load-level) cells. Delegates to <see cref="Desviador"/>.</summary>
        public IList<SelectiveGridCell> DesviadorOffCells => Desviador.OffCells;

        public bool DesviadorAt(int post, int level) => Desviador.At(post, level);

        /// <summary>DEFENSA: explicit per-post lengths. Delegates to <see cref="Defensa"/>.</summary>
        public IList<SafetyPostDefense> DefensaPosts => Defensa.Posts;

        /// <summary>GUIA: zero-based frente/level cells without entrance guides. Delegates to <see cref="Guia"/>.</summary>
        public IList<SelectiveGridCell> GuiaEntradaOffCells => Guia.OffCells;

        public bool GuiaEntradaAt(int frontIndex, int levelIndex) => Guia.At(frontIndex, levelIndex);

        /// <summary>PARRILLA: draw the deck in the FRONTAL view. Delegates to <see cref="Parrilla"/>.</summary>
        public bool ParrillaFrontal { get => Parrilla.Frontal; set => Parrilla.Frontal = value; }

        /// <summary>PARRILLA: draw the deck in the LATERAL view. Delegates to <see cref="Parrilla"/>.</summary>
        public bool ParrillaLateral { get => Parrilla.Lateral; set => Parrilla.Lateral = value; }

        /// <summary>PARRILLA: manual deck width (FRENTE, inches). Delegates to <see cref="Parrilla"/>.</summary>
        public double ParrillaFrente { get => Parrilla.Frente; set => Parrilla.Frente = value; }

        /// <summary>PARRILLA: manual deck count per load row. Delegates to <see cref="Parrilla"/>.</summary>
        public int ParrillaCantidad { get => Parrilla.Cantidad; set => Parrilla.Cantidad = value; }

        /// <summary>PARRILLA: the (frente, level) cells with NO deck. Delegates to <see cref="Parrilla"/>.</summary>
        public IList<SelectiveGridCell> ParrillaOffCells => Parrilla.OffCells;

        /// <summary>PARRILLA: true if a deck sits at (frente, level). Delegates to <see cref="Parrilla"/>.</summary>
        public bool ParrillaAt(int frente, int level) => Parrilla.At(frente, level);

        /// <summary>
        /// Deep working copy used when a selection crosses the design/resolver/view/UI boundaries. Delegates to each
        /// per-family config's own DeepCopy, so a new family adds its config's clone rather than another field here
        /// (I-22, E7). Persistence remains an explicit per-family DTO mapping so legacy fallbacks stay visible and tested.
        /// </summary>
        public SelectiveSafetySelection DeepCopy()
        {
            var copy = new SelectiveSafetySelection
            {
                ElementId = ElementId,
                Quantity = Quantity,
                Side = Side,
                LowEndOnly = LowEndOnly,
                AuthoredSide = AuthoredSide,
                BothEndsAreLoadFaces = BothEndsAreLoadFaces,
                NearFace = NearFace,
                FarFace = FarFace,
                DesviadorCellsAreByPost = DesviadorCellsAreByPost,
                BotaB = BotaB.DeepCopy(),
                BootSidesDeclared = BootSidesDeclared,
                Tope = Tope.DeepCopy(),
                Desviador = Desviador.DeepCopy(),
                Defensa = Defensa.DeepCopy(),
                Guia = Guia.DeepCopy(),
                Parrilla = Parrilla.DeepCopy(),
                Bota = Bota.DeepCopy()
            };

            foreach (var post in SecondLoadFacePosts)
            {
                copy.SecondLoadFacePosts.Add(post);
            }

            foreach (var post in PostSides)
            {
                if (post != null)
                {
                    copy.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
                }
            }

            foreach (var derived in DerivedAisles)
            {
                if (derived != null)
                {
                    copy.DerivedAisles.Add(new DerivedAisleEntry
                    {
                        PostIndex = derived.PostIndex,
                        Authored = derived.Authored,
                    });
                }
            }

            return copy;
        }
    }

    /// <summary>
    /// I-42 (A1/H8) — una entrada por poste que escribio una regla DERIVADA, con el valor AUTORADO que sustituyo
    /// (NULL = ahi no habia ninguna entrada del usuario).
    /// </summary>
    public sealed class DerivedAisleEntry
    {
        public int PostIndex { get; set; }

        public SafetySide? Authored { get; set; }
    }

    /// <summary>A per-post side override for a safety selection.</summary>
    public sealed class SafetyPostSide
    {
        public int PostIndex { get; set; }
        public SafetySide Side { get; set; }
    }

    /// <summary>An explicit dynamic forklift-defense length at one transverse post; zero means disabled.</summary>
    public sealed class SafetyPostDefense
    {
        public int PostIndex { get; set; }
        public double ExitLength { get; set; }
        public double EntranceLength { get; set; }

        /// <summary>
        /// PB-010 (I-32) — this end follows the AUTOMATIC rule (12" on an edge post, 36" on an intermediate one)
        /// instead of the stored length, so it is recomputed when the rack gains or loses fronts and a post that was
        /// an edge becomes an intermediate. FALSE is the historical meaning of a stored record — an explicit override,
        /// which is exactly what every document written before this field carried — so legacy data keeps its lengths.
        /// </summary>
        public bool ExitAuto { get; set; }

        /// <summary>PB-010 — same rule for the other end, decided independently.</summary>
        public bool EntranceAuto { get; set; }
    }

    /// <summary>A (frente, level) cell reference — used to mark which larguero cells carry (or skip) a tope.</summary>
    public sealed class SelectiveGridCell
    {
        public int Frente { get; set; }
        public int Level { get; set; }
    }

    /// <summary>One bay's column in the design matrix: its level cells (its own count), bottom to top.</summary>
    public sealed class SelectiveBayDesign
    {
        /// <summary>
        /// Whether the ground level (level 0) carries a larguero ("larguero a piso"). Default false: the ground
        /// pallet rests on the floor (from Y=0) with no beam, and the first larguero sits above it. When true,
        /// the ground level gets a beam at the lowest troquel and the pallet stacks from there.
        /// </summary>
        public bool FloorBeam { get; set; }

        /// <summary>Manual override for this bay's height (in). Null = auto. A post still takes the tallest of the bays it touches.</summary>
        public double? HeightOverride { get; set; }

        /// <summary>
        /// "Medio frente" generalizado: partition this bay into N tramos with N-1 INTERMEDIATE posts (of this fondo
        /// only, so the fondos stay aligned at the shared end posts). Each tramo has a larguero length and a loaded
        /// flag; the LAST tramo's length is CALCULATED (the remainder of the bay). Fewer than 2 tramos = a normal
        /// full-width bay. Lengths are free measures, NOT tied to a pallet count — a triple/quad frente can store
        /// fewer pallets. Marking which tramos carry largueros lets you tie one side, the other, or both. Per fondo.
        /// </summary>
        public IList<SelectiveSegment> Segments { get; } = new List<SelectiveSegment>();

        /// <summary>The level cells of this bay, bottom to top. Each cell can differ (pallet, count, beam).</summary>
        public IList<SelectiveCell> Levels { get; } = new List<SelectiveCell>();
    }

    /// <summary>
    /// One "tramo" of a split frente ("medio frente" generalizado). A larguero of length <see cref="Length"/> that
    /// either carries largueros (<see cref="Loaded"/>) or stays empty. A bay's tramos are separated by intermediate
    /// posts; the LAST tramo's length is CALCULATED (the remainder), so its <see cref="Length"/> is ignored.
    /// </summary>
    public sealed class SelectiveSegment
    {
        /// <summary>Larguero length (in) of this tramo. Ignored for the last tramo (calculated from the remainder).</summary>
        public double Length { get; set; }

        /// <summary>Whether this tramo carries largueros (a load position) or stays empty. Lets you tie one side, the other, or both.</summary>
        public bool Loaded { get; set; } = true;
    }

    /// <summary>One cell of the matrix (a level of a bay): the pallet stored there and its beam.</summary>
    public sealed class SelectiveCell
    {
        /// <summary>The pallet type at this cell (frente drives the beam length, alto the separation above it).</summary>
        public Tarima Pallet { get; set; } = new Tarima();

        /// <summary>How many pallets sit side by side at this level ("tarimas por nivel").</summary>
        public int PalletCount { get; set; } = 1;

        /// <summary>Catalog id of the beam (larguero) at this level.</summary>
        public string BeamId { get; set; }

        /// <summary>Beam peralte (block parameter) at this level.</summary>
        public double BeamPeralte { get; set; }

        /// <summary>Manual override for the larguero LONGITUD at this level (in). Null = auto (Frente*Count + tolerance). The bay uses the longest level.</summary>
        public double? BeamLengthOverride { get; set; }

        /// <summary>Manual override for the clear/separation BELOW this level's beam (in), snapped up to the troquel grid. Null = auto.</summary>
        public double? ClearOverride { get; set; }
    }

    /// <summary>A pallet ("tarima"). Frontal needs its front and height; depth (fondo) comes with the lateral view.</summary>
    public sealed class Tarima
    {
        /// <summary>Front width of the pallet (in), measured along the beam.</summary>
        public double Frente { get; set; }

        /// <summary>Height of the pallet + load (in); drives the clear opening to the level above.</summary>
        public double Alto { get; set; }
    }
}
