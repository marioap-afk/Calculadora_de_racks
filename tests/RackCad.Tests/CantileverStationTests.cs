using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The Cantilever STATION (I-37C): single and double gondolas, shared levels, the clear-height layout, the
    /// column height and the component BOM.
    ///
    /// Against the SHIPPED catalogue and through the real I-37A and I-37B resolvers, because the whole point of
    /// the initiative is that it COMPOSES them. A station tested against hand-made sub-assemblies would prove
    /// the wrong thing.
    /// </summary>
    public class CantileverStationTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string ArmDeep = "AISC-W-W10X33";
        private const string ArmChannel = "AISC-C-C10X15_3";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const double Tolerance = 1e-9;

        private static StructuralSectionId Id(string value) => StructuralSectionId.Parse(value);

        // ---- fixture -----------------------------------------------------------------------------------

        private static CantileverColumnBaseSectionPolicy ColumnBasePolicy() =>
            CantileverColumnBaseSectionPolicy.Create(
                new[]
                {
                    new CantileverColumnBaseVariant(
                        CantileverColumnBaseVariantKind.WFlangeConnected, Id(ColumnW), Id(BaseW))
                },
                new[] { StructuralSectionFamily.W });

        private static CantileverArmSectionPolicy ArmPolicy() =>
            CantileverArmSectionPolicy.Create(new[]
            {
                new CantileverArmVariant(Id(ArmHss), CantileverArmBodyArrangement.Single),
                new CantileverArmVariant(Id(ArmDeep), CantileverArmBodyArrangement.Single),
                new CantileverArmVariant(Id(ArmChannel), CantileverArmBodyArrangement.Single),
                new CantileverArmVariant(Id(ArmChannel), CantileverArmBodyArrangement.DoubleChannelFacing),
                new CantileverArmVariant(Id(ArmChannel), CantileverArmBodyArrangement.DoubleChannelBackToBack)
            });

        private static CantileverArmTemplateDesign ArmTemplate(
            string section = ArmHss,
            CantileverArmBodyArrangement arrangement = CantileverArmBodyArrangement.Single,
            double cutLength = 36.0,
            double slope = 0.0,
            int count = 2,
            double? offset = 1.5,
            CantileverArmEndPlateMode endMode = CantileverArmEndPlateMode.None,
            double extraStop = 0.0) =>
            new CantileverArmTemplateDesign
            {
                Body = new CantileverArmBodyDesign
                {
                    Arrangement = arrangement,
                    SectionId = section,
                    CutLength = cutLength,
                    SlopeRisePer12 = slope
                },
                MountingPlate = new CantileverArmMountingPlateTemplateDesign
                {
                    VerticalPunchCount = count,
                    VerticalEndOffset = offset
                },
                EndPlate = new CantileverArmEndPlateDesign
                {
                    Mode = endMode,
                    ExtraStopHeight = extraStop
                }
            };

        private static CantileverStationDesign Design(
            CantileverStationFaceMode faceMode = CantileverStationFaceMode.Single,
            CantileverArmSide singleSide = CantileverArmSide.PositiveY,
            int levels = 3,
            int firstIndex = 0,
            double clear = 24.0,
            double topFactor = 1.0 / 3.0,
            CantileverArmTemplateDesign defaultArm = null,
            CantileverStationColumnHeightMode heightMode = CantileverStationColumnHeightMode.Automatic,
            double? manualHeight = null)
        {
            var design = new CantileverStationDesign
            {
                FaceMode = faceMode,
                SingleSide = singleSide,
                FirstLevelPunchIndex = firstIndex,
                RequestedClearHeight = clear,
                TopClearFactor = topFactor,
                DefaultArmTemplate = defaultArm ?? ArmTemplate(),
                ColumnHeight = new CantileverStationColumnHeightDesign
                {
                    Mode = heightMode,
                    ManualHeight = manualHeight
                },
                Levels = Enumerable.Range(0, levels)
                    .Select(_ => new CantileverStationLevelDesign())
                    .ToList()
            };

            design.ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = ColumnW,
                Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
            };
            design.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            design.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;

            return design;
        }

        private static CantileverStationAssembly Resolve(CantileverStationDesign design = null) =>
            new CantileverStationResolver(Catalog, Factory, ColumnBasePolicy(), ArmPolicy())
                .Resolve(design ?? Design());

        private static bool Has(CantileverStationAssembly s, string code) =>
            s.Diagnostics.Any(d => d.Code == code);

        // ---- 0. the fixture resolves at all ------------------------------------------------------------

        [Fact]
        public void TheFixtureResolves()
        {
            var station = Resolve();

            Assert.False(
                station.IsBlocked,
                "El fixture debe resolver. Diagnosticos: " +
                string.Join(" | ", station.Diagnostics.Select(d => d.Code + ": " + d.Message)));

            Assert.NotNull(station.ColumnBase);
            Assert.Equal(3, station.Levels.Count);
        }

        // ---- 1. the single gondola ---------------------------------------------------------------------

        [Theory]
        [InlineData(CantileverArmSide.PositiveY)]
        [InlineData(CantileverArmSide.NegativeY)]
        public void ASingleStationHasOneColumnOneBaseAndOneArmPerLevel(CantileverArmSide side)
        {
            var station = Resolve(Design(singleSide: side));

            Assert.False(station.IsBlocked);
            Assert.Single(station.ColumnBase.Sides);
            Assert.Equal(side, station.ColumnBase.Sides[0].Side);
            Assert.Equal(side, station.SingleSide);

            Assert.Equal(3, station.Levels.Count);
            Assert.All(station.Levels, l => Assert.Single(l.Arms));
            Assert.All(station.Arms, a => Assert.Equal(side, a.Side));

            // One column member, plus one base per side, plus one member per arm.
            Assert.Single(station.Members.Where(m => m.Role == CantileverMemberRole.Column));
            Assert.Single(station.Members.Where(m => m.Role == CantileverMemberRole.Base));
        }

        [Theory]
        [InlineData(CantileverArmSide.PositiveY)]
        [InlineData(CantileverArmSide.NegativeY)]
        public void TheINACTIVESideOfASingleStationDoesNotEXIST(CantileverArmSide active)
        {
            var station = Resolve(Design(singleSide: active));
            var inactive = active == CantileverArmSide.PositiveY
                ? CantileverArmSide.NegativeY
                : CantileverArmSide.PositiveY;

            // Not an empty cell and not a disabled one: absent.
            Assert.DoesNotContain(inactive, station.ActiveSides);
            Assert.Empty(station.ColumnBase.Sides.Where(s => s.Side == inactive));
            Assert.Empty(station.Arms.Where(a => a.Side == inactive));
            Assert.All(station.Levels, l => Assert.Null(l.Arm(inactive)));
        }

        [Fact]
        public void ASingleStationOnTheNegativeSideIsTheMirrorOfThePositiveOne()
        {
            var positive = Resolve(Design(singleSide: CantileverArmSide.PositiveY));
            var negative = Resolve(Design(singleSide: CantileverArmSide.NegativeY));

            var p = positive.ColumnBase.Sides[0];
            var n = negative.ColumnBase.Sides[0];

            // Same piece, opposite side: every dimension survives the mirror.
            Assert.Equal(p.Member.GeometricLength, n.Member.GeometricLength, 12);
            Assert.Equal(p.FrontPlate.Thickness, n.FrontPlate.Thickness, 12);
            Assert.Equal(p.Gusset.VerticalLeg, n.Gusset.VerticalLeg, 12);
            Assert.Equal(p.RearPlatePunches.Count, n.RearPlatePunches.Count);

            // And it really is on the other side.
            Assert.True(p.Member.End.Y > Tolerance);
            Assert.True(n.Member.End.Y < -Tolerance);
        }

        // ---- 2. the double gondola --------------------------------------------------------------------

        [Fact]
        public void ADoubleStationHasONEColumnONEBottomPlateAndTWOBases()
        {
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double));

            Assert.False(station.IsBlocked);

            // THE invariant of the whole initiative.
            Assert.Single(station.Members.Where(m => m.Role == CantileverMemberRole.Column));
            Assert.Single(station.ColumnBase.Plates.Where(p => p.Kind == CantileverPlateKind.ColumnBottom));

            Assert.Equal(2, station.ColumnBase.Sides.Count);
            Assert.Equal(2, station.Members.Count(m => m.Role == CantileverMemberRole.Base));
            Assert.Equal(
                new[] { CantileverArmSide.PositiveY, CantileverArmSide.NegativeY },
                station.ColumnBase.Sides.Select(s => s.Side));
        }

        [Fact]
        public void TheTwoBasesOfADoubleStationAreSYMMETRIC()
        {
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double));
            var positive = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.PositiveY);
            var negative = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.NegativeY);

            // Same configuration on both sides, by construction: one resolve behind both.
            Assert.Equal(positive.Member.SectionId.Value, negative.Member.SectionId.Value);
            Assert.Equal(positive.Member.GeometricLength, negative.Member.GeometricLength, 12);
            Assert.Equal(positive.RearPlate.Thickness, negative.RearPlate.Thickness, 12);

            // Mirrored in Y, identical in X and Z.
            Assert.Equal(positive.Member.End.Y, -negative.Member.End.Y, 12);
            Assert.Equal(positive.Member.End.X, negative.Member.End.X, 12);
            Assert.Equal(positive.Member.End.Z, negative.Member.End.Z, 12);

            var pe = positive.Envelope();
            var ne = negative.Envelope();

            Assert.Equal(pe.MaxY, -ne.MinY, 9);
            Assert.Equal(pe.MinY, -ne.MaxY, 9);
            Assert.Equal(pe.MinZ, ne.MinZ, 9);
            Assert.Equal(pe.MaxZ, ne.MaxZ, 9);
        }

        [Fact]
        public void TheTwoREARPlatesOfADoubleStationShareTheirDATUMS()
        {
            // One bolt through the negative plate, the column and the positive plate is ONE logical hole. The
            // datum excludes the coordinate along its own axis precisely so this comes out true.
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double));
            var positive = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.PositiveY);
            var negative = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.NegativeY);

            Assert.Equal(
                positive.RearPlatePunches.Select(p => p.Datum).ToList(),
                negative.RearPlatePunches.Select(p => p.Datum).ToList());

            // The CENTRES differ, because the plates are on opposite faces. That is the point of the datum.
            Assert.All(
                positive.RearPlatePunches.Zip(negative.RearPlatePunches, (a, b) => (a, b)),
                pair => Assert.Equal(pair.a.Centre.Y, -pair.b.Centre.Y, 12));
        }

        [Fact]
        public void ADoubleStationHasTWOArmsPerLevelAtTheSAMEIndex()
        {
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));

            Assert.Equal(3, station.Levels.Count);
            Assert.All(station.Levels, l => Assert.Equal(2, l.Arms.Count));
            Assert.Equal(6, station.Arms.Count);

            foreach (var level in station.Levels)
            {
                var indices = level.Arms.Select(a => a.ConnectionPattern.LowerColumnPunchIndex).Distinct();
                Assert.Single(indices);

                var elevations = level.Arms.Select(a => a.ConnectionPattern.FirstElevation).Distinct();
                Assert.Single(elevations);
            }
        }

        [Fact]
        public void TheLevelsAreSHAREDBetweenBothFaces()
        {
            var single = Resolve(Design(faceMode: CantileverStationFaceMode.Single, levels: 3));
            var doubled = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));

            // Identical arms on both faces means identical geometry, so the indices must match the single case.
            Assert.Equal(
                single.Levels.Select(l => l.LowerPunchIndex),
                doubled.Levels.Select(l => l.LowerPunchIndex));
        }

        [Fact]
        public void EveryPieceIdOfADoubleStationIsDistinct()
        {
            var station = Resolve(Design(
                faceMode: CantileverStationFaceMode.Double,
                defaultArm: ArmTemplate(endMode: CantileverArmEndPlateMode.Cap)));

            var ids = station.Members.Select(m => m.Id.Value)
                .Concat(station.Plates.Select(p => p.Id.Value))
                .Concat(station.Gussets.Select(g => g.Id.Value))
                .Concat(station.Punches.Select(p => p.Id.Value))
                .ToList();

            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        }

        // ---- 3. the levels ----------------------------------------------------------------------------

        [Fact]
        public void AStationNeedsAtLeastOneLevel()
        {
            var design = Design();
            design.Levels.Clear();

            var station = Resolve(design);

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationNoLevels));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void TheFirstLevelUsesTheRequestedIndexEXACTLY(int firstIndex)
        {
            var station = Resolve(Design(firstIndex: firstIndex, levels: 2));

            Assert.False(station.IsBlocked);
            Assert.Equal(firstIndex, station.Levels[0].LowerPunchIndex);
            Assert.Equal(
                firstIndex,
                station.Levels[0].Arms[0].ConnectionPattern.LowerColumnPunchIndex);
        }

        [Fact]
        public void TheCLEARIsExactWhenItLandsOnAPunch()
        {
            // 24 in of clear with a 4 in pitch and a 4 in deep arm lands exactly on a punch, so the actual
            // clear is the requested one and not a rounded-up approximation of it.
            var station = Resolve(Design(clear: 24.0, levels: 3));

            Assert.False(station.IsBlocked);
            Assert.Equal(new[] { 0, 7, 14 }, station.Levels.Select(l => l.LowerPunchIndex));

            foreach (var level in station.Levels.Skip(1))
            {
                Assert.Equal(24.0, level.Plan.GoverningClear, 9);
            }
        }

        [Theory]
        [InlineData(22.0)]
        [InlineData(23.0)]
        [InlineData(25.0)]
        public void TheCLEARIsAdjustedUPWARDSAndNeverDown(double requested)
        {
            var station = Resolve(Design(clear: requested, levels: 3));

            Assert.False(station.IsBlocked);

            foreach (var level in station.Levels.Skip(1))
            {
                // Never below what was asked for, and never more than one pitch above it — the FIRST index
                // that fits, not a comfortable one.
                Assert.True(
                    level.Plan.GoverningClear >= requested - Tolerance,
                    "El claro real " + level.Plan.GoverningClear + " quedo por debajo del pedido " + requested);
                Assert.True(level.Plan.GoverningClear < requested + 4.0);
            }
        }

        [Fact]
        public void ThePLATESOfTwoLevelsNeverOverlap()
        {
            // The fixture is chosen so the PLATE rule is the ONLY binding one, which is what makes this test
            // discriminating. With a 4 in pitch, a 4 in deep body and a margin of 3 in, the plate spans
            // e(k)-3 .. e(k)+7 while the body only reaches e(k)+1. A 4 in clear and the punch-disjointness
            // rule are both satisfied by a gap of TWO indices; only the plate rule needs THREE.
            //
            // An earlier version of this test used a 1.5 in margin, and it passed with the plate check
            // disabled: for any margin below 2 in the punch rule already forces the same gap, so the fixture
            // proved nothing. The regression round is what exposed that.
            var station = Resolve(Design(
                clear: 4.0, levels: 3, defaultArm: ArmTemplate(count: 2, offset: 3.0)));

            Assert.False(station.IsBlocked);

            for (var i = 1; i < station.Levels.Count; i++)
            {
                foreach (var side in station.ActiveSides)
                {
                    var below = station.Levels[i - 1].Plan.Cell(side);
                    var above = station.Levels[i].Plan.Cell(side);

                    Assert.True(
                        above.PlateBottomZ >= below.PlateTopZ - Tolerance,
                        "Las placas de los niveles " + i + " y " + (i + 1) + " se traslapan: " +
                        above.PlateBottomZ + " < " + below.PlateTopZ);

                    // THREE indices, not two: the plate is what moved the level.
                    Assert.Equal(3, above.LowerPunchIndex - below.LowerPunchIndex);
                }
            }
        }

        [Fact]
        public void TwoLevelsNeverSHAREAPunchDatum()
        {
            var station = Resolve(Design(
                faceMode: CantileverStationFaceMode.Double,
                clear: 6.0, levels: 4, defaultArm: ArmTemplate(count: 3)));

            Assert.False(station.IsBlocked);

            var used = new List<HashSet<int>>();

            foreach (var level in station.Levels)
            {
                var indices = new HashSet<int>();

                foreach (var cell in level.Plan.Cells)
                {
                    for (var i = cell.LowerPunchIndex; i <= cell.UpperPunchIndex; i++)
                    {
                        indices.Add(i);
                    }
                }

                Assert.All(used, previous => Assert.Empty(previous.Intersect(indices)));
                used.Add(indices);
            }

            // And the datums themselves are disjoint, which is the physical statement.
            var datums = station.Arms.SelectMany(a => a.ConnectionPattern.SelectedDatums).ToList();
            var byLevel = station.Levels
                .Select(l => l.Arms.SelectMany(a => a.ConnectionPattern.Elevations).Distinct().ToList())
                .ToList();

            for (var i = 1; i < byLevel.Count; i++)
            {
                Assert.Empty(byLevel[i - 1].Intersect(byLevel[i]));
            }

            Assert.NotEmpty(datums);
        }

        [Fact]
        public void ADeeperArmInONECellPushesTheNextLevelUp()
        {
            var shallow = Resolve(Design(levels: 2, clear: 12.0));

            var design = Design(levels: 2, clear: 12.0);
            design.Levels[0].SetOverride(
                CantileverArmSide.PositiveY, ArmTemplate(section: ArmDeep, count: 4));

            var deep = Resolve(design);

            Assert.False(shallow.IsBlocked);
            Assert.False(deep.IsBlocked);

            // The layout used the EFFECTIVE arm of the cell, not the default.
            Assert.True(
                deep.Levels[1].LowerPunchIndex > shallow.Levels[1].LowerPunchIndex,
                "Un brazo mas aperaltado en el nivel inferior debe empujar el siguiente hacia arriba.");
        }

        [Fact]
        public void TheMOSTRESTRICTIVESideGovernsInADoubleStation()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 2, clear: 12.0);
            design.Levels[0].SetOverride(
                CantileverArmSide.NegativeY, ArmTemplate(section: ArmDeep, count: 4));

            var station = Resolve(design);

            Assert.False(station.IsBlocked);

            var plan = station.Levels[1].Plan;

            // Both sides are reported, and the governing clear is the SMALLEST of them.
            Assert.Equal(2, plan.ClearBySide.Count);
            Assert.Equal(plan.ClearBySide.Values.Min(), plan.GoverningClear, 12);

            // And both satisfy the request, which is what "both must hold" means.
            Assert.All(plan.ClearBySide.Values, c => Assert.True(c >= 12.0 - Tolerance));

            // The deep side is the tighter one: it is what moved the level.
            var symmetric = Resolve(Design(
                faceMode: CantileverStationFaceMode.Double, levels: 2, clear: 12.0));
            Assert.True(station.Levels[1].LowerPunchIndex > symmetric.Levels[1].LowerPunchIndex);
        }

        [Fact]
        public void TheREALClearsAreReportedPerSide()
        {
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));

            Assert.Empty(station.Levels[0].Plan.ClearBySide);
            Assert.True(double.IsNaN(station.Levels[0].Plan.GoverningClear));

            foreach (var level in station.Levels.Skip(1))
            {
                Assert.Equal(
                    new[] { CantileverArmSide.PositiveY, CantileverArmSide.NegativeY },
                    level.Plan.ClearBySide.Keys.OrderBy(k => k));
            }
        }

        [Fact]
        public void AClearThatNoIndexCanSatisfyIsREJECTED()
        {
            // What makes a clear impossible is now the grid's DOMAIN, not an arbitrary candidate cap. The
            // earlier version of this test used 10^6 in and passed only because the search gave up after 250
            // candidates — a limit the review removed, because a valid level 300 indices up is still valid.
            //
            // 10^10 in needs an index past MaxDefinedIndex, which is where the grid genuinely stops being
            // defined, so this rejection is a statement about arithmetic rather than about taste.
            var station = Resolve(Design(clear: 1.0e10, levels: 2));

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationLevelDoesNotFit));
        }

        [Fact]
        public void AClearWhoseFirstValidIndexIsFarUPTheColumnStillRESOLVES()
        {
            // The regression the 250-candidate cap caused. With a 4 in pitch and a 4 in body, a clear of
            // 1 200 in puts the second level 301 indices above the first — inside the grid, well past the old
            // limit, and a perfectly ordinary tall rack.
            var station = Resolve(Design(clear: 1200.0, levels: 2));

            Assert.False(
                station.IsBlocked,
                "Un nivel 301 indices mas arriba es valido: " +
                string.Join(" | ", station.Diagnostics.Select(d => d.Code)));

            var gap = station.Levels[1].LowerPunchIndex - station.Levels[0].LowerPunchIndex;

            Assert.True(gap > 250, "El hueco deberia superar el viejo tope de 250; fue " + gap + ".");
            Assert.True(station.Levels[1].Plan.GoverningClear >= 1200.0 - Tolerance);
        }

        // ---- 4. the matrix ----------------------------------------------------------------------------

        [Fact]
        public void EveryCellStartsOnTheDEFAULT()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);

            Assert.Equal(6, matrix.Cells.Count);
            Assert.Equal(0, matrix.OverrideCount);
            Assert.All(matrix.Cells, c => Assert.False(matrix.HasOverride(c)));
            Assert.All(matrix.Cells, c => Assert.Same(design.DefaultArmTemplate, matrix.Effective(c)));
        }

        [Fact]
        public void ASingleStationHasNoFALSECellForTheInactiveSide()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Single, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);

            Assert.Equal(3, matrix.Cells.Count);
            Assert.All(matrix.Cells, c => Assert.Equal(CantileverArmSide.PositiveY, c.Side));
            Assert.False(matrix.IsActive(new CantileverStationCell(0, CantileverArmSide.NegativeY)));

            // And a level operation touches ONE cell, not two.
            var change = matrix.Apply(
                CantileverStationApplyScope.Level,
                new CantileverStationCell(1, CantileverArmSide.PositiveY),
                ArmTemplate(section: ArmDeep));

            Assert.Single(change.Touched);
            Assert.Null(design.Levels[1].NegativeYOverride);
        }

        [Fact]
        public void ACellOperationTouchesEXACTLYOneCell()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);
            var cell = new CantileverStationCell(1, CantileverArmSide.NegativeY);

            var change = matrix.Apply(CantileverStationApplyScope.Cell, cell, ArmTemplate(section: ArmDeep));

            Assert.Single(change.Touched);
            Assert.Equal(cell, change.Touched[0]);
            Assert.Equal(1, matrix.OverrideCount);
            Assert.True(matrix.HasOverride(cell));
            Assert.False(matrix.HasOverride(new CantileverStationCell(1, CantileverArmSide.PositiveY)));
        }

        [Fact]
        public void ALevelOperationOnADoubleStationTouchesBOTHSides()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);

            var change = matrix.Apply(
                CantileverStationApplyScope.Level,
                new CantileverStationCell(2, CantileverArmSide.PositiveY),
                ArmTemplate(section: ArmDeep));

            Assert.Equal(2, change.Touched.Count);
            Assert.All(change.Touched, c => Assert.Equal(2, c.LevelIndex));
            Assert.Equal(2, matrix.OverrideCount);
        }

        [Fact]
        public void AStationOperationTouchesEveryActiveCell()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 4);
            var matrix = new CantileverStationArmMatrix(design);

            var change = matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                ArmTemplate(section: ArmDeep));

            Assert.Equal(8, change.Touched.Count);
            Assert.Equal(8, matrix.OverrideCount);
        }

        [Fact]
        public void ApplyingAScopeProducesONEAggregateResult()
        {
            // One result and not N notifications: the user made one edit.
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 5);
            var matrix = new CantileverStationArmMatrix(design);

            var change = matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                ArmTemplate());

            Assert.Equal(CantileverStationApplyScope.Station, change.Scope);
            Assert.Equal(10, change.Count);
            Assert.Equal(change.Count, change.Touched.Count);
        }

        [Theory]
        [InlineData(CantileverStationApplyScope.Cell, 1)]
        [InlineData(CantileverStationApplyScope.Level, 2)]
        [InlineData(CantileverStationApplyScope.Station, 6)]
        public void RestoringClearsTheOverrideInsteadOfCopyingTheDefault(
            CantileverStationApplyScope scope, int expected)
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);
            var anchor = new CantileverStationCell(1, CantileverArmSide.PositiveY);

            matrix.Apply(CantileverStationApplyScope.Station, anchor, ArmTemplate(section: ArmDeep));
            Assert.Equal(6, matrix.OverrideCount);

            var change = matrix.Restore(scope, anchor);

            Assert.Equal(expected, change.Count);
            Assert.Equal(6 - expected, matrix.OverrideCount);

            // A restored cell FOLLOWS the default; it does not hold a copy of it.
            foreach (var cell in change.Touched)
            {
                Assert.False(matrix.HasOverride(cell));
                Assert.Same(design.DefaultArmTemplate, matrix.Effective(cell));
            }
        }

        [Fact]
        public void AnOverrideIsADEEPCopyPerCell()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 2);
            var matrix = new CantileverStationArmMatrix(design);
            var template = ArmTemplate(section: ArmDeep);

            matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                template);

            var a = matrix.Effective(new CantileverStationCell(0, CantileverArmSide.PositiveY));
            var b = matrix.Effective(new CantileverStationCell(1, CantileverArmSide.NegativeY));

            Assert.NotSame(a, b);
            Assert.NotSame(template, a);

            a.Body.CutLength = 999.0;
            Assert.NotEqual(999.0, b.Body.CutLength);
        }

        [Fact]
        public void NoMatrixOperationChangesTheTOPOLOGY()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3, clear: 24.0);
            var before = Resolve(design.DeepCopy());

            var matrix = new CantileverStationArmMatrix(design);
            matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                ArmTemplate());
            matrix.Restore(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY));

            var after = Resolve(design);

            Assert.Equal(3, design.LevelCount);
            Assert.Equal(24.0, design.RequestedClearHeight);
            Assert.Equal(CantileverStationFaceMode.Double, design.FaceMode);
            Assert.Equal(
                before.Levels.Select(l => l.LowerPunchIndex),
                after.Levels.Select(l => l.LowerPunchIndex));
            Assert.Equal(before.Signature(), after.Signature());
        }

        [Theory]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndeclaredScopeIsRejected(int raw)
        {
            var matrix = new CantileverStationArmMatrix(Design());

            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.InScope(
                (CantileverStationApplyScope)raw,
                new CantileverStationCell(0, CantileverArmSide.PositiveY)));
        }

        // ---- 5. the column height ---------------------------------------------------------------------

        [Fact]
        public void AnAUTOMATICHeightIsExactlyTheMinimum()
        {
            var station = Resolve(Design(heightMode: CantileverStationColumnHeightMode.Automatic));

            Assert.False(station.IsBlocked);
            Assert.Equal(station.MinimumColumnHeight, station.ResolvedColumnHeight, 12);
            Assert.Equal(station.ResolvedColumnHeight, station.ColumnBase.ColumnHeight, 12);
        }

        [Fact]
        public void TheDefaultTopFactorIsONETHIRD()
        {
            Assert.Equal(1.0 / 3.0, CantileverStationDefaults.TopClearFactor, 12);
            Assert.Equal(1.0 / 3.0, new CantileverStationDesign().TopClearFactor, 12);
        }

        [Fact]
        public void ALargerTopFactorMakesATallerColumn()
        {
            var third = Resolve(Design(topFactor: 1.0 / 3.0));
            var half = Resolve(Design(topFactor: 0.5));

            Assert.False(half.IsBlocked);
            Assert.True(half.MinimumColumnHeight > third.MinimumColumnHeight + Tolerance);
        }

        [Theory]
        [InlineData(0.25)]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        public void ATopFactorBelowONETHIRDIsREJECTED(double factor)
        {
            var station = Resolve(Design(topFactor: factor));

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationTopClearFactorTooSmall));
        }

        [Fact]
        public void TheMinimumHeightCountsTheLastBODYTheLastPLATEAndTheLastPUNCH()
        {
            var station = Resolve(Design(levels: 3, clear: 24.0));
            var top = station.Levels[station.Levels.Count - 1].Plan;

            var requestedTopClear = 24.0 / 3.0;
            var byOccupation = top.OccupiedTopZ + requestedTopClear;

            // The plate reaches above the body here, so the occupation is the PLATE — which is exactly the
            // term a naive "body + margin" would miss.
            Assert.True(top.Cells.Max(c => c.PlateTopZ) > top.Cells.Max(c => c.BodyTopZ) + Tolerance);
            Assert.Equal(top.Cells.Max(c => c.PlateTopZ), top.OccupiedTopZ, 12);

            // And the punch requirement is the other term of the max.
            var highest = station.Arms.SelectMany(a => a.ConnectionPattern.Elevations).Max();
            var byPunch = highest + 4.0;

            Assert.Equal(Math.Max(byOccupation, byPunch), station.MinimumColumnHeight, 9);
        }

        [Fact]
        public void TheCapAndTheStopDoNOTCountTowardsTheColumnHeight()
        {
            // They are out at the free end, not in the connection plane.
            var none = Resolve(Design(defaultArm: ArmTemplate(endMode: CantileverArmEndPlateMode.None)));
            var cap = Resolve(Design(defaultArm: ArmTemplate(endMode: CantileverArmEndPlateMode.Cap)));
            var stop = Resolve(Design(defaultArm: ArmTemplate(
                endMode: CantileverArmEndPlateMode.Stop, extraStop: 12.0)));

            Assert.Equal(none.MinimumColumnHeight, cap.MinimumColumnHeight, 12);
            Assert.Equal(none.MinimumColumnHeight, stop.MinimumColumnHeight, 12);
        }

        [Fact]
        public void AValidMANUALHeightIsHonouredAndMovesNothing()
        {
            var automatic = Resolve(Design());
            var taller = automatic.MinimumColumnHeight + 24.0;

            var station = Resolve(Design(
                heightMode: CantileverStationColumnHeightMode.Manual, manualHeight: taller));

            Assert.False(station.IsBlocked);
            Assert.Equal(taller, station.ResolvedColumnHeight, 12);
            Assert.Equal(automatic.MinimumColumnHeight, station.MinimumColumnHeight, 12);

            // The levels did not move.
            Assert.Equal(
                automatic.Levels.Select(l => l.LowerPunchIndex),
                station.Levels.Select(l => l.LowerPunchIndex));
        }

        [Fact]
        public void AnINSUFFICIENTManualHeightBlocksInsteadOfBeingNormalised()
        {
            var automatic = Resolve(Design());
            var tooShort = automatic.MinimumColumnHeight - 6.0;

            var station = Resolve(Design(
                heightMode: CantileverStationColumnHeightMode.Manual, manualHeight: tooShort));

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationManualHeightBelowMinimum));

            // Nothing was built: no trimmed levels, no moved arms, no reduced clear.
            Assert.Null(station.ColumnBase);
            Assert.Empty(station.Levels);
            Assert.Equal(0.0, station.ResolvedColumnHeight);
        }

        [Fact]
        public void AManualHeightIsMANDATORYInManualMode()
        {
            var station = Resolve(Design(
                heightMode: CantileverStationColumnHeightMode.Manual, manualHeight: null));

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationManualHeightMissing));
        }

        [Fact]
        public void UnderAutomaticTheManualHeightIsDORMANTData()
        {
            // A value left behind by an earlier edit must not reject a station that no longer reads it.
            var station = Resolve(Design(
                heightMode: CantileverStationColumnHeightMode.Automatic, manualHeight: 1.0));

            Assert.False(station.IsBlocked);
            Assert.Equal(station.MinimumColumnHeight, station.ResolvedColumnHeight, 12);
        }

        [Theory]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndeclaredHeightModeIsRejected(int raw)
        {
            var design = Design();
            design.ColumnHeight.Mode = (CantileverStationColumnHeightMode)raw;

            var station = Resolve(design);

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationHeightModeNotSupported));
        }

        [Fact]
        public void TheFINALPassAgreesWithTheLayout()
        {
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));

            Assert.False(station.IsBlocked);
            Assert.False(Has(station, CantileverDiagnostics.StationFinalPassDiffersFromLayout));

            foreach (var level in station.Levels)
            {
                foreach (var cell in level.Plan.Cells)
                {
                    var pattern = level.Arm(cell.Side).ConnectionPattern;

                    Assert.Equal(cell.LowerPunchIndex, pattern.LowerColumnPunchIndex);
                    Assert.Equal(cell.VerticalPunchCount, pattern.VerticalPunchCount);
                    Assert.Equal(cell.FirstElevation, pattern.FirstElevation, 9);
                    Assert.Equal(cell.LastElevation, pattern.LastElevation, 9);
                    Assert.Equal(cell.PlateBottomZ, pattern.PlateBottomZ, 9);
                    Assert.Equal(cell.PlateTopZ, pattern.PlateTopZ, 9);
                }
            }
        }

        // ---- 6. the component BOM ---------------------------------------------------------------------

        [Fact]
        public void ASingleStationBomHasONEColumnBaseAndOneArmPerLevel()
        {
            var bom = CantileverStationBomBuilder.Build(Resolve(Design(levels: 3)));

            Assert.True(bom.IsComponentBased);

            var columnBase = bom.Components.Single(c => c.Category == CantileverStationBomBuilder.ColumnBaseCategory);
            var arms = bom.Components.Where(c => c.Category == CantileverStationBomBuilder.ArmCategory).ToList();

            Assert.Equal(1, columnBase.Quantity);
            Assert.Single(arms);
            Assert.Equal(3, arms[0].Quantity);

            // Its recipe: one column, one bottom plate, one base, two plates and one gusset.
            Assert.Equal(1, columnBase.Pieces.Single(p => p.Category == CantileverStationBomBuilder.ColumnCategory).Quantity);
            Assert.Equal(1, columnBase.Pieces.Single(p => p.Category == CantileverStationBomBuilder.BaseCategory).Quantity);
            Assert.Equal(1, columnBase.Pieces.Single(p => p.Category == CantileverStationBomBuilder.GussetCategory).Quantity);
        }

        [Fact]
        public void ADoubleStationBomStillHasONEColumnBaseComponentWithTWOBases()
        {
            var bom = CantileverStationBomBuilder.Build(
                Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3)));

            var columnBase = bom.Components.Where(c => c.Category == CantileverStationBomBuilder.ColumnBaseCategory).ToList();

            // ONE component, not two. This is the invariant a "two stations" model would break.
            Assert.Single(columnBase);
            Assert.Equal(1, columnBase[0].Quantity);

            Assert.Equal(1, columnBase[0].Pieces.Single(p => p.Category == CantileverStationBomBuilder.ColumnCategory).Quantity);
            Assert.Equal(2, columnBase[0].Pieces.Single(p => p.Category == CantileverStationBomBuilder.BaseCategory).Quantity);
            Assert.Equal(2, columnBase[0].Pieces.Single(p => p.Category == CantileverStationBomBuilder.GussetCategory).Quantity);

            // And exactly one column bottom plate in the whole recipe.
            Assert.Single(columnBase[0].Pieces.Where(p => p.ProfileId == "Placa inferior de columna"));
            Assert.Equal(1, columnBase[0].Pieces.Single(p => p.ProfileId == "Placa inferior de columna").Quantity);
        }

        [Fact]
        public void SIXArmsForThreeLevelsOfADoubleStation()
        {
            var bom = CantileverStationBomBuilder.Build(
                Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3)));

            var arms = bom.Components.Where(c => c.Category == CantileverStationBomBuilder.ArmCategory).ToList();

            Assert.Single(arms);
            Assert.Equal(6, arms[0].Quantity);
        }

        [Fact]
        public void IdenticalArmsAreNOTSeparatedByside()
        {
            // The owner's decision: without a right/left variant, the same arm on +Y and on −Y is the same
            // purchase line.
            var bom = CantileverStationBomBuilder.Build(
                Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 2)));

            Assert.Single(bom.Components.Where(c => c.Category == CantileverStationBomBuilder.ArmCategory));

            var signature = CantileverStationBomBuilder.ArmSignature(
                Resolve(Design(faceMode: CantileverStationFaceMode.Double)).Arms[0]);

            Assert.DoesNotContain("PositiveY", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("NegativeY", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("STN", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("L1", signature, StringComparison.Ordinal);
        }

        [Fact]
        public void AnOverrideSPLITSTheArmRecipes()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3, clear: 12.0);
            design.Levels[2].SetOverride(
                CantileverArmSide.PositiveY, ArmTemplate(cutLength: 60.0));

            var station = Resolve(design);
            Assert.False(station.IsBlocked);

            var arms = CantileverStationBomBuilder.Build(station).Components
                .Where(c => c.Category == CantileverStationBomBuilder.ArmCategory)
                .ToList();

            Assert.Equal(2, arms.Count);
            Assert.Equal(6, arms.Sum(a => a.Quantity));
            Assert.Contains(arms, a => a.Quantity == 5);
            Assert.Contains(arms, a => a.Quantity == 1);
        }

        [Theory]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelFacing)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelBackToBack)]
        public void APairedArmContributesTWOProfilesPerComponent(CantileverArmBodyArrangement arrangement)
        {
            var station = Resolve(Design(
                levels: 2, clear: 12.0,
                defaultArm: ArmTemplate(section: ArmChannel, arrangement: arrangement, count: 3)));

            Assert.False(station.IsBlocked);

            var arm = CantileverStationBomBuilder.Build(station).Components
                .Single(c => c.Category == CantileverStationBomBuilder.ArmCategory);

            var profiles = arm.Pieces.Single(p => p.Category == CantileverStationBomBuilder.ArmProfileCategory);

            Assert.Equal(2, profiles.Quantity);
            Assert.Equal(ArmChannel, profiles.ProfileId);
        }

        [Theory]
        [InlineData(CantileverArmEndPlateMode.None, 2)]
        [InlineData(CantileverArmEndPlateMode.Cap, 3)]
        public void TheEndPlateIsOptionalInTheArmRecipe(CantileverArmEndPlateMode mode, int pieces)
        {
            var station = Resolve(Design(defaultArm: ArmTemplate(endMode: mode)));

            var arm = CantileverStationBomBuilder.Build(station).Components
                .Single(c => c.Category == CantileverStationBomBuilder.ArmCategory);

            Assert.Equal(pieces, arm.Pieces.Count);
        }

        [Fact]
        public void ASTOPIsADifferentRecipeFromACap()
        {
            var cap = Resolve(Design(defaultArm: ArmTemplate(endMode: CantileverArmEndPlateMode.Cap)));
            var stop = Resolve(Design(defaultArm: ArmTemplate(
                endMode: CantileverArmEndPlateMode.Stop, extraStop: 8.0)));

            var capSignature = CantileverStationBomBuilder.ArmSignature(cap.Arms[0]);
            var stopSignature = CantileverStationBomBuilder.ArmSignature(stop.Arms[0]);

            Assert.NotEqual(capSignature, stopSignature);
            Assert.Contains("Tope", string.Join("|", CantileverStationBomBuilder.Build(stop).Lines
                .Select(l => l.Description)), StringComparison.Ordinal);
        }

        [Fact]
        public void PunchesAreNOTBomLines()
        {
            var station = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));
            var bom = CantileverStationBomBuilder.Build(station);

            Assert.NotEmpty(station.Punches);

            var text = string.Join("|",
                bom.Lines.Select(l => l.Category + " " + l.ProfileId + " " + l.Description));

            Assert.DoesNotContain("Troquel", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("PCH", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Punch", text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TheFLATPiecesFollowTheVigentConventions()
        {
            var station = Resolve(Design(defaultArm: ArmTemplate(endMode: CantileverArmEndPlateMode.Cap)));
            var bom = CantileverStationBomBuilder.Build(station);

            foreach (var line in bom.Lines)
            {
                if (line.Category == CantileverStationBomBuilder.PlateCategory ||
                    line.Category == CantileverStationBomBuilder.GussetCategory)
                {
                    // A plate has no linear length. Its dimensions live in the description.
                    Assert.Equal(0.0, line.Length);
                    Assert.Contains("\"", line.Description, StringComparison.Ordinal);
                }
                else
                {
                    // A profile is identified by its section id and measured by its nominal cut.
                    Assert.True(line.Length > 0.0, line.Category + " deberia tener longitud.");
                    Assert.StartsWith("AISC-", line.ProfileId, StringComparison.Ordinal);
                }
            }
        }

        [Fact]
        public void TheBomInventsNoWeightMaterialOrCost()
        {
            var bom = CantileverStationBomBuilder.Build(Resolve(Design()));
            var text = string.Join("|", bom.Lines.Select(l => l.Description)) + "|" +
                       string.Join("|", bom.Components.Select(c => c.Description));

            foreach (var word in new[] { "kg", "lb", "peso", "costo", "acero", "A36", "soldad", "tornil", "ancla" })
            {
                Assert.DoesNotContain(word, text, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void ABLOCKEDStationProducesNoBom()
        {
            var station = Resolve(Design(
                heightMode: CantileverStationColumnHeightMode.Manual, manualHeight: 1.0));

            Assert.True(station.IsBlocked);

            var bom = CantileverStationBomBuilder.Build(station);

            Assert.Empty(bom.Components);
            Assert.Empty(bom.Lines);
            Assert.Equal(0, bom.TotalPieces);
        }

        // ---- 7. determinism ---------------------------------------------------------------------------

        [Fact]
        public void TheSameDesignProducesTheSameSignature()
        {
            var a = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));
            var b = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));

            Assert.Equal(a.Signature(), b.Signature());
            Assert.DoesNotContain("BLOCKED", a.Signature(), StringComparison.Ordinal);
        }

        [Fact]
        public void TheSameDesignProducesTheSameBom()
        {
            var a = CantileverStationBomBuilder.Build(Resolve(Design(levels: 3)));
            var b = CantileverStationBomBuilder.Build(Resolve(Design(levels: 3)));

            Assert.Equal(
                a.Components.Select(c => c.Category + c.Description + c.Quantity),
                b.Components.Select(c => c.Category + c.Description + c.Quantity));
            Assert.Equal(a.TotalPieces, b.TotalPieces);
        }

        [Fact]
        public void ChangingTheClearChangesTheIndices()
        {
            var tight = Resolve(Design(clear: 12.0, levels: 3));
            var wide = Resolve(Design(clear: 36.0, levels: 3));

            Assert.NotEqual(tight.Signature(), wide.Signature());
            Assert.True(wide.Levels[2].LowerPunchIndex > tight.Levels[2].LowerPunchIndex);
        }

        [Fact]
        public void ChangingTheFaceModeChangesCardinalityAndNeverDuplicatesTheColumn()
        {
            var single = Resolve(Design(faceMode: CantileverStationFaceMode.Single, levels: 3));
            var doubled = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3));

            Assert.NotEqual(single.Signature(), doubled.Signature());

            Assert.Equal(3, single.Arms.Count);
            Assert.Equal(6, doubled.Arms.Count);
            Assert.Equal(1, single.ColumnBase.Sides.Count);
            Assert.Equal(2, doubled.ColumnBase.Sides.Count);

            // The column is ONE either way, and the same one.
            Assert.Single(single.Members.Where(m => m.Role == CantileverMemberRole.Column));
            Assert.Single(doubled.Members.Where(m => m.Role == CantileverMemberRole.Column));
            Assert.Equal(
                single.Members.Single(m => m.Role == CantileverMemberRole.Column).GeometricLength,
                doubled.Members.Single(m => m.Role == CantileverMemberRole.Column).GeometricLength,
                12);
        }

        [Fact]
        public void AStationCarriesNoLongitudinalPositionOrNeighbour()
        {
            // What is absent is the contract: a run is a later initiative.
            var properties = typeof(CantileverStationAssembly).GetProperties().Select(p => p.Name).ToArray();

            Assert.DoesNotContain("PositionX", properties);
            Assert.DoesNotContain("RunIndex", properties);
            Assert.DoesNotContain("Spacers", properties);
            Assert.DoesNotContain("Braces", properties);
            Assert.DoesNotContain("Neighbour", properties);
            Assert.DoesNotContain("Next", properties);
            Assert.DoesNotContain("Previous", properties);
        }

        [Fact]
        public void OnlyTheDeclaredFaceModesAndScopesExist()
        {
            Assert.Equal(
                new[] { CantileverStationFaceMode.Single, CantileverStationFaceMode.Double },
                Enum.GetValues(typeof(CantileverStationFaceMode)).Cast<CantileverStationFaceMode>());

            Assert.Equal(
                new[]
                {
                    CantileverStationColumnHeightMode.Automatic,
                    CantileverStationColumnHeightMode.Manual
                },
                Enum.GetValues(typeof(CantileverStationColumnHeightMode)).Cast<CantileverStationColumnHeightMode>());

            Assert.Equal(
                new[]
                {
                    CantileverStationApplyScope.Cell,
                    CantileverStationApplyScope.Level,
                    CantileverStationApplyScope.Station
                },
                Enum.GetValues(typeof(CantileverStationApplyScope)).Cast<CantileverStationApplyScope>());
        }

        [Fact]
        public void LevelCountIsDerivedAndNotStored()
        {
            var properties = typeof(CantileverStationDesign).GetProperties().Select(p => p.Name).ToArray();

            Assert.Contains("Levels", properties);
            Assert.Contains("LevelCount", properties);
            Assert.False(
                typeof(CantileverStationDesign).GetProperty("LevelCount").CanWrite,
                "LevelCount debe ser derivado, no persistido.");
        }

        [Fact]
        public void TheTemplatesCarryNoDuplicatedAuthority()
        {
            // The two values the station owns must not exist in the templates.
            Assert.DoesNotContain(
                "Height",
                typeof(CantileverStationColumnBaseTemplateDesign).GetProperties().Select(p => p.Name));

            Assert.DoesNotContain(
                "LowerColumnPunchIndex",
                typeof(CantileverArmMountingPlateTemplateDesign).GetProperties().Select(p => p.Name));
        }

    }
}
