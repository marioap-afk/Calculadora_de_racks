using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1b — CT-PBT1B-01: the PARITY of the loads of the editable state of Push Back with the window of today. The
    /// window is the oracle: for each design and each way of loading it (a new window, <c>LoadExisting</c>,
    /// <c>LoadDesignForNew</c>, «Restaurar» after loading, after retiring side B and after <c>SetGap(60)</c>, and
    /// «Actualizar») the state runs the sequence of the window with the recompute pass of §5.0, and the persisted design of
    /// both must be the same, with the same verdict of the output gate. It includes the cases c-01 to c-16 of CT-PB0-17, the
    /// four blocking cases of CT-12 with their status text, and the two recipes of X-01 and F-01 (window and Core).
    ///
    /// <para>The method builds a list of lines and ends in ONE <c>Assert.Equal</c>. Every window is created with
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, its load goes inside the <c>try</c> and it is closed in
    /// the <c>finally</c>; none is shown, the keyboard and the real focus are not used, and the whole method runs inside
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> with a counter of discards that the last line of
    /// each window carries, read after closing it. Every step that calls the new API goes through <c>Paso</c>, which turns an
    /// exception into an «EXCEPCION» line; the expected lines contain none.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackEditableStateParityTests
    {
        private const string MissingBeam = "PB0_VIGA_INEXISTENTE";

        private static readonly RackCatalog Catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ------------------------------------------------------------------------------------------------ the sources

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

        private static Origen DeDiseno(string nombre, Func<PushBackDesign> diseno) => new Origen
        {
            Nombre = nombre,
            Abrir = () => (diseno(), FixedId, nombre, null),
        };

        // ------------------------------------------------------------------------------------------------ the state side

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

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
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

        /// <summary>The persisted design of the last pass, or null when there is none.</summary>
        private static string Persistido(Estado e, PushBackEditorComputation computation)
            => computation != null && computation.IsValid ? Persist(computation.Design) : null;

        /// <summary>A new window, in the state: <c>LoadNew</c>, a pass, the default safety and a pass (W:280-288).</summary>
        private static string Nueva(List<string> log, Estado e)
        {
            Paso(log, () => e.S.LoadNew(e.Catalog));
            Pasada(e);
            Paso(log, () => e.S.SeedDefaultSafety(e.Catalog));
            return Persistido(e, Pasada(e));
        }

        /// <summary>The window of today plus a design: the new window and then the load of the design, with a pass (W:439).</summary>
        private static string Cargada(List<string> log, Estado e, Origen origen)
        {
            Nueva(log, e);
            var abierto = origen.Abrir();
            Paso(log, () => e.S.LoadDesign(abierto.Design, abierto.Name, e.A.Resolver));
            return Persistido(e, Pasada(e));
        }

        private static string Restaurada(List<string> log, Estado e)
        {
            var ultimo = e.Last?.Design;
            Paso(log, () => e.S.Restore(ultimo, e.A.Resolver));
            return Persistido(e, Pasada(e));
        }

        private static bool BloqueadoEstado(Estado e)
            => e.Last != null && PushBackEditorOutputGate.IsBlocked(e.S.Editor, e.Last.System);

        // ------------------------------------------------------------------------------------------------ the window side

        private static string Ventanada(RackPushBackSystemWindow w)
            => w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid ? Persist(w.LastComputation.Design) : null;

        /// <summary>A recompute without editing a field: a <c>LostFocus</c> on a box that is not edited (Input_Changed).</summary>
        private static void Recalcular(RackPushBackSystemWindow w)
            => Field(w, "AnnotationScaleBox").RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, Field(w, "AnnotationScaleBox")));

        /// <summary>Closes the window and ends the line with the counter of discards, read after closing it.</summary>
        private static string Fin(RackPushBackSystemWindow w, string linea, Func<int> descartes)
        {
            Cerrar(w);
            return linea + " descartes=" + descartes();
        }

        private static string Linea(string diseno, string camino, string ventana, string estado, bool bloqueadoVentana, bool bloqueadoEstado)
            => diseno + " " + camino + " iguales=" + (ventana != null && ventana == estado)
               + " bloqueado-ventana=" + bloqueadoVentana + " bloqueado-estado=" + bloqueadoEstado;

        // ------------------------------------------------------------------------------------------------ the paths

        private static readonly string[] CaminosDeUnLado = { "LoadExisting", "LoadDesignForNew", "Restaurar-tras-cargar" };

        /// <summary>M08: «retirar B» only has meaning where side B is declared; on a one-sided rack it degenerates.</summary>
        private const string CaminoRetirarB = "Restaurar-tras-retirar-B";

        /// <summary>M08: «SetGap(60)» runs on ALL the designs (it does not degenerate on a one-sided rack).</summary>
        private const string CaminoSetGap = "Restaurar-tras-SetGap(60)";

        /// <summary>The designs that open with side B declared (the ones that also run «Restaurar» after retiring B).</summary>
        private static bool TieneLadoB(Origen origen)
        {
            var abierto = origen.Abrir();
            return abierto.Design.SideB != null && abierto.Design.SideB.IsPresent;
        }

        /// <summary>One path over one design: a window of its own, the state with the same sequence, and the line.</summary>
        private static string Camino(List<string> log, Origen origen, string camino, Func<int> descartes)
        {
            var w = Ventana();
            try
            {
                var e = new Estado(w.Session.Catalog);
                var abierto = origen.Abrir();
                var estado = Cargada(log, e, origen);
                switch (camino)
                {
                    case "LoadExisting":
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        break;
                    case "LoadDesignForNew":
                        w.LoadDesignForNew(abierto.Design, abierto.Name, abierto.Project);
                        break;
                    case "Restaurar-tras-cargar":
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        Click(w, "RestoreButton");
                        estado = Restaurada(log, e);
                        break;
                    case CaminoRetirarB:
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        Check(w, "SideBPresentCheck").IsChecked = false;
                        Paso(log, () => e.S.Editor.SetSideBPresent(false));
                        Pasada(e);
                        Click(w, "RestoreButton");
                        estado = Restaurada(log, e);
                        break;
                    case CaminoSetGap:
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        w.CompositeState.SetGap(60);
                        Recalcular(w);
                        Paso(log, () => e.S.Editor.SetGap(60));
                        Pasada(e);
                        Click(w, "RestoreButton");
                        estado = Restaurada(log, e);
                        break;
                    case "Actualizar":
                        w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                        Click(w, "UpdateButton");
                        estado = Persistido(e, Pasada(e));
                        return Fin(w, Linea(origen.Nombre, camino, w.DesignToInsert == null ? null : Persist(w.DesignToInsert), estado, w.OutputIsBlockedForTest, BloqueadoEstado(e)), descartes);
                }

                return Fin(w, Linea(origen.Nombre, camino, Ventanada(w), estado, w.OutputIsBlockedForTest, BloqueadoEstado(e)), descartes);
            }
            finally { Cerrar(w); }
        }

        // ------------------------------------------------------------------------------------------------ the recipes

        /// <summary>X-01, window recipe (§5.0): the design of the pass, with its id and name.</summary>
        private static Origen X01Ventana()
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
                EditorWindowTestSupport.SetFrontBlank(w, "PushBackMatrixGrid", 0, false);
                persistido = Persist(w.LastComputation.Design);
                id = w.RackId;
                nombre = w.RackName;
            }
            finally { Cerrar(w); }

            return DePersistido("X-01", persistido, id, nombre);
        }

        /// <summary>F-01, window recipe (§5.0).</summary>
        private static Origen F01Ventana()
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

        /// <summary>X-01, Core recipe (§5.0): the persisted design of the state after its steps.</summary>
        private static string X01Estado(List<string> log)
        {
            var e = new Estado(Catalog);
            Cargada(log, e, DeSobre("pb-21-cola", Fixture("pb-21-cola")));
            Paso(log, () => e.S.Editor.SetSideBPresent(true));
            Paso(log, () => e.S.Editor.SetActiveSelection(PushBackSideSelection.B));
            Paso(log, () => e.S.Editor.SetSlotPresent(PushBackSide.B, 0, true));
            return Persistido(e, Pasada(e));
        }

        /// <summary>F-01, Core recipe (§5.0): the first separator of A and then the first one of B, lengthened by 2.</summary>
        private static string F01Estado(List<string> log)
        {
            var e = new Estado(Catalog);
            Cargada(log, e, DeSobre("pb-20-ab", Fixture("pb-20-ab")));
            PushBackEditorComputation computation = null;
            foreach (var enB in new[] { false, true })
            {
                var separador = Paso(log, () => e.S.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == enB));
                if (separador == null) return null;
                Paso(log, () => e.S.Editor.SideA.ModuleSession.SetLength(separador.ModuleId, separador.Length + 2.0));
                Paso(log, () => e.S.Editor.SideA.CommitModuleEdits());
                computation = Pasada(e);
            }

            return Persistido(e, computation);
        }

        // ------------------------------------------------------------------------------------------------ the cases c-01 to c-16

        private static JsonNode Level(JsonNode inner, int front) => StructureNode(inner)["Fronts"][front]["Levels"][0];

        private static double TooDeep(JsonNode inner, int front)
        {
            var beamId = Level(inner, front)["InOutBeamCatalogId"].GetValue<string>();
            return DynamicRackLevelGeometry.AllowedPeraltes(Catalog, beamId).Max() + 0.5;
        }

        private static IEnumerable<KeyValuePair<string, Action<JsonNode>>> Cases()
        {
            yield return new KeyValuePair<string, Action<JsonNode>>("c-01 HeaderPostCatalogId vacio", inner => StructureNode(inner)["HeaderPostCatalogId"] = string.Empty);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-02 HeaderPostCatalogId inexistente", inner => StructureNode(inner)["HeaderPostCatalogId"] = "PB0_POSTE_INEXISTENTE");
            yield return new KeyValuePair<string, Action<JsonNode>>("c-03 HighEndBeamPeraltes[0]=7.25", inner => inner["PushBack"]["Fronts"][0]["HighEndBeamPeraltes"][0] = 7.25);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-04 InOutBeamCatalogId inexistente", inner => Level(inner, 0)["InOutBeamCatalogId"] = MissingBeam);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-05 InOutBeamDepth fuera del maximo", inner => Level(inner, 0)["InOutBeamDepth"] = TooDeep(inner, 0));
            yield return new KeyValuePair<string, Action<JsonNode>>("c-06 IntermediateBeamCatalogId inexistente", inner => Level(inner, 0)["IntermediateBeamCatalogId"] = MissingBeam);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-07 InOutBeamDepth fuera del maximo en el frente 1", inner => Level(inner, 1)["InOutBeamDepth"] = TooDeep(inner, 1));
            yield return new KeyValuePair<string, Action<JsonNode>>("c-08 PalletWeightUnit vacia", inner => StructureNode(inner)["PalletWeightUnit"] = string.Empty);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-09 PalletWeightUnit ton", inner => StructureNode(inner)["PalletWeightUnit"] = "ton");
            yield return new KeyValuePair<string, Action<JsonNode>>("c-10 Dimensions 7", inner => StructureNode(inner)["Dimensions"] = 7);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-11 Dimensions -1", inner => StructureNode(inner)["Dimensions"] = -1);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-12 PalletsDeep 1", inner => StructureNode(inner)["PalletsDeep"] = 1);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-13 PalletTolerance 0", inner => StructureNode(inner)["PalletTolerance"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-14 BeamDepth 0", inner => StructureNode(inner)["BeamDepth"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-15 AnnotationScale 0", inner => StructureNode(inner)["AnnotationScale"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-16 Dimensions 3 limite valido", inner => StructureNode(inner)["Dimensions"] = 3);
        }

        /// <summary>One case: the window and the state load the edited design; when either rejects it, the line says how.</summary>
        private static string Caso(List<string> log, string nombre, string sobre, Func<int> descartes)
        {
            Origen origen;
            try
            {
                origen = DeSobre(nombre, sobre);
                origen.Abrir();
            }
            catch (Exception rechazado)
            {
                // The store refuses the document before any load: neither the window nor the state sees it.
                return nombre + " LoadExisting rechazado-por-el-almacen=" + rechazado.GetType().Name + ": " + rechazado.Message + " descartes=" + descartes();
            }

            var w = Ventana();
            try
            {
                var e = new Estado(w.Session.Catalog);
                string ventanaRechazo = null;
                string estadoRechazo = null;
                var abierto = origen.Abrir();
                try
                {
                    w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                }
                catch (Exception rechazado)
                {
                    ventanaRechazo = rechazado.GetType().Name + ": " + rechazado.Message;
                }

                Nueva(log, e);
                var otro = origen.Abrir();
                try
                {
                    e.S.LoadDesign(otro.Design, otro.Name, e.A.Resolver);
                }
                catch (Exception rechazado)
                {
                    estadoRechazo = rechazado.GetType().Name + ": " + rechazado.Message;
                }

                if (ventanaRechazo != null || estadoRechazo != null)
                {
                    return Fin(w, nombre + " LoadExisting rechazo-igual=" + (ventanaRechazo == estadoRechazo) + " rechazo=[" + (ventanaRechazo ?? "<null>") + "]", descartes);
                }

                return Fin(w, Linea(nombre, "LoadExisting", Ventanada(w), Persistido(e, Pasada(e)), w.OutputIsBlockedForTest, BloqueadoEstado(e)), descartes);
            }
            finally { Cerrar(w); }
        }

        // ------------------------------------------------------------------------------------------------ the blocking cases (CT-12)

        /// <summary>The design of <c>PushBackOutputGateTests.Blocking()</c> (<c>:41-48</c>, its calls copied): a cell that does not fit.</summary>
        private static PushBackDesign CeldaQueNoCabe()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 8,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0,
                },
                SideB = new PushBackSideDesign { IsPresent = true, LoadLevels = 2, FirstLevelHeight = 4.0 },
                Composite = new PushBackCompositeDesign { Gap = 0.0, CentralSeparator = false, DefaultTopology = PushBackCellTopology.Encontradas },
            };
            for (var slot = 0; slot < 2; slot++)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 8, DepthStartPosition = 1 });
                design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 8 });
                design.SideB.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 8, DepthStartPosition = 1 });
                design.SideB.FrontConfigs.Add(new PushBackFrontConfig { DefaultPalletsDeep = 8 });
            }

            design.Composite.StructureOverrideA = PushBackCellDepth.MinimumPalletsDeep;
            design.Composite.StructureOverrideB = PushBackCellDepth.MinimumPalletsDeep;
            return design;
        }

        private static JsonNode CompositeNode(JsonNode inner) => inner["PushBack"]["Composite"];

        /// <summary>
        /// One blocking case: the window loads it (and, for the negative gap, the seam <c>SetGap(-1)</c> and a recompute without
        /// editing a field); the state does the same; the lines compare the persisted design, the verdict and the status text
        /// of the window against <c>StatusOr</c>.
        /// </summary>
        private static string[] Bloqueante(List<string> log, string nombre, Origen origen, bool huecoNegativo, Func<int> descartes)
        {
            var w = Ventana();
            try
            {
                var e = new Estado(w.Session.Catalog);
                var abierto = origen.Abrir();
                w.LoadExisting(abierto.Design, abierto.Id, abierto.Name, abierto.Project);
                var estado = Cargada(log, e, origen);
                if (huecoNegativo)
                {
                    w.CompositeState.SetGap(-1);
                    Recalcular(w);
                    Paso(log, () => e.S.Editor.SetGap(-1));
                    estado = Persistido(e, Pasada(e));
                }

                var textoVentana = StatusText(w);
                var textoEstado = PushBackEditorOutputGate.StatusOr(e.S.Editor, e.Last?.System, "Vista recalculada.");
                var principal = Linea(nombre, "LoadExisting", Ventanada(w), estado, w.OutputIsBlockedForTest, BloqueadoEstado(e));
                return new[]
                {
                    Fin(w, principal, descartes),
                    nombre + " texto-igual=" + (textoVentana == textoEstado),
                };
            }
            finally { Cerrar(w); }
        }

        // ------------------------------------------------------------------------------------------------ the test

        private static string Ok(string diseno, string camino) => diseno + " " + camino + " iguales=True bloqueado-ventana=False bloqueado-estado=False descartes=0";

        private static string Bloqueado(string diseno) => diseno + " LoadExisting iguales=True bloqueado-ventana=True bloqueado-estado=True descartes=0";

        [Fact]
        public void CT_PBT1B_01_TheLoadsOfTheStateMatchTheWindowOfToday()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // A new window.
                    var nueva = Ventana();
                    try
                    {
                        var e = new Estado(nueva.Session.Catalog);
                        var estado = Nueva(log, e);
                        log.Add(Fin(nueva, Linea("(sin-diseno)", "nueva", Ventanada(nueva), estado, nueva.OutputIsBlockedForTest, BloqueadoEstado(e)), () => descartes));
                    }
                    finally { Cerrar(nueva); }

                    // The 17 designs of Fixtures/I64, X-01 and F-01, by every path.
                    var origenes = CorpusNames().Select(nombre => DeSobre(nombre, Fixture(nombre))).ToList();
                    origenes.Add(X01Ventana());
                    origenes.Add(F01Ventana());
                    foreach (var origen in origenes)
                    {
                        foreach (var camino in CaminosDeUnLado)
                        {
                            log.Add(Camino(log, origen, camino, () => descartes));
                        }

                        if (TieneLadoB(origen))
                        {
                            log.Add(Camino(log, origen, CaminoRetirarB, () => descartes));
                        }

                        log.Add(Camino(log, origen, CaminoSetGap, () => descartes));
                        log.Add(Camino(log, origen, "Actualizar", () => descartes));
                    }

                    // M08, declared in ONE line: «retirar B» only runs where B exists; «SetGap(60)» runs on every design.
                    var conLadoB = origenes.Where(TieneLadoB).Select(origen => origen.Nombre).ToList();
                    log.Add("Restaurar-tras-retirar-B solo-con-lado-B=[" + string.Join(", ", conLadoB) + "] omitido-en=" + (origenes.Count - conLadoB.Count)
                        + " Restaurar-tras-SetGap(60) en=" + origenes.Count);

                    // The two recipes: the window and the state give the same design.
                    var x01 = X01Ventana();
                    var f01 = F01Ventana();
                    var x01Estado = X01Estado(log);
                    var f01Estado = F01Estado(log);
                    log.Add("X-01 receta-ventana-y-estado iguales=" + (x01Estado != null && x01Estado == Persist(x01.Abrir().Design)) + " descartes=" + descartes);
                    log.Add("F-01 receta-ventana-y-estado iguales=" + (f01Estado != null && f01Estado == Persist(f01.Abrir().Design)) + " descartes=" + descartes);

                    // The cases c-01 to c-16 of CT-PB0-17.
                    foreach (var caso in Cases())
                    {
                        log.Add(Caso(log, caso.Key, EditInner(Fixture("pb-01-base"), caso.Value), () => descartes));
                    }

                    // The four blocking cases of CT-12, with their status text.
                    var ab = Fixture("pb-20-ab");
                    log.AddRange(Bloqueante(log, "hueco-negativo", DeSobre("hueco-negativo", ab), true, () => descartes));
                    log.AddRange(Bloqueante(
                        log, "separador-sin-hueco",
                        DeSobre("separador-sin-hueco", EditInner(ab, inner => { CompositeNode(inner)["CentralSeparator"] = true; CompositeNode(inner)["Gap"] = 0; })),
                        false, () => descartes));
                    log.AddRange(Bloqueante(
                        log, "override-no-construible",
                        DeSobre("override-no-construible", EditInner(ab, inner => CompositeNode(inner)["StructureOverrideA"] = 1)),
                        false, () => descartes));
                    log.AddRange(Bloqueante(log, "celda-no-cabe", DeDiseno("celda-no-cabe", CeldaQueNoCabe), false, () => descartes));
                });
            }

            Assert.Equal(Esperadas(), log);
        }

        /// <summary>The designs that open with side B declared (M08): the expected list, written out (the run derives it from the designs).</summary>
        private static bool ConLadoB(string nombre) => nombre == "pb-20-ab" || nombre == "X-01" || nombre == "F-01";

        private static List<string> Esperadas()
        {
            var esperadas = new List<string> { Ok("(sin-diseno)", "nueva") };
            var nombres = CorpusNames().ToList();
            nombres.Add("X-01");
            nombres.Add("F-01");
            foreach (var nombre in nombres)
            {
                foreach (var camino in CaminosDeUnLado) esperadas.Add(Ok(nombre, camino));
                if (ConLadoB(nombre)) esperadas.Add(Ok(nombre, CaminoRetirarB));
                esperadas.Add(Ok(nombre, CaminoSetGap));
                esperadas.Add(Ok(nombre, "Actualizar"));
            }

            esperadas.Add("Restaurar-tras-retirar-B solo-con-lado-B=[pb-20-ab, X-01, F-01] omitido-en=" + nombres.Count(n => !ConLadoB(n)) + " Restaurar-tras-SetGap(60) en=" + nombres.Count);

            esperadas.Add("X-01 receta-ventana-y-estado iguales=True descartes=0");
            esperadas.Add("F-01 receta-ventana-y-estado iguales=True descartes=0");

            // c-01 to c-16 (CT-PB0-17): the store refuses c-12 before any window; c-13 throws on loading, in the window and
            // in the state, with the same type and the same message.
            foreach (var caso in Cases())
            {
                if (caso.Key.StartsWith("c-12 ", StringComparison.Ordinal))
                {
                    esperadas.Add(caso.Key + " LoadExisting rechazado-por-el-almacen=InvalidOperationException: El proyecto no tiene datos válidos para el sistema Push Back (¿archivo vacío o incompleto?). descartes=0");
                }
                else if (caso.Key.StartsWith("c-13 ", StringComparison.Ordinal))
                {
                    esperadas.Add(caso.Key + " LoadExisting rechazo-igual=True rechazo=[ArgumentException: La holgura transversal debe ser mayor que cero. (Parameter 'design')] descartes=0");
                }
                else
                {
                    esperadas.Add(Ok(caso.Key, "LoadExisting"));
                }
            }

            esperadas.Add(Bloqueado("hueco-negativo"));
            esperadas.Add("hueco-negativo texto-igual=True");
            esperadas.Add(Bloqueado("separador-sin-hueco"));
            esperadas.Add("separador-sin-hueco texto-igual=True");
            esperadas.Add(Bloqueado("override-no-construible"));
            esperadas.Add("override-no-construible texto-igual=True");
            esperadas.Add(Bloqueado("celda-no-cabe"));
            esperadas.Add("celda-no-cabe texto-igual=True");
            return esperadas;
        }
    }
}
