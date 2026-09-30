using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;

namespace I52Auth15.HostHarness
{
    /// <summary>
    /// Harness-owned fixtures: plans, envelopes and their fingerprints (pure, AutoCAD-free) plus the few database
    /// helpers the cases share. Nothing here reads a rack, a catalog block or a blocks-library: the piece block the
    /// header-run plan refers to is created by the harness itself, in a committed setup transaction.
    /// </summary>
    internal static class Fixtures
    {
        public const string PieceBlock = "AUTH15HV_PIECE";
        public const string AbsentBlock = "AUTH15HV_ABSENT";
        public const string HeaderName = "AUTH15HV_HDR";

        // ---------------------------------------------------------------- envelopes

        /// <summary>A composed envelope, deterministic per <paramref name="n"/>. Its Design is long enough to need several
        /// 255-character chunks and carries quotes and non-ASCII text, which the serializer escapes.</summary>
        public static RackEmbedDocument Envelope(string kind, string name, string view, int n)
        {
            var pad = new StringBuilder();
            for (var i = 0; i < 900; i++)
            {
                pad.Append((char)('a' + (i % 26)));
            }

            return new RackEmbedDocument
            {
                Kind = kind,
                View = view,
                Section = -1,
                Id = "a15a15a1-0000-4000-8000-" + n.ToString("D12", CultureInfo.InvariantCulture),
                Name = name,
                Design = "{\"auth15\":\"host-validation\",\"seq\":" + n.ToString(CultureInfo.InvariantCulture)
                         + ",\"text\":\"Ñandú ✓ \\\"quoted\\\"\",\"pad\":\"" + pad + "\"}",
            };
        }

        // ---------------------------------------------------------------- header-run plans

        private static HeaderBlockInstance Piece(string blockName, string view, double x, double y)
        {
            var piece = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = "HV-PIECE",
                BlockName = blockName,
                View = view,
                Insertion = new Point2D(x, y),
                ConnectionAnchor = new Point2D(x, y),
            };
            return piece;
        }

        /// <summary>One header group (2 placements, one mirrored) plus loose pieces: a block, an annotation and a dimension.</summary>
        public static HeaderRunPlan HeaderRun(double secondPlacementX = 60.0, string headerName = HeaderName)
        {
            var header = new HeaderGroup(
                headerName,
                new List<HeaderBlockInstance> { Piece(PieceBlock, "frontal", 0.0, 0.0), Piece(PieceBlock, "frontal", 12.0, 0.0) },
                new List<HeaderPlacement> { new HeaderPlacement(0.0, false), new HeaderPlacement(secondPlacementX, true) });

            var annotation = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Annotation,
                Text = "HV-A",
                TextHeight = 3.0,
                Insertion = new Point2D(24.0, 12.0),
                View = "frontal",
            };

