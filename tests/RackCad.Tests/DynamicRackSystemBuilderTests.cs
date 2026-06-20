using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicRackSystemBuilderTests
    {
        private static PalletSpecification Pallet48()
        {
            return new PalletSpecification(front: 42.0, depth: 48.0, height: 60.0, weight: 1000.0, weightUnit: "kg");
        }

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem Build(int palletsDeep)
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                Pallet48(), palletsDeep, RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0);
        }

        private static (DynamicRackModuleKind Kind, double Start, double End) Triple(DynamicRackModule module)
        {
            return (module.Kind, module.StartX, module.EndX);
        }

        // ---- N modulos; total = N*fondo+12 (solo los extremos llevan +6) ----

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(8)]
        public void BuildDefault_ModuleCountEqualsN_AndTotalMatchesRule(int palletsDeep)
        {
            var system = Build(palletsDeep);

            Assert.Equal(palletsDeep, system.Modules.Count);
            Assert.Equal(palletsDeep * 48.0 + 12.0, system.TotalLength);
            Assert.Equal(system.TotalLength, system.Modules.Sum(m => m.Length));
        }

        [Fact]
        public void BuildDefault_OnlyEndsAreDepthPlusSix_EverythingElseIsDepth()
        {
            var system = Build(5);

            Assert.Equal(54.0, system.Modules.First().Length);
            Assert.Equal(54.0, system.Modules.Last().Length);
            Assert.All(system.Modules.Skip(1).Take(system.Modules.Count - 2), m => Assert.Equal(48.0, m.Length));
        }

        // ---- Layout alternado exacto (ejemplos del usuario) ----

        [Fact]
        public void BuildDefault_Three_ProducesHeaderSeparatorHeader()
        {
            Assert.Equal(new[]
            {
                (DynamicRackModuleKind.HeaderStart, 0.0, 54.0),
                (DynamicRackModuleKind.Separator, 54.0, 102.0),
                (DynamicRackModuleKind.HeaderEnd, 102.0, 156.0)
            }, Build(3).Modules.Select(Triple).ToArray());
        }

        [Fact]
        public void BuildDefault_Four_HasTwoConsecutiveSeparators_AndOneDerivedCenterPost()
        {
            var system = Build(4);

            Assert.Equal(new[]
            {
                (DynamicRackModuleKind.HeaderStart, 0.0, 54.0),
                (DynamicRackModuleKind.Separator, 54.0, 102.0),
                (DynamicRackModuleKind.Separator, 102.0, 150.0),
                (DynamicRackModuleKind.HeaderEnd, 150.0, 204.0)
            }, system.Modules.Select(Triple).ToArray());

            Assert.Equal(new[] { 102.0 }, system.GetDerivedPostOffsets()); // poste between the two separators (center)
        }

        [Fact]
        public void BuildDefault_Five_HasInteriorHeaderOfDepth_AndNoPost()
        {
            var system = Build(5);

            Assert.Equal(new[]
            {
                (DynamicRackModuleKind.HeaderStart, 0.0, 54.0),
                (DynamicRackModuleKind.Separator, 54.0, 102.0),
                (DynamicRackModuleKind.HeaderIntermediate, 102.0, 150.0),
                (DynamicRackModuleKind.Separator, 150.0, 198.0),
                (DynamicRackModuleKind.HeaderEnd, 198.0, 252.0)
            }, system.Modules.Select(Triple).ToArray());

            Assert.Empty(system.GetDerivedPostOffsets());

            var interiorHeader = system.Modules[2];
            Assert.True(interiorHeader.IsHeader);
            Assert.Equal(48.0, interiorHeader.AssociatedFrameConfiguration.Depth); // interior header is fondo deep
        }

        [Fact]
        public void BuildDefault_Eight_AlternatesWithCenterPost()
        {
            var system = Build(8);

            Assert.Equal(
                new[]
                {
                    DynamicRackModuleKind.HeaderStart, DynamicRackModuleKind.Separator, DynamicRackModuleKind.HeaderIntermediate,
                    DynamicRackModuleKind.Separator, DynamicRackModuleKind.Separator, DynamicRackModuleKind.HeaderIntermediate,
                    DynamicRackModuleKind.Separator, DynamicRackModuleKind.HeaderEnd
                },
                system.Modules.Select(m => m.Kind).ToArray());
            Assert.Equal(396.0, system.TotalLength);
            Assert.Equal(new[] { 198.0 }, system.GetDerivedPostOffsets());
        }

        // ---- Postes derivados ----

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void BuildDefault_OddCount_HasNoDerivedPost(int palletsDeep)
        {
            Assert.Empty(Build(palletsDeep).GetDerivedPostOffsets());
        }

        [Fact]
        public void GetDerivedPostOffsets_AppearsWhenEditsCreateConsecutiveSeparators()
        {
            var builder = new DynamicRackSystemBuilder(Catalog);
            var system = builder.BuildDefault(Pallet48(), 5, RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0);
            Assert.Empty(system.GetDerivedPostOffsets());

            // Turn the interior header (index 2) into a separator -> now separators 1,2,3 are consecutive.
            system.Modules[2].Kind = DynamicRackModuleKind.Separator;
            system.Modules[2].AssociatedFrameConfiguration = null;
            builder.Refresh(system);

            Assert.NotEmpty(system.GetDerivedPostOffsets());
        }

        // ---- Cabeceras independientes + edicion ----

        [Fact]
        public void BuildDefault_EachHeaderHasItsOwnConfiguration()
        {
            var system = Build(5);
            var headers = system.Modules.Where(m => m.IsHeader).Select(m => m.AssociatedFrameConfiguration).ToList();

            Assert.Equal(3, headers.Count);
            Assert.Equal(headers.Count, headers.Distinct().Count()); // all distinct instances
            Assert.All(headers, h => Assert.NotEmpty(h.Members));
        }

        [Fact]
        public void Refresh_AfterEditingHeaderLength_SyncsConfigDepthAndPositions()
        {
            var builder = new DynamicRackSystemBuilder(Catalog);
            var system = builder.BuildDefault(Pallet48(), 4, RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0);

            var startHeader = system.Modules.First();
            startHeader.Length = 60.0;
            startHeader.IsManualOverride = true;

            builder.Refresh(system);

            Assert.Equal(60.0, startHeader.AssociatedFrameConfiguration.Depth);
            Assert.Equal(60.0, startHeader.EndX);
            Assert.Equal(system.Modules.Sum(m => m.Length), system.TotalLength);
        }

        // ---- Validaciones ----

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        public void BuildDefault_FewerThanTwoPallets_Throws(int palletsDeep)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Build(palletsDeep));
        }

        [Fact]
        public void BuildDefault_NullPallet_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamicRackSystemBuilder(Catalog).BuildDefault(null, 4, RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0));
        }
    }
}
