using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The defects the review of I-37C found, each pinned by the behaviour that was wrong.
    ///
    /// They live in their own file because a regression on any of them reads as what it is: not "a station test
    /// broke" but "the 250-candidate cap came back", "something normalised an invalid input again", "a plate is
    /// being measured with a world box again".
    /// </summary>
    public class CantileverStationCorrectionTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string ArmChannel = "AISC-C-C10X15_3";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const double Tolerance = 1e-9;

        private static StructuralSectionId Id(string value) => StructuralSectionId.Parse(value);

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
                new CantileverArmVariant(Id(ArmChannel), CantileverArmBodyArrangement.Single),
                new CantileverArmVariant(Id(ArmChannel), CantileverArmBodyArrangement.DoubleChannelFacing)
            });

        private static CantileverArmTemplateDesign ArmTemplate(
            string section = ArmHss,
            CantileverArmBodyArrangement arrangement = CantileverArmBodyArrangement.Single,
            double cutLength = 36.0,
            double slope = 0.0,
            int count = 2,
            double? offset = 1.5,
            double thickness = 0.25,
            CantileverArmEndPlateMode endMode = CantileverArmEndPlateMode.None,
            double endThickness = 0.25,
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
                    Thickness = thickness,
                    VerticalPunchCount = count,
                    VerticalEndOffset = offset
                },
                EndPlate = new CantileverArmEndPlateDesign
                {
                    Mode = endMode,
                    Thickness = endThickness,
                    ExtraStopHeight = extraStop
                }
            };

        private static CantileverStationDesign Design(
            CantileverStationFaceMode faceMode = CantileverStationFaceMode.Single,
            int levels = 2,
            int firstIndex = 0,
            double clear = 12.0,
            CantileverArmTemplateDesign defaultArm = null)
        {
            var design = new CantileverStationDesign
            {
                FaceMode = faceMode,
                FirstLevelPunchIndex = firstIndex,
                RequestedClearHeight = clear,
                DefaultArmTemplate = defaultArm ?? ArmTemplate(),
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

        private static CantileverStationAssembly Resolve(CantileverStationDesign design) =>
            new CantileverStationResolver(Catalog, Factory, ColumnBasePolicy(), ArmPolicy()).Resolve(design);

        private static bool Has(CantileverStationAssembly s, string code) =>
            s.Diagnostics.Any(d => d.Code == code);

        private static CantileverColumnRegularPunchGrid Grid(double pitch = 4.0)
        {
            var design = new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign { SectionId = ColumnW, Height = 240.0 },
                Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
            };
            design.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            design.Connection.Punches.ColumnTopPunchOffset = 4.0;
            design.Connection.Punches.RegularColumnPitch = pitch;

            var diagnostics = new List<CantileverDiagnostic>();
            var grid = new CantileverColumnBaseResolver(Catalog, Factory, ColumnBasePolicy())
                .ResolveRegularPunchGrid(design, diagnostics);

            Assert.NotNull(grid);
            return grid;
        }

        // ---- DEFECTO 1: no hay tope de candidatos ------------------------------------------------------

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(100000)]
        [InlineData(CantileverColumnRegularPunchGrid.MaxDefinedIndex)]
        public void TheGridAnswersEveryIndexINSIDEItsDomain(int index)
        {
            Assert.True(Grid().TryElevationAt(index, out var z));
            Assert.True(GeometryTolerance.IsFinite(z));
        }

        [Theory]
        [InlineData(CantileverColumnRegularPunchGrid.MaxDefinedIndex + 1)]
        [InlineData(int.MaxValue - 1)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public void TheGridRejectsAnIndexOUTSIDEItsDomainWithoutFreezing(long index)
        {
            // Constant time, no exception, no empty answer. Without the domain bound this walk would be two
            // billion additions for int.MaxValue.
            Assert.False(Grid().TryElevationAt(index, out _));
        }

        [Fact]
        public void TheLastDEFINEDIndexIsAcceptedAndTheNextIsNot()
        {
            var grid = Grid();

            Assert.True(grid.TryElevationAt(CantileverColumnRegularPunchGrid.MaxDefinedIndex, out _));
            Assert.False(grid.TryElevationAt(CantileverColumnRegularPunchGrid.MaxDefinedIndex + 1, out _));
        }

        [Theory]
        [InlineData(3.7)]
        [InlineData(4.0)]
        [InlineData(5.25)]
        public void FirstIndexAtOrAboveAgreesWithTheACCUMULATEDElevations(double pitch)
        {
            var grid = Grid(pitch);

            for (var i = 0; i < 40; i++)
            {
                Assert.True(grid.TryElevationAt(i, out var z));
                Assert.True(grid.TryFirstIndexAtOrAbove(z, out var found));

                // The elevation of index i is reached first by index i, never by i-1 and never by i+1.
                Assert.Equal(i, found);

                // And a target just above it moves to the next index.
                Assert.True(grid.TryFirstIndexAtOrAbove(z + (pitch / 2.0), out var next));
                Assert.Equal(i + 1, next);
            }
        }

        [Fact]
        public void FirstIndexAtOrAboveIsMONOTONICAndNeverSkips()
        {
            var grid = Grid(3.7);
            var previous = -1;

            for (var step = 0; step < 200; step++)
            {
                var target = grid.FirstElevation + (step * 0.9);

                Assert.True(grid.TryFirstIndexAtOrAbove(target, out var index));
                Assert.True(index >= previous, "El indice devuelto debe ser monotono.");
                Assert.True(grid.TryElevationAt(index, out var z));
                Assert.True(z >= target - Tolerance);

                if (index > 0)
                {
                    Assert.True(grid.TryElevationAt(index - 1, out var below));
                    Assert.True(below < target, "No puede ser el PRIMERO si el anterior tambien servia.");
                }

                previous = index;
            }
        }

        [Fact]
        public void AGridThatDoesNotRISEIsRejectedBeforeAnySearch()
        {
            var design = Design();
            design.ColumnBaseTemplate.Connection.Punches.RegularColumnPitch = 0.0;

            var station = Resolve(design);

            Assert.True(station.IsBlocked);
            // I-37A rejects a non-positive pitch on its own; what matters is that nothing loops.
            Assert.True(
                Has(station, CantileverDiagnostics.StationGridNotIncreasing) ||
                Has(station, CantileverDiagnostics.ParameterNotPositive) ||
                Has(station, CantileverDiagnostics.StationColumnBaseBlocked));
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MaxValue - 3)]
        [InlineData(1_000_000_000)]
        public void AnEXTREMEFirstLevelIndexBlocksWithoutOverflowOrFreeze(int firstIndex)
        {
            var station = Resolve(Design(firstIndex: firstIndex, levels: 1));

            Assert.True(station.IsBlocked);
            Assert.True(
                Has(station, CantileverDiagnostics.StationPunchIndexDomainOverflow) ||
                Has(station, CantileverDiagnostics.StationNotEnoughColumnPunches),
                "Diagnosticos: " + string.Join(" | ", station.Diagnostics.Select(d => d.Code)));

            // And nothing was built from a wrapped number.
            Assert.Null(station.ColumnBase);
        }

        [Fact]
        public void TheROWRANGEOverflowIsRejectedRatherThanWrapped()
        {
            // lowerIndex + count - 1 overflows int here. On longs it is simply out of domain.
            var metrics = CantileverArmConnectionMetricsResolver.Resolve(
                CantileverArmSide.PositiveY,
                CantileverArmBodyArrangement.Single,
                CantileverArmBodyOrientation.DepthPerpendicularToAxis,
                4.0,
                0.0,
                int.MaxValue - 1,
                5,
                1.5,
                Grid(),
                0.75);

            Assert.True(metrics.IsBlocked);
            Assert.Contains(
                metrics.Diagnostics,
                d => d.Code == CantileverDiagnostics.StationPunchIndexDomainOverflow);
        }

        [Fact]
        public void TheSHIPPEDFixturesStillLandOnTheSameIndices()
        {
            // The layout rewrite must not have moved anything that already worked.
            var station = Resolve(Design(levels: 3, clear: 24.0));

            Assert.False(station.IsBlocked);
            Assert.Equal(new[] { 0, 7, 14 }, station.Levels.Select(l => l.LowerPunchIndex));
        }

        // ---- DEFECTO 2: una sola autoridad valida y mide -----------------------------------------------

        public static IEnumerable<object[]> InvalidConnections()
        {
            yield return new object[] { ArmTemplate(slope: -1.0), CantileverDiagnostics.ArmSlopeInvalid };
            yield return new object[] { ArmTemplate(slope: double.NaN), CantileverDiagnostics.ArmSlopeInvalid };
            yield return new object[]
                { ArmTemplate(slope: 1e12), CantileverDiagnostics.ArmSlopeFrameUndefined };
            yield return new object[] { ArmTemplate(count: 0), CantileverDiagnostics.ArmVerticalCountTooSmall };
            yield return new object[] { ArmTemplate(count: 1), CantileverDiagnostics.ArmVerticalCountTooSmall };
            yield return new object[] { ArmTemplate(count: -3), CantileverDiagnostics.ArmVerticalCountTooSmall };
            yield return new object[]
                { ArmTemplate(offset: null), CantileverDiagnostics.RequiredParameterMissing };
            yield return new object[]
                { ArmTemplate(offset: double.NaN), CantileverDiagnostics.ParameterNotPositive };
            yield return new object[]
                { ArmTemplate(offset: -1.0), CantileverDiagnostics.ParameterNotPositive };
            yield return new object[]
                { ArmTemplate(offset: 0.1), CantileverDiagnostics.EdgeOffsetBelowRadius };
            yield return new object[]
                { ArmTemplate(section: ArmChannel, count: 2), CantileverDiagnostics.ArmPlateTooShortForBody };
        }

        [Theory]
        [MemberData(nameof(InvalidConnections))]
        public void AnINVALIDConnectionBlocksTheStationWithoutNormalisingOrThrowing(
            CantileverArmTemplateDesign arm, string code)
        {
            // Every one of these used to be either normalised by the station's own Math.Max / ?? 0, or
            // discovered only after the column had been sized from the normalised number.
            var station = Resolve(Design(defaultArm: arm));

            Assert.True(station.IsBlocked, "Deberia bloquear: " + code);
            Assert.True(
                Has(station, code),
                "Falta " + code + ". Diagnosticos: " +
                string.Join(" | ", station.Diagnostics.Select(d => d.Code)));

            Assert.Null(station.ColumnBase);
            Assert.Empty(station.Levels);
        }

        [Fact]
        public void TheAuthorityDoesNotCLAMPTheRowCount()
        {
            var metrics = CantileverArmConnectionMetricsResolver.Resolve(
                CantileverArmSide.PositiveY,
                CantileverArmBodyArrangement.Single,
                CantileverArmBodyOrientation.DepthPerpendicularToAxis,
                4.0, 0.0, 0, 0, 1.5, Grid(), 0.75);

            Assert.True(metrics.IsBlocked);
            Assert.Contains(
                metrics.Diagnostics, d => d.Code == CantileverDiagnostics.ArmVerticalCountTooSmall);

            // NOT silently two.
            Assert.Equal(0, metrics.VerticalPunchCount);
        }

        [Fact]
        public void TheAuthorityDoesNotDEFAULTTheMargin()
        {
            var metrics = CantileverArmConnectionMetricsResolver.Resolve(
                CantileverArmSide.PositiveY,
                CantileverArmBodyArrangement.Single,
                CantileverArmBodyOrientation.DepthPerpendicularToAxis,
                4.0, 0.0, 0, 2, null, Grid(), 0.75);

            Assert.True(metrics.IsBlocked);
            Assert.Contains(
                metrics.Diagnostics, d => d.Code == CantileverDiagnostics.RequiredParameterMissing);
            Assert.True(double.IsNaN(metrics.VerticalEndOffset));
        }

        [Fact]
        public void BOTHConsumersGetTheSameMetricsForTheSameInputs()
        {
            // The point of the extraction: the station's prediction and I-37B's resolve are the same numbers.
            var station = Resolve(Design(levels: 2, clear: 12.0, defaultArm: ArmTemplate(slope: 1.0)));

            Assert.False(station.IsBlocked);

            foreach (var level in station.Levels)
            {
                foreach (var cell in level.Plan.Cells)
                {
                    var arm = level.Arm(cell.Side);

                    Assert.Equal(cell.FirstElevation, arm.ConnectionPattern.FirstElevation, 12);
                    Assert.Equal(cell.PlateTopZ, arm.ConnectionPattern.PlateTopZ, 12);
                    Assert.Equal(
                        cell.BodyTopZ,
                        arm.ConnectionPattern.PlateBottomZ +
                        (Math.Cos(arm.Body.AngleRadians) * arm.Body.SectionHeight),
                        12);
                }
            }
        }

        // ---- DEFECTO 4: no se persisten overrides iguales al default ----------------------------------

        [Fact]
        public void ApplyingTheDEFAULTCreatesNoOverride()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);

            // A structurally equal COPY, not the same object: reference equality would have missed this.
            var change = matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                design.DefaultArmTemplate.DeepCopy());

            Assert.Equal(6, change.InScope.Count);
            Assert.Empty(change.Changed);
            Assert.True(change.IsNoOp);
            Assert.Equal(0, matrix.OverrideCount);
            Assert.All(design.Levels, l => Assert.Null(l.PositiveYOverride));
            Assert.All(design.Levels, l => Assert.Null(l.NegativeYOverride));
        }

        [Fact]
        public void ACellThatFollowsTheDefaultTRACKSAlaterChangeOfIt()
        {
            // The reason storing a copy of the default is wrong: from then on the cell stops following it, and
            // nothing in the design says which cells were pinned on purpose.
            var design = Design(levels: 2);
            var matrix = new CantileverStationArmMatrix(design);
            var cell = new CantileverStationCell(0, CantileverArmSide.PositiveY);

            matrix.Apply(CantileverStationApplyScope.Station, cell, design.DefaultArmTemplate.DeepCopy());

            design.DefaultArmTemplate.Body.CutLength = 60.0;

            Assert.Equal(60.0, matrix.Effective(cell).Body.CutLength);
        }

        [Fact]
        public void ApplyingADIFFERENTTemplateDoesCreateAnOverride()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 2);
            var matrix = new CantileverStationArmMatrix(design);

            var change = matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                ArmTemplate(cutLength: 60.0));

            Assert.Equal(4, change.InScope.Count);
            Assert.Equal(4, change.Changed.Count);
            Assert.False(change.IsNoOp);
            Assert.Equal(4, matrix.OverrideCount);
        }

        [Fact]
        public void ApplyingTheSAMEOverrideTwiceIsANoOp()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 2);
            var matrix = new CantileverStationArmMatrix(design);
            var anchor = new CantileverStationCell(0, CantileverArmSide.PositiveY);
            var template = ArmTemplate(cutLength: 60.0);

            var first = matrix.Apply(CantileverStationApplyScope.Station, anchor, template);
            var stored = design.Levels[0].PositiveYOverride;

            var second = matrix.Apply(CantileverStationApplyScope.Station, anchor, template.DeepCopy());

            Assert.Equal(4, first.Changed.Count);
            Assert.Empty(second.Changed);
            Assert.True(second.IsNoOp);

            // And the stored object was not even replaced: the design is untouched.
            Assert.Same(stored, design.Levels[0].PositiveYOverride);
        }

        [Fact]
        public void EachCellStillGetsItsOWNCopy()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 2);
            var matrix = new CantileverStationArmMatrix(design);

            matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                ArmTemplate(cutLength: 60.0));

            var a = design.Levels[0].PositiveYOverride;
            var b = design.Levels[1].NegativeYOverride;

            Assert.NotSame(a, b);
            a.Body.CutLength = 999.0;
            Assert.NotEqual(999.0, b.Body.CutLength);
        }

        [Fact]
        public void RestoringADefaultFollowingCellIsAlsoANoOp()
        {
            var design = Design(levels: 3);
            var matrix = new CantileverStationArmMatrix(design);

            var change = matrix.Restore(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY));

            Assert.Equal(3, change.InScope.Count);
            Assert.Empty(change.Changed);
        }

        [Fact]
        public void RestoringALevelAndAStationClearsOnlyWhatWasSet()
        {
            var design = Design(faceMode: CantileverStationFaceMode.Double, levels: 3);
            var matrix = new CantileverStationArmMatrix(design);
            var anchor = new CantileverStationCell(1, CantileverArmSide.PositiveY);

            matrix.Apply(CantileverStationApplyScope.Station, anchor, ArmTemplate(cutLength: 60.0));
            Assert.Equal(6, matrix.OverrideCount);

            var level = matrix.Restore(CantileverStationApplyScope.Level, anchor);

            Assert.Equal(2, level.InScope.Count);
            Assert.Equal(2, level.Changed.Count);
            Assert.Equal(4, matrix.OverrideCount);

            var station = matrix.Restore(CantileverStationApplyScope.Station, anchor);

            Assert.Equal(6, station.InScope.Count);
            Assert.Equal(4, station.Changed.Count);
            Assert.Equal(0, matrix.OverrideCount);
        }

        [Fact]
        public void ASingleStationStillHasNoInactiveCell()
        {
            var design = Design(levels: 3);
            var matrix = new CantileverStationArmMatrix(design);

            matrix.Apply(
                CantileverStationApplyScope.Station,
                new CantileverStationCell(0, CantileverArmSide.PositiveY),
                ArmTemplate(cutLength: 60.0));

            Assert.All(design.Levels, l => Assert.Null(l.NegativeYOverride));
            Assert.Equal(3, matrix.OverrideCount);
        }

        [Fact]
        public void TheComparerLooksAtEVERYEditableField()
        {
            var baseline = ArmTemplate();

            var variations = new[]
            {
                ArmTemplate(section: ArmChannel),
                ArmTemplate(arrangement: CantileverArmBodyArrangement.DoubleChannelFacing),
                ArmTemplate(cutLength: 37.0),
                ArmTemplate(slope: 0.5),
                ArmTemplate(thickness: 0.5),
                ArmTemplate(count: 3),
                ArmTemplate(offset: 2.0),
                ArmTemplate(offset: null),
                ArmTemplate(endMode: CantileverArmEndPlateMode.Cap),
                ArmTemplate(endThickness: 0.5),
                ArmTemplate(extraStop: 6.0)
            };

            Assert.True(CantileverArmTemplateComparer.AreEqual(baseline, baseline.DeepCopy()));

            foreach (var variation in variations)
            {
                Assert.False(
                    CantileverArmTemplateComparer.AreEqual(baseline, variation),
                    "El comparador no distingue: " + CantileverArmTemplateComparer.Signature(variation));
            }

            // A missing margin is NOT the same as zero: one is rejected and the other is not.
            Assert.False(CantileverArmTemplateComparer.AreEqual(
                ArmTemplate(offset: null), ArmTemplate(offset: 0.0)));
        }

        // ---- DEFECTO 6: las placas se miden en su propio plano ----------------------------------------

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(4.0)]
        public void ACapKeepsItsPHYSICALSizeAtEverySlope(double slope)
        {
            var station = Resolve(Design(
                levels: 1,
                defaultArm: ArmTemplate(slope: slope, endMode: CantileverArmEndPlateMode.Cap)));

            Assert.False(station.IsBlocked);

            var arm = station.Arms[0];
            var size = CantileverPlateInPlaneDimensions.Measure(arm.EndPlate);

            // The cap closes the body, so its in-plane dimensions ARE the body's section — whatever the slope.
            Assert.Equal(arm.Body.SectionWidth, size.Width, 9);
            Assert.Equal(arm.Body.SectionHeight, size.Height, 9);
        }

        [Theory]
        [InlineData(0.0, 6.0)]
        [InlineData(1.0, 6.0)]
        [InlineData(2.0, 12.0)]
        [InlineData(4.0, 3.0)]
        public void ASTOPKeepsItsExtraHeightEXACTLYAtEverySlope(double slope, double extra)
        {
            var station = Resolve(Design(
                levels: 1,
                defaultArm: ArmTemplate(
                    slope: slope, endMode: CantileverArmEndPlateMode.Stop, extraStop: extra)));

            Assert.False(station.IsBlocked);

            var arm = station.Arms[0];
            var up = arm.Body.Members[0].Placement.Frame.AxisY;
            var height = CantileverPlateInPlaneDimensions.ExtentAlong(arm.EndPlate.Outline, up);

            Assert.Equal(arm.Body.SectionHeight + extra, height, 9);
        }

        [Fact]
        public void TheWorldBoundingBoxWouldHaveSHRUNKTheCap()
        {
            // The defect, made visible. The plate is the same physical size at both slopes; only its world
            // spans differ, and the old code measured those.
            var flat = Resolve(Design(
                levels: 1, defaultArm: ArmTemplate(endMode: CantileverArmEndPlateMode.Cap))).Arms[0];
            var tilted = Resolve(Design(
                levels: 1,
                defaultArm: ArmTemplate(slope: 4.0, endMode: CantileverArmEndPlateMode.Cap))).Arms[0];

            var flatSize = CantileverPlateInPlaneDimensions.Measure(flat.EndPlate);
            var tiltedSize = CantileverPlateInPlaneDimensions.Measure(tilted.EndPlate);

            Assert.Equal(flatSize.Height, tiltedSize.Height, 9);

            // And the world span really does differ, so the test is not passing by accident.
            var worldZ = tilted.EndPlate.Outline.Max(p => p.Z) - tilted.EndPlate.Outline.Min(p => p.Z);
            Assert.True(
                Math.Abs(worldZ - tiltedSize.Height) > 1e-3,
                "El span mundial deberia diferir de la altura real; fue " + worldZ + " contra " +
                tiltedSize.Height + ".");
        }

        [Theory]
        [InlineData(ArmHss)]
        [InlineData(ArmChannel)]
        public void APlateIsMeasuredTheSameWhetherItIsTallerOrWiderThanItself(string section)
        {
            // A world box comes back SORTED, so a section taller than it is wide and one wider than it is tall
            // describe themselves identically. In-plane, width and height stay distinguishable.
            var station = Resolve(Design(
                levels: 1,
                clear: 12.0,
                defaultArm: ArmTemplate(
                    section: section, count: 4, endMode: CantileverArmEndPlateMode.Cap)));

            Assert.False(station.IsBlocked);

            var arm = station.Arms[0];
            var size = CantileverPlateInPlaneDimensions.Measure(arm.EndPlate);

            Assert.Equal(arm.Body.SectionWidth, size.Width, 9);
            Assert.Equal(arm.Body.SectionHeight, size.Height, 9);
        }

        [Fact]
        public void ADegenerateOrNonPlanarOutlineIsREJECTED()
        {
            Assert.False(CantileverPlateInPlaneDimensions.TryMeasure(
                new[] { new Point3D(0, 0, 0), new Point3D(1, 0, 0) }, out _, out var few));
            Assert.Contains("tres vertices", few, StringComparison.Ordinal);

            Assert.False(CantileverPlateInPlaneDimensions.TryMeasure(
                new[] { new Point3D(0, 0, 0), new Point3D(0, 0, 0), new Point3D(1, 0, 0) },
                out _, out var degenerate));
            Assert.Contains("degenerada", degenerate, StringComparison.Ordinal);

            Assert.False(CantileverPlateInPlaneDimensions.TryMeasure(
                new[]
                {
                    new Point3D(0, 0, 0), new Point3D(2, 0, 0),
                    new Point3D(2, 3, 0), new Point3D(0, 3, 5)
                },
                out _, out var folded));
            Assert.Contains("plano", folded, StringComparison.Ordinal);

            Assert.Throws<ArgumentException>(() => CantileverPlateInPlaneDimensions.Measure(
                new[] { new Point3D(0, 0, 0), new Point3D(1, 0, 0) }));
        }

        [Fact]
        public void TheBOMDescribesATiltedCapWithItsREALDimensions()
        {
            // Through the BOM, which is what the world-span bug actually corrupted: the plate identity. The cap
            // is the body section at every slope, so its BOM line must carry the same dimensions — a projected
            // measurement would shrink them and split one part into several.
            var flat = Line(0.0, CantileverArmEndPlateMode.Cap, 0.0);
            var tilted = Line(4.0, CantileverArmEndPlateMode.Cap, 0.0);

            Assert.Equal(flat.ProfileId, tilted.ProfileId);
            Assert.Equal(flat.Description, tilted.Description);
        }

        [Theory]
        [InlineData(6.0)]
        [InlineData(12.0)]
        public void TheBOMSignatureKeepsTheStopExtensionAtEverySlope(double extra)
        {
            // The stop extension is measured along the arm's own UP. With world spans it shrank as the arm
            // tilted, so the same physical stop signed differently at every slope.
            var flat = StopSignature(0.0, extra);
            var tilted = StopSignature(4.0, extra);

            Assert.Contains("stop=" + extra.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
                flat, StringComparison.Ordinal);
            Assert.Contains("stop=" + extra.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
                tilted, StringComparison.Ordinal);
        }

        private static BomLine Line(double slope, CantileverArmEndPlateMode mode, double extra)
        {
            var station = Resolve(Design(
                levels: 1,
                defaultArm: ArmTemplate(slope: slope, endMode: mode, extraStop: extra)));

            Assert.False(station.IsBlocked);

            return CantileverStationBomBuilder.Build(station).Lines
                .Single(l => l.ProfileId.StartsWith(
                    mode == CantileverArmEndPlateMode.Stop ? "Tope de brazo" : "Tapa de brazo",
                    StringComparison.Ordinal));
        }

        private static string StopSignature(double slope, double extra)
        {
            var station = Resolve(Design(
                levels: 1,
                defaultArm: ArmTemplate(
                    slope: slope, endMode: CantileverArmEndPlateMode.Stop, extraStop: extra)));

            Assert.False(station.IsBlocked);

            return CantileverStationBomBuilder.ArmSignature(station.Arms[0]);
        }

        // ---- DEFECTO 7: la firma incluye los brazos reales -------------------------------------------

        /// <summary>Two designs whose LAYOUT is identical and whose physical arms are not.</summary>
        public static IEnumerable<object[]> SameLayoutDifferentArm()
        {
            yield return new object[] { ArmTemplate(cutLength: 36.0), ArmTemplate(cutLength: 60.0) };
            yield return new object[]
            {
                ArmTemplate(endMode: CantileverArmEndPlateMode.Cap),
                ArmTemplate(endMode: CantileverArmEndPlateMode.Stop, extraStop: 6.0)
            };
            yield return new object[]
            {
                ArmTemplate(endMode: CantileverArmEndPlateMode.Cap, endThickness: 0.25),
                ArmTemplate(endMode: CantileverArmEndPlateMode.Cap, endThickness: 0.75)
            };
            yield return new object[]
            {
                ArmTemplate(section: ArmChannel, count: 4),
                ArmTemplate(
                    section: ArmChannel,
                    arrangement: CantileverArmBodyArrangement.DoubleChannelFacing,
                    count: 4)
            };
        }

        [Theory]
        [MemberData(nameof(SameLayoutDifferentArm))]
        public void TheSignatureMOVESWhenAnyPhysicalPieceDoes(
            CantileverArmTemplateDesign a, CantileverArmTemplateDesign b)
        {
            var first = Resolve(Design(levels: 2, defaultArm: a));
            var second = Resolve(Design(levels: 2, defaultArm: b));

            Assert.False(first.IsBlocked);
            Assert.False(second.IsBlocked);

            // The LAYOUT is the same — same indices, same clears — which is exactly why the earlier signature
            // could not tell these two stations apart.
            Assert.Equal(
                first.Levels.Select(l => l.LowerPunchIndex),
                second.Levels.Select(l => l.LowerPunchIndex));

            Assert.NotEqual(first.Signature(), second.Signature());
        }

        [Fact]
        public void TheSameInputStillProducesTheSameSignature()
        {
            var a = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3, clear: 24.0));
            var b = Resolve(Design(faceMode: CantileverStationFaceMode.Double, levels: 3, clear: 24.0));

            Assert.Equal(a.Signature(), b.Signature());
        }

        [Fact]
        public void TheSignatureCarriesTheACTIVESideOfASingleStation()
        {
            var positive = Design(levels: 2);
            var negative = Design(levels: 2);
            negative.SingleSide = CantileverArmSide.NegativeY;

            Assert.NotEqual(Resolve(positive).Signature(), Resolve(negative).Signature());
        }

        // ---- DEFECTO 8: mappings de enums cerrados ----------------------------------------------------

        [Theory]
        [InlineData(CantileverStationFaceMode.Single, 1)]
        [InlineData(CantileverStationFaceMode.Double, 2)]
        public void EveryDECLAREDFaceModeHasItsSides(CantileverStationFaceMode mode, int expected)
        {
            var design = Design();
            design.FaceMode = mode;

            Assert.True(design.TryActiveSides(out var sides));
            Assert.Equal(expected, sides.Count);
            Assert.Equal(expected, design.ActiveSides().Count);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUNDECLAREDFaceModeFailsClosed(int raw)
        {
            var design = Design();
            design.FaceMode = (CantileverStationFaceMode)raw;

            Assert.False(design.TryActiveSides(out var sides));
            Assert.Empty(sides);
            Assert.Throws<ArgumentOutOfRangeException>(() => design.ActiveSides());

            var station = Resolve(design);

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationFaceModeNotSupported));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUNDECLAREDSingleSideFailsClosed(int raw)
        {
            var design = Design();
            design.SingleSide = (CantileverArmSide)raw;

            Assert.False(design.TryActiveSides(out _));

            var station = Resolve(design);

            Assert.True(station.IsBlocked);
            Assert.True(Has(station, CantileverDiagnostics.StationFaceModeNotSupported));
        }

        [Theory]
        [InlineData(CantileverArmSide.PositiveY)]
        [InlineData(CantileverArmSide.NegativeY)]
        public void EveryDECLAREDSideHasItsOverrideSlot(CantileverArmSide side)
        {
            var level = new CantileverStationLevelDesign();
            var template = ArmTemplate();

            Assert.Null(level.OverrideFor(side));

            level.SetOverride(side, template);
            Assert.Same(template, level.OverrideFor(side));

            level.SetOverride(side, null);
            Assert.Null(level.OverrideFor(side));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUNDECLAREDSideHasNoOverrideSlot(int raw)
        {
            var level = new CantileverStationLevelDesign();
            var side = (CantileverArmSide)raw;

            Assert.Throws<ArgumentOutOfRangeException>(() => level.OverrideFor(side));
            Assert.Throws<ArgumentOutOfRangeException>(() => level.SetOverride(side, ArmTemplate()));

            // And nothing landed in the negative slot, which is what the ternary used to do.
            Assert.Null(level.PositiveYOverride);
            Assert.Null(level.NegativeYOverride);
        }
    }
}
