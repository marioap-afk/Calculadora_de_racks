using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (S1F, contrato del dueño) — LA IDENTIDAD FISICA DE UNA BOTA: <b>lado × cara × linea de postes</b>.
    ///
    /// <para>
    /// Un Push Back compuesto tiene CUATRO caras identificables por linea: el exterior de A, el interior de A, el
    /// interior de B y el exterior de B. Las dos interiores pertenecen a lados distintos —A termina en su linea y B
    /// empieza en la suya—, asi que <b>Posterior(A) ≠ Posterior(B)</b> y <b>Posterior(A) ≠ Entrada/Salida(B)</b>.
    /// </para>
    /// <para>
    /// <b>Lo que S1F corrige.</b> S1E convertia (lado, cara) en un unico eje global cercano/lejano antes de tener
    /// geometria. Medido: con «A = Posterior» la bota salia en X=792.39 —el pasillo de B— y con «B = Posterior» en
    /// X=-0.39 —el de A—; con «Ambas» en los dos lados solo habia DOS piezas por linea en vez de cuatro, porque las
    /// dos caras interiores no tenian ancla. La identidad se colapsaba por reflexion y por coordenada.
    /// </para>
    /// <para>
    /// Con hueco CERO las dos caras interiores se tocan y siguen siendo dos piezas: el LADO rompe el empate de
    /// coordenadas, que es la regla que I-42 ya cerro para la interfaz.
    /// </para>
    /// </summary>
    public class PushBackBootIdentityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        /// <summary>Un compuesto completo con el hueco pedido: 3 ranuras, los dos lados con almacenamiento.</summary>
        private static PushBackSystem Resolve(double gap, BootPlacement? sideA, BootPlacement? sideB)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(3);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(3);
            for (var slot = 0; slot < 3; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetGap(gap);

            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            design.Structure.SafetySelections.Clear();
            var selection = new SelectiveSafetySelection
            {
                ElementId = BootId,
                Quantity = 1,
                BootSidesDeclared = true,
            };
            selection.Bota.Placement = sideA;
            selection.BotaB.Placement = sideB;
            design.Structure.SafetySelections.Add(selection);
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<ResolvedBoot> Physical(PushBackSystem system)
            => PushBackBootPlan.Resolve(system, Catalog);

        private static IReadOnlyList<HeaderBlockInstance> Boots(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, BootId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, BootId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private static int Bom(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, BootId, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);

        private static double ExteriorA(PushBackSystem system) => 0.0;

        private static double ExteriorB(PushBackSystem system) => system.Structure.TotalLength;

        private static double InteriorA(PushBackSystem system) => system.Structure.InteriorFaceStartX.Value;

        private static double InteriorB(PushBackSystem system) => system.Structure.InteriorFaceEndX.Value;

        // ==================================================================== §21 la identidad

        [Fact]
        public void BootIdentity_IncludesSide()
        {
            var a = Physical(Resolve(0.0, BootPlacement.Rear, BootPlacement.None)).First();
            var b = Physical(Resolve(0.0, BootPlacement.None, BootPlacement.Rear)).First();

            Assert.Equal(PushBackSide.A, a.Side);
            Assert.Equal(PushBackSide.B, b.Side);
            Assert.NotEqual(a.Identity, b.Identity);
        }

        [Fact]
        public void BootIdentity_IncludesSemanticFace()
        {
            var entry = Physical(Resolve(0.0, BootPlacement.EntryExit, BootPlacement.None)).First();
            var rear = Physical(Resolve(0.0, BootPlacement.Rear, BootPlacement.None)).First();

            Assert.Equal(BootFace.EntryExit, entry.Face);
            Assert.Equal(BootFace.Rear, rear.Face);
            Assert.NotEqual(entry.Identity, rear.Identity);
        }

        [Fact]
        public void BootIdentity_IncludesPhysicalPostOrLine()
        {
            var boots = Physical(Resolve(0.0, BootPlacement.EntryExit, BootPlacement.None));

            Assert.True(boots.Count > 1);
            Assert.Equal(boots.Count, boots.Select(boot => boot.PostIndex).Distinct().Count());
            Assert.Equal(boots.Count, boots.Select(boot => boot.Identity).Distinct().Count());
        }

        /// <summary>EL ERROR DE S1E, en una sola linea: la posterior de A no es la entrada de B.</summary>
        [Fact]
        public void SideARear_IsNotSideBEntryExit()
        {
            var system = Resolve(0.0, BootPlacement.Rear, BootPlacement.EntryExit);
            var rearA = Physical(system).Single(boot => boot.Side == PushBackSide.A && boot.PostIndex == 1);
            var entryB = Physical(system).Single(boot => boot.Side == PushBackSide.B && boot.PostIndex == 1);

            Assert.NotEqual(rearA.Identity, entryB.Identity);
            Assert.NotEqual(rearA.FaceX, entryB.FaceX);
            Assert.Equal(InteriorA(system), rearA.FaceX, 3);
            Assert.Equal(ExteriorB(system), entryB.FaceX, 3);
        }

        [Fact]
        public void SideARear_IsNotSideBRear()
        {
            var system = Resolve(0.0, BootPlacement.Rear, BootPlacement.Rear);
            var rearA = Physical(system).Single(boot => boot.Side == PushBackSide.A && boot.PostIndex == 1);
            var rearB = Physical(system).Single(boot => boot.Side == PushBackSide.B && boot.PostIndex == 1);

            Assert.NotEqual(rearA.Identity, rearB.Identity);
            Assert.NotEqual(rearA.PlantaAt.X, rearB.PlantaAt.X);   // ni siquiera con el hueco a cero
        }

        [Fact]
        public void SideBRear_IsNotSideBEntryExit()
        {
            var system = Resolve(0.0, BootPlacement.None, BootPlacement.Both);
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Equal(2, line.Count);
            Assert.All(line, boot => Assert.Equal(PushBackSide.B, boot.Side));
            Assert.Equal(2, line.Select(boot => boot.Face).Distinct().Count());
            Assert.Equal(2, line.Select(boot => Math.Round(boot.FaceX, 3)).Distinct().Count());
        }

        [Fact]
        public void SideAEntry_IsNotSideARear()
        {
            var system = Resolve(0.0, BootPlacement.Both, BootPlacement.None);
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Equal(2, line.Count);
            Assert.All(line, boot => Assert.Equal(PushBackSide.A, boot.Side));
            Assert.Equal(ExteriorA(system), line.Single(b => b.Face == BootFace.EntryExit).FaceX, 3);
            Assert.Equal(InteriorA(system), line.Single(b => b.Face == BootFace.Rear).FaceX, 3);
        }

        // ==================================================================== §22 las cuatro caras

        [Fact]
        public void SideAEntry_ResolvesExteriorA()
        {
            var system = Resolve(12.0, BootPlacement.EntryExit, BootPlacement.None);

            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot =>
            {
                Assert.Equal(PushBackSide.A, boot.Side);
                Assert.Equal(BootFace.EntryExit, boot.Face);
                Assert.Equal(ExteriorA(system), boot.FaceX, 3);
            });
        }

        [Fact]
        public void SideARear_ResolvesInteriorA()
        {
            var system = Resolve(12.0, BootPlacement.Rear, BootPlacement.None);

            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot => Assert.Equal(InteriorA(system), boot.FaceX, 3));
        }

        [Fact]
        public void SideBRear_ResolvesInteriorB()
        {
            var system = Resolve(12.0, BootPlacement.None, BootPlacement.Rear);

            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot => Assert.Equal(InteriorB(system), boot.FaceX, 3));
        }

        [Fact]
        public void SideBEntry_ResolvesExteriorB()
        {
            var system = Resolve(12.0, BootPlacement.None, BootPlacement.EntryExit);

            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot => Assert.Equal(ExteriorB(system), boot.FaceX, 3));
        }

        /// <summary>Las cuatro, en la misma linea: cuatro identidades y cuatro planos.</summary>
        [Fact]
        public void FourBootFaces_HaveCorrectSemanticIdentity()
        {
            var system = Resolve(12.0, BootPlacement.Both, BootPlacement.Both);
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Equal(4, line.Count);
            Assert.Equal(4, line.Select(boot => boot.Identity).Distinct().Count());
            Assert.Equal(ExteriorA(system), Face(line, PushBackSide.A, BootFace.EntryExit), 3);
            Assert.Equal(InteriorA(system), Face(line, PushBackSide.A, BootFace.Rear), 3);
            Assert.Equal(InteriorB(system), Face(line, PushBackSide.B, BootFace.Rear), 3);
            Assert.Equal(ExteriorB(system), Face(line, PushBackSide.B, BootFace.EntryExit), 3);
        }

        /// <summary>Y ninguna se pierde por la reflexion del lado B: cuatro piezas dibujadas, cuatro contadas.</summary>
        [Fact]
        public void FourBootFaces_DoNotCollapseThroughReflection()
        {
            foreach (var gap in new[] { 0.0, 12.0 })
            {
                var system = Resolve(gap, BootPlacement.Both, BootPlacement.Both);
                var lines = Physical(system).Select(boot => boot.PostIndex).Distinct().Count();

                Assert.Equal(4 * lines, Physical(system).Count);
                Assert.Equal(Physical(system).Count, Boots(system).Count);
                Assert.Equal(Physical(system).Count, Bom(system));
            }
        }

        private static double Face(IEnumerable<ResolvedBoot> line, PushBackSide side, BootFace face)
            => line.Single(boot => boot.Side == side && boot.Face == face).FaceX;

        // ==================================================================== §23 el hueco

        [Fact]
        public void GapPositive_RearAAndRearBAreDistinct()
        {
            var system = Resolve(12.0, BootPlacement.Rear, BootPlacement.Rear);
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Equal(2, line.Count);
            Assert.Equal(12.0, Face(line, PushBackSide.B, BootFace.Rear) - Face(line, PushBackSide.A, BootFace.Rear), 3);
        }

        [Fact]
        public void GapZero_RearAAndRearBRemainDistinctBySide()
        {
            var system = Resolve(0.0, BootPlacement.Rear, BootPlacement.Rear);
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Equal(2, line.Count);
            Assert.Equal(
                Face(line, PushBackSide.A, BootFace.Rear),
                Face(line, PushBackSide.B, BootFace.Rear),
                3);   // el MISMO plano…
            Assert.Equal(2, line.Select(boot => boot.Side).Distinct().Count());   // …y dos piezas, una por lado
            Assert.Equal(2, line.Select(boot => boot.Identity).Distinct().Count());
        }

        [Fact]
        public void GapZero_CoincidentProjectionDoesNotDeduplicateBoots()
        {
            var system = Resolve(0.0, BootPlacement.Rear, BootPlacement.Rear);

            Assert.Equal(Physical(system).Count, Boots(system).Count);
            Assert.Equal(Physical(system).Count, Bom(system));
            Assert.Equal(2, Physical(system).Count(boot => boot.PostIndex == 1));
        }

        [Fact]
        public void GapDoesNotChangeEntryExitSemanticFace()
        {
            foreach (var gap in new[] { 0.0, 12.0 })
            {
                var system = Resolve(gap, BootPlacement.EntryExit, BootPlacement.EntryExit);
                var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

                Assert.Equal(2, line.Count);
                Assert.All(line, boot => Assert.Equal(BootFace.EntryExit, boot.Face));
                Assert.Equal(ExteriorA(system), Face(line, PushBackSide.A, BootFace.EntryExit), 3);
                Assert.Equal(ExteriorB(system), Face(line, PushBackSide.B, BootFace.EntryExit), 3);
            }
        }

        // ==================================================================== §24 las vistas

        [Fact]
        public void SideAEntry_AppearsOnlyAtPhysicalEntryACut()
            => AssertOnlyIn(BootPlacement.EntryExit, null, PushBackSide.A, PushBackFrontalEnd.EntradaSalida);

        [Fact]
        public void SideARear_AppearsOnlyAtPhysicalRearACut()
            => AssertOnlyIn(BootPlacement.Rear, null, PushBackSide.A, PushBackFrontalEnd.Posterior);

        [Fact]
        public void SideBRear_AppearsOnlyAtPhysicalRearBCut()
            => AssertOnlyIn(null, BootPlacement.Rear, PushBackSide.B, PushBackFrontalEnd.Posterior);

        [Fact]
        public void SideBEntry_AppearsOnlyAtPhysicalEntryBCut()
            => AssertOnlyIn(null, BootPlacement.EntryExit, PushBackSide.B, PushBackFrontalEnd.EntradaSalida);

        /// <summary>La pieza sale EN SU corte y en ninguno de los otros tres.</summary>
        private static void AssertOnlyIn(
            BootPlacement? sideA, BootPlacement? sideB, PushBackSide side, PushBackFrontalEnd end)
        {
            var system = Resolve(12.0, sideA ?? BootPlacement.None, sideB ?? BootPlacement.None);
            Assert.NotEmpty(Physical(system));

            foreach (var cutSide in new[] { PushBackSide.A, PushBackSide.B })
            {
                foreach (var cutEnd in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    var expected = cutSide == side && cutEnd == end ? Physical(system).Count : 0;
                    Assert.Equal(expected, Cut(system, cutEnd, cutSide).Count);
                }
            }
        }

        /// <summary>Un corte filtra por identidad; no reinterpreta la cara segun su marco.</summary>
        [Fact]
        public void CutFiltering_DoesNotReinterpretSemanticFace()
        {
            var system = Resolve(12.0, BootPlacement.Both, BootPlacement.Both);
            var perLine = Physical(system).Select(boot => boot.PostIndex).Distinct().Count();

            Assert.Equal(perLine, Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A).Count);
            Assert.Equal(perLine, Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A).Count);
            Assert.Equal(perLine, Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B).Count);
            Assert.Equal(perLine, Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B).Count);
        }

        /// <summary>
        /// El corte del lado B se dibuja sobre una copia ESPEJO. La reflexion transforma coordenadas y mano; la
        /// entrada de B sigue siendo la entrada de B, y aparece en su corte, no en el posterior.
        /// </summary>
        [Fact]
        public void ReflectedB_CutKeepsSemanticFace()
        {
            var system = Resolve(12.0, BootPlacement.None, BootPlacement.EntryExit);

            Assert.All(Physical(system), boot =>
            {
                Assert.Equal(PushBackSide.B, boot.Side);
                Assert.Equal(BootFace.EntryExit, boot.Face);
                Assert.True(boot.Mirrored);   // su mano, que es lo unico que la reflexion decide
            });

            Assert.Equal(Physical(system).Count, Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B).Count);
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B));
        }

        // ==================================================================== §25 el BOM

        [Fact]
        public void Bom_DoesNotMergeRearAAndRearB()
        {
            var onlyA = Resolve(0.0, BootPlacement.Rear, BootPlacement.None);
            var both = Resolve(0.0, BootPlacement.Rear, BootPlacement.Rear);

            Assert.Equal(2 * Bom(onlyA), Bom(both));
        }

        [Fact]
        public void Bom_DoesNotMergeRearAAndEntryB()
        {
            var rearA = Resolve(0.0, BootPlacement.Rear, BootPlacement.None);
            var entryB = Resolve(0.0, BootPlacement.None, BootPlacement.EntryExit);
            var both = Resolve(0.0, BootPlacement.Rear, BootPlacement.EntryExit);

            Assert.Equal(Bom(rearA) + Bom(entryB), Bom(both));
        }

        [Fact]
        public void Bom_DistinguishesSidePhysicalIdentity()
        {
            var system = Resolve(0.0, BootPlacement.Both, BootPlacement.Both);
            var lines = Physical(system).Select(boot => boot.PostIndex).Distinct().Count();

            Assert.Equal(4 * lines, Bom(system));
        }

        [Fact]
        public void BootPlan_Draw_Bom_SamePhysicalIdentitySet()
        {
            foreach (var gap in new[] { 0.0, 12.0 })
            {
                foreach (var (a, b) in new[]
                         {
                             (BootPlacement.EntryExit, BootPlacement.None),
                             (BootPlacement.Rear, BootPlacement.None),
                             (BootPlacement.None, BootPlacement.Rear),
                             (BootPlacement.None, BootPlacement.EntryExit),
                             (BootPlacement.Both, BootPlacement.Both),
                         })
                {
                    var system = Resolve(gap, a, b);
                    var resolved = Physical(system)
                        .Select(boot => FormattableString.Invariant(
                            $"{boot.PlantaAt.X:0.###}|{boot.PlantaAt.Y:0.###}|{boot.Mirrored}"))
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToList();
                    var drawn = Boots(system)
                        .Select(instance => FormattableString.Invariant(
                            $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}"))
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToList();

                    Assert.Equal(resolved, drawn);
                    Assert.Equal(resolved.Count, Bom(system));
                }
            }
        }

        [Fact]
        public void GapZero_BomRetainsDistinctPhysicalBoots()
        {
            var system = Resolve(0.0, BootPlacement.Both, BootPlacement.Both);

            Assert.Equal(Physical(system).Count, Bom(system));
            Assert.Equal(4, Physical(system).Count(boot => boot.PostIndex == 1));
        }

        // ==================================================================== §26 bites

        /// <summary>
        /// BITE A — SIN EL LADO en la identidad, las dos caras interiores de un rack con hueco cero serian la misma:
        /// mismo plano, misma linea. Es el lado lo que rompe el empate.
        /// </summary>
        [Fact]
        public void Bite_IdentityWithoutSide_CollapsesGapZeroFaces()
        {
            var system = Resolve(0.0, BootPlacement.Rear, BootPlacement.Rear);
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Single(line.Select(boot => (boot.Face, X: Math.Round(boot.FaceX, 3))).Distinct());
            Assert.Equal(2, line.Select(boot => boot.Identity).Distinct().Count());
        }

        /// <summary>
        /// BITE B — DEDUCIR LA CARA de un eje global cercano/lejano deja dos de las cuatro sin sitio: es lo que
        /// hacia S1E, y por eso «A = Posterior» acababa en el pasillo de B.
        /// </summary>
        [Fact]
        public void Bite_InferringFaceFromGlobalNearFar_BreaksTheFourFaces()
        {
            var system = Resolve(12.0, BootPlacement.Both, BootPlacement.Both);
            var planes = Physical(system)
                .Where(boot => boot.PostIndex == 1)
                .Select(boot => Math.Round(boot.FaceX, 3))
                .Distinct()
                .ToList();

            Assert.Equal(4, planes.Count);   // cuatro planos, no dos
            Assert.Contains(Math.Round(InteriorA(system), 3), planes);
            Assert.Contains(Math.Round(InteriorB(system), 3), planes);
        }

        /// <summary>
        /// BITE C — DEDUPLICAR POR ANCLA perderia una pieza en cuanto dos lados proyectan igual. Ni siquiera con el
        /// hueco a cero comparten ancla, y aunque la compartieran seguirian siendo dos.
        /// </summary>
        [Fact]
        public void Bite_DedupByAnchor_LosesACoincidentBoot()
        {
            var system = Resolve(0.0, BootPlacement.Rear, BootPlacement.Rear);
            var anchors = Boots(system)
                .Select(instance => Math.Round(instance.Insertion.X, 3) + "|" + Math.Round(instance.Insertion.Y, 3))
                .ToList();

            Assert.Equal(anchors.Count, anchors.Distinct().Count());
            Assert.Equal(Physical(system).Count, anchors.Count);
        }

        /// <summary>
        /// BITE D — REINTERPRETAR EL LADO B despues de la reflexion cambiaria su cara: su entrada pasaria a leerse
        /// como una posterior y saldria en el corte equivocado.
        /// </summary>
        [Fact]
        public void Bite_ReinterpretingBAfterReflection_BreaksItsFace()
        {
            var system = Resolve(12.0, BootPlacement.None, BootPlacement.EntryExit);

            Assert.All(Physical(system), boot => Assert.Equal(BootFace.EntryExit, boot.Face));
            Assert.NotEmpty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B));
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A));
        }
    }
}
