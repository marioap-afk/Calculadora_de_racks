using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D6, contrato del dueño) — UNA LINEA FISICA EN UNA PROFUNDIDAD TIENE UNA SOLA ALTURA, y las cuatro
    /// salidas la consumen: el corte, el lateral general, el lateral seccionado y el BOM —tanto sus separadores como
    /// sus postes derivados—.
    ///
    /// <para>
    /// Complementa a <see cref="PushBackHeaderHeightAuthorityTests"/>, que ya fija el eje LINEA —una linea toma la
    /// demanda de sus frentes adyacentes y de nadie mas, y A no arrastra a B—. Aqui se fija el eje PROFUNDIDAD: la
    /// misma linea puede medir dos alturas, una por zona, y quien pregunta debe decir en que X.
    /// </para>
    ///
    /// <para>
    /// El fixture es deliberadamente ASIMETRICO: el lado A pide 264" y el B 48". Con una altura por linea aplanada
    /// las dos mitades responden lo mismo y el defecto es invisible.
    /// </para>
    /// </summary>
    public class PushBackHeaderDepthHeightTests
    {
        private const string DerivedPost = "Poste reforzado";
        private const string Separator = "Separador";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Compuesto asimetrico: A con 4 niveles, B con 1. Sus zonas piden alturas muy distintas.</summary>
        private static PushBackSystem Asymmetric()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: 4, levelsB: 1, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            design.Structure.FirstLevelHeight = 6.0;
            design.SideB.FirstLevelHeight = 4.0;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static DynamicHeaderHeightZone ZoneOf(PushBackSystem system, PushBackSide side)
        {
            var view = system.Composite.Of(side);
            var start = Math.Min(view.OuterX, view.InnerX);
            return system.Structure.HeaderHeightZones.Single(zone => Math.Abs(zone.StartX - start) < 1e-6);
        }

        private static double MidOf(DynamicHeaderHeightZone zone) => 0.5 * (zone.StartX + zone.EndX);

        private static IReadOnlyList<HeaderBlockInstance> Posts(HeaderRunPlan plan)
            => plan.Flatten().Instances.Where(instance => instance.Role == HeaderBlockRole.Post).ToList();

        private static double LengthOf(HeaderBlockInstance instance)
            => instance.DynamicParameters.TryGetValue("LONGITUD", out var value) ? Convert.ToDouble(value) : 0.0;

        /// <summary>El poste (derivado o de cabecera) que el plan dibuja mas cerca de una X.</summary>
        private static double PostLengthNear(HeaderRunPlan plan, double x)
            => Posts(plan)
                .Where(instance => Math.Abs(instance.Insertion.X - x) <= 6.0)
                .Select(LengthOf)
                .DefaultIfEmpty(0.0)
                .Max();

        private static IReadOnlyList<(double Length, int Quantity)> Components(PushBackSystem system, string category)
            => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => string.Equals(component.Category, category, StringComparison.Ordinal))
                .Select(component => (component.Length, component.Quantity))
                .OrderBy(entry => entry.Length)
                .ToList();

        // ---------------------------------------------------------------- H2: postes derivados en el BOM

        [Fact]
        public void HeaderDerivedPosts_BomUsesPostHeightAtItsDepth()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var offsets = structure.GetDerivedPostOffsets();
            var lines = structure.Fronts.Count + 1;

            Assert.NotEmpty(offsets);
            var expected = new Dictionary<double, int>();
            foreach (var offset in offsets)
            {
                for (var post = 0; post < lines; post++)
                {
                    var height = DynamicFrontGeometry.DerivedPostHeightAtPost(
                        structure, post, DynamicFrontGeometry.PostHeightAt(structure, post, offset));
                    expected[height] = expected.TryGetValue(height, out var current) ? current + 1 : 1;
                }
            }

            // Dos alturas distintas: una por zona. El BOM las compra por separado.
            Assert.Equal(2, expected.Count);
            Assert.Equal(
                expected.OrderBy(entry => entry.Key).Select(entry => (entry.Key, entry.Value)).ToList(),
                Components(system, DerivedPost));
        }

        [Fact]
        public void HeaderDerivedPosts_AsymmetricABDoNotUseGlobalEnvelope()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var global = DynamicFrontGeometry.PostHeight(structure, 0);
            var zoneB = ZoneOf(system, PushBackSide.B);
            var offsetB = structure.GetDerivedPostOffsets()
                .Single(offset => offset > zoneB.StartX && offset < zoneB.EndX);

            var atB = DynamicFrontGeometry.PostHeightAt(structure, 0, offsetB);
            Assert.True(atB < global, "la zona B es mas baja que la envolvente aplanada de la linea");

            // Y el BOM compra esa altura, no la global.
            Assert.Contains(Components(system, DerivedPost), entry => Math.Abs(entry.Length - atB) < 1e-6);
        }

        [Fact]
        public void HeaderDerivedPosts_BomLengthMatchesCutPostLength()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var zoneB = ZoneOf(system, PushBackSide.B);
            var offsetB = structure.GetDerivedPostOffsets()
                .Single(offset => offset > zoneB.StartX && offset < zoneB.EndX);

            for (var post = 0; post <= structure.Fronts.Count; post++)
            {
                var drawn = PostLengthNear(
                    new PushBackSystemLateralBuilder().Build(system, Catalog, post), offsetB);
                var bought = DynamicFrontGeometry.DerivedPostHeightAtPost(
                    structure, post, DynamicFrontGeometry.PostHeightAt(structure, post, offsetB));

                Assert.Equal(bought, drawn, 6);
                Assert.Contains(Components(system, DerivedPost), entry => Math.Abs(entry.Length - drawn) < 1e-6);
            }
        }

        // ---------------------------------------------------------------- H3: el lateral general

        [Fact]
        public void GeneralLateral_UsesTheFinalResolvedHeightOfItsDepthZone()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var zoneA = ZoneOf(system, PushBackSide.A);
            var zoneB = ZoneOf(system, PushBackSide.B);
            var offsets = structure.GetDerivedPostOffsets();
            var offsetA = offsets.Single(offset => offset > zoneA.StartX && offset < zoneA.EndX);
            var offsetB = offsets.Single(offset => offset > zoneB.StartX && offset < zoneB.EndX);

            var general = new PushBackSystemLateralBuilder().Build(system, Catalog);

            Assert.Equal(DynamicFrontGeometry.HeightAt(structure, offsetA), PostLengthNear(general, offsetA), 6);
            Assert.Equal(DynamicFrontGeometry.HeightAt(structure, offsetB), PostLengthNear(general, offsetB), 6);
            Assert.True(
                PostLengthNear(general, offsetB) < PostLengthNear(general, offsetA),
                "la mitad baja del rack no puede heredar la altura de la mitad alta");
        }

        [Fact]
        public void GeneralLateral_AgreesWithTheSectionedCuts()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var zoneB = ZoneOf(system, PushBackSide.B);
            var offsetB = structure.GetDerivedPostOffsets()
                .Single(offset => offset > zoneB.StartX && offset < zoneB.EndX);

            var general = PostLengthNear(new PushBackSystemLateralBuilder().Build(system, Catalog), offsetB);
            for (var post = 0; post <= structure.Fronts.Count; post++)
            {
                var sectioned = PostLengthNear(
                    new PushBackSystemLateralBuilder().Build(system, Catalog, post), offsetB);
                Assert.Equal(sectioned, general, 6);
            }
        }

        // ---------------------------------------------------------------- paridad de las cuatro salidas

        [Fact]
        public void HeaderPhysicalLineHeight_IsSharedByCutLateralAndBom()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var zones = new[] { ZoneOf(system, PushBackSide.A), ZoneOf(system, PushBackSide.B) };
            var bom = Components(system, DerivedPost);

            foreach (var zone in zones)
            {
                var x = MidOf(zone);
                var offset = structure.GetDerivedPostOffsets()
                    .Single(candidate => candidate > zone.StartX && candidate < zone.EndX);

                for (var post = 0; post <= structure.Fronts.Count; post++)
                {
                    // LA AUTORIDAD: la altura de ESA linea en ESA profundidad.
                    var expected = DynamicFrontGeometry.PostHeightAt(structure, post, x);
                    Assert.True(expected > 0.0);

                    // El CORTE lateral de esa linea la dibuja.
                    var cut = new PushBackSystemLateralBuilder().Build(system, Catalog, post);
                    Assert.Equal(expected, PostLengthNear(cut, offset), 6);

                    // El BOM la compra.
                    Assert.Contains(bom, entry => Math.Abs(entry.Length - expected) < 1e-6);
                }

                // Y el lateral GENERAL, que no representa una linea, dibuja la envolvente de esa misma zona.
                Assert.Equal(
                    DynamicFrontGeometry.HeightAt(structure, x),
                    PostLengthNear(new PushBackSystemLateralBuilder().Build(system, Catalog), offset),
                    6);
            }
        }

        [Fact]
        public void HeaderSeparators_StillUsePostHeightAtItsDepth()
        {
            // A1B-1 no se reabre: los separadores siguen contandose con la altura de su zona.
            var system = Asymmetric();
            var structure = system.Structure;
            var expected = 0;
            for (var post = 0; post <= structure.Fronts.Count; post++)
            {
                if (!DynamicFrontActivation.BoundaryExists(structure, post))
                {
                    continue;
                }

                foreach (var module in structure.Modules.Where(module =>
                             module.Kind == DynamicRackModuleKind.Separator))
                {
                    if (!DynamicDepthGeometry.CoverageAtPost(structure, post).Contains(module.Index + 1))
                    {
                        continue;
                    }

                    expected += DynamicSeparatorGeometry.Levels(
                        structure,
                        Catalog,
                        DynamicFrontGeometry.PostHeightAt(
                            structure, post, 0.5 * (module.StartX + module.EndX))).Count;
                }
            }

            Assert.Equal(expected, Components(system, Separator).Sum(entry => entry.Quantity));
        }

        // ---------------------------------------------------------------- A y B, por ZONA

        [Fact]
        public void AB_HeightsStayIndependentWithinTheSameLine()
        {
            var system = Asymmetric();
            var structure = system.Structure;
            var zoneA = ZoneOf(system, PushBackSide.A);
            var zoneB = ZoneOf(system, PushBackSide.B);

            for (var post = 0; post <= structure.Fronts.Count; post++)
            {
                var atA = DynamicFrontGeometry.PostHeightAt(structure, post, MidOf(zoneA));
                var atB = DynamicFrontGeometry.PostHeightAt(structure, post, MidOf(zoneB));
                Assert.True(atB < atA, "subir el lado A no puede alargar la mitad de B");
            }
        }

        // ---------------------------------------------------------------- legado sin zonas

        [Fact]
        public void HeaderDerivedPosts_DynamicWithoutZonesPreservesLegacyHeight()
        {
            // Un Push Back de un solo sentido no declara zonas: PostHeightAt degrada a PostHeight y HeightAt a la
            // altura del rack, asi que el BOM y el lateral responden exactamente lo de siempre.
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new RackCad.Domain.Systems.Shared.PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0,
                },
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6 });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 6 });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 6 });

            var system = new PushBackResolver(Catalog).Resolve(design);
            var structure = system.Structure;

            Assert.Empty(structure.HeaderHeightZones);
            Assert.Equal(DynamicFrontGeometry.Height(structure), DynamicFrontGeometry.HeightAt(structure, 0.0), 6);
            for (var post = 0; post <= structure.Fronts.Count; post++)
            {
                Assert.Equal(
                    DynamicFrontGeometry.PostHeight(structure, post),
                    DynamicFrontGeometry.PostHeightAt(structure, post, 12.0),
                    6);
            }

            // Y todos sus postes derivados se compran con una sola altura, la de siempre.
            var derived = Components(system, DerivedPost);
            Assert.True(derived.Count <= 1);
        }
    }
}
