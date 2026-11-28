using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — los ayudantes comunes de las caracterizaciones de la ventana CLASICA de Push Back (paquete
    /// del contrato, §4.1). Solo ejecucion, sin guardas de texto, sin costuras nuevas: usa los ayudantes existentes
    /// (<see cref="StaTestRunner"/>, <see cref="EditorWindowTestSupport"/>) y las costuras internas ya declaradas por la
    /// ventana. No contiene pruebas.
    ///
    /// <para><b>Determinismo.</b> Toda ventana se crea con <see cref="Ventana"/>, con el GUID fijo de
    /// <c>newIdFactory</c>; la cultura del hilo STA se fija a la invariante y se restaura en un <c>finally</c>; el orden es
    /// ordinal; no hay hora ni aleatoriedad. Ninguna ventana se muestra (salvo la de CT-PB0-14, fuera de pantalla) y toda
    /// ventana se cierra en un <c>finally</c>.</para>
    ///
    /// <para><b>Sin escrituras.</b> Nada de aqui ejecuta «Guardar en biblioteca», el BOM, el configurador real ni un
    /// <c>MessageBox</c> real (INV-PBT0-06): la salida de una ventana se LEE de su <c>LastComputation.Design</c>.</para>
    /// </summary>
    internal static class PushBackFullEditorCharacterizationSupport
    {
        /// <summary>El GUID que <c>newIdFactory</c> devuelve: ningun flujo de PB-T0 deberia acuñar otro.</summary>
        internal const string FixedId = "00000000-0000-4000-8000-0000000000b0";

        // ---- STA, cultura y ventanas ----------------------------------------------------------------------------

        /// <summary>Ejecuta <paramref name="body"/> en el hilo STA compartido con la cultura invariante fijada y restaurada.</summary>
        internal static void Sta(Action body)
            => StaTestRunner.Run(() =>
            {
                var culture = CultureInfo.CurrentCulture;
                var uiCulture = CultureInfo.CurrentUICulture;
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
                try
                {
                    body();
                }
                finally
                {
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = uiCulture;
                }
            });

        private static readonly ConditionalWeakTable<Window, object> ClosedWindows = new ConditionalWeakTable<Window, object>();

        /// <summary>La ventana clasica, sin mostrar, con el GUID fijo (M-11).</summary>
        internal static RackPushBackSystemWindow Ventana()
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true, newIdFactory: () => FixedId);
            window.Closed += (sender, args) => ClosedWindows.GetValue(window, key => new object());
            return window;
        }

        /// <summary>Cierra la ventana si no se cerro ya (Actualizar la cierra ella misma). Va en el <c>finally</c> de cada prueba.</summary>
        internal static void Cerrar(Window window)
        {
            if (window != null && !ClosedWindows.TryGetValue(window, out _))
            {
                window.Close();
            }
        }

        /// <summary>True cuando la ventana ya se cerro (por si misma o con <see cref="Cerrar"/>).</summary>
        internal static bool EstaCerrada(Window window) => ClosedWindows.TryGetValue(window, out _);

        // ---- El corpus ------------------------------------------------------------------------------------------

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        /// <summary>El texto del sobre de un fixture de I64 (solo lectura).</summary>
        internal static string Fixture(string name)
        {
            var path = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        /// <summary>Los diecisiete diseños de I64 en orden ordinal.</summary>
        internal static IReadOnlyList<string> CorpusNames()
        {
            var dir = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64");
            return Directory.GetFiles(dir, "pb-*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.Ordinal).ToList();
        }

        /// <summary>Edita el JSON INTERIOR de un sobre (la propiedad Design) y devuelve el texto del sobre completo.</summary>
        internal static string EditInner(string envelopeText, Action<JsonNode> edit)
        {
            var envelope = JsonNode.Parse(envelopeText);
            var inner = JsonNode.Parse(envelope["Design"].GetValue<string>());
            edit(inner);
            envelope["Design"] = inner.ToJsonString();
            return envelope.ToJsonString();
        }

        /// <summary>El nodo <c>PushBack.Structure</c> del JSON interior.</summary>
        internal static JsonNode StructureNode(JsonNode inner) => inner["PushBack"]["Structure"];

        // ---- Abrir, Salida y Sobre ------------------------------------------------------------------------------

        internal static string Persist(PushBackDesign design)
            => new RackProjectStore().Serialize(RackProject.ForPushBack(design)).Replace("\r\n", "\n");

        /// <summary>Abrir(t): deserializa el sobre, crea <see cref="Ventana"/> y llama a LoadExisting, sin mostrarla.</summary>
        internal static RackPushBackSystemWindow Abrir(string envelopeText)
        {
            var embed = new RackEmbedStore().Deserialize(envelopeText);
            var project = new RackProjectStore().Deserialize(embed.Design);
            var window = Ventana();
            window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);
            return window;
        }

        /// <summary>
        /// Abrir(t) con la ventana MOSTRADA fuera de pantalla y sin activarse: solo la usa CT-PB0-14, porque la rejilla por
        /// poste de botas se crea con <c>Owner = this</c> (W:2716-2721) antes de su costura y exige la ventana mostrada.
        /// </summary>
        internal static RackPushBackSystemWindow AbrirFueraDePantalla(string envelopeText)
        {
            var embed = new RackEmbedStore().Deserialize(envelopeText);
            var project = new RackProjectStore().Deserialize(embed.Design);
            var window = Ventana();
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.ShowInTaskbar = false;
            window.ShowActivated = false;
            window.Left = -10000;
            window.Top = -10000;
            window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);
            window.Show();
            window.UpdateLayout();
            return window;
        }

        /// <summary>El Id del sobre.</summary>
        internal static string IdOf(string envelopeText) => new RackEmbedStore().Deserialize(envelopeText).Id;

        /// <summary>Salida(w): lo que la ventana ESCRIBIRIA, leido de su LastComputation.Design (nunca se ejecuta Insertar).</summary>
        internal static string Salida(RackPushBackSystemWindow window)
            => window.CurrentInputsAreValid && window.LastComputation != null && window.LastComputation.IsValid
                ? Persist(window.LastComputation.Design)
                : "NO-VÁLIDA: " + StatusText(window);

        /// <summary>Sobre(t): lo mismo sobre el diseño interior DESERIALIZADO OTRA VEZ desde t, nunca el objeto que recibio la ventana.</summary>
        internal static string Sobre(string envelopeText)
            => Persist(new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(envelopeText).Design).PushBackDesign);

        /// <summary>El <c>IsComposite</c> del diseño que un texto de salida persiste (se lee con el almacen del producto).</summary>
        internal static bool IsComposite(string persistedDesign)
            => new RackProjectStore().Deserialize(persistedDesign).PushBackDesign.IsComposite;

        // ---- Delta y clases -------------------------------------------------------------------------------------

        /// <summary>Delta(x, y): las lineas «ruta: x -> y» en orden ordinal, con el recorrido de WLC:95-181 (copiado, no referenciado).</summary>
        internal static List<string> Delta(string x, string y)
        {
            var into = new List<string>();
            using (var a = JsonDocument.Parse(x))
            using (var b = JsonDocument.Parse(y))
            {
                Walk(a.RootElement, b.RootElement, "$", into);
            }

            return into.OrderBy(line => line, StringComparer.Ordinal).ToList();
        }

        private static bool IsContainer(JsonElement element) =>
            element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array;

        private static void Walk(JsonElement a, JsonElement b, string path, List<string> into)
        {
            if (a.ValueKind == JsonValueKind.Object && b.ValueKind == JsonValueKind.Object)
            {
                var names = a.EnumerateObject().Select(p => p.Name).Concat(b.EnumerateObject().Select(p => p.Name)).Distinct().OrderBy(n => n, StringComparer.Ordinal);
                foreach (var name in names)
                {
                    var hasA = a.TryGetProperty(name, out var left);
                    var hasB = b.TryGetProperty(name, out var right);
                    if (hasA && hasB) Walk(left, right, path + "." + name, into);
                    else if (hasA) Leaves(left, path + "." + name, "<absent>", true, into);
                    else Leaves(right, path + "." + name, "<absent>", false, into);
                }

                return;
            }

            if (a.ValueKind == JsonValueKind.Array && b.ValueKind == JsonValueKind.Array)
            {
                var count = Math.Max(a.GetArrayLength(), b.GetArrayLength());
                for (var index = 0; index < count; index++)
                {
                    var itemPath = path + "[" + index + "]";
                    if (index < a.GetArrayLength() && index < b.GetArrayLength()) Walk(a[index], b[index], itemPath, into);
                    else if (index < a.GetArrayLength()) Leaves(a[index], itemPath, "<absent>", true, into);
                    else Leaves(b[index], itemPath, "<absent>", false, into);
                }

                return;
            }

            if (IsContainer(a) && !IsContainer(b)) { Leaves(a, path, Short(b), true, into); return; }
            if (!IsContainer(a) && IsContainer(b)) { Leaves(b, path, Short(a), false, into); return; }

            if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
            {
                if (a.GetDouble() != b.GetDouble()) into.Add(path + ": " + Short(a) + " -> " + Short(b));
                return;
            }

            if (a.ValueKind != b.ValueKind || a.GetRawText() != b.GetRawText())
            {
                into.Add(path + ": " + Short(a) + " -> " + Short(b));
            }
        }

        private static void Leaves(JsonElement element, string path, string other, bool before, List<string> into)
        {
            if (element.ValueKind == JsonValueKind.Object && element.EnumerateObject().Any())
            {
                foreach (var property in element.EnumerateObject()) Leaves(property.Value, path + "." + property.Name, other, before, into);
                return;
            }

            if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0)
            {
                var index = 0;
                foreach (var item in element.EnumerateArray()) Leaves(item, path + "[" + index++ + "]", other, before, into);
                return;
            }

            var text = element.ValueKind == JsonValueKind.Object ? "{}" : element.ValueKind == JsonValueKind.Array ? "[]" : Short(element);
            into.Add(path + ": " + (before ? text + " -> " + other : other + " -> " + text));
        }

        private static string Short(JsonElement element)
            => element.ValueKind == JsonValueKind.Number
                ? element.GetDouble().ToString("R", CultureInfo.InvariantCulture)
                : element.GetRawText();

        /// <summary>R3(v): el valor tal como lo devuelve un control formateado con «0.###».</summary>
        internal static double R3(double value)
            => double.Parse(value.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        /// <summary>
        /// La clase de una linea «ruta: x -> y» (M-01): <c>a</c> cuando x e y son numeros, x != y e y == R3(x);
        /// <c>b</c> cuando la ruta es el estilo de cotas; vacia en cualquier otro caso (c, d, CARGA-APPLICATION u OTRA).
        /// </summary>
        internal static string ClassOf(string line)
        {
            if (line.StartsWith("$.PushBack.Structure.DimensionStyle: ", StringComparison.Ordinal)) return "b";

            var colon = line.IndexOf(": ", StringComparison.Ordinal);
            var arrow = line.IndexOf(" -> ", StringComparison.Ordinal);
            if (colon > 0 && arrow > colon
                && double.TryParse(line.Substring(colon + 2, arrow - colon - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(line.Substring(arrow + 4), NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                && x != y
                && y == R3(x))
            {
                return "a";
            }

            return string.Empty;
        }

        /// <summary>El delta de Sobre frente a Salida SIN las lineas de clase (a) y (b) (D-PBT0-03): se retiran antes de comparar.</summary>
        internal static List<string> Filtered(string sobre, string salida)
            => Delta(sobre, salida).Where(line => ClassOf(line).Length == 0).ToList();

        /// <summary>Las lineas de clase (a) o (b) que el filtro retiro, en el mismo orden: la entrega las lista.</summary>
        internal static List<string> Removed(string sobre, string salida)
            => Delta(sobre, salida).Where(line => ClassOf(line).Length > 0).ToList();

        /// <summary>El SHA-256 (hex en minusculas) de las lineas unidas con salto de linea: una huella exacta y compacta de una lista larga.</summary>
        internal static string Sha(IEnumerable<string> lines)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines))).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>
        /// El contenedor al que pertenece una linea del delta: hasta el primer indice o, si no lo tiene, los tres primeros
        /// segmentos de la ruta.
        /// </summary>
        internal static string Group(string line)
        {
            var path = line.Substring(0, line.IndexOf(": ", StringComparison.Ordinal));
            var close = path.IndexOf(']');
            if (close >= 0) return path.Substring(0, close + 1);
            return string.Join(".", path.Split('.').Take(3));
        }

        /// <summary>
        /// El delta como texto fijable: el numero de lineas y su huella SHA-256 siempre; las lineas enteras si son pocas y, si son
        /// muchas, el numero de lineas por contenedor.
        /// </summary>
        internal static string Describe(IReadOnlyList<string> delta)
            => "n=" + delta.Count + " sha256=" + Sha(delta)
               + (delta.Count <= 14
                   ? " lineas=[" + string.Join(" ;; ", delta) + "]"
                   : " contenedores=[" + string.Join("; ", delta.GroupBy(Group).Select(g => g.Key + " x" + g.Count())) + "]");

        // ---- Controles ------------------------------------------------------------------------------------------

        internal static NumericField Field(Window window, string name) => (NumericField)window.FindName(name);
        internal static CheckBox Check(Window window, string name) => (CheckBox)window.FindName(name);
        internal static ComboBox Combo(Window window, string name) => (ComboBox)window.FindName(name);
        internal static Button Btn(Window window, string name) => (Button)window.FindName(name);
        internal static string StatusText(Window window) => ((TextBlock)window.FindName("StatusText")).Text;

        internal static void Click(Window window, string name) => EditorWindowTestSupport.ClickNamed(window, name);

        /// <summary>Escribe un numero con «R» (sin perdida) en un campo y lo compromete saliendo del campo (LostFocus elevado).</summary>
        internal static void SetNumberR(Window window, string name, double value)
        {
            var field = Field(window, name);
            field.SetNumber(value, "R");
            field.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, field));
        }

        /// <summary>El valor numerico actual de un campo, tal cual lo tiene el control.</summary>
        internal static double? ValueOf(Window window, string name) => Field(window, name).Value;

        /// <summary>Elige un modulo en el desplegable por su ModuleId, segun la sesion de modulos del editor.</summary>
        internal static void SelectModule(RackPushBackSystemWindow window, string moduleId)
        {
            Check(window, "AdvancedModulesToggle").IsChecked = true;
            var module = window.EditorStateForTest.ModuleSession.Modules.First(m => m.ModuleId == moduleId);
            Combo(window, "ModuleBox").SelectedIndex = module.Index;
        }

        /// <summary>La secuencia de modulos del ultimo diseño: (ModuleId, Kind, Length, IsCalculated, IsManualOverride).</summary>
        internal static string ModuleSequence(RackPushBackSystemWindow window)
            => string.Join(
                ";",
                window.LastComputation.Design.Structure.Modules.Select(module =>
                    module.ModuleId + "/" + module.Kind + "/" + module.Length.ToString("R", CultureInfo.InvariantCulture)
                    + "/" + (module.IsCalculated ? "calc" : "-") + "/" + (module.IsManualOverride ? "manual" : "-")));
    }
}
