using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One command of «Órdenes y atajos» as the page shows it: its alias («sin atajo» when it has none), what it does, the section of the
    /// window that does the same («En la ventana», a link when there is one) and whether «Ejecutar» runs it. Its names for UI Automation
    /// carry the command, so they are unique in the page.
    /// </summary>
    public sealed record RackCadHelpRow(string Command, string Alias, string Summary, string WindowText, RackCadWindowPage? WindowPage, bool CanRun)
    {
        public bool HasAlias => !string.IsNullOrWhiteSpace(Alias);

        public string AliasText => HasAlias ? Alias : "sin atajo";

        public bool HasWindowLink => WindowPage.HasValue;

        public bool HasNoWindowLink => !WindowPage.HasValue;

        public string RunAccessibleText => "Ejecutar " + Command + " en AutoCAD";

        public string WindowAccessibleText => Command + ", en la ventana: " + WindowText;
    }

    /// <summary>One group of the help list (Diseñar, Editar y copiar, …).</summary>
    public sealed record RackCadHelpGroup(string Name, IReadOnlyList<RackCadHelpRow> Rows);

    /// <summary>
    /// The page «Órdenes y atajos» of the RackCad window (I-64 A-14, line D, fourth batch, part «ut»; prototype «utilidades», UT-09 to UT-11):
    /// what RACKAYUDA lists (<see cref="RackCommandReference"/>, the one list of both), inside the window and without blocking AutoCAD, with a
    /// search, «Ejecutar» (the same door as the sections, only with no command in progress; RACKCAD, the window itself, is not run) and «En
    /// la ventana», which goes to the section that does the same. RACKAYUDA keeps its dialog until the list of phase 9.
    /// </summary>
    public sealed class RackCadHelpViewModel : ObservableObject
    {
        /// <summary>The paragraph of the page: the one of RACKAYUDA, without the promise of a setting that does not exist (UT-11).</summary>
        public const string IntroText = "Escribe la orden completa o su atajo en la línea de órdenes de AutoCAD, o pulsa «Ejecutar». Los atajos viajan con el "
                                        + "plugin (no hay que editar acad.pgp). Si un atajo ya lo usa tu acad.pgp, gana el del PGP: usa la orden completa.";

        // «En la ventana» of each command (the prototype): the section that does the same, or what the window says when there is none.
        private static readonly IReadOnlyDictionary<string, (string Text, RackCadWindowPage? Page)> WindowOf =
            new Dictionary<string, (string Text, RackCadWindowPage? Page)>(StringComparer.Ordinal)
            {
                ["RACKCAD"] = ("Es esta ventana", null),
                ["RACKMENU"] = ("Pie de la columna", null),
                ["RACKSELECTIVO"] = ("Nuevo", RackCadWindowPage.Nuevo),
                ["RACKCABECERA"] = ("Nuevo", RackCadWindowPage.Nuevo),
                ["RACKSISTEMADINAMICO"] = ("Nuevo", RackCadWindowPage.Nuevo),
                ["RACKCANTILEVER"] = ("Nuevo", RackCadWindowPage.Nuevo),
                ["RACKPUSHBACK"] = ("Nuevo", RackCadWindowPage.Nuevo),
                ["RACKSECCION"] = ("Nuevo › Perfil estructural", RackCadWindowPage.PerfilEstructural), // T7 per: RACKSECCION opens that page
                ["RACKEDITAR"] = ("Página del rack: «Editar aquí» (Selectivo y Dinámico)", null),
                ["RACKVARIABLES"] = ("Variables", RackCadWindowPage.Variables),
                ["RACKPROPIEDADES"] = ("Página del rack: su pestaña «Propiedades»", null), // T6 rl
                ["RACKLAYOUT"] = ("Almacén", RackCadWindowPage.Almacen), // T6 rl: RLY opens it on «Rejilla»
                ["RACKRELLENAR"] = ("Almacén", RackCadWindowPage.Almacen), // T6 rl: RR opens it on «Rellenar un área»
                ["RACKLISTA"] = ("BOM y lista de racks", RackCadWindowPage.Bom),
                ["RACKBOMTOTAL"] = ("BOM y lista de racks", RackCadWindowPage.Bom),
                ["RACKAYUDA"] = ("Es esta página", null),
            };

        private readonly Action<string> _run;
        private readonly Action<RackCadWindowPage> _go;
        private string _search = string.Empty;

        /// <param name="run">«Ejecutar»: the window hands the command to AutoCAD through its door.</param>
        /// <param name="go">«En la ventana»: the window shows that section.</param>
        public RackCadHelpViewModel(Action<string> run, Action<RackCadWindowPage> go)
        {
            _run = run;
            _go = go;
        }

        public string Intro => IntroText;

        /// <summary>The search by command, alias or text, with no case and no accents.</summary>
        public string SearchText
        {
            get => _search;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_search, next, StringComparison.Ordinal)) return;
                _search = next;
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(Groups));
                OnPropertyChanged(nameof(CountText));
                OnPropertyChanged(nameof(NoMatch));
            }
        }

        /// <summary>Every command of the list, in its order.</summary>
        public static IReadOnlyList<RackCadHelpRow> AllRows => RackCommandReference.Commands.Select(RowOf).ToList();

        public IReadOnlyList<RackCadHelpGroup> Groups
        {
            get
            {
                var query = Fold(_search.Trim());
                return RackCommandReference.Commands
                    .Where(c => query.Length == 0 || Fold(c.Command).Contains(query, StringComparison.Ordinal)
                                || Fold(c.Alias).Contains(query, StringComparison.Ordinal) || Fold(c.Summary).Contains(query, StringComparison.Ordinal))
                    .GroupBy(c => c.Group)
                    .Select(g => new RackCadHelpGroup(g.Key, g.Select(RowOf).ToList()))
                    .ToList();
            }
        }

        public string CountText
        {
            get
            {
                var total = RackCommandReference.Commands.Count;
                var shown = Groups.Sum(g => g.Rows.Count);
                return _search.Trim().Length == 0 ? total + " órdenes" : "Mostrando " + shown + " de " + total;
            }
        }

        public bool NoMatch => Groups.Count == 0;

        /// <summary>«Ejecutar» of a row; nothing for a command the window does not run.</summary>
        public void Run(RackCadHelpRow row)
        {
            if (row == null || !row.CanRun) return;
            _run?.Invoke(row.Command);
        }

        /// <summary>«En la ventana» of a row: the section that does the same.</summary>
        public void GoTo(RackCadHelpRow row)
        {
            if (row?.WindowPage is RackCadWindowPage page) _go?.Invoke(page);
        }

        private static RackCadHelpRow RowOf(RackCommandInfo info)
        {
            var (text, page) = WindowOf.TryGetValue(info.Command, out var known) ? known : ("Solo como orden", (RackCadWindowPage?)null);
            return new RackCadHelpRow(info.Command, info.Alias, info.Summary, text, page, RackCadHelpCommands.TextOf(info.Command) != null);
        }

        private static string Fold(string text)
        {
            var decomposed = (text ?? string.Empty).ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var c in decomposed)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) builder.Append(c);
            return builder.ToString();
        }
    }
}
