using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D5, contrato del dueño) — CONTINUIDAD ESTRUCTURAL Y COBERTURA DE ALMACENAMIENTO SON DOS COSAS.
    ///
    /// <para>
    /// Una ranura en blanco en los DOS lados conserva su claro, su columna y la estructura que la continuidad
    /// exija —eso es correcto y no se toca—, pero no almacena nada: no crea cara de ataque, ni bota automatica, ni
    /// defensa automatica, ni posicion de tarima. Lo que la seguridad AUTOMATICA pregunta es la cobertura de
    /// ALMACENAMIENTO; lo que el rack sostiene lo sigue diciendo la cobertura estructural.
    /// </para>
    /// </summary>
    public class PushBackBlankStorageCoverageTests
    {
        private const string DefensePiece = "DEFENSA_MONTACARGAS";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>
        /// 4 ranuras x 2 niveles. <paramref name="storeA"/> y <paramref name="storeB"/> dicen en que ranuras
        /// almacena cada lado; las demas quedan EN BLANCO en ese lado, conservando su declaracion fisica.
        /// </summary>
        private static PushBackSystem Build(IEnumerable<int> storeA, IEnumerable<int> storeB)
        {
            const int levels = 2;
            const int slots = 4;
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slots, slotsB: slots, deepA: 4, deepB: 4, levelsA: levels, levelsB: levels, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;

            var a = new HashSet<int>(storeA);
            var b = new HashSet<int>(storeB);
            for (var slot = 0; slot < slots; slot++)
            {
                if (!a.Contains(slot))
                {
                    design.Composite.AbsentSlotsA.Add(slot);
                }

                if (!b.Contains(slot))
                {
                    design.Composite.AbsentSlotsB.Add(slot);
                }

                for (var level = 0; level < levels; level++)
                {
                    design.Fronts[slot].DrawPallets.Add(true);
                    design.SideB.FrontConfigs[slot]?.DrawPallets.Add(true);
                }
            }

            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = DefensePiece;
            design.SideB.DefensePieceId = DefensePiece;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>El rack de la ronda: la ranura 1 no almacena en NINGUNO de los dos lados.</summary>
        private static PushBackSystem BlankBoth(bool neighboursInA = true)
            => neighboursInA
                ? Build(new[] { 0, 2, 3 }, new[] { 3 })
                : Build(new[] { 3 }, new[] { 0, 2, 3 });

        /// <summary>Las fronteras de la ranura en blanco: las dos lineas que la delimitan.</summary>
        private static readonly int[] BlankBoundaries = { 1, 2 };

        private static SelectiveSafetySelection BootSelection(PushBackSystem system)
            => SelectiveSafetyFamilies.SelectedOfType(
                system.Structure.SafetySelections, Catalog.SafetyElements, SelectiveSafetyDefaults.BotaType);

        private static IReadOnlyList<ResolvedBoot> Boots(PushBackSystem system)
            => PushBackBootPlan.Resolve(system, Catalog);

        private static IReadOnlyList<ResolvedDefense> Defenses(PushBackSystem system)
            => PushBackDefensePlan.Resolve(system, Catalog);

        // ---------------------------------------------------------------- la prueba critica (§25)

        [Fact]
        public void BlankBoth_StructuralContinuityDoesNotBecomeStorage()
        {
            var system = BlankBoth();
            var structure = system.Structure;

            foreach (var post in BlankBoundaries)
            {
                // LA ESTRUCTURA SIGUE AHI: la linea existe y su cobertura estructural incluye el tramo del frente en
                // blanco. Nada de esto se elimina para apagar la seguridad.
                Assert.True(DynamicFrontActivation.BoundaryExists(structure, post));
                var structural = DynamicDepthGeometry.CoverageAtPost(structure, post);
                Assert.False(structural.IsEmpty);
                Assert.Contains(structural.Segments, segment => segment.StartPosition >= 8);

                // Y SIN EMBARGO no hay almacenamiento del lado B en esa linea.
                var storage = DynamicDepthGeometry.StorageCoverageAtPost(structure, post);
                Assert.DoesNotContain(storage.Segments, segment => segment.StartPosition >= 8);
                Assert.False(PushBackDefenseSides.HasFace(structure, post, PushBackSide.B));
            }
        }

        [Fact]
        public void BlankBoth_DoesNotCreateStorageCoverage()
        {
            var system = BlankBoth();
            var structure = system.Structure;
            var blank = structure.Fronts[1];

            Assert.False(blank.IsActive);
            Assert.Equal(0, DynamicFrontActivation.EffectiveLoadLevels(blank));

            foreach (var post in BlankBoundaries)
            {
                var storage = DynamicDepthGeometry.StorageCoverageAtPost(structure, post);
                var structural = DynamicDepthGeometry.CoverageAtPost(structure, post);

                // La cobertura de almacenamiento es un SUBCONJUNTO estricto de la estructural en estas lineas.
                Assert.True(storage.EndPosition < structural.EndPosition);
                foreach (var segment in storage.Segments)
                {
                    Assert.Contains(structural.Segments, other =>
                        segment.StartPosition >= other.StartPosition && segment.EndPosition <= other.EndPosition);
                }
            }
        }

        [Fact]
        public void BlankBoth_DoesNotCreateStorageDepth()
        {
            var system = BlankBoth();
            var runs = PushBackRuns.Resolve(system);

            // Ninguna cama nace de la ranura en blanco, ni de un lado ni del otro.
            Assert.DoesNotContain(runs.Runs, run => run.Slot == 1);
            Assert.Equal(0, DynamicFrontActivation.EffectiveLoadLevels(system.Structure.Fronts[1]));
        }

        [Fact]
        public void BlankBoth_DoesNotCreatePalletPosition()
        {
            var system = BlankBoth();

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                Assert.DoesNotContain(
                    PushBackPalletProjection.Resolve(system, Catalog, side, end),
                    row => row.Slot == 1);
            }
        }

        // ---------------------------------------------------------------- seguridad AUTOMATICA

        [Fact]
        public void BlankBoth_DoesNotCreateAutomaticBootFace()
        {
            var system = BlankBoth();
            var selection = BootSelection(system);
            var boots = Boots(system);

            foreach (var post in BlankBoundaries)
            {
                // El lado que no almacena ahi queda declarado EN BLANCO y su automatico no coloca nada.
                Assert.Contains(post, selection.BotaB.BlankPosts);
                Assert.DoesNotContain(boots, boot => boot.PostIndex == post && boot.Side == PushBackSide.B);

                // El lado que SI almacena conserva la suya: la ranura vecina existe.
                Assert.Contains(boots, boot => boot.PostIndex == post && boot.Side == PushBackSide.A);
            }
        }

        [Fact]
        public void BlankBoth_DoesNotCreateAutomaticDefenseFace()
        {
            var system = BlankBoth(neighboursInA: false);
            var defenses = Defenses(system);

            foreach (var post in BlankBoundaries)
            {
                Assert.DoesNotContain(defenses, defense => defense.PostLine == post && defense.Side == PushBackSide.A);
                Assert.Contains(defenses, defense => defense.PostLine == post && defense.Side == PushBackSide.B);
            }
        }

        [Fact]
        public void BlankBoth_WithActiveNeighborOnlyA_DoesNotCreateBStorageFace()
        {
            var system = BlankBoth();
            var structure = system.Structure;

            foreach (var post in BlankBoundaries)
            {
                Assert.True(PushBackDefenseSides.HasFace(structure, post, PushBackSide.A));
                Assert.False(PushBackDefenseSides.HasFace(structure, post, PushBackSide.B));
                Assert.DoesNotContain(Boots(system), boot => boot.PostIndex == post && boot.Side == PushBackSide.B);
                Assert.DoesNotContain(
                    Defenses(system), defense => defense.PostLine == post && defense.Side == PushBackSide.B);
            }
        }

        [Fact]
        public void BlankBoth_WithActiveNeighborOnlyB_DoesNotCreateAStorageFace()
        {
            var system = BlankBoth(neighboursInA: false);
            var structure = system.Structure;

            foreach (var post in BlankBoundaries)
            {
                Assert.True(PushBackDefenseSides.HasFace(structure, post, PushBackSide.B));
                Assert.False(PushBackDefenseSides.HasFace(structure, post, PushBackSide.A));
                Assert.DoesNotContain(Boots(system), boot => boot.PostIndex == post && boot.Side == PushBackSide.A);
                Assert.DoesNotContain(
                    Defenses(system), defense => defense.PostLine == post && defense.Side == PushBackSide.A);
            }
        }

        // ---------------------------------------------------------------- dos blancos seguidos

        [Fact]
        public void TwoConsecutiveBlankSlots_DoNotRequireUselessInteriorHeader()
        {
            // Ranuras 1 y 2 en blanco en los dos lados: la linea que las separa no sostiene nada.
            var system = Build(new[] { 0, 3 }, new[] { 0, 3 });
            var structure = system.Structure;

            Assert.False(DynamicFrontActivation.BoundaryExists(structure, 2));
            Assert.DoesNotContain(2, DynamicFrontActivation.PresentBoundaries(structure));
            Assert.False(PushBackDefenseSides.HasFace(structure, 2, PushBackSide.A));
            Assert.False(PushBackDefenseSides.HasFace(structure, 2, PushBackSide.B));
            Assert.DoesNotContain(Boots(system), boot => boot.PostIndex == 2);
            Assert.DoesNotContain(Defenses(system), defense => defense.PostLine == 2);

            // Y las lineas que SI sostienen algo siguen ahi: no se ha borrado estructura legitima.
            foreach (var post in new[] { 0, 1, 3, 4 })
            {
                Assert.True(DynamicFrontActivation.BoundaryExists(structure, post));
            }
        }

        // ---------------------------------------------------------------- el override explicito manda (§20)

        [Fact]
        public void BlankSide_ExplicitBootOverrideStillAppliesWhenPhysicalPostExists()
        {
            var system = BlankBoth();
            var selection = BootSelection(system);

            // El lado B esta en blanco en esas lineas, pero el POSTE FISICO existe: si el usuario pide ahi su bota,
            // se coloca. El blanco apaga el AUTOMATICO, no la intencion explicita (contrato S1).
            Assert.Contains(1, selection.BotaB.BlankPosts);
            selection.BotaB.Posts.Add(new BootPostPlacement
            {
                PostIndex = 1,
                Placement = BootPlacement.EntryExit,
            });

            var boots = PushBackBootPlan.Resolve(system, Catalog);

            Assert.Contains(boots, boot => boot.PostIndex == 1 && boot.Side == PushBackSide.B);
            Assert.DoesNotContain(boots, boot => boot.PostIndex == 2 && boot.Side == PushBackSide.B);
        }

        // ---------------------------------------------------------------- lo que NO cambia

        [Fact]
        public void AnActiveRack_KeepsEveryStorageFace()
        {
            // Sin ninguna ranura en blanco, cobertura estructural y de almacenamiento coinciden y nada cambia.
            var system = Build(new[] { 0, 1, 2, 3 }, new[] { 0, 1, 2, 3 });
            var structure = system.Structure;

            for (var post = 0; post <= structure.Fronts.Count; post++)
            {
                var structural = DynamicDepthGeometry.CoverageAtPost(structure, post);
                var storage = DynamicDepthGeometry.StorageCoverageAtPost(structure, post);
                Assert.Equal(structural.StartPosition, storage.StartPosition);
                Assert.Equal(structural.EndPosition, storage.EndPosition);
                Assert.True(PushBackDefenseSides.HasFace(structure, post, PushBackSide.A));
                Assert.True(PushBackDefenseSides.HasFace(structure, post, PushBackSide.B));
            }
        }

        [Fact]
        public void GapZero_KeepsTheTwoSidesDistinct()
        {
            // Con hueco cero las dos lineas interiores comparten X, y aun asi el almacenamiento de A no enciende la
            // cara de B: la pregunta es por LADO, no por coordenada.
            var system = BlankBoth();
            var structure = system.Structure;

            foreach (var post in BlankBoundaries)
            {
                Assert.NotEqual(
                    PushBackDefenseSides.HasFace(structure, post, PushBackSide.A),
                    PushBackDefenseSides.HasFace(structure, post, PushBackSide.B));
            }
        }
    }
}
