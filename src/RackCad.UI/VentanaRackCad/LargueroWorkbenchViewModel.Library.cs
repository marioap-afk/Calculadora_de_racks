using System;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Larguero;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda): the page «Larguero» and the library (the Owner, points 1 and 5: «Un campo nuevo», and «Abrir» of the library
    /// brings a larguero here) and «Exportar el BOM» in «Más» (point 4, as the BOM of the window: Excel and CSV, an automatic name, and the
    /// notice with «Abrir», «Mostrar en la carpeta» and «Guardar como…»).
    /// </summary>
    public sealed partial class LargueroWorkbenchViewModel
    {
        private string _loadedNotice = string.Empty;

        /// <summary>The project of the library the larguero shown was opened from: a save keeps its unknown fields (I-11); null for a new one.</summary>
        internal RackCad.Application.Persistence.RackProject LibrarySource { get; set; }
        private bool _moreOpen;
        private Exported _exported;

        private sealed record Exported(string Path, string FileName, string Detail, byte[] Content, string Extension, bool Failed);

        // ------------------------------------------------------------------ «Guardar en la biblioteca…» (point 1)

        /// <summary>«Guardar en la biblioteca…» is offered with the window (its page «Biblioteca de diseños»).</summary>
        public bool CanSaveToLibrary => _host != null;

        /// <summary>«Guardar en la biblioteca…»: the line of the save in «Biblioteca de diseños», with the name of the larguero; nothing is written yet.</summary>
        public void SaveToLibrary() => _host?.SaveToLibrary(this);

        /// <summary>Why the larguero cannot be saved now; null when it can (the library asks it again at «Guardar» and «Reemplazar»).</summary>
        public string SaveRefusal() =>
            _summary == null || !_summary.IsComplete || _section == null
                ? "El larguero no está completo: " + string.Join(" ", _summary?.Missing ?? Array.Empty<string>())
                : null;

        /// <summary>
        /// The design that «Guardar» writes, with <paramref name="name"/> as its name: a larguero of RackCad by its id (as every larguero saved
        /// before), or a section of AISC by its catalog and its id (the new field, point 1). The ménsula is written when it was chosen (or for an
        /// AISC section, which has none of its row); a ménsula that follows the row stays blank, as «(auto — según perfil)» of the classic.
        /// </summary>
        public LargueroDesign BuildDesign(string name)
        {
            if (SaveRefusal() != null) return null;
            var aisc = _section.Source == LargueroSectionSource.Aisc;
            double peralte = aisc ? _section.Depth ?? _plan?.Peralte ?? 0.0
                : double.TryParse(_peralte, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : 0.0;
            return new LargueroDesign
            {
                Name = (name ?? string.Empty).Trim(),
                BeamProfileId = aisc ? null : _section.Id,
                SectionCatalog = aisc ? LargueroDesign.AiscCatalog : null,
                SectionId = aisc ? _section.Id : null,
                Peralte = peralte,
                Length = Length ?? 0.0,
                MensulaOverride = aisc || _mensulaChosen ? _mensula : null,
            };
        }

        /// <summary>What the page says of the design of the library it shows (opened from it), and what it could not keep.</summary>
        public string LoadedNotice => _loadedNotice;

        public bool HasLoadedNotice => _loadedNotice.Length > 0;

        /// <summary>
        /// «Abrir» of the library (point 5): the larguero on this page, with its name, its section, its peralte, its length and its ménsula. A
        /// section that is not in the catalog (or a catalog this build does not know) is said and none is chosen in its place; what could be
        /// kept is kept.
        /// </summary>
        public void LoadDesign(LargueroDesign design, string designName)
        {
            if (design == null) return;
            ResetToDefaults();
            var title = string.IsNullOrWhiteSpace(designName) ? (design.Name ?? string.Empty).Trim() : designName.Trim();
            var notices = new System.Collections.Generic.List<string> { "Abierto de la biblioteca: «" + title + "». «Guardar en la biblioteca…» con el mismo nombre lo reemplaza; el archivo no cambia hasta entonces." };

            LargueroSectionOption section = null;
            if (!string.IsNullOrWhiteSpace(design.SectionCatalog))
            {
                if (string.Equals(design.SectionCatalog.Trim(), LargueroDesign.AiscCatalog, StringComparison.OrdinalIgnoreCase))
                {
                    section = _sections.Find(LargueroSectionSource.Aisc, design.SectionId);
                    if (section == null)
                        notices.Add("La sección «" + design.SectionId + "» del larguero no está entre los perfiles AISC habilitados del catálogo: elige otra.");
                }
                else
                {
                    notices.Add("El larguero usa una sección del catálogo «" + design.SectionCatalog.Trim() + "», que esta versión no conoce: elige otra.");
                }
            }
            else
            {
                section = _sections.Find(LargueroSectionSource.RackCad, design.BeamProfileId);
                if (section == null)
                    notices.Add("El larguero «" + design.BeamProfileId + "» no está en el catálogo de RackCad (secciones.csv): elige otra sección.");
            }

            _name = (design.Name ?? string.Empty).Trim();
            if (section != null)
            {
                _catalogFilter = section.Source == LargueroSectionSource.Aisc ? AiscKey : RackCadKey;
                RefreshTypes(raise: false);
                RefreshRows(raise: false);
                _section = section;
                RefreshMensulaOptions(raise: false);
                var peralte = LargueroSectionNaming.Number(design.Peralte);
                _peralte = PeralteOptions.Contains(peralte) ? peralte : PeralteOptions.FirstOrDefault();
                if (section.HasPeralteChoice && !PeralteOptions.Contains(peralte) && design.Peralte > 0.0)
                    notices.Add("El peralte guardado (" + peralte + " in) no es uno de los de este larguero en el catálogo: se eligió " + (_peralte ?? "ninguno") + ".");
            }
            else
            {
                _section = null;
                _peralte = null;
                RefreshMensulaOptions(raise: false);
            }

            _length = design.Length > 0.0 ? LargueroSectionNaming.Number(design.Length) : string.Empty;
            if (!string.IsNullOrWhiteSpace(design.MensulaOverride))
            {
                var chosen = _mensulaOptions.FirstOrDefault(o => string.Equals(o.Key, design.MensulaOverride.Trim(), StringComparison.OrdinalIgnoreCase));
                _mensula = chosen?.Key;
                _mensulaChosen = chosen != null;
                if (chosen == null) notices.Add("La ménsula «" + design.MensulaOverride.Trim() + "» no está en el catálogo (mensulas.csv): elige otra.");
            }
            else
            {
                _mensulaChosen = false;
                _mensula = DefaultMensulaOf(_section);
            }

            _loadedNotice = string.Join(" ", notices);
            OnPropertyChanged(nameof(NameText));
            OnPropertyChanged(nameof(LoadedNotice));
            OnPropertyChanged(nameof(HasLoadedNotice));
            OnPropertyChanged(nameof(SelectedCatalog));
            OnPropertyChanged(nameof(TypeOptions));
            OnPropertyChanged(nameof(SelectedType));
            OnPropertyChanged(nameof(LengthText));
            OnPropertyChanged(nameof(LengthError));
            OnPropertyChanged(nameof(HasLengthError));
            RaiseRows();
            RaiseSection();
        }

        // ------------------------------------------------------------------ «Más» › «Exportar el BOM» (point 4)

        /// <summary>The files of the export: the disk and the shell of Windows; the tests give theirs.</summary>
        internal IRackBomFiles Files { get; set; } = new RackBomFileShell();

        public bool IsMoreOpen
        {
            get => _moreOpen;
            set
            {
                if (_moreOpen == value) return;
                _moreOpen = value;
                OnPropertyChanged(nameof(IsMoreOpen));
                OnPropertyChanged(nameof(MoreItemStatus));
            }
        }

        public string MoreItemStatus => _moreOpen ? "abierto" : "cerrado";

        public void ToggleMore() => IsMoreOpen = !IsMoreOpen;

        /// <summary>The BOM is exported when the larguero is complete.</summary>
        public bool CanExportBom => _summary?.IsComplete == true;

        public string ExportBomHint => CanExportBom
            ? "En Documentos, con el nombre «" + BomFileName(RackBomFiles.Excel) + "» (o .csv); nunca reemplaza otro archivo."
            : "Completa el larguero para exportar su BOM.";

        /// <summary>«Exportar el BOM a Excel…» / «… a CSV…»: the bill of materials of the summary, with the exporters of the BOM of the window.</summary>
        public void ExportBom(bool excel)
        {
            IsMoreOpen = false;
            if (!CanExportBom) return;
            var extension = excel ? RackBomFiles.Excel : RackBomFiles.Csv;
            var bill = _summary.Bill;
            var content = excel ? BomXlsxExporter.ToXlsx(bill) : new System.Text.UTF8Encoding(false).GetBytes(BomCsvExporter.ToCsv(bill) ?? string.Empty);
            var detail = (excel ? "Una hoja «Lista de materiales», como el BOM de un rack de la ventana" : "El componente y sus piezas")
                         + ": el larguero, su perfil ×1 y sus ménsulas ×2, con los nombres de esta página.";

            string folder;
            try
            {
                folder = Files.FallbackFolder;
            }
            catch (Exception)
            {
                folder = null;
            }

            // In the documents of the user a file of the same name may be someone else's: it is never replaced (as the BOM of the window).
            var baseName = BomFileName(extension);
            var fileName = baseName;
            var path = WindowsFileNames.Combine(folder, fileName);
            var number = 1;
            while (SafeExists(path) && number < 1000)
            {
                number++;
                fileName = RackBomFiles.WithNumber(baseName, number);
                path = WindowsFileNames.Combine(folder, fileName);
            }

            var where = "En Documentos. " + (number > 1 ? "Ya había un archivo con ese nombre: este se guardó como «" + fileName + "». " : string.Empty);
            try
            {
                Files.Write(path, content);
                _exported = new Exported(path, fileName, where + detail, content, extension, false);
            }
            catch (Exception ex)
            {
                _exported = new Exported(path, fileName, "No se pudo escribir en " + (folder ?? "la carpeta") + ": " + ex.Message.Trim()
                                                         + " Si el archivo está abierto en Excel, ciérralo, o usa «Guardar como…».", content, extension, true);
            }

            RaiseExport();
        }

        public bool HasExported => _exported != null;

        public bool ExportedFailed => _exported?.Failed == true;

        public string ExportedTitle => _exported == null ? string.Empty
            : _exported.Failed ? "No se exportó «" + _exported.FileName + "»" : "BOM exportado: «" + _exported.FileName + "»";

        public string ExportedDetail => _exported?.Detail ?? string.Empty;

        public bool CanOpenExported => _exported != null && !_exported.Failed;

        public void OpenExported()
        {
            if (CanOpenExported) Files.Open(_exported.Path);
        }

        public void ShowExportedInFolder()
        {
            if (CanOpenExported) Files.ShowInFolder(_exported.Path);
        }

        /// <summary>«Guardar como…»: the dialog of the system, only when asked; the same file elsewhere.</summary>
        public void SaveExportedAs()
        {
            var exported = _exported;
            if (exported == null) return;
            var chosen = Files.SaveAs(exported.FileName, exported.Extension, WindowsFileNames.DirectoryOf(exported.Path));
            if (string.IsNullOrWhiteSpace(chosen)) return;
            var file = chosen.Substring(Math.Max(chosen.LastIndexOf((char)0x5C), chosen.LastIndexOf('/')) + 1); // 0x5C: the separator of Windows
            try
            {
                Files.Write(chosen, exported.Content);
                _exported = exported with { Path = chosen, FileName = file, Failed = false, Detail = "En la carpeta que elegiste. " + exported.Detail };
            }
            catch (Exception ex)
            {
                _exported = exported with { Path = chosen, FileName = file, Failed = true, Detail = "No se pudo escribir: " + ex.Message.Trim() };
            }

            RaiseExport();
        }

        public void CloseExported()
        {
            _exported = null;
            RaiseExport();
        }

        // «Larguero - Larguero 96 in · P3 - BOM.xlsx»: the name of the larguero, with what a file name cannot carry made «_».
        private string BomFileName(string extension)
        {
            var name = WindowsFileNames.Clean(EffectiveName.Length > 0 ? EffectiveName : "Larguero").Trim().TrimEnd('.');
            return "Larguero - " + (name.Length == 0 ? "Larguero" : name) + " - BOM" + extension;
        }

        private bool SafeExists(string path)
        {
            try
            {
                return Files.Exists(path);
            }
            catch (Exception)
            {
                return true; // unknown is taken: never replace what may be there
            }
        }

        private void RaiseExport()
        {
            OnPropertyChanged(nameof(CanExportBom));
            OnPropertyChanged(nameof(ExportBomHint));
            OnPropertyChanged(nameof(HasExported));
            OnPropertyChanged(nameof(ExportedFailed));
            OnPropertyChanged(nameof(ExportedTitle));
            OnPropertyChanged(nameof(ExportedDetail));
            OnPropertyChanged(nameof(CanOpenExported));
        }
    }
}
