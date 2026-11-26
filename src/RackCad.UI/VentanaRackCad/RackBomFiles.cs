using System;
using System.Diagnostics;
using System.IO;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the page «BOM y lista de racks» does with the files it exports (T4 bom, BM-03): it writes them with their automatic name, opens
    /// one with its application, shows it in its folder and, only if the user asks, offers «Guardar como…» (the dialog of the system, the
    /// only dialog of the page). An interface so the page is tested without touching the disk or opening anything.
    /// </summary>
    public interface IRackBomFiles
    {
        /// <summary>Where the files go when the drawing was never saved (it has no folder): the documents of the user.</summary>
        string FallbackFolder { get; }

        /// <summary>
        /// Writes the file, replacing one with the same name, atomically: an interrupted write never leaves half a file in place of the one
        /// that was there (correction 3 of the review). A failure throws (the page says it).
        /// </summary>
        void Write(string path, byte[] content);

        /// <summary>A file with that path is there (the page does not replace one in the documents of the user).</summary>
        bool Exists(string path);

        /// <summary>Opens the file with its application (Excel, for a .xlsx).</summary>
        void Open(string path);

        /// <summary>Opens the folder of the file with the file selected.</summary>
        void ShowInFolder(string path);

        /// <summary>«Guardar como…»: the dialog of the system; the chosen path, or null when the user cancelled.</summary>
        string SaveAs(string suggestedName, string extension, string initialFolder);
    }

    /// <summary>The files of the machine: the disk, the shell of Windows and its dialog «Guardar como».</summary>
    public sealed class RackBomFileShell : IRackBomFiles
    {
        public string FallbackFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // The pattern of AtomicFile (I-03), which is internal to Application and writes text: the bytes go to a temporary file in the SAME
        // folder (the same volume, so the swap is atomic), then File.Replace over an existing file or File.Move into a new one. The folder is
        // not created; a real error of the disk still throws, and the temporary file is removed.
        public void Write(string path, byte[] content)
        {
            var folder = Path.GetDirectoryName(path);
            var temp = Path.Combine(string.IsNullOrEmpty(folder) ? "." : folder, Path.GetFileName(path) + ".tmp-" + Guid.NewGuid().ToString("N"));
            try
            {
                File.WriteAllBytes(temp, content ?? Array.Empty<byte>());
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);
            }
            catch
            {
                try
                {
                    if (File.Exists(temp)) File.Delete(temp);
                }
                catch (Exception)
                {
                    // The temporary file could not be removed: the original error is the one that matters.
                }

                throw;
            }
        }

        public bool Exists(string path) => File.Exists(path);

        public void Open(string path) => Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });

        public void ShowInFolder(string path) =>
            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"") { UseShellExecute = true });

        public string SaveAs(string suggestedName, string extension, string initialFolder)
        {
            var excel = string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase);
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = excel ? "Guardar la lista de materiales como Excel" : "Guardar la lista de materiales como CSV",
                Filter = excel ? "Excel (*.xlsx)|*.xlsx|Todos (*.*)|*.*" : "CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                FileName = suggestedName,
                DefaultExt = extension,
                OverwritePrompt = true,
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(initialFolder) && Directory.Exists(initialFolder)) dialog.InitialDirectory = initialFolder;
            }
            catch (Exception)
            {
                // An invalid folder only means the dialog opens at its default folder.
            }

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
