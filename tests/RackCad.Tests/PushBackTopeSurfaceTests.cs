using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42, ERROR 10 — INTENCIÓN, APLICABILIDAD y MATERIALIZACIÓN son tres cosas distintas.
    ///
    /// <list type="bullet">
    /// <item>la INTENCIÓN es lo que el usuario guardó por lado, y no se borra nunca;</item>
    /// <item>la APLICABILIDAD es lo que la topología de esa celda admite hoy;</item>
    /// <item>la MATERIALIZACIÓN es la pieza, y solo existe donde las dos coinciden.</item>
    /// </list>
    ///
    /// <para>
    /// La superficie vive en Application (<see cref="PushBackTopeSurface"/>) y no en el code-behind: es la MISMA
    /// respuesta que consumen la ventana, sus pruebas y estas. Calcularla dentro de la ventana habría sido una
    /// segunda autoridad, que es de donde salió este error.
    /// </para>
    /// </summary>
    public class PushBackTopeSurfaceTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackCompositeEditorState State(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots = 2, int levels = 2)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            state.SetDefaults(topology, direction);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var slot = 0; slot < matrix.Count; slot++)
                {
                    state.Of(side).AdjustLevels(slot, levels - matrix.Fronts[slot].LoadLevels);
                }
            }

            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state, bool safety = false)
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            if (safety)
            {
                foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
                {
                    inputs.SafetySelections.Add(selection);
                }
            }

            return new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System;
        }

        // ===================== aplicabilidad por topología ====================================================

        /// <summary>Camas ENCONTRADAS son dos decisiones independientes, y las dos topan en la línea interior.</summary>
        [Fact]
        public void Encontradas_OffersTwoIndependentTopes_AtTheInterface()
        {
            var surface = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB).TopeSurface(0, 0);

            Assert.Equal(PushBackCellTopology.Encontradas, surface.Topology);
            Assert.True(surface.IsIndependentPair);
            Assert.True(surface.AtInterface);
        }

        /// <summary>Una cama de UN lado tiene un solo extremo alto, y mira al centro del rack.</summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA, true, false)]
        [InlineData(PushBackCellTopology.SoloB, false, true)]
        public void ASingleSidedCell_HasOneHighEnd_AtTheInterface(
            PushBackCellTopology topology, bool a, bool b)
        {
            var surface = State(topology, PushBackRunDirection.AToB).TopeSurface(0, 0);

            Assert.Equal(a, surface.AppliesToA);
            Assert.Equal(b, surface.AppliesToB);
            Assert.False(surface.IsIndependentPair);
            Assert.True(surface.AtInterface);
        }

        /// <summary>
        /// Una CORRIDA es UNA sola cama que cruza el rack: solo el lado hacia el que fluye tiene extremo alto, y ese
        /// extremo está en la orilla EXTERIOR, al final del recorrido — no en la interfaz.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB, false, true)]
        [InlineData(PushBackRunDirection.BToA, true, false)]
        public void ACorrida_HasOneHighEnd_AtTheFarOuterLine(
            PushBackRunDirection direction, bool a, bool b)
        {
            var surface = State(PushBackCellTopology.Corrida, direction).TopeSurface(0, 0);

            Assert.Equal(PushBackCellTopology.Corrida, surface.Topology);
            Assert.Equal(direction, surface.Direction);
            Assert.Equal(a, surface.AppliesToA);
            Assert.Equal(b, surface.AppliesToB);
            Assert.False(surface.AtInterface);
        }

        /// <summary>
        /// Y lo que la superficie promete se cumple en el DIBUJO: el extremo alto de la cama cae en la interfaz o en
        /// la orilla exterior, según lo que dijo. Sin esto la superficie sería una explicación bonita y no un
        /// contrato.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.SoloA, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.SoloB, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.BToA)]
        public void WhatTheSurfacePromises_IsWhereTheBedActuallyEnds(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var state = State(topology, direction);
            var system = Build(state);
            var surface = state.TopeSurface(0, 0);
            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                .Where(axis => axis.Slot == 0 && axis.Level == 1)
                .ToList();
            Assert.NotEmpty(axes);

            var total = system.Structure.TotalLength;
            var interior = total / 2.0;
            foreach (var axis in axes)
            {
                var atOuter = Math.Min(axis.HighContact.X, total - axis.HighContact.X) < 12.0;
                Assert.Equal(!surface.AtInterface, atOuter);
                if (surface.AtInterface)
                {
                    Assert.True(Math.Abs(axis.HighContact.X - interior) < 12.0);
                }
            }
        }

        // ===================== la intención no se borra =======================================================

        /// <summary>
        /// Cambiar el SENTIDO mueve la efectividad de un lado al otro y NO toca la intención guardada. Es la
        /// pregunta 7 del dueño, aquí sobre el modelo puro.
        /// </summary>
        [Fact]
        public void ChangingTheDirection_MovesEffectiveness_AndKeepsBothIntents()
        {
            var state = State(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, true, DynamicRackCellScope.All);

            Assert.False(state.TopeSurface(0, 0).AppliesToA);
            Assert.True(state.TopeSurface(0, 0).AppliesToB);

            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);

            Assert.True(state.TopeSurface(0, 0).AppliesToA);
            Assert.False(state.TopeSurface(0, 0).AppliesToB);
            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 0));
            Assert.True(state.RearTopeAt(PushBackSide.B, 0, 0));
        }

        /// <summary>
        /// Y la intención DORMANTE no se cotiza ni se dibuja: el BOM cuenta un tope por cama con intención activa en
        /// su lado ALTO, así que una corrida con las dos intenciones puestas produce UNO por cama, no dos.
        /// </summary>
        [Fact]
        public void ADormantIntent_IsNeitherDrawnNorQuoted()
        {
            var state = State(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, slots: 2, levels: 2);
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, true, DynamicRackCellScope.All);
            var system = Build(state);

            var beds = PushBackRuns.Resolve(system).Runs.Count;
            var quoted = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(c => string.Equals(c.Category, PushBackBomBuilder.RearTope, StringComparison.Ordinal))
                .Sum(c => c.Quantity);

            Assert.True(beds > 0);
            Assert.Equal(beds, quoted);
        }

        // ===================== sección P: la seguridad no se mueve por editar topes ===========================

        /// <summary>
        /// SECCIÓN P del dueño. El error 10 toca una zona vecina a la seguridad, así que se fija que editar topes no
        /// mueve ni una pieza de seguridad, que ninguna selección adquiere <c>Side = Both</c> —el modelado que causó
        /// los errores 2 y 3— y que los protectores siguen solo en las dos líneas de orilla.
        /// </summary>
        [Fact]
        public void EditingTopes_NeverDisturbsSafety()
        {
            var state = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 3, levels: 2);

            string Signature(PushBackSystem system) => string.Join(
                "\n",
                new PushBackSystemPlantaBuilder().Build(system, Catalog)
                    .Where(i => i.Role == HeaderBlockRole.Safety)
                    .Select(i => FormattableString.Invariant(
                        $"{i.PieceId}|{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{i.MirroredX}"))
                    .OrderBy(line => line, StringComparer.Ordinal));

            var before = Signature(Build(state, safety: true));

            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, false, DynamicRackCellScope.All);
            var system = Build(state, safety: true);

            Assert.Equal(before, Signature(system));
            Assert.All(
                system.Structure.SafetySelections,
                selection => Assert.NotEqual(SafetySide.Both, selection.Side));
        }

        // ===================== sección Q: el error 1, revalidado tras la inversión vertical ===================

        /// <summary>
        /// SECCIÓN Q. Con A y B idénticos, las camas de los dos lados quedan a la MISMA altura después de invertir
        /// la autoridad vertical: la reflexión cambia X y orientación e invierte la pendiente, pero no introduce
        /// ningún desplazamiento en Z. Y en cuanto se toca solo B, sí difieren.
        /// </summary>
        [Fact]
        public void IdenticalSides_StillShareTheirElevations_AfterTheVerticalInversion()
        {
            var state = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 2, levels: 3);
            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(Build(state)), Catalog)
                .Where(axis => axis.Slot == 0)
                .ToList();

            IReadOnlyList<(double Low, double High)> Of(bool forward) => axes
                .Where(axis => axis.FlowsForward == forward)
                .OrderBy(axis => axis.Level)
                .Select(axis => (Math.Round(axis.LowContact.Y, 6), Math.Round(axis.HighContact.Y, 6)))
                .ToList();

            Assert.NotEmpty(Of(true));
            Assert.Equal(Of(true), Of(false));

            // Y en cuanto B deja de ser igual, dejan de coincidir: la prueba anterior no pasa por casualidad.
            state.Of(PushBackSide.B).Structure.Fronts[0].FirstLevelHeight = 24.0;
            var moved = PushBackRunGeometry.Axes(PushBackRuns.Resolve(Build(state)), Catalog)
                .Where(axis => axis.Slot == 0)
                .ToList();
            Assert.NotEqual(
                moved.Where(a => a.FlowsForward).Select(a => Math.Round(a.LowContact.Y, 6)).ToList(),
                moved.Where(a => !a.FlowsForward).Select(a => Math.Round(a.LowContact.Y, 6)).ToList());
        }
    }
}
