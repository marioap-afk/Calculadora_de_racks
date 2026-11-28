using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackStateNetSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1c — CT-PBT1C-01 and CT-PBT1C-02: the classic Push Back window keeps its editable state in the
    /// <c>PushBackEditableState</c> of PB-T1b, in CLASSIC mode and without an observable change (decision D-PBT1-13 of the package
    /// of PB-T1). CT-PBT1C-01: the window edits the state it exposes and, along the recompute passes of §5.0, that state is equal,
    /// with the comparer of PB-T1b, to the one of a state alone that followed the same sequence (a new window, <c>LoadExisting</c>,
    /// <c>LoadDesignForNew</c> and «Restaurar»), over the 17 designs of <c>Fixtures/I64</c>, X-01 and F-01. CT-PBT1C-02: no control
    /// is the source of truth (the design computed from a COPY of the state of the window is equal, persisted, to the one the
    /// window computed, and the name of the state is the rack name) and, with the conditions 3, 4, 5 and 8 of decision D2
    /// of the Owner (F4B-E11), no gesture mutates the resolved rack or the module report the state adopts (their deep
    /// fingerprints do not change).
    ///
    /// <para>Each method builds a list of lines and ends in ONE <c>Assert.Equal</c>. Every window is created with
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, its load goes inside the <c>try</c> and it is closed in
    /// the <c>finally</c>; none is shown, the keyboard and the real focus are not used, and the whole method runs inside
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> with a counter of discards that the last line of
    /// each window carries, read after closing it. Every step that calls the new API goes through <c>Paso</c>, which turns an
    /// exception into an «EXCEPCION» line; the expected lines contain none. No design is added to <c>Fixtures/I64</c>: X-01 and
    /// F-01 are built in code.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackWindowOnStateTests
    {
        // ------------------------------------------------------------------------------------------------ the «EXCEPCION» rule

        /// <summary>A step that throws becomes a line, and the expected lines never contain one.</summary>
        private static T Paso<T>(List<string> log, Func<T> step)
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

        private static void Paso(List<string> log, Action step) => Paso<int>(log, () => { step(); return 0; });

        // ------------------------------------------------------------------------------------------------ the state alone (CT-01)

        /// <summary>A design, opened as a FRESH deserialization every time (the window and the state never share objects).</summary>
        private sealed class Origen
        {
            public string Nombre;
            public Func<(PushBackDesign Design, string Id, string Name, RackProject Project)> Abrir;
        }

        private static Origen DeSobre(string nombre, string sobre) => new Origen
        {
            Nombre = nombre,
            Abrir = () =>
            {
                var embed = new RackEmbedStore().Deserialize(sobre);
                var project = new RackProjectStore().Deserialize(embed.Design);
                return (project.PushBackDesign, embed.Id, embed.Name, project);
            },
        };

        private static Origen DePersistido(string nombre, string persistido, string id, string name) => new Origen
        {
            Nombre = nombre,
            Abrir = () => (new RackProjectStore().Deserialize(persistido).PushBackDesign, id, name, null),
        };

        private sealed class Estado
        {
            public Estado(RackCatalog catalog)
            {
                Catalog = catalog;
                S = new PushBackEditableState();
                A = new PushBackEditorDesignAssembler(catalog);
                CA = new PushBackCompositeEditorAssembler(catalog);
            }

            public RackCatalog Catalog;
            public PushBackEditableState S;
            public PushBackEditorDesignAssembler A;
            public PushBackCompositeEditorAssembler CA;

            /// <summary>The last VALID computation, as the window keeps its <c>lastComputation</c>.</summary>
            public PushBackEditorComputation Last;
        }

        /// <summary>A recompute pass as the window runs it (W:659-704), adapted to the state (§5.0).</summary>
        private static PushBackEditorComputation Pasada(Estado e)
        {
            PushBackEditorComputation computation;
            try { computation = e.A.BuildFrom(e.CA.BuildDesign(e.S.Editor, e.S.BuildInputs(e.A)), PushBackSide.A); }
            catch (Exception) { return null; }
            if (!computation.IsValid) return computation;
            e.A.AcceptComputation(e.S.Editor.SideA, computation);
            e.S.Editor.CommitDormantTailConsumption();
            var standardRestoreLanded = e.S.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;
            e.S.Editor.SideA.ClearModuleCommit();
            if (standardRestoreLanded) e.S.Editor.Active.RestoreAdvancedRackParameters(e.S.Advanced);
            _ = e.S.Editor.SideA.ModuleSession;
            e.Last = computation;
            return computation;
        }

        /// <summary>A new window, in the state: <c>LoadNew</c>, a pass, the default safety and a pass (W:280-288).</summary>
        private static void Nueva(List<string> log, Estado e)
        {
            Paso(log, () => e.S.LoadNew(e.Catalog));
            Pasada(e);
            Paso(log, () => e.S.SeedDefaultSafety(e.Catalog));
            Pasada(e);
        }

        /// <summary>The window of today plus a design: the new window and then the load of the design, with a pass (W:439).</summary>
        private static void Cargada(List<string> log, Estado e, Origen origen)
        {
            Nueva(log, e);
            var abierto = origen.Abrir();
            Paso(log, () => e.S.LoadDesign(abierto.Design, abierto.Name, e.A.Resolver));
            Pasada(e);
        }

        private static void Restaurada(List<string> log, Estado e)
        {
            var ultimo = e.Last?.Design;
            Paso(log, () => e.S.Restore(ultimo, e.A.Resolver));
            Pasada(e);
        }

        /// <summary>The state of the window and the state alone, equal for the comparer of PB-T1b (a comparer that throws is never «equal»).</summary>
        private static bool Iguales(List<string> log, RackPushBackSystemWindow w, Estado e)
            => Paso(log, () => PushBackEditableStateComparer.Instance.Equals(w.EditableStateForTest, e.S));

        /// <summary>Closes the window and ends the line with the counter of discards, read after closing it.</summary>
        private static string Fin(RackPushBackSystemWindow w, string linea, Func<int> descartes)
        {
            Cerrar(w);
            return linea + " descartes=" + descartes();
        }

        private static string LineaCamino(string diseno, string camino, bool iguales) => diseno + " " + camino + " iguales=" + iguales;

        /// <summary>One path over one design: a window of its own, the state alone with the same sequence, and the line.</summary>
        private static string Camino(List<string> log, Origen origen, string camino, Func<int> descartes)
        {
            var w = Ventana();
            try
            {
                var e = new Estado(w.Session.Catalog);
                var abierto = origen.Abrir();
                Cargada(log, e, origen);
                switch (camino)
                {
                    case "LoadExisting":
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        break;
                    case "LoadDesignForNew":
                        w.LoadDesignForNew(abierto.Design, abierto.Name, abierto.Project);
                        break;
                    case "Restaurar":
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        Click(w, "RestoreButton");
                        Restaurada(log, e);
                        break;
                }

                return Fin(w, LineaCamino(origen.Nombre, camino, Iguales(log, w, e)), descartes);
            }
            finally { Cerrar(w); }
        }

        /// <summary>X-01, window recipe (§5.0): the design of the pass, with its id and name.</summary>
        private static Origen X01()
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
                EditorWindowTestSupport.SetFrontBlank(w, PushBackStateNetSupport.Grid, 0, false);
                persistido = Persist(w.LastComputation.Design);
                id = w.RackId;
                nombre = w.RackName;
            }
            finally { Cerrar(w); }

            return DePersistido("X-01", persistido, id, nombre);
        }

        /// <summary>F-01, window recipe (§5.0): the first separator of A and then the first one of B, lengthened by 2.</summary>
        private static Origen F01()
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

            return DePersistido("F-01", persistido, id, nombre);
        }

        [Fact]
        public void CT_PBT1C_01_TheWindowEditsTheExtractedStateAndLoadsItLikeTheStateAlone()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // The window edits the state it exposes: the composite editor of the window IS the editor of that state. A new
                    // window is then equal to the state alone that ran the sequence of a new window.
                    var nueva = Ventana();
                    try
                    {
                        var e = new Estado(nueva.Session.Catalog);
                        Nueva(log, e);
                        log.Add("referencia-del-editor-igual=" + Paso(log, () => ReferenceEquals(nueva.EditableStateForTest.Editor, nueva.CompositeState)));
                        log.Add(Fin(nueva, LineaCamino("(sin-diseno)", "nueva", Iguales(log, nueva, e)), () => descartes));
                    }
                    finally { Cerrar(nueva); }

                    // The 17 designs of Fixtures/I64, X-01 and F-01, by every path.
                    var origenes = CorpusNames().Select(nombre => DeSobre(nombre, Fixture(nombre))).ToList();
                    origenes.Add(X01());
                    origenes.Add(F01());
                    foreach (var origen in origenes)
                    {
                        foreach (var camino in new[] { "LoadExisting", "LoadDesignForNew", "Restaurar" })
                        {
                            log.Add(Camino(log, origen, camino, () => descartes));
                        }
                    }
                });
            }

            var esperadas = new List<string>
            {
                "referencia-del-editor-igual=True",
                "(sin-diseno) nueva iguales=True descartes=0",
            };
            var nombres = CorpusNames().ToList();
            nombres.Add("X-01");
            nombres.Add("F-01");
            foreach (var nombre in nombres)
            {
                foreach (var camino in new[] { "LoadExisting", "LoadDesignForNew", "Restaurar" })
                {
                    esperadas.Add(nombre + " " + camino + " iguales=True descartes=0");
                }
            }

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ the deep fingerprint (CT-02, part 2)

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

        /// <summary>The deep fingerprint of an object graph (the one of CT-PBT1B-16, copied): every object with its type and each
        /// instance field (public, private and backing) with its value; numbers with «R» and the invariant culture, strings as they
        /// are, references by their index in the walk, lists and dictionaries in order.</summary>
        private static string Fingerprint(object root)
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

        /// <summary>The resolved rack and the module report of each side: the two shared references of decision D2, the non-null ones.</summary>
        private static IEnumerable<object> SharedRoots(PushBackEditableState state)
        {
            foreach (var side in new[] { state.Editor.SideA, state.Editor.SideB })
            {
                if (side.WorkingBaseline != null) yield return side.WorkingBaseline;
                if (side.LastModuleReconciliation != null) yield return side.LastModuleReconciliation;
            }
        }

        /// <summary>The two shared references of the state of ONE window, each measured from the moment it is adopted.</summary>
        private sealed class Vigia
        {
            private readonly List<(object Object, string Before)> items = new List<(object, string)>();

            public int Cuenta => items.Count;

            public void Capturar(List<string> log, RackPushBackSystemWindow w)
            {
                var estado = Paso(log, () => w.EditableStateForTest);
                if (estado == null) return;
                foreach (var reference in SharedRoots(estado))
                {
                    if (items.Any(item => ReferenceEquals(item.Object, reference))) continue;
                    items.Add((reference, Fingerprint(reference)));
                }
            }

            public bool Intacta() => items.All(item => Fingerprint(item.Object) == item.Before);
        }

        // ------------------------------------------------------------------------------------------------ the gestures (CT-02)

        /// <summary>A gesture of the census of §5.5: its name, the design it runs over and its steps; each step is a gesture of the user.</summary>
        private sealed class Gesto
        {
            public string Nombre;
            public string Diseno;

            /// <summary>False for the gestures that leave the last VALID design behind the controls (an error, an invalid calculation).</summary>
            public bool Compara = true;

            public Action<RackPushBackSystemWindow>[] Pasos;
        }

        private const string Base = "pb-01-base";
        private const string Compuesto = "pb-20-ab";

        private static Gesto G(string nombre, string diseno, params Action<RackPushBackSystemWindow>[] pasos)
            => new Gesto { Nombre = nombre, Diseno = diseno, Pasos = pasos };

        private static Gesto SinComparar(string nombre, string diseno, params Action<RackPushBackSystemWindow>[] pasos)
            => new Gesto { Nombre = nombre, Diseno = diseno, Compara = false, Pasos = pasos };

        /// <summary>A click of the user on a box that listens to <c>Click</c>: it inverts the box and raises the real click.</summary>
        private static void Pulsar(CheckBox casilla)
        {
            casilla.IsChecked = !(casilla.IsChecked == true);
            casilla.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, casilla));
        }

        /// <summary>The other option of a list: the first one, or the second when the first is chosen.</summary>
        private static void Otro(ComboBox caja) => caja.SelectedIndex = caja.Items.Count > 1 && caja.SelectedIndex == 0 ? 1 : 0;

        private static void Alternar(CheckBox casilla) => casilla.IsChecked = !(casilla.IsChecked == true);

        private static void Escribir(RackPushBackSystemWindow w, string caja, double valor) => SetNumberR(w, caja, valor);

        /// <summary>A recompute without editing a field: a <c>LostFocus</c> on a box that is not edited (Input_Changed).</summary>
        private static void Recalcular(RackPushBackSystemWindow w)
            => Field(w, "AnnotationScaleBox").RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, Field(w, "AnnotationScaleBox")));

        /// <summary>The census of §5.5, one gesture per control, plus the gestures of D2 (a field in error, an invalid calculation, a box
        /// written without leaving it, «Restaurar» and closing the window without asking).</summary>
        private static IEnumerable<Gesto> Gestos()
        {
            // The rack-wide boxes.
            yield return G("DepthBox=54", Base, w => Escribir(w, "DepthBox", 54));
            yield return G("ToleranceBox=4", Base, w => Escribir(w, "ToleranceBox", 4));
            yield return G("PostPeralteBox=+0.5", Base, w => Escribir(w, "PostPeralteBox", ValueOf(w, "PostPeralteBox").Value + 0.5));
            yield return G("BeamDepthBox=+1", Base, w => Escribir(w, "BeamDepthBox", ValueOf(w, "BeamDepthBox").Value + 1.0));
            yield return G("AnnotationScaleBox=2", Base, w => Escribir(w, "AnnotationScaleBox", 2));
            yield return G("WeightUnitBox=lb", Base, w => Combo(w, "WeightUnitBox").SelectedItem = "lb");
            yield return G("PalletsDeepBox=9", Base, w => Escribir(w, "PalletsDeepBox", 9));
            yield return G("PostBox=otro", Base, w => Otro(Combo(w, "PostBox")));
            yield return G("NumberFrontsCheck", Base, w => Pulsar(Check(w, "NumberFrontsCheck")));
            yield return G("NumberLevelsCheck", Base, w => Pulsar(Check(w, "NumberLevelsCheck")));
            yield return G("DrawRackNameCheck", Base, w => Pulsar(Check(w, "DrawRackNameCheck")));
            yield return G("DimensionsBox=3", Base, w => Combo(w, "DimensionsBox").SelectedIndex = 3);
            yield return G("DimensionsFrontalCheck", Base, w => Alternar(Check(w, "DimensionsFrontalCheck")));
            yield return G("DimensionsLateralCheck", Base, w => Alternar(Check(w, "DimensionsLateralCheck")));
            yield return G("DimensionsPlantaCheck", Base, w => Alternar(Check(w, "DimensionsPlantaCheck")));
            yield return G("NameBox", Base, w => EditorWindowTestSupport.SetText(w, "NameBox", "Rack de prueba"));

            // The boxes of the front and of the cell.
            yield return G("FondosBox=4", "pb-03-fondos", w => w.SelectMatrixCell(1, 0, false), w => Escribir(w, "FondosBox", 4));
            yield return G("DepthStartBox=2", "pb-03-fondos", w => Escribir(w, "DepthStartBox", 2));
            yield return G("PositionsBox=2", "pb-03-fondos", w => w.SelectMatrixCell(1, 0, false), w => Escribir(w, "PositionsBox", 2));
            yield return G("LevelsBox=2", "pb-03-fondos", w => w.SelectMatrixCell(1, 0, false), w => Escribir(w, "LevelsBox", 2));
            yield return G("FirstLevelHeightBox=5", Base, w => Escribir(w, "FirstLevelHeightBox", 5));
            yield return G("CellPalletFrontBox=40", Base, w => Escribir(w, "CellPalletFrontBox", 40));
            yield return G("CellPalletHeightBox=61.5", Base, w => Escribir(w, "CellPalletHeightBox", 61.5));
            yield return G("CellPalletWeightBox=900", Base, w => Escribir(w, "CellPalletWeightBox", 900));
            yield return G("CellClearBox=7", Base, w => Escribir(w, "CellClearBox", 7));
            yield return G("CellInOutBeamBox=otro", Base, w => Otro(Combo(w, "CellInOutBeamBox")));
            yield return G("CellInOutPeralteBox=otro", Base, w => Otro(Combo(w, "CellInOutPeralteBox")));
            yield return G("CellIntermediateBeamBox=otro", Base, w => Otro(Combo(w, "CellIntermediateBeamBox")));
            yield return G("CellIntermediatePeralteBox=otro", Base, w => Otro(Combo(w, "CellIntermediatePeralteBox")));
            yield return G("CellFondoOverrideBox=6", Base, w => Escribir(w, "CellFondoOverrideBox", 6));
            yield return G("CellDrawPalletCheck", Base, w => Check(w, "CellDrawPalletCheck").IsChecked = true);
            yield return G("CellBeamLengthOverrideBox=100", Base, w => Escribir(w, "CellBeamLengthOverrideBox", 100));
            yield return G("RearPeralteBox=4.5", Base, w => Combo(w, "RearPeralteBox").SelectedItem = 4.5);

            // The composite part.
            yield return G("GapBox=60", Compuesto, w => Escribir(w, "GapBox", 60));
            yield return G("CentralSeparatorCheck", Compuesto, w => Check(w, "CentralSeparatorCheck").IsChecked = true);
            yield return G("StructureOverrideBox=6", Compuesto, w => Escribir(w, "StructureOverrideBox", 6), w => Click(w, "ApplyStructureButton"));
            yield return G(
                "CellTopologyBox+RunDirectionBox+TopologyScopeBox", Compuesto,
                w => Combo(w, "CellTopologyBox").SelectedIndex = 3,
                w => Combo(w, "RunDirectionBox").SelectedIndex = 1,
                w => Combo(w, "TopologyScopeBox").SelectedIndex = 4,
                w => Click(w, "ApplyTopologyButton"));
            yield return G("FrontCountBox=3", Compuesto, w => Escribir(w, "FrontCountBox", 3));
            yield return G("SideBPresentCheck", Base, w => Check(w, "SideBPresentCheck").IsChecked = true);
            yield return G("SideSelectorBox=1", Compuesto, w => Combo(w, "SideSelectorBox").SelectedIndex = 1);
            yield return G("FrontalSideBox=1", Compuesto, w => Combo(w, "FrontalSideBox").SelectedIndex = 1);

            // The advanced boxes and the modules.
            yield return G("RackHeaderHeightBox=155", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "RackHeaderHeightBox", 155));
            yield return G("DerivedPostHeightBox=140", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "DerivedPostHeightBox", 140));
            yield return G(
                "DerivedPostReinforcedCheck+DerivedPostReinforcementHeightBox=50", "pb-06-seguridad",
                w => Check(w, "AdvancedModulesToggle").IsChecked = true,
                w => Pulsar(Check(w, "DerivedPostReinforcedCheck")),
                w => Escribir(w, "DerivedPostReinforcementHeightBox", 50));
            yield return G("RackSeparatorCountBox=4", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "RackSeparatorCountBox", 4));
            yield return G("RackSeparatorSpacingBox=20", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "RackSeparatorSpacingBox", 20));
            yield return G(
                "ModuleLengthBox=+8", "pb-06-seguridad",
                w => SelectModule(w, "M2"),
                w => Escribir(w, "ModuleLengthBox", ValueOf(w, "ModuleLengthBox").Value + 8.0),
                w => Click(w, "ConfirmModuleButton"));
            yield return G(
                "DerivedPostLineHeightBox=137", Base,
                w => Check(w, "AdvancedModulesToggle").IsChecked = true,
                w => PushBackHeaderTestSupport.Target(w, Array.Empty<string>(), new[] { PushBackHeaderTestSupport.Lines(w)[1] }),
                w => Escribir(w, "DerivedPostLineHeightBox", 137),
                w => Click(w, "ApplyDerivedPostLinesButton"),
                w => Click(w, "ConfirmModuleButton"));

            // «Restaurar», after a box was edited.
            yield return G("Restaurar", Base, w => Escribir(w, "DepthBox", 54), w => Click(w, "RestoreButton"));

            // D2, conditions 4 and 5: a field in error, an invalid calculation (the reinforcement beyond the post), a box written
            // without leaving it, and the window closed without asking.
            yield return SinComparar(
                "campo-en-error", Base,
                w => { Field(w, "DepthBox").Text = "x"; Field(w, "DepthBox").RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, Field(w, "DepthBox"))); });
            yield return SinComparar(
                "calculo-invalido", Base,
                w => Check(w, "AdvancedModulesToggle").IsChecked = true,
                w => Check(w, "DerivedPostReinforcedCheck").IsChecked = true,
                w => Escribir(w, "DerivedPostReinforcementHeightBox", 100000));
            yield return SinComparar("caja-escrita-sin-salir", Base, w => Field(w, "DepthBox").SetNumber(54));
            yield return SinComparar("cerrar-sin-pedir", Base, w => Cerrar(w));
        }

        /// <summary>The design computed from a COPY of the state of the window (Copy, BuildInputs, BuildDesign and BuildFrom with the frontal
        /// side of the window), persisted, or null when there is none.</summary>
        private static string DisenoDeUnaCopia(List<string> log, RackPushBackSystemWindow w)
        {
            var copia = Paso(log, () => PushBackEditableStateComparer.Copy(w.EditableStateForTest));
            if (copia == null) return null;
            var a = new PushBackEditorDesignAssembler(w.Session.Catalog);
            var ca = new PushBackCompositeEditorAssembler(w.Session.Catalog);
            var computacion = Paso(log, () => a.BuildFrom(ca.BuildDesign(copia.Editor, copia.BuildInputs(a)), w.FrontalSideForTest));
            return computacion != null && computacion.IsValid ? Persist(computacion.Design) : null;
        }

        private static bool DisenoIgual(List<string> log, RackPushBackSystemWindow w)
        {
            var copia = DisenoDeUnaCopia(log, w);
            var ventana = w.LastComputation?.Design == null ? null : Persist(w.LastComputation.Design);
            return copia != null && copia == ventana;
        }

        private static bool NombreIgual(List<string> log, RackPushBackSystemWindow w)
            => string.Equals(Paso(log, () => w.EditableStateForTest.Name), w.RackName, StringComparison.Ordinal);

        /// <summary>One gesture on a window of its own: the lines of the part 1 (the design of a copy and the name) and of the part 2 (the fingerprints).</summary>
        private static void Correr(List<string> log, Gesto gesto, Func<int> descartes)
        {
            var w = Ventana();
            try
            {
                Cargar(w, Fixture(gesto.Diseno));
                var vigia = new Vigia();
                vigia.Capturar(log, w);
                foreach (var paso in gesto.Pasos)
                {
                    paso(w);
                    vigia.Capturar(log, w);
                }

                // A neutral recompute after the gesture, so that what the gesture adopted is compared against a later pass.
                if (!EstaCerrada(w)) Recalcular(w);
                vigia.Capturar(log, w);

                // The line of the part 1 is taken after that recompute, the same for every gesture.
                string parte1 = null;
                if (gesto.Compara)
                {
                    parte1 = gesto.Nombre + " valido=" + Valido(w) + " iguales=" + DisenoIgual(log, w) + " nombre-igual=" + NombreIgual(log, w);
                }

                Cerrar(w);
                if (parte1 != null) log.Add(parte1);
                log.Add(gesto.Nombre + " huellas-iguales=" + (vigia.Cuenta > 0 && vigia.Intacta()) + " descartes=" + descartes());
            }
            finally { Cerrar(w); }
        }

        [Fact]
        public void CT_PBT1C_02_NoControlIsTheSourceOfTruthAndNoGestureMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    foreach (var gesto in Gestos())
                    {
                        Correr(log, gesto, () => descartes);
                    }
                });
            }

            var esperadas = new List<string>();
            foreach (var gesto in Gestos())
            {
                if (gesto.Compara) esperadas.Add(gesto.Nombre + " valido=True iguales=True nombre-igual=True");
                esperadas.Add(gesto.Nombre + " huellas-iguales=True descartes=0");
            }

            Assert.Equal(esperadas, log);
        }
    }
}
