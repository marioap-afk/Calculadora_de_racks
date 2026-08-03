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
        public void TheRowsAreDerivedFromTheREARPLATEAndNotFromTheColumn()
        {
            // ESTE ENUNCIADO SE INVIRTIO, y por decision del dueno: «es desde el exterior de la placa hacia el
            // centro de la columna 1 pulgada». Antes se acotaba desde la columna, y con 1 in eso empujaba los
            // agujeros FUERA de una placa mas angosta que ella. El nombre viejo era
            // TheRowsAreDerivedFromTheColumnEnvelopeAndNotFromTheBase.
            //
            // La columna sigue gobernando el PATRON —son sus filas— pero no el borde desde el que se mide.
            var plate = Envelope(BaseW);
            var pattern = Resolve().Pattern;

            Assert.Equal(
                (-plate.Width / 2.0) + CantileverDefaults.PunchHorizontalEndOffset,
                pattern.LeftRowX,
                12);

            // Cambiar solo la COLUMNA ya no las mueve; cambiar la BASE, que es de donde sale la placa, si.
            var otherColumn = Resolve(Design(column: BaseW, @base: BaseW)).Pattern;
            Assert.Equal(pattern.LeftRowX, otherColumn.LeftRowX, 12);

            var otherBase = Resolve(Design(column: ColumnW, @base: ColumnW), PolicyFor((ColumnW, ColumnW))).Pattern;
            Assert.NotEqual(Math.Round(pattern.LeftRowX, 6), Math.Round(otherBase.LeftRowX, 6));
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
        public void AColumnTooNARROWForThePlateThatGovernsItsRowsIsRejected()
        {
            // La misma comprobacion con los papeles cambiados. Al acotar desde la placa, la pieza que puede
            // quedarse corta ya no es la placa sino la COLUMNA: una base ancha manda las filas hacia afuera y
            // una columna estrecha no las alcanza. Antes se llamaba
            // AColumnWhosePunchesOverflowTheRearPlateIsRejected y emparejaba columna ancha con base estrecha.
            var assembly = Resolve(
                Design(column: NarrowBase, @base: WideColumn),
                PolicyFor((NarrowBase, WideColumn)));

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

            // ApproxEquals, explicitly: physical coincidence is the GEOMETRIC question, and Equals is exact
            // value equality. They happen to agree here because both datums come from the one pattern, and
            // saying which one we mean is the point.
            Assert.True(plate.Datum.ApproxEquals(column.Datum));
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
        public void TheRegularRegionKeepsItsPitchAndStopsAtTheLastWholeHoleThatFits()
        {
            // I-37D ronda 2, motivo 1. El techo ya NO es un margen que el diseno escribe: es el ultimo agujero
            // ENTERO que cabe bajo el extremo fisico de la columna. El proposito de la prueba no cambia —el paso
            // se conserva y no se deja fuera un agujero que cabria—; cambia el limite que lo decide.
            var design = Design();
            var assembly = Resolve(design);
            var radius = design.Connection.Punches.Diameter / 2.0;
            var ceiling = design.Column.BottomPlate.Thickness + design.Column.Height - radius;

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
                "Cabria otro troquel regular entero y no se coloco.");
        }

        [Fact]
        public void AColumnTooShortForARegularPunchSaysSoWithoutBlocking()
        {
            // El primer troquel regular esta en z = 20.5 y su radio es 0.375, asi que el agujero entero mas bajo
            // exige que la columna llegue a 20.875. Con la placa de 0.25 levantandola, eso son 20.625 in de
            // corte. A 20.5 no cabe NINGUNO, y eso se avisa sin bloquear.
            var assembly = Resolve(Design(height: 20.5));

            Assert.False(assembly.IsBlocked);
            Assert.Empty(assembly.ColumnRegularPunches);
            Assert.True(Has(assembly, CantileverDiagnostics.NoRegularPunchFits));
        }

        [Fact]
        public void UnaColumnaQueSoloAdmiteUnAgujeroEnteroColocaEXACTAMENTEUno()
        {
            // 20.75 in de corte + 0.25 de placa = 21 in de tope: cabe el de 20.5 y no el de 24.5.
            var assembly = Resolve(Design(height: 20.75));

            Assert.False(assembly.IsBlocked);
            Assert.Single(assembly.ColumnRegularPunches.Select(p => p.Datum.V).Distinct());
            Assert.Equal(20.5, assembly.ColumnRegularPunches.First().Datum.V, 9);
        }

        [Fact]
        public void UnAgujeroTANGENTEAlExtremoSuperiorSEINCLUYE()
        {
            // Tope en 92.875 = 92.5 + 0.375: el agujero toca el borde exactamente y CABE. Con la placa de
            // 0.25 levantando la columna, ese tope son 92.625 in de corte.
            var assembly = Resolve(Design(height: 92.625));
            var elevations = assembly.ColumnRegularPunches.Select(p => Math.Round(p.Datum.V, 9)).Distinct().ToList();

            Assert.Contains(92.5, elevations);
        }

        [Fact]
        public void ElSiguienteAgujeroQueINVADEElExtremoSEEXCLUYE()
        {
            // Una milesima menos que la tangencia: el mismo agujero ya no cabe entero y se queda fuera.
            var assembly = Resolve(Design(height: 92.624));
            var elevations = assembly.ColumnRegularPunches.Select(p => Math.Round(p.Datum.V, 9)).Distinct().ToList();

            Assert.DoesNotContain(92.5, elevations);
            Assert.Contains(88.5, elevations);
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
        public void ChangingOnlyTheCOLUMNMovesNOTHINGOfThePattern()
        {
            // Tras la correccion del dueno la columna no gobierna NINGUNA de las dos coordenadas del patron:
            // la horizontal la da la placa y la vertical la da la base. Cambiar solo la columna deja el patron
            // entero igual, y esa es ahora la afirmacion util. Antes era
            // ChangingTheColumnSectionRecomposesTheDatumsItGoverns.
            var narrow = Resolve(Design(column: BaseW, @base: BaseW)).Pattern;
            var wide = Resolve().Pattern;

            Assert.Equal(narrow.LeftRowX, wide.LeftRowX, 12);
            Assert.Equal(narrow.RightRowX, wide.RightRowX, 12);
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

            // ... y TAMBIEN las dos filas, que ahora se acotan desde la placa y la placa sale de la base.
            // Antes esta linea afirmaba lo contrario, porque el datum era la columna.
            Assert.NotEqual(
                Math.Round(tall.Pattern.LeftRowX, 6),
                Math.Round(shortBase.Pattern.LeftRowX, 6));
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
        public void TheColumnStandsOnItsBottomPlateAndTheBaseProjectsInPositiveY()
        {
            // Correccion del datum (I-37D): la columna arranca en la CARA SUPERIOR de su placa inferior, no en
            // el piso. Su longitud NOMINAL no cambia: la placa la levanta, no la alarga.
            var design = Design();
            var assembly = Resolve(design);
            var thickness = design.Column.BottomPlate.Thickness;

            Assert.Equal(
                CantileverColumnBaseDatum.ColumnBottomPlateTopZ(thickness), assembly.Column.Start.Z, 12);
            Assert.Equal(thickness + design.Column.Height, assembly.Column.End.Z, 12);
            Assert.Equal(design.Column.Height, assembly.Column.End.Z - assembly.Column.Start.Z, 12);
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
        public void TheDefaultsAreTheOnesTheOwnerApproved()
        {
            var punches = new CantileverPunchParameters();

            Assert.Equal(0.75, punches.Diameter);
            // UNA pulgada: el dueno declaro que 1.5 era un error suyo y la corrigio.
            Assert.Equal(1.00, punches.HorizontalEndOffset);
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
            // La placa se apoya en el piso, asi que nada baja de z = 0.
            Assert.True(envelope.MinZ >= CantileverColumnBaseDatum.FloorZ - Tolerance);
            Assert.True(envelope.MinZ <= CantileverColumnBaseDatum.FloorZ + Tolerance);
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

        // ---- value semantics of CantileverPunchDatum -------------------------------------------------------
        //
        // Equals used to delegate to ApproxEquals while GetHashCode rounded to six decimals. That is not a
        // valid equality for a value type: it is intransitive, and two values that compared "equal" could
        // land in different hash buckets while two that were NOT equal shared one.

        [Fact]
        public void TwoDatumsThatDifferBelowTheToleranceAreApproxEqualButNotEqual()
        {
            var a = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 1.0, 2.0, 0.75);
            var b = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 1.0 + 5e-10, 2.0, 0.75);

            Assert.True(a.ApproxEquals(b));      // the geometric question
            Assert.False(a.Equals(b));           // the value question — this FAILED before the fix
            Assert.True(a != b);
        }

        [Fact]
        public void EqualityIsConsistentWithTheHashAcrossARoundingBoundary()
        {
            // Both values round to the same six decimals, so the old hash put them in the same bucket while
            // Equals — being tolerant — also said equal. The pair that breaks it is this one: they are
            // 1e-7 apart, which the OLD hash collapsed and the OLD Equals rejected (1e-7 > 1e-9 tolerance).
            // That combination is the defect: equal-by-hash, unequal-by-Equals.
            var a = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 0.5000000, 1.0, 0.75);
            var b = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 0.5000001, 1.0, 0.75);

            Assert.False(a.Equals(b));
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
            Assert.False(a.ApproxEquals(b));

            // And an exactly equal pair must agree on both.
            var c = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 0.5000001, 1.0, 0.75);
            Assert.True(b.Equals(c));
            Assert.Equal(b.GetHashCode(), c.GetHashCode());
        }

        [Fact]
        public void ADatumSurvivesADictionaryAndADistinct()
        {
            // The consistency that the old implementation could not guarantee.
            var a = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 1.0, 2.0, 0.75);
            var same = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 1.0, 2.0, 0.75);
            var nearly = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 1.0 + 5e-10, 2.0, 0.75);

            var set = new HashSet<CantileverPunchDatum> { a, same, nearly };

            Assert.Equal(2, set.Count);
            Assert.Contains(a, set);

            var map = new Dictionary<CantileverPunchDatum, string> { [a] = "primero" };
            Assert.Equal("primero", map[same]);
            Assert.False(map.ContainsKey(nearly));
        }

        [Fact]
        public void TheAxisIsPartOfTheIdentity()
        {
            var alongY = new CantileverPunchDatum(CantileverPunchAxis.AlongY, 1.0, 2.0, 0.75);
            var alongZ = new CantileverPunchDatum(CantileverPunchAxis.AlongZ, 1.0, 2.0, 0.75);

            Assert.False(alongY.Equals(alongZ));
            Assert.False(alongY.ApproxEquals(alongZ));
        }

        [Fact]
        public void ADatumRejectsAnUndefinedAxis()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CantileverPunchDatum((CantileverPunchAxis)99, 1.0, 2.0, 0.75));
        }

        [Fact]
        public void EveryPunchDirectionComesFromADeclaredAxis()
        {
            var assembly = Resolve();

            foreach (var punch in assembly.AllPunches)
            {
                var direction = punch.Direction;

                Assert.Equal(1.0, Math.Abs(direction.X) + Math.Abs(direction.Y) + Math.Abs(direction.Z), 12);
                Assert.True(
                    punch.Axis == CantileverPunchAxis.AlongY || punch.Axis == CantileverPunchAxis.AlongZ,
                    "Un troquel llego con un eje no declarado.");
            }
        }

        // ---- the registered orientation is a real authority -------------------------------------------------
        //
        // CantileverColumnBaseVariant declared ColumnOrientation and BaseOrientation and the resolver built
        // fixed frames anyway, so registering a different orientation changed nothing while looking as if it
        // had. CantileverColumnBaseFrameResolver is now the only place a frame is built.

        [Fact]
        public void TheFrameAuthorityReproducesTheCurrentColumnFrameExactly()
        {
            var geometry = Factory.Get(Id(ColumnW), SectionDetailLevel.Tabulated);
            var bounds = geometry.Bounds;

            // La autoridad recibe ahora la elevacion de ARRANQUE de la columna: la cara superior de su placa
            // inferior. Con espesor cero coincide con el piso, que es como estaba antes de la correccion.
            var thickness = Design().Column.BottomPlate.Thickness;
            var start = CantileverColumnBaseDatum.ColumnStartZ(thickness);

            var fromAuthority = CantileverColumnBaseFrameResolver.ColumnFrame(
                CantileverColumnOrientation.DepthAlongBase, geometry, start);

            var expected = LocalFrame3D.Create(
                new Point3D(
                    -bounds.Center.X,
                    CantileverColumnBaseDatum.ConnectionPlaneY - bounds.MaxY,
                    start),
                Vector3D.UnitZ,
                Vector3D.UnitX);

            AssertSameFrame(expected, fromAuthority);

            // ... and it is the frame the resolver actually placed the column with.
            AssertSameFrame(expected, Resolve().Column.Placement.Frame);
        }

        [Fact]
        public void TheFrameAuthorityReproducesTheCurrentBaseFrameExactly()
        {
            var geometry = Factory.Get(Id(BaseW), SectionDetailLevel.Tabulated);
            var bounds = geometry.Bounds;
            var rear = CantileverDefaults.PlateThickness;

            var fromAuthority = CantileverColumnBaseFrameResolver.BaseFrame(
                CantileverBaseOrientation.DepthVertical, geometry, rear);

            var expected = LocalFrame3D.Create(
                new Point3D(
                    bounds.Center.X,
                    CantileverColumnBaseDatum.ConnectionPlaneY + rear,
                    CantileverColumnBaseDatum.FloorZ - bounds.MinY),
                Vector3D.UnitY,
                Vector3D.UnitZ.Cross(Vector3D.UnitY));

            AssertSameFrame(expected, fromAuthority);
            AssertSameFrame(expected, Resolve().Base.Placement.Frame);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndefinedColumnOrientationIsRejectedAndNeverFallsBackToTheHistoricFrame(int raw)
        {
            var geometry = Factory.Get(Id(ColumnW), SectionDetailLevel.Tabulated);

            // The whole point: a value the enum does not declare must NOT silently produce the frame that
            // DepthAlongBase produces. Before the extraction there was no branch at all, so it always did.
            Assert.False(CantileverColumnBaseFrameResolver.IsSupported((CantileverColumnOrientation)raw));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CantileverColumnBaseFrameResolver.ColumnFrame(
                    (CantileverColumnOrientation)raw, geometry, CantileverColumnBaseDatum.FloorZ));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(99)]
        [InlineData(-1)]
        public void AnUndefinedBaseOrientationIsRejectedAndNeverFallsBackToTheHistoricFrame(int raw)
        {
            var geometry = Factory.Get(Id(BaseW), SectionDetailLevel.Tabulated);

            Assert.False(CantileverColumnBaseFrameResolver.IsSupported((CantileverBaseOrientation)raw));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CantileverColumnBaseFrameResolver.BaseFrame(
                    (CantileverBaseOrientation)raw, geometry, CantileverDefaults.PlateThickness));
        }

        [Fact]
        public void OnlyTheDeclaredOrientationsAreSupported()
        {
            // A future member added to either enum fails here until somebody writes its frame rule. That is
            // the guard against a new orientation quietly inheriting the historic behaviour.
            Assert.Equal(
                new[] { CantileverColumnOrientation.DepthAlongBase },
                Enum.GetValues(typeof(CantileverColumnOrientation)).Cast<CantileverColumnOrientation>());
            Assert.Equal(
                new[] { CantileverBaseOrientation.DepthVertical },
                Enum.GetValues(typeof(CantileverBaseOrientation)).Cast<CantileverBaseOrientation>());

            Assert.All(
                Enum.GetValues(typeof(CantileverColumnOrientation)).Cast<CantileverColumnOrientation>(),
                o => Assert.True(CantileverColumnBaseFrameResolver.IsSupported(o)));
            Assert.All(
                Enum.GetValues(typeof(CantileverBaseOrientation)).Cast<CantileverBaseOrientation>(),
                o => Assert.True(CantileverColumnBaseFrameResolver.IsSupported(o)));
        }

        [Fact]
        public void AVariantRegisteredWithAnUnsupportedOrientationIsRejectedWithADiagnostic()
        {
            // The user path: a bad REGISTRATION must read as a diagnostic, not as an exception escaping the
            // resolver.
            var policy = CantileverColumnBaseSectionPolicy.Create(
                new[]
                {
                    new CantileverColumnBaseVariant(
                        CantileverColumnBaseVariantKind.WFlangeConnected, Id(ColumnW), Id(BaseW),
                        (CantileverColumnOrientation)99)
                },
                new[] { StructuralSectionFamily.W });

            var assembly = Resolve(Design(), policy);

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.OrientationNotSupported));
        }

        private static void AssertSameFrame(LocalFrame3D expected, LocalFrame3D actual)
        {
            Assert.Equal(expected.Origin.X, actual.Origin.X, 15);
            Assert.Equal(expected.Origin.Y, actual.Origin.Y, 15);
            Assert.Equal(expected.Origin.Z, actual.Origin.Z, 15);
            Assert.True(expected.AxisX.ApproxEquals(actual.AxisX, 1e-15));
            Assert.True(expected.AxisY.ApproxEquals(actual.AxisY, 1e-15));
            Assert.True(expected.AxisZ.ApproxEquals(actual.AxisZ, 1e-15));
        }

        // ---- geometric compatibility of the punches ---------------------------------------------------------
        //
        // Offsets are edge-to-CENTRE, so a hole fits only if the offset is at least its RADIUS. Pitches are
        // centre-to-centre, so consecutive holes clear each other only at a whole DIAMETER.

        [Fact]
        public void TheApprovedDefaultsStillResolveUnchanged()
        {
            // The guard on the guards: none of the new validations may reject the approved configuration,
            // and the signature must be exactly what it was before they existed.
            var assembly = Resolve();

            Assert.False(assembly.IsBlocked);
            Assert.DoesNotContain(
                assembly.Diagnostics,
                d => d.Code == CantileverDiagnostics.EdgeOffsetBelowRadius ||
                     d.Code == CantileverDiagnostics.PitchBelowDiameter ||
                     d.Code == CantileverDiagnostics.PunchRowsOverlap);
            Assert.Equal(new[] { 2.5, 4.5, 6.5, 8.5, 10.5, 12.5, 14.5, 16.5 },
                assembly.Pattern.Elevations.Select(z => Math.Round(z, 9)));
            Assert.Equal(8, assembly.ColumnBottomPlatePunches.Count);
        }

        [Fact]
        public void AHoleThatSpillsPastAHorizontalEdgeIsRejected()
        {
            var assembly = Resolve(Design(tune: p => p.HorizontalEndOffset = p.Diameter / 2.0 - 0.01));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.EdgeOffsetBelowRadius));
        }

        [Fact]
        public void AHoleThatSpillsPastThePlateBottomEdgeIsRejected()
        {
            var assembly = Resolve(Design(tune: p => p.RearPlateVerticalEndOffset = p.Diameter / 2.0 - 0.01));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.EdgeOffsetBelowRadius));
        }

        [Fact]
        public void UnAgujeroQueSobresaleDelExtremoDeLaPlacaSeEXCLUYE()
        {
            // La placa inferior conserva TODOS los agujeros enteros que caben, y ninguno mas. El proposito de
            // la prueba se mantiene —un agujero que sobresaldria no se dibuja— pero ahora se EXCLUYE en vez de
            // bloquear, porque no hay un margen que validar: lo decide el radio contra el borde.
            var assembly = Resolve();
            var radius = assembly.Pattern.Parameters.Diameter / 2.0;
            var plate = assembly.ColumnBottomPlate.Envelope();

            Assert.NotEmpty(assembly.ColumnBottomPlatePunches);

            foreach (var punch in assembly.ColumnBottomPlatePunches)
            {
                Assert.True(punch.Datum.V - radius >= plate.MinY - Tolerance,
                    "Un troquel de la placa inferior sobresale por el extremo bajo.");
                Assert.True(punch.Datum.V + radius <= plate.MaxY + Tolerance,
                    "Un troquel de la placa inferior sobresale por el extremo alto.");
            }
        }

        [Fact]
        public void LaPlacaInferiorConservaElMAXIMODeAgujerosEnterosQueCaben()
        {
            // Y no uno menos: si cupiera otro paso completo dentro de la placa, faltaria.
            var assembly = Resolve();
            var radius = assembly.Pattern.Parameters.Diameter / 2.0;
            var pitch = assembly.Pattern.Parameters.ColumnBottomPlatePitch;
            var plate = assembly.ColumnBottomPlate.Envelope();

            var centres = assembly.ColumnBottomPlatePunches
                .Select(p => Math.Round(p.Datum.V, 9)).Distinct().OrderBy(v => v).ToList();

            Assert.True(centres.Count >= 2);
            Assert.True(centres.First() - pitch - radius < plate.MinY - Tolerance,
                "Cabria otro troquel entero por debajo del primero.");
            Assert.True(centres.Last() + pitch + radius > plate.MaxY + Tolerance,
                "Cabria otro troquel entero por encima del ultimo.");
        }

        [Fact]
        public void UnAgujeroQueSobresaleDelTopeDeLaColumnaSeEXCLUYE()
        {
            // Ningun troquel regular sobresale del extremo fisico de la columna, y ninguno de los que caben
            // falta. Antes esto era una validacion de margen; ahora es la regla del radio, y por eso se
            // comprueba sobre los agujeros COLOCADOS y no sobre un numero que el diseno escribia.
            var design = Design();
            var assembly = Resolve(design);
            var radius = design.Connection.Punches.Diameter / 2.0;

            Assert.NotEmpty(assembly.ColumnRegularPunches);
            var top = design.Column.BottomPlate.Thickness + design.Column.Height;

            Assert.All(
                assembly.ColumnRegularPunches,
                p => Assert.True(p.Datum.V + radius <= top + Tolerance));
        }

        [Fact]
        public void UnPitchNoDIADICOSigueColocandoTodosLosEnterosQueCaben()
        {
            // 3.7 in no divide nada bonito. La regla no depende de que el paso sea redondo.
            var assembly = Resolve(Design(tune: p => p.RegularColumnPitch = 3.7));
            var radius = assembly.Pattern.Parameters.Diameter / 2.0;

            var elevations = assembly.ColumnRegularPunches
                .Select(p => Math.Round(p.Datum.V, 9)).Distinct().OrderBy(z => z).ToList();

            Assert.NotEmpty(elevations);
            Assert.All(elevations, z => Assert.True(z + radius <= 96.25 + Tolerance));
            Assert.True(elevations.Last() + 3.7 + radius > 96.25 + Tolerance,
                "Cabria otro troquel entero con este paso y no se coloco.");
        }

        [Fact]
        public void AnOffsetExactlyEqualToTheRadiusIsAccepted()
        {
            // Tangent to the edge is a product decision, not a defect: the floor is inclusive.
            var assembly = Resolve(Design(tune: p =>
            {
                p.HorizontalEndOffset = p.Diameter / 2.0;
                p.RearPlateVerticalEndOffset = p.Diameter / 2.0;
            }));

            Assert.DoesNotContain(
                assembly.Diagnostics, d => d.Code == CantileverDiagnostics.EdgeOffsetBelowRadius);
        }

        [Theory]
        [InlineData("connection")]
        [InlineData("regular")]
        [InlineData("bottom")]
        public void APitchSmallerThanTheDiameterIsRejected(string which)
        {
            var assembly = Resolve(Design(tune: p =>
            {
                switch (which)
                {
                    case "connection": p.ConnectionPitch = p.Diameter - 0.01; break;
                    case "regular": p.RegularColumnPitch = p.Diameter - 0.01; break;
                    default: p.ColumnBottomPlatePitch = p.Diameter - 0.01; break;
                }
            }));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.PitchBelowDiameter));
        }

        [Fact]
        public void APitchExactlyEqualToTheDiameterIsAccepted()
        {
            var assembly = Resolve(Design(tune: p => p.ConnectionPitch = p.Diameter));

            Assert.DoesNotContain(
                assembly.Diagnostics, d => d.Code == CantileverDiagnostics.PitchBelowDiameter);
        }

        [Fact]
        public void TwoRowsThatWouldMergeIntoASlotAreRejectedWithTheirOwnCode()
        {
            // W10X33 is 7.96 in wide, so an offset just under half of it drives the two rows together.
            var assembly = Resolve(Design(tune: p => p.HorizontalEndOffset = 3.9));

            Assert.True(assembly.IsBlocked);
            Assert.True(Has(assembly, CantileverDiagnostics.PunchRowsOverlap));

            // NOT the plate diagnostic: rows that merge are a COLUMN problem, and the old code sent the
            // reader to look at the base.
            Assert.False(Has(assembly, CantileverDiagnostics.PunchOutsideRearPlate));
        }

        [Fact]
        public void RowsFartherApartThanADiameterAreAccepted()
        {
            var assembly = Resolve();

            Assert.True(
                assembly.Pattern.RightRowX - assembly.Pattern.LeftRowX >=
                assembly.Pattern.Parameters.Diameter - Tolerance);
        }
    }
}
