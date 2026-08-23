namespace RackCad.Domain.Systems.PushBack
{
    /// <summary>
    /// I-42 — uno de los dos LADOS (sentidos de carga) de un Push Back compuesto. No son dos racks: son las dos
    /// mitades funcionales de UNA sola estructura fisica, enfrentadas por sus extremos de fondo.
    /// <para>
    /// <see cref="A"/> es el lado de REFERENCIA: su extremo exterior es el origen de la profundidad y su flujo avanza
    /// hacia +X. Es tambien el unico lado que existe en un rack anterior a I-42, y por eso un documento legacy carga
    /// como «solo lado A» sin pedirle nada al usuario.
    /// </para>
    /// <para>
    /// <see cref="B"/> es el lado OPUESTO: su extremo exterior es el final de la profundidad y su flujo avanza hacia
    /// -X. Fisicamente es la imagen especular de un lado A respecto del plano medio del rack — las mismas piezas, la
    /// mano contraria—, no una decoracion espejada sobre una cama dibujada horizontal.
    /// </para>
    /// </summary>
    public enum PushBackSide
    {
        A = 0,
        B = 1
    }

    /// <summary>
    /// I-42 — la TOPOLOGIA de una celda compuesta (ranura transversal x nivel). No es global ni por frente: dos
    /// niveles del MISMO frente pueden tener topologias distintas.
    /// </summary>
    public enum PushBackCellTopology
    {
        /// <summary>Una sola cama fisica, en el lado A. El lado B no almacena en esa celda.</summary>
        SoloA = 0,

        /// <summary>Una sola cama fisica, en el lado B. El lado A no almacena en esa celda.</summary>
        SoloB = 1,

        /// <summary>
        /// DOS camas fisicas independientes, A y B, con sentidos opuestos y sus extremos ALTOS enfrentados hacia el
        /// centro. Cada una admite su propio tope.
        /// </summary>
        Encontradas = 2,

        /// <summary>
        /// UNA sola cama fisica que atraviesa A + gap + B: una longitud, una pendiente continua, un eje, una cama en
        /// el BOM y como maximo UN tope, en su extremo ALTO. El sentido lo fija <see cref="PushBackRunDirection"/>.
        /// </summary>
        Corrida = 3
    }

    /// <summary>
    /// I-42 — el SENTIDO de una cama corrida. Cambiarlo cambia fisicamente que extremo es ALTO y cual BAJO (y por
    /// tanto donde va el tope y como se derivan las elevaciones); no es un espejo grafico.
    /// </summary>
    public enum PushBackRunDirection
    {
        /// <summary>BAJO en el exterior de A, ALTO en el exterior de B.</summary>
        AToB = 0,

        /// <summary>BAJO en el exterior de B, ALTO en el exterior de A.</summary>
        BToA = 1
    }
}
