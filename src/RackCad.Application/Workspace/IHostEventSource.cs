#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// The seam between the pure bridge policy and whatever raises the events (AutoCAD in the Plugin, a simulation in tests).
    /// Attach makes exactly one native hookup for the attachment; Detach removes exactly that one. It only moves
    /// event hookups: it carries no read and no write of the document, the selection or the view.
    /// </summary>
    public interface IHostEventSource
    {
        void Attach(SourceAttachment attachment);

        void Detach(SourceAttachment attachment);
    }
}
