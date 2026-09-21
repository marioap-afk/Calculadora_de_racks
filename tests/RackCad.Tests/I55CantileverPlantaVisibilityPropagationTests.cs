using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G5: the persisted Cantilever planta policy reaches both Plugin plan callers.</summary>
    public sealed class I55CantileverPlantaVisibilityPropagationTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        [Fact]
        public void BothPluginCallersForwardTheDesignAuthorityAndThePlanHonorsIt()
        {
            var source = I55ProductCharacterizationTestSupport.Code(
                "src", "RackCad.Plugin", "RackCantileverCommands.cs");

            Assert.Equal(2, I55ProductCharacterizationTestSupport.Count(
                source, "CantileverViewPlanBuilder.Build("));
            Assert.Contains(
                "section < 0 ? 0 : section, design.PlantaVisibility)",
                source);
            Assert.Contains(
                "station < 0 ? 0 : station, design.PlantaVisibility)",
                source);
            Assert.Equal(2, I55ProductCharacterizationTestSupport.Count(source, "design.PlantaVisibility"));

            var historicalDefault = CantileverRoundTwoCharacterizationTests.Reference();
            var defaultPlan = Planta(historicalDefault, historicalDefault.PlantaVisibility);
            Assert.DoesNotContain(defaultPlan.Curves, curve => curve.Kind == CantileverViewPieceKind.Arm);
            Assert.DoesNotContain(defaultPlan.Curves, curve => curve.Kind == CantileverViewPieceKind.Brace);

            var nullablePlan = Planta(historicalDefault, null);
            Assert.Equal(Kinds(defaultPlan), Kinds(nullablePlan));

            var armsOnly = CantileverRoundTwoCharacterizationTests.Reference();
            armsOnly.PlantaVisibility = new CantileverPlantaVisibilityDesign
            {
                ShowArms = true,
                ShowBraces = false
            };

            var store = new RackProjectStore();
            var reopened = store.Deserialize(store.Serialize(RackProject.ForCantilever(armsOnly)))
                .CantileverLineDesign;
            var armsPlan = Planta(reopened, reopened.PlantaVisibility);
            Assert.Contains(armsPlan.Curves, curve => curve.Kind == CantileverViewPieceKind.Arm);
            Assert.DoesNotContain(armsPlan.Curves, curve => curve.Kind == CantileverViewPieceKind.Brace);

            var bracesOnly = CantileverRoundTwoCharacterizationTests.Reference();
            bracesOnly.PlantaVisibility = new CantileverPlantaVisibilityDesign
            {
                ShowArms = false,
                ShowBraces = true
            };
            var bracesPlan = Planta(bracesOnly, bracesOnly.PlantaVisibility);
            Assert.DoesNotContain(bracesPlan.Curves, curve => curve.Kind == CantileverViewPieceKind.Arm);
            Assert.Contains(bracesPlan.Curves, curve => curve.Kind == CantileverViewPieceKind.Brace);
        }

        private static CantileverViewPlan Planta(
            CantileverLineDesign design,
            CantileverPlantaVisibilityDesign visibility)
        {
            var line = new CantileverLineEditorAssembler(Catalog).Build(design).Line;
            return CantileverViewPlanBuilder.Build(line, CantileverViewKind.Planta, Factory, 0, visibility);
        }

        private static CantileverViewPieceKind[] Kinds(CantileverViewPlan plan) =>
            plan.Curves.Select(curve => curve.Kind).ToArray();
    }
}
