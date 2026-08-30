using System.Collections.Generic;

namespace RackCad.Domain.Systems.Selective
{
    /// <summary>
    /// I-42 (S1B, contrato del dueño) — DONDE VA UN PROTECTOR DE BOTA.
    ///
    /// <para>
    /// Una bota protege el POSTE de un impacto de montacargas, y eso NO depende de por donde se cargue el producto:
    /// la cara posterior puede necesitar proteccion aunque nunca se opere desde ahi —un rack que no esta contra muro
    /// tiene detras un pasillo de transito—. Por eso sus opciones son UBICACIONES FISICAS y se llaman por lo que
    /// son, no «izquierda» y «derecha».
    /// </para>
    ///
    /// <para>
    /// Es un tipo PROPIO de esta familia. <see cref="SafetySide"/> sigue significando lo que significaba para el
    /// PROTECTOR LATERAL —orientacion de su guia en su sitio— y para el DESVIADOR, y esta ronda no los toca. La
    /// correspondencia legacy es 1:1 y esta en <see cref="BootPlacements"/>.
    /// </para>
    /// </summary>
    public enum BootPlacement
    {
        /// <summary>Ninguna bota.</summary>
        None = 0,

        /// <summary>La cara de ENTRADA/SALIDA: el frente operativo.</summary>
        EntryExit = 1,

        /// <summary>La cara POSTERIOR. Puede necesitar proteccion aunque no se cargue por ella.</summary>
        Rear = 2,

        /// <summary>Las dos ubicaciones, una vez cada una.</summary>
        Both = 3,
    }

    /// <summary>
    /// La correspondencia entre <see cref="BootPlacement"/> y el <see cref="SafetySide"/> historico, que es como
    /// los documentos anteriores guardaron esta decision. Es 1:1 y por ordinal, asi que un documento antiguo se lee
    /// con su intencion intacta y uno nuevo sigue siendo legible por cualquier consumidor que aun mire el lado.
    /// </summary>
    public static class BootPlacements
    {
        /// <summary>Izquierda -> Entrada/Salida · Derecha -> Posterior · Ambas -> Ambas · Ninguno -> Ninguno.</summary>
        public static BootPlacement From(SafetySide side)
        {
            switch (side)
            {
                case SafetySide.Left: return BootPlacement.EntryExit;
                case SafetySide.Right: return BootPlacement.Rear;
                case SafetySide.Both: return BootPlacement.Both;
                default: return BootPlacement.None;
            }
        }

        /// <summary>La traduccion inversa, para los consumidores que aun guardan o leen un lado.</summary>
        public static SafetySide To(BootPlacement placement)
        {
            switch (placement)
            {
                case BootPlacement.EntryExit: return SafetySide.Left;
                case BootPlacement.Rear: return SafetySide.Right;
                case BootPlacement.Both: return SafetySide.Both;
                default: return SafetySide.None;
            }
        }

        /// <summary>Si esa colocacion incluye la cara de entrada/salida.</summary>
        public static bool IncludesEntryExit(BootPlacement placement)
            => placement == BootPlacement.EntryExit || placement == BootPlacement.Both;

        /// <summary>
        /// I-42 (S1D) — la colocacion que ocupa EXACTAMENTE las caras indicadas. Es la operacion inversa de
        /// <see cref="IncludesEntryExit"/>/<see cref="IncludesRear"/>, y la que permite quitar UNA cara sin tocar la
        /// otra: «Ambas» sin su cara de entrada es «Posterior», no «ninguna».
        /// </summary>
        public static BootPlacement Of(bool entryExit, bool rear)
            => entryExit
                ? (rear ? BootPlacement.Both : BootPlacement.EntryExit)
                : (rear ? BootPlacement.Rear : BootPlacement.None);

        /// <summary>Si esa colocacion incluye la cara posterior.</summary>
        public static bool IncludesRear(BootPlacement placement)
            => placement == BootPlacement.Rear || placement == BootPlacement.Both;
    }

    /// <summary>
    /// I-42 (S1E) — LAS DOS CARAS FISICAS de una linea de postes: la cercana y la lejana del rack.
    ///
    /// <para>
    /// Es el resultado de la resolucion, ya sin lados: dos intenciones distintas —la entrada/salida de B y la
    /// posterior de A, por ejemplo— nombran la MISMA cara fisica y producen UNA sola pieza. LADO y CARA son ejes
    /// distintos, y este tipo es el punto donde el primero deja de importar.
    /// </para>
    /// </summary>
    public readonly struct BootFaces
    {
        public BootFaces(bool near, bool far)
        {
            Near = near;
            Far = far;
        }

        /// <summary>La cara CERCANA del rack, que es el pasillo del lado A.</summary>
        public bool Near { get; }

        /// <summary>La cara LEJANA, que en un rack compuesto es el pasillo del lado B.</summary>
        public bool Far { get; }

        public bool Any => Near || Far;
    }

