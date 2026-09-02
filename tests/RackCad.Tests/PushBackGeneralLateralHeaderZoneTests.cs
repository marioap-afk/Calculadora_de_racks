using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-H3R, contrato del dueño) — EL LATERAL GENERAL DIBUJA LA ALTURA DE LA ZONA FISICA DE CADA CABECERA.
    ///
    /// <para>
    /// El lateral SECCIONADO y el BOM piden la configuracion a <see cref="DynamicFrontGeometry"/>
    /// —<c>HeaderConfigurationAtPost</c>—, que reconstruye la altura de una cabecera CALCULADA sobre la autoridad de
    /// zonas. El lateral general se quedaba con <c>module.AssociatedFrameConfiguration</c> tal cual, y ese objeto se
    /// asocio ANTES de que <c>PushBackHeaderHeight.Apply</c> resolviera la demanda por zona.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio</b> en un compuesto con camas CORRIDAS —A de tres niveles, B de uno—: la corrida
    /// atraviesa fisicamente los dos lados, asi que la zona de B sube a 192". El corte de cada linea y el BOM daban
    /// «Cabecera F54 A192» para las cuatro cabeceras; el lateral general dibujaba dos de ellas con «A48» —postes de
    /// 48" de largo—, la altura que el lado B tenia por su cuenta antes de que la cama corrida existiera. Misma
    /// pieza fisica, tres vistas, dos alturas.
    /// </para>
    ///
    /// <para>
    /// La correccion no cambia ninguna autoridad de altura ni el camino seccionado: el general pregunta por la
    /// configuracion EFECTIVA con la pregunta que le corresponde —<c>HeightAt</c>, la envolvente de la zona en esa
    /// X, jamas un maximo global—. Lo AUTHORED no se toca: una cabecera personalizada sigue mandando, y el objeto
    /// asociado del modulo no se muta.
    /// </para>
    /// </summary>
    public class PushBackGeneralLateralHeaderZoneTests
    {
        private const string HeaderPost = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string LengthParam = "LONGITUD";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackSystem Resolve(
            int levelsA,
            int levelsB,
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            int deepA = 4,
            int deepB = 4,
            double gap = 54.0)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: deepA, deepB: deepB, levelsA: levelsA, levelsB: levelsB, gap: gap);
            design.Composite.DefaultTopology = topology;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        // ---------------------------------------------------------------- las tres vistas, medidas en su geometria

        /// <summary>La altura FISICA que una cabecera dibuja: el largo de sus postes en el plan.</summary>
        private static double PostLength(HeaderGroup group)
            => group.Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post
                    && string.Equals(instance.PieceId, HeaderPost, StringComparison.OrdinalIgnoreCase))
                .Select(instance => instance.DynamicParameters.TryGetValue(LengthParam, out var value) ? value : 0.0)
                .DefaultIfEmpty(0.0)
                .Max();

        /// <summary>Cada cabecera del plan por su X de colocacion, con la altura que dibuja ahi.</summary>
        private static IReadOnlyDictionary<double, double> HeightsByX(HeaderRunPlan plan)
        {
            var result = new Dictionary<double, double>();
            foreach (var group in plan.Headers)
            {
                var length = PostLength(group);
                if (length <= 0.0)
                {
                    continue;
                }

                foreach (var placement in group.Placements)
                {
                    result[Math.Round(placement.InsertionX, 6)] = length;
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<double, double> General(PushBackSystem system)
            => HeightsByX(new PushBackSystemLateralBuilder().Build(system, Catalog));

        private static IReadOnlyDictionary<double, double> Cut(PushBackSystem system, int postIndex)
            => HeightsByX(new PushBackSystemLateralBuilder().Build(system, Catalog, postIndex));

        /// <summary>Los largos de poste de cabecera que el BOM cotiza.</summary>
        private static IReadOnlyList<double> BomPostLengths(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, HeaderPost, StringComparison.OrdinalIgnoreCase))
                .Select(line => Math.Round(line.Length, 6))
                .Distinct()
                .OrderBy(length => length)
                .ToList();

        private static IReadOnlyList<double> DistinctHeights(IReadOnlyDictionary<double, double> plan)
            => plan.Values.Select(height => Math.Round(height, 6)).Distinct().OrderBy(height => height).ToList();

        /// <summary>
        /// El modulo de cabecera de una colocacion. Una cabecera se inserta en su borde IZQUIERDO, o en el DERECHO
        /// cuando va espejada, asi que la X de colocacion es uno de los dos extremos del modulo.
        /// </summary>
        private static DynamicRackModule HeaderAt(PushBackSystem system, double x)
            => system.Structure.Modules.FirstOrDefault(module =>
                module.IsHeader && module.AssociatedFrameConfiguration != null
                && (Math.Abs(module.StartX - x) < 1e-6 || Math.Abs(module.EndX - x) < 1e-6));

        private static string Show(IReadOnlyDictionary<double, double> plan)
            => string.Join(
                " ",
                plan.OrderBy(entry => entry.Key)
                    .Select(entry => FormattableString.Invariant($"x{entry.Key:0.##}={entry.Value:0.##}")));

        // ---------------------------------------------------------------- la zona manda

        [Fact]
        public void GeneralLateral_HeaderUsesHeightOfItsPhysicalZone()
        {
            // Camas CORRIDAS: atraviesan los dos lados, asi que la salvaguarda de zona sube tambien la de B.
            var system = Resolve(levelsA: 3, levelsB: 1, topology: PushBackCellTopology.Corrida);
            var general = General(system);

            Assert.NotEmpty(general);
            foreach (var entry in general)
            {
                var module = HeaderAt(system, entry.Key);
                Assert.NotNull(module);
                var zone = DynamicFrontGeometry.HeightAt(
                    system.Structure, 0.5 * (module.StartX + module.EndX));
                Assert.Equal(zone, entry.Value, 6);
            }

            // Y la cabecera del lado bajo dibuja la altura de la zona, no la que su lado tenia por su cuenta.
            var low = general.First(entry => entry.Key > system.Structure.InteriorFaceEndX);
            Assert.Equal(192.0, low.Value, 6);
            Assert.Equal(48.0, HeaderAt(system, low.Key).AssociatedFrameConfiguration.Height, 6);
        }

        [Fact]
        public void GeneralLateral_HeaderMatchesSectionedCutForSamePhysicalLine()
        {
            var system = Resolve(levelsA: 3, levelsB: 1, topology: PushBackCellTopology.Corrida);
            var general = General(system);

            for (var postIndex = 0; postIndex <= system.Structure.Fronts.Count; postIndex++)
            {
                var cut = Cut(system, postIndex);
                foreach (var entry in cut)
                {
                    // La MISMA cabecera fisica —misma X de colocacion— en las dos vistas.
                    Assert.True(
                        general.ContainsKey(entry.Key),
                        FormattableString.Invariant($"el lateral general no coloca cabecera en x={entry.Key:0.##}")
                            + ": " + Show(general));
                    Assert.Equal(entry.Value, general[entry.Key], 6);
                }
            }
        }

        [Fact]
        public void GeneralLateral_HeaderLengthMatchesBomForSamePhysicalHeader()
        {
            foreach (var system in new[]
            {
                Resolve(levelsA: 3, levelsB: 1, topology: PushBackCellTopology.Corrida),
                Resolve(levelsA: 3, levelsB: 1),
            })
            {
                var general = DistinctHeights(General(system));
                var bom = BomPostLengths(system);

                Assert.NotEmpty(general);
                Assert.All(general, height => Assert.Contains(height, bom));
            }
        }

        // ---------------------------------------------------------------- una zona no infla a la otra

        [Fact]
        public void GeneralLateral_LowSideZoneIsNotInflatedByOppositeHighSide()
        {
            // Camas ENCONTRADAS: cada lado carga la suya, asi que el alto de A no puede alargar los postes de B.
            var system = Resolve(levelsA: 3, levelsB: 1);
            var general = General(system);

            var interior = system.Structure.InteriorFaceEndX;
            Assert.Equal(192.0, general.Where(e => e.Key < system.Structure.InteriorFaceStartX).Max(e => e.Value), 6);
            Assert.All(
                general.Where(entry => entry.Key > interior),
                entry => Assert.Equal(48.0, entry.Value, 6));
        }

        [Fact]
        public void GeneralLateral_SideBHighZoneDoesNotInflateSideA()
        {
            // El inverso: nada puede estar cableado hacia el lado A.
            var system = Resolve(levelsA: 1, levelsB: 3);
            var general = General(system);

            Assert.All(
                general.Where(entry => entry.Key < system.Structure.InteriorFaceStartX),
                entry => Assert.Equal(48.0, entry.Value, 6));
            Assert.Equal(
                192.0,
                general.Where(entry => entry.Key > system.Structure.InteriorFaceEndX).Max(entry => entry.Value),
                6);
        }

        [Fact]
        public void GeneralLateral_EachHeaderUsesItsOwnHeightZone()
        {
            // Dos alturas distintas en el MISMO plan: el general no elige una sola para todo el rack.
            var system = Resolve(levelsA: 3, levelsB: 1);
            var general = General(system);

            Assert.Equal(new[] { 48.0, 192.0 }, DistinctHeights(general));
            foreach (var entry in general)
            {
                var module = HeaderAt(system, entry.Key);
                Assert.Equal(
                    DynamicFrontGeometry.HeightAt(system.Structure, 0.5 * (module.StartX + module.EndX)),
                    entry.Value,
                    6);
            }
        }

        // ---------------------------------------------------------------- authored vs derivado (I-40)

        [Fact]
        public void GeneralLateral_EffectiveZoneHeightPreservesCustomHeaderConfiguration()
        {
            var system = Resolve(levelsA: 3, levelsB: 1, topology: PushBackCellTopology.Corrida);
            var custom = system.Structure.Modules.Last(module =>
                module.IsHeader && module.AssociatedFrameConfiguration != null);

            // Una cabecera PERSONALIZADA de I-40: deja de ser calculada y su altura es intencion, no derivada.
            custom.UseCalculatedHeaderConfiguration = false;
            custom.AssociatedFrameConfiguration.Height = 137.0;
            custom.AssociatedFrameConfiguration.PasoTroquel = 3.5;
            var moduleId = custom.ModuleId;
            var configuration = custom.AssociatedFrameConfiguration;

            var general = General(system);
            var mine = general.First(entry =>
                Math.Abs(entry.Key - custom.StartX) < 1e-6 || Math.Abs(entry.Key - custom.EndX) < 1e-6);

            // Se dibuja SU altura, no la de la zona, y la configuracion sigue siendo la misma —el mismo objeto—.
            Assert.Equal(137.0, mine.Value, 6);
            Assert.Same(configuration, custom.AssociatedFrameConfiguration);
            Assert.Equal(3.5, custom.AssociatedFrameConfiguration.PasoTroquel, 6);
            Assert.Equal(moduleId, custom.ModuleId);

            // Y el corte de cada linea dice exactamente lo mismo de esa cabecera.
            for (var postIndex = 0; postIndex <= system.Structure.Fronts.Count; postIndex++)
            {
                var cut = Cut(system, postIndex);
                if (cut.TryGetValue(mine.Key, out var height))
                {
                    Assert.Equal(137.0, height, 6);
                }
            }
        }

        [Fact]
        public void GeneralLateral_DoesNotMutateTheAuthoredHeaderConfiguration()
        {
            var system = Resolve(levelsA: 3, levelsB: 1, topology: PushBackCellTopology.Corrida);
            var before = system.Structure.Modules
                .Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .Select(module => new
                {
                    module.ModuleId,
                    Configuration = module.AssociatedFrameConfiguration,
                    module.AssociatedFrameConfiguration.Height,
                    module.UseCalculatedHeaderConfiguration,
                })
                .ToList();

            General(system);
            General(system); // dos veces: dibujar no puede acumular estado

            foreach (var snapshot in before)
            {
                var module = system.Structure.Modules.Single(candidate =>
                    string.Equals(candidate.ModuleId, snapshot.ModuleId, StringComparison.Ordinal)
                    && candidate.IsHeader);

                // La altura DERIVADA es salida: no se escribe como intencion ni ensucia la linea base.
                Assert.Same(snapshot.Configuration, module.AssociatedFrameConfiguration);
                Assert.Equal(snapshot.Height, module.AssociatedFrameConfiguration.Height, 6);
                Assert.Equal(snapshot.UseCalculatedHeaderConfiguration, module.UseCalculatedHeaderConfiguration);
                Assert.Equal(snapshot.ModuleId, module.ModuleId);
            }

            // Y sigue existiendo la contradiccion de origen —el objeto asociado guarda 48— sin que la vista la dibuje.
            Assert.Contains(before, snapshot => Math.Abs(snapshot.Height - 48.0) < 1e-6);
            Assert.All(DistinctHeights(General(system)), height => Assert.Equal(192.0, height, 6));
        }

        [Fact]
        public void GeneralLateral_SingleSidedRackKeepsItsHeaderHeights()
        {
            // Un rack de un solo sentido no declara zonas: la respuesta es exactamente la de siempre.
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 0, deepA: 4, deepB: 0, levelsA: 3, levelsB: 0, gap: 0.0);
            design.SideB.IsPresent = false;
            var system = new PushBackResolver(Catalog).Resolve(design);

            var general = General(system);
            Assert.NotEmpty(general);
            Assert.Empty(system.Structure.HeaderHeightZones);
            foreach (var entry in general)
            {
                var module = HeaderAt(system, entry.Key);
                Assert.Equal(module.AssociatedFrameConfiguration.Height, entry.Value, 6);
            }
        }
    }
}
