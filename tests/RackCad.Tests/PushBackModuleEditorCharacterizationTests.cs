using System.Globalization;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-35 (PB-011) — CHARACTERIZATION of the base, captured BEFORE any behavior changes.
    ///
    /// These tests do not assert what Push Back SHOULD do; they pin what it does TODAY, so the initiative that
    /// adds the advanced module editor cannot move any of it by accident. Four of the seven facts are healthy
    /// invariants to protect; three describe defects that are currently INERT and become real the moment Push
    /// Back gains custom cabeceras — those carry a REGRESION marker naming what must invert them.
    ///
    /// Pure: the real resolve/BOM/plan pipeline runs against the distributed catalog. No WPF, no AutoCAD.
    /// </summary>
    public class PushBackModuleEditorCharacterizationTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs(int palletsDeep = 4, double palletDepth = 48.0)
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, palletDepth, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep
            };

        private static DynamicRackDesign Structure(int palletsDeep = 6, int loadLevels = 2)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep,
                LoadLevels = loadLevels,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        // ===== Fact 4 — every Push Back cabecera is "calculated" ================================================

        /// <summary>
        /// The premise of PB-011's technical note. <c>DynamicRackSystemBuilder.CreateHeader</c> stamps
        /// <c>UseCalculatedHeaderConfiguration = true</c> and only the DYNAMIC window ever sets it false, so a Push
        /// Back rack built end to end has no custom cabecera anywhere. Everything the two "inert" facts below
        /// describe hangs off this: the day Push Back can set it false, they stop being inert.
        /// </summary>
        [Fact]
        public void Fact4_EveryHeaderOfAFreshPushBack_IsCalculated_InState_Design_AndResolvedSystem()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();

            var computation = assembler.Build(state, Inputs());

            Assert.True(computation.IsValid, computation.Error);
            var headers = computation.Design.Structure.Modules.Where(module => module.IsHeader).ToList();
            Assert.NotEmpty(headers);
            Assert.All(headers, header => Assert.True(header.UseCalculatedHeaderConfiguration));
            Assert.All(
                computation.System.Structure.Modules.Where(module => module.IsHeader),
                header => Assert.True(header.UseCalculatedHeaderConfiguration));
        }

        /// <summary>
        /// The same fact stated over the SOURCE of the flag, so a future edit to the builder is caught even if no
        /// editor runs: the standard structure any system composes is born fully calculated.
        /// </summary>
        [Fact]
        public void Fact4_TheSharedBuilder_StampsCalculatedProvenance_OnEveryHeaderItCreates()
        {
            var catalog = Catalog;
            var builder = new DynamicRackSystemBuilder(catalog);
            var layout = DynamicDepthGeometry.Resolve(new[] { new DynamicRackFrontDesign { PalletsDeep = 4 } }, 4);

            var system = builder.BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                layout,
                RackFrameTemplateCatalog.Default,
                catalog.Defaults?.Post,
                120.0,
                0.0);

            var headers = system.Modules.Where(module => module.IsHeader).ToList();
            Assert.NotEmpty(headers);
            Assert.All(headers, header => Assert.True(header.UseCalculatedHeaderConfiguration));
        }

        // ===== Fact 2 — the preservation machinery already works ================================================

        /// <summary>
        /// A recompute WITHOUT a structural change reuses a copy of <c>WorkingBaseline</c>, so a custom cabecera —
        /// provenance flag AND semantic configuration — survives verbatim. This is the healthy half of the base and
        /// the reason the editor does not need to be rebuilt from scratch: the machinery exists, the surface does not.
        /// </summary>
        [Fact]
        public void Fact2_RecomputeWithoutStructuralChange_PreservesACustomCabecera_AndItsProvenance()
        {
            const double marker = 40.0;   // the frame default PanelClear is 44
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(CustomConfigDesign(marker), assembler.Resolver);

            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            var custom = computation.Design.Structure.Modules
                .FirstOrDefault(module => module.IsHeader && !module.UseCalculatedHeaderConfiguration);
            Assert.NotNull(custom);
            Assert.Equal(marker, custom.HeaderConfiguration.PanelClear, 4);
        }

        /// <summary>The baseline only advances on a SUCCESSFUL computation, so a failed recompute can never corrupt
        /// the structure the user customized. Protects the transactional property the foundation builds on.</summary>
        [Fact]
        public void Fact2_AFailedRecompute_LeavesTheWorkingBaselineUntouched()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();
            state.LoadFromDesign(CustomConfigDesign(40.0), assembler.Resolver);
            var before = state.WorkingBaseline;
            Assert.NotNull(before);

            var broken = Inputs(palletDepth: 0.0);   // zero pallet depth: the structure build throws
            var failed = assembler.Build(state, broken);
            assembler.AcceptComputation(state, failed);

            Assert.False(failed.IsValid);
            Assert.Same(before, state.WorkingBaseline);
        }

        // ===== Fact 5 — the reconciliation loses the custom cabecera (INERT defect) =============================

        /// <summary>
        /// REGRESION de I-35: esta prueba fija el DEFECTO, no el comportamiento deseado. Un cambio estructural
        /// (tarima o fondos) fuerza el rebuild, y <c>RestoreHeaderFondos</c> guarda SOLO el fondo por ordinal: al
        /// restaurarlo pone <c>UseCalculatedHeaderConfiguration = true</c> y reconstruye la configuracion desde la
        /// fabrica. La cabecera personalizada se pierde en silencio.
        ///
        /// Hoy es INERTE en Push Back porque ninguna cabecera suya puede ser personalizada (Fact 4). La fase que
        /// implemente PB-011 debe INVERTIR estas dos aserciones: la cabecera personalizada debe sobrevivir al
        /// rebuild, y solo una restauracion explicita debe descartarla.
        /// </summary>
        [Fact]
        public void Fact5_REGRESION_AStructuralChange_RevertsACustomCabecera_ToCalculated()
        {
            const double marker = 40.0;
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(CustomConfigDesign(marker), assembler.Resolver);

            // Sanity: with no structural change the custom cabecera is there (Fact 2).
            var stable = assembler.Build(state, inputs);
            Assert.True(stable.IsValid, stable.Error);
            assembler.AcceptComputation(state, stable);
            Assert.Contains(
                stable.Design.Structure.Modules,
                module => module.IsHeader && !module.UseCalculatedHeaderConfiguration);

            // A pallet-depth change is a STRUCTURAL change: MustRebuild fires and the structure is rebuilt.
            inputs.Pallet = new PalletSpecification(42.0, 52.0, 60.0, 1000.0, "kg");
            var rebuilt = assembler.Build(state, inputs);
            Assert.True(rebuilt.IsValid, rebuilt.Error);

            // TODAY: nothing survives as custom, and the semantic marker is gone with it.
            Assert.DoesNotContain(
                rebuilt.Design.Structure.Modules,
                module => module.IsHeader && !module.UseCalculatedHeaderConfiguration);
            Assert.DoesNotContain(
                rebuilt.Design.Structure.Modules.Where(module => module.IsHeader && module.HeaderConfiguration != null),
                module => module.HeaderConfiguration.PanelClear == marker);
        }

        /// <summary>The snapshot the reconciliation is built on carries the FONDO and nothing else — the direct cause
        /// of the defect above, pinned at its source so the fix has an unambiguous target.</summary>
        [Fact]
        public void Fact5_SnapshotHeaderFondos_CapturesOnlyManualFondos_NotConfigurationsNorProvenance()
        {
            const double manualFondo = 55.0;
            var catalog = Catalog;
            var assembler = new DynamicEditorDesignAssembler(
                catalog,
                new DynamicRackSystemBuilder(catalog),
                new DynamicRackSystemResolver(catalog));
            var system = new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = Structure() }).Structure;

            var header = system.Modules.First(module => module.IsHeader);
            header.Length = manualFondo;
            header.IsManualOverride = true;
            header.IsCalculated = false;
            header.UseCalculatedHeaderConfiguration = false;      // fully custom
            header.AssociatedFrameConfiguration.PanelClear = 40.0;

            var fondos = assembler.SnapshotHeaderFondos(system);

            // The snapshot is a list of nullable doubles: it CANNOT carry a configuration or a provenance flag.
            Assert.Equal(system.Modules.Count(module => module.IsHeader), fondos.Count);
            Assert.Equal(manualFondo, fondos[0].Value, 4);
            Assert.All(fondos.Skip(1), fondo => Assert.False(fondo.HasValue));
        }

        // ===== Fact 6 — the resolver's clone is not the canonical one (INERT defect) ============================

        /// <summary>
        /// REGRESION de I-35: el clon del resolver no es el clon canonico de I-17. Las <c>Exceptions</c> son estado
        /// RUNTIME que el documento no persiste y que <c>RackFrameProjectStore.DeepCopy</c> reanexa a proposito; el
        /// round-trip por documento que usa <c>DynamicRackSystemResolver.CloneHeader</c> las descarta.
        ///
        /// Esta prueba contrasta los dos clones sobre la MISMA cabecera para que la diferencia sea inequivoca.
        /// Hoy es inerte en Push Back por Fact 4; deja de serlo con la primera cabecera personalizada.
        /// </summary>
        [Fact]
        public void Fact6_REGRESION_TheDocumentRoundTrip_DropsRuntimeExceptions_WhileDeepCopyKeepsThem()
        {
            var configuration = HeaderWithRuntimeException();
            Assert.Single(configuration.Exceptions);

            var canonical = new RackFrameProjectStore().DeepCopy(configuration);
            var documentRoundTrip = RackFrameProjectDocument.FromConfiguration(configuration).ToConfiguration();

            Assert.Single(canonical.Exceptions);                 // I-17: the canonical clone is complete
            Assert.Empty(documentRoundTrip.Exceptions);          // what CloneHeader does today
        }

        /// <summary>
        /// The same loss, observed END TO END on the path Push Back actually walks: every recompute without a
        /// structural change goes through <c>CopyStructureSystem</c> (Snapshot + Resolve), and both halves clone the
        /// header by document. On the RESOLVED system — the object the drawing and the BOM read — the persisted model
        /// survives and the derived model is rebuilt by <c>builder.Refresh</c>; only the runtime overrides are
        /// dropped, which is exactly why nothing visible breaks today.
        /// <para>
        /// Note the asymmetry the base actually has: on the DESIGN (a snapshot) the derived model is NOT rebuilt,
        /// because <c>CloneHeader</c> skips <c>RefreshPhysicalModel</c> and only the system gets <c>Refresh</c>.
        /// </para>
        /// </summary>
        [Fact]
        public void Fact6_REGRESION_ARecompute_DropsTheRuntimeExceptions_ButKeepsPersistedAndDerivedModel()
        {
            const double marker = 40.0;
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var design = CustomConfigDesign(marker);
            design.Structure.Modules
                .First(module => module.IsHeader && module.HeaderConfiguration != null)
                .HeaderConfiguration.Exceptions.Add(RuntimeException());

            var inputs = state.LoadFromDesign(design, assembler.Resolver);
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);

            var resolved = computation.System.Structure.Modules
                .First(module => module.IsHeader && !module.UseCalculatedHeaderConfiguration);
            Assert.Equal(marker, resolved.AssociatedFrameConfiguration.PanelClear, 4);   // persisted model: survives
            Assert.NotEmpty(resolved.AssociatedFrameConfiguration.Members);              // derived model: rebuilt
            Assert.Empty(resolved.AssociatedFrameConfiguration.Exceptions);              // runtime state: LOST

            var persisted = computation.Design.Structure.Modules
                .First(module => module.IsHeader && !module.UseCalculatedHeaderConfiguration);
            Assert.Equal(marker, persisted.HeaderConfiguration.PanelClear, 4);
            Assert.Empty(persisted.HeaderConfiguration.Exceptions);
            Assert.Empty(persisted.HeaderConfiguration.Members);   // the snapshot never refreshes the derived model
        }

        // ===== Fact 3 — the restore exists with no consumer =====================================================

        /// <summary>
        /// <c>forceRebuild</c> is the "restaurar estandar" semantics and it works: it discards the manual fondo the
        /// non-forced path preserves. The Push Back WINDOW never calls it — that is the missing button, not a missing
        /// capability, so the initiative wires it instead of inventing one.
        /// </summary>
        [Fact]
        public void Fact3_ForceRebuild_DiscardsTheManualFondo_ThatTheNonForcedRebuildPreserves()
        {
            const double manualFondo = 55.0;
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(CustomFondoDesign(manualFondo), assembler.Resolver);

            // Structural change, NOT forced: the fondo is snapshotted and restored by ordinal.
            inputs.Pallet = new PalletSpecification(42.0, 52.0, 60.0, 1000.0, "kg");
            var preserved = assembler.BuildDesign(state, inputs, forceRebuild: false);
            Assert.Contains(
                preserved.Structure.Modules,
                module => module.IsHeader && module.Length == manualFondo);

            // Same inputs, FORCED: the standard structure wins and the manual fondo is gone.
            var reset = assembler.BuildDesign(state, inputs, forceRebuild: true);
            Assert.DoesNotContain(
                reset.Structure.Modules,
                module => module.IsHeader && module.Length == manualFondo);
        }

        /// <summary>
        /// REGRESION de I-35: hoy NINGUN codigo fuera del Dinamico pide `forceRebuild: true`. Guardia por fuente —
        /// lee el `.cs` de la UI como texto, sin cargar WPF ni AutoCAD (patron de I-05/I-33). La fase que anada
        /// «Restaurar estandar» a Push Back debe INVERTIR esta asercion.
        /// </summary>
        [Fact]
        public void Fact3_REGRESION_NoPushBackSurface_RequestsAForcedRebuild_Today()
        {
            var pushBackWindow = File.ReadAllText(UiSourcePath("RackPushBackSystemWindow.xaml.cs"));
            var dynamicWindow = File.ReadAllText(UiSourcePath("RackDynamicSystemWindow.xaml.cs"));

            Assert.DoesNotContain("forceRebuild", pushBackWindow);
            Assert.Contains("forceRebuild: true", dynamicWindow);   // the only consumer in the product
        }

        // ===== Fact 1 — Push Back has no module surface =========================================================

        /// <summary>
        /// REGRESION de I-35: el XAML de Push Back no tiene NINGUNA de las piezas con las que el Dinamico edita un
        /// modulo. Comparar los dos XAML por los mismos nombres deja la carencia y su objetivo en una sola prueba;
        /// la fase que anada la superficie debe INVERTIR la mitad de Push Back.
        /// </summary>
        [Fact]
        public void Fact1_REGRESION_ThePushBackXaml_HasNoneOfTheModuleEditingControls_TheDynamicOneHas()
        {
            var pushBack = File.ReadAllText(UiSourcePath("RackPushBackSystemWindow.xaml"));
            var dynamicXaml = File.ReadAllText(UiSourcePath("RackDynamicSystemWindow.xaml"));

            var moduleControls = new[]
            {
                "AdvancedPanel", "ModulesGrid", "ModuleLengthBox", "ConfigBox",
                "ApplyModuleButton", "EditHeaderButton", "SeparatorCountBox", "SeparatorSpacingBox"
            };

            Assert.All(moduleControls, name => Assert.Contains(name, dynamicXaml));
            Assert.All(moduleControls, name => Assert.DoesNotContain(name, pushBack));
        }

        // ===== Fact 7 — no confirm/cancel anywhere ==============================================================

        /// <summary>
        /// REGRESION de I-35: el configurador de cabecera —la ventana que el Dinamico abre para personalizar una
        /// cabecera y que Push Back necesitaria— recibe la configuracion POR REFERENCIA, la muta, y no expone
        /// Aceptar/Cancelar ni `DialogResult`. Cerrarlo sin querer el cambio deja el cambio aplicado.
        ///
        /// El snapshot del Dinamico existe, pero solo compara para NO duplicar el preset «Personalizada N»: la
        /// palabra clave es que nunca se reasigna la configuracion desde el. Por eso la sesion transaccional de la
        /// fundacion es necesaria, y por eso tocar esta ventana compartida esta fuera de alcance sin decision.
        /// </summary>
        [Fact]
        public void Fact7_REGRESION_TheSharedHeaderConfigurator_HasNoConfirmCancel_AndMutatesByReference()
        {
            var configurator = File.ReadAllText(UiSourcePath("RackFrameConfiguratorWindow.xaml.cs"));
            var configuratorXaml = File.ReadAllText(UiSourcePath("RackFrameConfiguratorWindow.xaml"));
            var dynamicWindow = File.ReadAllText(UiSourcePath("RackDynamicSystemWindow.xaml.cs"));

            // No confirm/cancel contract: neither a DialogResult nor an Aceptar/Cancelar pair.
            Assert.DoesNotContain("DialogResult", configurator);
            Assert.DoesNotContain("Content=\"Aceptar\"", configuratorXaml);
            Assert.DoesNotContain("Content=\"Cancelar\"", configuratorXaml);

            // The ViewModel is built ON the caller's instance, so every edit lands on the caller's object.
            Assert.Contains("new RackFrameConfiguratorViewModel(configuration)", configurator);

            // The dynamic editor snapshots before the dialog but never restores from it.
            Assert.Contains("var beforeEdit = new RackProjectStore().Serialize(", dynamicWindow);
            Assert.DoesNotContain("AssociatedFrameConfiguration = beforeEdit", dynamicWindow);
        }

        // ===== The axis: modules are ONE rack-wide longitudinal sequence ========================================

        /// <summary>
        /// The fact that decides whether PB-011 is even the right scope. The module sequence is derived from the
        /// DEPTH layout: adding FRONTS leaves <c>Modules</c> byte-identical, while adding FONDOS changes it.
        /// Customizing a module therefore customizes the WHOLE rack — there is no per-front or per-post module. If
        /// the Owner expects per-front/per-post customization, the model does not support it.
        /// <para>
        /// The depth layout is fed by each front's own <c>PalletsDeep</c> (<c>BuildFrontMatrix</c> →
        /// <c>BuildFrontDesigns</c>), not by the rack-wide input — which is why the fondo arm below changes the
        /// FRONT and not <c>Inputs.PalletsDeep</c>. The single sequence still spans the union of every front's range.
        /// </para>
        /// </summary>
        [Fact]
        public void Axis_TheModuleSequence_FollowsTheDepthLayout_NotTheFrontCount()
        {
            var assembler = Assembler();

            var oneFront = new PushBackEditorState();
            var threeFronts = new PushBackEditorState();
            threeFronts.SetFrontCount(3);

            var baseline = Signature(assembler.BuildDesign(oneFront, Inputs()));
            var moreFronts = Signature(assembler.BuildDesign(threeFronts, Inputs()));

            var deeperState = new PushBackEditorState();
            deeperState.Structure.Fronts[0].PalletsDeep += 2;   // the same rack, two fondos deeper
            var deeper = Signature(assembler.BuildDesign(deeperState, Inputs()));

            Assert.Equal(baseline, moreFronts);      // more fronts: the very same module sequence
            Assert.NotEqual(baseline, deeper);       // more fondos: a different module sequence
        }

        /// <summary>A lateral section reads a RANGE of that same shared list, which is why a per-post module edit has
        /// nowhere to live: the sections do not own modules, they window into the rack's sequence.</summary>
        [Fact]
        public void Axis_ALateralSection_WindowsIntoTheSharedModuleList_ItDoesNotOwnModules()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();
            state.SetFrontCount(3);

            var computation = assembler.Build(state, Inputs(palletsDeep: 6));
            Assert.True(computation.IsValid, computation.Error);

            var structure = computation.System.Structure;
            var seen = 0;

            for (var postIndex = 0; postIndex < structure.Fronts.Count; postIndex++)
            {
                var range = DynamicDepthGeometry.AtPost(structure, postIndex);
                foreach (var module in DynamicDepthGeometry.ModulesInRange(structure, range))
                {
                    Assert.Contains(module, structure.Modules);   // the very same instances, not per-section copies
                    seen++;
                }
            }

            Assert.True(seen > 0);
        }

        // ===== Fixtures ========================================================================================

        private static string UiSourcePath(string fileName)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src", "RackCad.UI")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            var path = Path.Combine(directory.FullName, "src", "RackCad.UI", fileName);
            Assert.True(File.Exists(path), path);
            return path;
        }

        private static string Signature(PushBackDesign design)
            => string.Join(
                "|",
                design.Structure.Modules.Select(module => string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}:{1:0.####}",
                    module.Kind,
                    module.Length)));

        /// <summary>A Push Back design whose first header is REALLY custom: provenance false plus a semantically
        /// modified configuration, so only a faithful preservation path keeps it.</summary>
        private static PushBackDesign CustomConfigDesign(double panelClear)
        {
            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(new PushBackDesign { Structure = Structure() });

            var header = system.Structure.Modules
                .First(module => module.IsHeader && module.AssociatedFrameConfiguration != null);
            header.UseCalculatedHeaderConfiguration = false;
            header.AssociatedFrameConfiguration.PanelClear = panelClear;

            return resolver.Snapshot(system);
        }

        /// <summary>A Push Back design whose first header carries a MANUAL fondo (the thing the snapshot does capture).</summary>
        private static PushBackDesign CustomFondoDesign(double manualFondo)
        {
            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(new PushBackDesign { Structure = Structure() });

            var header = system.Structure.Modules.First(module => module.IsHeader);
            header.Length = manualFondo;
            header.IsManualOverride = true;
            header.IsCalculated = false;

            return resolver.Snapshot(system);
        }

        private static RackFrameConfiguration HeaderWithRuntimeException()
        {
            var catalog = Catalog;
            var configuration = new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default,
                catalog.Defaults?.Post,
                120.0,
                48.0);
            configuration.Exceptions.Add(RuntimeException());
            return configuration;
        }

        private static FrameExceptionOverride RuntimeException()
            => new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.SpecialClear,
                TargetId = "H1",
                StandardValue = "44",
                OverrideValue = "40",
                Reason = "runtime-only override; the document never persists it"
            };
    }
}