    /// <summary>La colocacion elegida para UN poste. Su ausencia en la lista significa «por defecto».</summary>
    public sealed class BootPostPlacement
    {
        public int PostIndex { get; set; }

        public BootPlacement Placement { get; set; }
    }

    /// <summary>
    /// I-42 (S1B) — la configuracion de la familia BOTA, con el mismo patron por familia que el resto (I-22, E7).
    ///
    /// <para>
    /// <see cref="Placement"/> NULO significa AUTOMATICO: la colocacion la resuelve el sistema segun las caras que
    /// tenga —es lo que hace un rack recien abierto y lo que trae todo documento anterior—. Un valor explicito es
    /// una decision del usuario y manda siempre.
    /// </para>
    /// <para>
    /// <see cref="Posts"/> son overrides POR POSTE. Un poste ausente hereda la general: «por defecto» no es una
    /// quinta ubicacion, es la ausencia de decision propia.
    /// </para>
    /// </summary>
    public sealed class SelectiveBotaConfig
    {
        /// <summary>La colocacion general elegida, o NULL si nadie ha elegido y la resuelve el sistema.</summary>
        public BootPlacement? Placement { get; set; }

        /// <summary>Los postes con decision propia. Un poste ausente hereda la general.</summary>
        public IList<BootPostPlacement> Posts { get; } = new List<BootPostPlacement>();

        /// <summary>
        /// I-42 (S1E) — la colocacion que el SISTEMA resuelve para ESTE LADO cuando nadie ha elegido: su pasillo.
        /// Es DERIVADA y no se persiste: la autoridad del sistema la impone en cada resolucion.
        /// </summary>
        public BootPlacement? Automatic { get; set; }

        /// <summary>
        /// I-42 (S1E) — los postes donde ESTE LADO no tiene almacenamiento, y por tanto su automatico no coloca
        /// nada. Es DERIVADA como <see cref="Automatic"/>, y es del LADO: el blanco de A no apaga las botas de B.
        /// Nunca bloquea una decision propia — esa se resuelve antes.
        /// </summary>
        public IList<int> BlankPosts { get; } = new List<int>();

        /// <summary>True cuando ESE poste no tiene almacenamiento de este lado.</summary>
        public bool IsBlankAt(int postIndex)
        {
            foreach (var post in BlankPosts)
            {
                if (post == postIndex)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// I-42 (S1E) — LA COLOCACION EFECTIVA de este lado en un poste, con la precedencia del contrato:
        ///
        /// <list type="number">
        /// <item>la decision PROPIA del poste, que gana siempre;</item>
        /// <item>el BLANCO de este lado, que apaga su automatico ahi;</item>
        /// <item>la general de este lado, y si nadie la eligio, su automatico.</item>
        /// </list>
        /// </summary>
        public BootPlacement PlacementAt(int postIndex)
        {
            var own = At(postIndex);
            if (own.HasValue)
            {
                return own.Value;
            }

            if (IsBlankAt(postIndex))
            {
                return BootPlacement.None;
            }

            return Inherited();
        }

        /// <summary>Lo que hereda un poste sin decision propia: la general, o el automatico del sistema.</summary>
        public BootPlacement Inherited()
            => Placement ?? Automatic ?? BootPlacement.None;

        /// <summary>True cuando ESE poste tiene decision propia en este lado.</summary>
        public bool HasOwnAt(int postIndex) => At(postIndex).HasValue;

        /// <summary>
        /// True cuando este lado pide ALGO: lo que hereda —su general, o el automatico del sistema si nadie
        /// eligio— o la decision de algun poste. Una general en «Ninguno» es el DEFECTO de los postes que heredan,
        /// no un interruptor: un poste con decision propia sigue pidiendo.
        /// </summary>
        public bool AsksForSomething()
        {
            if (Inherited() != BootPlacement.None)
            {
                return true;
            }

            foreach (var post in Posts)
            {
                if (post != null && post.Placement != BootPlacement.None)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>La decision propia de un poste, o NULL si hereda.</summary>
        public BootPlacement? At(int postIndex)
        {
            foreach (var post in Posts)
            {
                if (post != null && post.PostIndex == postIndex)
                {
                    return post.Placement;
                }
            }

            return null;
        }

        public SelectiveBotaConfig DeepCopy()
        {
            var copy = new SelectiveBotaConfig { Placement = Placement, Automatic = Automatic };
            foreach (var post in Posts)
            {
                if (post != null)
                {
                    copy.Posts.Add(new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement });
                }
            }

            foreach (var post in BlankPosts)
            {
                copy.BlankPosts.Add(post);
            }

            return copy;
        }
    }
}
