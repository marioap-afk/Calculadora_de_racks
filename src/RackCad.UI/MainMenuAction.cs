namespace RackCad.UI
{
    /// <summary>
    /// Something the main menu asks the host to do that is NOT inserting a rack.
    ///
    /// The menu already carries one typed <see cref="Editor.RackInsertionRequest"/> for the six systems it can
    /// design (I-15). A structural section does not fit there, and forcing it in would be wrong rather than
    /// merely inelegant: a section is **not a rack**. It has no <c>RackSystemKind</c> to dispatch on, no design
    /// payload to embed, and no round-trip — what it produces is plain geometry the user measures and copies,
    /// not an editable rack with a GUID. A request whose <c>Kind</c> had to be faked would be a lie the host
    /// then has to special-case.
    ///
    /// So the menu reports an ACTION instead. The host reads it **after** the modal window has closed, which is
    /// the same rule the insertion request follows and for the same reason: the AutoCAD editor must be free
    /// before anything prompts for a point.
    ///
    /// This enum stays small on purpose. If it ever grows a family of variants with payloads, that is the
    /// signal to give it its own typed hierarchy — not to widen the enum.
    /// </summary>
    public enum MainMenuAction
    {
        /// <summary>The menu closed without asking for anything. The default.</summary>
        None = 0,

        /// <summary>
        /// Open the structural-section generator: the same flow the <c>RACKSECCION</c> command runs.
        ///
        /// The generator already exists —catalogue, geometry, inspector, preview and materialisation— and this
        /// action only makes it reachable from the menu. It does not describe a second generator.
        /// </summary>
        GenerateStructuralSection = 1
    }
}
