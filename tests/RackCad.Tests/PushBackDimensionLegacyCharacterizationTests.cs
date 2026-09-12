using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-05 (G4 Paso 1) — caracterización LEGACY de las cotas de Push Back, capturada sobre producción intacta
    /// ANTES de existir la política de cotas por vista.
    ///
    /// <para>
    /// <see cref="PushBackGoldenTests"/> fija la geometría de un sentido, pero su firma no lleva texto, desfase de cota
    /// ni altura de texto y su escenario no activa cotas ni etiquetas. Esta clase fija la capa de anotación en los
    /// cuatro niveles de detalle, con numeración de frentes y de niveles, nombre del rack y estilo de cota (MIN-5):
    /// en un rack de UN sentido —el escenario rico de la golden— y en uno COMPUESTO A/B, recorriendo el camino del
    /// Plugin: los cortes frontales (entrada-salida y posterior; en el compuesto, los cuatro), el lateral entero y
    /// cada corte por poste, y la planta.
    /// </para>
    /// <para>
    /// Estas pruebas tienen que seguir VERDES con <c>DimensionViews = null</c> en todos los gates de I-50: son la
    /// prueba del legacy exacto. NO se re-fijan por conveniencia: un pin que se mueve es un fallo, no un valor nuevo.
    /// </para>
    /// </summary>
    public class PushBackDimensionLegacyCharacterizationTests
    {
        private const string RackName = "RACK I50";
        private const string DimensionStyle = "I50_ESTILO";

        /// <summary>Un sentido: el escenario de <see cref="PushBackGoldenTests"/> (dos frentes de fondo e inicio
        /// distintos, peraltes posteriores por nivel, una celda sin tope y una bota a ambos lados).</summary>
        private const string SingleSided = "1S";

        /// <summary>Compuesto encontrado: dos ranuras por lado, A con 3 niveles y fondo 5, B con 2 niveles y fondo 4.</summary>
        private const string Composite = "AB";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static void Annotate(DynamicRackDesign structure, DimensionDetail detail)
        {
            structure.NumberFronts = true;
            structure.NumberLevels = true;
            structure.DrawRackName = true;
            structure.Dimensions = detail;
            structure.DimensionStyle = DimensionStyle;
        }

        private static PushBackDesign SingleSidedDesign(DimensionDetail detail)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            Annotate(design.Structure, detail);
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 });
            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            design.RearTope.Disable(0, 0);
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1, Side = SafetySide.Both });
            return design;
        }

        private static PushBackDesign CompositeDesign(DimensionDetail detail)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 3,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                },
                SideB = new PushBackSideDesign { IsPresent = true, LoadLevels = 2, FirstLevelHeight = 4.0 },
                Composite = new PushBackCompositeDesign
                {
                    Gap = 0.0,
                    CentralSeparator = false,
                    DefaultTopology = PushBackCellTopology.Encontradas
                }
            };
            Annotate(design.Structure, detail);
            for (var slot = 0; slot < 2; slot++)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 5, DepthStartPosition = 1 });
                design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 5 });
                design.SideB.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
                design.SideB.FrontConfigs.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            }

            return design;
        }

        private static PushBackSystem Resolve(string scenario, DimensionDetail detail, RackCatalog catalog)
        {
            var design = scenario == Composite ? CompositeDesign(detail) : SingleSidedDesign(detail);
            var system = new PushBackResolver(catalog).Resolve(design);
            system.Name = RackName; // el comando asigna el nombre del sobre antes de dibujar
            return system;
        }

        /// <summary>Cada vista del rack por el camino del Plugin, con su clave estable.</summary>
        private static List<(string View, List<HeaderBlockInstance> Instances)> Views(PushBackSystem system, RackCatalog catalog)
        {
            var frontal = new PushBackSystemFrontalBuilder();
            var lateral = new PushBackSystemLateralBuilder();
            var views = new List<(string View, List<HeaderBlockInstance> Instances)>();
            var sides = system.IsComposite ? new[] { PushBackSide.A, PushBackSide.B } : new[] { PushBackSide.A };
            foreach (var side in sides)
            {
                var suffix = system.IsComposite ? "-" + side : string.Empty;
                views.Add(("frontal-entrada-salida" + suffix,
                    frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida, side).Flatten().Instances.ToList()));
                views.Add(("frontal-posterior" + suffix,
                    frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior, side).Flatten().Instances.ToList()));
            }

            views.Add(("lateral", lateral.Build(system, catalog).Flatten().Instances.ToList()));
            foreach (var corte in lateral.Cortes(system, catalog))
            {
                views.Add(($"lateral-corte{corte.PostIndex}", corte.Plan.Flatten().Instances.ToList()));
            }

            views.Add(("planta", new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances.ToList()));
            return views;
        }

        private static IEnumerable<string> Scenarios()
        {
            yield return SingleSided;
            yield return Composite;
        }

        private static Dictionary<string, string> Signatures(Func<string, bool> belongs)
        {
            var catalog = Catalog;
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var scenario in Scenarios())
            {
                foreach (var level in DimensionLegacySignature.Levels)
                {
                    foreach (var (view, instances) in Views(Resolve(scenario, level, catalog), catalog))
                    {
                        var key = $"{scenario}/{view}/{level}";
                        if (belongs(key))
                        {
                            result[key] = DimensionLegacySignature.Of(instances);
                        }
                    }
                }
            }

            return result;
        }

        private static void AssertFamily(string family, Func<string, bool> belongs)
        {
            var drift = DimensionLegacySignature.Drift(DimensionLegacySignature.Where(Pins, belongs), Signatures(belongs));
            Assert.True(drift.Length == 0, $"T-05 ({family}): la capa de anotación legacy de Push Back se movió:\n{drift}");
        }

        [Fact]
        public void T05_SingleSided_FrontalEntradaSalidaAndPosterior_KeepTheirLegacyCotasAndLabels()
            => AssertFamily("un sentido, frontales", key => key.StartsWith(SingleSided + "/frontal-", StringComparison.Ordinal));

        [Fact]
        public void T05_SingleSided_LateralAndEveryCorte_KeepTheirLegacyCotasAndLabels()
            => AssertFamily("un sentido, lateral y cortes", key => key.StartsWith(SingleSided + "/lateral", StringComparison.Ordinal));

        [Fact]
        public void T05_SingleSided_Planta_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("un sentido, planta", key => key.StartsWith(SingleSided + "/planta/", StringComparison.Ordinal));

        [Fact]
        public void T05_Composite_TheFourFrontalCuts_KeepTheirLegacyCotasAndLabels()
            => AssertFamily("compuesto, cuatro cortes frontales", key => key.StartsWith(Composite + "/frontal-", StringComparison.Ordinal));

        [Fact]
        public void T05_Composite_LateralAndEveryCorte_KeepTheirLegacyCotasAndLabels()
            => AssertFamily("compuesto, lateral y cortes", key => key.StartsWith(Composite + "/lateral", StringComparison.Ordinal));

        [Fact]
        public void T05_Composite_Planta_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("compuesto, planta", key => key.StartsWith(Composite + "/planta/", StringComparison.Ordinal));

        [Fact]
        public void T05_ThePinTable_CoversExactlyTheCharacterizedViews()
        {
            var actual = Signatures(_ => true);
            Assert.Equal(
                actual.Keys.OrderBy(key => key, StringComparer.Ordinal),
                Pins.Keys.OrderBy(key => key, StringComparer.Ordinal));
        }

        /// <summary>
        /// Lo que la tabla de pines da por hecho, dicho en claro: el compuesto es de verdad compuesto y tiene cuatro
        /// cortes frontales; sin cotas no hay ninguna y con cualquier nivel activo todas las vistas tienen; la
        /// numeración aparece en todas; y el ALCANCE de hoy: las etiquetas se desplazan al activar las cotas en todas las
        /// vistas, y en los cortes frontales y en el lateral también entre Mínimo, Estándar y Detallado.
        ///
        /// <para>
        /// Y el NOMBRE del rack tal como sale hoy: en un sentido, en todas las vistas; en el compuesto, SOLO en la
        /// planta. Los cortes frontales y el lateral del compuesto se decoran sobre la sub-estructura de cada lado, que
        /// no lleva el nombre que el comando asigna al sistema. Es legacy y queda fijado como tal; I-50 no lo cambia.
        /// </para>
        /// </summary>
        [Fact]
        public void T05_TheScenarios_ExerciseCotasNumberingSidesAndTodaysReach()
        {
            var catalog = Catalog;
            foreach (var scenario in Scenarios())
            {
                var byLevel = DimensionLegacySignature.Levels.ToDictionary(
                    level => level,
                    level => Views(Resolve(scenario, level, catalog), catalog).ToDictionary(view => view.View, view => view.Instances));

                var composite = Resolve(scenario, DimensionDetail.Detailed, catalog).IsComposite;
                Assert.Equal(scenario == Composite, composite);
                Assert.Equal(composite ? 4 : 2, byLevel[DimensionDetail.None].Keys.Count(view => view.StartsWith("frontal-", StringComparison.Ordinal)));

                foreach (var view in byLevel[DimensionDetail.None].Keys)
                {
                    var where = $"{scenario}/{view}";
                    Assert.True(DimensionLegacySignature.DimensionCount(byLevel[DimensionDetail.None][view]) == 0, where + ": None no emite cotas");
                    foreach (var level in DimensionLegacySignature.Levels)
                    {
                        var instances = byLevel[level][view];
                        if (level != DimensionDetail.None)
                        {
                            Assert.True(DimensionLegacySignature.DimensionCount(instances) > 0, $"{where}/{level}: sin cotas");
                            Assert.All(
                                instances.Where(instance => instance.Role == HeaderBlockRole.Dimension),
                                dimension => Assert.Equal(DimensionStyle, dimension.DimensionStyleName));
                        }

                        Assert.Contains(instances, instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == "1");

                        var named = instances.Any(instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == RackName);
                        Assert.True(
                            named == (!composite || view == "planta"),
                            $"{where}/{level}: el nombre del rack está {(named ? "presente" : "ausente")}");
                    }

                    var labels = DimensionLegacySignature.Levels.ToDictionary(
                        level => level, level => DimensionLegacySignature.LabelsOnly(byLevel[level][view]));
                    Assert.NotEqual(labels[DimensionDetail.None], labels[DimensionDetail.Minimal]);
                    if (view != "planta")
                    {
                        Assert.NotEqual(labels[DimensionDetail.Minimal], labels[DimensionDetail.Standard]);
                        Assert.NotEqual(labels[DimensionDetail.Standard], labels[DimensionDetail.Detailed]);
                    }
                }

                if (composite)
                {
                    foreach (var view in new[] { "lateral", "planta" })
                    {
                        Assert.Contains(byLevel[DimensionDetail.Detailed][view], instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == "A");
                        Assert.Contains(byLevel[DimensionDetail.Detailed][view], instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == "B");
                    }
                }
            }
        }

        // Pines capturados sobre producción intacta (padre 9b592e9), antes de I-50. Formato de cada valor:
        // D<cotas> L<etiquetas> [caja de etiquetas: xmin,ymin;xmax,ymax] SHA-256 de las filas de cotas y etiquetas.
        private static readonly IReadOnlyDictionary<string, string> Pins = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["1S/frontal-entrada-salida/Detailed"] = "D8 L5 [-74,-46;80.241,130] 77AB051530C16B6DB609749F7637B18AAD8B5CF3ED787887A0EBE43EC6426623",
            ["1S/frontal-entrada-salida/Minimal"] = "D2 L5 [-24,-24;80.241,130] 1F6E57204F881C2774A022B0DD8D60B16FB7A9BD9FE5DA62DD8727B0B508A62A",
            ["1S/frontal-entrada-salida/None"] = "D0 L5 [-10,-10;80.241,130] 0BA3C607A5E4497AB9568FA7AA3B472B0548D78BCFA76FEBBC087391E37A1FFD",
            ["1S/frontal-entrada-salida/Standard"] = "D6 L5 [-46,-46;80.241,130] 2332BACBDDB6A0C16BE6E575D0A3E84F833116EB5FB3FBE0DEB064ED73791275",
            ["1S/frontal-posterior/Detailed"] = "D8 L5 [-74,-46;80.241,130] DFB76EF6AECFF0DBE29F6B6D1838A129261578E777240640D008DC2A75A93A01",
            ["1S/frontal-posterior/Minimal"] = "D2 L5 [-24,-24;80.241,130] AD1D74999A30709ADDF335539D80EC6E07B52D30C2C0C092CCC0AB6F5A443C25",
            ["1S/frontal-posterior/None"] = "D0 L5 [-10,-10;80.241,130] 13E7D5CBB96C18744C3CFC1CA20441112297E043782F59A00A529561E8018E9A",
            ["1S/frontal-posterior/Standard"] = "D6 L5 [-46,-46;80.241,130] 7C37CEB5072103768D1646EC6650362B61F952B8157955678A25C122F93EFC27",
            ["1S/lateral-corte0/Detailed"] = "D12 L3 [-74,6.6053;0,130] 8DCB2DD67299B0CA1030969D74344E93A20B24420C258AC4A3A52509176B9C32",
            ["1S/lateral-corte0/Minimal"] = "D2 L3 [-24,6.6053;0,130] 58C2937256E61FBCBF18E7CAB451B40BA0C9E554585111DC6B63D2A831894143",
            ["1S/lateral-corte0/None"] = "D0 L3 [-10,6.6053;0,130] B57CFF9241B0599AAB78E655DDADC842AE7A4079D671BF43498DD25E67054D35",
            ["1S/lateral-corte0/Standard"] = "D10 L3 [-46,6.6053;0,130] 12B9FDB5D6521FD5A32004921661D19989D5AAABA983B1FC3A7EEC51BC26DF6E",
            ["1S/lateral-corte1/Detailed"] = "D12 L3 [-74,6.6053;0,130] 8DCB2DD67299B0CA1030969D74344E93A20B24420C258AC4A3A52509176B9C32",
            ["1S/lateral-corte1/Minimal"] = "D2 L3 [-24,6.6053;0,130] 58C2937256E61FBCBF18E7CAB451B40BA0C9E554585111DC6B63D2A831894143",
            ["1S/lateral-corte1/None"] = "D0 L3 [-10,6.6053;0,130] B57CFF9241B0599AAB78E655DDADC842AE7A4079D671BF43498DD25E67054D35",
            ["1S/lateral-corte1/Standard"] = "D10 L3 [-46,6.6053;0,130] 12B9FDB5D6521FD5A32004921661D19989D5AAABA983B1FC3A7EEC51BC26DF6E",
            ["1S/lateral-corte2/Detailed"] = "D9 L3 [70,6.6053;144,130] EE3E8C78886C104F4BFCE1E568077CE83F623738CDF5D8AEFEA32E18B861655D",
            ["1S/lateral-corte2/Minimal"] = "D2 L3 [120,6.6053;144,130] D410D8EDF63885BE9795F154E3DE3E18B5CB161637CB5468A54EA7C7073CB98D",
            ["1S/lateral-corte2/None"] = "D0 L3 [134,6.6053;144,130] BAED44C02F7BCD14ECB7633254AB1CBE3CDC182E35FA2D29AF9972B9482C97A4",
            ["1S/lateral-corte2/Standard"] = "D7 L3 [98,6.6053;144,130] C3332A73B7EDAEBF986B7120927E81F07EA1FDBBDBB28F990B3FB151C77590B6",
            ["1S/lateral/Detailed"] = "D12 L3 [-74,6.6053;0,130] 8DCB2DD67299B0CA1030969D74344E93A20B24420C258AC4A3A52509176B9C32",
            ["1S/lateral/Minimal"] = "D2 L3 [-24,6.6053;0,130] 58C2937256E61FBCBF18E7CAB451B40BA0C9E554585111DC6B63D2A831894143",
            ["1S/lateral/None"] = "D0 L3 [-10,6.6053;0,130] B57CFF9241B0599AAB78E655DDADC842AE7A4079D671BF43498DD25E67054D35",
            ["1S/lateral/Standard"] = "D10 L3 [-46,6.6053;0,130] 12B9FDB5D6521FD5A32004921661D19989D5AAABA983B1FC3A7EEC51BC26DF6E",
            ["1S/planta/Detailed"] = "D9 L3 [-24,26.747;0,116.988] 04EE8119B3E5D170169D9EA3673EFEEE584B4371C268F8FEE998090F41AB2934",
            ["1S/planta/Minimal"] = "D2 L3 [-24,26.747;0,116.988] 4C92A0A23D03FB89CA3CEE27A46777E60EAB83BF4C4EFF6597A8855CA2CFAF36",
            ["1S/planta/None"] = "D0 L3 [-10,26.747;0,116.988] EB874FFA6ED242165D6D1D0E3F6608FEA87D96E08166E3189F960D1B7B879848",
            ["1S/planta/Standard"] = "D9 L3 [-24,26.747;0,116.988] 04EE8119B3E5D170169D9EA3673EFEEE584B4371C268F8FEE998090F41AB2934",
            ["AB/frontal-entrada-salida-A/Detailed"] = "D10 L5 [-88,-46;80.241,148.6053] 61FF479D5CFB796912EC3174F52AF2D9B67166E7A31832233789CF59791609DF",
            ["AB/frontal-entrada-salida-A/Minimal"] = "D2 L5 [-24,-24;80.241,148.6053] 46B61BFB40ACFC5CFCC2FF891C03FEFAE48D99B1B6FA812C317D8191D67E9A14",
            ["AB/frontal-entrada-salida-A/None"] = "D0 L5 [-10,-10;80.241,148.6053] CB6C223446D58934307D33E88F2315FB05986309D8FF8EFFCBB1891228FDC343",
            ["AB/frontal-entrada-salida-A/Standard"] = "D7 L5 [-46,-46;80.241,148.6053] EC9EBF1EF2A2D5469F78B2F26ABE101B98527BE7271634B174511BF37A92EFE8",
            ["AB/frontal-entrada-salida-B/Detailed"] = "D8 L4 [-74,-46;80.241,76.6053] 451C6308A8B38B3B3A5B46CCB0160633B437D16D9108DFB504DC817E83E44230",
            ["AB/frontal-entrada-salida-B/Minimal"] = "D2 L4 [-24,-24;80.241,76.6053] 87AEFC5A089914F12483A39481D74D17320791F80A229471B2ED4F196928127E",
            ["AB/frontal-entrada-salida-B/None"] = "D0 L4 [-10,-10;80.241,76.6053] 10BD43B7BFE5C5B7971A5973E7C0A2098269606858B35F0027CBFF64C8DD4687",
            ["AB/frontal-entrada-salida-B/Standard"] = "D6 L4 [-46,-46;80.241,76.6053] 0B97286BDC73D87B0E1728E91DF9AE0A3BE044766C91AE3F4E37AB34DED4D141",
            ["AB/frontal-posterior-A/Detailed"] = "D10 L5 [-88,-46;80.241,148.6053] 61FF479D5CFB796912EC3174F52AF2D9B67166E7A31832233789CF59791609DF",
            ["AB/frontal-posterior-A/Minimal"] = "D2 L5 [-24,-24;80.241,148.6053] 46B61BFB40ACFC5CFCC2FF891C03FEFAE48D99B1B6FA812C317D8191D67E9A14",
            ["AB/frontal-posterior-A/None"] = "D0 L5 [-10,-10;80.241,148.6053] CB6C223446D58934307D33E88F2315FB05986309D8FF8EFFCBB1891228FDC343",
            ["AB/frontal-posterior-A/Standard"] = "D7 L5 [-46,-46;80.241,148.6053] EC9EBF1EF2A2D5469F78B2F26ABE101B98527BE7271634B174511BF37A92EFE8",
            ["AB/frontal-posterior-B/Detailed"] = "D8 L4 [-74,-46;80.241,76.6053] 451C6308A8B38B3B3A5B46CCB0160633B437D16D9108DFB504DC817E83E44230",
            ["AB/frontal-posterior-B/Minimal"] = "D2 L4 [-24,-24;80.241,76.6053] 87AEFC5A089914F12483A39481D74D17320791F80A229471B2ED4F196928127E",
            ["AB/frontal-posterior-B/None"] = "D0 L4 [-10,-10;80.241,76.6053] 10BD43B7BFE5C5B7971A5973E7C0A2098269606858B35F0027CBFF64C8DD4687",
            ["AB/frontal-posterior-B/Standard"] = "D6 L4 [-46,-46;80.241,76.6053] 0B97286BDC73D87B0E1728E91DF9AE0A3BE044766C91AE3F4E37AB34DED4D141",
            ["AB/lateral-corte0/Detailed"] = "D23 L7 [-88,-10;530,148.6053] A4E110D93AFEF31834036BC01B5CD199B2DFF34DD22704017AFD24CF5004A4AA",
            ["AB/lateral-corte0/Minimal"] = "D4 L7 [-24,-10;480,148.6053] E7219470BCCDCDCB5E870353113DA5DF8F640048A4D3EF03F091F015A0CBA4AA",
            ["AB/lateral-corte0/None"] = "D0 L7 [-10,-10;466,148.6053] 419BB85B61059D6A5208802A71C1DCBC2278BE997C4A909E5A1015FA8FA97E77",
            ["AB/lateral-corte0/Standard"] = "D18 L7 [-46,-10;502,148.6053] 9B108605F4B1645341835766057EC61637C141CA6C74DBF4F2DB7B49CA5B5B02",
            ["AB/lateral-corte1/Detailed"] = "D23 L7 [-88,-10;530,148.6053] A4E110D93AFEF31834036BC01B5CD199B2DFF34DD22704017AFD24CF5004A4AA",
            ["AB/lateral-corte1/Minimal"] = "D4 L7 [-24,-10;480,148.6053] E7219470BCCDCDCB5E870353113DA5DF8F640048A4D3EF03F091F015A0CBA4AA",
            ["AB/lateral-corte1/None"] = "D0 L7 [-10,-10;466,148.6053] 419BB85B61059D6A5208802A71C1DCBC2278BE997C4A909E5A1015FA8FA97E77",
            ["AB/lateral-corte1/Standard"] = "D18 L7 [-46,-10;502,148.6053] 9B108605F4B1645341835766057EC61637C141CA6C74DBF4F2DB7B49CA5B5B02",
            ["AB/lateral-corte2/Detailed"] = "D23 L7 [-88,-10;530,148.6053] A4E110D93AFEF31834036BC01B5CD199B2DFF34DD22704017AFD24CF5004A4AA",
            ["AB/lateral-corte2/Minimal"] = "D4 L7 [-24,-10;480,148.6053] E7219470BCCDCDCB5E870353113DA5DF8F640048A4D3EF03F091F015A0CBA4AA",
            ["AB/lateral-corte2/None"] = "D0 L7 [-10,-10;466,148.6053] 419BB85B61059D6A5208802A71C1DCBC2278BE997C4A909E5A1015FA8FA97E77",
            ["AB/lateral-corte2/Standard"] = "D18 L7 [-46,-10;502,148.6053] 9B108605F4B1645341835766057EC61637C141CA6C74DBF4F2DB7B49CA5B5B02",
            ["AB/lateral/Detailed"] = "D23 L7 [-88,-10;530,148.6053] A4E110D93AFEF31834036BC01B5CD199B2DFF34DD22704017AFD24CF5004A4AA",
            ["AB/lateral/Minimal"] = "D4 L7 [-24,-10;480,148.6053] E7219470BCCDCDCB5E870353113DA5DF8F640048A4D3EF03F091F015A0CBA4AA",
            ["AB/lateral/None"] = "D0 L7 [-10,-10;466,148.6053] 419BB85B61059D6A5208802A71C1DCBC2278BE997C4A909E5A1015FA8FA97E77",
            ["AB/lateral/Standard"] = "D18 L7 [-46,-10;502,148.6053] 9B108605F4B1645341835766057EC61637C141CA6C74DBF4F2DB7B49CA5B5B02",
            ["AB/planta/Detailed"] = "D12 L5 [-24,-10;456,116.988] 98439CEDDC2C485CB2F9DFF659140BB80CF8B39C421951EFB01B544D6F6605BE",
            ["AB/planta/Minimal"] = "D2 L5 [-24,-10;456,116.988] 052BC5D4B90D945A482CA8DDF247E0FA7B61D4E93D224CBD89433CD0204337B8",
            ["AB/planta/None"] = "D0 L5 [-10,-10;456,116.988] C4AD2DE938F18B5956578177959E893DB07F9D2A72BDC563155CB55AE67EF53C",
            ["AB/planta/Standard"] = "D12 L5 [-24,-10;456,116.988] 98439CEDDC2C485CB2F9DFF659140BB80CF8B39C421951EFB01B544D6F6605BE",
        };
    }
}
