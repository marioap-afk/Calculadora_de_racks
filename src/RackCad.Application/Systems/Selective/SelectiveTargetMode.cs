namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// How the target fondos were chosen (I-43, gate 8A correction). It is the user's INTENT, which the set alone
    /// cannot express: "Actual" and "Fondo 2" look identical while fondo 2 is the one on screen, yet only the first
    /// should follow when the user navigates elsewhere.
    /// <para>Runtime only, like the target set itself: nothing about it is persisted.</para>
    /// </summary>
    public enum SelectiveTargetMode
    {
        /// <summary>"Actual": the targets are the fondo being edited and keep following it.</summary>
        FollowCurrent,

        /// <summary>A deliberate set — one fondo, several, or all — which navigating must not change.</summary>
        Explicit
    }
}
