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
    /// I-42 (S1G, contrato del dueño) — EL TIPO DE BOTA ES DE CADA LADO, y «Ninguno» es un tipo.
    ///
    /// <para>
    /// S1F dejo la ubicacion y los postes por lado, pero el TIPO seguia viviendo en una fila global al fondo de la
    /// ventana: una tercera autoridad que gobernaba A y B desde abajo — el mismo defecto que las defensas ya habian
    /// tenido. Desde S1G cada lado posee su configuracion entera: <b>tipo + ubicacion + postes</b>.
    /// </para>
    /// <para>
    /// TIPO y UBICACION son ejes distintos. «Sin pieza» no es «sin ubicacion»: poner el tipo en «Ninguno» no borra
    /// nada —la intencion queda DORMIDA— y volver a elegir una pieza la recupera entera.
    /// </para>
    /// </summary>
    public class PushBackBootTypeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static IReadOnlyList<string> BootVariants => Catalog.SafetyElements
            .Where(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType))
            .Select(entry => entry.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        private static string BootId => BootVariants[0];

        private static string OtherBootId => BootVariants.Count > 1 ? BootVariants[1] : BootVariants[0];

        private const string NoneType = PushBackDefaults.NonePieceId;

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(bool composite, params int[] blanksA)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(4);
            if (composite)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(4);
                for (var slot = 0; slot < 4; slot++)
                {
                    state.SetSlotPresent(PushBackSide.B, slot, true);
                }
            }

            foreach (var blank in blanksA ?? Array.Empty<int>())
            {
                state.SetSlotPresent(PushBackSide.A, blank, false);
            }

            return state;
        }

        /// <summary>La intencion de un lado tal y como la escribe su seccion desde S1G: tipo, ubicacion y postes.</summary>
        private sealed class SideIntent
        {
            public string PieceId;
            public BootPlacement? Placement;
            public List<(int Post, BootPlacement Placement)> Posts { get; } = new List<(int, BootPlacement)>();

            public static SideIntent Of(
                string pieceId, BootPlacement? placement, params (int Post, BootPlacement Placement)[] posts)
            {
                var intent = new SideIntent { PieceId = pieceId, Placement = placement };
                intent.Posts.AddRange(posts ?? Array.Empty<(int, BootPlacement)>());
                return intent;
            }
        }

        private static SelectiveSafetySelection Selection(SideIntent sideA, SideIntent sideB)
        {
            var selection = new SelectiveSafetySelection
            {
                ElementId = BootId,
                Quantity = 1,
                BootSidesDeclared = true,
            };
            Write(selection.Bota, sideA);
            Write(selection.BotaB, sideB);
            return selection;
        }

        private static void Write(SelectiveBotaConfig config, SideIntent intent)
        {
            if (intent == null)
            {
                return;
            }

            config.PieceId = intent.PieceId;
            config.Placement = intent.Placement;
            foreach (var (post, placement) in intent.Posts)
            {
                config.Posts.Add(new BootPostPlacement { PostIndex = post, Placement = placement });
            }
        }

        private static PushBackSystem Resolve(
            PushBackCompositeEditorState state, SideIntent sideA, SideIntent sideB = null)
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            design.Structure.SafetySelections.Clear();
            design.Structure.SafetySelections.Add(Selection(sideA, sideB));
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<ResolvedBoot> Physical(PushBackSystem system)
            => PushBackBootPlan.Resolve(system, Catalog);

        private static IReadOnlyList<HeaderBlockInstance> Drawn(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                .Where(instance => BootVariants.Contains(instance.PieceId, StringComparer.OrdinalIgnoreCase))
                .ToList();

        private static int Bom(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => BootVariants.Contains(line.ProfileId, StringComparer.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);

        private static int Of(PushBackSystem system, PushBackSide side)
            => Physical(system).Count(boot => boot.Side == side);

        // ==================================================================== §25 el modelo

        [Fact]
        public void BootType_IsIndependentBySide()
        {
            var selection = Selection(
                SideIntent.Of(BootId, BootPlacement.EntryExit),
                SideIntent.Of(NoneType, BootPlacement.Rear));

            Assert.Equal(BootId, selection.BootPieceOf(selection.Bota));
            Assert.Equal(NoneType, selection.BootPieceOf(selection.BotaB));
        }

        [Fact]
        public void BootTypeA_DoesNotMutateBootTypeB()
        {
            var selection = Selection(
                SideIntent.Of(BootId, BootPlacement.EntryExit),
                SideIntent.Of(OtherBootId, BootPlacement.Rear));

            selection.Bota.PieceId = NoneType;

            Assert.Equal(OtherBootId, selection.BotaB.PieceId);
            Assert.Equal(BootPlacement.Rear, selection.BotaB.Placement);
        }

        [Fact]
        public void BootTypeB_DoesNotMutateBootTypeA()
        {
            var selection = Selection(
                SideIntent.Of(BootId, BootPlacement.Both, (2, BootPlacement.Rear)),
                SideIntent.Of(OtherBootId, BootPlacement.Rear));

            selection.BotaB.PieceId = NoneType;

            Assert.Equal(BootId, selection.Bota.PieceId);
            Assert.Equal(BootPlacement.Both, selection.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, selection.Bota.At(2));
        }

        [Fact]
        public void BootTypeNone_DoesNotErasePlacement()
        {
            var selection = Selection(SideIntent.Of(NoneType, BootPlacement.Both), null);

            Assert.Equal(BootPlacement.Both, selection.Bota.Placement);
            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(0));   // la intencion sigue resolviendo
        }

        [Fact]
        public void BootTypeNone_DoesNotErasePostOverrides()
        {
            var selection = Selection(
                SideIntent.Of(NoneType, BootPlacement.Both, (2, BootPlacement.Rear), (3, BootPlacement.None)),
                null);

            Assert.Equal(BootPlacement.Rear, selection.Bota.At(2));
            Assert.Equal(BootPlacement.None, selection.Bota.At(3));
        }

        /// <summary>Y al volver a elegir pieza reaparece EXACTAMENTE lo que habia, sin reconfigurar nada.</summary>
        [Fact]
        public void RestoringBootType_RestoresDormantIntent()
        {
            var state = State(composite: true);
            var dormant = SideIntent.Of(NoneType, BootPlacement.Both, (2, BootPlacement.Rear));
            var awake = SideIntent.Of(BootId, BootPlacement.Both, (2, BootPlacement.Rear));
            var none = SideIntent.Of(NoneType, BootPlacement.None);

            Assert.Equal(0, Of(Resolve(state, dormant, none), PushBackSide.A));

            var restored = Resolve(state, awake, none);
            Assert.NotEmpty(Physical(restored));
            Assert.All(Physical(restored), boot => Assert.Equal(PushBackSide.A, boot.Side));
            Assert.Single(Physical(restored).Where(boot => boot.PostIndex == 2 && boot.Face == BootFace.Rear));
        }

        // ==================================================================== §26 la materializacion

        [Fact]
        public void SideA_TypeNone_ProducesNoABoots()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(NoneType, BootPlacement.Both),
                SideIntent.Of(BootId, BootPlacement.EntryExit));

            Assert.Equal(0, Of(system, PushBackSide.A));
            Assert.True(Of(system, PushBackSide.B) > 0);
        }

        [Fact]
        public void SideB_TypeNone_ProducesNoBBoots()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(BootId, BootPlacement.EntryExit),
                SideIntent.Of(NoneType, BootPlacement.Both));

            Assert.Equal(0, Of(system, PushBackSide.B));
            Assert.True(Of(system, PushBackSide.A) > 0);
        }

        [Fact]
        public void SideA_TypeH_SideBTypeNone_OnlyProducesA()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(BootId, BootPlacement.EntryExit),
                SideIntent.Of(NoneType, BootPlacement.Rear));

            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot => Assert.Equal(PushBackSide.A, boot.Side));
            Assert.Equal(Physical(system).Count, Drawn(system).Count);
            Assert.Equal(Physical(system).Count, Bom(system));
        }

        [Fact]
        public void SideA_TypeNone_SideBTypeH_OnlyProducesB()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(NoneType, BootPlacement.EntryExit),
                SideIntent.Of(BootId, BootPlacement.Rear));

            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot => Assert.Equal(PushBackSide.B, boot.Side));
            Assert.Equal(Physical(system).Count, Drawn(system).Count);
            Assert.Equal(Physical(system).Count, Bom(system));
        }

        /// <summary>Con los dos tipos elegidos —y pueden ser DISTINTOS— cada lado materializa el suyo.</summary>
        [Fact]
        public void BothTypesEnabled_ProducesBothSides()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(BootId, BootPlacement.EntryExit),
                SideIntent.Of(OtherBootId, BootPlacement.EntryExit));

            Assert.True(Of(system, PushBackSide.A) > 0);
            Assert.True(Of(system, PushBackSide.B) > 0);
            Assert.All(
                Physical(system).Where(boot => boot.Side == PushBackSide.A),
                boot => Assert.Equal(BootId, boot.PieceId));
            Assert.All(
                Physical(system).Where(boot => boot.Side == PushBackSide.B),
                boot => Assert.Equal(OtherBootId, boot.PieceId));
            Assert.Equal(Physical(system).Count, Drawn(system).Count);
            Assert.Equal(Physical(system).Count, Bom(system));
        }

        /// <summary>TIPO «Ninguno» y UBICACION «Ninguno» son ejes distintos: el primero no reescribe al segundo.</summary>
        [Fact]
        public void BootTypeNone_IsNotPlacementNone()
        {
            var typeNone = Selection(SideIntent.Of(NoneType, BootPlacement.Both), null);
            var placementNone = Selection(SideIntent.Of(BootId, BootPlacement.None), null);

            Assert.Equal(BootPlacement.Both, typeNone.BootPlacementAt(0));     // la ubicacion sigue ahi
            Assert.Equal(BootPlacement.None, placementNone.BootPlacementAt(0));
            Assert.Equal(BootId, placementNone.BootPieceOf(placementNone.Bota));
        }

        // ==================================================================== §27 la general «Ninguno»

        [Fact]
        public void TypeH_GeneralNone_ExplicitRear_ProducesRear()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(BootId, BootPlacement.None, (2, BootPlacement.Rear)),
                SideIntent.Of(NoneType, BootPlacement.None));

            Assert.Single(Physical(system));
            Assert.Equal(BootFace.Rear, Physical(system)[0].Face);
            Assert.Equal(PushBackSide.A, Physical(system)[0].Side);
        }

        [Fact]
        public void TypeNone_GeneralNone_ExplicitRear_ProducesNone()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(NoneType, BootPlacement.None, (2, BootPlacement.Rear)),
                SideIntent.Of(NoneType, BootPlacement.None));

            Assert.Empty(Physical(system));
            Assert.Equal(0, Bom(system));
        }

        [Fact]
        public void RestoringType_ReactivatesExplicitRear()
        {
            var state = State(composite: true);
            var none = SideIntent.Of(NoneType, BootPlacement.None);
            var dormant = Resolve(state, SideIntent.Of(NoneType, BootPlacement.None, (2, BootPlacement.Rear)), none);
            var awake = Resolve(state, SideIntent.Of(BootId, BootPlacement.None, (2, BootPlacement.Rear)), none);

            Assert.Empty(Physical(dormant));
            Assert.Single(Physical(awake));
            Assert.Equal(2, Physical(awake)[0].PostIndex);
        }

        // ==================================================================== §28 el blanco

        [Fact]
        public void TypeH_Blank_Default_KeepsS1fResolution()
        {
            var none = SideIntent.Of(NoneType, BootPlacement.None);
            var full = Resolve(State(composite: true), SideIntent.Of(BootId, null), none);
            var blanked = Resolve(State(true, 1, 2), SideIntent.Of(BootId, null), none);

            Assert.Equal(Of(full, PushBackSide.A) - 1, Of(blanked, PushBackSide.A));
        }

        [Fact]
        public void TypeH_Blank_Explicit_StillMaterializes()
        {
            var none = SideIntent.Of(NoneType, BootPlacement.None);
            var automatic = Resolve(State(true, 1, 2), SideIntent.Of(BootId, null), none);
            var chosen = Resolve(
                State(true, 1, 2), SideIntent.Of(BootId, null, (2, BootPlacement.EntryExit)), none);

            Assert.Equal(Of(automatic, PushBackSide.A) + 1, Of(chosen, PushBackSide.A));
        }

        [Fact]
        public void TypeNone_Blank_KeepsIntentDormant()
        {
            var state = State(true, 1, 2);
            var none = SideIntent.Of(NoneType, BootPlacement.None);
            var dormant = Resolve(state, SideIntent.Of(NoneType, null, (2, BootPlacement.EntryExit)), none);
            var awake = Resolve(state, SideIntent.Of(BootId, null, (2, BootPlacement.EntryExit)), none);

            Assert.Empty(Physical(dormant));
            Assert.Contains(Physical(awake), boot => boot.PostIndex == 2 && boot.Face == BootFace.EntryExit);
        }

        // ==================================================================== §29 la persistencia

        [Fact]
        public void BootTypesAB_RoundTripIndependently()
        {
            var restored = SafetySelectionDocument
                .From(Selection(
                    SideIntent.Of(BootId, BootPlacement.EntryExit),
                    SideIntent.Of(OtherBootId, BootPlacement.Rear)))
                .ToDomain();

            Assert.Equal(BootId, restored.Bota.PieceId);
            Assert.Equal(OtherBootId, restored.BotaB.PieceId);
        }

        [Fact]
        public void BootTypeNone_PreservesDormantPlacementRoundTrip()
        {
            var restored = SafetySelectionDocument
                .From(Selection(SideIntent.Of(NoneType, BootPlacement.Both), SideIntent.Of(NoneType, BootPlacement.Rear)))
                .ToDomain();

            Assert.Equal(NoneType, restored.Bota.PieceId);
            Assert.Equal(BootPlacement.Both, restored.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, restored.BotaB.Placement);
        }

        [Fact]
        public void BootTypeNone_PreservesDormantPostOverridesRoundTrip()
        {
            var restored = SafetySelectionDocument
                .From(Selection(
                    SideIntent.Of(NoneType, BootPlacement.Both, (2, BootPlacement.Rear), (3, BootPlacement.None)),
                    SideIntent.Of(NoneType, BootPlacement.EntryExit, (1, BootPlacement.Both))))
                .ToDomain();

            Assert.Equal(BootPlacement.Rear, restored.Bota.At(2));
            Assert.Equal(BootPlacement.None, restored.Bota.At(3));
            Assert.Equal(BootPlacement.Both, restored.BotaB.At(1));
        }

        /// <summary>Un documento anterior a S1G trae UN tipo global: los dos lados que existen lo heredan.</summary>
        [Fact]
        public void LegacyGlobalBootType_FallsBackToBothExistingSides()
        {
            var legacy = new SelectiveSafetySelection { ElementId = OtherBootId, Quantity = 1, BootSidesDeclared = true };
            legacy.Bota.Placement = BootPlacement.EntryExit;
            legacy.BotaB.Placement = BootPlacement.EntryExit;

            var restored = SafetySelectionDocument.From(legacy).ToDomain();

            Assert.Null(restored.Bota.PieceId);
            Assert.Null(restored.BotaB.PieceId);
            Assert.Equal(OtherBootId, restored.BootPieceOf(restored.Bota));
            Assert.Equal(OtherBootId, restored.BootPieceOf(restored.BotaB));
        }

        [Fact]
        public void LegacySimpleGlobalBootType_FallsBackToSingleSide()
        {
            var legacy = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1, Side = SafetySide.Left };
            var restored = SafetySelectionDocument.From(legacy).ToDomain();
            var system = Resolve(State(composite: false), SideIntent.Of(null, BootPlacement.EntryExit));

            Assert.Equal(BootId, restored.BootPieceOf(restored.Bota));
            Assert.NotEmpty(Physical(system));
            Assert.All(Physical(system), boot => Assert.Equal(BootId, boot.PieceId));
        }

        [Fact]
        public void RackEditar_PreservesBootTypeAndDormantIntent()
        {
            var selection = Selection(
                SideIntent.Of(BootId, BootPlacement.Both, (2, BootPlacement.Rear), (3, BootPlacement.None)),
                SideIntent.Of(NoneType, BootPlacement.EntryExit, (1, BootPlacement.Both), (4, BootPlacement.Rear)));

            var restored = SafetySelectionDocument.From(selection).ToDomain();

            Assert.Equal(BootId, restored.Bota.PieceId);
            Assert.Equal(BootPlacement.Both, restored.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, restored.Bota.At(2));
            Assert.Equal(BootPlacement.None, restored.Bota.At(3));

            Assert.Equal(NoneType, restored.BotaB.PieceId);
            Assert.Equal(BootPlacement.EntryExit, restored.BotaB.Placement);
            Assert.Equal(BootPlacement.Both, restored.BotaB.At(1));
            Assert.Equal(BootPlacement.Rear, restored.BotaB.At(4));
        }

        // ==================================================================== §32 bites

        /// <summary>BITE A — UN SOLO TIPO GLOBAL haria que apagar A apagara tambien B.</summary>
        [Fact]
        public void Bite_OneGlobalType_BreaksSideIndependence()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(NoneType, BootPlacement.EntryExit),
                SideIntent.Of(BootId, BootPlacement.EntryExit));

            Assert.Equal(0, Of(system, PushBackSide.A));
            Assert.True(Of(system, PushBackSide.B) > 0);   // con un tipo global, esto seria 0
        }

        /// <summary>BITE B — SI «Ninguno» BORRARA la ubicacion, al volver el tipo no habria nada que recuperar.</summary>
        [Fact]
        public void Bite_TypeNoneErasingPlacement_BreaksDormantIntent()
        {
            var dormant = Selection(SideIntent.Of(NoneType, BootPlacement.Both, (2, BootPlacement.Rear)), null);

            Assert.Equal(BootPlacement.Both, dormant.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, dormant.Bota.At(2));

            dormant.Bota.PieceId = BootId;
            Assert.Equal(BootPlacement.Both, dormant.BootPlacementAt(0));
            Assert.Equal(BootPlacement.Rear, dormant.BootPlacementAt(2));
        }

        /// <summary>
        /// BITE C — ESCONDER LA FILA no basta: si la materializacion siguiera leyendo un tipo global, un lado en
        /// «Ninguno» seguiria dibujando. Se comprueba sobre la resolucion fisica, que es quien decide.
        /// </summary>
        [Fact]
        public void Bite_HidingTheRowButResolvingFromTheGlobalType_BreaksMaterialization()
        {
            var system = Resolve(
                State(composite: true),
                SideIntent.Of(NoneType, BootPlacement.Both),
                SideIntent.Of(NoneType, BootPlacement.Both));

            Assert.Empty(Physical(system));
            Assert.Empty(Drawn(system));
            Assert.Equal(0, Bom(system));
        }

        /// <summary>BITE D — CAMBIAR EL TIPO DE A no puede tocar nada de B.</summary>
        [Fact]
        public void Bite_ChangingTypeATouchingB_BreaksIndependence()
        {
            var selection = Selection(
                SideIntent.Of(BootId, BootPlacement.EntryExit),
                SideIntent.Of(OtherBootId, BootPlacement.Both, (1, BootPlacement.Rear)));

            selection.Bota.PieceId = NoneType;
            selection.Bota.Placement = BootPlacement.None;
            selection.Bota.Posts.Clear();

            Assert.Equal(OtherBootId, selection.BotaB.PieceId);
            Assert.Equal(BootPlacement.Both, selection.BotaB.Placement);
            Assert.Equal(BootPlacement.Rear, selection.BotaB.At(1));
        }
    }
}
