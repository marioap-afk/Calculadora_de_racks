using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2a — the port through which the full Push Back editor, in Workspace mode, returns its edited state to the
    /// draft (CT-PBT2A-01 to 03): what the port hands the draft, what the typed launch carries and refuses, and the round trip
    /// of the whole corpus without losses and without sharing any mutable object outside the two declared references of D2.
    /// Nothing here opens a window or writes to disk.
    /// </summary>
    public sealed class PushBackDraftStatePortTests
    {
        private const string RackId = "GUID-PBT2-0001";

        // ------------------------------------------------------------------------------------------------ helpers

        private static RackCatalog Catalogo => PilotKit.Catalog;

        private static readonly PushBackEditorDesignAssembler Assembler = new PushBackEditorDesignAssembler(PilotKit.Catalog);
        private static readonly PushBackCompositeEditorAssembler CompositeAssembler = new PushBackCompositeEditorAssembler(PilotKit.Catalog);

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines contain only the ones the contract fixes.</summary>
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

        private static PushBackEditableState Copy(PushBackEditableState state) => PushBackEditableStateComparer.Copy(state);

        private static bool Igual(PushBackEditableState a, PushBackEditableState b) => PushBackEditableStateComparer.Instance.Equals(a, b);

        private static string Fixture(string name) => PilotKit.Text(name);

        private static PushBackDesign DesignOfText(string envelope)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(envelope).Design).PushBackDesign;

        private static string NameOfText(string envelope) => new RackEmbedStore().Deserialize(envelope).Name;

        private static string Persist(PushBackDesign design) => PilotKit.Persist(design);

        /// <summary>A recompute pass as the window runs it (W:659-704), adapted to the state.</summary>
        private static PushBackEditorComputation Pasada(PushBackEditableState s, PushBackSide frontal)
        {
            PushBackEditorComputation computation;
            try { computation = Assembler.BuildFrom(CompositeAssembler.BuildDesign(s.Editor, s.BuildInputs(Assembler)), frontal); }
            catch (Exception) { return null; }
            if (!computation.IsValid) return computation;
            Assembler.AcceptComputation(s.Editor.SideA, computation);
            s.Editor.CommitDormantTailConsumption();
            var standardRestoreLanded = s.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;
            s.Editor.SideA.ClearModuleCommit();
            if (standardRestoreLanded) s.Editor.Active.RestoreAdvancedRackParameters(s.Advanced);
            _ = s.Editor.SideA.ModuleSession;
            return computation;
        }

        /// <summary>The state of a design: a new load, its default safety, the design itself, one pass after each.</summary>
        private static PushBackEditableState Estado(List<string> log, PushBackDesign design, string name)
        {
            var s = new PushBackEditableState();
            Paso(log, () => s.LoadNew(Catalogo));
            Pasada(s, PushBackSide.A);
            Paso(log, () => s.SeedDefaultSafety(Catalogo));
            Pasada(s, PushBackSide.A);
            Paso(log, () => s.LoadDesign(design, name, Assembler.Resolver));
            Pasada(s, PushBackSide.A);
            return s;
        }

        private static PushBackEditableState Estado(List<string> log, string envelope)
            => Estado(log, DesignOfText(envelope), NameOfText(envelope));

        /// <summary>X-01, Core recipe: pb-21-cola, side B declared, side B selected and its slot 0 present.</summary>
        private static PushBackDesign X01Design(List<string> log)
        {
            var text = Fixture("pb-21-cola");
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), Assembler.Resolver));
            Pasada(state, PushBackSide.A);
            Paso(log, () => state.Editor.SetSideBPresent(true));
            Paso(log, () => state.Editor.SetActiveSelection(PushBackSideSelection.B));
            Paso(log, () => state.Editor.SetSlotPresent(PushBackSide.B, 0, true));
            var computation = Pasada(state, PushBackSide.A);
            return computation == null ? null : new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign;
        }

        /// <summary>F-01, Core recipe: pb-20-ab with the first separator of A and then the first one of B lengthened by 2.</summary>
        private static PushBackDesign F01Design(List<string> log)
        {
            var text = Fixture("pb-20-ab");
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), Assembler.Resolver));
            Pasada(state, PushBackSide.A);
            PushBackEditorComputation computation = null;
            foreach (var inB in new[] { false, true })
            {
                var separator = Paso(log, () => state.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == inB));
                if (separator == null) return null;
                Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 2.0));
                Paso(log, () => state.Editor.SideA.CommitModuleEdits());
                computation = Pasada(state, PushBackSide.A);
            }

            return computation == null ? null : new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign;
        }

        /// <summary>The draft of a state, as the host begins it: a registry, a session, a stale base and the state.</summary>
        private static (DraftRegistry Registry, DraftSession<PushBackEditableState> Draft) Borrador(PushBackEditableState s)
        {
            var registry = new DraftRegistry();
            var begin = registry.Begin(
                new SessionId(64),
                RackId,
                DraftIntentRead<PushBackEditableState>.Admitted(
                    DraftStaleBase.Create(new[] { new DraftStaleBaseEntry("BLK-PBT2", DraftMembership.Mutable, "planta", null, "{}") }), s),
                PushBackEditableStateComparer.Instance,
                PushBackEditableStateComparer.Copy);
            return (registry, begin.Draft);
        }

        private static string DisenoDe(PushBackEditableState estado, PushBackSide frontal)
        {
            var computation = Pasada(Copy(estado), frontal);
            return computation == null || !computation.IsValid ? "<sin diseno>" : Persist(computation.Design);
        }

        private static IEnumerable<string> FixtureNames()
        {
            var dir = System.IO.Path.Combine(PilotKit.RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64");
            return System.IO.Directory.GetFiles(dir, "pb-*.json")
                .Select(System.IO.Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
        }

        // ------------------------------------------------------------------------------ graph walks and fingerprints

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

        /// <summary>A type is immutable when it is a string or every instance field of it is init-only and immutable.</summary>
        private static bool IsImmutable(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum) return true;
            if (!IsProductType(type)) return false;
            return InstanceFields(type).All(field => field.IsInitOnly && (field.FieldType.IsValueType || IsImmutable(field.FieldType)));
        }

        /// <summary>Every reference-type object a graph holds (collections included), by walking ALL fields, and also the keys
        /// and the values of every dictionary: its entries are value types and the dictionary is not a product type.</summary>
        private static HashSet<object> AllObjects(object root, Func<FieldInfo, bool> skip = null)
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

        /// <summary>The non-null references of D2 of a state: the resolved rack and the module report of each side.</summary>
        private static IEnumerable<object> SharedRoots(PushBackEditableState state)
        {
            foreach (var side in new[] { state.Editor.SideA, state.Editor.SideB })
            {
                if (side.WorkingBaseline != null) yield return side.WorkingBaseline;
                if (side.LastModuleReconciliation != null) yield return side.LastModuleReconciliation;
            }
        }

        /// <summary>The deep fingerprint of an object graph: every object with its type and each instance field (public,
        /// private and backing) with its value; numbers with «R» and the invariant culture, strings as they are,
        /// references by their index in the walk, lists and dictionaries in order.</summary>
        private static string Huella(object root)
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

        // ------------------------------------------------------------------------------------------------- CT-PBT2A-01

        [Fact]
        public void CT_PBT2A_01_ThePortHandsACopyOfTheEditedStateToTheDraftAndNothingElse()
        {
            var log = new List<string>();
            var s = Estado(log, Fixture("pb-01-base"));
            var (_, draft) = Borrador(s);
            var antes = Copy(draft.CurrentDraftState);
            var port = Paso(log, () => new PushBackDraftSessionPort(draft));
            log.Add("inicio sucio=" + draft.IsDirty);

            // The edited state is applied: the draft holds a copy of it, never the object itself, and its initial state is intact.
            var e = Copy(draft.CurrentDraftState);
            e.PalletTolerance = 4.5;
            var aplicado = Paso(log, () => port.ApplyToDraft(e));
            log.Add("aplicar estado=" + aplicado.Status + " sucio=" + aplicado.IsDirty
                + " igual-al-editado=" + Igual(draft.CurrentDraftState, e)
                + " mismo-objeto=" + ReferenceEquals(draft.CurrentDraftState, e)
                + " inicial-intacto=" + Igual(draft.InitialDraftState, antes));

            // Whatever the editor does to its state afterwards does not change the draft.
            var tras = Copy(draft.CurrentDraftState);
            e.PalletTolerance = 9.0;
            log.Add("mutar-tras-aplicar sin-cambio=" + Igual(draft.CurrentDraftState, tras));

            // Applying the initial state again leaves the draft clean.
            var vuelta = Paso(log, () => port.ApplyToDraft(Copy(antes)));
            log.Add("volver estado=" + vuelta.Status + " sucio=" + vuelta.IsDirty);

            // A draft in error does not accept the state and does not change.
            draft.EnterError("prueba");
            var enError = Copy(draft.CurrentDraftState);
            var rechazado = Copy(enError);
            rechazado.PalletTolerance = 7.5;
            var error = Paso(log, () => port.ApplyToDraft(rechazado));
            log.Add("error estado=" + error.Status + " sin-cambio=" + Igual(draft.CurrentDraftState, enError));

            // A discarded draft does not accept it either.
            var (otroRegistro, otro) = Borrador(Estado(log, Fixture("pb-01-base")));
            otroRegistro.Discard(new SessionId(64), RackId);
            var otroPuerto = Paso(log, () => new PushBackDraftSessionPort(otro));
            var cambiado = Copy(antes);
            cambiado.PalletTolerance = 6.5;
            var descartado = Paso(log, () => otroPuerto.ApplyToDraft(cambiado));
            log.Add("descartado estado=" + descartado.Status);

            // The two refusals of the contract.
            Paso(log, () => new PushBackDraftSessionPort(null));
            Paso(log, () => port.ApplyToDraft(null));

            Assert.Equal(
                new[]
                {
                    "inicio sucio=False",
                    "aplicar estado=Applied sucio=True igual-al-editado=True mismo-objeto=False inicial-intacto=True",
                    "mutar-tras-aplicar sin-cambio=True",
                    "volver estado=Applied sucio=False",
                    "error estado=NotEditable sin-cambio=True",
                    "descartado estado=NotEditable",
                    "EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'draft')",
                    "EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'edited')",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-PBT2A-02

        [Fact]
        public void CT_PBT2A_02_TheLaunchCarriesTheStateTheIdentityTheSourceAndThePortAndRefusesWhatIsMissing()
        {
            var log = new List<string>();
            var s = Estado(log, Fixture("pb-01-base"));
            var (_, draft) = Borrador(s);
            var puerto = Paso(log, () => new PushBackDraftSessionPort(draft));
            var p = RackProject.ForPushBack(DesignOfText(Fixture("pb-01-base")));

            // The launch carries what it is given, as it is given.
            var lanzamiento = Paso(log, () => new PushBackDraftEditorLaunch(s, RackId, p, puerto));
            log.Add("lanzamiento estado-mismo=" + ReferenceEquals(lanzamiento?.State, s) + " id=" + lanzamiento?.RackId
                + " proyecto-mismo=" + ReferenceEquals(lanzamiento?.SourceProject, p) + " puerto-mismo=" + ReferenceEquals(lanzamiento?.Port, puerto));

            // The source project may be missing.
            var sinProyecto = Paso(log, () => new PushBackDraftEditorLaunch(s, RackId, null, puerto));
            log.Add("proyecto=" + (sinProyecto == null || sinProyecto.SourceProject == null ? "<null>" : "presente"));

            // What is missing, in the order of the checks.
            Paso(log, () => new PushBackDraftEditorLaunch(null, RackId, p, puerto));
            foreach (var vacio in new string[] { null, string.Empty, "   " })
            {
                Paso(log, () => new PushBackDraftEditorLaunch(s, vacio, p, puerto));
            }

            Paso(log, () => new PushBackDraftEditorLaunch(s, RackId, p, null));

            // The launch of a draft: its current state, its id and a port that belongs to that draft.
            var desdeBorrador = Paso(log, () => PushBackDraftSessionPort.LaunchFor(draft, p));
            var lectura = Paso(log, () =>
            {
                // The state and the id are read BEFORE the edit: applying an edit leaves the draft holding a new copy.
                var estadoEsElActual = ReferenceEquals(desdeBorrador.State, draft.CurrentDraftState).ToString();
                var id = desdeBorrador.RackId;
                var edited = Copy(desdeBorrador.State);
                edited.PalletTolerance = 4.5;
                desdeBorrador.Port.ApplyToDraft(edited);
                return "launchfor estado-es-el-actual=" + estadoEsElActual
                    + " id=" + id
                    + " puerto-es-del-borrador=" + (Igual(draft.CurrentDraftState, edited) && draft.IsDirty);
            });
            if (lectura != null) log.Add(lectura);

            Paso(log, () => PushBackDraftSessionPort.LaunchFor(null, p));

            Assert.Equal(
                new[]
                {
                    "lanzamiento estado-mismo=True id=GUID-PBT2-0001 proyecto-mismo=True puerto-mismo=True",
                    "proyecto=<null>",
                    "EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'state')",
                    "EXCEPCION ArgumentException: A rack id is required. (Parameter 'rackId')",
                    "EXCEPCION ArgumentException: A rack id is required. (Parameter 'rackId')",
                    "EXCEPCION ArgumentException: A rack id is required. (Parameter 'rackId')",
                    "EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'port')",
                    "launchfor estado-es-el-actual=True id=GUID-PBT2-0001 puerto-es-del-borrador=True",
                    "EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'draft')",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-PBT2A-03

        [Fact]
        public void CT_PBT2A_03_ThePortRoundTripIsLosslessAndSharesNothingOutsideTheTwoDeclaredReferences()
        {
            var log = new List<string>();
            var expected = new List<string>();

            // The corpus C18: the 17 of Fixtures/I64 without pb-21-cola, plus X-01 and F-01, in the order of CT-PBT1B-02.
            var estados = new List<(string Name, PushBackEditableState State)>();
            foreach (var name in FixtureNames().Where(n => n != "pb-21-cola"))
            {
                estados.Add((name, Estado(log, Fixture(name))));
            }

            var x01 = X01Design(log);
            var f01 = F01Design(log);
            estados.Add(("X-01", x01 == null ? new PushBackEditableState() : Estado(log, x01, "X-01")));
            estados.Add(("F-01", f01 == null ? new PushBackEditableState() : Estado(log, f01, "F-01")));

            foreach (var entry in estados)
            {
                var s = entry.State;
                var (_, d) = Borrador(s);

                // The draft starts from a different state, put by a direct call to the core.
                var otro = Copy(s);
                otro.PalletTolerance = s.PalletTolerance + 1.0;
                d.SetCurrentDraftState(otro);

                // The deep fingerprints of the references of D2 of the state and of the draft, before the call.
                var vigilados = new List<(object Object, string Before)>();
                foreach (var reference in SharedRoots(s).Concat(SharedRoots(d.CurrentDraftState)))
                {
                    if (vigilados.Any(item => ReferenceEquals(item.Object, reference))) continue;
                    vigilados.Add((reference, Huella(reference)));
                }

                var puerto = Paso(log, () => new PushBackDraftSessionPort(d));
                Paso(log, () => puerto.ApplyToDraft(Copy(s)));

                var shared = Mutable(AllObjects(d.CurrentDraftState).Intersect(AllObjects(s), ReferenceEqualityComparer.Instance));
                var declared = Mutable(SharedRoots(s).Concat(SharedRoots(d.CurrentDraftState)).SelectMany(root => AllObjects(root)));
                var fuera = shared.Except(declared, ReferenceEqualityComparer.Instance).Count();
                var huellasIguales = vigilados.All(item => Huella(item.Object) == item.Before);

                log.Add(entry.Name + " iguales=" + Igual(d.CurrentDraftState, s)
                    + " diseno-igual=" + (DisenoDe(d.CurrentDraftState, PushBackSide.A) == DisenoDe(s, PushBackSide.A))
                    + " compartidos-fuera-de-D2=" + fuera
                    + " huellas-iguales=" + huellasIguales);
                expected.Add(entry.Name + " iguales=True diseno-igual=True compartidos-fuera-de-D2=0 huellas-iguales=True");
            }

            Assert.Equal(expected, log);
        }
    }
}
