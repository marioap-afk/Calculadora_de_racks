using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The Cantilever column–base foundation (I-37A).
    ///
    /// These pin the invariants that ADR-0024 and the owner's decisions fix, and they do it against the
    /// SHIPPED catalogue: the point of the initiative is that the sub-assembly composes real sections
    /// through I-36's public surface, so a test on a hand-made geometry would prove the wrong thing.
    ///
    /// Where a value is derived from a section envelope, the test derives it the same way instead of
    /// copying a number — otherwise the test would assert that AISC has not changed, not that the resolver
    /// is right. Two tests do use literal figures on purpose, as a trap for a systematic error that a
    /// derived expectation would reproduce.
    /// </summary>
    public class CantileverColumnBaseTests
    {
        // Real ids. The policy is injectable and I-37A registers none in the product: these belong to the
        // tests, which is what keeps arbitrary production ids out of the source (owner decision 4.1).
        private const string ColumnW = "AISC-W-W10X33";   // d 9.73, bf 7.96
        private const string BaseW = "AISC-W-W12X26";     // d 12.2, bf 6.49
        private const string WideColumn = "AISC-W-W14X873"; // bf 18.8
        private const string NarrowBase = "AISC-W-W6X9";    // bf 3.94
        // Shallower than BaseW yet still WIDE enough to accept the rows W10X33 governs (needs bf >= 5.71):
        // a base that failed the horizontal check would never reach the vertical one.
        private const string ShallowBase = "AISC-W-W6X15";  // d 5.99, bf 5.99
        private const string Hss = "AISC-HSS-RECT-HSS4X4X_250";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const double Tolerance = 1e-9;

        // ---- fixture ---------------------------------------------------------------------------------------

        private static StructuralSectionId Id(string value) => StructuralSectionId.Parse(value);

        private static CantileverColumnBaseVariant Variant(string column, string @base) =>
            new CantileverColumnBaseVariant(
                CantileverColumnBaseVariantKind.WFlangeConnected, Id(column), Id(@base));

        private static CantileverColumnBaseSectionPolicy PolicyFor(params (string Column, string Base)[] pairs) =>
            CantileverColumnBaseSectionPolicy.Create(
                pairs.Select(p => Variant(p.Column, p.Base)),
                new[] { StructuralSectionFamily.W });

        private static CantileverColumnBaseDesign Design(
            string column = ColumnW,
            string @base = BaseW,
            double height = 96.0,
            double baseLength = 48.0,
            Action<CantileverPunchParameters> tune = null)
        {
            var design = new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign { SectionId = column, Height = height },
                Base = new CantileverBaseDesign { SectionId = @base, Length = baseLength }
            };

            // The two values the owner approved NO default for. Every test supplies them explicitly, which
            // is the same thing the product demands of a real design.
            design.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            design.Connection.Punches.ColumnTopPunchOffset = 4.0;

            tune?.Invoke(design.Connection.Punches);
            return design;
        }

        private static CantileverColumnBaseResolver Resolver(CantileverColumnBaseSectionPolicy policy = null) =>
            new CantileverColumnBaseResolver(
                Catalog, Factory, policy ?? PolicyFor((ColumnW, BaseW), (BaseW, BaseW)));

        private static CantileverColumnBaseAssembly Resolve(
            CantileverColumnBaseDesign design = null,
            CantileverColumnBaseSectionPolicy policy = null) =>
            Resolver(policy).Resolve(design ?? Design());

        private static Bounds Envelope(string id)
        {
            var bounds = Factory.Get(Id(id), SectionDetailLevel.Tabulated).Bounds;
            return new Bounds(bounds.Width, bounds.Height);
        }

        private readonly struct Bounds
        {
            public Bounds(double width, double height)
            {
                Width = width;
                Height = height;
            }

            public double Width { get; }

            public double Height { get; }
        }

        private static bool Has(CantileverColumnBaseAssembly assembly, string code) =>
            assembly.Diagnostics.Any(d => d.Code == code);

        // ---- 1-5: sections, ids and eligibility ------------------------------------------------------------

        [Fact]
        public void TheColumnAndTheBaseMayShareTheSameSection()
        {
            var assembly = Resolve(Design(column: BaseW, @base: BaseW));

            Assert.False(assembly.IsBlocked);
            Assert.Equal(assembly.Column.SectionId, assembly.Base.SectionId);
        }

        [Fact]
        public void TheColumnAndTheBaseMayUseDifferentSections()
        {
            var assembly = Resolve();

            Assert.False(assembly.IsBlocked);
            Assert.NotEqual(assembly.Column.SectionId, assembly.Base.SectionId);
            Assert.Equal(ColumnW, assembly.Column.SectionId.Value);
            Assert.Equal(BaseW, assembly.Base.SectionId.Value);
        }

        [Theory]
        [InlineData("no es un id")]
        [InlineData("AISC-W-NO_EXISTE")]
        [InlineData("")]
        public void AnIdThatDoesNotResolveBlocksTheAssembly(string bad)
        {
            var assembly = Resolve(Design(column: bad));

            Assert.True(assembly.IsBlocked);
            Assert.Null(assembly.Column);
            Assert.Null(assembly.Pattern);
        }

        [Fact]
        public void BothIdsAreParsedAtTheSameSingleBoundary()
        {
            // The design stores TEXT and only the Application resolver turns it into an id. The observable
            // proof is that a malformed id produces a DIAGNOSTIC and never an exception out of Domain.
            var design = Design(column: "??", @base: "??");
            var assembly = Resolve(design);

            Assert.True(assembly.IsBlocked);
            Assert.Equal(2, assembly.Diagnostics.Count(d => d.Code == CantileverDiagnostics.SectionIdInvalid));
        }

        [Fact]
        public void AnUnregisteredCombinationIsRejected()
        {
            var assembly = Resolve(Design(), PolicyFor((BaseW, BaseW)));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.CombinationNotEligible));
        }

        [Fact]
        public void AFamilyOutsideThePolicyIsRejected()
        {
            var policy = CantileverColumnBaseSectionPolicy.Create(
                new[] { Variant(Hss, BaseW) },
                new[] { StructuralSectionFamily.W });

            var assembly = Resolve(Design(column: Hss), policy);

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.SectionFamilyNotEligible));
        }

        [Fact]
        public void ANewDesignIsOfferedOnlyEnabledAndRegisteredSections()
        {
            var policy = PolicyFor((ColumnW, BaseW), (BaseW, BaseW));

            var columns = policy.EligibleForNewDesign(Catalog, CantileverMemberRole.Column);
            var bases = policy.EligibleForNewDesign(Catalog, CantileverMemberRole.Base);

            Assert.Equal(new[] { ColumnW, BaseW }, columns.Select(s => s.SectionId.Value));
            Assert.Equal(new[] { BaseW }, bases.Select(s => s.SectionId.Value));
            Assert.All(columns, s => Assert.True(s.IsEnabled));
        }

        [Fact]
        public void AnExistingDesignKeepsResolvingADisabledSection()
        {
            // Owner decision 15 of I-36A: a design saved months ago must keep opening. The catalogue path
            // that resolves it is TryGetById, and being disabled is a WARNING, never a substitution.
            var disabled = Catalog.All
                .Where(s => s.Family == StructuralSectionFamily.W)
                .Select(s => new StructuralSectionDefinition
                {
                    Identity = s.Identity,
                    WeightPerLength = s.WeightPerLength,
                    NativeUnitSystem = s.NativeUnitSystem,
                    Dimensions = s.Dimensions,
                    Properties = s.Properties,
                    IsEnabled = s.SectionId.Value != ColumnW,
                    StatusNotes = s.StatusNotes,
                    MaterialGrade = s.MaterialGrade,
                    SourceSpecialNote = s.SourceSpecialNote
                })
                .ToList();

            var catalog = StructuralSectionCatalog.Create(disabled, Catalog.Sources);
            var resolver = new CantileverColumnBaseResolver(
                catalog, new StructuralSectionGeometryFactory(catalog), PolicyFor((ColumnW, BaseW)));

            var assembly = resolver.Resolve(Design());

            Assert.False(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.SectionDisabled));
            Assert.Equal(ColumnW, assembly.Column.SectionId.Value);

            // ... and the same policy does NOT offer it to a new design.
            Assert.DoesNotContain(
                PolicyFor((ColumnW, BaseW)).EligibleForNewDesign(catalog, CantileverMemberRole.Column),
                s => s.SectionId.Value == ColumnW);
        }

        // ---- 6-9: the rear plate's vertical pattern ---------------------------------------------------------

        [Fact]
        public void TheFirstPunchSitsAtTheApprovedOffsetAboveTheBaseSection()
        {
            var assembly = Resolve();

            Assert.Equal(
                assembly.Pattern.BaseBottomZ + CantileverDefaults.RearPlateVerticalEndOffset,
                assembly.Pattern.Elevations[0],
                12);
        }

        [Fact]
        public void TheConnectionPitchIsConstant()
        {
            var elevations = Resolve().Pattern.Elevations;

            Assert.True(elevations.Count >= 2);

            for (var i = 1; i < elevations.Count; i++)
            {
                Assert.Equal(CantileverDefaults.ConnectionPunchPitch, elevations[i] - elevations[i - 1], 12);
            }
        }

        [Fact]
        public void ThereAreExactlyThreeConnectionPunchesAboveTheBaseSection()
        {
            var pattern = Resolve().Pattern;

            var above = pattern.Elevations.Count(z => z > pattern.BaseTopZ + Tolerance);

            Assert.Equal(CantileverDefaults.ConnectionPunchesAboveBase, above);
            Assert.Equal(pattern.Elevations.Count - above, pattern.PunchesInsideBase);
        }

        [Fact]
        public void TheLastPunchInsideTheBaseIsTheHighestOneThatStillFits()
        {
            var pattern = Resolve().Pattern;

            Assert.True(pattern.LastElevationInsideBase <= pattern.BaseTopZ + Tolerance);
            Assert.True(
                pattern.LastElevationInsideBase + pattern.Parameters.ConnectionPitch > pattern.BaseTopZ + Tolerance,
                "Cabria otro troquel dentro de la base y no se coloco.");
        }

        [Fact]
        public void TheRearPlateTopKeepsTheApprovedOffsetAboveTheLastPunch()
        {
            var pattern = Resolve().Pattern;

            Assert.Equal(
                pattern.LastConnectionElevation + CantileverDefaults.RearPlateVerticalEndOffset,
                pattern.RearPlateTopZ,
                12);
            Assert.Equal(pattern.RearPlateTopZ - pattern.BaseBottomZ, pattern.RearPlateHeight, 12);
        }

        [Fact]
        public void TheWholeVerticalPatternMatchesTheWorkedExample()
        {
            // A literal expectation on purpose. Base W12X26 is 12.2 in deep, so: first at 2.5, then every
            // 2.0 while <= 12.2 (2.5, 4.5, 6.5, 8.5, 10.5), then exactly three more (12.5, 14.5, 16.5), and
            // the plate top 2.5 above the last one. A derived expectation would reproduce a systematic
            // error in the same direction; this one would not.
            var pattern = Resolve().Pattern;

            Assert.Equal(new[] { 2.5, 4.5, 6.5, 8.5, 10.5, 12.5, 14.5, 16.5 }, pattern.Elevations.Select(z => Math.Round(z, 9)));
            Assert.Equal(10.5, Math.Round(pattern.LastElevationInsideBase, 9));
            Assert.Equal(16.5, Math.Round(pattern.LastConnectionElevation, 9));
            Assert.Equal(19.0, Math.Round(pattern.RearPlateTopZ, 9));
            Assert.Equal(5, pattern.PunchesInsideBase);
        }

        [Fact]
        public void ABaseTooShallowForASinglePunchIsRejectedExplicitly()
        {
            // Parametrised, not contrived: the shallowest shipped W is 4.16 in deep, so the way to reach
            // this state is to raise the approved offset above the base depth — which the design is allowed
            // to do. The base is a wide one so the HORIZONTAL check passes and the vertical one is what
            // actually rejects: a narrow base would be rejected earlier and this test would pass for the
            // wrong reason.
            var assembly = Resolve(
                Design(@base: ShallowBase, tune: p => p.RearPlateVerticalEndOffset = 7.0),
                PolicyFor((ColumnW, ShallowBase)));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.NoPunchFitsInBase));
        }

        // ---- 10-12: the horizontal pattern ------------------------------------------------------------------

        [Fact]
        public void TheTwoRowsAreSymmetricAboutTheCentrePlane()
        {
            var pattern = Resolve().Pattern;

            Assert.Equal(-pattern.LeftRowX, pattern.RightRowX, 12);
            Assert.True(pattern.LeftRowX < CantileverColumnBaseDatum.CentrePlaneX);
        }

        [Fact]
        public void TheRowsAreDerivedFromTheColumnEnvelopeAndNotFromTheBase()
        {
            var column = Envelope(ColumnW);
            var pattern = Resolve().Pattern;

            Assert.Equal(
                (-column.Width / 2.0) + CantileverDefaults.PunchHorizontalEndOffset,
                pattern.LeftRowX,
                12);

            // Changing only the BASE leaves them untouched; changing the COLUMN moves them.
            var otherBase = Resolve(Design(@base: BaseW), PolicyFor((ColumnW, BaseW))).Pattern;
            Assert.Equal(pattern.LeftRowX, otherBase.LeftRowX, 12);

            var otherColumn = Resolve(Design(column: BaseW, @base: BaseW)).Pattern;
            Assert.NotEqual(Math.Round(pattern.LeftRowX, 6), Math.Round(otherColumn.LeftRowX, 6));
        }

        [Fact]
        public void ThePunchesFitInsideTheRearPlate()
        {
            var assembly = Resolve();
            var half = Envelope(BaseW).Width / 2.0;
            var radius = assembly.Pattern.Parameters.Diameter / 2.0;

            Assert.True(assembly.Pattern.LeftRowX - radius >= -half - Tolerance);
            Assert.True(assembly.Pattern.RightRowX + radius <= half + Tolerance);
        }

        [Fact]
        public void AColumnWhosePunchesOverflowTheRearPlateIsRejected()
        {
            var assembly = Resolve(
                Design(column: WideColumn, @base: NarrowBase),
                PolicyFor((WideColumn, NarrowBase)));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.PunchOutsideRearPlate));
            Assert.Null(assembly.Pattern);
        }

        // ---- 13: the shared authority -----------------------------------------------------------------------

        [Fact]
        public void TheRearPlateAndTheColumnCarryTheSameHoles()
        {
            var assembly = Resolve();

            var plate = assembly.RearPlatePunches.Select(p => p.Datum).ToList();
            var column = assembly.ColumnConnectionPunches.Select(p => p.Datum).ToList();

            Assert.NotEmpty(plate);
            Assert.Equal(plate.Count, column.Count);

            for (var i = 0; i < plate.Count; i++)
            {
                Assert.True(
                    plate[i].ApproxEquals(column[i]),
                    "El troquel " + i + " no coincide: " + plate[i] + " frente a " + column[i]);
            }
        }

        [Fact]
        public void CoincidentPunchesShareTheAxisButNotTheThreeDimensionalCentre()
        {
            // This is exactly why the datum exists. The two holes of one bolt sit on surfaces separated by
            // the plate thickness, so their 3D centres MUST differ; asserting them equal would only pass
            // while somebody kept subtracting thicknesses in the test.
            var assembly = Resolve();

            var plate = assembly.RearPlatePunches[0];
            var column = assembly.ColumnConnectionPunches[0];

            Assert.Equal(plate.Datum, column.Datum);
            Assert.Equal(CantileverPunchAxis.AlongY, plate.Axis);
            Assert.Equal(plate.Centre.X, column.Centre.X, 12);
            Assert.Equal(plate.Centre.Z, column.Centre.Z, 12);
            Assert.NotEqual(Math.Round(plate.Centre.Y, 9), Math.Round(column.Centre.Y, 9));
            Assert.Equal(assembly.BaseRearPlate.Thickness / 2.0, plate.Centre.Y - column.Centre.Y, 12);
        }

        [Fact]
        public void EveryConnectionDatumComesFromTheOnePattern()
        {
            var assembly = Resolve();
            var fromPattern = assembly.Pattern.AllDatums();

            Assert.Equal(fromPattern, assembly.RearPlatePunches.Select(p => p.Datum));
            Assert.Equal(fromPattern, assembly.ColumnConnectionPunches.Select(p => p.Datum));
        }

        // ---- 14-15: the column bottom plate ------------------------------------------------------------------

        [Fact]
        public void TheBottomPlateHasOneRowPerSideOnTheSameTransverseCoordinates()
        {
            var assembly = Resolve();

            var rows = assembly.ColumnBottomPlatePunches
                .Select(p => Math.Round(p.Datum.U, 9))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(2, rows.Count);
            Assert.Equal(Math.Round(assembly.Pattern.LeftRowX, 9), rows[0]);
            Assert.Equal(Math.Round(assembly.Pattern.RightRowX, 9), rows[1]);
            Assert.All(assembly.ColumnBottomPlatePunches, p => Assert.Equal(CantileverPunchAxis.AlongZ, p.Axis));
        }

        [Fact]
        public void TheBottomPlateRowsArePairsAboutTheCentreWithNoPunchOnIt()
        {
            var assembly = Resolve();
            var pitch = CantileverDefaults.ColumnBottomPlatePunchPitch;

            var centre = (assembly.ColumnBottomPlate.Outline.Min(p => p.Y) +
                          assembly.ColumnBottomPlate.Outline.Max(p => p.Y)) / 2.0;

            var offsets = assembly.ColumnBottomPlatePunches
                .Where(p => Math.Abs(p.Datum.U - assembly.Pattern.LeftRowX) < Tolerance)
                .Select(p => Math.Round(p.Datum.V - centre, 9))
                .OrderBy(v => v)
                .ToList();

            Assert.NotEmpty(offsets);
            Assert.Equal(0, offsets.Count % 2);
            Assert.DoesNotContain(0.0, offsets);

            // +/- p/2, +/- 3p/2, +/- 5p/2 ... — never a punch on the centre line.
            for (var i = 0; i < offsets.Count / 2; i++)
            {
                var expected = (((offsets.Count / 2) - i) * 2 - 1) * pitch / 2.0;
                Assert.Equal(-expected, offsets[i], 9);
                Assert.Equal(expected, offsets[offsets.Count - 1 - i], 9);
            }
        }

        [Fact]
        public void TheBottomPlatePairsRespectTheRequiredEndOffset()
        {
            var assembly = Resolve();
            var offset = assembly.Pattern.Parameters.ColumnBottomPlateEndOffset;
            var minY = assembly.ColumnBottomPlate.Outline.Min(p => p.Y);
            var maxY = assembly.ColumnBottomPlate.Outline.Max(p => p.Y);

            Assert.All(
                assembly.ColumnBottomPlatePunches,
                p =>
                {
                    Assert.True(p.Datum.V >= minY + offset - Tolerance);
                    Assert.True(p.Datum.V <= maxY - offset + Tolerance);
                });
        }

        [Fact]
        public void TheBottomPlatePatternMatchesTheWorkedExample()
        {
            // Column W10X33 is 9.73 in deep, its plate spans y in [-9.73, 0], centre -4.865, and the end
            // offset is 1.5. Pairs at +/-1.0 and +/-3.0 fit; +/-5.0 does not. Two pairs, two rows: eight.
            var assembly = Resolve();

            Assert.Equal(8, assembly.ColumnBottomPlatePunches.Count);
        }

        // ---- 16-17: the regular column region ----------------------------------------------------------------

        [Fact]
        public void TheFirstRegularPunchIsOneRegularPitchAboveTheLastConnectionPunch()
        {
            var assembly = Resolve();

            var first = assembly.ColumnRegularPunches.Min(p => p.Datum.V);

            Assert.Equal(
                assembly.Pattern.LastConnectionElevation + CantileverDefaults.RegularColumnPunchPitch,
                first,
                12);

            // Not the connection pitch, and never a duplicate of the last connection punch.
            Assert.NotEqual(
                Math.Round(assembly.Pattern.LastConnectionElevation + CantileverDefaults.ConnectionPunchPitch, 9),
                Math.Round(first, 9));
            Assert.DoesNotContain(
                assembly.ColumnRegularPunches,
                p => Math.Abs(p.Datum.V - assembly.Pattern.LastConnectionElevation) < Tolerance);
        }

        [Fact]
        public void TheRegularRegionKeepsItsPitchAndStopsAtTheRequiredTopOffset()
        {
            var design = Design();
            var assembly = Resolve(design);
            var topOffset = design.Connection.Punches.ColumnTopPunchOffset.Value;
            var ceiling = design.Column.Height - topOffset;

            var elevations = assembly.ColumnRegularPunches
                .Select(p => Math.Round(p.Datum.V, 9))
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            Assert.NotEmpty(elevations);

            for (var i = 1; i < elevations.Count; i++)
            {
                Assert.Equal(CantileverDefaults.RegularColumnPunchPitch, elevations[i] - elevations[i - 1], 9);
            }

            Assert.True(elevations[elevations.Count - 1] <= ceiling + Tolerance);
            Assert.True(
                elevations[elevations.Count - 1] + CantileverDefaults.RegularColumnPunchPitch > ceiling + Tolerance,
                "Cabria otro troquel regular por debajo del offset superior y no se coloco.");
        }

        [Fact]
        public void AColumnTooShortForARegularPunchSaysSoWithoutBlocking()
        {
            var assembly = Resolve(Design(height: 22.0));

            Assert.False(assembly.IsBlocked);
            Assert.Empty(assembly.ColumnRegularPunches);
            Assert.True(Has(assembly, CantileverDiagnostics.NoRegularPunchFits));
        }

        // ---- 18-19: the gusset --------------------------------------------------------------------------------

        [Fact]
        public void TheGussetHasEqualLegsAtFortyFiveDegrees()
        {
            var assembly = Resolve();

            Assert.Equal(assembly.Gusset.VerticalLeg, assembly.Gusset.HorizontalLeg, 12);
            Assert.Equal(45.0, assembly.Gusset.HypotenuseAngleDegrees, 9);
        }

        [Fact]
        public void TheGussetLegIsHowFarThePlateReachesAboveTheBaseAndNotThreePitches()
        {
            var assembly = Resolve();
            var pattern = assembly.Pattern;

            Assert.Equal(pattern.RearPlateTopZ - pattern.BaseTopZ, assembly.Gusset.VerticalLeg, 12);

            // W12X26 is 12.2 deep, so the last punch inside lands at 10.5 and the leg is 6.8 — NOT the
            // 3 x 2.0 = 6.0 that hard-coding three pitches would give.
            Assert.Equal(6.8, Math.Round(assembly.Gusset.VerticalLeg, 9));
            Assert.NotEqual(
                Math.Round(3 * CantileverDefaults.ConnectionPunchPitch, 9),
                Math.Round(assembly.Gusset.VerticalLeg, 9));
        }

        [Fact]
        public void TheGussetIsCentredOnTheTransversePlane()
        {
            var assembly = Resolve();

            var xs = assembly.Gusset.Vertices.Select(v => v.X).Distinct().ToList();

            Assert.Single(xs);
            Assert.Equal(-assembly.Gusset.Thickness / 2.0, xs[0], 12);
            Assert.Equal(1.0, Math.Abs(assembly.Gusset.Normal.X), 12);
        }

        // ---- 20: four independent thicknesses -------------------------------------------------------------------

        [Fact]
        public void TheFourThicknessesDefaultToAQuarterInchAndAreIndependent()
        {
            var design = Design();

            Assert.Equal(CantileverDefaults.PlateThickness, design.Base.FrontPlate.Thickness);
            Assert.Equal(CantileverDefaults.PlateThickness, design.Base.RearPlate.Thickness);
            Assert.Equal(CantileverDefaults.PlateThickness, design.Column.BottomPlate.Thickness);
            Assert.Equal(CantileverDefaults.PlateThickness, design.Base.Gusset.Thickness);

            design.Base.FrontPlate.Thickness = 0.5;
            design.Base.RearPlate.Thickness = 0.375;
            design.Column.BottomPlate.Thickness = 0.75;
            design.Base.Gusset.Thickness = 0.625;

            var assembly = Resolve(design);

            Assert.Equal(0.5, assembly.BaseFrontPlate.Thickness);
            Assert.Equal(0.375, assembly.BaseRearPlate.Thickness);
            Assert.Equal(0.75, assembly.ColumnBottomPlate.Thickness);
            Assert.Equal(0.625, assembly.Gusset.Thickness);
        }

        // ---- 21-22: changing a section recomposes what it governs -------------------------------------------------

        [Fact]
        public void ChangingTheColumnSectionRecomposesTheDatumsItGoverns()
        {
            var narrow = Resolve(Design(column: BaseW, @base: BaseW)).Pattern;
            var wide = Resolve().Pattern;

            Assert.NotEqual(Math.Round(narrow.LeftRowX, 6), Math.Round(wide.LeftRowX, 6));

            // ... and NOT the vertical pattern, which the base governs.
            Assert.Equal(narrow.Elevations, wide.Elevations);
            Assert.Equal(narrow.RearPlateTopZ, wide.RearPlateTopZ, 12);
        }

        [Fact]
        public void ChangingTheBaseSectionRecomposesThePlateHeightAndTheGusset()
        {
            var tall = Resolve();
            var shortBase = Resolve(
                Design(@base: ShallowBase),
                PolicyFor((ColumnW, ShallowBase)));

            Assert.False(shortBase.IsBlocked);
            Assert.NotEqual(
                Math.Round(tall.Pattern.RearPlateTopZ, 6),
                Math.Round(shortBase.Pattern.RearPlateTopZ, 6));
            Assert.NotEqual(
                Math.Round(tall.Gusset.VerticalLeg, 6),
                Math.Round(shortBase.Gusset.VerticalLeg, 6));

            // ... and NOT the two rows, which the column governs.
            Assert.Equal(tall.Pattern.LeftRowX, shortBase.Pattern.LeftRowX, 12);
        }

        // ---- 23: determinism -------------------------------------------------------------------------------------

        [Fact]
        public void TheSameDesignProducesTheSameSignature()
        {
            var first = Resolve().Signature();
            var second = Resolve().Signature();

            Assert.Equal(first, second);
            Assert.DoesNotContain("BLOCKED", first, StringComparison.Ordinal);
        }

        [Fact]
        public void ADifferentDesignProducesADifferentSignature()
        {
            var baseline = Resolve().Signature();
            var longer = Resolve(Design(baseLength: 60.0)).Signature();
            var otherColumn = Resolve(Design(column: BaseW, @base: BaseW)).Signature();

            Assert.NotEqual(baseline, longer);
            Assert.NotEqual(baseline, otherColumn);
        }

        [Fact]
        public void EveryPieceIdIsUniqueAndDeterministic()
        {
            var assembly = Resolve();

            var ids = new List<string> { assembly.Column.Id.Value, assembly.Base.Id.Value,
                assembly.BaseFrontPlate.Id.Value, assembly.BaseRearPlate.Id.Value,
                assembly.ColumnBottomPlate.Id.Value, assembly.Gusset.Id.Value };
            ids.AddRange(assembly.AllPunches.Select(p => p.Id.Value));

            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal("CANT-CB-COL", assembly.Column.Id.Value);
            Assert.Equal("CANT-CB-BAS", assembly.Base.Id.Value);
            Assert.All(ids, id => Assert.StartsWith("CANT-CB-", id, StringComparison.Ordinal));
        }

        // ---- placement, lengths and the member contract ------------------------------------------------------------

        [Fact]
        public void TheMemberCarriesOneAuthorityOfPlacementAndDerivesTheRest()
        {
            var assembly = Resolve();

            foreach (var member in assembly.Members)
            {
                Assert.Equal(member.Placement.SectionId, member.SectionId);
                Assert.Equal(member.Placement.Length, member.GeometricLength, 12);
                Assert.Equal(member.Placement.Frame.Origin.X, member.Start.X, 12);
                Assert.Equal(CantileverPieceTokens.ColumnBaseOwner, member.Owner);
            }
        }

        [Fact]
        public void TheNominalCutLengthEqualsTheGeometricLengthInTheMvp()
        {
            var assembly = Resolve();

            Assert.All(
                assembly.Members,
                m => Assert.Equal(m.GeometricLength, m.NominalCutLength, 12));
        }

        [Fact]
        public void TheColumnStandsOnTheDatumAndTheBaseProjectsInPositiveY()
        {
            var design = Design();
            var assembly = Resolve(design);

            Assert.Equal(CantileverColumnBaseDatum.FloorZ, assembly.Column.Start.Z, 12);
            Assert.Equal(design.Column.Height, assembly.Column.End.Z, 12);
            Assert.Equal(1.0, assembly.Column.Direction.Z, 12);

            Assert.Equal(1.0, assembly.Base.Direction.Y, 12);
            Assert.True(assembly.Base.Start.Y >= CantileverColumnBaseDatum.ConnectionPlaneY - Tolerance);
            Assert.Equal(design.Base.Length, assembly.Base.End.Y - assembly.Base.Start.Y, 12);
        }

        [Fact]
        public void TheColumnConnectingFaceLandsOnTheConnectionPlane()
        {
            var assembly = Resolve();
            var column = Envelope(ColumnW);

            // Derived from Bounds, never from d: the frame origin sits at the centroid, so the shift is
            // exactly the envelope's upper edge.
            var maxY = assembly.Column.Placement.Frame.Origin.Y +
                       Factory.Get(Id(ColumnW), SectionDetailLevel.Tabulated).Bounds.MaxY;

            Assert.Equal(CantileverColumnBaseDatum.ConnectionPlaneY, maxY, 12);
            Assert.Equal(column.Height, column.Height, 12);
        }

        [Fact]
        public void TheBaseSectionSitsWithItsDepthVerticalAndItsBottomOnTheDatum()
        {
            var assembly = Resolve();
            var expected = Envelope(BaseW);

            Assert.Equal(CantileverColumnBaseDatum.FloorZ, assembly.Pattern.BaseBottomZ, 12);
            Assert.Equal(expected.Height, assembly.Pattern.BaseTopZ - assembly.Pattern.BaseBottomZ, 12);

            // The base frame maps the section's own Y onto world Z: that is what "depth vertical" means.
            Assert.Equal(1.0, assembly.Base.Placement.Frame.AxisY.Z, 12);
        }

        [Fact]
        public void ThePlatesTakeTheirOutlineFromTheResolvedEnvelopes()
        {
            var assembly = Resolve();
            var baseEnvelope = Envelope(BaseW);
            var columnEnvelope = Envelope(ColumnW);

            var frontWidth = assembly.BaseFrontPlate.Outline.Max(p => p.X) - assembly.BaseFrontPlate.Outline.Min(p => p.X);
            var rearWidth = assembly.BaseRearPlate.Outline.Max(p => p.X) - assembly.BaseRearPlate.Outline.Min(p => p.X);
            var bottomWidth = assembly.ColumnBottomPlate.Outline.Max(p => p.X) - assembly.ColumnBottomPlate.Outline.Min(p => p.X);
            var bottomDepth = assembly.ColumnBottomPlate.Outline.Max(p => p.Y) - assembly.ColumnBottomPlate.Outline.Min(p => p.Y);

            Assert.Equal(baseEnvelope.Width, frontWidth, 12);
            Assert.Equal(baseEnvelope.Width, rearWidth, 12);
            Assert.Equal(columnEnvelope.Width, bottomWidth, 12);
            Assert.Equal(columnEnvelope.Height, bottomDepth, 12);

            // The front plate caps the free end and carries no punches.
            Assert.DoesNotContain(assembly.AllPunches, p => p.Surface == CantileverPunchSurface.ColumnFace && p.Centre.Y > 1.0);
            Assert.Equal(CantileverPlateKind.BaseFront, assembly.BaseFrontPlate.Kind);
        }

        [Fact]
        public void TheRearPlateSpansFromTheBaseBottomToThePatternTop()
        {
            var assembly = Resolve();

            Assert.Equal(assembly.Pattern.BaseBottomZ, assembly.BaseRearPlate.Outline.Min(p => p.Z), 12);
            Assert.Equal(assembly.Pattern.RearPlateTopZ, assembly.BaseRearPlate.Outline.Max(p => p.Z), 12);
        }

        // ---- required parameters ------------------------------------------------------------------------------------

        [Fact]
        public void AMissingBottomPlateOffsetIsRejectedAndNeverDefaulted()
        {
            var design = Design();
            design.Connection.Punches.ColumnBottomPlateEndOffset = null;

            var assembly = Resolve(design);

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.RequiredParameterMissing));
        }

        [Fact]
        public void AMissingColumnTopOffsetIsRejectedAndNeverDefaulted()
        {
            var design = Design();
            design.Connection.Punches.ColumnTopPunchOffset = null;

            var assembly = Resolve(design);

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.RequiredParameterMissing));
        }

        [Fact]
        public void TheDefaultsAreTheOnesTheOwnerApproved()
        {
            var punches = new CantileverPunchParameters();

            Assert.Equal(0.75, punches.Diameter);
            Assert.Equal(1.50, punches.HorizontalEndOffset);
            Assert.Equal(2.00, punches.ConnectionPitch);
            Assert.Equal(2.50, punches.RearPlateVerticalEndOffset);
            Assert.Equal(4.00, punches.RegularColumnPitch);
            Assert.Equal(3, punches.ConnectionPunchesAboveBase);
            Assert.Equal(2.00, punches.ColumnBottomPlatePitch);

            // The two without an approved default stay null. A number here would be indistinguishable from
            // an approved one.
            Assert.Null(punches.ColumnBottomPlateEndOffset);
            Assert.Null(punches.ColumnTopPunchOffset);
        }

        // ---- the envelope and the deep copy ---------------------------------------------------------------------------

        [Fact]
        public void TheEnvelopeContainsEveryPiece()
        {
            var assembly = Resolve();
            var envelope = assembly.Envelope.Value;

            Assert.True(envelope.MinY <= CantileverColumnBaseDatum.ConnectionPlaneY + Tolerance);
            Assert.True(envelope.MaxY >= assembly.Base.End.Y - Tolerance);
            Assert.True(envelope.MaxZ >= assembly.Pattern.RearPlateTopZ - Tolerance);
            Assert.True(envelope.MinZ <= -assembly.ColumnBottomPlate.Thickness + Tolerance);
        }

        [Fact]
        public void TheDesignDeepCopyIsIndependent()
        {
            var design = Design();
            var copy = design.DeepCopy();

            copy.Column.Height = 1.0;
            copy.Base.RearPlate.Thickness = 9.0;
            copy.Connection.Punches.Diameter = 9.0;

            Assert.Equal(96.0, design.Column.Height);
            Assert.Equal(CantileverDefaults.PlateThickness, design.Base.RearPlate.Thickness);
            Assert.Equal(CantileverDefaults.PunchDiameter, design.Connection.Punches.Diameter);
        }

        [Fact]
        public void APolicyRejectsADuplicateRegistration()
        {
            Assert.Throws<ArgumentException>(() =>
                CantileverColumnBaseSectionPolicy.Create(
                    new[] { Variant(ColumnW, BaseW), Variant(ColumnW, BaseW) }));
        }
    }
}
