using RackCad.Application.Persistence;
using RackCad.Domain.Systems;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// One rack system's editor, as the menu and the design library see it (initiative I-15, audit §4.1/§4.2). The
    /// <see cref="EditorModuleRegistry"/> iterates these instead of <see cref="RackMainMenuWindow"/> carrying ~19 payload
    /// properties and five near-identical <c>Design*_Click</c> handlers plus a five-branch library switch. A module knows
    /// its <see cref="Kind"/>, whether it can insert, how to open its window for a NEW design and FROM a library design,
    /// and how to recognize its own library entries. Opening a window is a UI concern, so modules live in UI and adapt
    /// the existing editor windows verbatim (no window is rewritten this iteration).
    /// </summary>
    public interface IRackEditorModule
    {
        /// <summary>The canonical system kind this module edits (unique within a registry).</summary>
        RackSystemKind Kind { get; }

        /// <summary>False for a component with no AutoCAD block (the larguero), which opens but never inserts.</summary>
        bool CanInsert { get; }

        /// <summary>
        /// True for the module that catches any library project by payload rather than by <see cref="Kind"/> — today the
        /// cabecera, matched by <c>project.Header != null</c> as the historic fallback. Fallback modules are resolved
        /// AFTER the kind-specific ones (see <see cref="EditorModuleRegistry.ResolveForLibrary"/>), preserving the exact
        /// order of the old <c>OpenDesignLibrary_Click</c> if/else.
        /// </summary>
        bool IsLibraryFallback { get; }

        /// <summary>The exact "No se pudo abrir …: " prefix the menu shows if opening this editor throws (frozen verbatim).</summary>
        string OpenFailureMessage { get; }

        /// <summary>
        /// True when this module handles <paramref name="project"/> loaded from library <paramref name="entry"/> — the
        /// per-kind data check of the old library switch (e.g. <c>entry.Kind == PalletFlow &amp;&amp; project.DynamicDesign
        /// != null</c>), or the cabecera fallback (<c>project.Header != null</c>). Pure: no window is opened.
        /// </summary>
        bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project);

        /// <summary>
        /// Opens this editor for a brand-new design (the menu's <c>Design*_Click</c>). Returns the insertion request when
        /// the user asked to draw it, or null when they closed/cancelled or the module never inserts (the larguero).
        /// </summary>
        RackInsertionRequest OpenForNew(RackEditorLaunchContext context);

        /// <summary>
        /// Opens this editor pre-loaded from a library <paramref name="project"/>/<paramref name="entry"/> as a NEW design
        /// (the menu's <c>OpenDesignLibrary_Click</c>). Returns the insertion request or null (cancel / never inserts).
        /// </summary>
        RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context);
    }
}
