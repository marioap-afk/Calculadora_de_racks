using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-54 G7 — utilidades de las pruebas de la ventana de propiedades personalizadas (Proposal V5 D-18.4, §12.7).
    ///
    /// <para>
    /// Los workspaces se construyen como los construye el Plugin: el Proyecto sale del store de produccion sobre un
    /// payload, y el Rack de la autoridad de produccion sobre definiciones cuyo sobre se lee de su texto JSON. Ningun
    /// tipo de AutoCAD cruza a estas pruebas, y la ventana nunca ve el store, la autoridad ni el commit: solo lo que
    /// estas utilidades le entregan.
    /// </para>
    /// </summary>
    internal static class CustomPropertiesWindowTestKit
    {
        internal const string RackA = "a1a1a1a1-1111-4111-8111-aaaaaaaaaaaa";

        internal const string IdA = "0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10";

        internal const string IdB = "5d1c2a90-7b44-4f0e-8a3b-0c9e6d2f1a77";

        internal const string IdC = "9f4e3b2a-1c0d-4e5f-8a7b-6c5d4e3f2a1b";

        /// <summary>Los kinds que este build conoce, sin distinguir mayusculas, como hace el registro de handlers.</summary>
        internal static readonly Func<string, bool> KnownKinds = kind =>
            new[] { "selective", "dynamic", "cabecera", "cama", "pushback", "cantilever" }
                .Contains(kind, StringComparer.OrdinalIgnoreCase);

        /// <summary>Dos propiedades bien formadas, en el orden en que la ventana tiene que listarlas.</summary>
        internal static readonly string Base = Props(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Área", "Norte"));

        // ------------------------------------------------------------------ JSON

        internal static string JsonString(string value) => JsonSerializer.Serialize(value);

        internal static string Entry(string id, string name, string value)
            => "{\"Id\":\"" + id + "\",\"Name\":" + JsonString(name) + ",\"Value\":" + JsonString(value) + "}";

        internal static string Doc(string version, string entries, string extra = "")
            => "{\"SchemaVersion\":\"" + version + "\",\"Entries\":" + entries + extra + "}";

        internal static string Props(params string[] entries) => Doc("1.0", "[" + string.Join(",", entries) + "]");

        /// <summary>Anida <paramref name="levels"/> objetos alrededor de un numero.</summary>
        internal static string Nested(int levels)
            => new StringBuilder().Insert(0, "{\"x\":", levels).Append('1').Append('}', levels).ToString();

        /// <summary>Una coleccion por encima de la cota de profundidad del formato 1.x.</summary>
        internal static string TooDeep() => Doc("1.0", "[]", ",\"P\":" + Nested(16));

        /// <summary>Dos entradas con el mismo id: identidad ambigua.</summary>
        internal static string Ambiguous() => Props(Entry(IdA, "Uno", "1"), Entry(IdA, "Dos", "2"));

        /// <summary>Una coleccion de un MAJOR futuro.</summary>
        internal static string FutureMajor() => Doc("3.1", "[]");

        /// <summary>Un contenedor presente que no es una coleccion.</summary>
        internal const string Unreadable = "[1]";

        // ------------------------------------------------------------------ Proyecto

        internal static CustomPropertiesWorkspace Project(string text)
            => CustomPropertiesWorkspace.ForProject(new CustomPropertiesStore().Read(CustomPropertiesPayload.Present(text)));

        internal static CustomPropertiesWorkspace ProjectAbsent()
            => CustomPropertiesWorkspace.ForProject(new CustomPropertiesStore().Read(CustomPropertiesPayload.Absent()));

        // ------------------------------------------------------------------ Rack

        internal static string EnvelopeText(string rackId, string properties, string kind, string view, int section)
        {
            var builder = new StringBuilder();
            builder.Append("{\"SchemaVersion\":\"1.0\",\"Kind\":").Append(kind == null ? "null" : JsonString(kind));
            builder.Append(",\"View\":").Append(JsonString(view));
            builder.Append(",\"Section\":").Append(section);
            builder.Append(",\"Id\":").Append(JsonString(rackId));
            builder.Append(",\"Name\":\"Rack A\",\"Design\":\"{}\"");

            if (properties != null)
            {
                builder.Append(",\"CustomProperties\":").Append(properties);
            }

            return builder.Append('}').ToString();
        }

        /// <summary>Una definicion interpretable: su sobre sale del texto por el store de produccion.</summary>
        internal static RackCustomPropertiesDefinition View(
            string handle,
            string properties = null,
            string rackId = RackA,
            string kind = "selective",
            string view = "frontal",
            int section = -1)
        {
            var envelope = new RackEmbedStore().Deserialize(EnvelopeText(rackId, properties, kind, view, section));
            Assert.NotNull(envelope);
            return new RackCustomPropertiesDefinition(handle, "RACK_" + handle, true, false, envelope);
        }

        /// <summary>Una definicion con payload RackCad que este build no interpreta: llega sin sobre.</summary>
        internal static RackCustomPropertiesDefinition Uninterpretable(string handle, bool placed = false)
            => new RackCustomPropertiesDefinition(handle, "RACK_" + handle, placed, false, null);

        internal static RackCustomPropertiesAuthorityResult Authority(string selected, params RackCustomPropertiesDefinition[] definitions)
            => RackCustomPropertiesAuthority.Evaluate(definitions, new RackCustomPropertiesSelection(selected, false), KnownKinds);

        /// <summary>Un rack Single: dos vistas con la misma coleccion.</summary>
        internal static RackCustomPropertiesAuthorityResult SingleRack(string properties = null)
            => Authority("1", View("1", properties ?? Base), View("2", properties ?? Base, view: "lateral", section: 0));

        /// <summary>
        /// Un rack Divergent con unificacion disponible: la frontal con <see cref="Base"/>, la lateral 4 con otra coleccion y
        /// la planta sin coleccion.
        /// </summary>
        internal static RackCustomPropertiesAuthorityResult DivergentRack()
            => Authority(
                "1",
                View("1", Base),
                View("2", Props(Entry(IdA, "Cliente", "OTRO")), view: "lateral", section: 4),
                View("3", view: "planta"));

        /// <summary>Un rack Divergent en el que ninguna vista puede ser origen: las dos llevan miembros desconocidos.</summary>
        internal static RackCustomPropertiesAuthorityResult DivergentRackWithoutUnify()
            => Authority(
                "1",
                View("1", Doc("1.0", "[" + Entry(IdA, "Cliente", "ACME") + "]", ",\"Raiz\":1")),
                View("2", Doc("1.0", "[" + Entry(IdA, "Cliente", "OTRO") + "]", ",\"Raiz\":2"), view: "lateral", section: 0));

        internal static CustomPropertiesWorkspace Rack(RackCustomPropertiesAuthorityResult authority)
            => CustomPropertiesWorkspace.ForRack(authority);

        /// <summary>Lo que el comando captura de un rack escribible antes de abrir la ventana.</summary>
        internal static RackCustomPropertiesDisplayedState Displayed(RackCustomPropertiesAuthorityResult authority)
            => RackCustomPropertiesDisplayedState.Capture(authority);

        // ------------------------------------------------------------------ estados de solo lectura (D-18.5)

        /// <summary>
        /// Cada estado de solo lectura que la orden de G7 enumera, con el alcance, el motivo del workspace y, cuando el motivo
        /// es el contenedor de un rack, el estado de la vista que lo bloquea.
        /// </summary>
        internal static IReadOnlyList<ReadOnlyCase> ReadOnlyCases() => new[]
        {
            new ReadOnlyCase("rack-xref", CustomPropertiesReadOnlyReason.XrefRejected, null, () => Rack(
                RackCustomPropertiesAuthority.Evaluate(new[] { View("1", Base) }, new RackCustomPropertiesSelection("X", true), KnownKinds))),
            new ReadOnlyCase("rack-noidentity", CustomPropertiesReadOnlyReason.NoIdentity, null, () => Rack(
                Authority("1", View("1", Base, rackId: "")))),
            new ReadOnlyCase("rack-indeterminate", CustomPropertiesReadOnlyReason.IndeterminateMembership, null, () => Rack(
                Authority("1", View("1", Base), Uninterpretable("9")))),
            new ReadOnlyCase("rack-mixed", CustomPropertiesReadOnlyReason.MixedKind, null, () => Rack(
                Authority("1", View("1", Base), View("2", Base, kind: null, view: "lateral", section: 0)))),
            new ReadOnlyCase("rack-unknown", CustomPropertiesReadOnlyReason.UnknownKind, null, () => Rack(
                Authority("1", View("1", Base, kind: "futuro")))),
            new ReadOnlyCase("rack-unreadable", CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly, CustomPropertiesReadOutcome.PresentButUnreadable, () => Rack(
                Authority("1", View("1", Unreadable)))),
            new ReadOnlyCase("rack-ambiguous", CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly, CustomPropertiesReadOutcome.AmbiguousIdentity, () => Rack(
                Authority("1", View("1", Ambiguous())))),
            new ReadOnlyCase("rack-major", CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly, CustomPropertiesReadOutcome.IncompatibleMajor, () => Rack(
                Authority("1", View("1", FutureMajor())))),
            new ReadOnlyCase("rack-depth", CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly, CustomPropertiesReadOutcome.DepthLimitExceeded, () => Rack(
                Authority("1", View("1", TooDeep())))),
            new ReadOnlyCase("project-unreadable", CustomPropertiesReadOnlyReason.PresentButUnreadable, null, () => Project(Unreadable)),
            new ReadOnlyCase("project-ambiguous", CustomPropertiesReadOnlyReason.AmbiguousIdentity, null, () => Project(Ambiguous())),
            new ReadOnlyCase("project-major", CustomPropertiesReadOnlyReason.IncompatibleMajor, null, () => Project(FutureMajor())),
            new ReadOnlyCase("project-depth", CustomPropertiesReadOnlyReason.DepthLimitExceeded, null, () => Project(TooDeep())),
        };

        internal static ReadOnlyCase ReadOnly(string name) => ReadOnlyCases().Single(item => item.Name == name);

        internal sealed class ReadOnlyCase
        {
            internal ReadOnlyCase(
                string name, CustomPropertiesReadOnlyReason reason, CustomPropertiesReadOutcome? viewState, Func<CustomPropertiesWorkspace> build)
            {
                Name = name;
                Reason = reason;
                ViewState = viewState;
                Build = build;
            }

            internal string Name { get; }

            internal CustomPropertiesReadOnlyReason Reason { get; }

            /// <summary>El estado de la vista que bloquea un rack <c>CustomPropertiesReadOnly</c>; null en los demas.</summary>
            internal CustomPropertiesReadOutcome? ViewState { get; }

            internal Func<CustomPropertiesWorkspace> Build { get; }
        }

        // ------------------------------------------------------------------ la ventana real

        internal static RackCustomPropertiesWindow Open(CustomPropertiesWorkspace workspace, RackCustomPropertiesDisplayedState displayed = null)
            => new RackCustomPropertiesWindow(workspace, displayed);

        internal static T Named<T>(Window window, string name) where T : class
        {
            var element = window.FindName(name) as T;
            Assert.True(element != null, "La ventana no tiene " + typeof(T).Name + " '" + name + "'.");
            return element;
        }

        internal static Button Button(Window window, string name) => Named<Button>(window, name);

        internal static ListBox List(Window window) => Named<ListBox>(window, "PropertiesList");

        /// <summary>Los botones de la ventana, por etiqueta, recorriendo el arbol logico.</summary>
        internal static IReadOnlyList<Button> Buttons(Window window) => EditorWindowTestSupport.FindAll<Button>(window);

        internal static IReadOnlyList<string> ButtonLabels(Window window)
            => Buttons(window).Select(button => button.Content as string).OrderBy(label => label, StringComparer.Ordinal).ToList();

        internal static IReadOnlyList<RadioButton> UnifySources(Window window) => EditorWindowTestSupport.FindAll<RadioButton>(window);

        internal static string RadioText(RadioButton radio)
            => radio.Content is TextBlock block ? block.Text : radio.Content as string;

        /// <summary>Todo el texto visible de la ventana en el arbol logico, en orden.</summary>
        internal static string AllText(Window window)
            => string.Join(
                "\n",
                EditorWindowTestSupport.Descendants(window).Select(node => node switch
                {
                    TextBlock block => block.Text,
                    ContentControl control when control.Content is string text => text,
                    _ => null,
                })
                .Where(text => !string.IsNullOrEmpty(text)));

        internal static string PresenterText(Window window, string name)
        {
            var presenter = Named<RackCad.UI.Shell.EditorStatusPresenter>(window, name);
            return presenter.Visibility == Visibility.Visible ? presenter.Message?.Text ?? string.Empty : string.Empty;
        }

        // ------------------------------------------------------------------ fuentes

        internal static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizo la raiz del repo (RackCad.sln).");
            return dir;
        }

        internal static string Source(params string[] relative)
            => File.ReadAllText(Path.Combine(new[] { RepoRoot().FullName }.Concat(relative).ToArray())).Replace("\r\n", "\n");

        internal static string WindowCodeText() => Source("src", "RackCad.UI", "RackCustomPropertiesWindow.xaml.cs");

        internal static string WindowXamlText() => Source("src", "RackCad.UI", "RackCustomPropertiesWindow.xaml");

        /// <summary>El codigo C# sin comentarios: una guarda caza acoplamientos escritos en codigo, no prosa que los nombre.</summary>
        internal static string CodeWithoutComments(string text)
            => Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline), @"//[^\n]*", string.Empty);

        internal static string XamlWithoutComments(string text)
            => Regex.Replace(text, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
    }
}
