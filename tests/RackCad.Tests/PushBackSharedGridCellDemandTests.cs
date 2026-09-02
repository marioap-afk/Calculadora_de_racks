using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A4-GRID, contrato del dueño) — LA ENVOLVENTE TRANSVERSAL COMPARTIDA ALCANZA TAMBIEN A LOS NIVELES CON
    /// OVERRIDE DE CELDA.
    ///
    /// <para>
    /// A y B comparten el ancho de bahia y por tanto sus columnas: para el mismo rack,
    /// <c>PostPositions(compuesta) == PostPositions(local A) == PostPositions(local B)</c>, venga la demanda de
    /// donde venga. G1/G2 dejaron eso cerrado para las calles, el override de frente y un override de celda
    /// SUELTO, pero la envolvente viajaba como override de FRENTE y la celda tiene precedencia sobre el frente:
    /// un lado con TODOS sus niveles resueltos personalizados no la veia.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio.</b> Con el lado A en 90"/95" por celda y el lado B pidiendo 125", la compuesta
    /// ponia su segunda columna en 128.494 y el marco local de A en 98.494; con tres niveles (90/110/100) contra
    /// 125, en 113.494. El caso simetrico —B capado y A gobernando— fallaba igual, asi que no era una asimetria de
    /// un lado.
    /// </para>
    ///
    /// <para>
    /// La correccion no cambia ninguna precedencia authored: la envolvente viaja ademas como SUELO fisico de la
    /// bahia, y la demanda de cada nivel no puede quedar por debajo de el. El override de la celda sigue siendo
    /// intencion local, no un limite superior del rack, y no se materializa en ninguna celda.
    /// </para>
    /// </summary>
    public class PushBackSharedGridCellDemandTests
    {
        private const string Defense = "DEFENSA_MONTACARGAS";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static void Cells(DynamicRackFrontDesign front, params double?[] overrides)
        {
            front.Levels.Clear();
            foreach (var value in overrides)
            {
                front.Levels.Add(new DynamicRackLevelDesign { BeamLengthOverride = value });
            }
        }

        /// <summary>Un compuesto con los overrides de celda y de frente que cada caso necesita.</summary>
        private static PushBackDesign Design(
            double?[] cellsA,
            double?[] cellsB,
            double? frontA = null,
            double? frontB = null,
            int levels = 2,
            bool safety = false)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: levels, levelsB: levels, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            Cells(design.Structure.Fronts[0], cellsA);
            Cells(design.SideB.Fronts[0], cellsB);
            design.Structure.Fronts[0].BeamLengthOverride = frontA;
            design.SideB.Fronts[0].BeamLengthOverride = frontB;
            if (safety)
            {
                foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
                {
                    design.Structure.SafetySelections.Add(selection);
                }

                design.DefensePieceId = Defense;
                design.SideB.DefensePieceId = Defense;
            }

            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design)
            => new PushBackResolver(Catalog).Resolve(design);

        private static IReadOnlyList<double> Posts(DynamicRackSystem structure)
            => structure == null
                ? Array.Empty<double>()
                : DynamicFrontGeometry.Compute(structure, Catalog).PostPositions
                    .Select(x => Math.Round(x, 6))
                    .ToList();

        private static DynamicRackSystem Local(PushBackSystem system, PushBackSide side)
            => system.Composite.Of(side).Local.Structure;

        /// <summary>El contrato: una sola retícula fisica para el rack y sus dos marcos locales.</summary>
        private static void AssertOneGrid(PushBackSystem system)
        {
            var composite = Posts(system.Structure);
            Assert.NotEmpty(composite);
            Assert.Equal(composite, Posts(Local(system, PushBackSide.A)));
            Assert.Equal(composite, Posts(Local(system, PushBackSide.B)));
        }

        private static double BayWidth(PushBackSystem system)
            => system.Structure.Fronts[0].BeamLength;

        // ---------------------------------------------------------------- el hallazgo

        [Fact]
        public void CompositeGrid_AllCellOverridesOnOneSideStillUseSharedEnvelope()
        {
            var design = Design(new double?[] { 90.0, 95.0 }, new double?[] { null, null }, frontB: 125.0);
            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(125.0, BayWidth(system), 6);
            Assert.Equal(125.0, Local(system, PushBackSide.A).Fronts[0].BeamLength, 6);

            // Y la intencion del lado A sigue siendo la suya: 90 y 95, sin materializar la envolvente.
            Assert.Equal(new double?[] { 90.0, 95.0 }, design.Structure.Fronts[0].Levels.Select(l => l.BeamLengthOverride));
        }

        [Fact]
        public void CompositeGrid_OppositeSideCanGovernOverAllCellOverrides()
        {
            // El simetrico: ahora es B quien tiene todos sus niveles personalizados y A quien gobierna.
            var design = Design(new double?[] { null, null }, new double?[] { 90.0, 95.0 }, frontA: 125.0);
            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(125.0, BayWidth(system), 6);
            Assert.Equal(125.0, Local(system, PushBackSide.B).Fronts[0].BeamLength, 6);
            Assert.Equal(new double?[] { 90.0, 95.0 }, design.SideB.Fronts[0].Levels.Select(l => l.BeamLengthOverride));
        }

        [Fact]
        public void CompositeGrid_CellOverridesOnBothSidesUseTheLargestDemand()
        {
            var system = Resolve(Design(new double?[] { 90.0, 95.0 }, new double?[] { 125.0, 130.0 }));

            AssertOneGrid(system);
            Assert.Equal(130.0, BayWidth(system), 6);
        }

        [Fact]
        public void CompositeGrid_SingleCellOverrideBehaviorRemainsCorrect()
        {
            // G2: con un nivel SIN override, la envolvente ya llegaba por el override de frente. Sigue igual.
            var system = Resolve(Design(new double?[] { 90.0, null }, new double?[] { null, null }, frontB: 125.0));

            AssertOneGrid(system);
            Assert.Equal(125.0, BayWidth(system), 6);

            // Y un override de celda MAYOR sigue gobernando el rack entero.
            var governing = Resolve(Design(new double?[] { 150.0, null }, new double?[] { null, null }));
            AssertOneGrid(governing);
            Assert.Equal(150.0, BayWidth(governing), 6);
        }

        [Fact]
        public void CompositeGrid_MultilevelCellDemandsUseMaximumPhysicalEnvelope()
        {
            // Tres niveles heterogeneos, todos personalizados, contra una demanda mayor del otro lado.
            var lower = Resolve(Design(
                new double?[] { 90.0, 110.0, 100.0 }, new double?[] { null, null, null }, frontB: 125.0, levels: 3));
            AssertOneGrid(lower);
            Assert.Equal(125.0, BayWidth(lower), 6);

            // Y cuando es el lado de las celdas el que pide mas, manda su maximo.
            var higher = Resolve(Design(
                new double?[] { 130.0, 110.0, 100.0 }, new double?[] { null, null, null }, frontB: 125.0, levels: 3));
            AssertOneGrid(higher);
            Assert.Equal(130.0, BayWidth(higher), 6);
        }

        [Fact]
        public void CompositeGrid_CellAndFrontOverridesRespectAuthoredPrecedenceAndSharedEnvelope()
        {
            // La precedencia authored no cambia: la celda manda sobre el frente DE SU LADO...
            var design = Design(new double?[] { 120.0, null }, new double?[] { null, null }, frontA: 100.0);
            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(120.0, BayWidth(system), 6);
            Assert.Equal(100.0, design.Structure.Fronts[0].BeamLengthOverride);
            Assert.Equal(120.0, design.Structure.Fronts[0].Levels[0].BeamLengthOverride);
        }

        // ---------------------------------------------------------------- authored vs derivado

        [Fact]
        public void CompositeGrid_SharedEnvelopeDoesNotMaterializeIntoCellOverrides()
        {
            var design = Design(new double?[] { 90.0, 95.0 }, new double?[] { null, null }, frontB: 125.0);
            var system = Resolve(design);

            // La geometria fisica es 125 en todas partes...
            AssertOneGrid(system);
            Assert.Equal(125.0, BayWidth(system), 6);

            // ...y la INTENCION de cada lado sigue intacta: nadie escribio 125 en una celda ni en un frente de A.
            Assert.Equal(new double?[] { 90.0, 95.0 }, design.Structure.Fronts[0].Levels.Select(l => l.BeamLengthOverride));
            Assert.Null(design.Structure.Fronts[0].BeamLengthOverride);
            Assert.All(design.SideB.Fronts[0].Levels, level => Assert.Null(level.BeamLengthOverride));
            Assert.Equal(125.0, design.SideB.Fronts[0].BeamLengthOverride);

            // El nivel resuelto del lado A conserva su override authored aunque su bahia mida la compartida.
            var levelA = Local(system, PushBackSide.A).Fronts[0].Levels[0];
            Assert.Equal(90.0, levelA.BeamLengthOverride);
            Assert.Equal(125.0, levelA.BeamLength, 6);
        }

        [Fact]
        public void CompositeGrid_NullCellOverrideStaysNull()
        {
            // A3-CELL: null es intencion —«sin override»—, y la envolvente compartida no lo convierte en un numero.
            var design = Design(new double?[] { null, null }, new double?[] { null, null }, frontB: 125.0);
            Resolve(design);

            Assert.All(design.Structure.Fronts[0].Levels, level => Assert.Null(level.BeamLengthOverride));
        }

        // ---------------------------------------------------------------- lo que cuelga de la retícula

        [Fact]
        public void CompositeGrid_SecurityAnchorsStillLandOnRealPostPositions()
        {
            var system = Resolve(Design(
                new double?[] { 90.0, 95.0 }, new double?[] { null, null }, frontB: 125.0, safety: true));

            AssertOneGrid(system);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var posts = Posts(Local(system, side));
                var defenses = new PushBackSystemFrontalBuilder()
                    .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, side)
                    .Flatten().Instances
                    .Where(instance => PushBackDefensePlan.IsDefense(instance, Catalog))
                    .ToList();

                Assert.NotEmpty(defenses);
                Assert.All(
                    defenses,
                    instance => Assert.Contains(
                        posts,
                        post => Math.Abs(post - instance.Insertion.X) < 1e-6));
            }
        }

        [Fact]
        public void CompositeGrid_BomDoesNotEmitContradictoryBayWidths()
        {
            var system = Resolve(Design(new double?[] { 90.0, 95.0 }, new double?[] { null, null }, frontB: 125.0));
            var beams = PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => line.ProfileId != null
                    && line.ProfileId.StartsWith("LARGUERO_IN_OUT", StringComparison.OrdinalIgnoreCase))
                .Select(line => Math.Round(line.Length, 6))
                .Distinct()
                .ToList();

            // Cada bahia tiene UN ancho fisico —el de su frente resuelto, que es el mismo para A, para B y para la
            // compuesta—, asi que el BOM no puede cotizar ninguno que la reticula no tenga.
            var widths = system.Structure.Fronts.Select(front => Math.Round(front.BeamLength, 6)).ToList();
            Assert.NotEmpty(beams);
            Assert.All(beams, length => Assert.Contains(length, widths));
            Assert.Contains(Math.Round(BayWidth(system), 6), beams);

            // Y esos anchos son los mismos en los dos marcos locales: ninguna vista cotiza otra cosa.
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                Assert.Equal(
                    widths,
                    Local(system, side).Fronts.Select(front => Math.Round(front.BeamLength, 6)).ToList());
            }
        }
    }
}
