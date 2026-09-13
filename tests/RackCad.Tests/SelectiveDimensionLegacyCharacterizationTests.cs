using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-03 (G4 Paso 1) — caracterización LEGACY de las cotas del Selectivo, capturada sobre producción intacta
    /// ANTES de existir la política de cotas por vista.
    ///
    /// <para>
    /// Recorre el mismo camino que el Plugin: la frontal de cada fondo por <c>FondoSystemView</c> + <c>BuildPlan</c>,
    /// cada corte lateral de <c>Cortes()</c> y la planta por <c>BuildPlan</c>, con el nombre asignado al sistema como
    /// hace el comando. Fija los cuatro niveles de detalle con numeración de frentes y de niveles, nombre del rack y
    /// un estilo de cota, en un rack de un fondo y en uno de dos fondos en esquina —el segundo fondo menos profundo,
    /// con menos frentes y otros niveles—, que es donde los cortes lejanos pierden un fondo.
    /// </para>
    /// <para>
    /// Estas pruebas tienen que seguir VERDES con <c>DimensionViews = null</c> en todos los gates de I-50: son la
    /// prueba del legacy exacto. NO se re-fijan por conveniencia: un pin que se mueve es un fallo, no un valor nuevo.
    /// </para>
    /// </summary>
    public class SelectiveDimensionLegacyCharacterizationTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        private const string RackName = "RACK I50";
        private const string DimensionStyle = "I50_ESTILO";

        /// <summary>Un fondo: tres frentes de cuatro niveles.</summary>
        private const string OneFondo = "1F";

        /// <summary>Dos fondos en esquina: el fondo 1 tiene 40" de fondo, dos frentes y tres niveles más bajos.</summary>
        private const string TwoFondos = "2F";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign Design(DimensionDetail detail, bool twoFondos)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletDepth = 48.0,
                NumberFronts = true,
                NumberLevels = true,
                DrawRackName = true,
                Dimensions = detail,
                DimensionStyle = DimensionStyle
            };

            for (var b = 0; b < 3; b++)
            {
                design.Bays.Add(Bay(levels: 4, palletHeight: 60.0));
            }

            if (twoFondos)
            {
                design.DepthCount = 2;
                design.SeparatorLengths.Add(12.0);
                design.ExtraFondoDepths.Add(40.0);
                design.ExtraFondoBays.Add(new List<SelectiveBayDesign>
                {
                    Bay(levels: 3, palletHeight: 48.0),
                    Bay(levels: 3, palletHeight: 48.0)
                });
            }

            return design;
        }

        private static SelectiveBayDesign Bay(int levels, double palletHeight)
        {
            var bay = new SelectiveBayDesign();
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = palletHeight },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        private static SelectiveRackSystem Resolve(DimensionDetail detail, bool twoFondos, RackCatalog catalog)
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(detail, twoFondos), catalog);
            system.Name = RackName; // el comando asigna el nombre del sobre antes de dibujar
            return system;
        }

        /// <summary>Cada vista del rack por el camino del Plugin, con su clave estable.</summary>
        private static List<(string View, List<HeaderBlockInstance> Instances)> Views(SelectiveRackSystem system, RackCatalog catalog)
        {
            var views = new List<(string View, List<HeaderBlockInstance> Instances)>();
            var fondos = SelectiveDepthLayout.Offsets(system).Count;
            for (var k = 0; k < fondos; k++)
            {
                var fondoView = SelectiveDepthLayout.FondoSystemView(system, k);
                fondoView.Name = RackName;
                views.Add(($"frontal-fondo{k}", new SelectiveFrontalBuilder().BuildPlan(fondoView, catalog).Flatten().Instances.ToList()));
            }

            foreach (var corte in new SelectiveLateralBuilder().Cortes(system, catalog))
            {
                views.Add(($"lateral-corte{corte.PostIndex}", corte.Largueros.ToList()));
            }

            views.Add(("planta", new SelectivePlantaBuilder().BuildPlan(system, catalog).Flatten().Instances.ToList()));
            return views;
        }

        private static IEnumerable<(string Scenario, bool TwoFondos)> Scenarios()
        {
            yield return (OneFondo, false);
            yield return (TwoFondos, true);
        }

        private static Dictionary<string, string> Signatures(Func<string, bool> belongs)
        {
            var catalog = Catalog;
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (scenario, twoFondos) in Scenarios())
            {
                foreach (var level in DimensionLegacySignature.Levels)
                {
                    foreach (var (view, instances) in Views(Resolve(level, twoFondos, catalog), catalog))
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
            Assert.True(drift.Length == 0, $"T-03 ({family}): la capa de anotación legacy del Selectivo se movió:\n{drift}");
        }

        [Fact]
        public void T03_FrontalOfOneFondo_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("frontal, 1 fondo", key => key.StartsWith(OneFondo + "/frontal-", StringComparison.Ordinal));

        [Fact]
        public void T03_FrontalOfEachOfTwoFondos_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("frontal, 2 fondos", key => key.StartsWith(TwoFondos + "/frontal-", StringComparison.Ordinal));

        [Fact]
        public void T03_EveryLateralCorte_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("cortes laterales", key => key.Contains("/lateral-corte", StringComparison.Ordinal));

        [Fact]
        public void T03_Planta_KeepsItsLegacyCotasAndLabels()
            => AssertFamily("planta", key => key.Contains("/planta/", StringComparison.Ordinal));

        [Fact]
        public void T03_ThePinTable_CoversExactlyTheCharacterizedViews()
        {
            var actual = Signatures(_ => true);
            Assert.Equal(
                actual.Keys.OrderBy(key => key, StringComparer.Ordinal),
                Pins.Keys.OrderBy(key => key, StringComparer.Ordinal));
        }

        /// <summary>
        /// Lo que la tabla de pines da por hecho, dicho en claro: sin cotas no hay ninguna, con cualquier nivel activo
        /// todas las vistas tienen, y la numeración y el nombre aparecen en todas. Y el ALCANCE tal como es hoy: en la
        /// frontal los números de frente bajan al activar las cotas (Mínimo los baja menos que Estándar, y Detallado
        /// igual que Estándar), mientras que las etiquetas del corte lateral y de la planta no dependen del nivel.
        /// </summary>
        [Fact]
        public void T03_TheScenarios_ExerciseCotasNumberingNameAndTodaysReach()
        {
            var catalog = Catalog;
            foreach (var (scenario, twoFondos) in Scenarios())
            {
                var byLevel = DimensionLegacySignature.Levels.ToDictionary(
                    level => level,
                    level => Views(Resolve(level, twoFondos, catalog), catalog).ToDictionary(view => view.View, view => view.Instances));

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

                        Assert.Contains(instances, instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == RackName);
                        Assert.Contains(instances, instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == "1");
                    }

                    var labels = DimensionLegacySignature.Levels.ToDictionary(
                        level => level, level => DimensionLegacySignature.LabelsOnly(byLevel[level][view]));
                    if (view.StartsWith("frontal-", StringComparison.Ordinal))
                    {
                        double LowestLabel(DimensionDetail level) => byLevel[level][view]
                            .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                            .Min(instance => instance.Insertion.Y);

                        Assert.True(LowestLabel(DimensionDetail.Minimal) < LowestLabel(DimensionDetail.None), where + ": Mínimo baja los números");
                        Assert.True(LowestLabel(DimensionDetail.Standard) < LowestLabel(DimensionDetail.Minimal), where + ": Estándar los baja más");
                        Assert.Equal(labels[DimensionDetail.Standard], labels[DimensionDetail.Detailed]);
                    }
                    else
                    {
                        Assert.Equal(labels[DimensionDetail.None], labels[DimensionDetail.Minimal]);
                        Assert.Equal(labels[DimensionDetail.None], labels[DimensionDetail.Standard]);
                        Assert.Equal(labels[DimensionDetail.None], labels[DimensionDetail.Detailed]);
                    }
                }
            }
        }

        // Pines capturados sobre producción intacta (padre 9b592e9), antes de I-50. Formato de cada valor:
        // D<cotas> L<etiquetas> [caja de etiquetas: xmin,ymin;xmax,ymax] SHA-256 de las filas de cotas y etiquetas.
        private static readonly IReadOnlyDictionary<string, string> Pins = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["1F/frontal-fondo0/Detailed"] = "D11 L7 [-10,-46;248.75,246] CF0FC7292434391A342258FB8F20B00BC83AF7761FD39C28AC4D019008A89F6E",
            ["1F/frontal-fondo0/Minimal"] = "D2 L7 [-10,-24;248.75,246] E3868573911DA145CA16675F4CFCD5A6688DBD398E412CB600DB0BAC98D21ED7",
            ["1F/frontal-fondo0/None"] = "D0 L7 [-10,-10;248.75,246] F17830335325C8DB9638EF4AE133268957A8AD23E6082917E01FC17C73F71DC9",
            ["1F/frontal-fondo0/Standard"] = "D8 L7 [-10,-46;248.75,246] DCE85513028F04E17A5F86EACD9361669E1828DB61151562F9348B8143F8E29A",
            ["1F/lateral-corte0/Detailed"] = "D9 L5 [-10,66.6053;0,262] 606A3772F223ED6A69B5CEBD4827CF1F857C6475C128F3FA948DB0E054C19109",
            ["1F/lateral-corte0/Minimal"] = "D2 L5 [-10,66.6053;0,262] 74AF30128FF82822781F91995675F854DC3C9A597B7E3EBFE939A89AFD45BC7A",
            ["1F/lateral-corte0/None"] = "D0 L5 [-10,66.6053;0,262] 3CCB2C99A540D81554DD59DCBCD56B859E273D20A00B2F5E8E9E6F76AC35515E",
            ["1F/lateral-corte0/Standard"] = "D6 L5 [-10,66.6053;0,262] F30DDD7E9C14061A78471B74CCED403E8DFDC65BB02B9638CAE150536843B3EF",
            ["1F/lateral-corte1/Detailed"] = "D9 L5 [-10,66.6053;0,262] A401D9AA6F2C1195AB33FB59FE9FA106C3111B50936D077B280E079202BA137B",
            ["1F/lateral-corte1/Minimal"] = "D2 L5 [-10,66.6053;0,262] 3D5406BEE31E88074B6FC68B0FC93D37F1CDC87B52BCF109CAF5007BDCFA3B17",
            ["1F/lateral-corte1/None"] = "D0 L5 [-10,66.6053;0,262] 4A27CED79FC86CDC59510336F4F85713BD283F6F1A634257F6F018CE947FDF89",
            ["1F/lateral-corte1/Standard"] = "D6 L5 [-10,66.6053;0,262] 680C154C874A39DA2E76EA6963E04059424C37113507293BB8ED80C7743E8ECE",
            ["1F/lateral-corte2/Detailed"] = "D9 L5 [-10,66.6053;0,262] 92030BB0CAD9FF04D0DAFCDE4AD18A45F0D4CB06093D74C70A3A5EA6F735939D",
            ["1F/lateral-corte2/Minimal"] = "D2 L5 [-10,66.6053;0,262] F26156F7A731C2752DDE452AC070CAB77FEA2D76C71E82200BB8FC4400CCCCBA",
            ["1F/lateral-corte2/None"] = "D0 L5 [-10,66.6053;0,262] 845E1436953771851C06011BFEB21599E2CC957FBB0103F4A539CE7E8D96E275",
            ["1F/lateral-corte2/Standard"] = "D6 L5 [-10,66.6053;0,262] 60C247A7509B1CD464E517ADFB13F41BCF911B12BBCBF1F95F6B2CFB7497E982",
            ["1F/lateral-corte3/Detailed"] = "D9 L5 [-10,66.6053;0,262] 7E25781AE6AECA2CA8F1270216BC4663047E540C1A1B757839D182D74A2198AF",
            ["1F/lateral-corte3/Minimal"] = "D2 L5 [-10,66.6053;0,262] A039211244220AF20153FAAE7486379BB6F86E1E3C56460C97DD40E9EB44091E",
            ["1F/lateral-corte3/None"] = "D0 L5 [-10,66.6053;0,262] E383F1828239A11A7459003ABC53B3E352D14B780B6B04D9B2974F2AA3EB3663",
            ["1F/lateral-corte3/Standard"] = "D6 L5 [-10,66.6053;0,262] 4B02135080067DE07CA128B069F8C0D02A86408FC3855209994165F349BECA78",
            ["1F/planta/Detailed"] = "D5 L4 [-10,49.75;0,308.5] 25C46DBFF5D5B43958D0255456F9B75483C4AE6650AA69C7CFFC77E409D25AE4",
            ["1F/planta/Minimal"] = "D2 L4 [-10,49.75;0,308.5] 1447E483153596670A77FD5E54040A6A56D5CDB639C15ABE4DB4CBC0980EEFC5",
            ["1F/planta/None"] = "D0 L4 [-10,49.75;0,308.5] CB53C756939AA69647FB82A585FF3B065E3E3815636C5CC732EC34A927C1D49E",
            ["1F/planta/Standard"] = "D5 L4 [-10,49.75;0,308.5] 25C46DBFF5D5B43958D0255456F9B75483C4AE6650AA69C7CFFC77E409D25AE4",
            ["2F/frontal-fondo0/Detailed"] = "D11 L7 [-10,-46;248.75,246] CF0FC7292434391A342258FB8F20B00BC83AF7761FD39C28AC4D019008A89F6E",
            ["2F/frontal-fondo0/Minimal"] = "D2 L7 [-10,-24;248.75,246] E3868573911DA145CA16675F4CFCD5A6688DBD398E412CB600DB0BAC98D21ED7",
            ["2F/frontal-fondo0/None"] = "D0 L7 [-10,-10;248.75,246] F17830335325C8DB9638EF4AE133268957A8AD23E6082917E01FC17C73F71DC9",
            ["2F/frontal-fondo0/Standard"] = "D8 L7 [-10,-46;248.75,246] DCE85513028F04E17A5F86EACD9361669E1828DB61151562F9348B8143F8E29A",
            ["2F/frontal-fondo1/Detailed"] = "D8 L5 [-10,-46;149.25,246] 0534DBBBAEB7ECF9723D2FED7C7CD6A2925958B25741E193DB284DB9D88944A4",
            ["2F/frontal-fondo1/Minimal"] = "D2 L5 [-10,-24;149.25,246] A13F3E2FCCCF71CBEA4FD1F9E98687F21B74888D3783C1AD297239A817F48871",
            ["2F/frontal-fondo1/None"] = "D0 L5 [-10,-10;149.25,246] 5AC00FC7853287B35AFA61A1BB996E3F4D17922BC795B3FCFF0C6E55F3BDAB27",
            ["2F/frontal-fondo1/Standard"] = "D6 L5 [-10,-46;149.25,246] 534F20EA544E2A37D2A6F008215424B61AADDCD9ACD1475E10614CC03796B47B",
            ["2F/lateral-corte0/Detailed"] = "D10 L5 [-10,66.6053;0,262] FAD126FC3FEEEAD21CBC650B76B39902273425E3C8DEE8EAE31B74BAB99F0364",
            ["2F/lateral-corte0/Minimal"] = "D2 L5 [-10,66.6053;0,262] DF2F44E22EC5569A5DD82345823A6F9011DFAD75F5140B5F716D0190322142AE",
            ["2F/lateral-corte0/None"] = "D0 L5 [-10,66.6053;0,262] 3CCB2C99A540D81554DD59DCBCD56B859E273D20A00B2F5E8E9E6F76AC35515E",
            ["2F/lateral-corte0/Standard"] = "D7 L5 [-10,66.6053;0,262] F88C31B47C362A99FC3CC7B0614CC179221EA2B7DF98AC2A28A82E219E4A58A8",
            ["2F/lateral-corte1/Detailed"] = "D10 L5 [-10,66.6053;0,262] F89B8822728C3DDDE0593A0D36659278BFDD730E7B9E535762F89039171FBD63",
            ["2F/lateral-corte1/Minimal"] = "D2 L5 [-10,66.6053;0,262] 86F6977357F8F4C76707F71D054D6FEA9826C71512CF3DA007B3C255AC62B596",
            ["2F/lateral-corte1/None"] = "D0 L5 [-10,66.6053;0,262] 4A27CED79FC86CDC59510336F4F85713BD283F6F1A634257F6F018CE947FDF89",
            ["2F/lateral-corte1/Standard"] = "D7 L5 [-10,66.6053;0,262] E48DE48196E15EE79592ECB2D02CE1D63A4BDB99D1F2A6917099F80A79ECCFE0",
            ["2F/lateral-corte2/Detailed"] = "D10 L5 [-10,66.6053;0,262] 7666D28009CD2F2212E749AC7BBB58F2FC2D7275968CD6C87788F699075FD03D",
            ["2F/lateral-corte2/Minimal"] = "D2 L5 [-10,66.6053;0,262] 9BBAF24B5A76EA83CB4F14C3B98729C268C259F6DA12130777163A3C367C34EC",
            ["2F/lateral-corte2/None"] = "D0 L5 [-10,66.6053;0,262] 845E1436953771851C06011BFEB21599E2CC957FBB0103F4A539CE7E8D96E275",
            ["2F/lateral-corte2/Standard"] = "D7 L5 [-10,66.6053;0,262] 1BE3D0357EE5DAA8F4D843DC3EDEAA09B6A6CB397DC0A89ECB45D46E36BB7475",
            ["2F/lateral-corte3/Detailed"] = "D9 L5 [-10,66.6053;0,262] 7E25781AE6AECA2CA8F1270216BC4663047E540C1A1B757839D182D74A2198AF",
            ["2F/lateral-corte3/Minimal"] = "D2 L5 [-10,66.6053;0,262] A039211244220AF20153FAAE7486379BB6F86E1E3C56460C97DD40E9EB44091E",
            ["2F/lateral-corte3/None"] = "D0 L5 [-10,66.6053;0,262] E383F1828239A11A7459003ABC53B3E352D14B780B6B04D9B2974F2AA3EB3663",
            ["2F/lateral-corte3/Standard"] = "D6 L5 [-10,66.6053;0,262] 4B02135080067DE07CA128B069F8C0D02A86408FC3855209994165F349BECA78",
            ["2F/planta/Detailed"] = "D6 L4 [-10,49.75;0,308.5] B2FC7ADB84DB8CFA86745D326FCDF02F9A8B1277DF7548383BAFF4691971F778",
            ["2F/planta/Minimal"] = "D2 L4 [-10,49.75;0,308.5] 3B967874787505CBA9932282F39E54CC33126A2D966E1B9AD96448C16CDEF1F6",
            ["2F/planta/None"] = "D0 L4 [-10,49.75;0,308.5] CB53C756939AA69647FB82A585FF3B065E3E3815636C5CC732EC34A927C1D49E",
            ["2F/planta/Standard"] = "D6 L4 [-10,49.75;0,308.5] B2FC7ADB84DB8CFA86745D326FCDF02F9A8B1277DF7548383BAFF4691971F778",
        };
    }
}
