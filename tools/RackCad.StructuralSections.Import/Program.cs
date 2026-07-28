using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RackCad.Application.StructuralSections;

namespace RackCad.StructuralSections.Import
{
    /// <summary>
    /// Command line of the reproducible importer.
    ///
    /// It takes a LOCAL workbook and never downloads anything: RackCad must not depend on a network at runtime
    /// or at build time, and the provenance of the data must be a file whose SHA-256 someone checked. The
    /// workbook is not versioned in the repository either — <c>docs/guias/secciones-estructurales.md</c>
    /// explains where to get it and where to put it.
    ///
    ///   dotnet run --project tools/RackCad.StructuralSections.Import --
    ///     --workbook &lt;ruta al .xlsx&gt; --output assets/catalogs [--worksheet "Database v16.0"] [--check]
    ///
    /// <c>--check</c> compares against what is already on disk and changes nothing; it is how CI or a human
    /// verifies that the distributed catalog still is what this workbook produces.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                if (!TryParse(args, out var options))
                {
                    Console.Error.WriteLine(Usage);
                    return 2;
                }

                var importer = new AiscShapesImporter();
                var result = importer.Import(options.WorkbookPath, options.Worksheet);

                var existingStatus = ImportOutputWriter.ReadExistingStatus(options.OutputDirectory);
                var known = new HashSet<StructuralSectionId>(result.Sections.Select(section => section.SectionId));
                var preserved = existingStatus.Where(entry => known.Contains(entry.SectionId)).ToArray();
                var dropped = existingStatus.Where(entry => !known.Contains(entry.SectionId)).ToArray();

                if (dropped.Length > 0)
                {
                    // Never silently: an overlay entry whose section disappeared is a decision that would be
                    // lost, and the operator has to see it.
                    Console.Error.WriteLine(
                        "AVISO: el overlay de estado referencia " + dropped.Length +
                        " id(s) que esta importacion ya no produce: " +
                        string.Join(", ", dropped.Select(entry => entry.SectionId.Value)) + ".");
                }

                var output = ImportOutputWriter.Build(result, preserved);

                Report(result, output);

                if (options.CheckOnly)
                {
                    return Compare(output, options.OutputDirectory) ? 0 : 1;
                }

                ImportOutputWriter.Publish(output, options.OutputDirectory);
                Console.WriteLine("Escritos " + output.Files.Count + " archivos en '" + options.OutputDirectory + "'.");
                return 0;
            }
            catch (AiscRowRejectedException ex)
            {
                Console.Error.WriteLine("FILA SELECCIONADA RECHAZADA: " + ex.Message);
                return 1;
            }
            catch (XlsxFormatException ex)
            {
                Console.Error.WriteLine("LIBRO NO VALIDO: " + ex.Message);
                return 1;
            }
            catch (StructuralSectionCsvException ex)
            {
                Console.Error.WriteLine("CSV NO VALIDO: " + ex.Message);
                return 1;
            }
        }

        private const string Usage =
            "Uso: --workbook <ruta.xlsx> --output <directorio> [--worksheet <nombre>] [--check]";

        private sealed class Options
        {
            public string WorkbookPath;
            public string OutputDirectory;
            public string Worksheet = AiscShapesImporter.DefaultWorksheetName;
            public bool CheckOnly;
        }

        private static bool TryParse(string[] args, out Options options)
        {
            options = new Options();

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--workbook":
                        if (++i >= args.Length) return false;
                        options.WorkbookPath = args[i];
                        break;
                    case "--output":
                        if (++i >= args.Length) return false;
                        options.OutputDirectory = args[i];
                        break;
                    case "--worksheet":
                        if (++i >= args.Length) return false;
                        options.Worksheet = args[i];
                        break;
                    case "--check":
                        options.CheckOnly = true;
                        break;
                    default:
                        return false;
                }
            }

            return !string.IsNullOrWhiteSpace(options.WorkbookPath) &&
                   !string.IsNullOrWhiteSpace(options.OutputDirectory);
        }

        private static void Report(AiscImportResult result, ImportOutput output)
        {
            Console.WriteLine("Libro   : " + result.SourceFileName);
            Console.WriteLine("SHA-256 : " + result.SourceSha256);
            Console.WriteLine("Hoja    : " + result.Worksheet);
            Console.WriteLine("Filas de datos      : " + result.TotalDataRows);
            Console.WriteLine("Filas seleccionadas : " + result.SelectedRowCount);
            Console.WriteLine("Secciones importadas: " + result.Sections.Count);

            foreach (var family in StructuralSectionFamilies.All)
            {
                Console.WriteLine("  " + StructuralSectionFamilies.ToToken(family).PadRight(9) + " " +
                                  result.Sections.Count(section => section.Family == family));
            }

            Console.WriteLine("Tipos excluidos (reportados, no son error):");

            foreach (var pair in result.ExcludedTypeCounts.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                Console.WriteLine("  " + pair.Key.PadRight(9) + " " + pair.Value);
            }

            Console.WriteLine("Contraste con el bloque metrico oficial:");

            foreach (var check in result.MetricCoherence)
            {
                Console.WriteLine(
                    "  " + check.Header.PadRight(6) + " n=" + check.Compared.ToString().PadLeft(4) +
                    "  desviacion maxima " +
                    check.MaxRelativeDeviation.ToString("P3", CultureInfo.InvariantCulture) +
                    " (tolerancia " + check.Tolerance.ToString("P1", CultureInfo.InvariantCulture) + ")" +
                    (check.WorstDesignation == null ? string.Empty : " en " + check.WorstDesignation));
            }

            Console.WriteLine("Archivos generados:");

            foreach (var file in output.Manifest.Files)
            {
                Console.WriteLine("  " + file.Name.PadRight(36) + " " + file.Sha256);
            }
        }

        private static bool Compare(ImportOutput output, string outputDirectory)
        {
            var identical = true;

            foreach (var file in output.Files)
            {
                var path = Path.Combine(outputDirectory, file.Key);

                if (!File.Exists(path))
                {
                    Console.Error.WriteLine("FALTA: " + file.Key);
                    identical = false;
                    continue;
                }

                var onDisk = File.ReadAllText(path);

                if (!string.Equals(onDisk, file.Value, StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("DIFIERE: " + file.Key);
                    identical = false;
                }
            }

            Console.WriteLine(identical
                ? "El catalogo distribuido coincide exactamente con el que produce este libro."
                : "El catalogo distribuido NO coincide con el que produce este libro.");

            return identical;
        }
    }
}
