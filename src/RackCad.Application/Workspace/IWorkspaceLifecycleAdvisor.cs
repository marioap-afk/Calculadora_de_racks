#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T2B2B (D-T2B2-09): the one seam through which the pure bridge core asks the host what to do with the close and the
    /// destruction of a document. The core owns no state of drafts: it asks, and it keeps only what it can observe itself. Both calls
    /// are synchronous and in memory: they neither read nor write the drawing and open no dialog. A decorator may write a notice on the
    /// command line (I-64 F4-T3C2).
    /// </summary>
    public interface IWorkspaceLifecycleAdvisor
    {
        /// <summary>
        /// The advice for one attempt to close the document of the session: Allow, or Veto while some work could be lost. Every
        /// attempt is evaluated anew; repeating it is never consent. The core turns an exception of the advisor into a Veto.
        /// </summary>
        CloseDecision AdviseClose(SessionId session);

        /// <summary>
        /// The session was destroyed (active or not). <paramref name="closeSequenceIntact"/> says that the core saw the close of this
        /// very session allowed and, since then, no other input and no fault of the session. The advisor must not call the bridge back.
        /// </summary>
        void SessionDestroyed(SessionId session, bool closeSequenceIntact);
    }
}