            var dimension = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Dimension,
                Insertion = new Point2D(0.0, -10.0),
                ConnectionAnchor = new Point2D(48.0, -10.0),
                DimensionOffset = -6.0,
                TextHeight = 3.0,
                View = "frontal",
            };

            return new HeaderRunPlan(
                new List<HeaderGroup> { header },
                new List<HeaderBlockInstance> { Piece(PieceBlock, "frontal", 30.0, 0.0), annotation, dimension });
        }

        /// <summary>The header group and one loose piece only: no annotation and no dimension (RB-02a isolates the native dimension
        /// residue from everything else the family creator does).</summary>
        public static HeaderRunPlan HeaderRunPlain()
        {
            var header = new HeaderGroup(
                HeaderName,
                new List<HeaderBlockInstance> { Piece(PieceBlock, "frontal", 0.0, 0.0), Piece(PieceBlock, "frontal", 12.0, 0.0) },
                new List<HeaderPlacement> { new HeaderPlacement(0.0, false), new HeaderPlacement(60.0, true) });

            return new HeaderRunPlan(new List<HeaderGroup> { header }, new List<HeaderBlockInstance> { Piece(PieceBlock, "frontal", 30.0, 0.0) });
        }

        /// <summary>A plan whose loose pieces name a block that is not in the drawing: two of them in the same view and one
        /// in another, so the distinct (BlockName|View) representatives are exactly two.</summary>
        public static HeaderRunPlan HeaderRunWithMissing(out List<HeaderBlockInstance> firstOfEachKey)
        {
            var presentPiece = Piece(PieceBlock, "frontal", 0.0, 0.0);
            var absentFrontalFirst = Piece(AbsentBlock, "frontal", 10.0, 0.0);
            var absentFrontalSecond = Piece(AbsentBlock, "frontal", 20.0, 0.0);
            var absentLateral = Piece(AbsentBlock, "lateral", 30.0, 0.0);

            firstOfEachKey = new List<HeaderBlockInstance> { absentFrontalFirst, absentLateral };

            return new HeaderRunPlan(
                new List<HeaderGroup>(),
                new List<HeaderBlockInstance> { presentPiece, absentFrontalFirst, absentFrontalSecond, absentLateral });
        }

        // ---------------------------------------------------------------- cantilever plan

        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        /// <summary>The frontal view of the reference line used by the Cantilever characterization tests, built through the
        /// PUBLIC assembler over the catalogs copied next to the Application assembly.</summary>
        public static CantileverViewPlan CantileverFrontal(string catalogDirectory)
        {
            var catalog = new CsvStructuralSectionCatalogProvider(catalogDirectory).Load();
            var computation = new CantileverLineEditorAssembler(catalog).Build(ReferenceLine());
            var plan = computation.Views.FirstOrDefault(v => v.View == CantileverViewKind.Frontal);

            if (plan == null || plan.Curves.Count == 0)
            {
                throw new InvalidOperationException("the reference line produced no frontal curves");
            }

            return plan;
        }

        private static CantileverLineDesign ReferenceLine()
        {
            var design = new CantileverLineDesign
            {
                Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                Name = "AUTH15 host validation",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = new CantileverLineStationTopologyDesign
                {
                    FaceMode = CantileverStationFaceMode.Single,
                    LevelCount = 2,
                    RequestedClearHeight = 24.0,
                    ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                    {
                        ColumnSectionId = ColumnW,
                        Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 },
                    },
                },
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5,
                    },
                },
            };

            var punches = design.StationTopology.ColumnBaseTemplate.Connection.Punches;
            punches.ColumnBottomPlateEndOffset = 1.5;
            punches.ColumnTopPunchOffset = 4.0;

            return design;
        }

        // ---------------------------------------------------------------- fingerprints

        public static string Sha256Hex(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

        private static string N(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static void Instance(StringBuilder sb, HeaderBlockInstance i)
        {
            sb.Append("I|").Append(i.Role).Append('|').Append(i.PieceId).Append('|').Append(i.BlockName).Append('|').Append(i.View)
              .Append('|').Append(N(i.Insertion.X)).Append(',').Append(N(i.Insertion.Y))
              .Append('|').Append(N(i.ConnectionAnchor.X)).Append(',').Append(N(i.ConnectionAnchor.Y))
              .Append('|').Append(N(i.RotationRadians)).Append('|').Append(i.MirroredX).Append(i.MirroredY)
              .Append('|').Append(i.Text).Append('|').Append(N(i.TextHeight)).Append('|').Append(N(i.DimensionOffset))
              .Append('|').Append(i.DimensionStyleName);

            foreach (var pair in i.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                sb.Append('|').Append(pair.Key).Append('=').Append(N(pair.Value));
            }

            sb.Append('\n');
        }

        public static string Fingerprint(HeaderRunPlan plan)
        {
            var sb = new StringBuilder();

            foreach (var group in plan.Headers)
            {
                sb.Append("G|").Append(group.Name).Append('\n');
                foreach (var i in group.Instances)
                {
                    Instance(sb, i);
                }

                foreach (var p in group.Placements)
                {
                    sb.Append("P|").Append(N(p.InsertionX)).Append(',').Append(N(p.InsertionY)).Append('|').Append(p.Mirrored).Append('\n');
                }
            }

            foreach (var i in plan.LooseInstances)
            {
                Instance(sb, i);
            }

            return Sha256Hex(sb.ToString());
        }

        public static string Fingerprint(CantileverViewPlan plan)
        {
            var sb = new StringBuilder(plan.Signature()).Append('\n');

            foreach (var curve in plan.Curves)
            {
                sb.Append(curve.Kind).Append('|').Append(curve.Role).Append('|').Append(curve.PieceId.Value).Append('|').Append(curve.IsClosed);
                sb.Append('|').Append(curve.CircleDiameter.HasValue ? N(curve.CircleDiameter.Value) : "-");
                foreach (var point in curve.Points)
                {
                    sb.Append('|').Append(N(point.X)).Append(',').Append(N(point.Y));
                }

                sb.Append('\n');
            }

            return Sha256Hex(sb.ToString());
        }

        /// <summary>Serialized bytes (what gets written) plus every field the envelope carries, so a field the serializer
        /// happens not to emit cannot change unnoticed.</summary>
        public static string Fingerprint(RackEmbedDocument envelope)
        {
            var fields = string.Join(
                "|", envelope.SchemaVersion, envelope.Kind, envelope.View, envelope.Section.ToString(CultureInfo.InvariantCulture),
                envelope.Id, envelope.Name, envelope.Design, envelope.CustomProperties.HasValue ? envelope.CustomProperties.Value.GetRawText() : "-",
                envelope.ExtensionData == null ? "-" : string.Join(",", envelope.ExtensionData.Keys.OrderBy(k => k, StringComparer.Ordinal)));
            return Sha256Hex(new RackEmbedStore().Serialize(envelope) + "\n" + fields);
        }

        // ---------------------------------------------------------------- database helpers

        /// <summary>Creates a small, committed block definition (one line). The setup is its own transaction; AUTH-15 is
        /// never involved.</summary>
        public static void CreateBlock(Database database, string name)
        {
            using (var tr = database.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(database.BlockTableId, OpenMode.ForWrite);
                var record = new BlockTableRecord { Name = name, Origin = Point3d.Origin };
                blockTable.Add(record);
                tr.AddNewlyCreatedDBObject(record, true);

                var line = new Line(Point3d.Origin, new Point3d(10.0, 0.0, 0.0));
                record.AppendEntity(line);
                tr.AddNewlyCreatedDBObject(line, true);

                tr.Commit();
            }
        }
    }
}
