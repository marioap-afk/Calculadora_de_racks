using System;
using RackCad.Application.Catalogs;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// The snapshot passed to the request builder in <see cref="RackEditorSession{TDesign,TSystem}.RequestInsert"/> /
    /// <see cref="RackEditorSession{TDesign,TSystem}.RequestUpdate"/>: the ensured identity, the current model and the
    /// normalized view/section so a concrete editor only has to wrap them in the right <see cref="RackInsertionRequest"/>
    /// subtype. The session owns the flag block + normalization; the payload shape is the only per-system part.
    /// </summary>
    public readonly struct RackInsertionContext<TDesign, TSystem>
    {
        public RackInsertionContext(string id, string name, TDesign design, TSystem system, string view, int section, bool updateOnly)
        {
            Id = id;
            Name = name;
            Design = design;
            System = system;
            View = view;
            Section = section;
            UpdateOnly = updateOnly;
        }

        public string Id { get; }

        public string Name { get; }

        public TDesign Design { get; }

        public TSystem System { get; }

        /// <summary>The requested view, or null on an update (in-place redraw of every existing view).</summary>
        public string View { get; }

        /// <summary>The requested section, or -1 on an update or a system with no sections.</summary>
        public int Section { get; }

        public bool UpdateOnly { get; }
    }

    /// <summary>
    /// The shared editor "shell" session (initiative I-15, audit §4.2): it concentrates the four things each editor
    /// window clones today — the <see cref="Catalog"/>, the rack <see cref="Identity"/> (GUID + name), the coalesced
    /// <see cref="Recompute"/> and the insert/update contract (<see cref="InsertRequested"/>/<see cref="UpdateOnly"/>/
    /// <see cref="InsertView"/>/<see cref="InsertSection"/> + <see cref="InsertionRequest"/>). It is generic over the
    /// editable design and the resolved system, so a future editor (Push Back, or the selective/dynamic state extraction
    /// of I-20/I-21) builds on it instead of re-inlining the pipeline. This iteration introduces the shell and wires the
    /// menu/library to the module registry; it does NOT migrate the five existing windows onto it (strangler, like I-14).
    /// UI-layer: no AutoCAD; the payload it produces is drawn by the Plugin host.
    /// </summary>
    public sealed class RackEditorSession<TDesign, TSystem>
    {
        /// <summary>Creates a session. <paramref name="catalog"/> defaults to the shared safe loader
        /// (<c>UiSupport.LoadCatalogSafe</c>); <paramref name="recompute"/> is the editor's rebuild (defaults to a no-op,
        /// for callers that only use identity/contract); <paramref name="newIdFactory"/> is the injectable GUID source.</summary>
        public RackEditorSession(RackCatalog catalog = null, Action recompute = null, Func<string> newIdFactory = null)
        {
            Catalog = catalog ?? UiSupport.LoadCatalogSafe();
            Identity = new RackEditorIdentity(newIdFactory);
            Recompute = new RecomputeGate(recompute ?? (() => { }));
        }

        /// <summary>The catalog every editor loads once through <c>UiSupport.LoadCatalogSafe</c>.</summary>
        public RackCatalog Catalog { get; }

        /// <summary>The rack identity (lazy GUID + name) for the drawing round-trip.</summary>
        public RackEditorIdentity Identity { get; }

        /// <summary>The synchronous, scope-based coalescing of the editor's recompute.</summary>
        public RecomputeGate Recompute { get; }

        /// <summary>The current editable design (set by <see cref="SetModel"/>).</summary>
        public TDesign Design { get; private set; }

        /// <summary>The current resolved system (set by <see cref="SetModel"/>).</summary>
        public TSystem System { get; private set; }

        /// <summary>True once <see cref="RequestInsert"/> or <see cref="RequestUpdate"/> ran (the window then closes).</summary>
        public bool InsertRequested { get; private set; }

        /// <summary>True when the last request was an in-place update ("Actualizar"), false for an insert ("Insertar").</summary>
        public bool UpdateOnly { get; private set; }

        /// <summary>The requested view (null on an update), mirroring the editors' <c>updateOnly ? null : view</c>.</summary>
        public string InsertView { get; private set; }

        /// <summary>The requested section (-1 on an update), mirroring the editors' <c>updateOnly ? -1 : section</c>.</summary>
        public int InsertSection { get; private set; } = -1;

        /// <summary>The produced payload the host draws, or null until a request is made.</summary>
        public RackInsertionRequest InsertionRequest { get; private set; }

        /// <summary>Raised when a request is made, so the window can close and let the host draw.</summary>
        public event EventHandler InsertRequestedRaised;

        /// <summary>Sets the current model (design + resolved system) that a later request captures.</summary>
        public void SetModel(TDesign design, TSystem system)
        {
            Design = design;
            System = system;
        }

        /// <summary>
        /// "Insertar": ensures the id, records view/section (no update), builds the payload from the current model via
        /// <paramref name="build"/> and raises <see cref="InsertRequestedRaised"/>. Same effect as the editors'
        /// <c>RequestDraw(view, section, updateOnly:false)</c>.
        /// </summary>
        public void RequestInsert(string view, int section, Func<RackInsertionContext<TDesign, TSystem>, RackInsertionRequest> build)
            => Complete(view, section, updateOnly: false, build);

        /// <summary>
        /// "Actualizar": ensures the id, clears view/section (in-place redraw), builds the payload and raises
        /// <see cref="InsertRequestedRaised"/>. Same effect as the editors' <c>RequestDraw(view:null, updateOnly:true)</c>.
        /// </summary>
        public void RequestUpdate(Func<RackInsertionContext<TDesign, TSystem>, RackInsertionRequest> build)
            => Complete(view: null, section: -1, updateOnly: true, build);

        private void Complete(string view, int section, bool updateOnly, Func<RackInsertionContext<TDesign, TSystem>, RackInsertionRequest> build)
        {
            if (build == null)
            {
                throw new ArgumentNullException(nameof(build));
            }

            Identity.EnsureId();
            UpdateOnly = updateOnly;
            InsertView = updateOnly ? null : view;
            InsertSection = updateOnly ? -1 : section;
            InsertionRequest = build(new RackInsertionContext<TDesign, TSystem>(
                Identity.Id, Identity.Name, Design, System, InsertView, InsertSection, updateOnly));
            InsertRequested = true;
            InsertRequestedRaised?.Invoke(this, EventArgs.Empty);
        }
    }
}
