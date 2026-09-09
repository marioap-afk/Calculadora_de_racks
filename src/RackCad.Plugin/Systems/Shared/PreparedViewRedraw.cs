using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Plugin.Drawing;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>
    /// Everything one view-block redraw needs, resolved BEFORE the semantic transaction opens (I-47 G9.1).
    ///
    /// <para>
    /// It exists because the expensive and dangerous half of a redraw is the preparation, not the write:
    /// loading the catalog, building the plan and — above all — IMPORTING the block definitions the plan
    /// references, which mutates the database on its own account. None of that can happen inside a
    /// transaction that is meant to be one atomic unit over many racks.
    /// </para>
    /// <para>
    /// So the shape is: prepare every view first, then open one lock and one transaction, then write. This
    /// object is what survives between those two moments.
    /// </para>
    /// </summary>
    internal sealed class PreparedViewRedraw
    {
        internal PreparedViewRedraw(
            ObjectId blockId,
            LateralHeaderDrawer drawer,
            HeaderRunPlan plan,
            string payloadJson,
            RackCatalog catalog)
        {
            BlockId = blockId;
            Drawer = drawer;
            Plan = plan;
            PayloadJson = payloadJson;
            Catalog = catalog;
        }

        /// <summary>The block definition to redefine.</summary>
        internal ObjectId BlockId { get; }

        internal LateralHeaderDrawer Drawer { get; }

        /// <summary>The plan, already built — and whose block definitions are already imported.</summary>
        internal HeaderRunPlan Plan { get; }

        /// <summary>The payload this VIEW carries. One per sibling: the envelope is never shared.</summary>
        internal string PayloadJson { get; }

        internal RackCatalog Catalog { get; }
    }
}
