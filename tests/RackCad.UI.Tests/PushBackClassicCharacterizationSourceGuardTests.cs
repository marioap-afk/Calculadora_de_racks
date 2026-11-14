using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — GUARDAS DE FUENTE de la ventana CLASICA de Push Back: las ausencias, los duenos de los
    /// subdialogos que no tienen costura de instancia, el host del menu, el orden de las escrituras de la propia
    /// ventana (clase A de INV-F4T1B-16: guardar en biblioteca y la disposicion del configurador, que se fijan por
    /// fuente y NUNCA se ejecutan) y la parte sin costura de «Insertar varias…». CT-16, CT-19 a CT-22 y la parte de
    /// fuente de CT-23 del paquete v5.
    ///
    /// <para><b>Son guardas de TEXTO</b> (INV-F4T1B-08): se usan solo donde ninguna prueba puede ejercer el camino
    /// sin abrir un modal real o escribir en el perfil. Normalizan CRLF a LF (Core corre en Ubuntu y en local se
    /// trabaja con <c>core.autocrlf=true</c>), quitan los comentarios y buscan por las RAICES de la clausura de la
    /// ventana, sin depender de en cual de sus archivos vive cada sentencia.</para>
    ///
    /// <para>No afirman que la ventana funcione: eso lo cobra la validacion fisica (Smoke-2). Antes de que F5 extraiga
    /// la superficie, estas guardas se sustituyen siguiendo el patron de I-39: la vieja queda con <c>Skip</c> y su
    /// evidencia versionada, y la prueba de ejecucion equivalente va en una clase de contrato; no se borra nada.</para>
    /// </summary>
    public sealed class PushBackClassicCharacterizationSourceGuardTests
    {
        /// <summary>La constante que marca estas pruebas como guardas de texto y que F5 sustituye (patron de I-39).</summary>
        private const string TextGuard =
            "GUARDA DE TEXTO (F4-T1B, INV-F4T1B-08): fija una estructura del codigo fuente que ninguna prueba de ejecucion puede observar sin abrir un modal real o escribir en el perfil; "
            + "F5 la sustituye por una prueba de ejecucion segun el patron de I-39 (la guarda vieja queda con Skip y su evidencia versionada).";

        // ----------------------------------------------------------------------------------------------- las raices

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizo la raiz del repo (RackCad.sln).");
            return dir;
        }

        private static string UiRoot() => Path.Combine(RepoRoot().FullName, "src", "RackCad.UI");

        private static IEnumerable<string> SourceFiles(string root, string pattern)
            => Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                    && !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                .OrderBy(path => path, StringComparer.Ordinal);

        // -------------------------------------------------------------------------------------- la normalizacion

        private static string StripCSharpComments(string text)
        {
            var result = new StringBuilder(text.Length);
            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];
                var next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (c == '/' && next == '/')
                {
                    while (i < text.Length && text[i] != '\n')
                    {
                        i++;
                    }

                    continue;
                }

                if (c == '/' && next == '*')
                {
                    i += 2;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                    {
                        i++;
                    }

                    i += 2;
                    result.Append(' ');
                    continue;
                }

                if (c == '@' && next == '"')
                {
                    result.Append("@\"");
                    i += 2;
                    while (i < text.Length)
                    {
                        if (text[i] == '"')
                        {
                            if (i + 1 < text.Length && text[i + 1] == '"')
                            {
                                result.Append("\"\"");
                                i += 2;
                                continue;
                            }

                            result.Append('"');
                            i++;
                            break;
                        }

                        result.Append(text[i]);
                        i++;
                    }

                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    result.Append(c);
                    i++;
                    while (i < text.Length)
                    {
                        if (text[i] == '\\' && i + 1 < text.Length)
                        {
                            result.Append(text[i]);
                            result.Append(text[i + 1]);
                            i += 2;
                            continue;
                        }

                        if (text[i] == c)
                        {
                            result.Append(c);
                            i++;
                            break;
                        }

                        result.Append(text[i]);
                        i++;
                    }

                    continue;
                }

                result.Append(c);
                i++;
            }

            return result.ToString();
        }

        private static string StripXamlComments(string text)
            => Regex.Replace(text, "<!--.*?-->", " ", RegexOptions.Singleline);

        private static string Collapse(string text) => Regex.Replace(text, @"\s+", " ").Trim();

        /// <summary>CRLF a LF, sin comentarios y con los espacios en blanco colapsados.</summary>
        private static string Normalize(string raw, bool xaml)
        {
            var lf = raw.Replace("\r\n", "\n").Replace('\r', '\n');
            return Collapse(xaml ? StripXamlComments(lf) : StripCSharpComments(lf));
        }

        private static string Read(params string[] relative)
            => Normalize(
                File.ReadAllText(Path.Combine(new[] { UiRoot() }.Concat(relative).ToArray())),
                relative.Last().EndsWith(".xaml", StringComparison.Ordinal));

        /// <summary>G1: todo lo que vive bajo <c>Systems/PushBack</c>, sin depender de la composicion de archivos.</summary>
        private static string ClassicWindowSources()
        {
            var root = Path.Combine(UiRoot(), "Systems", "PushBack");
            Assert.True(Directory.Exists(root), "No existe la raiz de la clausura Systems/PushBack");
            var files = SourceFiles(root, "*.cs").Select(path => Normalize(File.ReadAllText(path), false))
                .Concat(SourceFiles(root, "*.xaml").Select(path => Normalize(File.ReadAllText(path), true)));
            return string.Join("\n", files);
        }

        /// <summary>El cuerpo (de llave a llave) del primer miembro que empieza por <paramref name="marker"/>.</summary>
        private static string Body(string source, string marker)
        {
            var at = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontro: " + marker);
            var open = source.IndexOf('{', at);
            Assert.True(open >= 0, "sin cuerpo: " + marker);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            throw new InvalidOperationException("cuerpo sin cerrar: " + marker);
        }

        private static void Has(string source, string expected, string what)
            => Assert.True(source.Contains(expected, StringComparison.Ordinal), TextGuard + " Falta " + what + ": " + expected);

        private static void Lacks(string source, string forbidden, string what)
            => Assert.True(!source.Contains(forbidden, StringComparison.Ordinal), TextGuard + " Sobra " + what + ": " + forbidden);

        private static void Matches(string source, string pattern, string what)
            => Assert.True(Regex.IsMatch(source, pattern, RegexOptions.CultureInvariant), TextGuard + " Falta " + what + " (patron " + pattern + ")");

        // ================================================================================================== CT-16

        [Fact]
        public void TheHeaderConfiguratorIsOwnedByTheWindowCentersOnItsOwnerAndItsConfigurationIsAlwaysStaged()
        {
            var window = ClassicWindowSources();
            var body = Body(window, "private RackFrameConfiguration ShowHeaderConfigurator(");

            // S-2: se crea con el dueno = la ventana.
            Has(body, "new RackFrameConfiguratorWindow(copy) { Owner = this };", "el dueno de S-2");

            // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D6, OD-3): cerrar el configurador por CUALQUIER camino devuelve su
            // configuracion y la ventana la escenifica: no existe un camino de cancelar que la descarte.
            Has(body, "return window.Configuration ?? copy;", "la configuracion que se escenifica al cerrar");

            // Ancla (el archivo no es de los mutantes permitidos): el configurador se centra en su dueno por su XAML.
            Has(Read("RackFrames", "RackFrameConfiguratorWindow.xaml"), "WindowStartupLocation=\"CenterOwner\"", "CenterOwner de S-2");
        }

        // ================================================================================================== CT-19

        [Fact]
        public void TheClassicWindowHasNoKeyboardHandlersNoTabOrderNoForcedFocusAndNoDialogResult()
        {
            var window = ClassicWindowSources();

            foreach (var token in new[]
                     {
                         "KeyDown", "PreviewKeyDown", "InputBindings", "KeyBinding", "TabIndex", "IsTabStop",
                         "KeyboardNavigation", ".Focus(", "DialogResult",
                     })
            {
                Lacks(window, token, "una ausencia de G1");
            }
        }

        [Fact]
        public void NoCodeAnywhereInSrcSubscribesToInsertRequestedRaised()
        {
            var subscribers = new List<string>();
            foreach (var path in SourceFiles(Path.Combine(RepoRoot().FullName, "src"), "*.cs"))
            {
                var text = StripCSharpComments(File.ReadAllText(path).Replace("\r\n", "\n"));
                if (Regex.IsMatch(text, @"InsertRequestedRaised\s*\+="))
                {
                    subscribers.Add(Path.GetFileName(path));
                }
            }

            Assert.True(subscribers.Count == 0, TextGuard + " Suscriptores de InsertRequestedRaised: " + string.Join(", ", subscribers));
        }

        // ================================================================================================== CT-20

        [Fact]
        public void TheDefenseGridIsOwnedByTheWindow()
        {
            Matches(ClassicWindowSources(), @"new SafetyDefensaGridWindow\([^;]*\) \{ Owner = this \};", "el dueno de S-5");
        }

        [Fact]
        public void TheTopeGridIsOwnedByTheWindow()
        {
            Matches(ClassicWindowSources(), @"new SafetyTopeGridWindow\([^;]*\) \{ Owner = this \};", "el dueno de S-6");
        }

        [Fact]
        public void TheBomWindowIsOwnedByTheWindow()
        {
            Has(ClassicWindowSources(), "new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();", "el dueno de S-7");
        }

        [Fact]
        public void TheBatchDialogIsShownThroughItsPresenterWithTheWindowAsOwner()
        {
            Has(ClassicWindowSources(), "Views.RackViewBatchDialogPresenter.TryShow(this, RackSystemKind.PushBack, options, out var views)", "TryShow(this, ...) de S-8");
            Has(Read("Views", "RackViewBatchDialogPresenter.cs"), "new RackViewBatchDialog(presenter) { Owner = owner };", "el dueno de S-8 en el presentador");
        }

        [Fact]
        public void TheSaveFileDialogIsOwnedByTheWindowThroughPromptSaveToLibrary()
        {
            Has(ClassicWindowSources(), "UiSupport.PromptSaveToLibrary(this, NameBox.Text, \"sistema\")", "PromptSaveToLibrary(this, ...) de S-9");
            Has(Read("UiSupport.cs"), "dialog.ShowDialog(owner) == true", "ShowDialog(owner) de S-9");
        }

        // ================================================================================================== CT-21

        [Fact]
        public void TheMenuHostOpensThePushBackWindowWithItsContextOwnerInBothPathsAndReturnsTheRequestOnlyWhenRequested()
        {
            var modules = Read("Editor", "EditorModules.cs");
            var start = modules.IndexOf("public sealed class PushBackEditorModule", StringComparison.Ordinal);
            var end = modules.IndexOf("public sealed class CantileverEditorModule", StringComparison.Ordinal);
            Assert.True(start >= 0 && end > start, "no se encontro PushBackEditorModule");
            var module = modules.Substring(start, end - start);

            var ownedWindow = "new RackPushBackSystemWindow(context.CanInsertInAutoCad) { Owner = context.Owner };";
            Assert.True(
                Regex.Matches(module, Regex.Escape(ownedWindow)).Count == 2,
                TextGuard + " El host del menu debe abrir la ventana con { Owner = context.Owner } en sus dos caminos.");
            Assert.True(
                Regex.Matches(module, Regex.Escape("window.ShowDialog();")).Count == 2,
                TextGuard + " El host del menu debe mostrar la ventana con ShowDialog() en sus dos caminos.");
            Has(module, "window.InsertRequested ? window.InsertionRequest : null", "la peticion solo si se pidio");
        }

        // ================================================================================================== CT-22

        [Fact]
        public void SaveToLibraryPromptsFirstThenMarksTheIdentityNamesItAndSavesTheLibraryProject()
        {
            // Clase A (INV-F4T1B-16): lo escribe la propia ventana. Se fija SOLO por fuente y no se ejecuta: ejecutarlo
            // crearia la carpeta de la biblioteca y abriria un SaveFileDialog real (OD-10).
            var body = Body(ClassicWindowSources(), "private void SaveLibrary_Click(");

            var prompt = body.IndexOf("UiSupport.PromptSaveToLibrary(this,", StringComparison.Ordinal);
            var ensureId = body.IndexOf("session.Identity.EnsureId();", StringComparison.Ordinal);
            var setName = body.IndexOf("session.Identity.SetName(NameBox.Text?.Trim());", StringComparison.Ordinal);
            var project = body.IndexOf("var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);", StringComparison.Ordinal);
            var save = body.IndexOf("new RackProjectStore().Save(project, path);", StringComparison.Ordinal);

            Assert.True(prompt >= 0, TextGuard + " Falta PromptSaveToLibrary(this, ...) en SaveLibrary_Click");
            Assert.True(ensureId > prompt, TextGuard + " EnsureId debe venir DESPUES de PromptSaveToLibrary");
            Assert.True(setName > ensureId, TextGuard + " SetName debe venir DESPUES de EnsureId");
            Assert.True(project > setName, TextGuard + " el proyecto de biblioteca se arma DESPUES de SetName");
            Assert.True(save > project, TextGuard + " Save(...) viene al final");
        }

        [Fact]
        public void TheHeaderConfiguratorWritesItsLayoutOnlyWhenItIsClosed()
        {
            // Clase A (INV-F4T1B-16): el configurador escribe su disposicion en %LOCALAPPDATA% en OnClosed, por una ruta
            // static readonly que no se puede redirigir. Ninguna prueba de T1B lo instancia (INV-F4T1B-06).
            var configurator = Read("RackFrames", "RackFrameConfiguratorWindow.xaml.cs");
            var body = Body(configurator, "protected override void OnClosed(EventArgs e)");

            Matches(body, @"^\{ SaveCurrentLayout\(\);", "SaveCurrentLayout() como primera sentencia de OnClosed");
        }

        // ================================================================================================== CT-23 (fuente)

        [Fact]
        public void InsertBatchShowsItsDialogThenRequestsTheBatchAndClosesWithoutLookingAtCanInsertInAutoCad()
        {
            var body = Body(ClassicWindowSources(), "private void InsertBatch_Click(");

            var confirm = body.IndexOf("if (moduleOwner.ModuleSession.HasPendingChanges) moduleOwner.CommitModuleEdits();", StringComparison.Ordinal);
            var dialog = body.IndexOf("Views.RackViewBatchDialogPresenter.TryShow(this, RackSystemKind.PushBack, options, out var views)", StringComparison.Ordinal);
            var request = body.IndexOf("session.RequestInsertViews(views", StringComparison.Ordinal);
            var close = body.LastIndexOf("Close();", StringComparison.Ordinal);

            Assert.True(dialog >= 0, TextGuard + " Falta el dialogo de lote en InsertBatch_Click");
            Assert.True(request > dialog, TextGuard + " RequestInsertViews viene DESPUES del dialogo");
            Assert.True(close > request, TextGuard + " Close() viene DESPUES de RequestInsertViews");

            // Ancla: los argumentos del lote (cinco vistas fijas y un lateral por cada poste con corte) y las rarezas.
            Assert.True(confirm >= 0 && confirm < dialog, TextGuard + " el modulo se confirma ANTES del dialogo (OD-2)");
            Has(body, "RackViewAddress.PushBackCut(RackCad.Application.Systems.Shared.RackPushBackEnd.EntradaSalida, RackCad.Application.Systems.Shared.RackPushBackSide.A)", "el frontal entrada/salida A");
            Has(body, "RackViewAddress.PushBackCut(RackCad.Application.Systems.Shared.RackPushBackEnd.Posterior, RackCad.Application.Systems.Shared.RackPushBackSide.B)", "el frontal posterior B");
            Has(body, "RackViewAddress.Whole(DimensionViewKind.Planta)", "la planta");
            Has(body, "RackViewAddress.Post(cut.PostIndex)", "un lateral por poste con corte");
            Has(body, "RackViewCodec.Encode(RackSystemKind.PushBack, ctx.Views[0])", "View/Section de la peticion = los de Views[0]");
            // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D6, OD-1): nunca comprueba canInsertInAutoCad.
            Lacks(body, "canInsertInAutoCad", "una comprobacion de canInsertInAutoCad en InsertBatch_Click");
        }
    }
}
