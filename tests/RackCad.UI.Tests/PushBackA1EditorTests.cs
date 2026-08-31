using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A1) — LA VENTANA: lo que se recupera al reabrir y lo que NO puede contaminarse al restaurar.
    /// </summary>
    public sealed class PushBackA1EditorTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DefenseId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;

        private static RackPushBackSystemWindow Shown()
        {
            var w = new RackPushBackSystemWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                Left = -10000,
                Top = -10000,
            };
            w.Show();
            w.UpdateLayout();
            return w;
        }

        /// <summary>Un rack compuesto con los dos lados poblados, tal y como lo guardaria el editor.</summary>
        private static PushBackDesign CompositeDesign(string defenseA, string defenseB, int slots = 3)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).Design;
            design.DefensePieceId = defenseA;
            if (design.SideB != null)
            {
                design.SideB.DefensePieceId = defenseB;
            }

            return design;
        }

        /// <summary>
        /// B4 — el TIPO DE DEFENSA del lado A vuelve entero por RACKEDITAR. La copia del lado A que la ventana usa
        /// para cargar omitia ese campo, asi que un «Ninguno» explicito resucitaba —o una pieza distinta se perdia—
        /// mientras el lado B, que si lo recuperaba, quedaba bien.
        /// </summary>
        [Fact]
        public void DefenseTypeA_RackEditarRoundTripsThroughLoadExisting()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    w.LoadExisting(
                        CompositeDesign(PushBackDefaults.NonePieceId, DefenseId), "GUID-A1-B4", "PB");

                    Assert.Equal(PushBackDefaults.NonePieceId, w.CompositeState.Of(PushBackSide.A).DefensePieceId);
                    Assert.Equal(DefenseId, w.CompositeState.Of(PushBackSide.B).DefensePieceId);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// B5 — RESTAURAR reconstruye el rack por el mismo camino que abrirlo. Antes cargaba el diseño compuesto
        /// ENTERO —A + hueco + B— dentro del estado del lado activo: la profundidad casi se duplicaba, aparecian
        /// modulos del otro lado dentro de A y la configuracion de B se perdia.
        /// </summary>
        [Fact]
        public void CompositeRestore_PreservesAAndB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    w.LoadExisting(CompositeDesign(DefenseId, PushBackDefaults.NonePieceId), "GUID-A1-B5", "PB");
                    var before = Snapshot(w);

                    EditorWindowTestSupport.ClickNamed(w, "RestoreButton");

                    Assert.Equal(before, Snapshot(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y en particular no mete el hueco ni los modulos del lado B dentro de la matriz del lado A.</summary>
        [Fact]
        public void CompositeRestore_DoesNotLoadGapOrBSideIntoA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    w.LoadExisting(CompositeDesign(DefenseId, DefenseId), "GUID-A1-B5B", "PB");
                    var depthA = w.CompositeState.Of(PushBackSide.A).Structure.Count;
                    var slots = w.CompositeState.SlotCount;

                    EditorWindowTestSupport.ClickNamed(w, "RestoreButton");

                    Assert.Equal(depthA, w.CompositeState.Of(PushBackSide.A).Structure.Count);
                    Assert.Equal(slots, w.CompositeState.SlotCount);
                    Assert.True(w.CompositeState.SideBPresent);
                    Assert.NotNull(w.LastComputation?.System);
                    Assert.True(w.LastComputation.System.IsComposite);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Lo que un restaurar no puede cambiar: la separacion A/B y lo que cada lado tiene.</summary>
        private static string Snapshot(RackPushBackSystemWindow w)
            => string.Join("|", new[]
            {
                "B=" + w.CompositeState.SideBPresent,
                "slots=" + w.CompositeState.SlotCount,
                "A=" + w.CompositeState.Of(PushBackSide.A).Structure.Count,
                "B=" + w.CompositeState.Of(PushBackSide.B).Structure.Count,
                "defA=" + w.CompositeState.Of(PushBackSide.A).DefensePieceId,
                "defB=" + w.CompositeState.Of(PushBackSide.B).DefensePieceId,
                "gap=" + w.CompositeState.Gap.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            });

        /// <summary>Y al reves: A con pieza y B sin ella.</summary>
        [Fact]
        public void DefenseTypeB_RackEditarRoundTripsThroughLoadExisting()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    w.LoadExisting(
                        CompositeDesign(DefenseId, PushBackDefaults.NonePieceId), "GUID-A1-B4B", "PB");

                    Assert.Equal(DefenseId, w.CompositeState.Of(PushBackSide.A).DefensePieceId);
                    Assert.Equal(PushBackDefaults.NonePieceId, w.CompositeState.Of(PushBackSide.B).DefensePieceId);
                }
                finally { w.Close(); }
            });
        }
    }
}
