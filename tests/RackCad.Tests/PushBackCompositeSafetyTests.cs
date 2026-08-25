using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 — la SEGURIDAD de un rack compuesto.
    ///
    /// <para>
    /// DECISION DEL DUEÑO: un Push Back compuesto son, fisicamente, dos Push Back opuestos. Tiene DOS pasillos de
    /// carga y los dos son extremos BAJOS, asi que los dos llevan su seguridad POR DEFECTO. No hay que activarla a
    /// mano para el segundo lado. Un rack de un sentido sigue teniendo un solo pasillo y no cambia en nada.
    /// </para>
    /// <para>
    /// La seguridad ordinaria —botas, protectores, desviadores, defensa— es del RACK y vive en una sola autoridad
    /// (<see cref="PushBackSafetyAuthority"/>); lo que I-42 añade es en cuantos EXTREMOS se materializa. El TOPE
    /// posterior es otra cosa: pertenece al extremo ALTO y tiene una autoridad POR LADO, que no se toca aqui.
    /// </para>
    /// </summary>
    public class PushBackCompositeSafetyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        private static PushBackCompositeEditorState State(bool sideB, PushBackCellTopology topology)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            if (sideB)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(2);
                state.SetDefaults(topology, PushBackRunDirection.AToB);
            }

            return state;
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>Las piezas de seguridad DIBUJADAS, agrupadas por la mitad del rack en la que caen.</summary>
        private static (int Near, int Far) DrawnSafety(PushBackSystem system)
        {
            var instances = new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety)
                .ToList();

            var middle = system.Structure.TotalLength / 2.0;
            return (instances.Count(i => i.Insertion.X < middle), instances.Count(i => i.Insertion.X >= middle));
        }

        // ================= K: un rack compuesto nace con seguridad en LOS DOS pasillos ==========================

        [Fact]
        public void ANewCompositeRack_HasSafetyOnBothAisles()
        {
            var single = DrawnSafety(Resolve(State(sideB: false, PushBackCellTopology.SoloA)));
            Assert.True(single.Near > 0, "un rack de un sentido lleva su seguridad, como siempre");
            Assert.Equal(0, single.Far);

            var composite = DrawnSafety(Resolve(State(sideB: true, PushBackCellTopology.Encontradas)));
            Assert.True(composite.Near > 0, "el pasillo de A sigue llevando la suya");
            Assert.True(composite.Far > 0, "y el pasillo de B tambien, sin pedirlo");
        }

        /// <summary>
        /// La regla se lee de las CAMAS: son ellas las que dicen por que extremo se carga. Con todas las celdas en
        /// Solo A no hay segundo pasillo, y con todas en Solo B el pasillo es el otro.
        /// </summary>
        /// <summary>
        /// Los dos pasillos existen por CONSTRUCCION, sea cual sea la topologia: la seguridad protege la cara de
        /// carga, y que hoy una mitad no tenga camas no la pone a salvo de un montacargas.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA)]
        [InlineData(PushBackCellTopology.SoloB)]
        [InlineData(PushBackCellTopology.Encontradas)]
        [InlineData(PushBackCellTopology.Corrida)]
        public void BothAisles_AreProtected_InEveryTopology(PushBackCellTopology topology)
        {
            var system = Resolve(State(sideB: true, topology));
            Assert.Equal(PushBackSafetyAisles.Both, PushBackResolver.AislesOf(system));

            var drawn = DrawnSafety(system);
            Assert.True(drawn.Near > 0, "el pasillo de A");
            Assert.True(drawn.Far > 0, "el pasillo de B");
        }

        /// <summary>
        /// El BOM cuenta las piezas de LOS DOS pasillos: la seguridad de un rack compuesto es mas cara que la de uno
        /// de un sentido porque fisicamente hay el doble de pasillo que proteger.
        /// </summary>
        [Fact]
        public void TheBom_CountsTheSafetyOfBothAisles()
        {
            var single = Quantity(Resolve(State(sideB: false, PushBackCellTopology.SoloA)));
            var composite = Quantity(Resolve(State(sideB: true, PushBackCellTopology.Encontradas)));

            Assert.True(single > 0);
            Assert.True(composite > single, "dos pasillos llevan mas seguridad que uno: " + composite + " vs " + single);
        }

        private static int Quantity(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SelectiveBomBuilder.Safety)
                .Sum(component => component.Quantity);

        // ================= W: el TOPE posterior es OTRA autoridad, y tambien nace encendido ====================

        /// <summary>
        /// El tope posterior no es «seguridad ordinaria»: pertenece al extremo ALTO y su autoridad es POR LADO. Un
        /// rack compuesto nuevo nace con los DOS encendidos, que es lo que tendrian dos Push Back independientes.
        /// </summary>
        [Fact]
        public void ANewCompositeRack_HasBothRearTopes_On()
        {
            var state = State(sideB: true, PushBackCellTopology.Encontradas);

            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 0));
            Assert.True(state.RearTopeAt(PushBackSide.B, 0, 0));

            var system = Resolve(state);
            var runs = PushBackRuns.Resolve(system);
            var topes = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == PushBackBomBuilder.RearTope)
                .Sum(component => component.Quantity);

            Assert.Equal(runs.Runs.Count, topes);
        }

        /// <summary>
        /// Y el default del lado B es el del PRODUCTO, no una copia de lo que el usuario ya hubiera cambiado en A.
        /// Apagar los topes de A y despues declarar B tiene que dar B encendido.
        /// </summary>
        [Fact]
        public void TheDefaultsOfANewSide_AreTheProductDefaults_NotACopyOfTheOther()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SideA.Structure.ToggleCell(0, 0, extendSelection: false);
            state.ApplyRearTope(PushBackSide.A, false, DynamicRackCellScope.All);
            Assert.False(state.RearTopeAt(PushBackSide.A, 0, 0));

            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);

            Assert.True(state.RearTopeAt(PushBackSide.B, 0, 0), "B nace con el default del producto, no con lo de A");
            Assert.False(state.RearTopeAt(PushBackSide.A, 0, 0), "y lo de A se conserva tal cual");
        }

        // ================= L: la intencion sobrevive a los cambios de topologia ================================

        [Fact]
        public void TurningOffOnlySideA_LeavesSideBOn_ThroughATopologyRoundTrip()
        {
            var state = State(sideB: true, PushBackCellTopology.Encontradas);
            state.ApplyRearTope(PushBackSide.A, false, DynamicRackCellScope.All);

            Assert.False(state.RearTopeAt(PushBackSide.A, 0, 0));
            Assert.True(state.RearTopeAt(PushBackSide.B, 0, 0));

            state.ApplyTopology(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, DynamicRackCellScope.All);
            state.ApplyTopology(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, DynamicRackCellScope.All);
            state.ApplyTopology(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, DynamicRackCellScope.All);

            Assert.False(state.RearTopeAt(PushBackSide.A, 0, 0));
            Assert.True(state.RearTopeAt(PushBackSide.B, 0, 0));

            var runs = PushBackRuns.Resolve(Resolve(state));
            var expected = runs.Runs.Count(run => run.HighSide == PushBackSide.B);
            var quoted = PushBackBomBuilder.Build(Resolve(state), Catalog).Components
                .Where(component => component.Category == PushBackBomBuilder.RearTope)
                .Sum(component => component.Quantity);

            Assert.Equal(expected, quoted);
        }

        // ================= El rack de un sentido no cambia =====================================================

        /// <summary>
        /// GUARDA legacy: la seguridad de un Push Back de un sentido sigue restringida al extremo bajo, pieza por
        /// pieza. Nada de esta ronda puede haberla movido.
        /// </summary>
        [Fact]
        public void ASingleSidedRack_KeepsItsSafetyAtTheLowEndOnly()
        {
            var system = Resolve(State(sideB: false, PushBackCellTopology.SoloA));

            Assert.Equal(PushBackSafetyAisles.NearOnly, PushBackResolver.AislesOf(system));
            Assert.NotEmpty(system.SafetySelections);
            Assert.All(system.SafetySelections, selection =>
            {
                Assert.True(selection.LowEndOnly);
                Assert.NotEqual(SafetySide.Right, selection.Side);
            });
        }
    }
}
