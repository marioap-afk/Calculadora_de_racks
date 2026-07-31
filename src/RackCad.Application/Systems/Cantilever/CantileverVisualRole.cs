using System;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// What a drawn curve IS, physically, for the purpose of telling it apart on paper.
    ///
    /// It is not a second name for <see cref="CantileverViewPieceKind"/>, which says which piece drew a curve.
    /// This says what NATURE that piece has, which is the question a consumer asks when it has to choose a
    /// colour, a layer or a lineweight. The two happen to line up today for most pieces and are free to stop:
    /// <see cref="Annotation"/> already has no kind at all, because a dimension is a nature the system will
    /// draw before it is a piece the resolver returns.
    ///
    /// It exists because the block came out of AutoCAD entirely uniform — every entity BYBLOCK on layer 0, so a
    /// column, a plate, a gusset and a hole read exactly alike, which is motivo 5 del rechazo de la ronda 2 de
    /// I-37D. Fixing that needed a vocabulary owned in ONE place: if the preview and the drawing each invented
    /// their own, the picture the user approves and the block they receive would drift apart the first time one
    /// of them gained a nature.
    /// </summary>
    public enum CantileverVisualRole
    {
        /// <summary>A standing structural member the whole assembly hangs off.</summary>
        Column = 0,

        /// <summary>A structural member lying on the floor.</summary>
        Base = 1,

        /// <summary>A structural member cantilevered off a column.</summary>
        Arm = 2,

        /// <summary>
        /// Placa de un BRAZO: la de montaje contra la columna y la de su extremo libre.
        ///
        /// Dejó de significar «cualquier placa que no sea del conjunto columna–base» en la ronda 3: la que une
        /// la columna con un separador salió de aquí a <see cref="ColumnSeparatorPlate"/> porque el dueño la
        /// quiere leer con la columna. Lo que queda es el conjunto del brazo, y por eso comparte color con él
        /// sin confundirse: el perfil es azul y sus placas moradas.
        /// </summary>
        Plate = 3,

        /// <summary>A stiffening triangle. Read apart from a plate because it is welded, not bolted.</summary>
        Gusset = 4,

        /// <summary>A member that ties two stations together longitudinally.</summary>
        Separator = 5,

        /// <summary>A diagonal in tension, and any fabricated end that adapts it.</summary>
        Brace = 6,

        /// <summary>A hole. Never a piece of steel — the ABSENCE of one — and that is why it reads apart.</summary>
        Punch = 7,

        /// <summary>
        /// Text, dimensions, marks: everything that describes the product without being part of it.
        ///
        /// Declared before anything emits it, on purpose. A consumer written today already has to handle it, so
        /// the day dimensions arrive they cannot land silently on the colour of whatever came last.
        /// </summary>
        Annotation = 8,

        /// <summary>
        /// Flat plate OF THE COLUMN–BASE subassembly: la inferior de la columna y las dos de la base.
        ///
        /// Aparte de <see cref="Plate"/> porque el dueño pidió que TODO el conjunto columna–base se lea junto
        /// —en rojo— y una placa de montaje de brazo no pertenece a ese conjunto. La distinción no se adivina
        /// mirando la curva: la decide el <see cref="CantileverPlateKind"/> que el modelo ya resolvió.
        /// </summary>
        ColumnBasePlate = 9,

        /// <summary>El ángulo de extremo de un tensor cold rolled, con sus dos alas.</summary>
        BraceAdapter = 10,

        /// <summary>Uno de los dos cartabones calibre 10 de ese adaptador.</summary>
        BraceGusset = 11,

        /// <summary>
        /// Un agujero DEL ARRIOSTRAMIENTO: el que bolta al separador y el que pasa la varilla.
        ///
        /// Aparte de <see cref="Punch"/> porque un troquel de columna y un agujero de adaptador se leen en
        /// planos distintos y se apagan por separado; comparten el blanco porque los dos son ausencia de
        /// acero.
        /// </summary>
        BracePunch = 12,

        /// <summary>
        /// La placa que une una columna con un separador.
        ///
        /// Aparte de <see cref="Plate"/> por decisión del dueño en la ronda 3: la quiere leer <b>del color de
        /// la columna</b>, porque es lo que le dice de un vistazo a qué columna pertenece un separador que
        /// cruza el dibujo entero. Y aparte de <see cref="ColumnBasePlate"/> porque no pertenece al conjunto
        /// columna–base: comparte su color, no su capa, así que se puede apagar sola.
        /// </summary>
        ColumnSeparatorPlate = 13
    }

    /// <summary>
    /// THE authority that says what nature a drawn piece has, and what to call its layer.
    ///
    /// Pure and total: every mapping is an explicit <c>switch</c> with a THROWING default. A default that
    /// returned some neutral role would let a new piece kind reach the drawing looking like something it is
    /// not, and «wrong but plausible» is the failure a reader cannot catch. Fail closed instead — the compiler
    /// will not force the case, so a test does.
    /// </summary>
    public static class CantileverVisualRoles
    {
        /// <summary>
        /// The prefix every Cantilever layer carries.
        ///
        /// Namespaced because the layers land in the USER's drawing, next to their own. A layer called
        /// «COLUMNA» would collide with whatever a customer already uses that name for and silently restyle it.
        /// </summary>
        public const string LayerPrefix = "RACKCAD_CANT_";

        /// <summary>The nature of a piece kind. Throws for a kind nobody has classified.</summary>
        public static CantileverVisualRole Of(CantileverViewPieceKind kind)
        {
            switch (kind)
            {
                case CantileverViewPieceKind.Column:
                    return CantileverVisualRole.Column;

                case CantileverViewPieceKind.Base:
                    return CantileverVisualRole.Base;

                case CantileverViewPieceKind.Arm:
                    return CantileverVisualRole.Arm;

                case CantileverViewPieceKind.Plate:
                    return CantileverVisualRole.Plate;

                case CantileverViewPieceKind.Gusset:
                    return CantileverVisualRole.Gusset;

                case CantileverViewPieceKind.Separator:
                    return CantileverVisualRole.Separator;

                // A cold-rolled adapter is part of the brace it ends: it is fabricated for that rod and has no
                // life of its own, so giving it a nature of its own would say otherwise on the drawing.
                case CantileverViewPieceKind.Brace:
                case CantileverViewPieceKind.ColdRolledAdapter:
                    return CantileverVisualRole.Brace;

                case CantileverViewPieceKind.Punch:
                    return CantileverVisualRole.Punch;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind), kind,
                        "La pieza de vista '" + kind + "' no tiene naturaleza visual declarada. " +
                        "Anadir una exige clasificarla aqui.");
            }
        }

        /// <summary>
        /// La naturaleza de una PLACA, que su tipo decide y su curva no.
        ///
        /// Una placa proyectada es un rectángulo y todas se parecen; a cuál conjunto pertenece sólo lo sabe el
        /// modelo que la resolvió. Preguntárselo a él es lo que permite que el subensamble columna–base se lea
        /// entero sin teñir de paso la placa de montaje de un brazo.
        /// </summary>
        public static CantileverVisualRole OfPlate(CantileverPlateKind kind)
        {
            switch (kind)
            {
                case CantileverPlateKind.ColumnBottom:
                case CantileverPlateKind.BaseFront:
                case CantileverPlateKind.BaseRear:
                    return CantileverVisualRole.ColumnBasePlate;

                case CantileverPlateKind.ArmMounting:
                case CantileverPlateKind.ArmEnd:
                    return CantileverVisualRole.Plate;

                // Salió de `Plate` en la ronda 3. No es una placa de brazo y el dueño la lee con la columna.
                case CantileverPlateKind.SeparatorColumn:
                    return CantileverVisualRole.ColumnSeparatorPlate;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind), kind,
                        "La placa '" + kind + "' no tiene naturaleza visual declarada. " +
                        "Anadir una exige clasificarla aqui.");
            }
        }

        /// <summary>The layer a role draws on. Throws for a role nobody has named.</summary>
        public static string LayerNameOf(CantileverVisualRole role)
        {
            switch (role)
            {
                case CantileverVisualRole.Column:
                    return LayerPrefix + "COLUMNA";

                case CantileverVisualRole.Base:
                    return LayerPrefix + "BASE";

                case CantileverVisualRole.Arm:
                    return LayerPrefix + "BRAZO";

                case CantileverVisualRole.Plate:
                    return LayerPrefix + "PLACA";

                case CantileverVisualRole.ColumnBasePlate:
                    return LayerPrefix + "PLACA_COLUMNA_BASE";

                case CantileverVisualRole.ColumnSeparatorPlate:
                    return LayerPrefix + "PLACA_COLUMNA_SEPARADOR";

                case CantileverVisualRole.Gusset:
                    return LayerPrefix + "CARTABON";

                case CantileverVisualRole.Separator:
                    return LayerPrefix + "SEPARADOR";

                case CantileverVisualRole.Brace:
                    return LayerPrefix + "TENSOR";

                case CantileverVisualRole.BraceAdapter:
                    return LayerPrefix + "TENSOR_ADAPTADOR";

                case CantileverVisualRole.BraceGusset:
                    return LayerPrefix + "TENSOR_CARTABON";

                case CantileverVisualRole.BracePunch:
                    return LayerPrefix + "TENSOR_TROQUEL";

                case CantileverVisualRole.Punch:
                    return LayerPrefix + "TROQUEL";

                case CantileverVisualRole.Annotation:
                    return LayerPrefix + "ANOTACION";

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role), role,
                        "El rol visual '" + role + "' no tiene capa declarada. Anadir uno exige nombrarla aqui.");
            }
        }

        /// <summary>
        /// The colour a role reads in, as an AutoCAD Color Index.
        ///
        /// ACI and not RGB because it is the Plugin that consumes it and a layer's colour is an ACI there; the
        /// preview converts. They are the SEVEN standard indices plus two greys, so the drawing looks the same
        /// on every AutoCAD, and they are chosen so that the two natures a reader most needs to separate — the
        /// steel and the holes in it — are furthest apart.
        ///
        /// It lives in Application and not in the Plugin because the preview must show the SAME colour: a user
        /// who approves a green column and receives a yellow one has been shown the wrong picture.
        /// </summary>
        public static short ColorIndexOf(CantileverVisualRole role)
        {
            switch (role)
            {
                // ---- La COLUMNA y todo lo que la acompaña, entero en ROJO -------------------------------
                // Decisión del dueño, ampliada en la ronda 3 con la placa que recibe un separador. Los cinco
                // roles siguen SEPARADOS —cada uno con su capa— así que se pueden apagar por separado; lo que
                // comparten es el color, que es lo que hace que el conjunto se lea como un conjunto.
                case CantileverVisualRole.Column:
                case CantileverVisualRole.Base:
                case CantileverVisualRole.ColumnBasePlate:
                case CantileverVisualRole.Gusset:
                case CantileverVisualRole.ColumnSeparatorPlate:
                    return 1; // rojo

                // ---- El BRAZO: el perfil y sus placas ----------------------------------------------------
                // Dos colores y no uno: el dueño pidió leer el perfil aparte de la ménsula que lo cuelga,
                // que es justo la pareja que un lector compara cuando revisa un nivel.
                case CantileverVisualRole.Arm:
                    return 5; // azul

                case CantileverVisualRole.Plate:
                    return 6; // morado

                case CantileverVisualRole.Separator:
                    return 30; // naranja

                // ---- El arriostramiento, ENTERO en cian --------------------------------------------------
                // El dueño revisó en la ronda 3 el reparto de la ronda anterior —cuerpo azul, adaptador cian,
                // cartabón magenta—: un tensor es UNA pieza compuesta y sus tres partes se leen juntas, igual
                // que la columna y su base. Las capas siguen separadas, así que apagar sólo los cartabones
                // sigue siendo posible; lo que se retira es el contraste DENTRO del conjunto.
                case CantileverVisualRole.Brace:
                case CantileverVisualRole.BraceAdapter:
                case CantileverVisualRole.BraceGusset:
                    return 4; // cian

                // Blanco por decisión del dueño, y es la elección física correcta: un troquel NO es acero, es
                // su ausencia, así que leerlo en el color opuesto al del acero es lo que un lector necesita.
                case CantileverVisualRole.Punch:
                case CantileverVisualRole.BracePunch:
                    return 7; // blanco

                // Comparte el blanco con el troquel, y no es un descuido: una cota tampoco es acero. Se
                // distinguen por capa, que es lo que permite apagar una sin la otra.
                case CantileverVisualRole.Annotation:
                    return 7; // blanco

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role), role,
                        "El rol visual '" + role + "' no tiene color declarado. Anadir uno exige elegirlo aqui.");
            }
        }
    }
}
