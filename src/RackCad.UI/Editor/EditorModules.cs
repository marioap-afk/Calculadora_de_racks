using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI.Editor
{
    // The five editor modules the menu and library consume (initiative I-15). Each ADAPTS an existing editor window
    // verbatim — no window is rewritten this iteration (that is I-20/I-21). Every Open* method reproduces the exact
    // gestures of the corresponding old RackMainMenuWindow handler, including the pre-existing asymmetries (e.g. the
    // menu's brand-new selective did NOT set dimension styles, while its library path did). The pure metadata
    // (Kind/CanInsert/IsLibraryFallback/OpenFailureMessage/MatchesLibrary) is unit-tested; opening a window is not.

    /// <summary>Selective pallet rack (<see cref="RackSystemKind.SelectiveRack"/>) → <see cref="RackSelectiveWindow"/>.</summary>
    public sealed class SelectiveEditorModule : IRackEditorModule
    {
        public RackSystemKind Kind => RackSystemKind.SelectiveRack;

        public bool CanInsert => true;

        public bool IsLibraryFallback => false;

        public string OpenFailureMessage => "No se pudo abrir el sistema selectivo: ";

        public bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project)
            => entry != null && entry.Kind == RackSystemKind.SelectiveRack && project?.SelectiveRack != null;

        public RackInsertionRequest OpenForNew(RackEditorLaunchContext context)
        {
            // Frozen verbatim from DesignSelective_Click: the brand-new selective does NOT set dimension styles
            // (unlike the RACKSELECTIVO command and the library path below). Preserved, not "fixed" (out of scope).
            var window = new RackSelectiveWindow(context.CanInsertInAutoCad) { Owner = context.Owner };
            window.ShowDialog();
            return Build(window);
        }

        public RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context)
        {
            var window = new RackSelectiveWindow(context.CanInsertInAutoCad) { Owner = context.Owner };
            window.SetDimensionStyles(context.DimensionStyles); // library path sets styles (frozen from OpenDesignLibrary_Click)
            window.LoadForNew(project.SelectiveRack, project); // pass the source project so a re-save preserves wrapper metadata (I-11)
            window.ShowDialog();
            return Build(window);
        }

        private static RackInsertionRequest Build(RackSelectiveWindow window)
            => window.InsertRequested
                ? new SelectiveInsertionRequest(window.SystemToInsert, window.DesignToInsert, window.RackId, window.RackName, window.InsertView)
                : null;
    }

    /// <summary>Dynamic pallet-flow (<see cref="RackSystemKind.PalletFlow"/>) → <see cref="RackDynamicSystemWindow"/>.</summary>
    public sealed class DynamicEditorModule : IRackEditorModule
    {
        public RackSystemKind Kind => RackSystemKind.PalletFlow;

        public bool CanInsert => true;

        public bool IsLibraryFallback => false;

        public string OpenFailureMessage => "No se pudo abrir el sistema dinámico: ";

        public bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project)
            => entry != null && entry.Kind == RackSystemKind.PalletFlow && project?.DynamicDesign != null;

        public RackInsertionRequest OpenForNew(RackEditorLaunchContext context)
        {
            var window = new RackDynamicSystemWindow(context.CanInsertInAutoCad) { Owner = context.Owner };
            window.SetDimensionStyles(context.DimensionStyles);
            window.ShowDialog();
            return Build(window);
        }

        public RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context)
        {
            var window = new RackDynamicSystemWindow(context.CanInsertInAutoCad) { Owner = context.Owner };
            window.SetDimensionStyles(context.DimensionStyles);
            window.LoadDesignForNew(project.DynamicDesign, entry.Name, project); // pass the source project so a re-save preserves its wrapper metadata (I-11)
            window.ShowDialog();
            return Build(window);
        }

        private static RackInsertionRequest Build(RackDynamicSystemWindow window)
            => window.InsertRequested
                ? new DynamicInsertionRequest(
                    window.SystemToInsert, window.DesignToInsert, window.RackId, window.RackName,
                    window.InsertView, window.InsertSection, window.SourceProjectToInsert) // null for a new design; the library wrapper metadata for a library open (I-11)
                : null;
    }

    /// <summary>Standalone cabecera (<see cref="RackSystemKind.Selective"/>) → <see cref="RackFrameConfiguratorWindow"/>.
    /// The library fallback: matched by <c>project.Header != null</c>, tried AFTER the kind-specific modules.</summary>
    public sealed class HeaderEditorModule : IRackEditorModule
    {
        public RackSystemKind Kind => RackSystemKind.Selective;

        public bool CanInsert => true;

        public bool IsLibraryFallback => true;

        public string OpenFailureMessage => "No se pudo abrir el configurador de cabeceras: ";

        public bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project)
            => project?.Header != null; // historic catch-all: any project with a header, regardless of entry.Kind

        public RackInsertionRequest OpenForNew(RackEditorLaunchContext context)
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var window = new RackFrameConfiguratorWindow(configuration, context.CanInsertInAutoCad) { Owner = context.Owner };
            window.ShowDialog();
            return window.InsertRequested ? new HeaderInsertionRequest(window.Configuration, sourceProject: null) : null;
        }

        public RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context)
        {
            var window = new RackFrameConfiguratorWindow(project.Header, context.CanInsertInAutoCad) { Owner = context.Owner };
            window.ShowDialog();
            // Carry the wrapper metadata when the header came from a RackProject wrapper; a bare legacy header's project has
            // no source document, so WithSourceMetadataFrom downstream is a no-op (I-11).
            return window.InsertRequested ? new HeaderInsertionRequest(window.Configuration, sourceProject: project) : null;
        }
    }

    /// <summary>Flow bed / cama (<see cref="RackSystemKind.Cama"/>) → <see cref="RackFlowBedWindow"/>.</summary>
    public sealed class FlowBedEditorModule : IRackEditorModule
    {
        public RackSystemKind Kind => RackSystemKind.Cama;

        public bool CanInsert => true;

        public bool IsLibraryFallback => false;

        public string OpenFailureMessage => "No se pudo abrir la cama de rodamiento: ";

        public bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project)
            => entry != null && entry.Kind == RackSystemKind.Cama && project?.FlowBed != null;

        public RackInsertionRequest OpenForNew(RackEditorLaunchContext context)
        {
            var window = new RackFlowBedWindow(context.CanInsertInAutoCad) { Owner = context.Owner };
            window.ShowDialog();
            return Build(window);
        }

        public RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context)
        {
            var window = new RackFlowBedWindow(context.CanInsertInAutoCad) { Owner = context.Owner };
            window.LoadForNew(project.FlowBed, entry.Name, project); // pass the source project so a re-save preserves its unknown metadata (I-11)
            window.ShowDialog();
            return Build(window);
        }

        private static RackInsertionRequest Build(RackFlowBedWindow window)
            => window.InsertRequested
                ? new FlowBedInsertionRequest(window.FlowBedToInsert, window.RackId, window.RackName, window.SourceFlowBedToInsert)
                : null;
    }

    /// <summary>Larguero component (<see cref="RackSystemKind.Larguero"/>) → <see cref="RackLargueroWindow"/>. Visual +
    /// BOM + save-to-library only: it opens but NEVER inserts, so both Open* return null.</summary>
    public sealed class LargueroEditorModule : IRackEditorModule
    {
        public RackSystemKind Kind => RackSystemKind.Larguero;

        public bool CanInsert => false;

        public bool IsLibraryFallback => false;

        public string OpenFailureMessage => "No se pudo abrir el editor de largueros: ";

        public bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project)
            => entry != null && entry.Kind == RackSystemKind.Larguero && project?.Larguero != null;

        public RackInsertionRequest OpenForNew(RackEditorLaunchContext context)
        {
            // Visual + BOM only (no AutoCAD block yet): opens, previews, and saves to the library. Never inserts.
            new RackLargueroWindow { Owner = context.Owner }.ShowDialog();
            return null;
        }

        public RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context)
        {
            // Larguero is visual-only (no AutoCAD block) — just open its editor pre-loaded.
            var window = new RackLargueroWindow { Owner = context.Owner };
            window.LoadExisting(project.Larguero, project); // pass the source project so a re-save preserves its unknown metadata (I-11)
            window.ShowDialog();
            return null;
        }
    }
}
