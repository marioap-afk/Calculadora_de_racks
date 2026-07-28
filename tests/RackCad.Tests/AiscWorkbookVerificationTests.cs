using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.StructuralSections.Import;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F3 — el importador no puede etiquetar como AISC v16.0 cualquier libro cuyos encabezados encajen.
    ///
    /// Antes lo hacía: leía la hoja que le dijeran y, si las columnas cuadraban, estampaba «AISC Shapes
    /// Database v16.0» en el manifiesto. Un fork, una copia editada a mano, otra revisión o la exportación de
    /// otro proveedor habrían quedado catalogados como la fuente oficial.
    ///
    /// La identidad se verifica por CONTENIDO Y ESTRUCTURA, no por el SHA-256 del archivo: fijar el hash
    /// actual como único libro admisible haría imposible importar cualquier revisión futura legítima. El hash
    /// se sigue REGISTRANDO en el manifiesto como procedencia; no es la compuerta.
    /// </summary>
    public class AiscWorkbookVerificationTests : IDisposable
    {
        private readonly List<string> _files = new List<string>();

        [Fact]
        public void ACorrectWorkbookIsAccepted()
        {
            var result = new AiscShapesImporter().Import(Workbook());

            Assert.Equal("16.0", result.Source.Revision);
            Assert.Equal("Database v16.0", result.Worksheet);
            Assert.Single(result.Sections);
        }

        [Fact]
        public void AWorkbookWithoutAReadmeIsRejected()
        {
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(Workbook(includeReadme: false)));

            Assert.Contains("Readme", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AnEmptyReadmeIsRejected()
        {
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(Workbook(readme: new string[0])));

            Assert.Contains("Readme", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AReadmeOfAnotherRevisionIsRejected()
        {
            // v15.0: mismo formato, mismos encabezados, otro producto.
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(
                    Workbook(readme: SyntheticAiscWorkbook.PreviousRevisionReadme)));

            Assert.Contains("AISC SHAPES DATABASE V16.0", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AReadmeWithoutTheManualEditionIsRejected()
        {
            var readme = SyntheticAiscWorkbook.ValidReadme
                .Select(line => line.Replace("16th Edition, 1st Printing", "sin edicion"))
                .ToArray();

            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(Workbook(readme: readme)));

            Assert.Contains("16TH EDITION", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AReadmeWithoutTheEdiConventionIsRejected()
        {
            var readme = SyntheticAiscWorkbook.ValidReadme
                .Where(line => !line.Contains("Electronic Data Interchange"))
                .ToArray();

            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(Workbook(readme: readme)));

            Assert.Contains("ELECTRONIC DATA INTERCHANGE", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AWorkbookWithoutTheV16DataSheetIsRejected()
        {
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(Workbook(dataSheetName: "Datos")));

            Assert.Contains("Database v16.0", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ARevisionThatContradictsTheDataSheetNameIsRejected()
        {
            // El Readme acredita la 16.0 y la hoja dice ser de otra revisión: la incoherencia se detecta.
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(Workbook(dataSheetName: "Database v15.0")));

            Assert.Contains("Database v16.0", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void TheMetadataItGeneratesAgreesWithWhatTheWorkbookProved()
        {
            var result = new AiscShapesImporter().Import(Workbook());

            Assert.Equal(AiscWorkbookVerifier.SupportedRevision, result.Source.Revision);
            Assert.Equal(
                AiscWorkbookVerifier.DataWorksheetFor(result.Source.Revision), result.Worksheet);
            Assert.Equal("AISC Shapes Database v16.0", result.Source.Title);
        }

        [Fact]
        public void TheWorkbookHashIsRecordedButIsNotTheGate()
        {
            // Dos libros DISTINTOS (distinto contenido, luego distinto SHA-256) se aceptan los dos, porque lo
            // que se verifica es la identidad publicada, no un hash concreto.
            var first = new AiscShapesImporter().Import(Workbook());
            var second = new AiscShapesImporter().Import(
                Workbook(SyntheticAiscWorkbook.W("W12X30", 30), SyntheticAiscWorkbook.W("W12X26", 26)));

            Assert.NotEqual(first.SourceSha256, second.SourceSha256);
            Assert.Matches("^[0-9A-F]{64}$", first.SourceSha256);
            Assert.Matches("^[0-9A-F]{64}$", second.SourceSha256);
        }

        [Fact]
        public void ThereIsNoPublicWayToPointTheImporterAtAnotherSheet()
        {
            // La CLI no expone --worksheet y el método público no toma el nombre de la hoja: la hoja de datos
            // sale de lo que el libro acredita, nunca de un argumento.
            var overloads = typeof(AiscShapesImporter)
                .GetMethods()
                .Where(method => method.Name == nameof(AiscShapesImporter.Import))
                .ToArray();

            var overload = Assert.Single(overloads);
            var parameter = Assert.Single(overload.GetParameters());

            Assert.Equal("workbookPath", parameter.Name);
        }

        private string Workbook(params SyntheticAiscWorkbook.RowBuilder[] rows) =>
            Workbook(rows, null, true, SyntheticAiscWorkbook.SheetName);

        private string Workbook(
            string[] readme = null,
            bool includeReadme = true,
            string dataSheetName = SyntheticAiscWorkbook.SheetName) =>
            Workbook(null, readme, includeReadme, dataSheetName);

        private string Workbook(
            SyntheticAiscWorkbook.RowBuilder[] rows,
            string[] readme,
            bool includeReadme,
            string dataSheetName)
        {
            var path = SyntheticAiscWorkbook.WriteToTempFile(
                rows != null && rows.Length > 0
                    ? rows
                    : new[] { SyntheticAiscWorkbook.W("W12X26", 26) },
                headers: null,
                readme: readme,
                includeReadme: includeReadme,
                dataSheetName: dataSheetName);

            _files.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var path in _files.Where(File.Exists))
            {
                try { File.Delete(path); } catch (IOException) { }
            }
        }
    }
}
