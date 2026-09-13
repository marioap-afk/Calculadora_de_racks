using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G6 — fixtures del consumidor Dinamico del nucleo compartido.
    ///
    /// <para>
    /// Todo pasa por las rutas reales: el resolver del Dinamico (diseno -> sistema), su snapshot (sistema -> diseno), el
    /// constructor y la reconstruccion de I-53. <see cref="Editor"/> hace lo que la ventana hace entre gestos: recomponer
    /// SIN reconstruir (los ids y la generacion viajan) o reconstruir por tarima, fondos o «Restaurar estandar» (la
    /// generacion avanza). Las ediciones de modulo que la ventana hace en sitio (fondo manual, cambio de tipo, cabecera
    /// editada) se reproducen con sus mismas asignaciones y su mismo <c>builder.Refresh</c>.
    /// </para>
    /// <para>
    /// Las huellas son de SOLO LECTURA: sirven para demostrar que PREPARE no toca el sistema vivo.
    /// </para>
    /// </summary>
    internal static class DynamicHeaderBatchFixtures
    {
        internal const string PostId = "POSTE_OMEGA_3X3";
        internal const int LoadLevels = 3;
        internal const double FirstLevel = 6.0;
        internal const double BeamDepth = 4.0;

        internal static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        internal static PalletSpecification Pallet(double depth = 48.0) => new PalletSpecification(42.0, depth, 60.0, 1000.0, "kg");

        /// <summary>Un rack de un solo frente declarado por su numero de fondos. Con 9: C S C S C S C S C (M1..M9).</summary>
        internal static DynamicRackDesign Design(int palletsDeep = 9, double palletDepth = 48.0)
            => new DynamicRackDesign
            {
                Pallet = Pallet(palletDepth),
                PalletsDeep = palletsDeep,
                LoadLevels = LoadLevels,
                FirstLevelHeight = FirstLevel,
                BeamDepth = BeamDepth,
                HeaderPostCatalogId = PostId
            };

        /// <summary>
        /// Frentes [activo 4][en blanco 4][en blanco 10][en blanco 4][activo 4]: las fronteras 2 y 3 separan dos frentes en
        /// blanco y no se construyen (I-33), asi que los modulos 5..10 —que solo ellas alcanzaban— existen logicamente y no
        /// se dibujan. Secuencia: M1 C, M2 S, M3 S, M4 C, M5 S, M6 C, M7 S, M8 C, M9 S, M10 C.
        /// </summary>
        internal static DynamicRackDesign DesignWithUndrawnModules()
        {
            var design = Design(palletsDeep: 10);
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 4 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 4, IsActive = false });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 10, IsActive = false });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 4, IsActive = false });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 4 });
            return design;
        }

        /// <summary>Una cabecera personalizada reconocible por su <c>PanelClear</c>, construida por el constructor real.</summary>
        internal static RackFrameConfiguration Custom(double panelClear, double depth = 48.0, double height = 132.0)
        {
            var configuration = new DynamicRackSystemBuilder(Catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, PostId, height, depth);
            configuration.PanelClear = panelClear;
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            return configuration;
        }

        internal static FrameExceptionOverride RuntimeException(string reason)
            => new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.PatternChange,
                TargetId = "P1",
                StandardValue = "SingleDiagonal",
                OverrideValue = "XBracing",
                Reason = reason
            };

        /// <summary>
        /// El editor: un sistema resuelto, su estado de runtime de I-53 y las dos maneras que tiene la ventana de cambiar la
        /// estructura.
        /// </summary>
        internal sealed class Editor
        {
            private readonly List<DynamicRackFrontDesign> fronts;

            internal Editor(DynamicRackDesign design)
            {
                Resolver = new DynamicRackSystemResolver(Catalog);
                Builder = new DynamicRackSystemBuilder(Catalog);
                fronts = design.Fronts.ToList();
                PalletDepth = design.Pallet.Depth;
                PalletsDeep = design.PalletsDeep;
                System = Resolver.Resolve(design).System;
                State = new DynamicHeaderBatchState();
            }

            internal DynamicRackSystemResolver Resolver { get; }

            internal DynamicRackSystemBuilder Builder { get; }

            internal DynamicHeaderBatchState State { get; }

            internal DynamicRackSystem System { get; set; }

            internal double PalletDepth { get; private set; }

            internal int PalletsDeep { get; private set; }

            internal IReadOnlyList<string> HeaderIds
                => System.Modules.Where(module => module.IsHeader).OrderBy(module => module.Index).Select(module => module.ModuleId).ToList();

            internal DynamicRackModule Module(string moduleId) => System.Modules.Single(module => module.ModuleId == moduleId);

            /// <summary>La intencion editable como la guarda el editor: el snapshot del resolver con los frentes y los fondos
            /// del propio editor (lo que hace <c>BuildDesign</c>).</summary>
            internal DynamicRackDesign Design()
            {
                var design = Resolver.Snapshot(System, LoadLevels, FirstLevel, BeamDepth, PostId);
                design.PalletsDeep = PalletsDeep;
                design.Fronts.Clear();
                foreach (var front in fronts)
                {
                    design.Fronts.Add(front);
                }

                return design;
            }

            /// <summary>Recomponer SIN reconstruir: snapshot -> resolve; los ids y la generacion viajan.</summary>
            internal void Recompose() => System = Resolver.Resolve(Design()).System;

            /// <summary>
            /// Reconstruir por la operacion de I-53 —otra tarima, otro numero de fondos o «Restaurar estandar»— y seguir
            /// como la ventana: peralte del rack, diseno y resolve.
            /// </summary>
            internal DynamicRackRebuildResult Rebuild(double? palletDepth = null, int? palletsDeep = null, bool restoreStandard = false)
            {
                PalletDepth = palletDepth ?? PalletDepth;
                PalletsDeep = palletsDeep ?? PalletsDeep;

                var probe = DynamicHeaderBatchFixtures.Design(PalletsDeep, PalletDepth);
                foreach (var front in fronts)
                {
                    probe.Fronts.Add(front);
                }

                var layout = DynamicDepthGeometry.Resolve(probe.Fronts, PalletsDeep);
                var height = Resolver.Resolve(probe).Height.HeaderHeight;
                var postPeralte = System.PostPeralte;

                var result = DynamicRackRebuild.Rebuild(
                    System,
                    State,
                    new DynamicRackRebuildRequest
                    {
                        Pallet = Pallet(PalletDepth),
                        DepthLayout = layout,
                        HeaderPostCatalogId = PostId,
                        HeaderHeight = height,
                        PostPeralte = postPeralte,
                        LoadLevels = LoadLevels,
                        FirstLevelHeight = FirstLevel,
                        BeamDepth = BeamDepth,
                        RestoreStandard = restoreStandard
                    },
                    Builder,
                    Resolver);

                Assert.NotNull(result);
                Assert.NotNull(result.System);
                Assert.NotNull(result.Reconciliation);
                System = result.System;
                Builder.ApplyPostPeralte(System, postPeralte);
                Recompose();
                return result;
            }

            /// <summary>Deja una cabecera personalizada como la deja hoy «Editar cabecera»: asignada, marcada y refrescada.</summary>
            internal RackFrameConfiguration Customize(string moduleId, double panelClear)
                => Customize(moduleId, Custom(panelClear, Module(moduleId).Length));

            internal RackFrameConfiguration Customize(string moduleId, RackFrameConfiguration configuration)
            {
                var module = Module(moduleId);
                module.AssociatedFrameConfiguration = configuration;
                module.UseCalculatedHeaderConfiguration = false;
                Builder.ApplyPostPeralte(System, System.PostPeralte);
                Builder.Refresh(System);
                return configuration;
            }

            /// <summary>Un fondo manual como lo deja «Aplicar» del modulo, sin cambiar su tipo.</summary>
            internal void ManualLength(string moduleId, double length)
            {
                var module = Module(moduleId);
                module.Length = length;
                module.IsManualOverride = true;
                module.IsCalculated = false;
                Builder.Refresh(System);
            }

            /// <summary>Convertir un modulo en separador como lo hace «Aplicar» del modulo (sin reconstruir).</summary>
            internal void ToSeparator(string moduleId, double length)
            {
                var module = Module(moduleId);
                module.Length = length;
                module.IsManualOverride = true;
                module.IsCalculated = false;
                module.Kind = DynamicRackModuleKind.Separator;
                module.AssociatedFrameConfiguration = null;
                module.UseCalculatedHeaderConfiguration = true;
                Builder.Refresh(System);
            }

            internal DynamicHeaderSource Source(string moduleId)
            {
                var source = DynamicHeaderSource.Of(System, State.Generation, moduleId);
                Assert.NotNull(source);
                return source;
            }

            internal DynamicHeaderSource RememberSource(string moduleId)
            {
                var source = State.RememberSource(System, moduleId);
                Assert.NotNull(source);
                return source;
            }

            internal DynamicModuleTargets Explicit(params string[] moduleIds)
            {
                var targets = new DynamicModuleTargets();
                targets.SetTargetModules(moduleIds, System, State.Generation);
                return targets;
            }

            internal DynamicHeaderBatchRequest Distribute(string sourceId, DynamicModuleTargets targets, string selectedModuleId = null)
                => Request(DynamicHeaderBatchRequest.Distribute(Source(sourceId), targets, selectedModuleId));

            internal DynamicHeaderBatchPreparation Prepare(DynamicHeaderBatchRequest request)
                => DynamicHeaderBatch.Prepare(System, State, request);

            internal HeaderBatchOutcome<DynamicHeaderAddress> Apply(DynamicHeaderBatchPreparation preparation)
                => DynamicHeaderBatch.Apply(preparation, System, State, Builder);
        }

        internal static DynamicHeaderBatchRequest Request(DynamicHeaderBatchRequest request)
        {
            Assert.NotNull(request);
            return request;
        }

        internal static DynamicModuleTargets All()
        {
            var targets = new DynamicModuleTargets();
            targets.FollowAllModules();
            return targets;
        }

        internal static DynamicModuleTargets Current() => new DynamicModuleTargets();

        internal static HeaderBatchPlan<DynamicHeaderAddress>.Prepared Prepared(DynamicHeaderBatchPreparation preparation)
        {
            Assert.NotNull(preparation);
            Assert.Null(preparation.PreconditionFailure);
            return Assert.IsType<HeaderBatchPlan<DynamicHeaderAddress>.Prepared>(preparation.Plan);
        }

        internal static HeaderRejectionCode RejectedCode(DynamicHeaderBatchPreparation preparation)
        {
            Assert.NotNull(preparation);
            Assert.Null(preparation.PreconditionFailure);
            return Assert.IsType<HeaderBatchPlan<DynamicHeaderAddress>.Rejected>(preparation.Plan).Code;
        }

        internal static HeaderRejectionCode RejectedCode(HeaderBatchOutcome<DynamicHeaderAddress> outcome)
            => Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Rejected>(outcome).Code;

        internal static HeaderBatchOutcome<DynamicHeaderAddress>.Committed Committed(HeaderBatchOutcome<DynamicHeaderAddress> outcome)
            => Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(outcome);

        internal static IReadOnlyList<string> Ids(IEnumerable<DynamicHeaderAddress> addresses)
            => addresses.Select(address => address.ModuleId).ToList();

        internal static IReadOnlyList<string> Omissions(IEnumerable<HeaderOmission<DynamicHeaderAddress>> omitted)
            => omitted.Select(omission => omission.Address.ModuleId + ":" + omission.Reason).ToList();

        internal static IReadOnlyList<RackFrameConfiguration> Copies(DynamicHeaderBatchPreparation preparation)
        {
            var plan = Prepared(preparation);
            Assert.NotNull(preparation.PreparedCopies);
            Assert.Equal(plan.Targets.Count, preparation.PreparedCopies.Count);
            return preparation.PreparedCopies;
        }

        // ---- Huellas de solo lectura -------------------------------------------------------------------------------

        private static string R(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>
        /// La receta de una cabecera: su proyeccion persistida SIN lo que el recompute del Dinamico impone (fondo = longitud
        /// del modulo y peralte del rack). Dos cabeceras con la misma receta son la misma personalizada en modulos distintos.
        /// </summary>
        internal static string Recipe(RackFrameConfiguration configuration)
        {
            Assert.NotNull(configuration);
            var copy = new RackFrameProjectStore().DeepCopy(configuration);
            copy.Depth = 1.0;
            copy.PostPeralte = 1.0;
            return HeaderConfigurationFixtures.Wire(copy);
        }

        /// <summary>El sistema en todo lo que I-53 podria tocar: cada modulo (identidad, tipo, geometria, banderas y cabecera
        /// completa: persistida, derivada y runtime), el peralte del rack y los overrides por linea.</summary>
        internal static string SystemFingerprint(DynamicRackSystem system)
        {
            var text = new StringBuilder();
            text.Append("peralte=").Append(R(system.PostPeralte)).Append(";total=").Append(R(system.TotalLength));
            foreach (var module in system.Modules)
            {
                text.Append(';').Append(module.ModuleId).Append(':').Append(module.Kind).Append(':').Append(module.Index)
                    .Append(':').Append(R(module.Length)).Append(':').Append(R(module.StartX)).Append('-').Append(R(module.EndX))
                    .Append(':').Append(module.IsCalculated).Append(':').Append(module.IsManualOverride)
                    .Append(':').Append(module.UseCalculatedHeaderConfiguration).Append(':').Append(module.Notes)
                    .Append(":cfg=").Append(SelectiveHeaderBatchFixtures.Configuration(module.AssociatedFrameConfiguration));
            }

            text.Append(";lineas=").Append(string.Join("|", system.HeaderLineOverrides.Select(line =>
                line.PostIndex.ToString(CultureInfo.InvariantCulture) + "/" + line.ModuleId + "/"
                + SelectiveHeaderBatchFixtures.Configuration(line.Header))));
            text.Append(";derivados=").Append(string.Join("|", system.DerivedPostLineOverrides.Select(derived =>
                derived.PostIndex.ToString(CultureInfo.InvariantCulture) + "/" + R(derived.Height))));
            return text.ToString();
        }

        /// <summary>El sistema serializado por la ruta de persistencia del editor (snapshot del resolver -> store).</summary>
        internal static string Serialized(Editor editor)
            => new RackProjectStore().Serialize(RackProject.ForDynamic(editor.Design()));

        /// <summary>Todo objeto que I-53 podria reemplazar o refrescar, por IDENTIDAD: modulos, cabeceras con su derivado y
        /// overrides por linea. Un <c>Refresh</c> o un <c>ApplyPostPeralte</c> regenera miembros y se veria aqui.</summary>
        internal static HashSet<object> ReferenceGraph(DynamicRackSystem system)
        {
            var graph = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var module in system.Modules)
            {
                graph.Add(module);
                if (module.AssociatedFrameConfiguration != null)
                {
                    graph.UnionWith(HeaderConfigurationFixtures.MutableGraph(module.AssociatedFrameConfiguration));
                }
            }

            foreach (var line in system.HeaderLineOverrides)
            {
                graph.Add(line);
                if (line.Header != null)
                {
                    graph.UnionWith(HeaderConfigurationFixtures.MutableGraph(line.Header));
                }
            }

            foreach (var derived in system.DerivedPostLineOverrides)
            {
                graph.Add(derived);
            }

            return graph;
        }

        internal static void AssertSameGraph(HashSet<object> expected, HashSet<object> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(expected.SetEquals(actual), "El grafo por identidad cambio: algo se reemplazo o se regenero.");
        }

        internal static string BomSignature(DynamicRackSystem system)
            => string.Join("|", SystemBomBuilder.Build(system, Catalog).Lines
                .Select(line => line.Category + ":" + line.ProfileId + ":" + line.Description + ":"
                                + line.Length.ToString("0.###", CultureInfo.InvariantCulture) + ":" + line.Quantity)
                .OrderBy(text => text, StringComparer.Ordinal));
    }
}
