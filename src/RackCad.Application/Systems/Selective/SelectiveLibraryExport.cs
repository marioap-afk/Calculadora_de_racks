using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>The document to write to the library, or the reason there is no artifact.</summary>
    public sealed class SelectiveLibraryExportResult
    {
        private SelectiveLibraryExportResult(bool success, SelectivePalletDesignDocument document, string error)
        {
            IsSuccess = success;
            Document = document;
            Error = error;
        }

        public bool IsSuccess { get; }

        /// <summary>The literal-only document. Null when blocked — there is no partial artifact.</summary>
        public SelectivePalletDesignDocument Document { get; }

        /// <summary>The visible reason. Null on success.</summary>
        public string Error { get; }

        public static SelectiveLibraryExportResult Success(SelectivePalletDesignDocument document)
            => new SelectiveLibraryExportResult(true, document, null);

        public static SelectiveLibraryExportResult Blocked(string error)
            => new SelectiveLibraryExportResult(false, null, error);
    }

    /// <summary>
    /// Turns a drawing's Selective rack into a LIBRARY artifact (I-47 G15).
    ///
    /// <para>
    /// A <c>.rackcad.json</c> lives outside the drawing, and project variables belong TO the drawing. Carrying
    /// a binding into the file would produce a reference pointing at a register that does not exist there:
    /// reopened in another drawing it is a broken reference from the first second, or — worse — one that
    /// resolves against someone else's variable that happens to share the id. So the export MATERIALISES: it
    /// stores the number in force and leaves the binding where it means something.
    /// </para>
    /// <para>
    /// The version goes back to the literal line for the same reason. The drawing's promoted major exists
    /// because there ARE bindings a previous build would not understand; a library file without bindings has
    /// no reason to refuse to open in one. Stickiness is a property of the drawing, not of the artifact.
    /// </para>
    /// <para>
    /// And a binding that cannot be resolved produces NO artifact. Exporting the frozen authored literal
    /// instead would write a number that is not the rack's value into a file that outlives the drawing — the
    /// silent fallback this whole contract exists to prevent, made permanent.
    /// </para>
    /// </summary>
    public static class SelectiveLibraryExport
    {
        private static readonly SelectiveEffectiveDesignResolver Resolver = new SelectiveEffectiveDesignResolver();

        /// <summary>
        /// The general form: the authored document as the drawing holds it, plus the drawing's register.
        /// A null register is a drawing with no variables — valid legacy, and a broken reference for a rack
        /// that names one.
        /// </summary>
        public static SelectiveLibraryExportResult Materialize(
            SelectivePalletDesignDocument authored, ProjectVariablesDocument projectVariables)
        {
            if (authored == null)
            {
                return SelectiveLibraryExportResult.Blocked("No hay diseño que exportar.");
            }

            var resolution = Resolver.Resolve(authored, projectVariables);

            if (!resolution.IsSuccess)
            {
                return SelectiveLibraryExportResult.Blocked(resolution.Error);
            }

            return SelectiveLibraryExportResult.Success(
                FromEffective(resolution.Design, authored.Id, authored.Name));
        }

        /// <summary>
        /// The form the editor uses: since G12 it already holds the EFFECTIVE design, so materialising is all
        /// that is left. Building from the domain is exactly what drops the binding and the promoted version —
        /// the domain cannot carry either, and here that loss is the point rather than a hazard.
        /// </summary>
        public static SelectivePalletDesignDocument FromEffective(
            SelectivePalletDesign effective, string id, string name)
        {
            var document = SelectivePalletDesignDocument.From(effective, id, name);

            // Said explicitly rather than inherited: an artifact is literal-only, whatever the drawing was on.
            document.PropertyValues = null;
            document.SchemaVersion = SelectivePalletDesignDocument.CurrentSchemaVersion;
            return document;
        }
    }
}
