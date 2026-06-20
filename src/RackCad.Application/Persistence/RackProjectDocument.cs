using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Top-level project wrapper (schema 2.0). A <c>Kind</c> discriminator selects the payload:
    /// a bare header for a selective project, or a dynamic system. Files without a <c>kind</c>
    /// (schema 1.0 bare headers) are still loadable and treated as selective.
    /// </summary>
    public sealed class RackProjectDocument
    {
        public string SchemaVersion { get; set; } = "2.0";
        public RackSystemKind Kind { get; set; } = RackSystemKind.Selective;
        public RackFrameProjectDocument Header { get; set; }
        public DynamicRackSystemDocument DynamicSystem { get; set; }
    }
}
