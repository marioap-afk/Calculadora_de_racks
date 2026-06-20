using System;
using System.Collections.Generic;
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

        // ---- Conteo de modulos con longitud = N; total = N*fondo+12 ----

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void BuildDefault_LengthBearingCountEqualsN_AndTotalMatchesRule(int palletsDeep)
        {
            var system = Build(palletsDeep);

            Assert.Equal(palletsDeep, system.LengthBearingModuleCount);
            Assert.Equal(palletsDeep * 48.0 + 12.0, system.TotalLength);
            Assert.Equal(system.TotalLength, system.Modules.Sum(m => m.Length));
        }

        [Fact]
        public void BuildDefault_EndsAreHeadersWithOwnConfig_InteriorsAreSeparators()
        {
            var system = Build(5);

            Assert.Equal(DynamicRackModuleKind.HeaderStart, system.Modules.First().Kind);
            Assert.Equal(DynamicRackModuleKind.HeaderEnd, system.Modules.Last().Kind);
            Assert.Equal(54.0, system.Modules.First().Length);
            Assert.Equal(54.0, system.Modules.Last().Length);

            var interiors = system.Modules.Where(m => m.Length > 0 && !m.IsHeader).ToList();
            Assert.Equal(3, interiors.Count);
            Assert.All(interiors, m => Assert.Equal(DynamicRackModuleKind.Separator, m.Kind));
            Assert.All(interiors, m => Assert.Equal(48.0, m.Length));
            Assert.All(interiors, m => Assert.Null(m.AssociatedFrameConfiguration));

            // Each header module owns an independent configuration.
            var start = system.Modules.First().AssociatedFrameConfiguration;
            var end = system.Modules.Last().AssociatedFrameConfiguration;
            Assert.NotNull(start);
            Assert.NotNull(end);
            Assert.NotSame(start, end);
            Assert.Equal(54.0, start.Depth);
            Assert.NotEmpty(start.Members);
        }

        // ---- Postes: modulo de longitud 0 en la lista, no suman longitud ni cuentan como length-bearing ----

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void BuildDefault_OddCount_HasNoPostModule(int palletsDeep)
        {
            Assert.DoesNotContain(Build(palletsDeep).Modules, m => m.Kind == DynamicRackModuleKind.IntermediatePost);
        }

        [Fact]
        public void BuildDefault_EvenCount_HasOneZeroLengthPostModuleAtCenter()
        {
            var system = Build(8);

            var post = Assert.Single(system.Modules.Where(m => m.Kind == DynamicRackModuleKind.IntermediatePost));
            Assert.Equal(0.0, post.Length);
            Assert.Equal(post.StartX, post.EndX);
            Assert.Equal(system.TotalLength / 2.0, post.StartX); // 198
            Assert.Equal(8, system.LengthBearingModuleCount);     // post does not change the count
            Assert.Equal(396.0, system.TotalLength);              // nor the total
        }

        // ---- Contigüidad ----

        [Theory]
        [InlineData(3)]
        [InlineData(8)]
        public void BuildDefault_ModulesAreContiguousAndIndexed(int palletsDeep)
        {
            var modules = Build(palletsDeep).Modules;

            Assert.Equal(0.0, modules.First().StartX);
            for (var i = 0; i < modules.Count; i++)
            {
                Assert.Equal(i, modules[i].Index);
                if (i > 0)
                {
                    Assert.Equal(modules[i - 1].EndX, modules[i].StartX);
                }
            }
        }

        // ---- Layouts exactos para 3, 5 y 8 ----

        [Fact]
        public void BuildDefault_Three_ProducesExpectedLayout()
        {
            Assert.Equal(new[]
            {
                (DynamicRackModuleKind.HeaderStart, 0.0, 54.0),
                (DynamicRackModuleKind.Separator, 54.0, 102.0),
                (DynamicRackModuleKind.HeaderEnd, 102.0, 156.0)
            }, Build(3).Modules.Select(Triple).ToArray());
        }

        [Fact]
        public void BuildDefault_Five_ProducesExpectedLayout()
        {
            Assert.Equal(new[]
            {
                (DynamicRackModuleKind.HeaderStart, 0.0, 54.0),
                (DynamicRackModuleKind.Separator, 54.0, 102.0),
                (DynamicRackModuleKind.Separator, 102.0, 150.0),
                (DynamicRackModuleKind.Separator, 150.0, 198.0),
                (DynamicRackModuleKind.HeaderEnd, 198.0, 252.0)
            }, Build(5).Modules.Select(Triple).ToArray());
        }

        [Fact]
        public void BuildDefault_Eight_ProducesExpectedLayoutWithCenterPost()
        {
            Assert.Equal(new[]
            {
                (DynamicRackModuleKind.HeaderStart, 0.0, 54.0),
                (DynamicRackModuleKind.Separator, 54.0, 102.0),
                (DynamicRackModuleKind.Separator, 102.0, 150.0),
                (DynamicRackModuleKind.Separator, 150.0, 198.0),
                (DynamicRackModuleKind.IntermediatePost, 198.0, 198.0),
                (DynamicRackModuleKind.Separator, 198.0, 246.0),
                (DynamicRackModuleKind.Separator, 246.0, 294.0),
                (DynamicRackModuleKind.Separator, 294.0, 342.0),
                (DynamicRackModuleKind.HeaderEnd, 342.0, 396.0)
            }, Build(8).Modules.Select(Triple).ToArray());
        }

        // ---- Edicion / override + Refresh ----

        [Fact]
        public void Refresh_AfterEditingHeaderLength_RecalculatesPositionsAndSyncsHeaderDepth()
        {
            var builder = new DynamicRackSystemBuilder(Catalog);
            var system = builder.BuildDefault(Pallet48(), 4, RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0);

            var startHeader = system.Modules.First();
            startHeader.Length = 60.0;
            startHeader.IsManualOverride = true;

            builder.Refresh(system);

            Assert.Equal(60.0, startHeader.AssociatedFrameConfiguration.Depth); // header depth follows the module length
            Assert.Equal(0.0, startHeader.StartX);
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

        [Fact]
        public void BuildDefault_WithNoPostRule_NeverAddsPostModules()
        {
            var builder = new DynamicRackSystemBuilder(new RackFrameConfigurationFactory(Catalog), new NoPostRule());
            var system = builder.BuildDefault(Pallet48(), 8, RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0);

            Assert.DoesNotContain(system.Modules, m => m.Kind == DynamicRackModuleKind.IntermediatePost);
            Assert.Equal(8, system.Modules.Count);
        }

        private sealed class NoPostRule : IIntermediatePostRule
        {
            public IReadOnlyList<double> ResolvePostOffsets(int palletsDeep, IReadOnlyList<DynamicRackModule> lengthModules)
            {
                return Array.Empty<double>();
            }
        }
    }
}
