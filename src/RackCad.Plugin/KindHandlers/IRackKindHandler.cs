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

        /// <summary>Rebuild ONE rack's bill of materials from its embedded design. Returns <c>null</c> for an
        /// unreadable/foreign payload — the caller skips it (best-effort, unchanged).</summary>
        BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog);

        /// <summary>Re-stamp the kind-specific inner identity of an INDEPENDENT copy's design (selective: Id+Name;
        /// cabecera: Header.Name). Kinds with no inner identity of their own (dynamic, cama) return
        /// <paramref name="designJson"/> untouched.</summary>
        string RestampDesign(string designJson, string newId, string copyName);
    }
}
