using System;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// The field of the bounded pilot (AnnotationScale of a Push Back rack) on the ONE surface of the browser (I-64 F4-T3B1, D-T3B1-02).
    /// The browser creates it, owns it and exposes it as <c>Browser.Field</c>; it is never null.
    ///
    /// This is the inert skeleton of the red commit: every member exists with its final signature and answers a neutral value, and none
    /// of them reads the port, changes anything or throws. The behavior comes in the commit that follows.
    /// </summary>
    public sealed class WorkspacePilotFieldViewModel : ObservableObject
    {
        /// <summary>What the box shows while the rack has no draft.</summary>
        public const string NoDraftMarker = "—";

        private readonly IWorkspacePilotEditPort _port;

        /// <param name="port">The edit port of the pilot; null leaves the field not available.</param>
        /// <param name="changed">Called after an operation that changed the draft, so the browser rebuilds its strip. It is a method, not a subscription.</param>
        public WorkspacePilotFieldViewModel(IWorkspacePilotEditPort port, Action changed = null)
        {
            _port = port;
        }

        /// <summary>False when the panel was built without the edit port of the pilot: the view shows nothing of the field.</summary>
        public bool IsAvailable => _port != null;

        /// <summary>The text of the box. Inert: it shows nothing and accepts nothing.</summary>
        public string Text
        {
            get => string.Empty;
            set
            {
            }
        }

        public bool IsEditable => false;

        public bool IsReadOnly => true;

        public bool ShowEditButton => false;

        public bool ShowReloadButton => false;

        public string StateText => string.Empty;

        public string MessageText => string.Empty;

        public string NoticeText => string.Empty;

        public string NotEditableText => string.Empty;

        public string ClassicEditorHintText => string.Empty;

        public void Show(SessionId session, string rackId, bool isPreview)
        {
        }

        public void Clear()
        {
        }

        public void Fail(Exception exception)
        {
        }

        public void ShowNotice(string notice)
        {
        }

        public void BeginEdit()
        {
        }

        public void CommitPending()
        {
        }

        public void Rematerialize()
        {
        }
    }
}
