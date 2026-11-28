using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Systems.PushBack;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2b - los ayudantes comunes de la red de la ventana de Push Back en sus dos modos (paquete del contrato de
    /// PB-T2, seccion 5.0, «Las recetas»). Solo ejecucion: usa los ayudantes de PB-T0 y PB-TC
    /// (<see cref="PushBackFullEditorCharacterizationSupport"/> y <see cref="PushBackDeclaredCorrectionSupport"/>), que no se
    /// editan, y las costuras internas de la ventana. No contiene pruebas.
    ///
    /// <para><b>Determinismo.</b> El id del borrador es <c>GUID-PBT2-0001</c>, la sesion es <c>SessionId(64)</c>, los numeros se
    /// escriben con «R» y la cultura invariante (la fija <see cref="PushBackFullEditorCharacterizationSupport.Sta"/>) y el orden
    /// es ordinal. No hay hora ni aleatoriedad.</para>
    ///
    /// <para><b>Sin escrituras.</b> La unica escritura en disco de la red es <see cref="ArchivoTemporal"/>, y su carpeta se crea
    /// antes del gesto y se borra en el <c>finally</c> de cada prueba. Ninguna ventana se muestra; nada abre un dialogo real.</para>
    /// </summary>
    internal static class PushBackWorkspaceSupport
    {
        /// <summary>El id del borrador (D-PBT2-02).</summary>
        internal const string IdBorrador = "GUID-PBT2-0001";

        /// <summary>El diseño base de casi todas las pruebas.</summary>
        internal const string DisenoBase = "pb-01-base";

        /// <summary>El diseño compuesto (los dos lados) de las pruebas.</summary>
        internal const string DisenoCompuesto = "pb-20-ab";

        // ---- Los once literales de D-PBT2-09, tal como los fija el paquete (la ventana los declara privados) -------------

        internal const string Rotulo = "Aplicar al borrador";
        internal const string AplicarTooltip = "Guarda los cambios en el borrador. El dibujo cambia con «Actualizar» del panel.";
        internal const string RechazoClasico = "En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel.";
        internal const string RechazoCarga = "La ventana del borrador solo carga el estado del borrador.";
        internal const string Invalido = "Corrige los datos: no se puede aplicar al borrador un modelo inválido.";
        internal const string SinCambios = "No hay cambios que aplicar al borrador.";
        internal const string Aplicado = "Cambios aplicados al borrador. El dibujo cambia con «Actualizar» del panel.";
        internal const string NoEditable = "El borrador no admite cambios ahora: cierra el editor y revisa el panel.";
        internal const string PreguntaSinAplicar = "Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?";
        internal const string PreguntaModulo = "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?";

        /// <summary>Los seis botones de insercion de la barra, los que WORKSPACE no muestra.</summary>
        internal static readonly string[] BotonesDeInsercion =
        {
            "InsertButton", "InsertLateralButton", "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "InsertPlantaButton",
            "InsertBatchButton",
        };

        // ---- La regla «EXCEPCION» ------------------------------------------------------------------------------------

        /// <summary>Un paso que lanza se vuelve una linea, y las lineas esperadas solo contienen las que fija el paquete.</summary>
        internal static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        internal static void Paso(List<string> log, Action step) => Paso<int>(log, () => { step(); return 0; });

        // ---- Los estados ---------------------------------------------------------------------------------------------

        private static readonly Lazy<RackCatalog> LazyCatalogo = new Lazy<RackCatalog>(() => JsonRackCatalogProvider.FromBaseDirectory().Load());

        private static readonly Lazy<PushBackEditorDesignAssembler> LazyAssembler
            = new Lazy<PushBackEditorDesignAssembler>(() => new PushBackEditorDesignAssembler(Catalogo));

        private static readonly Lazy<PushBackCompositeEditorAssembler> LazyCompuesto
            = new Lazy<PushBackCompositeEditorAssembler>(() => new PushBackCompositeEditorAssembler(Catalogo));

        /// <summary>El catalogo del producto (<c>JsonRackCatalogProvider.FromBaseDirectory().Load()</c>).</summary>
        internal static RackCatalog Catalogo => LazyCatalogo.Value;

        internal static PushBackEditorDesignAssembler Ensamblador => LazyAssembler.Value;

        internal static PushBackEditableState Copy(PushBackEditableState estado) => PushBackEditableStateComparer.Copy(estado);

        /// <summary>Igual(a, b): el comparador de PB-T1b.</summary>
        internal static bool Igual(PushBackEditableState a, PushBackEditableState b) => PushBackEditableStateComparer.Instance.Equals(a, b);

        /// <summary>Una pasada de recalculo como la corre la ventana (W:659-704), adaptada al estado (PT1, seccion 5.0).</summary>
        internal static PushBackEditorComputation Pasada(PushBackEditableState s, PushBackSide frontal)
        {
            PushBackEditorComputation computation;
            try { computation = Ensamblador.BuildFrom(LazyCompuesto.Value.BuildDesign(s.Editor, s.BuildInputs(Ensamblador)), frontal); }
            catch (Exception) { return null; }
            if (!computation.IsValid) return computation;
            Ensamblador.AcceptComputation(s.Editor.SideA, computation);
            s.Editor.CommitDormantTailConsumption();
            var standardRestoreLanded = s.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;
            s.Editor.SideA.ClearModuleCommit();
            if (standardRestoreLanded) s.Editor.Active.RestoreAdvancedRackParameters(s.Advanced);
            _ = s.Editor.SideA.ModuleSession;
            return computation;
        }

        internal static PushBackDesign DisenoDeSobre(string sobre)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(sobre).Design).PushBackDesign;

        internal static string NombreDeSobre(string sobre) => new RackEmbedStore().Deserialize(sobre).Name;

        /// <summary>Estado(diseño): una carga nueva, su seguridad por defecto, el diseño y una pasada despues de cada una.</summary>
        internal static PushBackEditableState Estado(PushBackDesign diseno, string nombre)
        {
            var s = new PushBackEditableState();
            s.LoadNew(Catalogo);
            Pasada(s, PushBackSide.A);
            s.SeedDefaultSafety(Catalogo);
            Pasada(s, PushBackSide.A);
            s.LoadDesign(diseno, nombre, Ensamblador.Resolver);
            Pasada(s, PushBackSide.A);
            return s;
        }

        /// <summary>Estado(sobre): el estado de un sobre de <c>Fixtures/I64</c>.</summary>
        internal static PushBackEditableState Estado(string sobre) => Estado(DisenoDeSobre(sobre), NombreDeSobre(sobre));

        /// <summary>X-01 (Core): <c>pb-21-cola</c>, el lado B declarado y elegido y su hueco 0 presente.</summary>
        internal static PushBackDesign X01Diseno()
        {
            var texto = Fixture("pb-21-cola");
            var estado = new PushBackEditableState();
            estado.LoadDesign(DisenoDeSobre(texto), NombreDeSobre(texto), Ensamblador.Resolver);
            Pasada(estado, PushBackSide.A);
            estado.Editor.SetSideBPresent(true);
            estado.Editor.SetActiveSelection(PushBackSideSelection.B);
            estado.Editor.SetSlotPresent(PushBackSide.B, 0, true);
            var computacion = Pasada(estado, PushBackSide.A);
            return new RackProjectStore().Deserialize(Persist(computacion.Design)).PushBackDesign;
        }

        /// <summary>F-01 (Core): <c>pb-20-ab</c> con el primer separador de A y despues el primero de B alargados 2.</summary>
        internal static PushBackDesign F01Diseno()
        {
            var texto = Fixture("pb-20-ab");
            var estado = new PushBackEditableState();
            estado.LoadDesign(DisenoDeSobre(texto), NombreDeSobre(texto), Ensamblador.Resolver);
            Pasada(estado, PushBackSide.A);
            PushBackEditorComputation computacion = null;
            foreach (var enB in new[] { false, true })
            {
                var separador = estado.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == enB);
                estado.Editor.SideA.ModuleSession.SetLength(separador.ModuleId, separador.Length + 2.0);
                estado.Editor.SideA.CommitModuleEdits();
                computacion = Pasada(estado, PushBackSide.A);
            }

            return new RackProjectStore().Deserialize(Persist(computacion.Design)).PushBackDesign;
        }

        /// <summary>El corpus «C18»: los 17 de <c>Fixtures/I64</c> sin <c>pb-21-cola</c>, mas X-01 y F-01. Son 18.</summary>
        internal static IReadOnlyList<(string Nombre, PushBackEditableState Estado)> CorpusC18()
        {
            var lista = new List<(string, PushBackEditableState)>();
            foreach (var nombre in CorpusNames().Where(n => n != "pb-21-cola"))
            {
                lista.Add((nombre, Estado(Fixture(nombre))));
            }

            lista.Add(("X-01", Estado(X01Diseno(), "X-01")));
            lista.Add(("F-01", Estado(F01Diseno(), "F-01")));
            return lista;
        }

        /// <summary>EstadoDeVentana: una ventana CLASSIC, la carga del sobre, los gestos y una copia de su estado editable.</summary>
        internal static PushBackEditableState EstadoDeVentana(string sobre, params Action<RackPushBackSystemWindow>[] gestos)
        {
            var w = Ventana();
            try
            {
                Cargar(w, sobre);
                foreach (var gesto in gestos)
                {
                    gesto(w);
                }

                return Copy(w.EditableStateForTest);
            }
            finally { Cerrar(w); }
        }

        // ---- El borrador, su puerto y la ventana de WORKSPACE ---------------------------------------------------------

        /// <summary>El borrador de un estado, como lo empieza el host: un registro, una sesion, una base y el estado.</summary>
        internal static (DraftRegistry Registro, DraftSession<PushBackEditableState> Draft) Borrador(PushBackEditableState s)
        {
            var registry = new DraftRegistry();
            var begin = registry.Begin(
                new SessionId(64),
                IdBorrador,
                DraftIntentRead<PushBackEditableState>.Admitted(
                    DraftStaleBase.Create(new[] { new DraftStaleBaseEntry("BLK-PBT2", DraftMembership.Mutable, "planta", null, "{}") }), s),
                PushBackEditableStateComparer.Instance,
                PushBackEditableStateComparer.Copy);
            return (registry, begin.Draft);
        }

        /// <summary>Un puerto de la prueba que cuenta las llamadas y delega en el puerto del borrador.</summary>
        internal sealed class PuertoContado : IPushBackDraftStatePort
        {
            private readonly IPushBackDraftStatePort interior;

            public PuertoContado(DraftSession<PushBackEditableState> draft)
            {
                interior = new PushBackDraftSessionPort(draft);
            }

            public int Llamadas { get; private set; }

            public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited)
            {
                Llamadas++;
                return interior.ApplyToDraft(edited);
            }
        }

        private static readonly ConditionalWeakTable<Window, object> VentanasCerradas = new ConditionalWeakTable<Window, object>();

        /// <summary>Registra el cierre de una ventana que no creo <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>.</summary>
        internal static T Registrar<T>(T ventana) where T : Window
        {
            ventana.Closed += (sender, args) => VentanasCerradas.GetValue(ventana, key => new object());
            return ventana;
        }

        internal static bool EstaCerradaVentana(Window ventana)
            => VentanasCerradas.TryGetValue(ventana, out _) || EstaCerrada(ventana);

        /// <summary>Cierra una sola vez una ventana registrada (de esta clase o de PB-T0). Va en el <c>finally</c> de cada prueba.</summary>
        internal static void CerrarVentana(Window ventana)
        {
            if (ventana != null && !EstaCerradaVentana(ventana))
            {
                ventana.Close();
            }
        }

        /// <summary>VentanaBorrador: la ventana de WORKSPACE de un lanzamiento, sin mostrar y con su registro de <c>Closed</c>.</summary>
        internal static RackPushBackSystemWindow VentanaBorrador(
            DraftSession<PushBackEditableState> draft, RackProject proyecto, IPushBackDraftStatePort puerto)
            => Registrar(RackPushBackSystemWindow.ForDraft(new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, proyecto, puerto)));

        /// <summary>
        /// Sondeo(w), la sonda del modo: si la ventana no es de WORKSPACE, deja la linea <c>modo=&lt;modo&gt;</c> y devuelve false:
        /// la prueba no hace ningun gesto mas y termina con su <c>Assert.Equal</c>.
        /// </summary>
        internal static bool Sondeo(List<string> log, RackPushBackSystemWindow w)
        {
            var modo = Paso(log, () => w.ModeForTest);
            if (modo == PushBackEditorMode.Workspace) return true;
            log.Add("modo=" + modo);
            return false;
        }

        /// <summary>El aviso de descarte sustituido: cuenta, recuerda la pregunta y responde lo que se le diga (copia de T-CL).</summary>
        internal sealed class PromptProbe
        {
            public PromptProbe(bool answer) { Answer = answer; }

            public bool Answer { get; set; }
            public int Asked { get; private set; }
            public string Question { get; private set; }

            public bool Ask(string question, Window owner)
            {
                Asked++;
                Question = question;
                return Answer;
            }

            /// <summary>Vuelve a cero el contador y la pregunta: una ventana nueva.</summary>
            public void Reiniciar()
            {
                Asked = 0;
                Question = null;
                Answer = true;
            }
        }

        /// <summary>Escenifica una edicion de modulo SIN confirmarla (copia de T-CL: StageModuleEdit).</summary>
        internal static void StageModuleEdit(RackPushBackSystemWindow window)
        {
            Check(window, "AdvancedModulesToggle").IsChecked = true;
            var module = window.EditorStateForTest.ModuleSession.Modules.First(m => m.IsLengthBearing);
            Combo(window, "ModuleBox").SelectedIndex = module.Index;
            var length = Field(window, "ModuleLengthBox");
            length.SetNumber(module.Length + 8.0);
            length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
        }

        /// <summary>Un recalculo sin editar ningun campo (copia de CT-PBT1C-02, la correccion C-1).</summary>
        internal static void Recalcular(RackPushBackSystemWindow w)
            => Field(w, "AnnotationScaleBox").RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, Field(w, "AnnotationScaleBox")));

        /// <summary>True cuando las entradas vigentes de la ventana recalculan a un modelo valido.</summary>
        internal static bool Valido(RackPushBackSystemWindow w)
            => w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid;

        /// <summary>DisenoDe(estado, frontal) = Persist(Pasada(Copy(estado), frontal).Design).</summary>
        internal static string DisenoDe(PushBackEditableState estado, PushBackSide frontal)
        {
            var computacion = Pasada(Copy(estado), frontal);
            return computacion == null || !computacion.IsValid ? "<sin diseno>" : Persist(computacion.Design);
        }

        /// <summary>El diseño persistido de la ultima computacion de la ventana.</summary>
        internal static string DisenoDeVentana(RackPushBackSystemWindow w)
            => w.LastComputation?.Design == null ? "<sin diseno>" : Persist(w.LastComputation.Design);

        /// <summary>Nombre(x) = &lt;null&gt; si x es nulo; si no, [x].</summary>
        internal static string Nombre(string x) => x == null ? "<null>" : "[" + x + "]";

        /// <summary>Sel(e) = la seleccion de un estado: su lado y el frente y el nivel seleccionados.</summary>
        internal static string Sel(PushBackEditableState e)
            => e.Editor.ActiveSelection + "/" + e.Editor.Active.Structure.SelectedFrontIndex + "," + e.Editor.Active.Structure.SelectedLevelIndex;

        /// <summary>La solicitud clasica de la ventana, o &lt;null&gt;.</summary>
        internal static string Solicitud(RackPushBackSystemWindow w)
            => w.InsertionRequest == null ? "<null>" : w.InsertionRequest.GetType().Name;

        // ---- El archivo temporal (la unica escritura en disco) -------------------------------------------------------

        /// <summary>La unica escritura en disco de la red; ninguna linea lleva su ruta.</summary>
        internal static string ArchivoTemporal
            => Path.Combine(Path.GetTempPath(), "rackcad-pbt2-0001", "biblioteca.rackcad.json");

        /// <summary>Crea la carpeta del archivo temporal, antes del gesto.</summary>
        internal static void CrearCarpetaTemporal() => Directory.CreateDirectory(Path.GetDirectoryName(ArchivoTemporal));

        /// <summary>Borra el archivo temporal si existe: antes de CADA clic en «Guardar en biblioteca».</summary>
        internal static void BorrarArchivoTemporal()
        {
            if (File.Exists(ArchivoTemporal)) File.Delete(ArchivoTemporal);
        }

        /// <summary>Borra la carpeta del archivo temporal: el <c>finally</c> de la prueba.</summary>
        internal static void LimpiarArchivoTemporal()
        {
            var carpeta = Path.GetDirectoryName(ArchivoTemporal);
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
        }

        /// <summary>El proyecto interior de un sobre con <c>"CampoFuturoPbt2": { "v": 1 }</c> en la raiz.</summary>
        internal static RackProject ProyectoConDesconocido(string sobre)
        {
            var interior = JsonNode.Parse(new RackEmbedStore().Deserialize(sobre).Design);
            interior["CampoFuturoPbt2"] = new JsonObject { ["v"] = 1 };
            return new RackProjectStore().Deserialize(interior.ToJsonString());
        }

        // ---- Los grafos y las huellas --------------------------------------------------------------------------------

        private static bool IsProductType(Type type)
            => type.Namespace != null && type.Namespace.StartsWith("RackCad", StringComparison.Ordinal) && !type.IsEnum;

        private static IEnumerable<FieldInfo> InstanceFields(Type type)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    yield return field;
                }
            }
        }

        /// <summary>Un tipo es inmutable cuando es una cadena o todos sus campos de instancia son de solo lectura e inmutables.</summary>
        private static bool IsImmutable(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum) return true;
            if (!IsProductType(type)) return false;
            return InstanceFields(type).All(field => field.IsInitOnly && (field.FieldType.IsValueType || IsImmutable(field.FieldType)));
        }

        /// <summary>Todo objeto por referencia que un grafo guarda (colecciones incluidas), recorriendo TODOS los campos.</summary>
        internal static HashSet<object> AllObjects(object root, Func<FieldInfo, bool> skip = null)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var pending = new Queue<object>();
            pending.Enqueue(root);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                if (current is string || !visited.Add(current)) continue;
                if (current is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Key != null && !entry.Key.GetType().IsValueType) pending.Enqueue(entry.Key);
                        if (entry.Value != null && !entry.Value.GetType().IsValueType) pending.Enqueue(entry.Value);
                    }
                }
                else if (current is IEnumerable items)
                {
                    foreach (var item in items)
                    {
                        if (item != null && !item.GetType().IsValueType) pending.Enqueue(item);
                    }
                }

                if (!IsProductType(current.GetType())) continue;
                foreach (var field in InstanceFields(current.GetType()))
                {
                    if (skip != null && skip(field)) continue;
                    if (field.FieldType.IsValueType && !field.FieldType.IsGenericType) continue;
                    var value = field.GetValue(current);
                    if (value != null && !value.GetType().IsValueType) pending.Enqueue(value);
                }
            }

            return visited;
        }

        private static HashSet<object> Mutable(IEnumerable<object> objects)
        {
            var set = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var item in objects)
            {
                if (!IsImmutable(item.GetType())) set.Add(item);
            }

            return set;
        }

        /// <summary>Las referencias de D2 de un estado, las no nulas: el rack resuelto y el informe de modulos de cada lado.</summary>
        internal static IEnumerable<object> SharedRoots(PushBackEditableState state)
        {
            foreach (var side in new[] { state.Editor.SideA, state.Editor.SideB })
            {
                if (side.WorkingBaseline != null) yield return side.WorkingBaseline;
                if (side.LastModuleReconciliation != null) yield return side.LastModuleReconciliation;
            }
        }

        /// <summary>Los objetos mutables comunes a dos estados, quitadas las referencias de D2 y lo que alcanzan.</summary>
        internal static int CompartidosFueraDeD2(PushBackEditableState a, PushBackEditableState b)
        {
            var shared = Mutable(AllObjects(a).Intersect(AllObjects(b), ReferenceEqualityComparer.Instance));
            var declared = Mutable(SharedRoots(a).Concat(SharedRoots(b)).SelectMany(root => AllObjects(root)));
            return shared.Except(declared, ReferenceEqualityComparer.Instance).Count();
        }

        /// <summary>La huella profunda de un grafo de objetos (la de CT-PBT1B-16, copiada).</summary>
        internal static string Huella(object root)
        {
            var index = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            var order = new List<object>();
            int Idx(object value)
            {
                if (!index.TryGetValue(value, out var position))
                {
                    position = order.Count;
                    index[value] = position;
                    order.Add(value);
                }

                return position;
            }

            string Value(object value)
            {
                if (value == null) return "null";
                if (value is string text) return "\"" + text + "\"";
                if (value is double number) return number.ToString("R", CultureInfo.InvariantCulture);
                if (value is float single) return single.ToString("R", CultureInfo.InvariantCulture);
                if (value is Enum || value.GetType().IsPrimitive) return Convert.ToString(value, CultureInfo.InvariantCulture);
                if (value.GetType().IsValueType)
                {
                    return "{" + string.Join(",", value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(field => field.Name + "=" + Value(field.GetValue(value)))) + "}";
                }

                return "#" + Idx(value);
            }

            var builder = new StringBuilder();
            Idx(root);
            for (var position = 0; position < order.Count; position++)
            {
                var current = order[position];
                var type = current.GetType();
                builder.Append('#').Append(position).Append(' ').Append(type.FullName).Append('\n');
                if (current is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary) builder.Append("  [").Append(Value(entry.Key)).Append("]=").Append(Value(entry.Value)).Append('\n');
                }
                else if (current is IEnumerable items && !(current is string))
                {
                    foreach (var item in items) builder.Append("  ").Append(Value(item)).Append('\n');
                }

                if (!IsProductType(type)) continue;
                foreach (var field in InstanceFields(type))
                {
                    builder.Append("  ").Append(field.Name).Append('=').Append(Value(field.GetValue(current))).Append('\n');
                }
            }

            using (var sha = SHA256.Create())
            {
                var hash = string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
                return hash + (root is PushBackSystem rack ? " Name=[" + rack.Name + "]" : string.Empty);
            }
        }
    }
}
