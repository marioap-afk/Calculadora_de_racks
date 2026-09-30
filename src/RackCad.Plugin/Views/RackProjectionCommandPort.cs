using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// AutoCAD side of RACKPROYECTAR: it asks, reads once, prints and writes what the pure run tells it to, and decides
    /// nothing. Points come from <c>GetPoint</c> with <c>AllowNone = true</c> and are converted from the SCP to the universal
    /// system; OK, None, Cancel and Error travel unchanged to the run.
    /// </summary>
    internal sealed class AutoCadRackProjectionCommandPort : IRackProjectionCommandPort
    {
        private readonly Document document;
        private Autodesk.AutoCAD.Geometry.Point3d basePointInUcs;

        internal AutoCadRackProjectionCommandPort(Document document)
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            WriteScopes = new AutoCadProjectionWriteScopeFactory(document);
        }

        public IRackProjectionWriteScopeFactory WriteScopes { get; }

        public RackProjectionSnapshot Capture()
        {
            var editor = document.Editor;

            // ACQUIRE: the selection and the class of the projected views, F -> L -> P (OD-5).
            var selection = editor.GetSelection(new PromptSelectionOptions
            {
                MessageForAdding = "\nSelecciona las vistas de rack a proyectar: "
            });
            if (selection.Status != PromptStatus.OK)
            {
                return RackProjectionSnapshot.Unavailable(RackProjectionSnapshotFailure.NoSelection, null);
            }

            // The first argument is the message WITH its bracketed keyword list; the second is the global keyword list.
            // The bare message throws "No bracketed keyword list" (found in the first Owner run of RACKPROYECTAR).
            var keywords = new PromptKeywordOptions("\nClase de vista a proyectar [Frontal/Lateral/Planta]", "Frontal Lateral Planta")
            {
                AllowNone = false,
            };
            var kind = editor.GetKeywords(keywords);
            if (kind.Status != PromptStatus.OK)
            {
                return RackProjectionSnapshot.Unavailable(RackProjectionSnapshotFailure.NoSelection, null);
            }

            // G16 C16-06: the orientation, after the class and before any read or point. Enter takes the product default
            // (Proyectada). Same keyword pattern as RACKCAMA (Keywords.Add + Default + AllowNone): AutoCAD appends the list. The two
            // words of the Owner start with the same letter, so their shortcuts are PR and PRE (the capitals of each keyword).
            var orientation = new PromptKeywordOptions("\nOrientacion") { AllowNone = true };
            orientation.Keywords.Add(ProjectedKeyword);
            orientation.Keywords.Add(CanonicalKeyword);
            orientation.Keywords.Default = ProjectedKeyword;
            var mode = editor.GetKeywords(orientation);
            if (mode.Status != PromptStatus.OK && mode.Status != PromptStatus.None)
            {
                return RackProjectionSnapshot.Unavailable(RackProjectionSnapshotFailure.Cancelled, null);
            }

            // SNAPSHOT: the one read.
            return RackProjectionSnapshotReader.Read(
                document, selection.Value.GetObjectIds(), KindOf(kind.StringResult), OrientationOf(mode));
        }

        public void Report(IReadOnlyList<string> lines)
        {
            foreach (var line in lines)
            {
                document.Editor.WriteMessage("\n" + line);
            }
        }

        public void BeforeWrite() => RackUnitsGuard.WarnIfNotInches(document);

        public RackProjectionPick PickBasePoint()
        {
            var result = document.Editor.GetPoint(new PromptPointOptions("\nPunto base: ") { AllowNone = true });
            if (result.Status == PromptStatus.OK) basePointInUcs = result.Value;
            return Convert(result);
        }

        public RackProjectionPick PickTargetPoint(Point3D basePoint)
            => Convert(document.Editor.GetPoint(new PromptPointOptions("\nPunto de destino: ")
            {
                AllowNone = true,
                UseBasePoint = true,
                BasePoint = basePointInUcs,
            }));

        /// <summary>
        /// Imports the library blocks of the accepted plan into the drawing, OUTSIDE any transaction and under the document
        /// lock, and observes the drawing afterwards through the supported query. It writes no rack.
        /// </summary>
        public RackProjectionLibraryObservation ImportAndObserve(IReadOnlyList<LibraryBlockRequirement> requirements)
        {
            using (document.LockDocument())
            {
                var flow = LibraryBlockAvailabilityFlow.Observe(
                    requirements,
                    allowImport: true,
                    new AutoCadLibraryBlockQuery(document.Database),
                    new Importer(document.Database));
                return new RackProjectionLibraryObservation(flow.Facts, !File.Exists(BlockLibraryImporter.LibraryPath));
            }
        }

        private RackProjectionPick Convert(PromptPointResult result)
        {
            switch (result.Status)
            {
                case PromptStatus.OK:
                    var world = result.Value.TransformBy(document.Editor.CurrentUserCoordinateSystem);
                    return RackProjectionPick.Ok(new Point3D(world.X, world.Y, world.Z));
                case PromptStatus.None:
                    return RackProjectionPick.None();
                case PromptStatus.Cancel:
                    return RackProjectionPick.Cancel();
                default:
                    return RackProjectionPick.Error("PROMPT_STATUS_" + result.Status);
            }
        }

        private const string ProjectedKeyword = "PRoyectada";
        private const string CanonicalKeyword = "PREdeterminada";

        /// <summary>Enter (None) and «Proyectada» are the product default; only «Predeterminada» selects the canonical mode.</summary>
        private static RackProjectionOrientationMode OrientationOf(PromptResult result)
            => result.Status == PromptStatus.OK
               && string.Equals(result.StringResult, CanonicalKeyword, StringComparison.OrdinalIgnoreCase)
                ? RackProjectionOrientationMode.Canonical
                : RackProjectionOrientationMode.Projected;

        private static DimensionViewKind KindOf(string keyword)
        {
            switch (keyword)
            {
                case "Lateral": return DimensionViewKind.Lateral;
                case "Planta": return DimensionViewKind.Planta;
                default: return DimensionViewKind.Frontal;
            }
        }

        private sealed class Importer : ILibraryBlockImporter
        {
            private readonly Database database;

            internal Importer(Database database) => this.database = database;

            public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
                => new LibraryBlockImportResult(true, BlockLibraryImporter.EnsureRequirements(database, requirements));
        }
    }
}
