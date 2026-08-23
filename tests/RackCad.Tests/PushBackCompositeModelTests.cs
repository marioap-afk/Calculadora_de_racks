using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G1) — el MODELO neutral A/B: configuracion por lado, topologia y sentido por celda, copia profunda y
    /// persistencia aditiva. Aqui no hay geometria: solo se fija que el modelo existe, que es independiente por lado
    /// y que un rack anterior a I-42 no cambia ni un byte por su existencia.
    /// </summary>
    public class PushBackCompositeModelTests
    {
        private static PushBackDesign Legacy()
            => new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 3
                }
            };

        private static PushBackSideDesign SideB(int slots = 2)
        {
            var side = new PushBackSideDesign { IsPresent = true, LoadLevels = 2, FirstLevelHeight = 5.0 };
            for (var slot = 0; slot < slots; slot++)
            {
                side.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1
                });
                side.FrontConfigs.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            }

            return side;
        }

        // ---- Legacy: la existencia del modelo no cambia nada ------------------------------------------------

        [Fact]
        public void ALegacyDesign_IsNotComposite_AndWritesNoCompositeField()
        {
            var design = Legacy();

            Assert.False(design.IsComposite);
            Assert.Null(design.SideB);
            Assert.Null(design.Composite);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            Assert.DoesNotContain("\"SideB\":{", json);
            Assert.DoesNotContain("\"Composite\":{", json);
        }

        [Fact]
        public void ASideDeclaredAbsent_IsNotComposite()
        {
            var design = Legacy();
            design.SideB = SideB();
            design.SideB.IsPresent = false;

            Assert.False(design.IsComposite);
        }

        // ---- El lado B es una configuracion INDEPENDIENTE ---------------------------------------------------

        [Fact]
        public void EachSide_KeepsItsOwnLevelsAndDepths()
        {
            var design = Legacy();
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 8 });
            design.SideB = SideB(slots: 1);
            design.SideB.Fronts[0].LoadLevels = 2;
            design.SideB.Fronts[0].PalletsDeep = 5;

            Assert.True(design.IsComposite);
            Assert.Equal(3, design.Structure.Fronts[0].LoadLevels);
            Assert.Equal(8, design.Structure.Fronts[0].PalletsDeep);
            Assert.Equal(2, design.SideB.Fronts[0].LoadLevels);
            Assert.Equal(5, design.SideB.Fronts[0].PalletsDeep);
        }

        [Fact]
        public void ASlotAbsentInOneSide_IsANullEntry_NotAZeroedFront()
        {
            var side = new PushBackSideDesign();
            side.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            side.Fronts.Add(null);   // A=1, B=2: la segunda ranura no existe en este lado

            Assert.Equal(2, side.Fronts.Count);
            Assert.Null(side.Front(1));
            Assert.NotNull(side.Front(0));
        }

        [Fact]
        public void DeepCopy_OfASide_IsIndependent()
        {
            var side = SideB(slots: 2);
            side.Fronts[1] = null;
            side.RearTope.Disable(0, 1);

            var copy = side.DeepCopy();
            copy.Fronts[0].PalletsDeep = 9;
            copy.FrontConfigs[0].DefaultPalletsDeep = 9;
            copy.RearTope.Disable(0, 0);

            Assert.Equal(4, side.Fronts[0].PalletsDeep);
            Assert.Equal(4, side.FrontConfigs[0].DefaultPalletsDeep);
            Assert.True(side.RearTope.At(0, 0));
            Assert.False(copy.RearTope.At(0, 0));
            Assert.Null(copy.Fronts[1]);
        }

        // ---- Topologia y sentido por celda ------------------------------------------------------------------

        [Fact]
        public void TopologyIsPerCell_AndCanDifferBetweenLevelsOfTheSameFront()
        {
            var composite = new PushBackCompositeDesign { DefaultTopology = PushBackCellTopology.Encontradas };
            composite.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            composite.SetCell(0, 2, PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            composite.SetCell(0, 3, PushBackCellTopology.SoloB, PushBackRunDirection.AToB);

            Assert.Equal(PushBackCellTopology.Corrida, composite.TopologyAt(0, 0));
            Assert.Equal(PushBackCellTopology.Encontradas, composite.TopologyAt(0, 1));   // heredada
            Assert.Equal(PushBackCellTopology.SoloA, composite.TopologyAt(0, 2));
            Assert.Equal(PushBackCellTopology.SoloB, composite.TopologyAt(0, 3));
        }

        [Fact]
        public void WritingTheDefault_RemovesTheStoredCell()
        {
            var composite = new PushBackCompositeDesign { DefaultTopology = PushBackCellTopology.Encontradas };
            composite.SetCell(1, 1, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            Assert.Single(composite.Topologies);

            composite.SetCell(1, 1, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            Assert.Empty(composite.Topologies);
        }

        [Fact]
        public void TheRunDirection_IsStoredPerCell()
        {
            var composite = new PushBackCompositeDesign();
            composite.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            composite.SetCell(0, 1, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);

            Assert.Equal(PushBackRunDirection.BToA, composite.DirectionAt(0, 0));
            Assert.Equal(PushBackRunDirection.AToB, composite.DirectionAt(0, 1));
        }

        // ---- Persistencia aditiva ---------------------------------------------------------------------------

        [Fact]
        public void ACompositeDesign_RoundTripsThroughTheDocument()
        {
            var design = Legacy();
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 3, PalletsDeep = 8, DepthStartPosition = 1
            });
            design.SideB = SideB(slots: 2);
            design.SideB.Fronts[1] = null;
            design.SideB.FrontConfigs[0].PalletsDeepOverrides.Add(3);
            design.SideB.FrontConfigs[0].DrawPallets.Add(true);
            design.SideB.RearTope.Disable(0, 1);
            design.Composite = new PushBackCompositeDesign
            {
                Gap = 12.5,
                CentralSeparator = true,
                StructureOverrideA = 9,
                StructureOverrideB = 6,
                DefaultTopology = PushBackCellTopology.Encontradas
            };
            design.Composite.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var restored = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();

            Assert.True(restored.IsComposite);
            Assert.Equal(2, restored.SideB.Fronts.Count);
            Assert.Null(restored.SideB.Fronts[1]);
            Assert.Equal(2, restored.SideB.LoadLevels);
            Assert.Equal(5.0, restored.SideB.FirstLevelHeight, 6);
            Assert.Equal(3, restored.SideB.FrontConfigs[0].PalletsDeepOverrides[0]);
            Assert.True(restored.SideB.FrontConfigs[0].DrawPallets[0]);
            Assert.False(restored.SideB.RearTope.At(0, 1));
            Assert.Equal(12.5, restored.Composite.Gap, 6);
            Assert.True(restored.Composite.CentralSeparator);
            Assert.Equal(9, restored.Composite.StructureOverrideA);
            Assert.Equal(6, restored.Composite.StructureOverrideB);
            Assert.Equal(PushBackCellTopology.Corrida, restored.Composite.TopologyAt(0, 0));
            Assert.Equal(PushBackRunDirection.BToA, restored.Composite.DirectionAt(0, 0));
        }

        [Fact]
        public void ACompositeDocument_IsIdempotentFromTheSecondWrite()
        {
            var design = Legacy();
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 3, PalletsDeep = 8, DepthStartPosition = 1
            });
            design.SideB = SideB(slots: 1);
            design.SideB.Fronts[0].DepthStartPosition = 1;
            design.Composite = new PushBackCompositeDesign { Gap = 6.0 };

            var first = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(first);
            var second = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(reloaded.ToDomain(), reloaded));

            Assert.Equal(first, second);
        }

        [Fact]
        public void AnUnknownTopologyName_FallsBackInsteadOfAbortingTheLoad()
        {
            var document = new PushBackCompositeDocument
            {
                DefaultTopology = "UnaTopologiaDelFuturo",
                Topologies = new List<PushBackTopologyCellDocument>
                {
                    new PushBackTopologyCellDocument { Frente = 0, Level = 0, Topology = "Tampoco", Direction = "Ni" }
                }
            };

            var composite = document.ToDomain();

            Assert.Equal(PushBackCellTopology.Encontradas, composite.DefaultTopology);
            Assert.Equal(PushBackCellTopology.Encontradas, composite.TopologyAt(0, 0));
            Assert.Equal(PushBackRunDirection.AToB, composite.DirectionAt(0, 0));
        }

        [Fact]
        public void UnknownJsonFields_SurviveACompositeRoundTrip()
        {
            var design = Legacy();
            design.SideB = SideB(slots: 1);
            design.Composite = new PushBackCompositeDesign { Gap = 4.0 };

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var withExtra = json.Insert(1, "\"UnCampoDelFuturo\":{\"x\":1},");

            var loaded = JsonSerializer.Deserialize<PushBackDesignDocument>(withExtra);
            var rewritten = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(loaded.ToDomain(), loaded));

            Assert.Contains("UnCampoDelFuturo", rewritten);
        }

        [Fact]
        public void TheResolvedSideAccessors_AnswerTheLegacyAuthorityForSideA()
        {
            var system = new PushBackSystem();
            system.HighEndBeams.Add(new PushBackResolvedFront { DefaultPalletsDeep = 4 });
            system.HighEndBeams[0].HighEndBeamPeraltes.Add(4.5);
            system.HighEndBeams[0].PalletsDeep.Add(4);
            system.HighEndBeams[0].DrawPallets.Add(true);

            // El lado A responde LA MISMA lista, no una copia: no puede existir una segunda autoridad.
            Assert.Same(system.HighEndBeams, system.ResolvedFronts(PushBackSide.A));
            Assert.Same(system.RearTope, system.RearTopeOf(PushBackSide.A));
            Assert.Equal(4, system.EffectivePalletsDeepAt(PushBackSide.A, 0, 0));
            Assert.True(system.DrawPalletAt(PushBackSide.A, 0, 0));
            Assert.Equal(4.5, system.HighEndBeamPeralteAt(PushBackSide.A, 0, 0), 6);
            Assert.False(system.IsComposite);
            Assert.Empty(system.ResolvedFronts(PushBackSide.B));
        }
    }
}
