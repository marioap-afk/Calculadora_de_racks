using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicSafetyDefaultsTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Build_EnablesEveryDynamicSafetyFamilyFromTheCatalog()
        {
            var selections = DynamicSafetyDefaults.Build(Catalog);

            Assert.Equal(5, selections.Count);
            Assert.All(new[]
            {
                SelectiveSafetyDefaults.BotaType,
                SelectiveSafetyDefaults.LateralType,
                SelectiveSafetyDefaults.DesviadorType,
                SelectiveSafetyDefaults.DefensaType,
                SelectiveSafetyDefaults.GuiaType
            }, family => Assert.Contains(selections, selection =>
            {
                var element = Catalog.SafetyElements.First(entry => entry.Id == selection.ElementId);
                return SelectiveSafetyDefaults.IsType(element.Type, family) && selection.Quantity == 1;
            }));

            Assert.Equal(SafetySide.Both, SelectionOfType(selections, SelectiveSafetyDefaults.BotaType).Side);
            Assert.Equal(SafetySide.Both, SelectionOfType(selections, SelectiveSafetyDefaults.DesviadorType).Side);
            Assert.Empty(SelectionOfType(selections, SelectiveSafetyDefaults.GuiaType).GuiaEntradaOffCells);
        }

        [Fact]
        public void LateralGuard_DefaultOrillasRemainAdaptiveUntilTheGridIsAuthored()
        {
            var selection = SelectionOfType(
                DynamicSafetyDefaults.Build(Catalog), SelectiveSafetyDefaults.LateralType);

            Assert.Equal(SafetySide.Left, DynamicLateralGuardPlan.SideAt(selection, 0, 3));
            Assert.Equal(SafetySide.None, DynamicLateralGuardPlan.SideAt(selection, 1, 3));
            Assert.Equal(SafetySide.Right, DynamicLateralGuardPlan.SideAt(selection, 2, 3));
            Assert.Equal(SafetySide.Right, DynamicLateralGuardPlan.SideAt(selection, 7, 8));

            selection.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.None });
            Assert.Equal(SafetySide.None, DynamicLateralGuardPlan.SideAt(selection, 0, 3));
            Assert.Equal(SafetySide.None, DynamicLateralGuardPlan.SideAt(selection, 2, 3));
        }

        private static SelectiveSafetySelection SelectionOfType(
            System.Collections.Generic.IEnumerable<SelectiveSafetySelection> selections,
            string type)
            => selections.First(selection =>
            {
                var element = Catalog.SafetyElements.First(entry => entry.Id == selection.ElementId);
                return SelectiveSafetyDefaults.IsType(element.Type, type);
            });
    }
}
