using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-04 (G4 Paso 1) — caracterización LEGACY de las cotas del Dinámico, capturada sobre producción intacta
    /// ANTES de existir la política de cotas por vista.
    ///
    /// <para>
    /// <see cref="DynamicNullOverrideGoldenTests"/> se conserva INTACTA y sigue fijando el plan completo de la frontal
    /// de salida y de entrada, del lateral y de sus cortes con cotas <c>Detailed</c>. Lo que no cubre, y cubre esta
    /// clase, es la capa de anotación en los otros niveles y con el nombre del rack asignado —la golden no le pone
    /// nombre, así que su rótulo nunca se emite—, y la PLANTA, que la golden no dibuja. Es el mismo escenario jagged
    /// de la golden, más nombre y estilo de cota, recorrido por el camino del Plugin.
    /// </para>
    /// <para>
    /// Estas pruebas tienen que seguir VERDES con <c>DimensionViews = null</c> en todos los gates de I-50: son la
    /// prueba del legacy exacto. NO se re-fijan por conveniencia: un pin que se mueve es un fallo, no un valor nuevo.
    /// </para>
    /// </summary>
    public class DynamicDimensionLegacyCharacterizationTests
    {
        private const string RackName = "RACK I50";
        private const string DimensionStyle = "I50_ESTILO";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>El escenario de <see cref="DynamicNullOverrideGoldenTests"/>: frentes con distinto número de niveles,
        /// distinta profundidad y distinta altura de primer nivel.</summary>
        private static DynamicRackSystem Scenario(DimensionDetail detail, RackCatalog catalog)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
                NumberLevels = true,
                NumberFronts = true,
                DrawRackName = true,
                Dimensions = detail,
                DimensionStyle = DimensionStyle
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 4.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1, FirstLevelHeight = 12.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 8.0 });
            foreach (var family in DynamicSafetyDefaults.Build(catalog))
            {
                design.SafetySelections.Add(family);
            }

            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            system.Name = RackName; // el comando asigna el nombre del sobre antes de dibujar
            return system;
        }

        /// <summary>Cada vista del rack por el camino del Plugin, con su clave estable.</summary>
        private static List<(string View, List<HeaderBlockInstance> Instances)> Views(DynamicRackSystem system, RackCatalog catalog)
        {
            var frontal = new DynamicSystemFrontalBuilder();
            var lateral = new DynamicSystemLateralBuilder();
            var views = new List<(string View, List<HeaderBlockInstance> Instances)>
            {
                ("frontal-salida", frontal.BuildPlan(system, catalog, DynamicRackEnd.Exit).Flatten().Instances.ToList()),
                ("frontal-entrada", frontal.BuildPlan(system, catalog, DynamicRackEnd.Entrance).Flatten().Instances.ToList()),
                ("lateral", lateral.Build(system, catalog).Flatten().Instances.ToList())
            };

            foreach (var corte in lateral.Cortes(system, catalog))
            {
                views.Add(($"lateral-corte{corte.PostIndex}", corte.Plan.Flatten().Instances.ToList()));
            }

            views.Add(("planta", new DynamicSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances.ToList()));
            return views;
        }

        private static Dictionary<string, string> Signatures(Func<string, bool> belongs)
        {
            var catalog = Catalog;
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var level in DimensionLegacySignature.Levels)
            {
                foreach (var (view, instances) in Views(Scenario(level, catalog), catalog))
                {
                    var key = $"{view}/{level}";
                    if (belongs(key))
                    {
                        result[key] = DimensionLegacySignature.Of(instances);
                    }
                }
            }

            return result;
        }

        private static void AssertFamily(string family, Func<string, bool> belongs)
        {
            var drift = DimensionLegacySignature.Drift(DimensionLegacySignature.Where(Pins, belongs), Signatures(belongs));
            Assert.True(drift.Length == 0, $"T-04 ({family}): la capa de anotación legacy del Dinámico se movió:\n{drift}");
        }

        [Fact]
        public void T04_FrontalSalidaAndEntrada_KeepTheirLegacyCotasAndLabels()
            => AssertFamily("frontal salida y entrada", key => key.StartsWith("frontal-", StringComparison.Ordinal));

        [Fact]
        public void T04_LateralAndEveryCorte_KeepTheirLegacyCotasAndLabels()
            => AssertFamily("lateral y cortes", key => key.StartsWith("lateral", StringComparison.Ordinal));

        [Fact]
        public void T04_Planta_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("planta", key => key.StartsWith("planta/", StringComparison.Ordinal));

        [Fact]
        public void T04_ThePinTable_CoversExactlyTheCharacterizedViews()
        {
            var actual = Signatures(_ => true);
            Assert.Equal(
                actual.Keys.OrderBy(key => key, StringComparer.Ordinal),
                Pins.Keys.OrderBy(key => key, StringComparer.Ordinal));
        }

        /// <summary>
        /// Lo que la tabla de pines da por hecho, dicho en claro: sin cotas no hay ninguna, con cualquier nivel activo
        /// todas las vistas tienen, y la numeración y el nombre aparecen en todas. Y el ALCANCE tal como es hoy: en las
        /// frontales y en el lateral las etiquetas se desplazan en CADA cambio de nivel (Detallado las aleja además por
        /// nivel), y en la planta solo al pasar de None a Mínimo.
        /// </summary>
        [Fact]
        public void T04_TheScenario_ExercisesCotasNumberingNameAndTodaysReach()
        {
            var catalog = Catalog;
            var byLevel = DimensionLegacySignature.Levels.ToDictionary(
                level => level,
                level => Views(Scenario(level, catalog), catalog).ToDictionary(view => view.View, view => view.Instances));

            foreach (var view in byLevel[DimensionDetail.None].Keys)
            {
                Assert.True(DimensionLegacySignature.DimensionCount(byLevel[DimensionDetail.None][view]) == 0, view + ": None no emite cotas");
                foreach (var level in DimensionLegacySignature.Levels)
                {
                    var instances = byLevel[level][view];
                    if (level != DimensionDetail.None)
                    {
                        Assert.True(DimensionLegacySignature.DimensionCount(instances) > 0, $"{view}/{level}: sin cotas");
                        Assert.All(
                            instances.Where(instance => instance.Role == HeaderBlockRole.Dimension),
                            dimension => Assert.Equal(DimensionStyle, dimension.DimensionStyleName));
                    }

                    Assert.Contains(instances, instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == RackName);
                    Assert.Contains(instances, instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == "1");
                }

                var labels = DimensionLegacySignature.Levels.ToDictionary(
                    level => level, level => DimensionLegacySignature.LabelsOnly(byLevel[level][view]));
                Assert.NotEqual(labels[DimensionDetail.None], labels[DimensionDetail.Minimal]);
                if (view == "planta")
                {
                    Assert.Equal(labels[DimensionDetail.Minimal], labels[DimensionDetail.Standard]);
                    Assert.Equal(labels[DimensionDetail.Standard], labels[DimensionDetail.Detailed]);
                }
                else
                {
                    Assert.NotEqual(labels[DimensionDetail.Minimal], labels[DimensionDetail.Standard]);
                    Assert.NotEqual(labels[DimensionDetail.Standard], labels[DimensionDetail.Detailed]);
                }
            }
        }

        // Pines capturados sobre producción intacta (padre 9b592e9), antes de I-50. Formato de cada valor:
        // D<cotas> L<etiquetas> [caja de etiquetas: xmin,ymin;xmax,ymax] SHA-256 de las filas de cotas y etiquetas.
        private static readonly IReadOnlyDictionary<string, string> Pins = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["frontal-entrada/Detailed"] = "D11 L7 [-88,-46;133.735,214] 8576513AB0319D7E6377C9110C82F07E5794A1083026948E59BD1FF7FED20328",
            ["frontal-entrada/Minimal"] = "D2 L7 [-24,-24;133.735,214] 0A9F385D4E62BA56BE3CE2970FE3D818960C37567666EACABA23CA61B23813E2",
            ["frontal-entrada/None"] = "D0 L7 [-10,-10;133.735,214] DEF53E13BB3B1D83C268902A300EC9FF0D05A3AF6754867E37E9CEE96BE964DE",
            ["frontal-entrada/Standard"] = "D8 L7 [-46,-46;133.735,214] 761E00EB427DAB769A7C3770CF2E93239FDAC197C4D6FB4D9EAC234FD6F4C899",
            ["frontal-salida/Detailed"] = "D11 L7 [-88,-46;133.735,214] 12BD90884FBF90069C9BA2858127F6825B3AD5CEE1048C5441355DE6D5533400",
            ["frontal-salida/Minimal"] = "D2 L7 [-24,-24;133.735,214] 8EB81B1F04718471C3495D09F3E2E9D8BE288C09299935B1E749FD468F847D1D",
            ["frontal-salida/None"] = "D0 L7 [-10,-10;133.735,214] 7AC5EB8F081ABEF61B9247BE51FC1073780DC6B701530E779EC319BD450E55E6",
            ["frontal-salida/Standard"] = "D8 L7 [-46,-46;133.735,214] A1F9943855E06E48FC8C5AD55C289EB153332702654B0EC6CDECCCCE22234088",
            ["lateral-corte0/Detailed"] = "D10 L3 [-74,12.6053;0,130] 1927C5A5234F11F6981A5377CEECEAF914151A7E05538904F9A545C4EC7AA4D1",
            ["lateral-corte0/Minimal"] = "D2 L3 [-24,12.6053;0,130] C3C93DCDE07E84EEB711A32EE4BCF9E75DEF29E6A0C3F77C7E60C48A38DAD2AF",
            ["lateral-corte0/None"] = "D0 L3 [-10,12.6053;0,130] FAC6045F289800B9705720DBE1BF40C9A1718F5B0093F30BED74964FD913A6E8",
            ["lateral-corte0/Standard"] = "D8 L3 [-46,12.6053;0,130] DDD1145C40BE3DB9F6B3E386ABB42A7AFD6893C57FE226013EDA560EFE24CA65",
            ["lateral-corte1/Detailed"] = "D14 L4 [-88,12.6053;0,214] D8FE730D25E688403DBD1CB71A430437BB712A0BBCE63EFFA38C235698C6C85F",
            ["lateral-corte1/Minimal"] = "D2 L4 [-24,12.6053;0,214] 034DAF4D97DF18A669557BB00E46865DBCFF42BC9079837DEAFF5B8684B1A690",
            ["lateral-corte1/None"] = "D0 L4 [-10,12.6053;0,214] D64F8BCFB2A4B6491CB239CE3C48A25143F1843D0DCE380AC6353B07E3570A2B",
            ["lateral-corte1/Standard"] = "D11 L4 [-46,12.6053;0,214] DB9E7F7A91128385927BFECA9B6F5A17A4B0912A6EE86BC84DB1287A50695FA1",
            ["lateral-corte2/Detailed"] = "D14 L4 [-88,12.6053;0,214] D8FE730D25E688403DBD1CB71A430437BB712A0BBCE63EFFA38C235698C6C85F",
            ["lateral-corte2/Minimal"] = "D2 L4 [-24,12.6053;0,214] 034DAF4D97DF18A669557BB00E46865DBCFF42BC9079837DEAFF5B8684B1A690",
            ["lateral-corte2/None"] = "D0 L4 [-10,12.6053;0,214] D64F8BCFB2A4B6491CB239CE3C48A25143F1843D0DCE380AC6353B07E3570A2B",
            ["lateral-corte2/Standard"] = "D11 L4 [-46,12.6053;0,214] DB9E7F7A91128385927BFECA9B6F5A17A4B0912A6EE86BC84DB1287A50695FA1",
            ["lateral-corte3/Detailed"] = "D12 L4 [-88,12.6053;0,202] 3D577CE7D2598B84046F62CD24C25035426ED8AC6A77B84728E59EAEC51BB2C7",
            ["lateral-corte3/Minimal"] = "D2 L4 [-24,12.6053;0,202] 598612FCFA96A1FB3E9DB099C259309C56C2642E63B857254227EB5D042981E7",
            ["lateral-corte3/None"] = "D0 L4 [-10,12.6053;0,202] 4E07EDC931B3AB694567905ABFE7097C39205EF6FBF05E32166DE14C75166BC0",
            ["lateral-corte3/Standard"] = "D9 L4 [-46,12.6053;0,202] 0FA231B242A787EAB522C7717E34594D0421F18FBB2C2DB483EEFA77CB041AFF",
            ["lateral/Detailed"] = "D14 L4 [-88,12.6053;0,214] D8FE730D25E688403DBD1CB71A430437BB712A0BBCE63EFFA38C235698C6C85F",
            ["lateral/Minimal"] = "D2 L4 [-24,12.6053;0,214] 034DAF4D97DF18A669557BB00E46865DBCFF42BC9079837DEAFF5B8684B1A690",
            ["lateral/None"] = "D0 L4 [-10,12.6053;0,214] D64F8BCFB2A4B6491CB239CE3C48A25143F1843D0DCE380AC6353B07E3570A2B",
            ["lateral/Standard"] = "D11 L4 [-46,12.6053;0,214] DB9E7F7A91128385927BFECA9B6F5A17A4B0912A6EE86BC84DB1287A50695FA1",
            ["planta/Detailed"] = "D10 L4 [-24,26.747;0,170.482] 8D7F658F11196D65EA9A141081CA1DB5F2A9CFFA534DC6E66907FEC69172D83E",
            ["planta/Minimal"] = "D2 L4 [-24,26.747;0,170.482] 06192B988464A733945E73EF30946664B9A4DDB021DF91B0E7E4FA722E77C6C9",
            ["planta/None"] = "D0 L4 [-10,26.747;0,170.482] B45683072B154629A55071C8493F9BA0497C13ABA2B50DFBA49071E19AE6DB3A",
            ["planta/Standard"] = "D10 L4 [-24,26.747;0,170.482] 8D7F658F11196D65EA9A141081CA1DB5F2A9CFFA534DC6E66907FEC69172D83E",
        };
    }
}
