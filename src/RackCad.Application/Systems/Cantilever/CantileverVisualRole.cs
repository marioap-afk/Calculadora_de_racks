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

        /// <summary>Flat plate: bolted or welded, and always secondary to what it joins.</summary>
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
        Annotation = 8
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

                case CantileverVisualRole.Gusset:
                    return LayerPrefix + "CARTABON";

                case CantileverVisualRole.Separator:
                    return LayerPrefix + "SEPARADOR";

                case CantileverVisualRole.Brace:
                    return LayerPrefix + "TENSOR";

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
                case CantileverVisualRole.Column:
                    return 3; // verde

                case CantileverVisualRole.Base:
                    return 5; // azul

                case CantileverVisualRole.Arm:
                    return 30; // naranja

                case CantileverVisualRole.Plate:
                    return 254; // gris claro: secundaria, y no debe competir con el acero que une

                case CantileverVisualRole.Gusset:
                    return 252; // gris medio

                case CantileverVisualRole.Separator:
                    return 6; // magenta

                case CantileverVisualRole.Brace:
                    return 2; // amarillo

                case CantileverVisualRole.Punch:
                    return 1; // rojo: lo que mas se busca en el plano de una columna troquelada

                case CantileverVisualRole.Annotation:
                    return 7; // blanco/negro, segun el fondo

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role), role,
                        "El rol visual '" + role + "' no tiene color declarado. Anadir uno exige elegirlo aqui.");
            }
        }
    }
}
