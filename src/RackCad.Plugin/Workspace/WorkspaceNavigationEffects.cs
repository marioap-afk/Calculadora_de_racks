using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Workspace;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The only effects a navigation action may have (I-64 D-07, F2-T2A), executed by the navigation host under the document
    /// lock and after the policy decided Act. It is the ONLY file of the workspace allowed to write: the implicit selection
    /// (SELECT) and the state of the current view (VIEW, CENTER and, through the policy, LOCATE VIEW). It writes nothing
    /// authored: no entity, no Xrecord, no XData, no dictionary, no system variable, no file, no layout and no TILEMODE.
    ///
    /// Its read of the drawing is the minimum of an already-selected target: for VIEW and CENTER it opens, read-only, the one
    /// reference the plan named, only to confirm that it exists, that it is a Model Space block reference and to take its
    /// extents. It discovers nothing: it enumerates no Model Space and no block table, looks for no sibling or alternative
    /// reference and derives no target; the identity of the target comes from the navigation authority and was revalidated by
    /// the policy right before this call.
    /// </summary>
    public sealed class WorkspaceNavigationEffects : INavigationEffects
    {
        // The margin of the plan view, the same 10 percent frame of the classic rack list.
        private const double ViewMargin = 1.10;

        private readonly Func<SessionId, long?> _nativeInstanceOf;

        public WorkspaceNavigationEffects(Func<SessionId, long?> nativeInstanceOf)
        {
            _nativeInstanceOf = nativeInstanceOf ?? throw new ArgumentNullException(nameof(nativeInstanceOf));
        }

        /// <summary>SELECT: the implicit selection becomes exactly the requested references (copies included).</summary>
        public void SelectReferences(SessionId session, IReadOnlyList<long> handles)
        {
            var document = ActiveDocumentOf(session);
            var ids = new List<ObjectId>();
            foreach (var handle in handles)
            {
                if (!document.Database.TryGetObjectId(new Handle(handle), out var id) || id.IsNull || id.IsErased)
                    throw new InvalidOperationException("A target of the selection is gone.");
                ids.Add(id);
            }

            if (ids.Count == 0) return;
            document.Editor.SetImpliedSelection(ids.ToArray());
        }

        /// <summary>VIEW: frames the extents of the reference with a 10 percent margin.</summary>
        public void ViewReference(SessionId session, long handle)
        {
            var document = ActiveDocumentOf(session);
            using (document.LockDocument())
            {
                // The transaction only reads: closing it without Commit would roll a view written inside it back (D-08).
                Extents3d extents;
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    extents = ExtentsOfTarget(document, transaction, handle);
                }

                var editor = document.Editor;
                using (var view = editor.GetCurrentView())
                {
                    var toView = ToViewPlane(view);
                    var low = new Point2d(double.MaxValue, double.MaxValue);
                    var high = new Point2d(double.MinValue, double.MinValue);
                    foreach (var corner in CornersOf(extents))
                    {
                        var local = corner.TransformBy(toView);
                        low = new Point2d(Math.Min(low.X, local.X), Math.Min(low.Y, local.Y));
                        high = new Point2d(Math.Max(high.X, local.X), Math.Max(high.Y, local.Y));
                    }

                    var width = Math.Max((high.X - low.X) * ViewMargin, 1.0);
                    var height = Math.Max((high.Y - low.Y) * ViewMargin, 1.0);

                    // Keep the aspect of the current view so that the whole reference stays inside it.
                    var aspect = view.Height > 0.0 ? view.Width / view.Height : 1.0;
                    if (width / height > aspect) height = width / aspect;
                    else width = height * aspect;

                    view.CenterPoint = new Point2d((low.X + high.X) / 2.0, (low.Y + high.Y) / 2.0);
                    view.Width = width;
                    view.Height = height;
                    editor.SetCurrentView(view);
                }
            }
        }

        /// <summary>CENTER: moves the center of the view to the reference without changing the scale of the view.</summary>
        public void CenterOnReference(SessionId session, long handle)
        {
            var document = ActiveDocumentOf(session);
            using (document.LockDocument())
            {
                // The transaction only reads: closing it without Commit would roll a view written inside it back (D-08).
                Extents3d extents;
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    extents = ExtentsOfTarget(document, transaction, handle);
                }

                var editor = document.Editor;
                using (var view = editor.GetCurrentView())
                {
                    var middle = new Point3d(
                        (extents.MinPoint.X + extents.MaxPoint.X) / 2.0,
                        (extents.MinPoint.Y + extents.MaxPoint.Y) / 2.0,
                        (extents.MinPoint.Z + extents.MaxPoint.Z) / 2.0);
                    var local = middle.TransformBy(ToViewPlane(view));
                    view.CenterPoint = new Point2d(local.X, local.Y);
                    editor.SetCurrentView(view);
                }
            }
        }

        // The document of the session, only while it is the active one: an effect never acts on another document.
        private Autodesk.AutoCAD.ApplicationServices.Document ActiveDocumentOf(SessionId session)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            var native = _nativeInstanceOf(session);
            if (document == null || !native.HasValue || document.UnmanagedObject.ToInt64() != native.Value)
                throw new InvalidOperationException("The document of the session is not the active document.");
            return document;
        }

        // Read-only access to the ONE target the plan named: existence, type and extents. Nothing else is read.
        private static Extents3d ExtentsOfTarget(Autodesk.AutoCAD.ApplicationServices.Document document, Transaction transaction, long handle)
        {
            if (!document.Database.TryGetObjectId(new Handle(handle), out var id) || id.IsNull || id.IsErased)
                throw new InvalidOperationException("The target is gone.");

            var reference = transaction.GetObject(id, OpenMode.ForRead) as BlockReference;
            if (reference == null || reference.OwnerId != SymbolUtilityServices.GetBlockModelSpaceId(document.Database))
                throw new InvalidOperationException("The target is not a Model Space block reference.");

            var bounds = reference.Bounds;
            if (!bounds.HasValue) throw new InvalidOperationException("The target has no extents.");
            return bounds.Value;
        }

        // World coordinates to the plane of the current view (its DCS), the standard transform of a view record.
        private static Matrix3d ToViewPlane(ViewTableRecord view)
        {
            var world = Matrix3d.PlaneToWorld(view.ViewDirection);
            world = Matrix3d.Displacement(view.Target - Point3d.Origin) * world;
            world = Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) * world;
            return world.Inverse();
        }

        private static IEnumerable<Point3d> CornersOf(Extents3d extents)
        {
            var a = extents.MinPoint;
            var b = extents.MaxPoint;
            yield return new Point3d(a.X, a.Y, a.Z);
            yield return new Point3d(b.X, a.Y, a.Z);
            yield return new Point3d(a.X, b.Y, a.Z);
            yield return new Point3d(b.X, b.Y, a.Z);
            yield return new Point3d(a.X, a.Y, b.Z);
            yield return new Point3d(b.X, a.Y, b.Z);
            yield return new Point3d(a.X, b.Y, b.Z);
            yield return new Point3d(b.X, b.Y, b.Z);
        }
    }
}
