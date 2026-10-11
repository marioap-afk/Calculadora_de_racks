using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.StructuralSections;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using K = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamElementsDiagnosticCodes;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T1 — D-06 and D-07: the strict reader and validator of the provider table (<c>rackcad-ram-elements-section-map/v1</c>),
    /// and the data file the product ships. A table that is not valid is never projected; a proposed row assigns nothing.
    /// </summary>
    public class StructuralAnalysisRamElementsMapReaderTests
    {
        // ---------------------------------------------------------------- the data file the product ships

        [Fact]
        public void ElArchivoDeDatosEsValidoYTieneExactamenteLasDosFilasProposedDeRefCant01()
        {
            var result = F.Read(F.DataFileText());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Table);
            Assert.Equal(
                new[] { "AISC-HSS-RECT-HSS4X4X_250", "AISC-W-W12X26" },
                result.Table.Rows.Select(r => r.SectionId.Value).ToArray());
            Assert.All(result.Table.Rows, row => Assert.Equal(RamSectionMapStatus.Proposed, row.Status));
            Assert.All(result.Table.Rows, row => Assert.False(row.IsVerified));
        }

        [Fact]
        public void LasDosFilasDelArchivoLlevanLosNombresQueRamAceptoEnP1()
        {
            var table = F.Read(F.DataFileText()).Table;

            Assert.NotNull(table);
            Assert.True(table.TryGetRamRow(StructuralSectionId.Parse(F.ColumnSection), out var column));
            Assert.Equal("W 12X26", column.RamSectionName);
            Assert.Equal("ASTM-A992-GR50", column.MaterialId.Value);
            Assert.Equal("A992 Gr50", column.RamMaterial);

            Assert.True(table.TryGetRamRow(StructuralSectionId.Parse(F.ArmSection), out var arm));
            Assert.Equal("HSS_SQR 4X4X1_4", arm.RamSectionName);
            Assert.Equal("ASTM-A500-GRC", arm.MaterialId.Value);
            Assert.Equal("A500 GrC rectangular", arm.RamMaterial);
        }

        [Fact]
        public void ElArchivoDeDatosNoTrajoValoresDeIngenieriaNiVerificacion()
        {
            var table = F.Read(F.DataFileText()).Table;

            Assert.NotNull(table);
            Assert.Equal(2, table.Rows.Count);

            foreach (var row in table.Rows)
            {
                Assert.Null(row.RamSectionTable);
                Assert.Null(row.RamSectionCountry);
                Assert.Null(row.VerifiedBy);
                Assert.Null(row.VerifiedOn);
                Assert.Null(row.ExpectedMaterial.ElasticModulus);
                Assert.Null(row.ExpectedMaterial.PoissonRatio);
                Assert.Null(row.ExpectedMaterial.ShearModulus);
                Assert.Null(row.ExpectedMaterial.UnitWeight);
                Assert.Null(row.ExpectedMaterial.Source);
            }
        }

        [Fact]
        public void ConLaTablaTalComoSeEntregaTodaExportacionEsUnsupportedYNoSeAsignaNingunMaterial()
        {
            var table = F.Read(F.DataFileText()).Table;
            Assert.NotNull(table);

            var result = F.BuildRef(table);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Equal(
                2,
                result.Diagnostics.Count(d => d.Code == StructuralDiagnosticCodes.MaterialRowUnverified));
        }

        // ---------------------------------------------------------------- a valid table

        [Fact]
        public void UnaTablaConFilasVerifiedCompletasEsValida()
        {
            var result = F.Read(F.RefDocument());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics.Where(d => d.IsBlocking));
            Assert.NotNull(result.Table);
            Assert.All(result.Table.Rows, row => Assert.True(row.IsVerified));
            Assert.Equal("TEST-FIXTURE", result.Table.Rows[0].VerifiedBy);
            Assert.Equal("2026-10-11", result.Table.Rows[0].VerifiedOn);
        }

        [Fact]
        public void LasFilasSeOrdenanPorIdDeSeccionOrdinalSinImportarElOrdenDelArchivo()
        {
            var forward = F.Table(F.Document(F.ColumnRow(), F.ArmRow()));
            var reverse = F.Table(F.Document(F.ArmRow(), F.ColumnRow()));

            Assert.Equal(forward.Rows.Select(r => r.SectionId.Value), reverse.Rows.Select(r => r.SectionId.Value));
            Assert.Equal(forward.Rows.Select(r => r.CanonicalText), reverse.Rows.Select(r => r.CanonicalText));
        }

        [Fact]
        public void UnaFilaProposedPuedeTenerTodoVacioYUnaTablaSinFilasEsValida()
        {
            var row = F.ColumnRow();
            row["ramSection"]["table"] = null;
            row["ramSection"]["country"] = null;
            row["expectedMaterial"] = new JsonObject
            {
                ["elasticModulus"] = null, ["poissonRatio"] = null, ["shearModulus"] = null, ["unitWeight"] = null, ["source"] = null,
            };
            row["status"] = "proposed";
            row["verifiedBy"] = null;
            row["verifiedOn"] = null;

            var proposed = F.Read(F.Document(row));
            var empty = F.Read(F.Document());

            Assert.Equal(StructuralExportOutcome.Valid, proposed.Outcome);
            Assert.Equal(RamSectionMapStatus.Proposed, proposed.Table.Rows.Single().Status);
            Assert.Equal(StructuralExportOutcome.Valid, empty.Outcome);
            Assert.Empty(empty.Table.Rows);
        }

        [Fact]
        public void UnaFilaVerifiedAdmiteElModuloDeCorteEnLugarDeLaRazonDePoisson()
        {
            var row = F.ColumnRow();
            row["expectedMaterial"]["poissonRatio"] = null;
            row["expectedMaterial"]["shearModulus"] = new JsonObject { ["value"] = 44.5, ["unit"] = "psi" };

            var result = F.Read(F.Document(row));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(44.5, result.Table.Rows.Single().ExpectedMaterial.ShearModulus.Value);
            Assert.Equal("psi", result.Table.Rows.Single().ExpectedMaterial.ShearModulus.Unit);
            Assert.Null(result.Table.Rows.Single().ExpectedMaterial.PoissonRatio);
        }

        [Theory]
        [InlineData("psi", "pcf")]
        [InlineData("psi", "lbf/in3")]
        [InlineData("ksi", "pcf")]
        [InlineData("ksi", "lbf/in3")]
        public void LasUnidadesDeLasTablasCerradasSeAceptanYNoSeConvierten(string modulusUnit, string weightUnit)
        {
            var row = F.ColumnRow();
            row["expectedMaterial"]["elasticModulus"]["unit"] = modulusUnit;
            row["expectedMaterial"]["unitWeight"]["unit"] = weightUnit;

            var result = F.Read(F.Document(row));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            var expected = result.Table.Rows.Single().ExpectedMaterial;
            Assert.Equal(modulusUnit, expected.ElasticModulus.Unit);
            Assert.Equal(111.5, expected.ElasticModulus.Value);
            Assert.Equal(weightUnit, expected.UnitWeight.Unit);
            Assert.Equal(222.5, expected.UnitWeight.Value);
        }

        [Fact]
        public void LaUnidadSeValidaTambienEnUnaFilaProposed()
        {
            var row = F.ColumnRow();
            row["status"] = "proposed";
            row["expectedMaterial"]["elasticModulus"]["unit"] = "MPa";

            var result = F.Read(F.Document(row));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Table);
            Assert.Contains(result.Diagnostics, d => d.Code == K.MapUnitUnknown);
        }

        // ---------------------------------------------------------------- an invalid table is never projected

        public static IEnumerable<object[]> InvalidDocuments()
        {
            var cases = new List<(string Name, string Json, string Code)>();

            void Add(string name, string json, string code) => cases.Add((name, json, code));

            void Row(string name, string code, Action<JsonObject> mutate)
            {
                var row = F.ColumnRow();
                mutate(row);
                cases.Add((name, F.Document(row, F.ArmRow()), code));
            }

            // the document
            Add("vacio", string.Empty, K.MapJsonMalformed);
            Add("basura", "{ not json", K.MapJsonMalformed);
            Add("coma-final", "{ \"schema\": \"" + F.Schema + "\", \"rows\": [], }", K.MapJsonMalformed);
            Add("comentario", "{ /* c */ \"schema\": \"" + F.Schema + "\", \"rows\": [] }", K.MapJsonMalformed);
            Add("dos-raices", "{ \"schema\": \"" + F.Schema + "\", \"rows\": [] } {}", K.MapJsonMalformed);
            Add("raiz-arreglo", "[]", K.MapWrongType);
            Add("esquema-otra-version", "{ \"schema\": \"rackcad-ram-elements-section-map/v2\", \"rows\": [] }", K.MapSchemaMismatch);
            Add("esquema-numero", "{ \"schema\": 1, \"rows\": [] }", K.MapWrongType);
            Add("raiz-propiedad-desconocida", "{ \"schema\": \"" + F.Schema + "\", \"rows\": [], \"extra\": 1 }", K.MapUnknownProperty);
            Add("raiz-propiedad-duplicada", "{ \"schema\": \"" + F.Schema + "\", \"schema\": \"" + F.Schema + "\", \"rows\": [] }", K.MapDuplicateProperty);
            Add("raiz-sin-rows", "{ \"schema\": \"" + F.Schema + "\" }", K.MapMissingProperty);
            Add("rows-no-es-arreglo", "{ \"schema\": \"" + F.Schema + "\", \"rows\": {} }", K.MapWrongType);
            Add("fila-no-es-objeto", "{ \"schema\": \"" + F.Schema + "\", \"rows\": [ 1 ] }", K.MapWrongType);

            // the row: unknown, duplicate and missing properties
            Row("fila-propiedad-desconocida", K.MapUnknownProperty, row => row["extra"] = "x");
            Row("ramSection-propiedad-desconocida", K.MapUnknownProperty, row => row["ramSection"]["extra"] = "x");
            Row("expectedMaterial-propiedad-desconocida", K.MapUnknownProperty, row => row["expectedMaterial"]["extra"] = "x");
            Row("cantidad-propiedad-desconocida", K.MapUnknownProperty, row => row["expectedMaterial"]["elasticModulus"]["extra"] = "x");
            Add(
                "fila-propiedad-duplicada",
                F.Document(F.ColumnRow()).Replace("\"status\":\"verified\"", "\"status\":\"verified\",\"status\":\"verified\"", StringComparison.Ordinal),
                K.MapDuplicateProperty);
            Add(
                "ramSection-propiedad-duplicada",
                F.Document(F.ColumnRow()).Replace("\"name\":\"W 12X26\"", "\"name\":\"W 12X26\",\"name\":\"W 12X26\"", StringComparison.Ordinal),
                K.MapDuplicateProperty);

            foreach (var property in new[] { "sectionId", "ramSection", "materialId", "ramMaterial", "expectedMaterial", "status", "verifiedBy", "verifiedOn" })
            {
                var captured = property;
                Row("fila-sin-" + property, K.MapMissingProperty, row => row.Remove(captured));
            }

            Row("ramSection-sin-name", K.MapMissingProperty, row => ((JsonObject)row["ramSection"]).Remove("name"));
            Row("ramSection-sin-table", K.MapMissingProperty, row => ((JsonObject)row["ramSection"]).Remove("table"));
            Row("ramSection-sin-country", K.MapMissingProperty, row => ((JsonObject)row["ramSection"]).Remove("country"));

            foreach (var property in new[] { "elasticModulus", "poissonRatio", "shearModulus", "unitWeight", "source" })
            {
                var captured = property;
                Row("expectedMaterial-sin-" + property, K.MapMissingProperty, row => ((JsonObject)row["expectedMaterial"]).Remove(captured));
            }

            Row("cantidad-sin-value", K.MapMissingProperty, row => ((JsonObject)row["expectedMaterial"]["elasticModulus"]).Remove("value"));
            Row("cantidad-sin-unit", K.MapMissingProperty, row => ((JsonObject)row["expectedMaterial"]["elasticModulus"]).Remove("unit"));

            // types
            Row("sectionId-numero", K.MapWrongType, row => row["sectionId"] = 7);
            Row("materialId-numero", K.MapWrongType, row => row["materialId"] = 7);
            Row("ramMaterial-numero", K.MapWrongType, row => row["ramMaterial"] = 7);
            Row("ramSection-cadena", K.MapWrongType, row => row["ramSection"] = "W 12X26");
            Row("ramSection-name-numero", K.MapWrongType, row => row["ramSection"]["name"] = 7);
            Row("ramSection-table-numero", K.MapWrongType, row => row["ramSection"]["table"] = 7);
            Row("expectedMaterial-cadena", K.MapWrongType, row => row["expectedMaterial"] = "x");
            Row("cantidad-cadena", K.MapWrongType, row => row["expectedMaterial"]["elasticModulus"] = "111");
            Row("cantidad-value-cadena", K.MapWrongType, row => row["expectedMaterial"]["elasticModulus"]["value"] = "111");
            Row("cantidad-unit-numero", K.MapWrongType, row => row["expectedMaterial"]["elasticModulus"]["unit"] = 7);
            Row("poisson-cadena", K.MapWrongType, row => row["expectedMaterial"]["poissonRatio"] = "0.25");
            Row("source-numero", K.MapWrongType, row => row["expectedMaterial"]["source"] = 7);
            Row("status-numero", K.MapWrongType, row => row["status"] = 1);
            Row("verifiedBy-numero", K.MapWrongType, row => row["verifiedBy"] = 1);
            Row("verifiedOn-numero", K.MapWrongType, row => row["verifiedOn"] = 20261011);

            // ids and names
            Row("sectionId-vacio", K.MapSectionIdInvalid, row => row["sectionId"] = string.Empty);
            Row("sectionId-en-blanco", K.MapSectionIdInvalid, row => row["sectionId"] = "   ");
            Row("sectionId-con-espacio", K.MapSectionIdInvalid, row => row["sectionId"] = "AISC W W12X26");
            Row("sectionId-en-minusculas", K.MapSectionIdInvalid, row => row["sectionId"] = "aisc-w-w12x26");
            Row("materialId-vacio", K.MapTextInvalid, row => row["materialId"] = string.Empty);
            Row("materialId-en-blanco", K.MapTextInvalid, row => row["materialId"] = "  ");
            Row("materialId-no-imprimible", K.MapTextInvalid, row => row["materialId"] = "A\u0001B");
            Row("ramMaterial-vacio", K.MapTextInvalid, row => row["ramMaterial"] = string.Empty);
            Row("ramMaterial-no-imprimible", K.MapTextInvalid, row => row["ramMaterial"] = "A992\tGr50");
            Row("ramSection-name-vacio", K.MapTextInvalid, row => row["ramSection"]["name"] = string.Empty);
            Row("ramSection-name-en-blanco", K.MapTextInvalid, row => row["ramSection"]["name"] = "   ");
            Row("ramSection-name-no-imprimible", K.MapTextInvalid, row => row["ramSection"]["name"] = "W 12X26\n");
            Row("ramSection-table-vacia", K.MapTextInvalid, row => row["ramSection"]["table"] = string.Empty);
            Row("ramSection-country-no-imprimible", K.MapTextInvalid, row => row["ramSection"]["country"] = "US\u0007");
            Row("source-vacia", K.MapTextInvalid, row => row["expectedMaterial"]["source"] = string.Empty);
            Row("verifiedBy-vacio", K.MapTextInvalid, row => row["verifiedBy"] = string.Empty);

            Add(
                "sectionId-repetido",
                F.Document(F.ColumnRow(), F.ColumnRow()),
                K.MapSectionIdDuplicate);

            // closed tables of units and values
            Row("modulo-unidad-MPa", K.MapUnitUnknown, row => row["expectedMaterial"]["elasticModulus"]["unit"] = "MPa");
            Row("modulo-unidad-mayusculas", K.MapUnitUnknown, row => row["expectedMaterial"]["elasticModulus"]["unit"] = "KSI");
            Row("modulo-unidad-pcf", K.MapUnitUnknown, row => row["expectedMaterial"]["elasticModulus"]["unit"] = "pcf");
            Row("modulo-unidad-vacia", K.MapUnitUnknown, row => row["expectedMaterial"]["elasticModulus"]["unit"] = string.Empty);
            Row("peso-unidad-psf", K.MapUnitUnknown, row => row["expectedMaterial"]["unitWeight"]["unit"] = "psf");
            Row("peso-unidad-lbf-ft3", K.MapUnitUnknown, row => row["expectedMaterial"]["unitWeight"]["unit"] = "lbf/ft3");
            Row("peso-unidad-ksi", K.MapUnitUnknown, row => row["expectedMaterial"]["unitWeight"]["unit"] = "ksi");
            Row(
                "corte-unidad-pcf",
                K.MapUnitUnknown,
                row =>
                {
                    row["expectedMaterial"]["poissonRatio"] = null;
                    row["expectedMaterial"]["shearModulus"] = new JsonObject { ["value"] = 4.5, ["unit"] = "pcf" };
                });
            Row("modulo-cero", K.MapValueInvalid, row => row["expectedMaterial"]["elasticModulus"]["value"] = 0);
            Row("modulo-negativo", K.MapValueInvalid, row => row["expectedMaterial"]["elasticModulus"]["value"] = -1);
            Row("peso-negativo", K.MapValueInvalid, row => row["expectedMaterial"]["unitWeight"]["value"] = -222.5);
            Row("poisson-un-medio", K.MapValueInvalid, row => row["expectedMaterial"]["poissonRatio"] = 0.5);
            Row("poisson-menos-uno", K.MapValueInvalid, row => row["expectedMaterial"]["poissonRatio"] = -1);

            // status and date
            Row("status-desconocido", K.MapStatusInvalid, row => row["status"] = "approved");
            Row("status-mayusculas", K.MapStatusInvalid, row => row["status"] = "Verified");
            Row("fecha-con-barras", K.MapDateInvalid, row => row["verifiedOn"] = "2026/10/11");
            Row("fecha-dia-primero", K.MapDateInvalid, row => row["verifiedOn"] = "11-10-2026");
            Row("fecha-inexistente", K.MapDateInvalid, row => row["verifiedOn"] = "2026-02-30");
            Row("fecha-con-hora", K.MapDateInvalid, row => row["verifiedOn"] = "2026-10-11T00:00:00");

            // a VERIFIED row needs every field
            Row("verified-sin-table", K.MapVerifiedIncomplete, row => row["ramSection"]["table"] = null);
            Row("verified-sin-country", K.MapVerifiedIncomplete, row => row["ramSection"]["country"] = null);
            Row("verified-sin-modulo", K.MapVerifiedIncomplete, row => row["expectedMaterial"]["elasticModulus"] = null);
            Row("verified-sin-peso", K.MapVerifiedIncomplete, row => row["expectedMaterial"]["unitWeight"] = null);
            Row("verified-sin-poisson-ni-corte", K.MapVerifiedIncomplete, row => row["expectedMaterial"]["poissonRatio"] = null);
            Row("verified-sin-fuente", K.MapVerifiedIncomplete, row => row["expectedMaterial"]["source"] = null);
            Row("verified-sin-verificador", K.MapVerifiedIncomplete, row => row["verifiedBy"] = null);
            Row("verified-sin-fecha", K.MapVerifiedIncomplete, row => row["verifiedOn"] = null);

            return cases.Select(c => new object[] { c.Name, c.Json, c.Code });
        }

        [Theory]
        [MemberData(nameof(InvalidDocuments))]
        public void UnaTablaInvalidaEsInvalidaConSuCodigoYNoSeProyecta(string name, string json, string code)
        {
            var result = F.Read(json);

            Assert.True(result.Outcome == StructuralExportOutcome.Invalid, name + ": resultado " + result.Outcome);
            Assert.Null(result.Table);
            Assert.True(
                result.Diagnostics.Any(d => d.Code == code && d.IsBlocking && d.ImpliedOutcome == StructuralExportOutcome.Invalid),
                name + ": se esperaba " + code + " y hay " + F.Codes(result.Diagnostics));
        }

        [Fact]
        public void LaLecturaReportaTodosLosHallazgosYNoSoloElPrimero()
        {
            var first = F.ColumnRow();
            first["status"] = "approved";
            var second = F.ArmRow();
            second["expectedMaterial"]["unitWeight"]["unit"] = "psf";

            var result = F.Read(F.Document(first, second));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == K.MapStatusInvalid);
            Assert.Contains(result.Diagnostics, d => d.Code == K.MapUnitUnknown);
        }

        [Fact]
        public void UnTextoNuloOMalformadoNoLanzaExcepcion()
        {
            var result = F.Read(null);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Table);
            Assert.Contains(result.Diagnostics, d => d.Code == K.MapJsonMalformed);
        }
    }
}
