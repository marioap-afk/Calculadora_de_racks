using RackCad.UI.Shell;

namespace RackCad.UI.Systems.Cantilever.Components
{
    /// <summary>
    /// TEMPORARY COMPATIBILITY FACADE (I-39A). The shell of the four Cantilever component editors is now
    /// <see cref="RackBoundedEditorShell"/>, which lives in shared infrastructure because it never knew anything
    /// about Cantilever: its seven slots are <c>object</c> and it has no branches. Only its NAME and its LOCATION
    /// were tied to a system, and a consumer from another system would have had to declare a dependency on this
    /// namespace to reuse it — the inverted dependency ADR-0029 D12 forbids.
    ///
    /// <para>This type exists so the four component XAMLs keep working unchanged: I-39A does not migrate them, and
    /// their diff must stay empty. It declares NO members of its own and does NOT override
    /// <c>DefaultStyleKeyProperty</c>, so it inherits the base type's style key and with it the very same template
    /// — no duplicated dependency properties, no second style, nothing written to satisfy a guard.</para>
    ///
    /// <para>I-39C migrates the four XAMLs to <see cref="RackBoundedEditorShell"/> and deletes this file.</para>
    /// </summary>
    public class CantileverComponentEditorShell : RackBoundedEditorShell
    {
    }
}
