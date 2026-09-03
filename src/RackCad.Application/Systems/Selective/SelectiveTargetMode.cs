namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// How the target fondos were chosen (I-43, gate 8A correction). It is the user's INTENT, which the set alone
    /// cannot express: "Actual" and "Fondo 2" look identical while fondo 2 is the one on screen, yet only the first
    /// should follow when the user navigates elsewhere.
    /// <para>
    /// Runtime only: nothing about it reaches the design or the rack. It IS remembered between openings as an editor
    /// preference (gate 8 correction) — see <see cref="SelectiveTargetPreference"/> — which is why the two "living"
    /// modes exist as modes instead of as the sets they currently resolve to: a remembered "Todos" must expand to the
    /// fondos the NEXT rack has, and a remembered "Actual" must aim at whichever fondo that rack opens on.
    /// </para>
    /// </summary>
    public enum SelectiveTargetMode
    {
        /// <summary>"Actual": the targets are the fondo being edited and keep following it.</summary>
        FollowCurrent,

        /// <summary>A deliberate set of specific fondos, which navigating must not change.</summary>
        Explicit,

        /// <summary>
        /// "Todos": every fondo of the rack, and it KEEPS meaning every fondo. A snapshot of the indices would not do:
        /// adding a fondo to a rack whose editor says "Todos" must include the new one, and the whole point of
        /// remembering the preference is that it survives onto a rack with a different number of fondos.
        /// </summary>
        All
    }
}
