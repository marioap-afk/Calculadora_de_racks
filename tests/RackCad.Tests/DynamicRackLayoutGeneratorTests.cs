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

        private static DynamicRackLayout Generate(int palletsDeep)
        {
            return new DynamicRackLayoutGenerator().Generate(Pallet48(), palletsDeep);
        }

        private static (RackModuleKind Kind, double Start, double End) Triple(RackModule module)
        {
            return (module.Kind, module.StartOffset, module.EndOffset);
        }

        // ---- Conteo de modulos = numero de tarimas de fondo ----

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(8)]
        [InlineData(20)]
        public void Generate_ModuleCount_EqualsPalletsDeep(int palletsDeep)
        {
            Assert.Equal(palletsDeep, Generate(palletsDeep).Modules.Count);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void Generate_AlwaysTwoHeadersAndNMinusTwoInteriorModules(int palletsDeep)
        {
            var modules = Generate(palletsDeep).Modules;

            Assert.Equal(RackModuleKind.HeaderStart, modules.First().Kind);
            Assert.Equal(RackModuleKind.HeaderEnd, modules.Last().Kind);
            Assert.Equal(palletsDeep - 2, modules.Count(m => m.Kind == RackModuleKind.Separator));
        }

        // ---- Longitud total = N x fondo + 12, primero/ultimo = fondo+6, interiores = fondo ----

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        public void Generate_TotalLength_EqualsNTimesDepthPlusTwelve(int palletsDeep)
        {
            var layout = Generate(palletsDeep);
            Assert.Equal(palletsDeep * 48.0 + 12.0, layout.TotalLength);
            Assert.Equal(layout.TotalLength, layout.SumOfModuleLengths);
        }

        [Fact]
        public void Generate_EndModulesAreDepthPlusSix_InteriorAreDepth()
        {
            var modules = Generate(5).Modules;

            Assert.Equal(54.0, modules.First().Length);
            Assert.Equal(54.0, modules.Last().Length);
            Assert.All(modules.Where(m => m.Kind == RackModuleKind.Separator), s => Assert.Equal(48.0, s.Length));
        }

        // ---- Postes intermedios: marcadores, no modulos; no agregan longitud ni cuentan como modulo ----

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void Generate_OddCount_HasNoIntermediatePost(int palletsDeep)
        {
            Assert.Empty(Generate(palletsDeep).IntermediatePosts);
        }

        [Fact]
        public void Generate_EvenCount_HasOnePostMarkerAtCenter_WithoutAddingModulesOrLength()
        {
            var layout = Generate(8);

            var post = Assert.Single(layout.IntermediatePosts);
            Assert.Equal(layout.TotalLength / 2.0, post);   // marker at the center boundary (198)
            Assert.Equal(8, layout.Modules.Count);          // module count unchanged by the post
            Assert.Equal(8 * 48.0 + 12.0, layout.TotalLength);
        }

        [Fact]
        public void Generate_TwoPallets_HasNoPostMarker()
        {
            Assert.Empty(Generate(2).IntermediatePosts); // no interior boundary
        }

        // ---- Contigüidad e indices ----

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
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
                    Assert.Equal(modules[i - 1].EndOffset, modules[i].StartOffset);
                }
            }
            Assert.Equal(Generate(palletsDeep).TotalLength, modules.Last().EndOffset);
        }

        // ---- Layouts exactos (ejemplo de salida) para 3, 5 y 8 ----

        [Fact]
        public void Generate_Three_ProducesExpectedLayout()
        {
            var layout = Generate(3);

            Assert.Equal(new[]
            {
                (RackModuleKind.HeaderStart, 0.0, 54.0),
                (RackModuleKind.Separator, 54.0, 102.0),
                (RackModuleKind.HeaderEnd, 102.0, 156.0)
            }, layout.Modules.Select(Triple).ToArray());
            Assert.Empty(layout.IntermediatePosts);
            Assert.Equal(156.0, layout.TotalLength);
        }

        [Fact]
        public void Generate_Five_ProducesExpectedLayout()
        {
            var layout = Generate(5);

            Assert.Equal(new[]
            {
                (RackModuleKind.HeaderStart, 0.0, 54.0),
                (RackModuleKind.Separator, 54.0, 102.0),
                (RackModuleKind.Separator, 102.0, 150.0),
                (RackModuleKind.Separator, 150.0, 198.0),
                (RackModuleKind.HeaderEnd, 198.0, 252.0)
            }, layout.Modules.Select(Triple).ToArray());
            Assert.Empty(layout.IntermediatePosts);
            Assert.Equal(252.0, layout.TotalLength);
        }

        [Fact]
        public void Generate_Eight_ProducesExpectedLayout()
        {
            var layout = Generate(8);

            Assert.Equal(new[]
            {
                (RackModuleKind.HeaderStart, 0.0, 54.0),
                (RackModuleKind.Separator, 54.0, 102.0),
                (RackModuleKind.Separator, 102.0, 150.0),
                (RackModuleKind.Separator, 150.0, 198.0),
                (RackModuleKind.Separator, 198.0, 246.0),
                (RackModuleKind.Separator, 246.0, 294.0),
                (RackModuleKind.Separator, 294.0, 342.0),
                (RackModuleKind.HeaderEnd, 342.0, 396.0)
            }, layout.Modules.Select(Triple).ToArray());
            Assert.Equal(new[] { 198.0 }, layout.IntermediatePosts);
            Assert.Equal(396.0, layout.TotalLength);
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
            var pallet = new PalletSpecification(42.0, 0.0, 60.0, 1000.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicRackLayoutGenerator().Generate(pallet, 4));
        }

        [Fact]
        public void Generate_NullPallet_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicRackLayoutGenerator().Generate(null, 4));
        }

        // ---- Regla de postes como estrategia ----

        [Fact]
        public void Generate_WithCustomNoPostRule_NeverAddsPostMarkers()
        {
            var layout = new DynamicRackLayoutGenerator(new NoPostRule()).Generate(Pallet48(), 8);

            Assert.Empty(layout.IntermediatePosts);
            Assert.Equal(8, layout.Modules.Count);
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
