using System;
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
    /// I-37D gate 4 — the editor's recompute authority.
    ///
    /// What is at stake here is not geometry (I-37A to I-37C own that, and gates 1 to 3 own the line): it is that
    /// ONE object answers "resolve, quote and project this design", so the editor and the Plugin cannot orchestrate
    /// those three steps differently. These tests also pin the two properties an editor depends on and a resolver
    /// does not: a failed pass yields nothing to draw, and a computation never aliases the caller's design.
    /// </summary>
    public class CantileverLineEditorAssemblerTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverLineEditorAssembler Assembler() => new CantileverLineEditorAssembler(Catalog);

        /// <summary>A line that resolves: the same shape the line tests use, expressed through the design alone.</summary>
        private static CantileverLineDesign ValidDesign(int stations = 3, int levels = 3)
        {
            var design = new CantileverLineDesign
            {
                StationCount = stations,
                ColumnCentreSpacing = 96.0,
                StationTopology = new CantileverLineStationTopologyDesign
                {
                    LevelCount = levels,
                    RequestedClearHeight = 24.0,
                    ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                    {
                        ColumnSectionId = ColumnW,
                        Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
                    }
                },
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                }
            };

            var punches = design.StationTopology.ColumnBaseTemplate.Connection.Punches;
            punches.ColumnBottomPlateEndOffset = 1.5;
            punches.ColumnTopPunchOffset = 4.0;

            return design;
        }

        [Fact]
        public void AValidDesignResolvesAndCarriesItsBomAndItsThreeViews()
        {
            var computation = Assembler().Build(ValidDesign());

            Assert.True(computation.IsValid);
            Assert.Null(computation.Error);
            Assert.NotNull(computation.Bom);
            Assert.Equal(3, computation.Views.Count);

            Assert.Equal(
                new[] { CantileverViewKind.Frontal, CantileverViewKind.Planta, CantileverViewKind.Lateral },
                computation.Views.Select(v => v.View));
        }

        [Fact]
        public void TheWholeLineViewsCarrySectionMinusOneAndOnlyTheLateralCarriesItsStation()
        {
            var computation = Assembler().Build(ValidDesign(), lateralStationIndex: 2);

            // The number stamped on the drawing envelope: a frontal of "station 3" is not a thing.
            Assert.Equal(-1, computation.Views.Single(v => v.View == CantileverViewKind.Frontal).StationIndex);
            Assert.Equal(-1, computation.Views.Single(v => v.View == CantileverViewKind.Planta).StationIndex);
            Assert.Equal(2, computation.Views.Single(v => v.View == CantileverViewKind.Lateral).StationIndex);
        }

        [Fact]
        public void ADesignWithNoSectionsIsBlockedAndHasNothingToDrawAndNothingToQuote()
        {
            // The state a BRAND-NEW line opens in: no section id is invented, so it must come back blocked with a
            // reason — and, above all, with no BOM. Quoting a line that cannot be built is worse than quoting
            // nothing, because the numbers look usable.
            var computation = Assembler().Build(new CantileverLineDesign());

            Assert.False(computation.IsValid);
            Assert.NotNull(computation.Error);
            Assert.Null(computation.Bom);
            Assert.Empty(computation.Views);
        }

        [Fact]
        public void UnaLineaSinLosMargenesLEGACYResuelvePERFECTAMENTE()
        {
            // I-37D ronda 2, motivo 1: los dos margenes dejaron de ser entradas. Una linea que no los trae
            // —que es ahora TODA linea nueva— resuelve igual, porque lo que limita un agujero es su radio.
            var design = ValidDesign();
            design.StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = null;
            design.StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = null;

            var computation = Assembler().Build(design);

            Assert.True(computation.IsValid);
            Assert.NotNull(computation.Bom);
        }

        [Fact]
        public void ALineOfOneStationIsBlocked_ThereIsNoIntervalAndSoNoBracing()
        {
            var computation = Assembler().Build(ValidDesign(stations: 1));

            Assert.False(computation.IsValid);
            Assert.Contains(
                computation.Diagnostics,
                d => d.Code == CantileverDiagnostics.LineNeedsTwoStations);
        }

        [Fact]
        public void TheComputationDoesNotAliasTheCallersDesign()
        {
            // The editor goes on mutating its live design after a recompute. A computation that held that instance
            // would silently describe a line nobody resolved.
            var design = ValidDesign();
            var computation = Assembler().Build(design);

            design.StationCount = 9;
            design.DefaultArmTemplate.Body.CutLength = 999.0;

            Assert.Equal(3, computation.Design.StationCount);
            Assert.Equal(36.0, computation.Design.DefaultArmTemplate.Body.CutLength, 9);
            Assert.Equal(3, computation.Line.StationCount);
        }

        [Fact]
        public void AnOutOfRangeLateralIndexFallsBackToTheFirstStationInsteadOfThrowing()
        {
            // An editor recomputes on every keystroke, and the station box can hold a number the line no longer
            // has for as long as it takes to type the next digit.
            var computation = Assembler().Build(ValidDesign(stations: 2), lateralStationIndex: 7);

            Assert.True(computation.IsValid);
            Assert.Equal(0, computation.Views.Single(v => v.View == CantileverViewKind.Lateral).StationIndex);
        }

        [Fact]
        public void ProjectingAnAlreadyResolvedLineIsDeterministicAndDoesNotReResolveIt()
        {
            var assembler = Assembler();
            var computation = assembler.Build(ValidDesign());

            var first = assembler.View(computation.Line, CantileverViewKind.Planta);
            var second = assembler.View(computation.Line, CantileverViewKind.Planta);

            Assert.Equal(first.Signature(), second.Signature());
        }

        [Fact]
        public void TheAssemblerRejectsANullCatalogueRatherThanResolvingAgainstNothing()
        {
            Assert.Throws<ArgumentNullException>(() => new CantileverLineEditorAssembler(null));
        }

        [Fact]
        public void SectionsOfAFamilyComeFromTheCatalogueAndNotFromAHardCodedList()
        {
            var w = Assembler().SectionsOf(StructuralSectionFamily.W);

            Assert.NotEmpty(w);
            Assert.All(w, section => Assert.Equal(StructuralSectionFamily.W, section.Family));
            Assert.Contains(w, section => section.SectionId.Value == ColumnW);
        }
    }
}
