using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-41 · PB-016 — COLOCACION VISUAL de la tarima Push Back, tras la validacion manual del Owner.
    ///
    /// El Owner reporto dos defectos, ambos de REPRESENTACION (el contrato de I-41 no cambia):
    /// <list type="number">
    /// <item><b>Lateral</b>: las tarimas se veian ESCALONADAS. Cada una se dibujaba horizontal y a una altura
    /// distinta, de modo que una calle inclinada producia una escalera. Ademas se apoyaban en la linea del ORIGEN
    /// del bloque de la cama, que es donde se atornilla el riel — no donde descansa la carga. La tarima descansa
    /// sobre los RODILLOS: <c>Y de apoyo = origen del rodillo + radio del rodillo</c>.</item>
    /// <item><b>Frontal y posterior</b>: la tarima no quedaba alineada con su calle. Se repartia con huecos
    /// IGUALES a lo largo del larguero, y las calles reales no estan repartidas asi: cada una mide BFR
    /// (frente de tarima + 2") y el larguero anade 6" que se reparten a los dos extremos.</item>
    /// </list>
    ///
    /// Estas pruebas FALLAN con la colocacion anterior y pasan con la corregida.
    /// </summary>
    public class PushBackTarimaPlacementTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>El radio del rodillo que la cama Push Back usa (la cama fija <see cref="FlowBedDefaults.RollerId"/>).</summary>
        private static double RollerRadius(RackCatalog catalog)
            => (catalog.FlowBedProfiles.First(entry => string.Equals(
                entry.Id, FlowBedDefaults.RollerId, StringComparison.OrdinalIgnoreCase)).Diameter) / 2.0;

        private static (PushBackEditorState State, PushBackEditorInputs Inputs, PushBackEditorDesignAssembler Assembler)
            Editor(int fronts = 1, int levels = 2, int palletsDeep = 4, int lanes = 1)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.PalletsDeep = palletsDeep;
            state.SetFrontCount(fronts);
            for (var front = 0; front < fronts; front++)
            {
                state.Structure.Fronts[front].LoadLevels = levels;
                state.Structure.Fronts[front].PalletsDeep = palletsDeep;
                state.Structure.Fronts[front].PalletCount = lanes;
                state.AdjustLevels(front, 0);
            }

            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.All);
            return (state, inputs, assembler);
        }

        private static PushBackEditorComputation Build(int fronts = 1, int levels = 2, int palletsDeep = 4, int lanes = 1)
        {
            var (state, inputs, assembler) = Editor(fronts, levels, palletsDeep, lanes);
            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation;
        }

        private static List<HeaderBlockInstance> Pallets(HeaderRunPlan plan)
            => plan.Flatten().Instances.Where(i => i.Role == HeaderBlockRole.Pallet).ToList();

        // ================= LATERAL: pendiente real y tangencia al rodillo =================

        [Fact]
        public void Lateral_EveryPalletCarriesTheBedRotation_SoItIsNotDrawnHorizontal()
        {
            var computation = Build(palletsDeep: 5);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var axes = PushBackFlowBedGeometry.Resolve(system, Catalog, front).ToList();

            var pallets = PushBackTarimaPlacement.Lateral(system, Catalog, front);

            Assert.NotEmpty(pallets);
            // La cama sube: su rotacion NO es cero, y cada tarima la lleva.
            Assert.All(axes, axis => Assert.True(Math.Abs(axis.RotationRadians) > 1e-6));
            Assert.All(pallets, pallet => Assert.Contains(
                axes,
                axis => Math.Abs(axis.RotationRadians - pallet.RotationRadians) < 1e-9));
        }

        [Fact]
        public void Lateral_ThePalletRestsOnTheROLLERLine_NotOnTheBedOriginLine()
        {
            var computation = Build(palletsDeep: 5, levels: 1);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var axis = PushBackFlowBedGeometry.Resolve(system, Catalog, front).Single();

            var pallets = PushBackTarimaPlacement.Lateral(system, Catalog, front);
            Assert.NotEmpty(pallets);

            // Distancia PERPENDICULAR de cada tarima a la linea del origen del bloque de la cama. Antes del fix era
            // CERO (se apoyaban en esa linea); ahora debe ser exactamente la altura local de apoyo: la Y del
            // troquel del riel donde se insertan los rodillos, mas el RADIO del rodillo.
            var railTope = CatalogLookup.Local(
                Catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailTopePoint, FlowBedDefaults.View);
            var expected = railTope.Y + RollerRadius(Catalog);
            Assert.True(Math.Abs(expected) > 1e-6, "la altura de apoyo local no puede ser cero");

            foreach (var pallet in pallets)
            {
                var perpendicular = PushBackBedRotation.PerpendicularDistanceToOriginLine(
                    pallet.Insertion, axis.ExitMate, axis.RailLocalMate, axis.RotationRadians);
                // El signo del convenio es negativo hacia arriba de la linea; se compara la magnitud con signo.
                Assert.Equal(-expected, perpendicular, 6);
            }
        }

        [Fact]
        public void Lateral_ThePalletIsTangentToTheActualRollers_OfItsOwnBed()
        {
            var computation = Build(palletsDeep: 5, levels: 1);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var axis = PushBackFlowBedGeometry.Resolve(system, Catalog, front).Single();
            var radius = RollerRadius(Catalog);

            // Los rodillos REALES de esta cama, ya colocados en mundo por el plan lateral.
            var corte = new PushBackSystemLateralBuilder().Build(system, Catalog, postIndex: 0);
            var rollers = corte.Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Roller)
                .ToList();
            Assert.NotEmpty(rollers);

            var pallets = PushBackTarimaPlacement.Lateral(system, Catalog, front);
            var pallet = pallets[0];

            // La cara inferior de la tarima es la recta que pasa por su insercion con la rotacion de la cama.
            double PalletBottomYAt(double x)
                => pallet.Insertion.Y + (x - pallet.Insertion.X) * Math.Tan(pallet.RotationRadians);

            // Cada rodillo toca esa recta por su parte ALTA: centro + radio, medido perpendicularmente.
            foreach (var roller in rollers)
            {
                var top = roller.Insertion.Y + radius * Math.Cos(axis.RotationRadians);
                var atRoller = PalletBottomYAt(roller.Insertion.X - radius * Math.Sin(axis.RotationRadians));
                Assert.Equal(top, atRoller, 4);
            }
        }

        [Fact]
        public void Lateral_ConsecutivePalletsClimb_ByTheBedSlope_NotByAStep()
        {
            var computation = Build(palletsDeep: 6, levels: 1);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var axis = PushBackFlowBedGeometry.Resolve(system, Catalog, front).Single();

            var pallets = PushBackTarimaPlacement.Lateral(system, Catalog, front)
                .OrderBy(pallet => pallet.Insertion.X)
                .ToList();
            Assert.True(pallets.Count >= 2);

            for (var index = 1; index < pallets.Count; index++)
            {
                var dx = pallets[index].Insertion.X - pallets[index - 1].Insertion.X;
                var dy = pallets[index].Insertion.Y - pallets[index - 1].Insertion.Y;
                // Estan sobre la MISMA recta inclinada: el cociente es exactamente la pendiente de la cama.
                Assert.Equal(Math.Tan(axis.RotationRadians), dy / dx, 9);
            }
        }

        [Fact]
        public void Lateral_ThePalletNeverSinksIntoTheRail_ItSitsAboveTheOriginLine()
        {
            var computation = Build(palletsDeep: 4, levels: 2);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var axes = PushBackFlowBedGeometry.Resolve(system, Catalog, front).ToList();
            var supportLocalY = PushBackTarimaPlacement.SupportLocalY(Catalog);

            // El troquel del riel esta POR ENCIMA del origen del bloque (2.75") y el rodillo suma su radio, asi que la
            // superficie de apoyo queda necesariamente sobre la linea del origen.
            Assert.True(supportLocalY > 0.0, "la superficie de apoyo debe estar sobre el datum del riel");

            var pallets = PushBackTarimaPlacement.Lateral(system, Catalog, front);
            Assert.NotEmpty(pallets);
            foreach (var pallet in pallets)
            {
                // Cada tarima pertenece al eje cuya linea de APOYO pasa por ella. Emparejar por rotacion no sirve:
                // dos niveles pueden compartir rotacion y estar separados un nivel entero en altura.
                var axis = axes
                    .OrderBy(candidate => Math.Abs(
                        PushBackTarimaPlacement.SupportYAt(candidate, supportLocalY, pallet.Insertion.X)
                        - pallet.Insertion.Y))
                    .First();
                Assert.Equal(
                    PushBackTarimaPlacement.SupportYAt(axis, supportLocalY, pallet.Insertion.X),
                    pallet.Insertion.Y, 6);
                Assert.True(
                    pallet.Insertion.Y > axis.RailOriginYAt(pallet.Insertion.X) + 1e-6,
                    "la tarima debe quedar POR ENCIMA de la linea del origen del riel, sobre los rodillos");
            }
        }

        // ================= FRONTAL / POSTERIOR: alineacion con la calle =================

        /// <summary>La X del larguero de un frente en un corte frontal (su columna: poste + troquel).</summary>
        private static double BeamAnchorX(PushBackSystem system, RackCatalog catalog, int frontIndex)
        {
            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            return layout.PostPositions[frontIndex] + layout.TroquelPositions[frontIndex];
        }

        [Fact]
        public void Frontal_EachPalletIsCentredOnItsOwnBfrLane_NotOnAnEvenGapSplit()
        {
            const int lanes = 3;
            var computation = Build(levels: 1, lanes: lanes);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var cell = DynamicRackLevelGeometry.At(system.Structure, front, 1);

            var bfr = cell.Bfr;
            var beamLength = PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1);
            var anchorX = BeamAnchorX(system, Catalog, 0);
            // Las calles van CENTRADAS en el larguero: los 6" de holgura se reparten a los dos extremos.
            var margin = (beamLength - lanes * bfr) / 2.0;

            var pallets = Pallets(new PushBackSystemFrontalBuilder()
                    .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida))
                .OrderBy(pallet => pallet.Insertion.X)
                .ToList();

            Assert.Equal(lanes, pallets.Count);
            for (var lane = 0; lane < lanes; lane++)
            {
                var expectedCentre = anchorX + margin + (lane + 0.5) * bfr;
                Assert.Equal(expectedCentre, pallets[lane].Insertion.X, 6);
            }
        }

        [Fact]
        public void Frontal_ThePalletRestsOnTheRollerSupport_NotOnTheBeamBedMate()
        {
            var computation = Build(palletsDeep: 5, levels: 1);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var elevation = PushBackElevations.Resolve(system, Catalog, front)[1];

            var pallet = Assert.Single(Pallets(new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida)));

            // Antes del fix la tarima se apoyaba EXACTAMENTE en el contacto del larguero con la cama, que es donde
            // se atornilla el riel — no donde descansa la carga. Ahora debe estar mas alta, por los rodillos.
            Assert.True(pallet.Insertion.Y > elevation.LowContact.Y + 1e-6,
                "la tarima frontal debe apoyarse sobre los rodillos, no en el troquel del larguero");
        }

        [Fact]
        public void FrontalAndRear_ShareTheirLaneColumns_AndTheRearSitsHigherByTheBedRise()
        {
            var computation = Build(palletsDeep: 5, levels: 1, lanes: 2);
            var system = computation.System;
            var builder = new PushBackSystemFrontalBuilder();

            var baja = Pallets(builder.BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida))
                .OrderBy(pallet => pallet.Insertion.X).ToList();
            var posterior = Pallets(builder.BuildPlan(system, Catalog, PushBackFrontalEnd.Posterior))
                .OrderBy(pallet => pallet.Insertion.X).ToList();

            Assert.Equal(2, baja.Count);
            Assert.Equal(2, posterior.Count);

            // MISMA columna en los dos cortes: es la misma calle vista desde los dos extremos.
            for (var lane = 0; lane < 2; lane++)
            {
                Assert.Equal(baja[lane].Insertion.X, posterior[lane].Insertion.X, 6);
            }

            // Y el extremo posterior queda MAS ALTO, porque la cama sube hacia el fondo. La subida es la de la
            // superficie de apoyo entre las dos X, que es UNA sola recta.
            //
            // Deliberadamente NO se compara con `RearContact.Y - LowContact.Y`: esos dos contactos viven en rectas
            // PARALELAS distintas —el bajo en la linea de TROQUEL_IN, el posterior en la del ORIGEN— separadas por la
            // componente perpendicular del mate (la leccion de I-32). Su diferencia de Y no es la subida de la cama.
            var front = system.Structure.Fronts[0];
            var elevation = PushBackElevations.Resolve(system, Catalog, front)[1];
            var axis = PushBackFlowBedGeometry.Resolve(system, Catalog, front).Single();
            var rise = (elevation.RearContact.X - elevation.LowContact.X) * Math.Tan(axis.RotationRadians);

            Assert.True(rise > 0.0);
            Assert.Equal(rise, posterior[0].Insertion.Y - baja[0].Insertion.Y, 6);
            Assert.NotEqual(
                elevation.RearContact.Y - elevation.LowContact.Y,
                posterior[0].Insertion.Y - baja[0].Insertion.Y, 3);
        }

        [Fact]
        public void Frontal_ThePalletKeepsItsOwnFrenteAndAlturaParameters()
        {
            var computation = Build(levels: 1, lanes: 2);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var cell = DynamicRackLevelGeometry.At(system.Structure, front, 1);

            foreach (var pallet in Pallets(new PushBackSystemFrontalBuilder()
                         .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida)))
            {
                Assert.Equal(cell.Pallet.Front, pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam], 6);
                Assert.Equal(cell.Pallet.Height, pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam], 6);
                // El corte frontal es transversal: la pendiente de la cama es perpendicular al plano, sin rotacion.
                Assert.Equal(0.0, pallet.RotationRadians, 9);
            }
        }

        [Fact]
        public void Frontal_ThePalletStaysInsideItsBeam_WithoutOverflowingTheEnds()
        {
            const int lanes = 3;
            var computation = Build(levels: 1, lanes: lanes);
            var system = computation.System;
            var front = system.Structure.Fronts[0];
            var beamLength = PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1);
            var anchorX = BeamAnchorX(system, Catalog, 0);
            var palletFront = DynamicRackLevelGeometry.At(system.Structure, front, 1).Pallet.Front;

            foreach (var pallet in Pallets(new PushBackSystemFrontalBuilder()
                         .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida)))
            {
                var left = pallet.Insertion.X - palletFront / 2.0;
                var right = pallet.Insertion.X + palletFront / 2.0;
                Assert.True(left >= anchorX - 1e-6, "la tarima no puede salirse por el extremo izquierdo del larguero");
                Assert.True(right <= anchorX + beamLength + 1e-6, "ni por el derecho");
            }
        }
    }
}
