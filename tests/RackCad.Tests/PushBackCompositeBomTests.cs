using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G4) — separador central, topes y PROPIEDAD FISICA en el BOM. El BOM cuenta piezas reales del plan: una
    /// cabecera, un poste o una placa presentes una vez fisicamente valen UNO aunque los usen los dos lados, una
    /// cama corrida vale UNA y dos encontradas valen DOS. No se genera A + B para deduplicar despues.
    /// </summary>
    public class PushBackCompositeBomTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static int Quantity(BillOfMaterials bom, string category)
            => bom.Components.Where(component => component.Category == category).Sum(component => component.Quantity);

        private static PushBackDesign Design(
            PushBackCellTopology topology, int levelsA = 1, int levelsB = 1, double gap = 0.0,
            bool centralSeparator = false, int slotsA = 1, int slotsB = 1)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slotsA, slotsB: slotsB, deepA: 4, deepB: 4,
                levelsA: levelsA, levelsB: levelsB, gap: gap, centralSeparator: centralSeparator);
            design.Composite.DefaultTopology = topology;
            return design;
        }

        // ---- Camas: una ejecucion fisica, una linea ---------------------------------------------------------

        [Fact]
        public void Encontradas_CountTwoPhysicalBeds_AndCorridaCountsOne()
        {
            var encontradas = new PushBackResolver(Catalog).Resolve(Design(PushBackCellTopology.Encontradas));
            var corrida = new PushBackResolver(Catalog).Resolve(Design(PushBackCellTopology.Corrida));

            Assert.Equal(2, Quantity(PushBackBomBuilder.Build(encontradas, Catalog), SystemBomBuilder.Cama));
            Assert.Equal(1, Quantity(PushBackBomBuilder.Build(corrida, Catalog), SystemBomBuilder.Cama));
        }

        [Fact]
        public void ACorridaBed_IsQuotedAtTheWholeRackLength()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design(PushBackCellTopology.Corrida, gap: 10.0));
            var bed = PushBackBomBuilder.Build(system, Catalog).Components
                .Single(component => component.Category == SystemBomBuilder.Cama);

            Assert.Equal(system.Structure.TotalLength, bed.Length, 4);
        }

        [Fact]
        public void EveryPhysicalBed_CarriesItsOwnLowAndHighBeam()
        {
            var encontradas = new PushBackResolver(Catalog).Resolve(Design(PushBackCellTopology.Encontradas));
            var bom = PushBackBomBuilder.Build(encontradas, Catalog);

            Assert.Equal(2, Quantity(bom, SystemBomBuilder.InOutBeam));
            Assert.Equal(2, Quantity(bom, PushBackBomBuilder.HighEndBeam));

            var corrida = new PushBackResolver(Catalog).Resolve(Design(PushBackCellTopology.Corrida));
            var corridaBom = PushBackBomBuilder.Build(corrida, Catalog);
            Assert.Equal(1, Quantity(corridaBom, SystemBomBuilder.InOutBeam));
            Assert.Equal(1, Quantity(corridaBom, PushBackBomBuilder.HighEndBeam));
        }

        // ---- Topes: uno por cama fisica, solo en su extremo ALTO --------------------------------------------

        [Fact]
        public void Encontradas_AdmitTwoIndependentTopes()
        {
            var design = Design(PushBackCellTopology.Encontradas);
            var both = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(2, Quantity(PushBackBomBuilder.Build(both, Catalog), PushBackBomBuilder.RearTope));

            design.RearTope.Disable(0, 0);        // solo el del lado A
            var onlyB = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(1, Quantity(PushBackBomBuilder.Build(onlyB, Catalog), PushBackBomBuilder.RearTope));

            design.SideB.RearTope.Disable(0, 0);  // ninguno
            var none = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(0, Quantity(PushBackBomBuilder.Build(none, Catalog), PushBackBomBuilder.RearTope));
        }

        [Fact]
        public void ACorrida_AdmitsAtMostOneTope_OnItsHighEnd()
        {
            var design = Design(PushBackCellTopology.Corrida);
            design.Composite.DefaultDirection = PushBackRunDirection.AToB;   // extremo alto = lado B
            var system = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(1, Quantity(PushBackBomBuilder.Build(system, Catalog), PushBackBomBuilder.RearTope));

            // Apagar el tope del lado BAJO no quita nada: ese extremo no tiene tope que apagar.
            design.RearTope.Disable(0, 0);
            var stillOne = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(1, Quantity(PushBackBomBuilder.Build(stillOne, Catalog), PushBackBomBuilder.RearTope));

            // Apagar el del lado ALTO si lo quita.
            design.SideB.RearTope.Disable(0, 0);
            var none = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(0, Quantity(PushBackBomBuilder.Build(none, Catalog), PushBackBomBuilder.RearTope));
        }

        [Fact]
        public void ACorrida_BToA_TakesItsTopeFromSideA()
        {
            var design = Design(PushBackCellTopology.Corrida);
            design.Composite.DefaultDirection = PushBackRunDirection.BToA;   // extremo alto = lado A
            Assert.Equal(
                1,
                Quantity(PushBackBomBuilder.Build(new PushBackResolver(Catalog).Resolve(design), Catalog),
                    PushBackBomBuilder.RearTope));

            design.RearTope.Disable(0, 0);   // el lado A es ahora el ALTO
            Assert.Equal(
                0,
                Quantity(PushBackBomBuilder.Build(new PushBackResolver(Catalog).Resolve(design), Catalog),
                    PushBackBomBuilder.RearTope));
        }

        // ---- Estructura: una sola propiedad fisica -----------------------------------------------------------

        [Fact]
        public void TheCentralSeparator_IsCountedExactlyOnce()
        {
            var without = new PushBackResolver(Catalog)
                .Resolve(Design(PushBackCellTopology.Encontradas, gap: 10.0));
            var with = new PushBackResolver(Catalog)
                .Resolve(Design(PushBackCellTopology.Encontradas, gap: 10.0, centralSeparator: true));

            var before = Quantity(PushBackBomBuilder.Build(without, Catalog), SystemBomBuilder.Separator);
            var after = Quantity(PushBackBomBuilder.Build(with, Catalog), SystemBomBuilder.Separator);

            // El separador central es la MISMA pieza del rack; aporta sus niveles UNA vez, no dos por tener dos lados.
            Assert.True(after > before);
            var separatorLengths = PushBackBomBuilder.Build(with, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.Separator)
                .Select(component => component.Length)
                .ToList();
            Assert.Contains(separatorLengths, length => System.Math.Abs(length - 10.0) < 1e-4);
            Assert.Single(separatorLengths, length => System.Math.Abs(length - 10.0) < 1e-4);
        }

        [Fact]
        public void TheStructure_IsNotDuplicatedBetweenSides()
        {
            var composite = new PushBackResolver(Catalog).Resolve(Design(PushBackCellTopology.Encontradas));
            var bom = PushBackBomBuilder.Build(composite, Catalog);

            // Cada linea de cabecera existe una vez: el numero de marcos del BOM coincide con el de modulos de
            // cabecera de la estructura compartida por linea transversal, no con el doble.
            var headerModules = composite.Structure.Modules.Count(module => module.IsHeader);
            var frames = bom.Components
                .Where(component => component.Category != SystemBomBuilder.Cama
                    && component.Category != SystemBomBuilder.InOutBeam
                    && component.Category != PushBackBomBuilder.HighEndBeam
                    && component.Category != PushBackBomBuilder.RearTope)
                .ToList();
            Assert.NotEmpty(frames);
            Assert.True(headerModules > 0);
            // El BOM del rack compuesto NO es la suma de dos BOM de un lado: la estructura se cuenta una sola vez.
            Assert.All(bom.Components, component => Assert.True(component.Quantity > 0));
        }

        [Fact]
        public void ASlotPresentOnlyInB_ContributesLoadOnlyOnThatSide()
        {
            var design = Design(PushBackCellTopology.Encontradas, slotsA: 1, slotsB: 2);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var bom = PushBackBomBuilder.Build(system, Catalog);

            // Ranura 0: dos camas (encontradas). Ranura 1: solo B, una cama. Total 3.
            Assert.Equal(3, Quantity(bom, SystemBomBuilder.Cama));
            Assert.Equal(3, Quantity(bom, SystemBomBuilder.InOutBeam));
            Assert.Equal(3, Quantity(bom, PushBackBomBuilder.HighEndBeam));
        }

        [Fact]
        public void ThePalletsNeverReachTheBom()
        {
            var design = Design(PushBackCellTopology.Encontradas);
            design.Fronts[0].DrawPallets.Add(true);
            design.SideB.FrontConfigs[0].DrawPallets.Add(true);
            var system = new PushBackResolver(Catalog).Resolve(design);

            var bom = PushBackBomBuilder.Build(system, Catalog);
            Assert.DoesNotContain(bom.Components, component =>
                component.Category != null && component.Category.IndexOf("arima", System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
