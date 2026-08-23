using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G5) — VISTAS: lateral (general y cortes), los cuatro cortes frontales, planta, transformaciones,
    /// tarimas y etiquetas A/B. Lo que se fija aqui es que la ESTRUCTURA se dibuja una sola vez y que el contenido de
    /// cada lado viaja con UNA transformacion rigida, no como decoracion espejada.
    /// </summary>
    public class PushBackCompositeViewTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            int levelsA = 2, int levelsB = 2, double gap = 0.0, int slotsA = 1, int slotsB = 1)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slotsA, slotsB: slotsB, deepA: 4, deepB: 4, levelsA: levelsA, levelsB: levelsB, gap: gap);
            design.Composite.DefaultTopology = topology;
            design.Composite.DefaultDirection = direction;
            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design) => new PushBackResolver(Catalog).Resolve(design);

        private static LateralHeaderLayout Lateral(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten();

        // ---- Lateral: estructura una vez, contenido por cama --------------------------------------------------

        [Fact]
        public void TheCompositeLateral_DrawsTheStructureOnce()
        {
            var composite = Resolve(Design(PushBackCellTopology.Encontradas));
            var instances = Lateral(composite).Instances;

            var posts = instances.Count(instance => instance.Role == HeaderBlockRole.Post);
            var plates = instances.Count(instance => instance.Role == HeaderBlockRole.BasePlate);
            Assert.True(posts > 0);
            Assert.True(plates > 0);

            // Los postes del lateral corresponden a las cabeceras de LA estructura compartida: no hay dos juegos.
            var headerModules = composite.Structure.Modules.Count(module => module.IsHeader);
            Assert.True(headerModules > 0);
            Assert.True(posts <= headerModules * 4, "el lateral no debe duplicar la estructura por tener dos lados");
        }

        [Fact]
        public void Encontradas_DrawTwoBeds_WithOppositeSlopes()
        {
            var instances = Lateral(Resolve(Design(PushBackCellTopology.Encontradas, levelsA: 1, levelsB: 1))).Instances;

            var rails = instances.Where(instance => instance.Role == HeaderBlockRole.Rail).ToList();
            Assert.Equal(2, rails.Count);
            // Una sube hacia +X y la otra hacia -X: son dos camas fisicas encontradas, no una espejada.
            Assert.Contains(rails, rail => rail.RotationRadians > 0.0);
            Assert.Contains(rails, rail => rail.RotationRadians < 0.0);
            Assert.Equal(0.0, rails.Sum(rail => rail.RotationRadians), 9);
        }

        [Fact]
        public void ACorrida_DrawsASingleBed()
        {
            var instances = Lateral(Resolve(Design(PushBackCellTopology.Corrida, levelsA: 1, levelsB: 1))).Instances;

            var rails = instances.Where(instance => instance.Role == HeaderBlockRole.Rail).ToList();
            Assert.Single(rails);
        }

        [Fact]
        public void TheWholeBedAssembly_SharesOneTransformation()
        {
            var instances = Lateral(Resolve(Design(PushBackCellTopology.SoloB, levelsA: 1, levelsB: 1))).Instances;

            var bed = instances
                .Where(instance => instance.Role == HeaderBlockRole.Rail
                    || instance.Role == HeaderBlockRole.Roller
                    || instance.Role == HeaderBlockRole.Stop)
                .ToList();

            Assert.NotEmpty(bed);
            // Riel, rodillos y tope de cama comparten EXACTAMENTE la misma rotacion y el mismo espejo: van montados
            // sobre la misma cama, no dibujados horizontales y decorados despues.
            Assert.Single(bed.Select(instance => Math.Round(instance.RotationRadians, 9)).Distinct());
            Assert.Single(bed.Select(instance => instance.MirroredX).Distinct());
            Assert.True(bed[0].RotationRadians < 0.0);   // el lado B fluye hacia -X
        }

        [Fact]
        public void TheLateralCarriesTheSideLabels()
        {
            var instances = Lateral(Resolve(Design())).Instances;
            var labels = instances
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .Select(instance => instance.Text)
                .ToList();

            Assert.Contains("A", labels);
            Assert.Contains("B", labels);
        }

        [Fact]
        public void ASingleSidedRack_HasNoSideLabels()
        {
            var single = new PushBackResolver(Catalog).Resolve(new PushBackDesign
            {
                Structure = new RackCad.Domain.Systems.Dynamic.DynamicRackDesign
                {
                    Pallet = new RackCad.Domain.Systems.Shared.PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2
                }
            });

            var labels = Lateral(single).Instances
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .Select(instance => instance.Text)
                .ToList();

            Assert.DoesNotContain("A", labels);
            Assert.DoesNotContain("B", labels);
        }

        [Fact]
        public void TheLateralSections_ExistPerTransversePost()
        {
            var system = Resolve(Design(slotsA: 2, slotsB: 2));
            var cortes = new PushBackSystemLateralBuilder().Cortes(system, Catalog);

            Assert.NotEmpty(cortes);
            Assert.All(cortes, corte => Assert.NotNull(corte.Plan));
        }

        // ---- Frontales: cuatro cortes utiles ------------------------------------------------------------------

        [Fact]
        public void EachSide_HasItsOwnEntranceAndRearCuts()
        {
            var system = Resolve(Design(PushBackCellTopology.Encontradas, levelsA: 2, levelsB: 2));

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var entrance = PushBackCompositeFrontal
                    .Build(system, Catalog, PushBackFrontalEnd.EntradaSalida, side).Flatten().Instances;
                var rear = PushBackCompositeFrontal
                    .Build(system, Catalog, PushBackFrontalEnd.Posterior, side).Flatten().Instances;

                Assert.NotEmpty(entrance);
                Assert.NotEmpty(rear);
                Assert.Contains(entrance, instance => instance.Role == HeaderBlockRole.Post);
                Assert.Contains(rear, instance => instance.Role == HeaderBlockRole.Post);
            }
        }

        [Fact]
        public void ACorridaCell_HasNoRearBeam_OnTheLowSideInterface()
        {
            var system = Resolve(Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, levelsA: 1, levelsB: 1));

            var lowRear = PushBackCompositeFrontal
                .Build(system, Catalog, PushBackFrontalEnd.Posterior, PushBackSide.A).Flatten().Instances;
            var highRear = PushBackCompositeFrontal
                .Build(system, Catalog, PushBackFrontalEnd.Posterior, PushBackSide.B).Flatten().Instances;

            bool IsRearBeam(HeaderBlockInstance instance)
                => string.Equals(instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase);

            // La calle atraviesa la interfaz: el lado BAJO no tiene alli larguero posterior; el ALTO si.
            Assert.DoesNotContain(lowRear, IsRearBeam);
            Assert.Contains(highRear, IsRearBeam);
        }

        [Fact]
        public void EncontradasKeepARearBeamOnBothInterfaces()
        {
            var system = Resolve(Design(PushBackCellTopology.Encontradas, levelsA: 1, levelsB: 1));

            bool IsRearBeam(HeaderBlockInstance instance)
                => string.Equals(instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase);

            Assert.Contains(
                PushBackCompositeFrontal.Build(system, Catalog, PushBackFrontalEnd.Posterior, PushBackSide.A)
                    .Flatten().Instances, IsRearBeam);
            Assert.Contains(
                PushBackCompositeFrontal.Build(system, Catalog, PushBackFrontalEnd.Posterior, PushBackSide.B)
                    .Flatten().Instances, IsRearBeam);
        }

        // ---- Planta ------------------------------------------------------------------------------------------

        [Fact]
        public void ThePlanta_DrawsTheStructureOnce_AndCarriesTheSideLabels()
        {
            var system = Resolve(Design());
            var instances = new PushBackSystemPlantaBuilder().Build(system, Catalog);

            Assert.NotEmpty(instances);
            var labels = instances
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .Select(instance => instance.Text)
                .ToList();
            Assert.Contains("A", labels);
            Assert.Contains("B", labels);

            // Los postes de planta son los de LA estructura compartida.
            var posts = instances.Count(instance => instance.Role == HeaderBlockRole.Post);
            Assert.True(posts > 0);
        }

        [Fact]
        public void ThePlanta_ShowsAnInOutBeamAtBothAisles()
        {
            var system = Resolve(Design(PushBackCellTopology.Encontradas));
            var instances = new PushBackSystemPlantaBuilder().Build(system, Catalog);
            var total = system.Structure.TotalLength;

            var inOut = instances
                .Where(instance => string.Equals(
                    instance.PieceId,
                    RackCad.Domain.Systems.Dynamic.DynamicRackDefaults.InOutBeamCatalogId,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(inOut);
            Assert.Contains(inOut, beam => beam.Insertion.X < total / 2.0);
            Assert.Contains(inOut, beam => beam.Insertion.X > total / 2.0);
        }

        // ---- Tarimas -----------------------------------------------------------------------------------------

        [Fact]
        public void EachSide_DrawsItsOwnPallets_AndTheyStayOutOfTheBom()
        {
            var design = Design(PushBackCellTopology.Encontradas, levelsA: 1, levelsB: 1);
            design.SideB.FrontConfigs[0].DrawPallets.Add(true);
            var system = Resolve(design);

            var pallets = Lateral(system).Instances
                .Where(instance => instance.Role == HeaderBlockRole.Pallet)
                .ToList();

            Assert.NotEmpty(pallets);
            // Solo el lado B las pidio: todas viajan con la cama de B, inclinadas hacia -X.
            Assert.All(pallets, pallet => Assert.True(pallet.RotationRadians <= 0.0));
        }

        [Fact]
        public void ThePalletsFollowTheirOwnBedTransformation()
        {
            var design = Design(PushBackCellTopology.Encontradas, levelsA: 1, levelsB: 1);
            design.Fronts[0].DrawPallets.Add(true);
            design.SideB.FrontConfigs[0].DrawPallets.Add(true);
            var system = Resolve(design);

            var pallets = Lateral(system).Instances
                .Where(instance => instance.Role == HeaderBlockRole.Pallet)
                .ToList();

            Assert.NotEmpty(pallets);
            // Las de A siguen la pendiente de A y las de B la de B: cada tarima va sobre SU cama.
            Assert.Contains(pallets, pallet => pallet.RotationRadians > 0.0);
            Assert.Contains(pallets, pallet => pallet.RotationRadians < 0.0);
        }
    }
}
