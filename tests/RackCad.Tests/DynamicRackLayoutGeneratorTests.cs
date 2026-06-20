using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicRackLayoutGeneratorTests
    {
        private static PalletSpecification Pallet48()
        {
            // Frente 42, Fondo 48, Altura 60, Peso 1000 kg (ejemplo tipico).
            return new PalletSpecification(front: 42.0, depth: 48.0, height: 60.0, weight: 1000.0, weightUnit: "kg");
        }

        private static DynamicRackLayout Generate(int palletsDeep, PalletSpecification pallet = null)
        {
            return new DynamicRackLayoutGenerator().Generate(pallet ?? Pallet48(), palletsDeep);
        }

        private static (RackModuleKind Kind, double Start, double End) Triple(RackModule module)
        {
            return (module.Kind, module.StartOffset, module.EndOffset);
        }

        // ---- Casos par / impar ----

        [Fact]
        public void Generate_OddCount_HasNoIntermediatePost()
        {
            var layout = Generate(3);

            Assert.DoesNotContain(layout.Modules, m => m.Kind == RackModuleKind.IntermediatePost);
            Assert.Equal(
                new[] { RackModuleKind.HeaderStart, RackModuleKind.Separator, RackModuleKind.HeaderEnd },
                layout.Modules.Select(m => m.Kind));
        }

        [Fact]
        public void Generate_EvenCount_HasOneIntermediatePostAtCenter()
        {
            var layout = Generate(4);

            var post = Assert.Single(layout.Modules.Where(m => m.Kind == RackModuleKind.IntermediatePost));
            Assert.Equal(layout.TotalLength / 2.0, post.StartOffset);
            Assert.Equal(post.StartOffset, post.EndOffset);
            Assert.Equal(0.0, post.Length);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(20)]
        public void Generate_EvenCountFourOrMore_HasExactlyOnePost(int palletsDeep)
        {
            var layout = Generate(palletsDeep);
            var expectedPosts = palletsDeep >= 4 ? 1 : 0; // N=2 has no middle boundary
            Assert.Equal(expectedPosts, layout.Modules.Count(m => m.Kind == RackModuleKind.IntermediatePost));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        [InlineData(9)]
        public void Generate_OddCount_NeverHasPost(int palletsDeep)
        {
            Assert.DoesNotContain(Generate(palletsDeep).Modules, m => m.Kind == RackModuleKind.IntermediatePost);
        }

        // ---- Longitud total ----

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(8)]
        public void Generate_TotalLength_EqualsNTimesDepthPlusTwelve(int palletsDeep)
        {
            var layout = Generate(palletsDeep);
            Assert.Equal(palletsDeep * 48.0 + 12.0, layout.TotalLength);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(8)]
        public void Generate_SumOfModuleLengths_EqualsTotalLength(int palletsDeep)
        {
            var layout = Generate(palletsDeep);
            Assert.Equal(layout.TotalLength, layout.SumOfModuleLengths);
        }

        // ---- Distribucion de modulos ----

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(8)]
        public void Generate_AlwaysTwoHeadersAndNMinusTwoSeparators(int palletsDeep)
        {
            var layout = Generate(palletsDeep);

            Assert.Equal(1, layout.Modules.Count(m => m.Kind == RackModuleKind.HeaderStart));
            Assert.Equal(1, layout.Modules.Count(m => m.Kind == RackModuleKind.HeaderEnd));
            Assert.Equal(palletsDeep - 2, layout.Modules.Count(m => m.Kind == RackModuleKind.Separator));
            Assert.Equal(RackModuleKind.HeaderStart, layout.Modules.First().Kind);
            Assert.Equal(RackModuleKind.HeaderEnd, layout.Modules.Last().Kind);
        }

        [Fact]
        public void Generate_HeaderModules_AreDepthPlusSix_SeparatorsAreDepth()
        {
            var layout = Generate(4);

            Assert.All(
                layout.Modules.Where(m => m.Kind == RackModuleKind.HeaderStart || m.Kind == RackModuleKind.HeaderEnd),
                header => Assert.Equal(54.0, header.Length));
            Assert.All(
                layout.Modules.Where(m => m.Kind == RackModuleKind.Separator),
                separator => Assert.Equal(48.0, separator.Length));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(8)]
        public void Generate_ModulesAreContiguousAndSequentiallyIndexed(int palletsDeep)
        {
            var modules = Generate(palletsDeep).Modules;

            Assert.Equal(0.0, modules.First().StartOffset);
            for (var i = 0; i < modules.Count; i++)
            {
                Assert.Equal(i, modules[i].Index);
                if (i > 0)
                {
                    // Each module starts exactly where the previous one ended (posts are zero-length).
                    Assert.Equal(modules[i - 1].EndOffset, modules[i].StartOffset);
                }
            }
            Assert.Equal(Generate(palletsDeep).TotalLength, modules.Last().EndOffset);
        }

        // ---- Layouts exactos (ejemplo de salida) ----

        [Fact]
        public void Generate_Three_ProducesExpectedLayout()
        {
            var modules = Generate(3).Modules.Select(Triple).ToArray();

            Assert.Equal(new[]
            {
                (RackModuleKind.HeaderStart, 0.0, 54.0),
                (RackModuleKind.Separator, 54.0, 102.0),
                (RackModuleKind.HeaderEnd, 102.0, 156.0)
            }, modules);
        }

        [Fact]
        public void Generate_Four_ProducesExpectedLayout()
        {
            var modules = Generate(4).Modules.Select(Triple).ToArray();

            Assert.Equal(new[]
            {
                (RackModuleKind.HeaderStart, 0.0, 54.0),
                (RackModuleKind.Separator, 54.0, 102.0),
                (RackModuleKind.IntermediatePost, 102.0, 102.0),
                (RackModuleKind.Separator, 102.0, 150.0),
                (RackModuleKind.HeaderEnd, 150.0, 204.0)
            }, modules);
        }

        [Fact]
        public void Generate_Eight_ProducesExpectedLayout()
        {
            var modules = Generate(8).Modules.Select(Triple).ToArray();

            Assert.Equal(new[]
            {
                (RackModuleKind.HeaderStart, 0.0, 54.0),
                (RackModuleKind.Separator, 54.0, 102.0),
                (RackModuleKind.Separator, 102.0, 150.0),
                (RackModuleKind.Separator, 150.0, 198.0),
                (RackModuleKind.IntermediatePost, 198.0, 198.0),
                (RackModuleKind.Separator, 198.0, 246.0),
                (RackModuleKind.Separator, 246.0, 294.0),
                (RackModuleKind.Separator, 294.0, 342.0),
                (RackModuleKind.HeaderEnd, 342.0, 396.0)
            }, modules);
        }

        // ---- Validaciones ----

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Generate_FewerThanTwoPallets_Throws(int palletsDeep)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Generate(palletsDeep));
        }

        [Fact]
        public void Generate_NonPositiveDepth_Throws()
        {
            var pallet = new PalletSpecification(front: 42.0, depth: 0.0, height: 60.0, weight: 1000.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicRackLayoutGenerator().Generate(pallet, 4));
        }

        [Fact]
        public void Generate_NullPallet_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicRackLayoutGenerator().Generate(null, 4));
        }

        // ---- Regla de postes como estrategia ----

        [Fact]
        public void Generate_WithCustomNoPostRule_NeverAddsPosts()
        {
            var generator = new DynamicRackLayoutGenerator(new NoPostRule());

            var layout = generator.Generate(Pallet48(), 4);

            Assert.DoesNotContain(layout.Modules, m => m.Kind == RackModuleKind.IntermediatePost);
        }

        private sealed class NoPostRule : IIntermediatePostRule
        {
            public IReadOnlyList<double> ResolvePostOffsets(int palletsDeep, IReadOnlyList<RackModule> lengthModules)
            {
                return Array.Empty<double>();
            }
        }
    }
}
