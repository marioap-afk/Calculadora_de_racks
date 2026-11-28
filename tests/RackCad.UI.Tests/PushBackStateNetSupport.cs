using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Systems.PushBack;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1a — los ayudantes comunes de la red de estado de la ventana CLASICA de Push Back (paquete del contrato de
    /// PB-T1, §5.0 y §5.2): las recetas de los diseños fabricados X-01, F-01, F-02 y F-03 y las huellas de la salida persistida.
    /// Solo ejecucion: usa los ayudantes de PB-T0 y PB-TC (<see cref="PushBackFullEditorCharacterizationSupport"/> y
    /// <see cref="PushBackDeclaredCorrectionSupport"/>), que no se editan, y las costuras internas que la ventana ya declara. No
    /// contiene pruebas, no crea ningun estado ni copia de estado y no muta el rack resuelto ni el informe de modulos de una
    /// ventana viva: cada receta solo teclea en los controles de la ventana, como lo haria el usuario.
    ///
    /// <para>Toda ventana se crea con <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va DENTRO del
    /// <c>try</c> y <see cref="PushBackFullEditorCharacterizationSupport.Cerrar"/> va en el <c>finally</c>. Nunca <c>Abrir</c> ni
    /// <c>AbrirFueraDePantalla</c>; ninguna ventana se muestra; sin teclado ni foco real.</para>
    /// </summary>
    internal static class PushBackStateNetSupport
    {
        /// <summary>La rejilla de la matriz de tarjetas de la ventana.</summary>
        internal const string Grid = "PushBackMatrixGrid";

        /// <summary>
        /// Un diseño del corpus extendido: su nombre y la manera de cargarlo en una ventana NUEVA. Los de <c>Fixtures/I64</c> cargan
        /// su sobre; los fabricados (X-01, F-01 y F-02) cargan una copia, reabierta del diseño de su receta, con el mismo id y nombre.
        /// </summary>
        internal sealed class Caso
        {
            internal Caso(string nombre, Action<RackPushBackSystemWindow> cargar)
            {
                Nombre = nombre;
                Cargar = cargar;
            }

            internal string Nombre { get; }

            internal Action<RackPushBackSystemWindow> Cargar { get; }
        }

        // ---- Lectura de la salida persistida ---------------------------------------------------------------------

        /// <summary>
        /// Leer(json, ruta): el nodo de esa ruta (bajo <c>PushBack</c>) del JSON persistido, en su texto JSON compacto, o
        /// <c>&lt;null&gt;</c> si falta o es nulo. Los numeros salen con la forma mas corta que vuelve al mismo valor.
        /// </summary>
        internal static string Leer(string json, string ruta)
        {
            var node = JsonNode.Parse(json)["PushBack"];
            foreach (var segment in ruta.Split('.'))
            {
                var bracket = segment.IndexOf('[');
                var name = bracket < 0 ? segment : segment.Substring(0, bracket);
                node = node?[name];
                while (bracket >= 0 && node != null)
                {
                    var close = segment.IndexOf(']', bracket);
                    var index = int.Parse(segment.Substring(bracket + 1, close - bracket - 1), CultureInfo.InvariantCulture);
                    var array = node as JsonArray;
                    node = array != null && index < array.Count ? array[index] : null;
                    bracket = segment.IndexOf('[', close);
                }
            }

            return node == null ? "<null>" : node.ToJsonString();
        }

        /// <summary>Cuenta(json, ruta): el numero de elementos del arreglo de esa ruta (bajo <c>PushBack</c>), o <c>&lt;null&gt;</c> si no es un arreglo.</summary>
        internal static string Cuenta(string json, string ruta)
        {
            var node = JsonNode.Parse(json)["PushBack"];
            foreach (var segment in ruta.Split('.'))
            {
                node = node?[segment];
            }

            return node is JsonArray array ? array.Count.ToString(CultureInfo.InvariantCulture) : "<null>";
        }

        // ---- Huellas ----------------------------------------------------------------------------------------------

        /// <summary>La huella SHA-256 (hex en minusculas) del texto persistido de un diseño.</summary>
        internal static string Huella(string persistido) => Sha(new[] { persistido });

        /// <summary>True cuando las entradas vigentes de la ventana recalculan a un modelo valido (la condicion de <c>Salida</c>).</summary>
        internal static bool Valido(RackPushBackSystemWindow w)
            => w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid;

        /// <summary>La huella del diseño de la ultima computacion valida de la ventana, o «-» si no hay ninguna.</summary>
        internal static string HuellaDe(RackPushBackSystemWindow w)
            => w.LastComputation?.Design == null ? "-" : Huella(Persist(w.LastComputation.Design));

        /// <summary>La linea de CT-PBT1A-14: <c>&lt;diseño&gt; &lt;guion&gt; valido=&lt;b&gt; bloqueado=&lt;b&gt; sha256=&lt;huella&gt;</c>.</summary>
        internal static string Linea(string diseno, string guion, RackPushBackSystemWindow w, string huella)
            => diseno + " " + guion + " valido=" + Valido(w) + " bloqueado=" + w.OutputIsBlockedForTest + " sha256=" + huella;

        // ---- Recetas ----------------------------------------------------------------------------------------------

        /// <summary>El diseño de un diseño ya persistido, deserializado otra vez: una copia independiente del que recibio otra ventana.</summary>
        private static PushBackDesign Reabierto(string persistido)
            => new RackProjectStore().Deserialize(persistido).PushBackDesign;

        /// <summary>
        /// Los diecisiete diseños de <c>Fixtures/I64</c>, en orden ordinal, como casos de ventana nueva (el sobre completo, con el
        /// proyecto de origen, por <c>Cargar</c>).
        /// </summary>
        internal static IReadOnlyList<Caso> Fixtures()
            => CorpusNames().Select(nombre => new Caso(nombre, w => Cargar(w, Fixture(nombre)))).ToList();

        /// <summary>
        /// X-01 (ventana): copia literal de <c>BuildX01</c> (<c>PushBackFullEditorCharacterizationStateTests.cs:90-113</c>):
        /// <c>pb-21-cola</c>; <c>SideBPresentCheck</c> a true; <c>SideSelectorBox</c> a 1; el frente 0 de la rejilla activo; y el
        /// diseño de su <c>LastComputation</c>, con su <c>RackId</c> y su <c>RackName</c>, reabierto con <c>LoadExisting</c>.
        /// </summary>
        internal static Caso BuildX01()
        {
            string persistido;
            string id;
            string nombre;
            var w = Ventana();
            try
            {
                Cargar(w, Fixture("pb-21-cola"));
                Check(w, "SideBPresentCheck").IsChecked = true;
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                EditorWindowTestSupport.SetFrontBlank(w, Grid, 0, false);
                persistido = Persist(w.LastComputation.Design);
                id = w.RackId;
                nombre = w.RackName;
            }
            finally { Cerrar(w); }

            return new Caso("X-01", destino => destino.LoadExisting(Reabierto(persistido), id, nombre));
        }

        /// <summary>
        /// F-01: <c>pb-20-ab</c> con la longitud del primer separador de A, y despues la del primer separador con prefijo
        /// <c>B:</c>, mas 2,0 cada una, por la sesion de modulos de la ventana (<c>SelectModule</c>, <c>ModuleLengthBox</c> y
        /// «Confirmar»); el diseño resultante se reabre.
        /// </summary>
        internal static Caso BuildF01()
        {
            string persistido;
            string id;
            string nombre;
            var w = Ventana();
            try
            {
                Cargar(w, Fixture("pb-20-ab"));
                foreach (var enB in new[] { false, true })
                {
                    var separador = w.EditorStateForTest.ModuleSession.Modules.First(
                        m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == enB);
                    SelectModule(w, separador.ModuleId);
                    SetNumberR(w, "ModuleLengthBox", separador.Length + 2.0);
                    Click(w, "ConfirmModuleButton");
                }

                persistido = Persist(w.LastComputation.Design);
                id = w.RackId;
                nombre = w.RackName;
            }
            finally { Cerrar(w); }

            return new Caso("F-01", destino => destino.LoadExisting(Reabierto(persistido), id, nombre));
        }

        /// <summary>F-02: <c>pb-20-ab</c> con <c>FrontalSideBox</c> a 1 («Frontal de B»); el diseño de su ultima computacion se reabre.</summary>
        internal static Caso BuildF02()
        {
            string persistido;
            string id;
            string nombre;
            var w = Ventana();
            try
            {
                Cargar(w, Fixture("pb-20-ab"));
                Combo(w, "FrontalSideBox").SelectedIndex = 1;
                persistido = Persist(w.LastComputation.Design);
                id = w.RackId;
                nombre = w.RackName;
            }
            finally { Cerrar(w); }

            return new Caso("F-02", destino => destino.LoadExisting(Reabierto(persistido), id, nombre));
        }

        /// <summary>
        /// F-03 (I-04): <c>pb-01-base</c> con <c>Structure.PalletTolerance = 3.00044</c>; el lado B declarado y sin frentes; el corte
        /// frontal de B; los avanzados con un refuerzo de 100000 (un calculo invalido); <c>ToleranceBox</c> a 4; y «Restaurar». Se
        /// comprueba a si misma: <c>RecomputePassesForTest</c> antes y despues del clic de «Restaurar» y <c>FrontalSideForTest</c>
        /// despues. Su unico guion es G-F03: devuelve su linea
        /// <c>F-03 G-F03 valido=&lt;b&gt; bloqueado=&lt;b&gt; sha256=&lt;huella&gt; PalletTolerance=&lt;R&gt; pasadas-restaurar=&lt;n&gt; frontal=&lt;lado&gt;</c>.
        /// </summary>
        internal static string LineaF03(Func<int> descartes)
        {
            var w = Ventana();
            try
            {
                Cargar(w, EditInner(Fixture("pb-01-base"), inner => StructureNode(inner)["PalletTolerance"] = 3.00044));
                Check(w, "SideBPresentCheck").IsChecked = true;
                Combo(w, "FrontalSideBox").SelectedIndex = 1;
                Check(w, "AdvancedModulesToggle").IsChecked = true;
                Check(w, "DerivedPostReinforcedCheck").IsChecked = true;
                SetNumberR(w, "DerivedPostReinforcementHeightBox", 100000);
                SetNumberR(w, "ToleranceBox", 4);
                var antes = w.RecomputePassesForTest;
                Click(w, "RestoreButton");
                var pasadas = w.RecomputePassesForTest - antes;
                var salida = w.LastComputation?.Design == null ? null : Persist(w.LastComputation.Design);
                var linea = Linea("F-03", "G-F03", w, HuellaDe(w))
                    + " PalletTolerance=" + (salida == null ? "<sin diseno>" : Leer(salida, "Structure.PalletTolerance"))
                    + " pasadas-restaurar=" + pasadas.ToString(CultureInfo.InvariantCulture)
                    + " frontal=" + w.FrontalSideForTest;
                Cerrar(w);
                return linea + " descartes=" + descartes();
            }
            finally { Cerrar(w); }
        }
    }
}
