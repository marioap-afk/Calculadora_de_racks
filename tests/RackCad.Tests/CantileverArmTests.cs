using System;
using System.Collections.Generic;
using System.Linq;
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
    /// The Cantilever ARM (I-37B): body, mounting plate, selected column punches, slope, cap and stop.
    ///
    /// Against the SHIPPED catalogue and against a REAL resolved column, because the point of the initiative
    /// is that the arm consumes what I-37A left resolved. A test on a hand-made column would prove the wrong
    /// thing.
    ///
    /// Where a value comes from a section envelope, the test derives it the same way instead of copying a
    /// number. Two tests use literal figures on purpose, as a trap for a systematic error that a derived
    /// expectation would reproduce in the same direction.
    /// </summary>
    public class CantileverArmTests
    {
        // Column and base come from I-37A's own fixture, so the arm is tested against a column that resolves
        // exactly as that initiative closed it.
        private const string ColumnW = "AISC-W-W10X33";   // d 9.73, bf 7.96
        private const string BaseW = "AISC-W-W12X26";     // d 12.2, bf 6.49

        private const string Channel = "AISC-C-C10X15_3"; // d 10, bf 2.6, x 0.634
        private const string ShallowChannel = "AISC-C-C8X11_5";
        private const string Hss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string SingleW = "AISC-W-W6X15";

        /// <summary>
        /// An ANGLE, used for one purpose only: its cross section is not symmetric about its own origin, so
        /// the two halves of the square-cut misfit come out as DIFFERENT numbers. A W or an HSS would hide a
        /// bug that conflated them.
        /// </summary>
        private const string Angle = "AISC-L-L8X6X1";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const double Tolerance = 1e-9;
        private const string Owner = "ARM-L1";

        // ---- fixture ---------------------------------------------------------------------------------------

        private static StructuralSectionId Id(string value) => StructuralSectionId.Parse(value);

        private static Bounds2D Bounds(string id) =>
            Factory.Get(Id(id), SectionDetailLevel.Tabulated).Bounds;

        /// <summary>A column–base assembly resolved exactly as I-37A resolves it.</summary>
        private static CantileverColumnBaseAssembly Column(double height = 96.0)
        {
            var policy = CantileverColumnBaseSectionPolicy.Create(
                new[]
                {
                    new CantileverColumnBaseVariant(
                        CantileverColumnBaseVariantKind.WFlangeConnected, Id(ColumnW), Id(BaseW))
                },
                new[] { StructuralSectionFamily.W });

            var design = new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign { SectionId = ColumnW, Height = height },
                Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
            };
            design.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            design.Connection.Punches.ColumnTopPunchOffset = 4.0;

            var assembly = new CantileverColumnBaseResolver(Catalog, Factory, policy).Resolve(design);
            Assert.False(assembly.IsBlocked, "El fixture de columna no debe estar bloqueado.");
            return assembly;
        }

        private static CantileverArmSectionPolicy ArmPolicy(
            params (string Section, CantileverArmBodyArrangement Arrangement)[] variants) =>
            CantileverArmSectionPolicy.Create(
                variants.Select(v => new CantileverArmVariant(Id(v.Section), v.Arrangement)));

        private static CantileverArmSectionPolicy DefaultArmPolicy() =>
            ArmPolicy(
                (Hss, CantileverArmBodyArrangement.Single),
                (SingleW, CantileverArmBodyArrangement.Single),
                (Channel, CantileverArmBodyArrangement.Single),
                (Channel, CantileverArmBodyArrangement.DoubleChannelFacing),
                (Channel, CantileverArmBodyArrangement.DoubleChannelBackToBack),
                (ShallowChannel, CantileverArmBodyArrangement.DoubleChannelFacing));

        private static CantileverArmDesign Design(
            string section = Hss,
            CantileverArmBodyArrangement arrangement = CantileverArmBodyArrangement.Single,
            CantileverArmSide side = CantileverArmSide.PositiveY,
            double cutLength = 48.0,
            double slope = 0.0,
            int lowerIndex = 0,
            int count = 2,
            double? offset = 1.5,
            CantileverArmEndPlateMode endMode = CantileverArmEndPlateMode.None,
            double extraStop = 0.0)
        {
            var design = new CantileverArmDesign
            {
                Side = side,
                Body = new CantileverArmBodyDesign
                {
                    Arrangement = arrangement,
                    SectionId = section,
                    CutLength = cutLength,
                    SlopeRisePer12 = slope
                },
                MountingPlate = new CantileverArmMountingPlateDesign
                {
                    LowerColumnPunchIndex = lowerIndex,
                    VerticalPunchCount = count,
                    VerticalEndOffset = offset
                },
                EndPlate = new CantileverArmEndPlateDesign
                {
                    Mode = endMode,
                    ExtraStopHeight = extraStop
                }
            };

            return design;
        }

        private static CantileverArmAssembly Resolve(
            CantileverArmDesign design = null,
            CantileverArmSectionPolicy policy = null,
            CantileverColumnBaseAssembly column = null,
            string owner = Owner) =>
            new CantileverArmResolver(Catalog, Factory, policy ?? DefaultArmPolicy())
                .Resolve(design ?? Design(), column ?? Column(), owner);

        private static bool Has(CantileverArmAssembly a, string code) =>
            a.Diagnostics.Any(d => d.Code == code);

        /// <summary>
        /// Where a member's WEB BACK lands in placed section coordinates.
        ///
        /// I-36 documents the canonical channel: the back of the web faces −X, so it sits at
        /// <c>Bounds.MinX</c> before the mirror and at <c>−Bounds.MinX</c> after it. This is the discriminator
        /// between the two paired arrangements, and it is geometry rather than a flag.
        /// </summary>
        private static double WebBack(CantileverArmMemberPlacement placement, Bounds2D bounds) =>
            (placement.Mirrored ? -bounds.MinX : bounds.MinX) + placement.TransverseOffset;

        // ---- 1. the single body ----------------------------------------------------------------------------

        [Fact]
        public void ASingleBodyResolvesToExactlyOneMember()
        {
            var arm = Resolve();

            Assert.False(arm.IsBlocked);
            Assert.Single(arm.Body.Members);
            Assert.Equal(CantileverArmBodyArrangement.Single, arm.Body.Arrangement);
            Assert.All(arm.Body.Members, m => Assert.Equal(CantileverMemberRole.Arm, m.Role));
        }

        [Fact]
        public void TheCutLengthIsTheProfileLengthAndNothingElse()
        {
            var arm = Resolve(Design(cutLength: 42.0));

            Assert.Equal(42.0, arm.Body.CutLength, 12);
            Assert.All(arm.Body.Members, m =>
            {
                Assert.Equal(42.0, m.GeometricLength, 12);
                Assert.Equal(42.0, m.NominalCutLength, 12);
            });
        }

        [Fact]
        public void APlateThicknessMovesTheArmInsteadOfShorteningIt()
        {
            var thin = Resolve(Design());
            var thick = Design();
            thick.MountingPlate.Thickness = 1.0;
            var moved = Resolve(thick);

            // Same cut, different start: the number the user captures is the number they order.
            Assert.Equal(thin.Body.CutLength, moved.Body.CutLength, 12);
            Assert.NotEqual(
                Math.Round(thin.Body.Members[0].Start.Y, 9),
                Math.Round(moved.Body.Members[0].Start.Y, 9));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.25)]
        [InlineData(0.75)]
        [InlineData(3.0)]
        public void TheSlopeIsAcceptedAndItsAngleIsDerived(double rise)
        {
            var arm = Resolve(Design(slope: rise));

            Assert.False(arm.IsBlocked);
            Assert.Equal(rise, arm.Body.SlopeRisePer12, 12);
            Assert.Equal(Math.Atan(rise / 12.0), arm.Body.AngleRadians, 12);
        }

        [Theory]
        [InlineData(CantileverArmSide.PositiveY, 1.0)]
        [InlineData(CantileverArmSide.NegativeY, -1.0)]
        public void ZeroSlopeRunsHorizontallyTowardsItsOwnSide(CantileverArmSide side, double sign)
        {
            var arm = Resolve(Design(side: side, slope: 0.0));

            Assert.Equal(sign, arm.Body.Direction.Y, 12);
            Assert.Equal(0.0, arm.Body.Direction.Z, 12);
            Assert.Equal(side, arm.Side);
        }

        [Theory]
        [InlineData(CantileverArmSide.PositiveY)]
        [InlineData(CantileverArmSide.NegativeY)]
        public void TheFreeEndRISESOnBothSides(CantileverArmSide side)
        {
            // The whole point of the slope being a camber: +sin on both sides. A 180-degree rotation would
            // invert it on the negative side — invisible head-on, obvious in profile.
            var arm = Resolve(Design(side: side, slope: 1.0));
            var member = arm.Body.Members[0];

            Assert.True(arm.Body.Direction.Z > 0.0, "El eje del brazo debe subir.");
            Assert.True(
                member.End.Z > member.Start.Z + Tolerance,
                "El extremo libre debe quedar mas alto que la raiz en el lado " + side + ".");
        }

        [Fact]
        public void TheSingleBodyIsFamilyIndependent()
        {
            // Three rows so the deepest of the three fits: the point here is the FAMILY, not the depth check.
            foreach (var section in new[] { Hss, SingleW, Channel })
            {
                var arm = Resolve(Design(section: section, count: 3));
                Assert.False(arm.IsBlocked, section + " deberia resolver como perfil sencillo.");
                Assert.Equal(section, arm.Body.Members[0].SectionId.Value);
            }
        }

        // ---- 2. the paired bodies --------------------------------------------------------------------------

        [Theory]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelFacing)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelBackToBack)]
        public void APairedBodyResolvesToTwoMembersThatShareEverythingButTheirIdentity(
            CantileverArmBodyArrangement arrangement)
        {
            var arm = Resolve(Design(section: Channel, arrangement: arrangement, cutLength: 36.0, count: 3));

            Assert.False(arm.IsBlocked);
            Assert.Equal(2, arm.Body.Members.Count);

            var a = arm.Body.Members[0];
            var b = arm.Body.Members[1];

            Assert.Equal(a.SectionId, b.SectionId);
            Assert.Equal(a.GeometricLength, b.GeometricLength, 12);
            Assert.Equal(36.0, a.GeometricLength, 12);
            Assert.Equal(a.Direction.X, b.Direction.X, 12);
            Assert.Equal(a.Direction.Y, b.Direction.Y, 12);
            Assert.Equal(a.Direction.Z, b.Direction.Z, 12);
            Assert.NotEqual(a.Id.Value, b.Id.Value);
        }

        [Theory]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelFacing)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelBackToBack)]
        public void APairedBodyTouchesWithZeroGapAndNoOverlap(CantileverArmBodyArrangement arrangement)
        {
            var geometry = Factory.Get(Id(Channel), SectionDetailLevel.Tabulated);

            Assert.True(CantileverArmBodyArrangementResolver.TouchesWithoutOverlap(
                arrangement, geometry, Tolerance, out var gap, out var overlap));
            Assert.Equal(0.0, gap, 12);
            Assert.Equal(0.0, overlap, 12);
        }

        [Theory]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelFacing)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelBackToBack)]
        public void APairedBodyIsSymmetricAboutItsCentralPlane(CantileverArmBodyArrangement arrangement)
        {
            var geometry = Factory.Get(Id(Channel), SectionDetailLevel.Tabulated);
            var b = geometry.Bounds;
            var placements = CantileverArmBodyArrangementResolver.Placements(arrangement, geometry);

            var first = placements[0].SpanX(b);
            var second = placements[1].SpanX(b);

            // Mirror images about x = 0, and they meet exactly there.
            Assert.Equal(-second.Max, first.Min, 12);
            Assert.Equal(-second.Min, first.Max, 12);
            Assert.Equal(0.0, first.Max, 12);
            Assert.Equal(0.0, second.Min, 12);

            var combined = CantileverArmBodyArrangementResolver.CombinedBounds(arrangement, geometry);
            Assert.Equal(-combined.MinX, combined.MaxX, 12);
            Assert.Equal(2.0 * b.Width, combined.Width, 12);
        }

        [Fact]
        public void FacingPutsTheFLANGETIPSOnTheCentralPlaneAndTheWebBacksOutside()
        {
            var geometry = Factory.Get(Id(Channel), SectionDetailLevel.Tabulated);
            var b = geometry.Bounds;
            var placements = CantileverArmBodyArrangementResolver.Placements(
                CantileverArmBodyArrangement.DoubleChannelFacing, geometry);

            // Openings face each other, so the web backs are the OUTER edges of the pair.
            Assert.Equal(-b.Width, WebBack(placements[0], b), 12);
            Assert.Equal(b.Width, WebBack(placements[1], b), 12);

            // And one member is mirrored: a channel is handed, so a facing pair needs both hands.
            Assert.False(placements[0].Mirrored);
            Assert.True(placements[1].Mirrored);
        }

        [Fact]
        public void BackToBackPutsTheWEBBACKSOnTheCentralPlaneAndTheOpeningsOutside()
        {
            var geometry = Factory.Get(Id(Channel), SectionDetailLevel.Tabulated);
            var b = geometry.Bounds;
            var placements = CantileverArmBodyArrangementResolver.Placements(
                CantileverArmBodyArrangement.DoubleChannelBackToBack, geometry);

            // Backs in contact: both web backs land ON the central plane.
            Assert.Equal(0.0, WebBack(placements[0], b), 12);
            Assert.Equal(0.0, WebBack(placements[1], b), 12);

            Assert.True(placements[0].Mirrored);
            Assert.False(placements[1].Mirrored);
        }

        [Fact]
        public void TheTwoPairedArrangementsAreNotTheSamePlacement()
        {
            // Same combined bounds, different hands: a test that only checked the envelope would pass on both
            // and prove nothing.
            var facing = Resolve(Design(
                section: Channel, arrangement: CantileverArmBodyArrangement.DoubleChannelFacing, count: 3));
            var back = Resolve(Design(
                section: Channel, arrangement: CantileverArmBodyArrangement.DoubleChannelBackToBack, count: 3));

            Assert.NotEqual(facing.Signature(), back.Signature());
            Assert.Equal(
                facing.Body.Members.Select(m => !m.Placement.Mirrored),
                back.Body.Members.Select(m => m.Placement.Mirrored));
        }

        [Fact]
        public void APairedArrangementWithANonChannelSectionIsRejected()
        {
            var policy = ArmPolicy((Hss, CantileverArmBodyArrangement.DoubleChannelFacing));
            var arm = Resolve(
                Design(section: Hss, arrangement: CantileverArmBodyArrangement.DoubleChannelFacing), policy);

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmArrangementRequiresChannel));
        }

        [Fact]
        public void ThereIsNoGapParameterToEdit()
        {
            // The gap is zero because the arrangement puts the two contact faces on one plane, not because a
            // parameter says so. A field whose only legal value is zero gets edited eventually.
            var names = typeof(CantileverArmBodyDesign).GetProperties().Select(p => p.Name).ToArray();

            Assert.DoesNotContain(names, n => n.IndexOf("Gap", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.DoesNotContain(names, n => n.IndexOf("Clear", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.DoesNotContain(names, n => n.IndexOf("Separ", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // ---- 3. the mounting plate -------------------------------------------------------------------------

        [Fact]
        public void TheMountingPlateSpansTheWholeColumnWidthCentredOnX()
        {
            var column = Column();
            var arm = Resolve(column: column);

            var columnMinX = column.ColumnBottomPlate.Outline.Min(p => p.X);
            var columnMaxX = column.ColumnBottomPlate.Outline.Max(p => p.X);

            Assert.Equal(columnMinX, arm.MountingPlate.Outline.Min(p => p.X), 12);
            Assert.Equal(columnMaxX, arm.MountingPlate.Outline.Max(p => p.X), 12);
            Assert.Equal(
                -arm.MountingPlate.Outline.Min(p => p.X), arm.MountingPlate.Outline.Max(p => p.X), 12);
            Assert.Equal(CantileverPlateKind.ArmMounting, arm.MountingPlate.Kind);
        }

        [Theory]
        [InlineData(CantileverArmSide.PositiveY)]
        [InlineData(CantileverArmSide.NegativeY)]
        public void TheBodyStartsOnThePlatesOuterFace(CantileverArmSide side)
        {
            var arm = Resolve(Design(side: side));
            var plateY = arm.MountingPlate.Outline[0].Y;
            var outward = side == CantileverArmSide.PositiveY ? 1.0 : -1.0;
            var outerY = plateY + (outward * arm.MountingPlate.Thickness);

            Assert.Equal(outerY, arm.Body.Members[0].Start.Y, 12);
        }

        [Fact]
        public void TheBodysLowerEnvelopeAtTheConnectionMatchesThePlateBottomEdge()
        {
            var arm = Resolve(Design(section: Channel, count: 3));
            var plateBottom = arm.MountingPlate.Outline.Min(p => p.Z);

            Assert.Equal(arm.ConnectionPattern.PlateBottomZ, plateBottom, 12);

            // With zero slope the section's own bottom lands exactly there.
            var frame = arm.Body.Members[0].Placement.Frame;
            var bottom = frame.ToWorld(new Point3D(0.0, arm.Body.SectionBounds.MinY, 0.0)).Z;

            Assert.Equal(plateBottom, bottom, 12);
        }

        [Fact]
        public void ThePlateThicknessesAreIndependentAndDefaultToAQuarterInch()
        {
            var design = Design(endMode: CantileverArmEndPlateMode.Cap);

            Assert.Equal(CantileverDefaults.PlateThickness, design.MountingPlate.Thickness);
            Assert.Equal(CantileverDefaults.PlateThickness, design.EndPlate.Thickness);

            design.MountingPlate.Thickness = 0.5;
            design.EndPlate.Thickness = 0.75;

            var arm = Resolve(design);

            Assert.Equal(0.5, arm.MountingPlate.Thickness);
            Assert.Equal(0.75, arm.EndPlate.Thickness);
        }

        // ---- 4. the selected column punches ----------------------------------------------------------------

        [Fact]
        public void TheTwoPunchColumnsAreTheColumnsOwn()
        {
            var column = Column();
            var arm = Resolve(column: column);

            Assert.Equal(column.Pattern.LeftRowX, arm.ConnectionPattern.LeftRowX, 12);
            Assert.Equal(column.Pattern.RightRowX, arm.ConnectionPattern.RightRowX, 12);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(6)]
        public void TheRowCountIsHonouredAndTheExtraRowsGrowUPWARDS(int count)
        {
            // HSS4X4 is 4 in deep, so it fits the 7-in plate that two rows give. A 10-in channel would not,
            // and this test is about the row count and not about the depth check.
            var two = Resolve(Design(section: Hss, count: 2));
            var arm = Resolve(Design(section: Hss, count: count));

            Assert.False(arm.IsBlocked);
            Assert.Equal(count, arm.ConnectionPattern.VerticalPunchCount);
            Assert.Equal(count, arm.ConnectionPattern.Elevations.Count);
            Assert.Equal(2 * count, arm.MountingPunches.Count);

            if (count == 2)
            {
                return;
            }

            // The bottom edge is anchored to the body; only the top moves.
            Assert.Equal(two.ConnectionPattern.PlateBottomZ, arm.ConnectionPattern.PlateBottomZ, 12);
            Assert.True(arm.ConnectionPattern.PlateTopZ > two.ConnectionPattern.PlateTopZ + Tolerance);
        }

        [Fact]
        public void ThePitchIsCONSUMEDFromTheColumnAndNotDeclaredHere()
        {
            var column = Column();
            var arm = Resolve(Design(section: Channel, count: 4), column: column);

            var columnElevations = column.ColumnRegularPunches
                .Select(p => p.Datum.V).Distinct().OrderBy(v => v).ToList();
            var columnPitch = columnElevations[1] - columnElevations[0];

            Assert.Equal(columnPitch, arm.ConnectionPattern.ObservedPitch, 12);

            // It happens to be 4 in today because the COLUMN governs it. The assertion is against the column,
            // not against the number.
            Assert.Equal(CantileverDefaults.RegularColumnPunchPitch, columnPitch, 12);
        }

        [Fact]
        public void TheSelectionIsContiguousAndStartsAtTheRequestedIndex()
        {
            var column = Column();
            var columnElevations = column.ColumnRegularPunches
                .Select(p => p.Datum.V).Distinct().OrderBy(v => v).ToList();

            var arm = Resolve(Design(section: Channel, lowerIndex: 4, count: 3), column: column);

            Assert.Equal(
                columnElevations.Skip(4).Take(3).Select(v => Math.Round(v, 9)),
                arm.ConnectionPattern.Elevations.Select(v => Math.Round(v, 9)));
        }

        [Fact]
        public void EveryArmPunchCOINCIDESWithAColumnPunchByDatum()
        {
            var column = Column();
            var arm = Resolve(Design(section: Channel, count: 3), column: column);

            Assert.NotEmpty(arm.MountingPunches);

            foreach (var punch in arm.MountingPunches)
            {
                Assert.Contains(
                    column.ColumnRegularPunches,
                    c => c.Datum.ApproxEquals(punch.Datum));
            }

            // ... and the 3D centres differ, because the plate is another surface. That is exactly why the
            // datum excludes the axis coordinate.
            var armPunch = arm.MountingPunches[0];
            var columnPunch = column.ColumnRegularPunches.First(c => c.Datum.ApproxEquals(armPunch.Datum));

            Assert.Equal(columnPunch.Centre.X, armPunch.Centre.X, 12);
            Assert.Equal(columnPunch.Centre.Z, armPunch.Centre.Z, 12);
            Assert.NotEqual(Math.Round(columnPunch.Centre.Y, 9), Math.Round(armPunch.Centre.Y, 9));
            Assert.Equal(CantileverPunchSurface.ArmMountingPlate, armPunch.Surface);
        }

        [Fact]
        public void TheArmDoesNotDuplicateTheColumnsPunches()
        {
            var arm = Resolve(Design(section: Channel, count: 3));

            Assert.All(arm.MountingPunches,
                p => Assert.Equal(CantileverPunchSurface.ArmMountingPlate, p.Surface));
            Assert.DoesNotContain(arm.MountingPunches, p => p.Surface == CantileverPunchSurface.ColumnFace);
        }

        [Fact]
        public void FewerThanTwoRowsIsRejected()
        {
            var arm = Resolve(Design(count: 1));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmVerticalCountTooSmall));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(17)]
        [InlineData(500)]
        public void AnIndexOutsideTheColumnsPunchesIsRejected(int lowerIndex)
        {
            var arm = Resolve(Design(lowerIndex: lowerIndex, count: 2));

            Assert.True(arm.IsBlocked);
            Assert.True(
                Has(arm, CantileverDiagnostics.ArmPunchIndexOutOfRange) ||
                Has(arm, CantileverDiagnostics.ArmNotEnoughColumnPunches));
        }

        [Fact]
        public void AColumnWithNoRegularPunchesCannotCarryAnArm()
        {
            // A 22-inch column has no room for a regular punch after the connection region — I-37A says so
            // with a warning; here it is what stops an arm.
            var arm = Resolve(column: Column(height: 22.0));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmNotEnoughColumnPunches));
        }

        [Fact]
        public void ADeepProfileWithTooFewRowsIsREJECTEDAndAsksForMore()
        {
            // C10X15.3 is 10 in deep. Two rows at the column's 4-in pitch with a 1.5-in margin give a 7-in
            // plate, which cannot hold it. Three rows give 11 in, which can.
            var tooFew = Resolve(Design(section: Channel, count: 2));

            Assert.True(tooFew.IsBlocked);
            Assert.True(Has(tooFew, CantileverDiagnostics.ArmPlateTooShortForBody));

            var enough = Resolve(Design(section: Channel, count: 3));
            Assert.False(enough.IsBlocked);
        }

        [Fact]
        public void ThePlateIsNotStretchedSilentlyToFitTheBody()
        {
            var blocked = Resolve(Design(section: Channel, count: 2));

            Assert.True(blocked.IsBlocked);
            Assert.Null(blocked.MountingPlate);
            Assert.Contains(
                "VerticalPunchCount",
                blocked.Diagnostics.Single(d => d.Code == CantileverDiagnostics.ArmPlateTooShortForBody).Message,
                StringComparison.Ordinal);
        }

        [Fact]
        public void AMissingVerticalMarginIsRejectedAndNeverDefaulted()
        {
            var arm = Resolve(Design(offset: null));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.RequiredParameterMissing));
        }

        [Fact]
        public void AVerticalMarginBelowThePunchRadiusIsRejected()
        {
            var arm = Resolve(Design(offset: 0.1)); // radius is 0.375

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.EdgeOffsetBelowRadius));
        }

        // ---- 5. the end plate -------------------------------------------------------------------------------

        [Fact]
        public void NoneMeansThereIsNoEndPlate()
        {
            var arm = Resolve(Design(endMode: CantileverArmEndPlateMode.None));

            Assert.False(arm.IsBlocked);
            Assert.Null(arm.EndPlate);
            Assert.Single(arm.Plates);
        }

        [Fact]
        public void ACapTakesItsSizeFromTheCombinedBodyEnvelope()
        {
            var arm = Resolve(Design(
                section: Channel,
                arrangement: CantileverArmBodyArrangement.DoubleChannelFacing,
                count: 3,
                endMode: CantileverArmEndPlateMode.Cap));

            Assert.NotNull(arm.EndPlate);
            Assert.Equal(CantileverPlateKind.ArmEnd, arm.EndPlate.Kind);

            var width = Distance(arm.EndPlate.Outline[0], arm.EndPlate.Outline[1]);
            var height = Distance(arm.EndPlate.Outline[1], arm.EndPlate.Outline[2]);

            Assert.Equal(arm.Body.SectionWidth, width, 12);
            Assert.Equal(arm.Body.SectionHeight, height, 12);
        }

        [Fact]
        public void AStopIsTheSamePlateGrownUPWARDS()
        {
            var cap = Resolve(Design(section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Cap));
            var stop = Resolve(Design(
                section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Stop, extraStop: 6.0));

            var capHeight = Distance(cap.EndPlate.Outline[1], cap.EndPlate.Outline[2]);
            var stopHeight = Distance(stop.EndPlate.Outline[1], stop.EndPlate.Outline[2]);

            Assert.Equal(capHeight + 6.0, stopHeight, 12);

            // The bottom edge does not move; the extension goes up.
            Assert.Equal(cap.EndPlate.Outline.Min(p => p.Z), stop.EndPlate.Outline.Min(p => p.Z), 12);
            Assert.True(stop.EndPlate.Outline.Max(p => p.Z) > cap.EndPlate.Outline.Max(p => p.Z) + Tolerance);
        }

        [Fact]
        public void TheStopDoesNotChangeTheProfilesCut()
        {
            var cap = Resolve(Design(section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Cap));
            var stop = Resolve(Design(
                section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Stop, extraStop: 9.0));

            Assert.Equal(cap.Body.CutLength, stop.Body.CutLength, 12);
            Assert.Equal(
                cap.Body.Members[0].GeometricLength, stop.Body.Members[0].GeometricLength, 12);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        public void TheEndPlateStaysPERPENDICULARToTheSlopedArm(double slope)
        {
            var arm = Resolve(Design(
                section: Channel, count: 3, slope: slope, endMode: CantileverArmEndPlateMode.Stop,
                extraStop: 4.0));

            var axis = arm.Body.Direction;
            var edgeAcross = arm.EndPlate.Outline[1] - arm.EndPlate.Outline[0];
            var edgeUp = arm.EndPlate.Outline[2] - arm.EndPlate.Outline[1];

            // Both in-plane edges are perpendicular to the arm's axis: the plate faces along it.
            Assert.Equal(0.0, edgeAcross.Dot(axis), 9);
            Assert.Equal(0.0, edgeUp.Dot(axis), 9);
            Assert.Equal(1.0, Math.Abs(arm.EndPlate.Normal.Dot(axis)), 9);

            // And its height direction still points up in the world, so the stop grows upwards.
            Assert.True(edgeUp.Z > 0.0, "El tope debe crecer hacia arriba.");
        }

        [Fact]
        public void AStopWithoutExtraHeightIsRejected()
        {
            var arm = Resolve(Design(
                section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Stop, extraStop: 0.0));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmStopWithoutHeight));
        }

        [Theory]
        [InlineData(CantileverArmEndPlateMode.None)]
        [InlineData(CantileverArmEndPlateMode.Cap)]
        public void AnExtraHeightWithoutAStopIsRejected(CantileverArmEndPlateMode mode)
        {
            var arm = Resolve(Design(section: Channel, count: 3, endMode: mode, extraStop: 3.0));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmEndPlateHeightWithoutStop));
        }

        // ---- 6. eligibility and undefined values ------------------------------------------------------------

        [Fact]
        public void AnUnregisteredCombinationOfSectionAndArrangementIsRejected()
        {
            var policy = ArmPolicy((Channel, CantileverArmBodyArrangement.Single));
            var arm = Resolve(
                Design(section: Channel, arrangement: CantileverArmBodyArrangement.DoubleChannelFacing), policy);

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.CombinationNotEligible));
        }

        [Fact]
        public void ANewDesignIsOfferedOnlyRegisteredEnabledSectionsForItsArrangement()
        {
            var policy = DefaultArmPolicy();

            var singles = policy.EligibleForNewDesign(Catalog, CantileverArmBodyArrangement.Single);
            var facing = policy.EligibleForNewDesign(
                Catalog, CantileverArmBodyArrangement.DoubleChannelFacing);

            Assert.Equal(new[] { Hss, SingleW, Channel }, singles.Select(s => s.SectionId.Value));
            Assert.Equal(new[] { Channel, ShallowChannel }, facing.Select(s => s.SectionId.Value));
            Assert.All(singles, s => Assert.True(s.IsEnabled));
        }

        [Fact]
        public void APolicyRejectsADuplicateRegistration()
        {
            Assert.Throws<ArgumentException>(() => ArmPolicy(
                (Channel, CantileverArmBodyArrangement.Single),
                (Channel, CantileverArmBodyArrangement.Single)));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndefinedArrangementIsRejectedAndNeverFallsBackToSingle(int raw)
        {
            var arrangement = (CantileverArmBodyArrangement)raw;
            var geometry = Factory.Get(Id(Channel), SectionDetailLevel.Tabulated);

            Assert.False(CantileverArmBodyArrangementResolver.IsSupported(arrangement));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverArmBodyArrangementResolver.Placements(arrangement, geometry));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverArmBodyArrangementResolver.MemberCount(arrangement));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndefinedSideIsRejectedAndNeverFallsBackToPositiveY(int raw)
        {
            var side = (CantileverArmSide)raw;

            Assert.False(CantileverArmFrameResolver.IsSupported(side));
            Assert.Throws<ArgumentOutOfRangeException>(() => CantileverArmFrameResolver.Axis(side, 0.0));
        }

        [Fact]
        public void OnlyTheDeclaredArrangementsSidesAndOrientationsAreSupported()
        {
            // A member added to any of these enums fails here until somebody writes its rule.
            Assert.Equal(
                new[]
                {
                    CantileverArmBodyArrangement.Single,
                    CantileverArmBodyArrangement.DoubleChannelFacing,
                    CantileverArmBodyArrangement.DoubleChannelBackToBack
                },
                Enum.GetValues(typeof(CantileverArmBodyArrangement)).Cast<CantileverArmBodyArrangement>());

            Assert.All(
                Enum.GetValues(typeof(CantileverArmBodyArrangement)).Cast<CantileverArmBodyArrangement>(),
                a => Assert.True(CantileverArmBodyArrangementResolver.IsSupported(a)));
            Assert.All(
                Enum.GetValues(typeof(CantileverArmSide)).Cast<CantileverArmSide>(),
                s => Assert.True(CantileverArmFrameResolver.IsSupported(s)));
            Assert.All(
                Enum.GetValues(typeof(CantileverArmBodyOrientation)).Cast<CantileverArmBodyOrientation>(),
                o => Assert.True(CantileverArmFrameResolver.IsSupported(o)));
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void ANegativeOrNonFiniteSlopeIsRejected(double slope)
        {
            var arm = Resolve(Design(slope: slope));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmSlopeInvalid));
        }

        [Fact]
        public void ABlockedColumnCannotCarryAnArm()
        {
            var policy = CantileverColumnBaseSectionPolicy.Create(
                Array.Empty<CantileverColumnBaseVariant>(), new[] { StructuralSectionFamily.W });
            var blockedColumn = new CantileverColumnBaseResolver(Catalog, Factory, policy)
                .Resolve(new CantileverColumnBaseDesign());

            Assert.True(blockedColumn.IsBlocked);

            var arm = Resolve(column: blockedColumn);

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmColumnAssemblyBlocked));
        }

        // ---- 7. determinism ----------------------------------------------------------------------------------

        [Fact]
        public void TheSameDesignProducesTheSameSignature()
        {
            var first = Resolve(Design(section: Channel, count: 3)).Signature();
            var second = Resolve(Design(section: Channel, count: 3)).Signature();

            Assert.Equal(first, second);
            Assert.DoesNotContain("BLOCKED", first, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangingTheSlopeChangesTheGeometryAndTheSignature()
        {
            var flat = Resolve(Design(section: Channel, count: 3, slope: 0.0));
            var sloped = Resolve(Design(section: Channel, count: 3, slope: 1.0));

            Assert.NotEqual(flat.Signature(), sloped.Signature());
            Assert.True(sloped.Body.Members[0].End.Z > flat.Body.Members[0].End.Z + Tolerance);
        }

        [Fact]
        public void ChangingTheRowCountChangesThePlateAndTheSignature()
        {
            var three = Resolve(Design(section: Channel, count: 3));
            var five = Resolve(Design(section: Channel, count: 5));

            Assert.NotEqual(three.Signature(), five.Signature());
            Assert.True(five.ConnectionPattern.PlateHeight > three.ConnectionPattern.PlateHeight + Tolerance);
        }

        [Fact]
        public void ChangingTheArrangementChangesTheMemberCountAndTheEnvelope()
        {
            var single = Resolve(Design(section: Channel, count: 3));
            var paired = Resolve(Design(
                section: Channel, arrangement: CantileverArmBodyArrangement.DoubleChannelFacing, count: 3));

            Assert.Single(single.Body.Members);
            Assert.Equal(2, paired.Body.Members.Count);
            // The BODY's envelope, not the assembly's: the mounting plate spans the whole column width and
            // dominates both, so comparing the assembly would compare the plate with itself.
            Assert.True(paired.Body.Envelope.Width > single.Body.Envelope.Width + Tolerance);
            Assert.NotEqual(single.Signature(), paired.Signature());
        }

        [Fact]
        public void TheOwnerTokenComesFromTheCallerAndShapesEveryId()
        {
            var arm = Resolve(Design(section: Channel, count: 3), owner: "ARM-L4");

            Assert.Equal("ARM-L4", arm.Owner);
            Assert.StartsWith("CANT-ARM-L4-", arm.Body.Members[0].Id.Value, StringComparison.Ordinal);
            Assert.StartsWith("CANT-ARM-L4-", arm.MountingPlate.Id.Value, StringComparison.Ordinal);
            Assert.All(arm.MountingPunches,
                p => Assert.StartsWith("CANT-ARM-L4-", p.Id.Value, StringComparison.Ordinal));
        }

        [Fact]
        public void EveryPieceIdIsDistinct()
        {
            var arm = Resolve(Design(
                section: Channel, arrangement: CantileverArmBodyArrangement.DoubleChannelBackToBack,
                count: 3, endMode: CantileverArmEndPlateMode.Stop, extraStop: 4.0));

            var ids = arm.Body.Members.Select(m => m.Id.Value)
                .Concat(arm.Plates.Select(p => p.Id.Value))
                .Concat(arm.MountingPunches.Select(p => p.Id.Value))
                .ToList();

            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void ReorderingTheDesignDoesNotChangeTheIds()
        {
            // Ids come from the owner and the piece token, never from iteration order.
            var a = Resolve(Design(section: Channel, count: 3));
            var b = Resolve(Design(section: Channel, count: 5));

            Assert.Equal(
                a.Body.Members.Select(m => m.Id.Value),
                b.Body.Members.Select(m => m.Id.Value));
            Assert.Equal(a.MountingPlate.Id.Value, b.MountingPlate.Id.Value);
        }

        // ---- 8. the declared approximation --------------------------------------------------------------------

        [Fact]
        public void ASlopedArmReportsThatItsSquareCutIsNotFlush()
        {
            var flat = Resolve(Design(section: Channel, count: 3, slope: 0.0));
            var sloped = Resolve(Design(section: Channel, count: 3, slope: 1.0));

            Assert.False(Has(flat, CantileverDiagnostics.ArmSquareCutAtSlopedPlate));
            Assert.True(Has(sloped, CantileverDiagnostics.ArmSquareCutAtSlopedPlate));
            Assert.All(sloped.Diagnostics, d => Assert.False(d.IsBlocking));
        }

        [Fact]
        public void TheWorkedExampleMatches()
        {
            // Literal expectations, on purpose. Column W10X33 with base W12X26 puts its last connection punch
            // at 16.5, so its regular grid starts at 20.5 and steps 4. An arm on rows 0..2 with a 1.5 margin
            // therefore spans 19.0 to 30.0, and C10X15.3 is 10 in deep, which fits in that 11.
            var arm = Resolve(Design(section: Channel, count: 3));

            Assert.Equal(new[] { 20.5, 24.5, 28.5 },
                arm.ConnectionPattern.Elevations.Select(v => Math.Round(v, 9)));
            Assert.Equal(19.0, Math.Round(arm.ConnectionPattern.PlateBottomZ, 9));
            Assert.Equal(30.0, Math.Round(arm.ConnectionPattern.PlateTopZ, 9));
            Assert.Equal(4.0, Math.Round(arm.ConnectionPattern.ObservedPitch, 9));
            Assert.Equal(6, arm.MountingPunches.Count);
        }

        [Fact]
        public void ThePairedChannelWorkedExampleMatches()
        {
            // C10X15.3: bf 2.6 and the tabulated x 0.634, so after centring the web back is at -0.634 and the
            // flange tip at 1.966. A pair therefore spans 5.2 across, exactly twice the flange width.
            var b = Bounds(Channel);

            Assert.Equal(-0.634, Math.Round(b.MinX, 6));
            Assert.Equal(1.966, Math.Round(b.MaxX, 6));

            var arm = Resolve(Design(
                section: Channel, arrangement: CantileverArmBodyArrangement.DoubleChannelFacing, count: 3));

            Assert.Equal(5.2, Math.Round(arm.Body.SectionWidth, 6));
            Assert.Equal(10.0, Math.Round(arm.Body.SectionHeight, 6));
        }

// ---- 9. the four defects of the correction round -----------------------------------------------------
        //
        // Each block below pins one defect that shipped in the first pass of I-37B and is now fixed. They are
        // grouped by defect, and not folded into the sections above, so a regression reads as what it is.

        // ---- 9.1 the end plate mode is validated EXHAUSTIVELY ---------------------------------------------

        [Fact]
        public void UnderNoneTheEndPlateThicknessIsDORMANTDataAndNotValidated()
        {
            // There is no plate, so the thickness is a number nobody uses. Blocking on it would reject a valid
            // arm because of a value left behind by an earlier edit.
            var design = Design(endMode: CantileverArmEndPlateMode.None);
            design.EndPlate.Thickness = 0.0;

            var arm = Resolve(design);

            Assert.False(arm.IsBlocked);
            Assert.Null(arm.EndPlate);
            Assert.False(Has(arm, CantileverDiagnostics.ParameterNotPositive));
        }

        [Theory]
        [InlineData(CantileverArmEndPlateMode.Cap, 0.0)]
        [InlineData(CantileverArmEndPlateMode.Stop, 6.0)]
        public void AModeThatDOESProduceAPlateRequiresItsThickness(
            CantileverArmEndPlateMode mode, double extraStop)
        {
            var design = Design(section: Channel, count: 3, endMode: mode, extraStop: extraStop);
            design.EndPlate.Thickness = 0.0;

            var arm = Resolve(design);

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ParameterNotPositive));
        }

        [Theory]
        [InlineData(CantileverArmEndPlateMode.None, double.NaN)]
        [InlineData(CantileverArmEndPlateMode.None, double.PositiveInfinity)]
        [InlineData(CantileverArmEndPlateMode.Cap, double.NaN)]
        [InlineData(CantileverArmEndPlateMode.Cap, double.PositiveInfinity)]
        public void AModeThatFORBIDSAnExtraHeightRejectsANonFiniteOne(
            CantileverArmEndPlateMode mode, double extraStop)
        {
            // `Math.Abs(NaN) > tolerance` is FALSE, so a comparison on its own would wave a NaN through as if it
            // were zero. Finiteness is checked first, and this is what proves it.
            var arm = Resolve(Design(section: Channel, count: 3, endMode: mode, extraStop: extraStop));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmEndPlateHeightWithoutStop));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void AStopWithANonFiniteHeightIsRejected(double extraStop)
        {
            var arm = Resolve(Design(
                section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Stop, extraStop: extraStop));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmStopWithoutHeight));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndeclaredEndPlateModeIsRejectedAndNeverMaterialisedAsACap(int raw)
        {
            // The defect exactly: the validation had guarded cases and NO default, so an undeclared value passed
            // without a word and the builder's `if None ... else ternary` then drew it as a cap.
            var design = Design(section: Channel, count: 3);
            design.EndPlate.Mode = (CantileverArmEndPlateMode)raw;

            var arm = Resolve(design);

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmEndPlateModeNotSupported));
            Assert.Null(arm.EndPlate);
            Assert.Empty(arm.Plates);
        }

        [Fact]
        public void OnlyTheDeclaredEndPlateModesExist()
        {
            // A member added to this enum fails here until somebody writes its rule in BOTH the validation and
            // the builder.
            Assert.Equal(
                new[]
                {
                    CantileverArmEndPlateMode.None,
                    CantileverArmEndPlateMode.Cap,
                    CantileverArmEndPlateMode.Stop
                },
                Enum.GetValues(typeof(CantileverArmEndPlateMode)).Cast<CantileverArmEndPlateMode>());
        }

        [Fact]
        public void TheThreeDeclaredModesKeepEXACTLYTheirGeometryAndSignature()
        {
            // The fix must not have moved anything that already worked.
            var none = Resolve(Design(section: Channel, count: 3, endMode: CantileverArmEndPlateMode.None));
            var cap = Resolve(Design(section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Cap));
            var stop = Resolve(Design(
                section: Channel, count: 3, endMode: CantileverArmEndPlateMode.Stop, extraStop: 5.0));

            Assert.All(new[] { none, cap, stop }, a => Assert.False(a.IsBlocked));

            Assert.Null(none.EndPlate);
            Assert.Single(none.Plates);
            Assert.Equal(2, cap.Plates.Count);
            Assert.Equal(2, stop.Plates.Count);

            var capHeight = Distance(cap.EndPlate.Outline[1], cap.EndPlate.Outline[2]);
            var stopHeight = Distance(stop.EndPlate.Outline[1], stop.EndPlate.Outline[2]);

            Assert.Equal(cap.Body.SectionHeight, capHeight, 12);
            Assert.Equal(cap.Body.SectionHeight + 5.0, stopHeight, 12);

            // The body is untouched by the mode, and the three signatures stay distinguishable.
            Assert.Equal(none.Body.Signature(), cap.Body.Signature());
            Assert.Equal(none.Body.Signature(), stop.Body.Signature());
            Assert.Equal(3, new[] { none.Signature(), cap.Signature(), stop.Signature() }.Distinct().Count());
        }

        // ---- 9.2 the punch index cannot overflow ----------------------------------------------------------

        /// <summary>How many distinct regular elevations the fixture column actually resolved.</summary>
        private static int RegularElevationCount(CantileverColumnBaseAssembly column) =>
            column.ColumnRegularPunches.Select(p => p.Datum.V).Distinct().Count();

        private static IReadOnlyList<double> RegularElevations(CantileverColumnBaseAssembly column) =>
            column.ColumnRegularPunches.Select(p => p.Datum.V).Distinct().OrderBy(v => v).ToList();

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MaxValue - 1)]
        [InlineData(1000000)]
        public void AHugeLowerIndexIsRejectedInsteadOfOverflowing(int lowerIndex)
        {
            // `lowerColumnPunchIndex + verticalPunchCount` wrapped NEGATIVE for a large index, the range check
            // passed, and the empty selection then threw where the pitch is read. It is a subtraction now.
            var arm = Resolve(Design(section: Channel, count: 3, lowerIndex: lowerIndex));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmPunchIndexOutOfRange));
        }

        [Fact]
        public void TheOverflowingSelectionIsRejectedByThePatternItselfWithoutThrowing()
        {
            // Straight at the arithmetic, with no resolver in between: the pattern must REPORT, never throw.
            var column = Column();
            var diagnostics = new List<CantileverDiagnostic>();

            var pattern = CantileverArmColumnConnectionPattern.Build(
                column.ColumnRegularPunches, int.MaxValue, 2, 1.5, diagnostics);

            Assert.Null(pattern);
            Assert.Contains(diagnostics, d => d.Code == CantileverDiagnostics.ArmPunchIndexOutOfRange);
            Assert.All(diagnostics, d => Assert.True(d.IsBlocking));
        }

        [Fact]
        public void TheLastValidLowerIndexIsAcceptedAndTheNextOneIsRejected()
        {
            // Derived from the column, not copied: the boundary moves with the fixture instead of rotting.
            var column = Column();
            var elevations = RegularElevations(column);

            Assert.True(
                elevations.Count >= 4, "El fixture necesita varias elevaciones para que el limite tenga sentido.");

            var last = Resolve(Design(lowerIndex: elevations.Count - 2, count: 2), column: column);
            var past = Resolve(Design(lowerIndex: elevations.Count - 1, count: 2), column: column);

            Assert.False(last.IsBlocked);
            Assert.Equal(2, last.ConnectionPattern.Elevations.Count);
            Assert.Equal(elevations[elevations.Count - 1], last.ConnectionPattern.LastElevation, 12);

            Assert.True(past.IsBlocked);
            Assert.True(Has(past, CantileverDiagnostics.ArmPunchIndexOutOfRange));
        }

        [Fact]
        public void TheOrdinaryIndicesAreUnchangedByTheFix()
        {
            var column = Column();
            var elevations = RegularElevations(column);

            for (var i = 0; i < 4; i++)
            {
                var arm = Resolve(Design(lowerIndex: i, count: 2), column: column);

                Assert.False(arm.IsBlocked);
                Assert.Equal(elevations.Skip(i).Take(2), arm.ConnectionPattern.Elevations);
            }
        }

        // ---- 9.3 a slope that collapses the frame is REJECTED, not thrown --------------------------------

        [Theory]
        [InlineData(1e12)]
        [InlineData(1e15)]
        [InlineData(double.MaxValue)]
        public void ASlopeSteepEnoughToCollapseTheFrameIsReportedAndNeverThrows(double slope)
        {
            // `atan` is bounded below 90 degrees but CONVERGES on it, so a finite non-negative rise can leave the
            // projection of +Z on the transverse plane at zero. That is a number somebody TYPED: it has to come
            // back as a diagnostic and not as an InvalidOperationException from inside the frame authority.
            var arm = Resolve(Design(slope: slope));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmSlopeFrameUndefined));
            Assert.False(Has(arm, CantileverDiagnostics.ArmSlopeInvalid));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(24.0)]
        [InlineData(1e6)]
        public void ALargeButREPRESENTABLESlopeStillResolves(double slope)
        {
            var arm = Resolve(Design(slope: slope));

            Assert.False(arm.IsBlocked);
            Assert.False(Has(arm, CantileverDiagnostics.ArmSlopeFrameUndefined));
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.NegativeInfinity)]
        public void AnInvalidSlopeReportsOnlyItsOwnCodeAndNotTheCollapseOne(double slope)
        {
            // Two codes for one bad number is noise, and a NaN cannot be judged for representability at all.
            var arm = Resolve(Design(slope: slope));

            Assert.True(arm.IsBlocked);
            Assert.True(Has(arm, CantileverDiagnostics.ArmSlopeInvalid));
            Assert.False(Has(arm, CantileverDiagnostics.ArmSlopeFrameUndefined));
        }

        [Theory]
        [InlineData(CantileverArmSide.PositiveY)]
        [InlineData(CantileverArmSide.NegativeY)]
        public void TheFrameAuthorityAgreesWithTheGateItBacks(CantileverArmSide side)
        {
            Assert.True(CantileverArmFrameResolver.IsRepresentableSlope(side, 0.0));
            Assert.True(CantileverArmFrameResolver.IsRepresentableSlope(side, 1e6));

            Assert.False(CantileverArmFrameResolver.IsRepresentableSlope(side, 1e12));
            Assert.False(CantileverArmFrameResolver.IsRepresentableSlope(side, double.MaxValue));
            Assert.False(CantileverArmFrameResolver.IsRepresentableSlope(side, -1.0));
            Assert.False(CantileverArmFrameResolver.IsRepresentableSlope(side, double.NaN));
        }

        [Fact]
        public void AVerticalAxisHasNoDepthDirectionAndSaysSo()
        {
            Assert.False(CantileverArmFrameResolver.TryDepthAxis(Vector3D.UnitZ, out _));
            Assert.False(CantileverArmFrameResolver.TryDepthAxis(new Vector3D(0.0, 0.0, 0.0), out _));
            Assert.Throws<InvalidOperationException>(() => CantileverArmFrameResolver.DepthAxis(Vector3D.UnitZ));

            // And a slope that IS representable produces a depth axis pointing up.
            var axis = CantileverArmFrameResolver.Axis(
                CantileverArmSide.PositiveY, CantileverArmFrameResolver.AngleRadians(2.0));

            Assert.True(CantileverArmFrameResolver.TryDepthAxis(axis, out var depth));
            Assert.True(depth.Z > 0.0);
        }

        [Fact]
        public void TheGateAndTheProjectionShareONEBound()
        {
            // Two numbers for one threshold is how a gate and the computation it guards end up disagreeing.
            Assert.True(CantileverArmFrameResolver.DegenerateProjection > 0.0);
            Assert.True(CantileverArmFrameResolver.DegenerateProjection < 1e-6);
        }

        // ---- 9.4 intrusion and clearance are TWO magnitudes ----------------------------------------------

        private static string Formatted(double value) =>
            value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

        [Fact]
        public void AnASYMMETRICSectionReportsIntrusionAndClearanceSeparately()
        {
            var bounds = Bounds(Angle);

            // The fixture verifies ITSELF: if this section were symmetric about its origin the test could pass
            // while the code conflated the two magnitudes.
            Assert.True(
                Math.Abs(bounds.MaxY + bounds.MinY) > 1e-3,
                "La seccion de prueba debe ser asimetrica respecto a su origen; " + Angle + " no lo es.");

            var arm = Resolve(
                Design(section: Angle, count: 3, slope: 1.0),
                ArmPolicy((Angle, CantileverArmBodyArrangement.Single)));

            Assert.False(arm.IsBlocked);

            var rise = Math.Sin(CantileverArmFrameResolver.AngleRadians(1.0));
            var intrusion = rise * Math.Max(0.0, bounds.MaxY);
            var clearance = rise * Math.Max(0.0, -bounds.MinY);

            Assert.NotEqual(Formatted(intrusion), Formatted(clearance));

            var message = arm.Diagnostics
                .Single(d => d.Code == CantileverDiagnostics.ArmSquareCutAtSlopedPlate)
                .Message;

            Assert.Contains(Formatted(intrusion), message, StringComparison.Ordinal);
            Assert.Contains(Formatted(clearance), message, StringComparison.Ordinal);
        }

        [Fact]
        public void ASymmetricSectionReportsTheSameNumberTwiceAndThatIsNotAnArtefact()
        {
            // The contrast that makes the test above mean something: on a section that IS symmetric the two
            // magnitudes coincide, so one shared number would have looked correct here.
            var bounds = Bounds(Hss);

            Assert.Equal(bounds.MaxY, -bounds.MinY, 9);

            var arm = Resolve(Design(slope: 1.0));
            var rise = Math.Sin(CantileverArmFrameResolver.AngleRadians(1.0));
            var message = arm.Diagnostics
                .Single(d => d.Code == CantileverDiagnostics.ArmSquareCutAtSlopedPlate)
                .Message;

            Assert.Contains(Formatted(rise * bounds.MaxY), message, StringComparison.Ordinal);
        }

        [Fact]
        public void TheDeclaredApproximationDoesNotClaimTheFacesAreFlush()
        {
            // The message is what a user reads. It must not promise a fit the geometry does not have.
            var arm = Resolve(Design(section: Channel, count: 3, slope: 1.0));
            var message = arm.Diagnostics
                .Single(d => d.Code == CantileverDiagnostics.ArmSquareCutAtSlopedPlate)
                .Message;

            Assert.Contains("NO queda a ras", message, StringComparison.Ordinal);
            Assert.Contains("penetra", message, StringComparison.Ordinal);
            Assert.Contains("holgura", message, StringComparison.Ordinal);
            Assert.Contains("aproximacion visual declarada", message, StringComparison.Ordinal);
            Assert.DoesNotContain("no se traslapan", message, StringComparison.Ordinal);
        }

        private static double Distance(Point3D a, Point3D b) => (b - a).Length;
    }
}
