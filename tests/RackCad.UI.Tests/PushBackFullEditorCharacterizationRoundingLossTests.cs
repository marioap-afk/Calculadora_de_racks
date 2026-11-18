using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — CARACTERIZACION de la perdida (a) en su clase propia (OB-06.3b): la ventana clasica escribe los campos
    /// numericos de sus controles con «0.###», asi que un valor con mas de tres decimales vuelve REDONDEADO a tres. CT-PB0-19
    /// fija los campos que redondean (F1 a F11 de la tabla de MB H-077) y CT-PB0-20 los que no (G1 a G9, MB H-079).
    ///
    /// <para>Cada fila abre una variante de un diseño del corpus cuyo campo vale su base + 0.00044 y fija el valor exacto, el valor
    /// del campo en la <c>Salida</c> de la ventana y su clase: <c>REDONDEA</c> (la salida es R3 del exacto y distinta de el),
    /// <c>CONSERVA</c> (la salida es el exacto) u <c>OTRA</c>. Una fila cuyo diseño no resuelve se fija <c>NO-VÁLIDA</c> tal cual.
    /// Solo estas clases aseveran (a): fuera de CT-PB0-18 a CT-PB0-20 el filtro retira las lineas de esa clase.</para>
    ///
    /// <para>Fija lo que HAY y no corrige nada. Los numeros se escriben con «R» y se comparan en texto ordinal; ninguna ventana se
    /// muestra y ninguna prueba ejecuta Insertar, «Guardar en biblioteca», el BOM ni el configurador real.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullEditorCharacterizationRoundingLossTests
    {
        private const double Increment = 0.00044;

        private sealed class Row
        {
            public string Name;
            public string Fixture;
            public string Describe;
            public double Base;
            public Func<JsonNode, KeyValuePair<JsonNode, string>> Locate;
        }

        private static JsonNode Structure(JsonNode root) => root["PushBack"]["Structure"];

        private static Func<JsonNode, KeyValuePair<JsonNode, string>> InStructure(string key)
            => root => new KeyValuePair<JsonNode, string>(Structure(root), key);

        private static Func<JsonNode, KeyValuePair<JsonNode, string>> InFront0(string key)
            => root => new KeyValuePair<JsonNode, string>(Structure(root)["Fronts"][0], key);

        private static Func<JsonNode, KeyValuePair<JsonNode, string>> InLevel(int front, string key)
            => root => new KeyValuePair<JsonNode, string>(Structure(root)["Fronts"][front]["Levels"][0], key);

        private static Row R(string name, string fixture, string describe, double baseValue, Func<JsonNode, KeyValuePair<JsonNode, string>> locate)
            => new Row { Name = name, Fixture = fixture, Describe = describe, Base = baseValue, Locate = locate };

        /// <summary>La tabla de campos de MB H-077 sobre pb-01-base.</summary>
        private static IEnumerable<Row> Redondean()
        {
            yield return R("F1", "pb-01-base", "Structure.PalletDepth", 48, InStructure("PalletDepth"));
            yield return R("F2", "pb-01-base", "Structure.PostPeralte", 3, InStructure("PostPeralte"));
            yield return R("F3", "pb-01-base", "Structure.PalletTolerance", 4, InStructure("PalletTolerance"));
            yield return R("F4", "pb-01-base", "Structure.BeamDepth", 6, InStructure("BeamDepth"));
            yield return R("F5", "pb-01-base", "Structure.AnnotationScale", 1.5, InStructure("AnnotationScale"));
            yield return R("F6", "pb-01-base", "Structure.Fronts[0].FirstLevelHeight", 4, InFront0("FirstLevelHeight"));
            yield return R("F7", "pb-01-base", "Structure.Fronts[0].Levels[0].PalletFront", 42, InLevel(0, "PalletFront"));
            yield return R("F8", "pb-01-base", "Structure.Fronts[0].Levels[0].PalletHeight", 60, InLevel(0, "PalletHeight"));
            yield return R("F9", "pb-01-base", "Structure.Fronts[0].Levels[0].PalletWeight", 1000, InLevel(0, "PalletWeight"));
            yield return R("F10", "pb-01-base", "Structure.Fronts[0].Levels[0].ClearHeight", 6, InLevel(0, "ClearHeight"));
            yield return R("F11", "pb-01-base", "Structure.Fronts[0].Levels[0].BeamLengthOverride", 100, InLevel(0, "BeamLengthOverride"));
        }

        /// <summary>Las filas de MB H-079: los campos que NO pasan por un control numerico formateado.</summary>
        private static IEnumerable<Row> NoRedondean()
        {
            yield return R("G1", "pb-01-base", "Structure.PalletFront", 42, InStructure("PalletFront"));
            yield return R("G2", "pb-01-base", "Structure.PalletHeight", 60, InStructure("PalletHeight"));
            yield return R("G3", "pb-01-base", "Structure.PalletWeight", 1000, InStructure("PalletWeight"));
            yield return R("G4", "pb-07-combinado", "Structure.DerivedPostHeight", 140, InStructure("DerivedPostHeight"));
            yield return R("G5", "pb-06-seguridad", "Structure.ManualHeaderHeightOverride", 150, InStructure("ManualHeaderHeightOverride"));
            yield return R("G6", "pb-06-seguridad", "Structure.SeparatorSpacingOverride", 18, InStructure("SeparatorSpacingOverride"));
            yield return R("G7", "pb-20-ab", "Composite.Gap", 54, root => new KeyValuePair<JsonNode, string>(root["PushBack"]["Composite"], "Gap"));
            yield return R("G8", "pb-31-v2-modulos", "Structure.Modules[M3].Length", 48, root =>
            {
                var modules = Structure(root)["Modules"].AsArray();
                var module = modules.First(node => node["ModuleId"].GetValue<string>() == "M3");
                return new KeyValuePair<JsonNode, string>(module, "Length");
            });
            yield return R("G9", "pb-01-base", "Structure.Fronts[1].Levels[0].PalletHeight", 60, InLevel(1, "PalletHeight"));
        }

        private static string Number(JsonNode node)
            => node == null ? "<null>" : node.GetValue<double>().ToString("R", CultureInfo.InvariantCulture);

        private static string ClassOfRow(double exact, double output)
        {
            if (output == exact) return "CONSERVA";
            return output == R3(exact) ? "REDONDEA" : "OTRA";
        }

        private static List<string> Measure(IEnumerable<Row> rows)
        {
            var log = new List<string>();
            foreach (var row in rows)
            {
                var exact = row.Base + Increment;
                var text = EditInner(Fixture(row.Fixture), inner =>
                {
                    var target = row.Locate(inner);
                    target.Key[target.Value] = exact;
                });
                var w = Abrir(text);
                try
                {
                    var salida = Salida(w);
                    if (salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal))
                    {
                        log.Add(row.Name + " " + row.Fixture + " " + row.Describe + " exacto=" + exact.ToString("R", CultureInfo.InvariantCulture) + " :: " + salida);
                        continue;
                    }

                    var target = row.Locate(JsonNode.Parse(salida));
                    var written = target.Key[target.Value];
                    var output = written == null ? double.NaN : written.GetValue<double>();
                    log.Add(row.Name + " " + row.Fixture + " " + row.Describe + " exacto=" + exact.ToString("R", CultureInfo.InvariantCulture)
                        + " salida=" + Number(written) + " " + (written == null ? "OTRA" : ClassOfRow(exact, output)));
                }
                finally { Cerrar(w); }
            }

            return log;
        }

        [Fact(Skip = "SUSTITUIDA por PB-TC (D-PB-06), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-03: la ejecuta PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading")]
        public void CT_PB0_19_TheFieldsThatPassThroughAFormattedControlComeBackRoundedToThreeDecimals()
        {
            var log = new List<string>();
            Sta(() => log.AddRange(Measure(Redondean())));

            Assert.Equal(
                new[]
                {
                    "F1 pb-01-base Structure.PalletDepth exacto=48.00044 salida=48 REDONDEA",
                    "F2 pb-01-base Structure.PostPeralte exacto=3.00044 salida=3 REDONDEA",
                    "F3 pb-01-base Structure.PalletTolerance exacto=4.00044 salida=4 REDONDEA",
                    "F4 pb-01-base Structure.BeamDepth exacto=6.00044 salida=6 REDONDEA",
                    "F5 pb-01-base Structure.AnnotationScale exacto=1.50044 salida=1.5 REDONDEA",
                    "F6 pb-01-base Structure.Fronts[0].FirstLevelHeight exacto=4.00044 salida=4 REDONDEA",
                    "F7 pb-01-base Structure.Fronts[0].Levels[0].PalletFront exacto=42.00044 salida=42 REDONDEA",
                    "F8 pb-01-base Structure.Fronts[0].Levels[0].PalletHeight exacto=60.00044 salida=60 REDONDEA",
                    "F9 pb-01-base Structure.Fronts[0].Levels[0].PalletWeight exacto=1000.00044 salida=1000 REDONDEA",
                    "F10 pb-01-base Structure.Fronts[0].Levels[0].ClearHeight exacto=6.00044 salida=6 REDONDEA",
                    "F11 pb-01-base Structure.Fronts[0].Levels[0].BeamLengthOverride exacto=100.00044 salida=100 REDONDEA",
                },
                log);
        }

        [Fact]
        public void CT_PB0_20_TheFieldsThatDoNotPassThroughAFormattedControlKeepTheirExactValue()
        {
            var log = new List<string>();
            Sta(() => log.AddRange(Measure(NoRedondean())));

            Assert.Equal(
                new[]
                {
                    "G1 pb-01-base Structure.PalletFront exacto=42.00044 salida=42.00044 CONSERVA",
                    "G2 pb-01-base Structure.PalletHeight exacto=60.00044 salida=60.00044 CONSERVA",
                    "G3 pb-01-base Structure.PalletWeight exacto=1000.00044 salida=1000.00044 CONSERVA",
                    "G4 pb-07-combinado Structure.DerivedPostHeight exacto=140.00044 salida=140.00044 CONSERVA",
                    "G5 pb-06-seguridad Structure.ManualHeaderHeightOverride exacto=150.00044 salida=150.00044 CONSERVA",
                    "G6 pb-06-seguridad Structure.SeparatorSpacingOverride exacto=18.00044 salida=18.00044 CONSERVA",
                    "G7 pb-20-ab Composite.Gap exacto=54.00044 salida=54.00044 CONSERVA",
                    "G8 pb-31-v2-modulos Structure.Modules[M3].Length exacto=48.00044 salida=48.00044 CONSERVA",
                    "G9 pb-01-base Structure.Fronts[1].Levels[0].PalletHeight exacto=60.00044 salida=60.00044 CONSERVA",
                },
                log);
        }
    }
}
