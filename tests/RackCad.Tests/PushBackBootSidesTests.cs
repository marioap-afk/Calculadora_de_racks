using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (S1E, contrato del dueño) — LAS BOTAS SE CONFIGURAN POR LADO.
    ///
    /// <para>
    /// Un Push Back compuesto tiene dos lados fisicos y cada uno tiene su propia intencion: su general y sus postes.
    /// Las palabras se leen DENTRO del lado —la entrada/salida de cada uno es SU pasillo y su posterior SU cara
    /// interior—, asi que ninguna vista tiene que reinterpretar la eleccion en su marco. La identidad fisica que
    /// resulta de esa lectura la cierra <see cref="PushBackBootIdentityTests"/> (S1F).
    /// </para>
    /// <para>
    /// <b>Lo que S1E corrige.</b> Con una sola configuracion global, «Entrada/Salida» significaba una cosa en la
    /// planta y otra en el corte del lado B, que se dibuja sobre una copia espejo. Medido antes de tocar codigo, en
    /// un compuesto sin blancos: planta 5 piezas en el pasillo de A, corte de A 5, corte de B 5 mas —que en planta
    /// no existian— y BOM 5. Con «Posterior», al reves: 5 piezas en planta y NINGUN corte que las mostrara.
    /// </para>
    /// <para>
    /// <b>Autoridad unica.</b> La pertenencia se resuelve en <see cref="SelectiveSafetySelection.BootFacesAt"/> y se
    /// proyecta a piezas fisicas en <see cref="PushBackBootPlan"/>. La planta, los cortes y el BOM consumen ESA
    /// resolucion; ninguno la vuelve a calcular.
    /// </para>
    /// </summary>
    public class PushBackBootSidesTests
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

        private static PushBackCompositeEditorState State(int slots, bool composite)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            if (composite)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(slots);
                for (var slot = 0; slot < slots; slot++)
                {
                    state.SetSlotPresent(PushBackSide.B, slot, true);
                }
            }

            return state;
        }

        /// <summary>Un lado en blanco: sus dos primeras ranuras, como la columna de nave del dueño.</summary>
        private static PushBackCompositeEditorState Blanked(PushBackSide side)
        {
            var state = State(4, composite: true);
            state.SetSlotPresent(side, 1, false);
            state.SetSlotPresent(side, 2, false);
            return state;
        }

        /// <summary>La intencion de botas de un lado, tal y como la escribe la ventana desde S1E.</summary>
        private sealed class SideIntent
        {
            public BootPlacement? General;

            public List<(int Post, BootPlacement Placement)> Posts { get; } =
                new List<(int Post, BootPlacement Placement)>();

            public static SideIntent Of(BootPlacement? general, params (int Post, BootPlacement Placement)[] posts)
            {
                var intent = new SideIntent { General = general };
                intent.Posts.AddRange(posts ?? Array.Empty<(int, BootPlacement)>());
                return intent;
            }
        }

        /// <summary>
        /// Resuelve el rack con la intencion DECLARADA POR LADO, que es lo que guarda un documento de S1E. Una
        /// general nula significa «este lado hereda su automatico».
        /// </summary>
        private static PushBackSystem Resolve(
            PushBackCompositeEditorState state, SideIntent sideA, SideIntent sideB = null)
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            design.Structure.SafetySelections.Clear();

            var selection = new SelectiveSafetySelection
            {
                ElementId = BootId,
                Quantity = 1,
                BootSidesDeclared = true,
            };
            Write(selection.Bota, sideA);
            Write(selection.BotaB, sideB);
            design.Structure.SafetySelections.Add(selection);
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static void Write(SelectiveBotaConfig config, SideIntent intent)
        {
            if (intent == null)
            {
                return;
            }

            config.Placement = intent.General;
            foreach (var (post, placement) in intent.Posts)
            {
                config.Posts.Add(new BootPostPlacement { PostIndex = post, Placement = placement });
            }
        }

        // ==================================================================== lo fisico

        private static IReadOnlyList<HeaderBlockInstance> Boots(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, BootId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private static IReadOnlyList<ResolvedBoot> Physical(PushBackSystem system)
            => PushBackBootPlan.Resolve(system, Catalog);

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, BootId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        /// <summary>Las botas de los CUATRO cortes de un rack compuesto (o los dos de uno simple).</summary>
        private static int CutBoots(PushBackSystem system)
        {
            var ends = new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior };
            var sides = system.IsComposite ? new[] { PushBackSide.A, PushBackSide.B } : new[] { PushBackSide.A };
            return ends.Sum(end => sides.Sum(side => Cut(system, end, side).Count));
        }

        private static int Bom(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, BootId, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);

        private static double Middle(PushBackSystem system) => system.Structure.TotalLength / 2.0;

        private static int NearBoots(PushBackSystem system)
            => Boots(system).Count(boot => boot.Insertion.X < Middle(system));

        private static int FarBoots(PushBackSystem system)
            => Boots(system).Count(boot => boot.Insertion.X > Middle(system));

        private static SelectiveSafetySelection Selection(PushBackSystem system)
            => system.Structure.SafetySelections.Single(selection =>
                string.Equals(selection.ElementId, BootId, StringComparison.OrdinalIgnoreCase));

        // ==================================================================== §28 el modelo

        /// <summary>Cada lado guarda su intencion, y son objetos distintos.</summary>
        [Fact]
        public void BootConfig_IsIndependentBySide()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            selection.Bota.Placement = BootPlacement.EntryExit;
            selection.BotaB.Placement = BootPlacement.Rear;

            Assert.NotSame(selection.Bota, selection.BotaB);
            Assert.Equal(BootPlacement.EntryExit, selection.BootPlacementAt(0));
            Assert.Equal(BootPlacement.Rear, selection.BootPlacementAtSideB(0));
        }

        [Fact]
        public void BootConfig_SideA_DoesNotMutateSideB()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            selection.BotaB.Placement = BootPlacement.Both;
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.Rear });

            selection.Bota.Placement = BootPlacement.None;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.EntryExit });

            Assert.Equal(BootPlacement.Both, selection.BotaB.Placement);
            Assert.Equal(BootPlacement.Rear, selection.BootPlacementAtSideB(1));
        }

        [Fact]
        public void BootConfig_SideB_DoesNotMutateSideA()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            selection.Bota.Placement = BootPlacement.EntryExit;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Both });

            selection.BotaB.Placement = BootPlacement.None;
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.None });

            Assert.Equal(BootPlacement.EntryExit, selection.Bota.Placement);
            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(2));
        }

        /// <summary>
        /// LADO y CARA son ejes distintos, y cada par nombra una cara fisica PROPIA. RETARGETEADO EN S1F: la version
        /// de S1E afirmaba que la entrada de A y la posterior de B eran la misma pieza —la conversion a un eje
        /// global cercano/lejano—, y el dueño lo rechazo: la posterior de B es SU cara interior, no el exterior de A.
        /// </summary>
        [Fact]
        public void BootConfig_SideAndFaceAreIndependentAxes()
        {
            var system = Resolve(
                State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.Rear));
            var line = Physical(system).Where(boot => boot.PostIndex == 1).ToList();

            Assert.Equal(2, line.Count);
            Assert.Equal(2, line.Select(boot => boot.Identity).Distinct().Count());
            Assert.Equal(0.0, line.Single(b => b.Side == PushBackSide.A).FaceX, 3);
            Assert.Equal(
                system.Structure.InteriorFaceEndX.Value,
                line.Single(b => b.Side == PushBackSide.B).FaceX,
                3);
        }

        [Fact]
        public void BootConfig_PerPost_IsSideScoped()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            selection.Bota.Placement = BootPlacement.None;
            selection.BotaB.Placement = BootPlacement.None;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.EntryExit });

            Assert.Equal(BootPlacement.EntryExit, selection.BootPlacementAt(2));
            Assert.Equal(BootPlacement.None, selection.BootPlacementAtSideB(2));
            Assert.True(selection.BootFacesAt(2).Near);
            Assert.False(selection.BootFacesAt(2).Far);
        }

        /// <summary>«Por defecto» y «Ninguno» son cosas distintas, y lo son POR LADO.</summary>
        [Fact]
        public void BootConfig_DefaultVsExplicitNone_IsSideScoped()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            selection.Bota.Automatic = BootPlacement.EntryExit;
            selection.BotaB.Automatic = BootPlacement.EntryExit;
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.None });

            Assert.False(selection.Bota.HasOwnAt(1));
            Assert.Equal(BootPlacement.EntryExit, selection.BootPlacementAt(1));     // A hereda
            Assert.True(selection.BotaB.HasOwnAt(1));
            Assert.Equal(BootPlacement.None, selection.BootPlacementAtSideB(1));     // B eligio «ninguna»
        }

        // ==================================================================== §29 la semantica A/B

        [Fact]
        public void SideA_EntryExit_OnlyCreatesAEntryBoots()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.None));

            Assert.NotEmpty(Boots(system));
            Assert.Equal(0, FarBoots(system));
            Assert.All(Physical(system), boot => Assert.Equal(PushBackSide.A, boot.Side));
            Assert.All(Physical(system), boot => Assert.Equal(BootFace.EntryExit, boot.Face));
        }

        [Fact]
        public void SideA_Rear_OnlyCreatesARearBoots()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.Rear), SideIntent.Of(BootPlacement.None));

            // RETARGETEADO EN S1F: la posterior de A es su cara INTERIOR, la que da a la interfaz — no el extremo
            // lejano del rack, que es el pasillo de B.
            Assert.NotEmpty(Boots(system));
            Assert.All(Physical(system), boot =>
            {
                Assert.Equal(PushBackSide.A, boot.Side);
                Assert.Equal(BootFace.Rear, boot.Face);
                Assert.Equal(system.Structure.InteriorFaceStartX.Value, boot.FaceX, 3);
            });
        }

        [Fact]
        public void SideA_Both_CreatesBothAFaces()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.Both), SideIntent.Of(BootPlacement.None));

            Assert.Equal(NearBoots(system), FarBoots(system));
            Assert.Equal(Boots(system).Count, NearBoots(system) + FarBoots(system));
        }

        [Fact]
        public void SideB_EntryExit_OnlyCreatesBEntryBoots()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.EntryExit));

            Assert.NotEmpty(Boots(system));
            Assert.Equal(0, NearBoots(system));
            Assert.All(Physical(system), boot => Assert.Equal(PushBackSide.B, boot.Side));
            Assert.All(Physical(system), boot => Assert.Equal(BootFace.EntryExit, boot.Face));
        }

        [Fact]
        public void SideB_Rear_OnlyCreatesBRearBoots()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.Rear));

            // RETARGETEADO EN S1F: la posterior de B es SU cara interior, no el extremo cercano del rack.
            Assert.NotEmpty(Boots(system));
            Assert.All(Physical(system), boot =>
            {
                Assert.Equal(PushBackSide.B, boot.Side);
                Assert.Equal(BootFace.Rear, boot.Face);
                Assert.Equal(system.Structure.InteriorFaceEndX.Value, boot.FaceX, 3);
            });
        }

        [Fact]
        public void SideB_Both_CreatesBothBFaces()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.Both));

            Assert.Equal(NearBoots(system), FarBoots(system));
            Assert.Equal(Boots(system).Count, NearBoots(system) + FarBoots(system));
        }

        /// <summary>Las dos entradas son ubicaciones fisicas DISTINTAS: la de A y la de B.</summary>
        [Fact]
        public void SideAEntry_SideBEntry_CreateBothPhysicalEntries()
        {
            var onlyA = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.None));
            var onlyB = Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.EntryExit));
            var both = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.EntryExit));

            Assert.Equal(Boots(onlyA).Count + Boots(onlyB).Count, Boots(both).Count);
            Assert.Equal(NearBoots(onlyA), NearBoots(both));
            Assert.Equal(FarBoots(onlyB), FarBoots(both));
        }

        /// <summary>Y lo que elija A no reinterpreta lo de B: cambiar A deja las piezas de B exactamente donde estaban.</summary>
        [Fact]
        public void SideAChoice_DoesNotReinterpretSideB()
        {
            var b = SideIntent.Of(BootPlacement.EntryExit);
            var far = new List<int>();
            foreach (var a in new[] { BootPlacement.None, BootPlacement.EntryExit, BootPlacement.Rear, BootPlacement.Both })
            {
                var system = Resolve(State(4, true), SideIntent.Of(a), b);
                far.Add(Physical(system).Count(boot => boot.Side == PushBackSide.B));
            }

            Assert.All(far, count => Assert.Equal(far[0], count));
        }

        // ==================================================================== §30 blancos

        [Fact]
        public void BlankA_Default_DoesNotRemoveB()
        {
            var full = Resolve(State(4, true), SideIntent.Of(null), SideIntent.Of(null));
            var blanked = Resolve(Blanked(PushBackSide.A), SideIntent.Of(null), SideIntent.Of(null));

            Assert.Equal(FarBoots(full), FarBoots(blanked));
            Assert.Equal(NearBoots(full) - 1, NearBoots(blanked));
        }

        [Fact]
        public void BlankB_Default_DoesNotRemoveA()
        {
            var full = Resolve(State(4, true), SideIntent.Of(null), SideIntent.Of(null));
            var blanked = Resolve(Blanked(PushBackSide.B), SideIntent.Of(null), SideIntent.Of(null));

            Assert.Equal(NearBoots(full), NearBoots(blanked));
            Assert.Equal(FarBoots(full) - 1, FarBoots(blanked));
        }

        [Fact]
        public void BlankA_ExplicitBoot_StillWorks()
        {
            var automatic = Resolve(Blanked(PushBackSide.A), SideIntent.Of(null), SideIntent.Of(null));
            var chosen = Resolve(
                Blanked(PushBackSide.A),
                SideIntent.Of(null, (2, BootPlacement.EntryExit)),
                SideIntent.Of(null));

            Assert.Equal(NearBoots(automatic) + 1, NearBoots(chosen));
            Assert.Equal(FarBoots(automatic), FarBoots(chosen));
        }

        [Fact]
        public void BlankB_ExplicitBoot_StillWorks()
        {
            var automatic = Resolve(Blanked(PushBackSide.B), SideIntent.Of(null), SideIntent.Of(null));
            var chosen = Resolve(
                Blanked(PushBackSide.B),
                SideIntent.Of(null),
                SideIntent.Of(null, (2, BootPlacement.EntryExit)));

            Assert.Equal(FarBoots(automatic) + 1, FarBoots(chosen));
            Assert.Equal(NearBoots(automatic), NearBoots(chosen));
        }

        [Fact]
        public void BlankAAndB_AreIndependent()
        {
            var a = Resolve(Blanked(PushBackSide.A), SideIntent.Of(null), SideIntent.Of(null));
            var b = Resolve(Blanked(PushBackSide.B), SideIntent.Of(null), SideIntent.Of(null));

            Assert.Equal(FarBoots(a) - 1, FarBoots(b));
            Assert.Equal(NearBoots(a) + 1, NearBoots(b));
        }

        /// <summary>La declaracion del blanco es de un LADO y de un POSTE, y se la hace la misma autoridad que la defensa.</summary>
        [Fact]
        public void BlankState_IsSidePostScoped()
        {
            var a = Selection(Resolve(Blanked(PushBackSide.A), SideIntent.Of(null), SideIntent.Of(null)));
            var b = Selection(Resolve(Blanked(PushBackSide.B), SideIntent.Of(null), SideIntent.Of(null)));

            Assert.Equal(new[] { 2 }, a.Bota.BlankPosts.ToArray());
            Assert.Empty(a.BotaB.BlankPosts);
            Assert.Empty(b.Bota.BlankPosts);
            Assert.Equal(new[] { 2 }, b.BotaB.BlankPosts.ToArray());
        }

        // ==================================================================== §31 la general «Ninguno»

        [Fact]
        public void SideA_GeneralNone_DoesNotDisableAOverrides()
        {
            var system = Resolve(
                State(4, true),
                SideIntent.Of(BootPlacement.None, (2, BootPlacement.EntryExit)),
                SideIntent.Of(BootPlacement.None));

            Assert.Single(Boots(system));
            Assert.Equal(1, NearBoots(system));
            Assert.Equal(Boots(system).Count, Bom(system));
        }

        [Fact]
        public void SideB_GeneralNone_DoesNotDisableBOverrides()
        {
            var system = Resolve(
                State(4, true),
                SideIntent.Of(BootPlacement.None),
                SideIntent.Of(BootPlacement.None, (2, BootPlacement.EntryExit)));

            Assert.Single(Boots(system));
            Assert.Equal(1, FarBoots(system));
            Assert.Equal(Boots(system).Count, Bom(system));
        }

        [Fact]
        public void SideA_GeneralNone_DoesNotAffectB()
        {
            var both = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.EntryExit));
            var aOff = Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.EntryExit));

            Assert.Equal(FarBoots(both), FarBoots(aOff));
            Assert.Equal(0, NearBoots(aOff));
        }

        [Fact]
        public void SideB_GeneralNone_DoesNotAffectA()
        {
            var both = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.EntryExit));
            var bOff = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.None));

            Assert.Equal(NearBoots(both), NearBoots(bOff));
            Assert.Equal(0, FarBoots(bOff));
        }

        // ==================================================================== §32 vistas y BOM

        /// <summary>La planta dibuja EXACTAMENTE el conjunto fisico resuelto, pieza a pieza.</summary>
        [Fact]
        public void ResolvedBoots_PlantMatchesPhysicalSet()
        {
            foreach (var system in Scenarios())
            {
                var drawn = Boots(system)
                    .Select(instance => FormattableString.Invariant(
                        $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}"))
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList();
                var resolved = Physical(system)
                    .Select(boot => FormattableString.Invariant(
                        $"{boot.PlantaAt.X:0.###}|{boot.PlantaAt.Y:0.###}|{boot.Mirrored}"))
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList();

                Assert.Equal(resolved, drawn);
            }
        }

        /// <summary>Y los cortes, entre todos, muestran ese mismo conjunto: cada pieza en el corte de su plano.</summary>
        [Fact]
        public void ResolvedBoots_CutsMatchPhysicalSet()
        {
            foreach (var system in Scenarios())
            {
                Assert.Equal(Physical(system).Count, CutBoots(system));
            }
        }

        [Fact]
        public void ResolvedBoots_BomMatchesPhysicalSet()
        {
            foreach (var system in Scenarios())
            {
                Assert.Equal(Physical(system).Count, Bom(system));
            }
        }

        [Fact]
        public void SideAEntryOnly_PlantCutsBomAgree()
            => AssertAgree(Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.None)));

        [Fact]
        public void SideBEntryOnly_PlantCutsBomAgree()
            => AssertAgree(Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.EntryExit)));

        [Fact]
        public void BothSidesEntry_PlantCutsBomAgree()
            => AssertAgree(Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.EntryExit)));

        [Fact]
        public void BothSidesBoth_PlantCutsBomAgree()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.Both), SideIntent.Of(BootPlacement.Both));
            AssertAgree(system);

            // «Ambas» en los dos lados son las MISMAS dos caras fisicas: dos piezas por linea, no cuatro.
            Assert.Equal(NearBoots(system) + FarBoots(system), Boots(system).Count);
            Assert.Equal(NearBoots(system), FarBoots(system));
        }

        private static void AssertAgree(PushBackSystem system)
        {
            Assert.NotEmpty(Physical(system));
            Assert.Equal(Physical(system).Count, Boots(system).Count);
            Assert.Equal(Physical(system).Count, CutBoots(system));
            Assert.Equal(Physical(system).Count, Bom(system));
        }

        /// <summary>
        /// El corte del lado B se dibuja sobre una copia ESPEJO, y aun asi «Entrada/Salida de B» sigue siendo su
        /// entrada: la reflexion transforma coordenadas, nunca el significado.
        /// </summary>
        [Fact]
        public void ReflectedSideB_DoesNotReinterpretEntryAsRear()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.EntryExit));

            Assert.All(Physical(system), boot =>
            {
                Assert.Equal(PushBackSide.B, boot.Side);
                Assert.Equal(BootFace.EntryExit, boot.Face);
                Assert.Equal(system.Structure.TotalLength, boot.FaceX, 3);   // el pasillo de B, su exterior
            });

            Assert.Empty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Equal(Physical(system).Count, Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B).Count);
        }

        /// <summary>Los escenarios que las tres vistas deben representar igual.</summary>
        private static IEnumerable<PushBackSystem> Scenarios()
        {
            yield return Resolve(State(4, true), SideIntent.Of(null), SideIntent.Of(null));
            yield return Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.None));
            yield return Resolve(State(4, true), SideIntent.Of(BootPlacement.None), SideIntent.Of(BootPlacement.EntryExit));
            yield return Resolve(State(4, true), SideIntent.Of(BootPlacement.Both), SideIntent.Of(BootPlacement.Rear));
            yield return Resolve(Blanked(PushBackSide.A), SideIntent.Of(null), SideIntent.Of(null));
            yield return Resolve(State(4, false), SideIntent.Of(BootPlacement.Both));
            yield return Resolve(State(4, false), SideIntent.Of(BootPlacement.Rear));
        }

        // ==================================================================== §34 legacy

        /// <summary>Un documento anterior de un rack SIMPLE: su unica configuracion es la del lado existente.</summary>
        [Fact]
        public void PushBackSimpleLegacyBoot_RoundTrips()
        {
            var legacy = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1, Side = SafetySide.Left };
            var restored = SafetySelectionDocument.From(legacy).ToDomain();

            Assert.False(restored.BootSidesDeclared);
            Assert.Null(restored.Bota.Placement);
            Assert.Equal(BootPlacement.EntryExit, restored.BootPlacementAt(0));   // por su lado historico
            Assert.Equal(BootPlacement.None, restored.BootPlacementAtSideB(0));
        }

        /// <summary>
        /// Y uno COMPUESTO con la unica configuracion global: se resuelve entera sobre el lado A —que es como se
        /// dibujaba— y el lado B no pide nada. La correspondencia es determinista y no inventa ninguna decision.
        /// </summary>
        [Theory]
        [InlineData(BootPlacement.EntryExit, true, false)]
        [InlineData(BootPlacement.Rear, false, true)]
        [InlineData(BootPlacement.Both, true, true)]
        [InlineData(BootPlacement.None, false, false)]
        public void CompositeLegacySingleBootConfig_HasDeterministicFallback(
            BootPlacement legacy, bool near, bool far)
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1 };
            selection.Bota.Placement = legacy;
            selection.Bota.Automatic = BootPlacement.EntryExit;
            selection.BotaB.Automatic = BootPlacement.EntryExit;

            Assert.Equal(near, selection.BootFacesAt(0).Near);
            Assert.Equal(far, selection.BootFacesAt(0).Far);
        }

        /// <summary>El lado historico sigue significando lo mismo para el PROTECTOR LATERAL (I-32).</summary>
        [Fact]
        public void LegacySafetySide_DoesNotChangeLateralProtector()
        {
            var lateral = new SelectiveSafetySelection { ElementId = "PROTECTOR_LATERAL", Side = SafetySide.Right };
            var copies = SelectiveSafetyEnds.CopiesForPost(lateral, 0);

            Assert.NotEmpty(copies);
            Assert.All(copies, copy => Assert.True(copy.Mirrored));
        }

        /// <summary>Y para el DESVIADOR, que sigue en el extremo bajo (R1).</summary>
        [Fact]
        public void LegacySafetySide_DoesNotChangeDiverter()
        {
            var diverter = new SelectiveSafetySelection { ElementId = "DESVIADOR", Side = SafetySide.Right, LowEndOnly = true };
            var copies = SelectiveSafetyEnds.CopiesForPost(diverter, 0);

            Assert.All(copies, copy => Assert.False(copy.AtHighEnd));
        }

        /// <summary>Un documento sin los campos nuevos se lee sin error y sin cambiar de intencion.</summary>
        [Fact]
        public void OldDocuments_NoNewFields_LoadSafely()
        {
            var document = new SafetySelectionDocument
            {
                ElementId = BootId,
                Quantity = 1,
                Side = (int)SafetySide.Both,
            };

            var restored = document.ToDomain();

            Assert.False(restored.BootSidesDeclared);
            Assert.Null(restored.BotaB.Placement);
            Assert.Empty(restored.BotaB.Posts);
            Assert.Empty(restored.Bota.BlankPosts);
        }

        /// <summary>Y uno de S1E vuelve con las dos intenciones intactas y distintas.</summary>
        [Fact]
        public void NewSideScopedBoots_RackEditarRoundTrip()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1, BootSidesDeclared = true };
            selection.Bota.Placement = BootPlacement.EntryExit;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Rear });
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 3, Placement = BootPlacement.Both });
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 4, Placement = BootPlacement.None });
            selection.BotaB.Placement = BootPlacement.Rear;
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.Both });
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 3, Placement = BootPlacement.EntryExit });
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 4, Placement = BootPlacement.None });

            var restored = SafetySelectionDocument.From(selection).ToDomain();

            Assert.True(restored.BootSidesDeclared);
            Assert.Equal(BootPlacement.EntryExit, restored.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, restored.BotaB.Placement);
            Assert.False(restored.Bota.HasOwnAt(1));                       // A/P1 sigue heredando
            Assert.Equal(BootPlacement.Both, restored.BotaB.At(1));        // B/P1 no
            Assert.Equal(BootPlacement.Rear, restored.Bota.At(2));
            Assert.Equal(BootPlacement.Both, restored.Bota.At(3));
            Assert.Equal(BootPlacement.EntryExit, restored.BotaB.At(3));
            Assert.Equal(BootPlacement.None, restored.Bota.At(4));
            Assert.Equal(BootPlacement.None, restored.BotaB.At(4));
        }

        // ==================================================================== §35 bites

        /// <summary>
        /// BITE A — LA INDEPENDENCIA. Compartir una sola configuracion entre los dos lados cambia el resultado
        /// fisico: es lo que hacia el modelo global, y es exactamente lo que S1E viene a separar.
        /// </summary>
        [Fact]
        public void Bite_SharingOneConfigBetweenSides_BreaksIndependence()
        {
            var shared = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            shared.Bota.Placement = BootPlacement.EntryExit;
            shared.BotaB = shared.Bota;   // el modelo global: una sola intencion para los dos lados

            var separate = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            separate.Bota.Placement = BootPlacement.EntryExit;
            separate.BotaB.Placement = BootPlacement.None;

            Assert.True(shared.BootFacesAt(0).Far);        // la entrada de B, que nadie pidio
            Assert.False(separate.BootFacesAt(0).Far);
        }

        /// <summary>
        /// BITE B — EL MARCO DE LA VISTA. Si un corte reinterpretara la eleccion en su marco local, el corte del
        /// lado B mostraria las piezas de A. Se comprueba sobre el conjunto fisico: el corte de B solo muestra las
        /// que estan en SU plano.
        /// </summary>
        [Fact]
        public void Bite_ReinterpretingTheCutFrame_BreaksViewConsistency()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.EntryExit), SideIntent.Of(BootPlacement.None));

            Assert.NotEmpty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Empty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }

        /// <summary>
        /// BITE C — LA AUTORIDAD UNICA. Planta, cortes y BOM cuentan el MISMO conjunto en todos los escenarios; si
        /// alguno resolviera la pertenencia por su cuenta, esta igualdad caeria en cuanto los lados no coinciden.
        /// </summary>
        [Fact]
        public void Bite_ResolvingMembershipPerView_BreaksTheSingleAuthority()
        {
            var system = Resolve(State(4, true), SideIntent.Of(BootPlacement.Rear), SideIntent.Of(BootPlacement.None));

            Assert.Equal(Physical(system).Count, Boots(system).Count);
            Assert.Equal(Physical(system).Count, CutBoots(system));
            Assert.Equal(Physical(system).Count, Bom(system));
        }

        /// <summary>
        /// BITE D — EL BLANCO POR LADO. Un blanco declarado sobre la linea entera —la autoridad de S1C— apagaria
        /// tambien el pasillo del lado contrario.
        /// </summary>
        [Fact]
        public void Bite_GlobalBlankLines_BreakSideScopedBlanks()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, BootSidesDeclared = true };
            selection.Bota.Automatic = BootPlacement.EntryExit;
            selection.BotaB.Automatic = BootPlacement.EntryExit;
            selection.Bota.BlankPosts.Add(2);

            Assert.False(selection.BootFacesAt(2).Near);   // el lado en blanco
            Assert.True(selection.BootFacesAt(2).Far);     // el contrario, intacto

            selection.BotaB.BlankPosts.Add(2);
            Assert.False(selection.BootFacesAt(2).Far);    // los dos en blanco: ahi si, ninguna
        }
    }
}
