using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>
    /// The per-<see cref="RackEmbedDocument.Kind"/> operations the Plugin dispatches for an already-drawn rack:
    /// reopen its editor (RACKEDITAR), rebuild its BOM (RACKBOMTOTAL) and re-stamp an INDEPENDENT copy's inner
    /// identity (RACKDUPLICAR / RACKLAYOUT). One implementation per embedded kind; <see cref="KindHandlerRegistry"/>
    /// maps the envelope's <see cref="RackEmbedDocument.Kind"/> string to the handler.
    ///
    /// Each handler is a THIN FAÇADE over the existing edit entry points, stores, resolvers and builders — it moves
    /// the former per-kind <c>switch</c>/<c>if</c> arm here verbatim, it does not reimplement any edit/BOM/restamp
    /// algorithm. <c>Larguero</c> has no embed discriminator and no handler (not every kind draws a rack block).
    /// </summary>
    internal interface IRackKindHandler
    {
        /// <summary>The persisted envelope discriminator this handler serves (one of the
        /// <c>RackEmbedDocument.Kind*</c> constants).</summary>
        string Kind { get; }

        /// <summary>Spanish display label for RACKBOMTOTAL's per-rack breakdown (e.g. "Selectivo").</summary>
        string BomLabel { get; }

        /// <summary>Reopen the right editor for this rack and redraw every view-block in place (RACKEDITAR).</summary>
        void Edit(Document document, ObjectId blockId, RackEmbedDocument embed);

        /// <summary>
        /// Rebuild ONE rack's bill of materials from its embedded design, TYPED (I-47 G13).
        ///
        /// <para>
        /// It used to return <c>null</c> for everything that was not a BOM, and the caller wrapped the call in a
        /// blanket <c>catch</c>. Between them, a corrupt payload and a rack whose project VARIABLE is missing
        /// arrived as the same thing — and the second is not a payload problem at all: the design is perfectly
        /// readable. <see cref="BomBuildResult"/> keeps them apart, and the caller applies a different policy to
        /// each.
        /// </para>
        /// <para>
        /// <paramref name="projectVariables"/> is the register snapshot the COMMAND read once. A handler never
        /// reads the drawing: five of the six kinds have no bindings in ID22A and ignore it entirely.
        /// </para>
        /// <para>
        /// A kind with NO handler is a different matter, resolved and reported by the caller up front, so it
        /// never reaches this method.
        /// </para>
        /// </summary>
        BomBuildResult BuildBom(
            RackEmbedDocument embed, RackCatalog catalog, ProjectVariablesDocument projectVariables);

        /// <summary>
        /// I-42 (A1C/H11) — el motivo por el que este rack NO puede producir salida final, o <c>null</c> si puede.
        ///
        /// <para>
        /// No es una regla nueva ni una segunda validacion: cada handler pregunta a la MISMA autoridad de
        /// diagnosticos que usa su editor. Un kind sin diagnosticos propios responde siempre <c>null</c>, que es
        /// exactamente lo que hacia antes.
        /// </para>
        /// </summary>
        string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog);

        /// <summary>
        /// Re-stamp the kind-specific inner identity of an INDEPENDENT copy's design (selective: Id+Name;
        /// cabecera: Header.Name), TYPED (I-47 G14). Kinds with no inner identity of their own (dynamic, cama)
        /// succeed with <paramref name="designJson"/> untouched.
        ///
        /// <para>
        /// It used to return a bare string, so a design it could not re-stamp came back as the ORIGINAL — and
        /// the copy was written with a fresh RackId outside and the source's identity inside. A failure is a
        /// value now, and the caller has to look at it BEFORE materialising anything.
        /// </para>
        /// </summary>
        RestampResult RestampDesign(string designJson, string newId, string copyName);
    }
}
