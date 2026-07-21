namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Pure factory for the uniform drawing envelope (<see cref="RackEmbedDocument"/>). Constructing a NEW envelope object
    /// for a redraw or an inserted view would DROP the unknown JSON fields (<see cref="RackEmbedDocument.ExtensionData"/>)
    /// of the block it replaces and could DOWNGRADE its schema version (I-11 D3). <see cref="Compose"/> keeps both: it
    /// inherits the extension data and a non-downgraded write version (via <see cref="SchemaVersionPolicy"/>) from the
    /// SOURCE envelope, while taking the fresh identity / view / section / design from the caller.
    ///
    /// Callers pass:
    /// <list type="bullet">
    /// <item>for an EXISTING view-block being redrawn — THAT block's own embed (preserving its per-view metadata);</item>
    /// <item>for a NEW view inserted during an edit — the initiating (picked) envelope, so it inherits its metadata;</item>
    /// <item>for a brand-new rack (a Quick* insert) — <c>null</c> (fresh current version, no extension data).</item>
    /// </list>
    /// No I/O; not a Plugin type. It preserves the envelope only — it does NOT recurse into the type-specific
    /// <see cref="RackEmbedDocument.Design"/> payload (see the FlowBed/Larguero documents for their own preservation).
    /// </summary>
    public static class RackEmbedComposer
    {
        public static RackEmbedDocument Compose(
            RackEmbedDocument source, string kind, string id, string name, string view, int section, string design)
        {
            return new RackEmbedDocument
            {
                SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source?.SchemaVersion, RackEmbedDocument.CurrentSchemaVersion),
                Kind = kind,
                Id = id,
                Name = name,
                View = view,
                Section = section,
                Design = design,
                ExtensionData = source?.ExtensionData
            };
        }
    }
}
