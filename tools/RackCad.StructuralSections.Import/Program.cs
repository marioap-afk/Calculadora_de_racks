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
    ///     --workbook &lt;ruta al .xlsx&gt; --output assets/catalogs [--check]
    ///
    /// There is deliberately NO <c>--worksheet</c> flag. The data sheet is whatever the workbook's own Readme
    /// proves it to be, so no argument can make another sheet or another revision be labelled v16.0.
    ///
    /// <c>--check</c> compares against what is already on disk and changes nothing; it is how CI or a human
    /// verifies that the distributed catalog still is what this workbook produces. It reports the reproducible
    /// data and the local overlay SEPARATELY, because they answer different questions.
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
                var result = importer.Import(options.WorkbookPath);

                RequireOverlayStillResolves(result, options.OutputDirectory);

                var output = ImportOutputWriter.Build(result);

                Report(result, output);

                if (options.CheckOnly)
                {
                    return Compare(output, options.OutputDirectory) ? 0 : 1;
                }

                ImportOutputWriter.Publish(output, options.OutputDirectory);
                Console.WriteLine("Escritos " + output.Files.Count + " archivos reproducibles en '" +
                                  options.OutputDirectory + "'. El overlay de estado no se toca.");
                return 0;
            }
            catch (StructuralSectionOverlayException ex)
            {
                Console.Error.WriteLine("OVERLAY DE ESTADO INCOHERENTE: " + ex.Message);
                return 1;
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
            "Uso: --workbook <ruta.xlsx> --output <directorio> [--check]";

        private sealed class Options
        {
            public string WorkbookPath;
            public string OutputDirectory;
            public bool CheckOnly;
        }

        /// <summary>
        /// A re-import must NOT quietly forget a decision. If the overlay names a section this workbook no
        /// longer produces, the import stops: withdrawing that decision is the operator's call, and the way to
        /// make it is to edit the overlay first. A warning would have let the decision evaporate.
        /// </summary>
        private static void RequireOverlayStillResolves(AiscImportResult result, string outputDirectory)
        {
            var existing = ImportOutputWriter.ReadExistingStatus(outputDirectory);

            if (existing.Count == 0)
            {
                return;
            }

            var produced = new HashSet<StructuralSectionId>(result.Sections.Select(section => section.SectionId));
            var orphaned = existing
                .Where(entry => !produced.Contains(entry.SectionId))
                .Select(entry => entry.SectionId.Value)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();

            if (orphaned.Length > 0)
            {
                throw new StructuralSectionOverlayException(
                    "el overlay de estado referencia " + orphaned.Length +
                    " id(s) que esta importacion ya no produce: " + string.Join(", ", orphaned) +
                    ". Edita '" + StructuralSectionCsvSchema.StatusFile +
                    "' y retira esas entradas conscientemente antes de reimportar.");
            }
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

        /// <summary>
        /// Reports the two questions separately, because they are different questions: are the REPRODUCIBLE
        /// data still exactly what this workbook produces, and is the LOCAL overlay a valid one. An edited
        /// overlay must never make the first answer "no".
        /// </summary>
        private static bool Compare(ImportOutput output, string outputDirectory)
        {
            var identical = true;

            Console.WriteLine("Datos generados reproducibles:");

            foreach (var file in output.Files)
            {
                var path = Path.Combine(outputDirectory, file.Key);

                if (!File.Exists(path))
                {
                    Console.Error.WriteLine("  FALTA:   " + file.Key);
                    identical = false;
                    continue;
                }

                if (!string.Equals(File.ReadAllText(path), file.Value, StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("  DIFIERE: " + file.Key);
                    identical = false;
                    continue;
                }

                Console.WriteLine("  OK:      " + file.Key);
            }

            Console.WriteLine(identical
                ? "  => los datos distribuidos coinciden exactamente con los que produce este libro."
                : "  => los datos distribuidos NO coinciden con los que produce este libro.");

            Console.WriteLine("Overlay local (" + StructuralSectionCsvSchema.StatusFile + "):");

            try
            {
                var overlay = ImportOutputWriter.ReadExistingStatus(outputDirectory);
                Console.WriteLine(
                    "  OK: valido, con " + overlay.Count + " excepcion(es). No participa de los hashes.");
            }
            catch (StructuralSectionCsvException ex)
            {
                Console.Error.WriteLine("  INVALIDO: " + ex.Message);
                identical = false;
            }

            return identical;
        }
    }
}
