using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6C (C4): cada FRONTERA TRANSACCIONAL compromete los campos pendientes antes de consumir estado.
    /// <para>
    /// Varias de esas fronteras no se pueden ejecutar en una prueba: <c>RequestDraw</c> necesita un documento de
    /// AutoCAD, y «Personalizar», el BOM y «Medio frente» abren diálogos modales que bloquean el hilo STA. Sin este
    /// guard, quitar el commit de cualquiera de ellas no rompería ninguna prueba — y el defecto que este gate corrige
    /// volvería en silencio justo por donde no miramos.
    /// </para>
    /// <para>
    /// Es una comprobación de FUENTES, no de comportamiento: verifica que la llamada está, no lo que hace. Las
    /// fronteras que sí se pueden ejecutar están cubiertas además por <see cref="SelectivePendingEditorsTests"/>.
    /// </para>
    /// </summary>
    public class SelectiveTransactionBoundaryGuardTests
    {
        /// <summary>Las fronteras de la tabla C4 marcadas con ✔, por la firma con la que empieza su handler.</summary>
        private static readonly (string Signature, string Boundary)[] Boundaries =
        {
            ("private void RequestDraw(", "Actualizar / Insertar (frontal, lateral, planta)"),
            ("private void SaveToLibrary_Click(", "Guardar en biblioteca"),
            ("private void ShowBom_Click(", "Lista de materiales"),
            ("private void CustomizePost_Click(", "Personalizar poste"),
            ("private void ResetPost_Click(", "Restablecer poste"),
            ("private void Safety_Click(", "Elementos de seguridad"),
            ("private void EditTramos(", "Medio frente"),
            ("private void ApplyScope(", "Celda / Seleccionadas / Nivel / Frente / Todas"),
            ("private void ApplyFrontOperation(", "Aplicar piso / elevación / niveles"),
            ("private void FondoSelector_Changed(", "Cambio de fondo visible"),
        };

        /// <summary>Las que NO comprometen (✘ en C4): comprometer aquí sería una mutación que el usuario no pidió.</summary>
        private static readonly (string Signature, string Why)[] NonBoundaries =
        {
            ("private void PostSelect_Changed(", "solo cambia status y preview"),
            ("private void PreviewView_Changed(", "solo repinta"),
            ("private void Close_Click(", "cerrar no persiste nada"),
        };

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizó la raíz del repo (RackCad.sln).");
            return dir;
        }

        private static string[] WindowSource() => File.ReadAllLines(Path.Combine(
            RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs"));

        /// <summary>
        /// El cuerpo del método que empieza en <paramref name="signature"/>: desde su firma hasta la llave de cierre
        /// al nivel de indentación de un miembro de clase (ocho espacios), que es el estilo del archivo.
        /// </summary>
        private static string BodyOf(string[] lines, string signature)
        {
            var start = Array.FindIndex(lines, l => l.TrimStart().StartsWith(signature, StringComparison.Ordinal));
            Assert.True(start >= 0, "No se encontró el handler '" + signature + "' en RackSelectiveWindow.xaml.cs.");

            // Un miembro con cuerpo de EXPRESION termina en su propia linea; escanear hasta la siguiente llave se
            // tragaria los metodos vecinos y haria que el guard leyera codigo que no es suyo.
            if (lines[start].Contains("=>", StringComparison.Ordinal)) return lines[start];

            var body = new List<string>();
            for (var i = start; i < lines.Length; i++)
            {
                body.Add(lines[i]);
                if (i > start && lines[i] == "        }") break;
            }

            return string.Join("\n", body);
        }

        [Fact]
        public void EveryTransactionalBoundary_CommitsThePendingEditorsFirst()
        {
            var lines = WindowSource();
            var missing = Boundaries
                .Where(b => !BodyOf(lines, b.Signature).Contains("CommitPendingEditors(", StringComparison.Ordinal))
                .Select(b => b.Boundary + "  (" + b.Signature + "…)")
                .ToList();

            Assert.True(
                missing.Count == 0,
                "Estas fronteras consumen estado definitivo sin comprometer antes los campos pendientes, así que un "
                    + "valor tecleado y no comprometido volvería a filtrarse (I-43, gate 8.6C, tabla C4):\n"
                    + string.Join("\n", missing));
        }

        [Fact]
        public void WhatIsNotABoundary_DoesNotCommit()
        {
            // La otra mitad del contrato: comprometer de más convierte un gesto de navegación en una escritura
            // multi-fondo que el usuario nunca pidió.
            var lines = WindowSource();
            var extra = NonBoundaries
                .Where(b => BodyOf(lines, b.Signature).Contains("CommitPendingEditors(", StringComparison.Ordinal))
                .Select(b => b.Signature + " — " + b.Why)
                .ToList();

            Assert.True(extra.Count == 0, "Estos handlers NO son fronteras transaccionales (C4, ✘):\n" + string.Join("\n", extra));
        }
    }
}
