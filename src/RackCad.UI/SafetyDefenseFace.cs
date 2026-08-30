using System.Collections.Generic;
using System.Linq;

namespace RackCad.UI
{
    /// <summary>
    /// I-42 (ronda 7D) — LA CARA que una rejilla por poste esta editando, declarada EXPLICITAMENTE.
    ///
    /// <para>
    /// La rejilla no deduce el lado de nada: ni del lado activo de la ventana principal, ni de un espejo, ni de una
    /// coordenada, ni de si la cara es la primera o la ultima. Lo recibe, con su nombre, con que extremo de la
    /// cobertura es, y con la lista de en que lineas esa cara EXISTE de verdad.
    /// </para>
    ///
    /// <para>
    /// La aplicabilidad viene de la misma autoridad que usa el dibujo, asi que la rejilla no puede pintar «apagado»
    /// donde el rack si lleva defensa —que era el defecto que quedo registrado en la ronda 7C— ni ofrecer una casilla
    /// para una cara que no existe.
    /// </para>
    ///
    /// <para>Es NEUTRAL: no menciona Push Back. Un dialogo sin cara declarada sigue comportandose como siempre.</para>
    /// </summary>
    public sealed class SafetyDefenseFace
    {
        public SafetyDefenseFace(string label, bool isFarEnd, IEnumerable<bool> applicableByPost)
        {
            Label = string.IsNullOrWhiteSpace(label) ? "Defensa" : label.Trim();
            IsFarEnd = isFarEnd;
            ApplicableByPost = (applicableByPost ?? Enumerable.Empty<bool>()).ToList();
        }

        /// <summary>Como se llama esta cara en la rejilla, p. ej. «Lado A».</summary>
        public string Label { get; }

        /// <summary>Si es el extremo LEJANO de la cobertura de la linea (el cercano es el otro).</summary>
        public bool IsFarEnd { get; }

        /// <summary>En que lineas existe esta cara de ataque. Fuera de rango se asume que si, como siempre.</summary>
        public IReadOnlyList<bool> ApplicableByPost { get; }

        public bool AppliesAt(int postIndex)
            => postIndex < 0 || postIndex >= ApplicableByPost.Count || ApplicableByPost[postIndex];
    }
}
