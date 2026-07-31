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
    /// The Cantilever LINE (I-37D): intervals, the panel distribution, the separators and their column plates,
    /// the braces with their adapters, the common height, the three-axis arm matrix and the line BOM.
    ///
    /// Against the SHIPPED catalogue and through the real I-37A, I-37B and I-37C resolvers, for the reason the
    /// station tests give: the initiative's content is that it COMPOSES them, and a line tested against
    /// hand-made stations would prove the wrong thing.
    /// </summary>
    public class CantileverLineTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string ArmChannel = "AISC-C-C10X15_3";
        private const string SeparatorC = "AISC-C-C4X4_5";
        private const string BraceAngle = "AISC-L-L2X2X1_4";

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
            CantileverArmEndPlateMode endMode = CantileverArmEndPlateMode.None) =>
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
                EndPlate = new CantileverArmEndPlateDesign { Mode = endMode }
            };

        private static CantileverLineDesign Design(
            int stations = 3,
            double spacing = 96.0,
            CantileverStationFaceMode faceMode = CantileverStationFaceMode.Single,
            CantileverArmSide singleSide = CantileverArmSide.PositiveY,
            int levels = 3,
            double clear = 24.0,
            CantileverArmTemplateDesign defaultArm = null,
            CantileverBracingDesign bracing = null,
            CantileverStationColumnHeightMode heightMode = CantileverStationColumnHeightMode.Automatic,
            double? manualHeight = null)
        {
            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = faceMode,
                SingleSide = singleSide,
                LevelCount = levels,
                RequestedClearHeight = clear,
                ColumnHeight = new CantileverStationColumnHeightDesign
                {
                    Mode = heightMode,
                    ManualHeight = manualHeight
                },
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = ColumnW,
                    Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
                }
            };

            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;

            return new CantileverLineDesign
            {
                Name = "Linea de prueba",
                StationCount = stations,
                ColumnCentreSpacing = spacing,
                StationTopology = topology,
                DefaultArmTemplate = defaultArm ?? ArmTemplate(),
                Bracing = bracing ?? new CantileverBracingDesign()
            };
        }

        private static CantileverLineAssembly Resolve(CantileverLineDesign design = null) =>
            CantileverLineResolver.Resolve(
                design ?? Design(), Catalog, Factory, ColumnBasePolicy(), ArmPolicy());

        private static bool Has(CantileverLineAssembly line, string code) =>
            line.Diagnostics.Any(d => d.Code == code);

        private static string Why(CantileverLineAssembly line) =>
            string.Join(" | ", line.Diagnostics.Select(d => d.Code + ": " + d.Message));

        /// <summary>
        /// Where one end of a brace is BOLTED, whichever kind of brace it is.
        ///
        /// A structural brace carries its own end punches; a cold-rolled one carries none, because its adapters
        /// do. Both bolt to the same separator hole, and this is what lets one test check both.
        /// </summary>
        private static Point3D Anchor(CantileverBracePlan brace, int endIndex) =>
            brace.Kind == CantileverBraceBodyKind.ColdRolledRound
                ? brace.Adapters[endIndex].SeparatorFacePunch.Centre
                : brace.Punches[endIndex].Centre;

        // ---- 0. the fixture resolves at all ------------------------------------------------------------

        [Fact]
        public void TheFixtureResolves()
        {
            var line = Resolve();

            Assert.False(line.IsBlocked, "La linea del fixture debe resolver. " + Why(line));
            Assert.Equal(3, line.Stations.Count);
            Assert.Equal(2, line.Intervals.Count);
            Assert.True(line.ColumnHeight > 0.0);
        }

        // ---- 1. the panel distribution IS the product table --------------------------------------------

        [Theory]
        [InlineData(96.0, 1)]
        [InlineData(120.0, 1)]
        [InlineData(132.0, 1)]
        [InlineData(144.0, 2)]
        [InlineData(168.0, 2)]
        [InlineData(192.0, 2)]
        [InlineData(216.0, 3)]
        [InlineData(240.0, 3)]
        [InlineData(252.0, 3)]
        [InlineData(264.0, 4)]
        [InlineData(288.0, 4)]
        [InlineData(336.0, 5)]
        public void TheRuleReproducesEveryRowOfTheProductTable(double height, int expected)
        {
            // Twelve rows of the approved table, and the ONE formula that answers all of them. Encoded as data
            // rather than as twelve branches in the resolver, which is the whole difference between a rule and a
            // table (ADR-0027, D4).
            Assert.Equal(expected, CantileverBracingLayoutResolver.StandardBracedPanelCount(height));
        }

        [Fact]
        public void TheRuleKeepsAnsweringPastTheLastRowOfTheTable()
        {
            // The point of a rule. 336 in is the tallest row; a table of twelve ifs would stop here.
            Assert.Equal(5, CantileverBracingLayoutResolver.StandardBracedPanelCount(372.0));
            Assert.Equal(6, CantileverBracingLayoutResolver.StandardBracedPanelCount(396.0));
            Assert.Equal(1, CantileverBracingLayoutResolver.StandardBracedPanelCount(72.0));
            Assert.Equal(1, CantileverBracingLayoutResolver.StandardBracedPanelCount(12.0));
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        [InlineData(3, 1)]
        [InlineData(4, 1)]
        [InlineData(5, 2)]
        [InlineData(6, 2)]
        public void TheCentralSpacesAreOnePerCompletedBlockOfTwo(int panels, int spaces)
        {
            Assert.Equal(spaces, CantileverBracingLayoutResolver.CentralEmptySpaceCount(panels));
        }

        [Fact]
        public void ThePanelsGroupInBlocksOfTwoFromTheBottomAndTheIncompleteBlockIsOnTop()
        {
            // Five panels: two, gap, two, gap, one. Distributing the remainder evenly, or putting a gap at an
            // end, gives a drawing that is not the product (ADR-0027, D4).
            var layout = CantileverBracingLayoutResolver.Resolve(
                new CantileverBracingDesign(), 336.0, false);

            Assert.False(layout.IsBlocked);
            Assert.Equal(5, layout.BracedPanelCount);
            Assert.Equal(2, layout.CentralEmptySpaceCount);

            var kinds = layout.Slots.Select(s => s.Kind).ToList();

            Assert.Equal(
                new[]
                {
                    CantileverBracingSlotKind.ExternalSpace,
                    CantileverBracingSlotKind.BracedPanel,
                    CantileverBracingSlotKind.BracedPanel,
                    CantileverBracingSlotKind.CentralEmptySpace,
                    CantileverBracingSlotKind.BracedPanel,
                    CantileverBracingSlotKind.BracedPanel,
                    CantileverBracingSlotKind.CentralEmptySpace,
                    CantileverBracingSlotKind.BracedPanel,
                    CantileverBracingSlotKind.ExternalSpace
                },
                kinds);
        }

        [Fact]
        public void TheTwoExternalSpacesAreEqualAndAbsorbWhatTheCoreDoesNotUse()
        {
            var layout = CantileverBracingLayoutResolver.Resolve(
                new CantileverBracingDesign(), 264.0, false);

            Assert.False(layout.IsBlocked);
            Assert.Equal(4, layout.BracedPanelCount);
            Assert.Equal(1, layout.CentralEmptySpaceCount);
            Assert.Equal((4 * 40.0) + (1 * 40.0), layout.CoreHeight, Tolerance);
            Assert.Equal((264.0 - layout.CoreHeight) / 2.0, layout.ExternalSpaceHeight, Tolerance);

            var externals = layout.Slots
                .Where(s => s.Kind == CantileverBracingSlotKind.ExternalSpace)
                .ToList();

            Assert.Equal(2, externals.Count);
            Assert.Equal(externals[0].Height, externals[1].Height, Tolerance);

            // The slots tile the column exactly: no gap and no overlap.
            Assert.Equal(0.0, layout.Slots[0].BottomZ, Tolerance);
            Assert.Equal(264.0, layout.Slots[^1].TopZ, Tolerance);

            for (var i = 1; i < layout.Slots.Count; i++)
            {
                Assert.Equal(layout.Slots[i - 1].TopZ, layout.Slots[i].BottomZ, Tolerance);
            }
        }

        [Fact]
        public void TwoAdjacentPanelsShareTheirSeparatorSoItIsCountedOnce()
        {
            // SeparatorCount = P + G + 1, not 2P. A panel has two separators and the panel above it reuses the
            // upper one (ADR-0027, D6).
            foreach (var height in new[] { 96.0, 144.0, 216.0, 264.0, 336.0 })
            {
                var layout = CantileverBracingLayoutResolver.Resolve(
                    new CantileverBracingDesign(), height, false);

                Assert.False(layout.IsBlocked);
                Assert.Equal(
                    layout.BracedPanelCount + layout.CentralEmptySpaceCount + 1,
                    layout.SeparatorCount);
                Assert.Equal(2 * layout.BracedPanelCount, layout.BraceCount);

                // And the elevations are strictly increasing, which is what makes "the one above" meaningful.
                for (var i = 1; i < layout.SeparatorElevations.Count; i++)
                {
                    Assert.True(layout.SeparatorElevations[i] > layout.SeparatorElevations[i - 1]);
                }
            }
        }

        [Fact]
        public void EveryBracedPanelKnowsWhichSeparatorsAreItsOwn()
        {
            var layout = CantileverBracingLayoutResolver.Resolve(
                new CantileverBracingDesign(), 336.0, false);

            for (var p = 0; p < layout.BracedPanels.Count; p++)
            {
                var lower = layout.LowerSeparatorIndexOf(p);

                Assert.Equal(layout.SeparatorElevations[lower], layout.BracedPanels[p].BottomZ, Tolerance);
                Assert.Equal(layout.SeparatorElevations[lower + 1], layout.BracedPanels[p].TopZ, Tolerance);
            }
        }

        [Fact]
        public void AManualPanelCountIsHonouredAndAnImpossibleOneIsRefused()
        {
            var manual = new CantileverBracingDesign
            {
                PanelCountMode = CantileverBracedPanelCountMode.Manual,
                ManualPanelCount = 2
            };

            var fits = CantileverBracingLayoutResolver.Resolve(manual, 336.0, false);

            Assert.False(fits.IsBlocked);
            Assert.Equal(2, fits.BracedPanelCount);

            // Two panels need 80 in of core. A 40 in column cannot hold them, and the layout says so rather
            // than shrinking a panel to fit.
            var tooShort = CantileverBracingLayoutResolver.Resolve(manual, 40.0, false);

            Assert.True(tooShort.IsBlocked);
            Assert.Contains(
                tooShort.Diagnostics,
                d => d.Code == CantileverDiagnostics.BracingDoesNotFitTheColumn);
            Assert.Empty(tooShort.SeparatorElevations);
        }

        [Fact]
        public void AManualPanelCountThatIsMissingOrNotPositiveIsRefused()
        {
            var missing = CantileverBracingLayoutResolver.Resolve(
                new CantileverBracingDesign { PanelCountMode = CantileverBracedPanelCountMode.Manual },
                264.0, false);

            Assert.True(missing.IsBlocked);
            Assert.Contains(
                missing.Diagnostics,
                d => d.Code == CantileverDiagnostics.BracingManualPanelCountMissing);

            var zero = CantileverBracingLayoutResolver.Resolve(
                new CantileverBracingDesign
                {
                    PanelCountMode = CantileverBracedPanelCountMode.Manual,
                    ManualPanelCount = 0
                },
                264.0, false);

            Assert.True(zero.IsBlocked);
            Assert.Contains(
                zero.Diagnostics,
                d => d.Code == CantileverDiagnostics.BracingManualPanelCountNotPositive);
        }

        // ---- 2. the line's shape ------------------------------------------------------------------------

        [Fact]
        public void ALineNeedsTwoStationsAndAPositivePitch()
        {
            var one = Resolve(Design(stations: 1));

            Assert.True(one.IsBlocked);
            Assert.True(Has(one, CantileverDiagnostics.LineNeedsTwoStations));
            Assert.Empty(one.Stations);
            Assert.Empty(one.Intervals);

            var flat = Resolve(Design(spacing: 0.0));

            Assert.True(flat.IsBlocked);
            Assert.True(Has(flat, CantileverDiagnostics.LineSpacingNotPositive));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(6)]
        public void TheStationsSitOnTheGridAndTheIntervalsAreOneFewer(int stations)
        {
            var design = Design(stations: stations);
            var line = Resolve(design);

            Assert.False(line.IsBlocked, Why(line));
            Assert.Equal(stations, line.Stations.Count);
            Assert.Equal(stations - 1, line.Intervals.Count);

            for (var i = 0; i < stations; i++)
            {
                Assert.Equal(i * design.ColumnCentreSpacing, line.Stations[i].OriginX, Tolerance);
                Assert.Equal(i, line.Stations[i].Index);
            }

            for (var i = 0; i < line.Intervals.Count; i++)
            {
                Assert.Equal(i, line.Intervals[i].LeftStationIndex);
                Assert.Equal(i + 1, line.Intervals[i].RightStationIndex);
            }
        }

        [Fact]
        public void EverySeparatorOfTheLineBelongsToExactlyOneInterval()
        {
            // The one thing an interval-owned separator is FOR. If a separator belonged to a station, the
            // interior stations would each claim it and the line would count it twice (ADR-0027, D3).
            var line = Resolve(Design(stations: 4));

            Assert.False(line.IsBlocked, Why(line));

            var perInterval = line.Intervals[0].Separators.Count;

            Assert.Equal(3, line.Intervals.Count);
            Assert.Equal(perInterval * 3, line.Separators.Count);
            Assert.All(line.Intervals, i => Assert.Equal(perInterval, i.Separators.Count));

            var owners = line.Separators.Select(s => s.IntervalIndex).Distinct().OrderBy(i => i).ToList();

            Assert.Equal(new[] { 0, 1, 2 }, owners);
        }

        [Fact]
        public void AnInteriorStationReceivesTwoPlatesPerElevationOneForEachInterval()
        {
            var line = Resolve(Design(stations: 3));

            Assert.False(line.IsBlocked, Why(line));

            // Station 1 is interior: interval 0 puts plates on it at its Right end, interval 1 at its Left end.
            var onStationOne = line.SeparatorColumnPlates
                .Where(p => p.StationIndex == 1)
                .ToList();

            var perInterval = line.Intervals[0].Separators.Count;

            Assert.Equal(perInterval * 2, onStationOne.Count);
            Assert.Equal(
                new[] { CantileverIntervalSide.Left, CantileverIntervalSide.Right },
                onStationOne.Select(p => p.Side).Distinct().OrderBy(s => s).ToArray());

            // And the two stations at the ends get plates from ONE interval each.
            Assert.Equal(perInterval, line.SeparatorColumnPlates.Count(p => p.StationIndex == 0));
            Assert.Equal(perInterval, line.SeparatorColumnPlates.Count(p => p.StationIndex == 2));

            // Every plate key is distinct: this is what stops the same plate being produced twice.
            Assert.Equal(
                line.SeparatorColumnPlates.Count,
                line.SeparatorColumnPlates.Select(p => p.Key).Distinct().Count());
        }

        // ---- 3. the separator is cut FROM the plate datums ----------------------------------------------

        [Fact]
        public void TheSeparatorsEndPunchesAreItsPlatesHoles()
        {
            var line = Resolve();

            Assert.False(line.IsBlocked, Why(line));

            foreach (var interval in line.Intervals)
            {
                foreach (var separator in interval.Separators)
                {
                    var plates = interval.ColumnPlates
                        .Where(p => p.SeparatorIndex == separator.SeparatorIndex)
                        .ToList();

                    Assert.Equal(2, plates.Count);

                    var left = plates.Single(p => p.Side == CantileverIntervalSide.Left);
                    var right = plates.Single(p => p.Side == CantileverIntervalSide.Right);

                    // The DATUMS coincide, not the 3D centres: the two surfaces are separated by half a plate
                    // thickness, so comparing centres would fail for a joint that is perfectly correct.
                    Assert.True(
                        separator.LeftColumnPunch.Datum.ApproxEquals(left.Punch.Datum),
                        "El troquel izquierdo del separador " + separator.SeparatorIndex +
                        " no coincide con su placa.");

                    Assert.True(
                        separator.RightColumnPunch.Datum.ApproxEquals(right.Punch.Datum),
                        "El troquel derecho del separador " + separator.SeparatorIndex +
                        " no coincide con su placa.");
                }
            }

            // And the resolver said so itself: no mismatch was reported.
            Assert.False(Has(line, CantileverDiagnostics.BracingDatumMismatch));
        }

        [Fact]
        public void TheSeparatorsCutIsTheDistanceBetweenTheHolesPlusTwoEdgeDistances()
        {
            var line = Resolve();
            var interval = line.Intervals[0];
            var separator = interval.Separators[0];

            var left = interval.ColumnPlates
                .Single(p => p.SeparatorIndex == 0 && p.Side == CantileverIntervalSide.Left);
            var right = interval.ColumnPlates
                .Single(p => p.SeparatorIndex == 0 && p.Side == CantileverIntervalSide.Right);

            var holeToHole = right.Punch.Datum.U - left.Punch.Datum.U;
            var edge = CantileverLineDefaults.SeparatorColumnPunchEdgeDistance;

            Assert.Equal(holeToHole + (2.0 * edge), separator.CutLength, Tolerance);

            // NOT the centre-to-centre pitch: the plates are on the columns' inner faces, and subtracting a
            // hard-coded column width is the error ADR-0024 avoided by deriving every outer dimension from
            // Bounds (ADR-0027, D5).
            Assert.True(
                separator.CutLength < line.ColumnCentreSpacing,
                "El separador no puede medir la retícula completa.");
        }

        [Fact]
        public void TheSeparatorCarriesFourPunchesTwoForItsColumnsAndTwoForItsBraces()
        {
            var line = Resolve();
            var separator = line.Intervals[0].Separators[0];

            Assert.Equal(4, separator.Punches.Count);

            var edge = CantileverLineDefaults.SeparatorColumnPunchEdgeDistance;
            var offset = CantileverLineDefaults.SeparatorBracePunchOffset;
            var startX = separator.LeftColumnPunch.Datum.U - edge;
            var length = separator.CutLength;

            // 1.25 / 5.25 / L − 5.25 / L − 1.25, measured from the separator's own left cut.
            Assert.Equal(startX + edge, separator.LeftColumnPunch.Datum.U, Tolerance);
            Assert.Equal(startX + edge + offset, separator.LeftBracePunch.Datum.U, Tolerance);
            Assert.Equal(startX + length - edge - offset, separator.RightBracePunch.Datum.U, Tolerance);
            Assert.Equal(startX + length - edge, separator.RightColumnPunch.Datum.U, Tolerance);

            // All four at the separator's own elevation, all four the same diameter, all four transverse.
            Assert.All(separator.Punches, p =>
            {
                Assert.Equal(separator.ElevationZ, p.Datum.V, Tolerance);
                Assert.Equal(CantileverLineDefaults.SeparatorPunchDiameter, p.Datum.Diameter, Tolerance);
                Assert.Equal(CantileverPunchAxis.AlongY, p.Datum.Axis);
            });

            // Left to right, in order, with no two in the same place.
            var xs = separator.Punches.Select(p => p.Datum.U).ToList();

            for (var i = 1; i < xs.Count; i++)
            {
                Assert.True(xs[i] > xs[i - 1], "Los cuatro troqueles van de izquierda a derecha.");
            }
        }

        [Fact]
        public void ASeparatorThatCannotHoldItsFourPunchesIsRefused()
        {
            // 1.25 + 4 + 4 + 1.25 = 10.5 in de troqueles.
            //
            // La separacion de prueba BAJO de 12 a 10 in en la ronda 3, y por la correccion del dueno: el
            // arriostramiento pasa a atarse al ALMA, asi que el claro ya no se mide de cara a cara de patin
            // -12 - 7.96 = 4.04 in, que no llegaba- sino de alma a alma -12 - 0.29 = 11.71 in, que sobra-.
            // Con 10 in el claro es 9.71 y el separador vuelve a no caber, que es lo que esta prueba mira.
            var line = Resolve(Design(spacing: 10.0));

            Assert.True(Has(line, CantileverDiagnostics.SeparatorTooShortForItsPunches),
                "Se esperaba el diagnostico de separador corto. " + Why(line));
            Assert.All(line.Intervals, i => Assert.Empty(i.Separators));
        }

        [Fact]
        public void TheSeparatorsBodyTouchesItsPlateAndNeverPassesThroughTheColumn()
        {
            var line = Resolve();
            var interval = line.Intervals[0];
            var separator = interval.Separators[0];

            // The separator runs along +X, starting at the left column's face.
            Assert.Equal(1.0, separator.Member.Direction.X, Tolerance);
            Assert.Equal(0.0, separator.Member.Direction.Y, Tolerance);
            Assert.Equal(0.0, separator.Member.Direction.Z, Tolerance);

            var geometry = Factory.Get(separator.Member.Placement.SectionId, SectionDetailLevel.Tabulated);
            var box = CantileverPrismExtent.CrossSection(separator.Member.Placement, geometry);

            var plate = interval.ColumnPlates.Single(
                p => p.SeparatorIndex == 0 && p.Side == CantileverIntervalSide.Left);

            var farFace = plate.Plate.FarOffset * plate.Plate.Normal.Y;

            // The back of the web is seated ON the plate's far face — touching, with no gap and no overlap.
            var seated = Math.Min(Math.Abs(box.MinY - farFace), Math.Abs(box.MaxY - farFace));

            Assert.True(
                seated <= 1e-6,
                "El separador debe apoyarse en la cara exterior de su placa; queda a " + seated + " pulgadas.");
        }

        // ---- 4. the braces -----------------------------------------------------------------------------

        [Fact]
        public void EachPanelCarriesTwoCrossedBracesInOnePlaneWithNoCentralJoint()
        {
            var line = Resolve();

            Assert.NotEmpty(line.BracedPanels);

            foreach (var panel in line.BracedPanels)
            {
                Assert.Equal('A', panel.BraceA.Diagonal);
                Assert.Equal('B', panel.BraceB.Diagonal);

                // A rises left to right, B falls: they cross.
                Assert.True(panel.BraceA.UpperEnd.X > panel.BraceA.LowerEnd.X);
                Assert.True(panel.BraceB.UpperEnd.X < panel.BraceB.LowerEnd.X);
                Assert.True(panel.BraceA.UpperEnd.Z > panel.BraceA.LowerEnd.Z);
                Assert.True(panel.BraceB.UpperEnd.Z > panel.BraceB.LowerEnd.Z);

                // Coplanar: one Y for all four ends. The MVP declares that they overlap visually and does not
                // compute their interference (ADR-0027, D6).
                foreach (var y in new[]
                         {
                             panel.BraceA.LowerEnd.Y, panel.BraceA.UpperEnd.Y,
                             panel.BraceB.LowerEnd.Y, panel.BraceB.UpperEnd.Y
                         })
                {
                    Assert.Equal(panel.PlaneY, y, Tolerance);
                }

                // Every diagonal hangs off the separators' brace punches and off nothing else. The BOLT is the
                // anchor, and it is checked at the bolt rather than at the brace's end, because a cold-rolled
                // brace's end is its adapter's rod hole and sits one half-adapter further in on purpose.
                Assert.Equal(
                    panel.LowerSeparator.LeftBracePunch.Datum.U,
                    Anchor(panel.BraceA, 0).X, 1e-9);
                Assert.Equal(
                    panel.UpperSeparator.RightBracePunch.Datum.U,
                    Anchor(panel.BraceA, 1).X, 1e-9);
                Assert.Equal(
                    panel.LowerSeparator.RightBracePunch.Datum.U,
                    Anchor(panel.BraceB, 0).X, 1e-9);
                Assert.Equal(
                    panel.UpperSeparator.LeftBracePunch.Datum.U,
                    Anchor(panel.BraceB, 1).X, 1e-9);
            }
        }

        [Fact]
        public void ColdRolledIsTheDefaultAndBringsTwoAdaptersAndFourGussetsPerBrace()
        {
            var line = Resolve();

            Assert.NotEmpty(line.Braces);
            Assert.All(line.Braces, b =>
            {
                Assert.Equal(CantileverBraceBodyKind.ColdRolledRound, b.Kind);
                Assert.Null(b.Member);
                Assert.Empty(b.Punches);
                Assert.Equal(CantileverLineDefaults.ColdRolledBraceDiameter, b.RoundDiameter, Tolerance);
                Assert.Equal(CantileverLineDefaults.AdaptersPerColdRolledBrace, b.Adapters.Count);

                Assert.Equal(
                    CantileverLineDefaults.AdaptersPerColdRolledBrace *
                        CantileverLineDefaults.GussetsPerAdapter,
                    b.Adapters.Sum(a => a.GussetCount));
            });
        }

        [Fact]
        public void AnAdapterIsAnAngleWhoseGussetsAreDescribedByGaugeAndNotByThickness()
        {
            var line = Resolve();
            var adapter = line.Adapters[0];

            Assert.Equal(CantileverLineDefaults.AdapterAngleLeg, adapter.Leg, Tolerance);
            Assert.Equal(CantileverLineDefaults.AdapterCutLength, adapter.CutLength, Tolerance);
            Assert.Equal(CantileverLineDefaults.AdapterAngleThickness, adapter.Thickness, Tolerance);
            Assert.Equal(10, adapter.GussetGaugeNumber);
            Assert.Equal("Cartabon de adaptador CAL_10", adapter.GussetDescription);

            // The gauge stays a NUMBER and a name. The repository has no gauge table, so a decimal thickness
            // here would be a figure no source backs.
            Assert.DoesNotContain("0.1", adapter.GussetDescription, StringComparison.Ordinal);
        }

        [Fact]
        public void TheRodSpansBetweenTheTwoAdapterHolesAndNotBetweenTheBolts()
        {
            var line = Resolve();
            var panel = line.BracedPanels[0];
            var brace = panel.BraceA;

            var boltToBolt =
                (panel.UpperSeparator.RightBracePunch.Centre - panel.LowerSeparator.LeftBracePunch.Centre)
                .Length;

            var inset = CantileverIntervalResolver.RodHoleAxialOffset(
                CantileverLineDefaults.AdapterCutLength);

            Assert.Equal(boltToBolt - (2.0 * inset), brace.BodyLength, 1e-9);
            Assert.True(brace.BodyLength < boltToBolt, "La varilla es mas corta que el eje entre tornillos.");

            // Each adapter's separator hole IS the separator's brace punch, and its rod hole is the rod's end.
            Assert.Equal(brace.LowerEnd.X, brace.Adapters[0].RodHoleCentre.X, Tolerance);
            Assert.Equal(brace.UpperEnd.X, brace.Adapters[1].RodHoleCentre.X, Tolerance);

            Assert.True(
                brace.Adapters[0].SeparatorFacePunch.Datum.ApproxEquals(
                    panel.LowerSeparator.LeftBracePunch.Datum),
                "El agujero del adaptador debe coincidir con el troquel de tensor del separador.");
        }

        [Fact]
        public void AStructuralBraceIsAProfileCutPastItsBoltsAndCarriesItsOwnPunches()
        {
            var line = Resolve(Design(bracing: new CantileverBracingDesign
            {
                BraceKind = CantileverBraceBodyKind.StructuralSection,
                BraceSectionId = BraceAngle
            }));

            Assert.False(line.IsBlocked, Why(line));
            Assert.NotEmpty(line.Braces);

            var panel = line.BracedPanels[0];
            var brace = panel.BraceA;

            Assert.Equal(CantileverBraceBodyKind.StructuralSection, brace.Kind);
            Assert.NotNull(brace.Member);
            Assert.Empty(brace.Adapters);
            Assert.Equal(2, brace.Punches.Count);
            Assert.Equal(BraceAngle, brace.Member.SectionId.Value);

            var boltToBolt =
                (panel.UpperSeparator.RightBracePunch.Centre - panel.LowerSeparator.LeftBracePunch.Centre)
                .Length;

            var edge = CantileverLineDefaults.BracePunchEdgeDistance;

            Assert.Equal(boltToBolt + (2.0 * edge), brace.Member.NominalCutLength, 1e-9);
            Assert.True(
                brace.BodyLength > boltToBolt,
                "Un tensor de perfil se corta MAS LARGO que el eje entre tornillos, para tener distancia al borde.");
        }

        [Fact]
        public void AStructuralBraceWithoutASectionIsRefusedAndSaysSoInItsOwnWords()
        {
            var line = Resolve(Design(bracing: new CantileverBracingDesign
            {
                BraceKind = CantileverBraceBodyKind.StructuralSection
            }));

            Assert.True(line.IsBlocked);
            Assert.True(
                Has(line, CantileverDiagnostics.BraceSectionMissing),
                "Se esperaba BraceSectionMissing y no el diagnostico genérico de seccion. " + Why(line));
            Assert.Empty(line.Braces);
        }

        [Fact]
        public void AnUnknownBraceSectionIsReportedAsABraceAndNotAsABase()
        {
            // The regression for the Noun defect: an unknown brace section used to be reported as "la base".
            var line = Resolve(Design(bracing: new CantileverBracingDesign
            {
                BraceKind = CantileverBraceBodyKind.StructuralSection,
                BraceSectionId = "AISC-W-NO-EXISTE"
            }));

            Assert.True(line.IsBlocked);

            var message = line.Diagnostics
                .First(d => d.Code == CantileverDiagnostics.SectionUnknown)
                .Message;

            Assert.Contains("el tensor", message, StringComparison.Ordinal);
            Assert.DoesNotContain("la base", message, StringComparison.Ordinal);
        }

        [Fact]
        public void ANonPositiveColdRolledDiameterIsRefused()
        {
            var line = Resolve(Design(bracing: new CantileverBracingDesign
            {
                BraceKind = CantileverBraceBodyKind.ColdRolledRound,
                ColdRolled = new CantileverColdRolledBraceDesign { Diameter = 0.0 }
            }));

            Assert.True(line.IsBlocked);
            Assert.True(Has(line, CantileverDiagnostics.ColdRolledDiameterNotPositive), Why(line));
        }

        // ---- 5. the common height ----------------------------------------------------------------------

        [Fact]
        public void EveryColumnOfALineHasExactlyTheSameHeight()
        {
            var line = Resolve(Design(stations: 4));

            Assert.False(line.IsBlocked, Why(line));
            Assert.All(line.Stations, s =>
                Assert.Equal(line.ColumnHeight, s.Station.ResolvedColumnHeight, Tolerance));
        }

        [Fact]
        public void TheTallestStationSetsTheHeightForAllOfThem()
        {
            // Station 2 gets an arm whose end plate is a STOP that reaches above the body, so it needs more
            // column than the others. The line must grow to it, and everything else must grow with it.
            var design = Design(stations: 3);

            design.ArmCellOverrides.Add(new CantileverArmCellOverride
            {
                StationIndex = 2,
                LevelIndex = 2,
                Side = CantileverArmSide.PositiveY,
                Arm = ArmTemplate(cutLength: 60.0)
            });

            var line = Resolve(design);

            Assert.False(line.IsBlocked, Why(line));

            var alone = new CantileverStationResolver(Catalog, Factory, ColumnBasePolicy(), ArmPolicy())
                .Resolve(design.ToStationDesign(0));

            Assert.True(
                line.ColumnHeight >= alone.ResolvedColumnHeight,
                "La altura comun nunca puede quedar por debajo de la que una estacion resolvia sola.");

            Assert.All(line.Stations, s =>
                Assert.Equal(line.ColumnHeight, s.Station.ResolvedColumnHeight, Tolerance));

            Assert.Equal(line.LargestStationMinimumHeight,
                line.Stations.Max(s => s.Station.MinimumColumnHeight), Tolerance);
        }

        [Fact]
        public void TheHeightIsVerifiedAndNotAssumed()
        {
            // The line ASKS for the common height and then checks it was granted. The check is the reason a
            // station whose snap moved it cannot leave the line with columns of different heights.
            var line = Resolve(Design(stations: 3));

            Assert.False(line.IsBlocked, Why(line));
            Assert.False(Has(line, CantileverDiagnostics.LineCommonHeightMovedALevel));

            var heights = line.Stations.Select(s => s.Station.ResolvedColumnHeight).Distinct().ToList();

            Assert.Single(heights);
        }

        [Fact]
        public void AManualHeightIsSharedByEveryStationToo()
        {
            var line = Resolve(Design(
                stations: 3,
                heightMode: CantileverStationColumnHeightMode.Manual,
                manualHeight: 264.0));

            Assert.False(line.IsBlocked, Why(line));
            Assert.Equal(264.0, line.ColumnHeight, Tolerance);
            Assert.All(line.Stations, s => Assert.Equal(264.0, s.Station.ResolvedColumnHeight, Tolerance));

            // And the bracing was laid out on THAT height, not on an automatic one.
            Assert.Equal(
                CantileverBracingLayoutResolver.StandardBracedPanelCount(264.0),
                line.Intervals[0].Layout.BracedPanelCount);
        }

        [Fact]
        public void ABlockedStationBlocksTheLineAndTheLineCarriesNothing()
        {
            var design = Design();
            design.StationTopology.ColumnBaseTemplate.ColumnSectionId = "AISC-W-NO-EXISTE";

            var line = Resolve(design);

            Assert.True(line.IsBlocked);
            Assert.True(Has(line, CantileverDiagnostics.LineStationBlocked));
            Assert.Empty(line.Stations);
            Assert.Empty(line.Intervals);
            Assert.Null(line.Envelope());
        }

        // ---- 6. identity -------------------------------------------------------------------------------

        [Fact]
        public void ALineHasONEIdentityAndEveryStationSharesIt()
        {
            var design = Design();

            Assert.NotEqual(Guid.Empty, design.Id);

            var line = Resolve(design);

            Assert.Equal(design.Id, line.Id);
            Assert.Equal(design.Name, line.Name);

            // Duplicating mints ONE new common GUID, not one per station.
            var copy = design.DuplicateWithNewIdentity();

            Assert.NotEqual(design.Id, copy.Id);
            Assert.Equal(design.StationCount, copy.StationCount);
        }

        [Fact]
        public void NoTwoPiecesOfALineShareAnId()
        {
            // The station authority numbers its pieces without knowing it is inside a line, so two stations
            // carry the same ids. Scoping is what makes the line's pieces addressable.
            var line = Resolve(Design(stations: 3));

            Assert.False(line.IsBlocked, Why(line));

            var ids = line.AllPieceIds;
            var duplicates = ids
                .GroupBy(i => i.Value, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            Assert.True(
                duplicates.Count == 0,
                "Ninguna pieza de la linea puede repetir id. Repetidas: " +
                string.Join(", ", duplicates.Take(5)) + ".");

            Assert.True(ids.Count > 3 * 10, "Se esperaban muchas piezas; hay " + ids.Count + ".");
        }

        [Fact]
        public void TheSignatureIgnoresTheNameAndTheGuid()
        {
            var a = Design();
            var b = Design();

            b.Name = "Otro nombre";

            Assert.NotEqual(a.Id, b.Id);
            Assert.Equal(Resolve(a).Signature(), Resolve(b).Signature());
        }

        [Fact]
        public void MovingAStationChangesTheSignature()
        {
            var a = Resolve(Design(spacing: 96.0));
            var b = Resolve(Design(spacing: 108.0));

            Assert.NotEqual(a.Signature(), b.Signature());
        }

        // ---- 7. the three-axis matrix ------------------------------------------------------------------

        [Fact]
        public void TheMatrixHasOneCellPerStationLevelAndActiveSide()
        {
            var single = new CantileverLineArmMatrix(Design(stations: 3, levels: 4));

            Assert.Single(single.ActiveSides);
            Assert.Equal(3 * 4 * 1, single.Cells.Count);

            var doubleSided = new CantileverLineArmMatrix(
                Design(stations: 3, levels: 4, faceMode: CantileverStationFaceMode.Double));

            Assert.Equal(2, doubleSided.ActiveSides.Count);
            Assert.Equal(3 * 4 * 2, doubleSided.Cells.Count);

            // Deterministic order: station, then level, then side.
            var cells = doubleSided.Cells;

            Assert.Equal(0, cells[0].StationIndex);
            Assert.Equal(0, cells[0].LevelIndex);
            Assert.True(cells.Zip(cells.Skip(1)).All(p =>
                p.First.StationIndex < p.Second.StationIndex ||
                (p.First.StationIndex == p.Second.StationIndex &&
                    (p.First.LevelIndex < p.Second.LevelIndex ||
                        (p.First.LevelIndex == p.Second.LevelIndex && p.First.Side != p.Second.Side)))));
        }

        [Theory]
        [InlineData(CantileverLineApplyScope.Cell, 1)]
        [InlineData(CantileverLineApplyScope.Station, 6)]
        [InlineData(CantileverLineApplyScope.Level, 6)]
        [InlineData(CantileverLineApplyScope.Side, 9)]
        [InlineData(CantileverLineApplyScope.Line, 18)]
        public void EachScopeReachesExactlyTheCellsItNames(CantileverLineApplyScope scope, int expected)
        {
            // Three stations, three levels, two sides: 18 cells. A station is 6, a level across the line is 6,
            // a side is 9.
            var matrix = new CantileverLineArmMatrix(
                Design(stations: 3, levels: 3, faceMode: CantileverStationFaceMode.Double));

            var anchor = new CantileverLineCell(1, 1, CantileverArmSide.PositiveY);

            Assert.Equal(expected, matrix.InScope(scope, anchor).Count);
        }

        [Fact]
        public void AnOverrideEqualToTheDefaultIsNotStored()
        {
            // A cell holding a copy of the default has stopped following it: the next change to the default
            // would leave that cell behind, invisibly (ADR-0026, D3).
            var design = Design();
            var matrix = new CantileverLineArmMatrix(design);
            var anchor = new CantileverLineCell(0, 0, CantileverArmSide.PositiveY);

            var change = matrix.Apply(
                CantileverLineApplyScope.Cell, anchor, design.DefaultArmTemplate.DeepCopy());

            Assert.True(change.IsNoOp);
            Assert.Empty(design.ArmCellOverrides);
            Assert.False(matrix.HasOverride(anchor));
        }

        [Fact]
        public void ARealOverrideIsStoredOncePerCellAsItsOwnCopy()
        {
            var design = Design(stations: 3);
            var matrix = new CantileverLineArmMatrix(design);
            var anchor = new CantileverLineCell(1, 1, CantileverArmSide.PositiveY);

            var change = matrix.Apply(
                CantileverLineApplyScope.Station, anchor, ArmTemplate(cutLength: 48.0));

            Assert.False(change.IsNoOp);
            Assert.Equal(3, change.Count);
            Assert.Equal(3, design.ArmCellOverrides.Count);
            Assert.Equal(3, matrix.OverrideCount);

            // Only station 1 moved.
            Assert.All(design.ArmCellOverrides, o => Assert.Equal(1, o.StationIndex));

            // Each cell has its OWN object: editing one must not change the others.
            var arms = design.ArmCellOverrides.Select(o => o.Arm).ToList();

            Assert.Equal(3, arms.Distinct().Count());

            arms[0].Body.CutLength = 72.0;

            Assert.Equal(48.0, arms[1].Body.CutLength, Tolerance);
        }

        [Fact]
        public void ApplyingTheSameOverrideTwiceIsANoOpTheSecondTime()
        {
            // What the editor asks before regenerating: ONE notification and ONE regeneration per operation,
            // and none at all when nothing moved.
            var design = Design();
            var matrix = new CantileverLineArmMatrix(design);
            var anchor = new CantileverLineCell(0, 0, CantileverArmSide.PositiveY);
            var arm = ArmTemplate(cutLength: 48.0);

            Assert.False(matrix.Apply(CantileverLineApplyScope.Cell, anchor, arm).IsNoOp);

            var again = matrix.Apply(CantileverLineApplyScope.Cell, anchor, arm.DeepCopy());

            Assert.True(again.IsNoOp);
            Assert.Single(design.ArmCellOverrides);
        }

        [Fact]
        public void RestoringClearsTheOverrideInsteadOfCopyingTheDefaultIn()
        {
            var design = Design();
            var matrix = new CantileverLineArmMatrix(design);
            var anchor = new CantileverLineCell(0, 0, CantileverArmSide.PositiveY);

            matrix.Apply(CantileverLineApplyScope.Cell, anchor, ArmTemplate(cutLength: 48.0));

            var change = matrix.Restore(CantileverLineApplyScope.Line, anchor);

            Assert.False(change.IsNoOp);
            Assert.Empty(design.ArmCellOverrides);
            Assert.False(matrix.HasOverride(anchor));

            // Restoring twice changes nothing.
            Assert.True(matrix.Restore(CantileverLineApplyScope.Line, anchor).IsNoOp);
        }

        [Fact]
        public void AnOverrideOnOneCellReachesThatCellOnlyWhenTheLineIsResolved()
        {
            var design = Design(stations: 3, levels: 2);

            design.ArmCellOverrides.Add(new CantileverArmCellOverride
            {
                StationIndex = 2,
                LevelIndex = 0,
                Side = CantileverArmSide.PositiveY,
                Arm = ArmTemplate(cutLength: 60.0)
            });

            var line = Resolve(design);

            Assert.False(line.IsBlocked, Why(line));

            var arms = line.Arms;
            var overridden = arms
                .Where(a => a.StationIndex == 2)
                .Select(a => a.Arm.Body.CutLength)
                .ToList();

            Assert.Contains(60.0, overridden);
            Assert.All(
                arms.Where(a => a.StationIndex != 2),
                a => Assert.Equal(36.0, a.Arm.Body.CutLength, Tolerance));
        }

        // ---- 8. the line BOM ---------------------------------------------------------------------------

        [Fact]
        public void IdenticalStationsAreONEComponentWithAQuantity()
        {
            var line = Resolve(Design(stations: 4));
            var bom = CantileverLineBomBuilder.Build(line);

            Assert.True(bom.IsComponentBased);

            var columnBase = bom.Components
                .Single(c => c.Category == CantileverStationBomBuilder.ColumnBaseCategory);

            Assert.Equal(4, columnBase.Quantity);

            // The arms too: four stations of three identical arms are twelve of one component.
            var arms = bom.Components.Where(c => c.Category == CantileverStationBomBuilder.ArmCategory).ToList();

            Assert.Single(arms);
            Assert.Equal(12, arms[0].Quantity);
        }

        [Fact]
        public void AStationWithADifferentArmGetsItsOwnComponentLine()
        {
            var design = Design(stations: 3, levels: 2);

            design.ArmCellOverrides.Add(new CantileverArmCellOverride
            {
                StationIndex = 1,
                LevelIndex = 0,
                Side = CantileverArmSide.PositiveY,
                Arm = ArmTemplate(cutLength: 60.0)
            });

            var bom = CantileverLineBomBuilder.Build(Resolve(design));

            var arms = bom.Components
                .Where(c => c.Category == CantileverStationBomBuilder.ArmCategory)
                .ToList();

            // Three stations, two levels, one side: six arms. Five follow the default and one does not.
            Assert.Equal(2, arms.Count);
            Assert.Equal(6, arms.Sum(c => c.Quantity));
            Assert.Equal(1, arms.Single(c => Math.Abs(c.Length - 60.0) < 1e-6).Quantity);
            Assert.Equal(5, arms.Single(c => Math.Abs(c.Length - 36.0) < 1e-6).Quantity);
            Assert.Contains(arms, c => Math.Abs(c.Length - 60.0) < 1e-6);
            Assert.Contains(arms, c => Math.Abs(c.Length - 36.0) < 1e-6);
        }

        [Fact]
        public void TheSeparatorsAreComponentsAndTheirPlatesArePiecesOfThem()
        {
            var line = Resolve(Design(stations: 3));
            var bom = CantileverLineBomBuilder.Build(line);

            var separators = bom.Components
                .Where(c => c.Category == CantileverLineBomBuilder.SeparatorCategory)
                .ToList();

            Assert.NotEmpty(separators);

            // Every separator of the line has the same cut and the same plates, so they merge into one line.
            Assert.Single(separators);
            Assert.Equal(line.Separators.Count, separators[0].Quantity);
            Assert.Equal(SeparatorC, separators[0].ProfileId);

            var pieces = separators[0].Pieces;

            Assert.Contains(pieces, p => p.Category == CantileverLineBomBuilder.SeparatorProfileCategory);

            var plate = pieces.Single(p => p.Category == CantileverStationBomBuilder.PlateCategory);

            Assert.Equal(2, plate.Quantity);
            Assert.Equal(0.0, plate.Length, Tolerance);
            Assert.Contains("t0.375", plate.ProfileId, StringComparison.Ordinal);
        }

        [Fact]
        public void TheBracesAreComponentsWithTheirRodAdaptersAndGussets()
        {
            var line = Resolve(Design(stations: 3));
            var bom = CantileverLineBomBuilder.Build(line);

            var braces = bom.Components
                .Where(c => c.Category == CantileverLineBomBuilder.BraceCategory)
                .ToList();

            Assert.NotEmpty(braces);
            Assert.Equal(line.Braces.Count, braces.Sum(c => c.Quantity));

            var one = braces[0];

            Assert.Contains(one.Pieces, p => p.Category == CantileverLineBomBuilder.ColdRolledRodCategory);

            var adapters = one.Pieces.Single(p => p.Category == CantileverLineBomBuilder.AdapterCategory);
            var gussets = one.Pieces.Single(p => p.Category == CantileverLineBomBuilder.GussetCategory);

            Assert.Equal(2, adapters.Quantity);
            Assert.Equal(4, gussets.Quantity);
            Assert.Equal("CAL_10", gussets.ProfileId);
            Assert.Equal(0.0, gussets.Length, Tolerance);
        }

        [Fact]
        public void TheBomOfABlockedLineIsEmptyAndNotPartial()
        {
            var line = Resolve(Design(stations: 1));

            Assert.True(line.IsBlocked);
            Assert.Empty(CantileverLineBomBuilder.Build(line).Components);
        }

        [Fact]
        public void TheLineBomAsksTheStationBuilderAndDoesNotDisturbIt()
        {
            // Accumulating into the station builder's own components would mean asking a station for its BOM
            // after asking the line for one reported the LINE's quantities.
            var line = Resolve(Design(stations: 4));

            CantileverLineBomBuilder.Build(line);

            var station = CantileverStationBomBuilder.Build(line.Stations[0].Station);

            Assert.All(station.Components, c => Assert.True(
                c.Quantity <= 3,
                "El BOM de una estacion no puede heredar las cantidades de la linea; leyo " + c.Quantity + "."));
        }

        [Fact]
        public void EveryComponentOfALineIsCountedExactlyOnce()
        {
            var line = Resolve(Design(stations: 4));
            var bom = CantileverLineBomBuilder.Build(line);

            var separators = bom.Components
                .Where(c => c.Category == CantileverLineBomBuilder.SeparatorCategory)
                .Sum(c => c.Quantity);

            var braces = bom.Components
                .Where(c => c.Category == CantileverLineBomBuilder.BraceCategory)
                .Sum(c => c.Quantity);

            // Three intervals. Walking neighbouring intervals must not count a shared separator twice.
            Assert.Equal(3 * line.Intervals[0].Separators.Count, separators);
            Assert.Equal(3 * line.Intervals[0].Braces.Count, braces);

            var columns = bom.Components
                .Where(c => c.Category == CantileverStationBomBuilder.ColumnBaseCategory)
                .Sum(c => c.Quantity);

            Assert.Equal(4, columns);
        }

        // ---- 9. the envelope ---------------------------------------------------------------------------

        [Fact]
        public void TheEnvelopeCoversEveryStationAndTheBracingBetweenThem()
        {
            var design = Design(stations: 3);
            var line = Resolve(design);

            Assert.False(line.IsBlocked, Why(line));

            var box = line.Envelope();

            Assert.NotNull(box);

            // It reaches from the first column to the last, which are two pitches apart.
            Assert.True(
                box.Value.Width >= 2 * design.ColumnCentreSpacing,
                "La envolvente debe abarcar las tres estaciones.");

            // Correccion del datum (I-37D): la placa inferior se APOYA en el piso en vez de colgar de el, asi
            // que nada de la linea baja de z = 0. Y el tope sube: la columna arranca sobre la placa, de modo
            // que su extremo esta un espesor por encima de su longitud nominal.
            var plate = line.Stations[0].Station.ColumnBase.ColumnBottomPlate;

            Assert.Equal(0.0, box.Value.MinZ, 1e-6);
            Assert.True(box.Value.MaxZ >= plate.Thickness + line.ColumnHeight - 1e-6);
        }
    }
}
