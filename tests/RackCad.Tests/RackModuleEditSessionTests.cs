using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-35 — the neutral foundation: module descriptor, transactional edit session and module reconciliation.
    ///
    /// Nothing here is wired to a window yet, so none of it can change behavior; these tests exist so the adopting
    /// phase inherits a foundation that is already proven. They also state the two properties the base does NOT have
    /// and that the audit demanded: an edit can be CANCELLED, and a rebuild can carry a custom cabecera across.
    /// </summary>
    public class RackModuleEditSessionTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem StandardSystem(int palletsDeep = 6)
            => new PushBackResolver(Catalog).Resolve(new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            }).Structure;

        // ===== Descriptor =======================================================================================

        [Fact]
        public void Descriptor_ProjectsEveryModule_InLongitudinalOrder_WithItsProvenance()
        {
            var system = StandardSystem();

            var descriptors = RackModuleDescriptor.Describe(system);

            Assert.Equal(system.Modules.Count, descriptors.Count);
            Assert.Equal(
                system.Modules.Select(module => module.ModuleId),
                descriptors.Select(descriptor => descriptor.ModuleId));
            Assert.Contains(descriptors, descriptor => descriptor.IsHeader);
            Assert.All(descriptors, descriptor => Assert.True(descriptor.UsesCalculatedHeaderConfiguration));
            Assert.DoesNotContain(descriptors, descriptor => descriptor.HasCustomHeaderConfiguration);
        }

        [Fact]
        public void Descriptor_IsAProjection_SoEditingItCannotReachTheSystem()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var before = header.Length;

            var descriptor = RackModuleDescriptor.Describe(system).First(item => item.ModuleId == header.ModuleId);

            // The descriptor exposes no setter at all: the only way to observe it is to read it.
            Assert.Equal(before, descriptor.Length, 6);
            Assert.DoesNotContain(typeof(RackModuleDescriptor).GetProperties(), property => property.CanWrite);
        }

        // ===== Session: staging, commit and the cancel the base does not have ===================================

        [Fact]
        public void Session_StagesALength_WithoutTouchingTheSystem_UntilItIsCommitted()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var original = header.Length;
            var session = RackModuleEditSession.Begin(system);

            Assert.True(session.SetLength(header.ModuleId, original + 7.0));

            Assert.True(session.HasPendingChanges);
            Assert.Equal(original, header.Length, 6);   // the live system is untouched

            var committed = session.Commit();

            Assert.False(session.HasPendingChanges);
            Assert.Equal(original + 7.0, committed.First(module => module.ModuleId == header.ModuleId).Length, 6);
            Assert.Equal(original, header.Length, 6);   // still untouched: committing yields intents, it does not apply
        }

        [Fact]
        public void Session_Cancel_DiscardsEveryStagedChange_AndTheRestoreRequest()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var original = header.Length;
            var session = RackModuleEditSession.Begin(system);

            session.SetLength(header.ModuleId, original + 7.0);
            session.SetHeaderConfiguration(header.ModuleId, CustomHeader(40.0));
            session.RequestStandardRestore();
            Assert.True(session.HasPendingChanges);
            Assert.True(session.StandardRestoreRequested);

            session.Cancel();

            Assert.False(session.HasPendingChanges);
            Assert.False(session.StandardRestoreRequested);
            var restored = session.Modules.First(module => module.ModuleId == header.ModuleId);
            Assert.Equal(original, restored.Length, 6);
            Assert.True(restored.UsesCalculatedHeaderConfiguration);
        }

        [Fact]
        public void Session_Commit_ReBaselines_SoALaterCancelRevertsToTheCommittedState()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);

            session.SetLength(header.ModuleId, 55.0);
            session.Commit();

            session.SetLength(header.ModuleId, 77.0);
            session.Cancel();

            Assert.Equal(55.0, session.Modules.First(module => module.ModuleId == header.ModuleId).Length, 6);
        }

        [Fact]
        public void Session_StagingACustomHeader_FlipsProvenance_AndKeepsAnIndependentCanonicalCopy()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);
            var source = CustomHeader(40.0);
            source.Exceptions.Add(RuntimeException());

            Assert.True(session.SetHeaderConfiguration(header.ModuleId, source));

            var staged = session.Modules.First(module => module.ModuleId == header.ModuleId);
            Assert.True(staged.HasCustomHeaderConfiguration);
            Assert.False(staged.UsesCalculatedHeaderConfiguration);

            var copy = session.HeaderConfigurationCopy(header.ModuleId);
            Assert.NotSame(source, copy);
            Assert.Equal(40.0, copy.PanelClear, 4);
            Assert.Single(copy.Exceptions);   // I-17: only DeepCopy carries the runtime overrides

            source.PanelClear = 12.0;         // mutating the caller's instance must not reach the session
            Assert.Equal(40.0, session.HeaderConfigurationCopy(header.ModuleId).PanelClear, 4);
        }

        [Fact]
        public void Session_ResetHeaderToCalculated_ReturnsProvenance_AndDropsTheConfiguration()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);
            session.SetHeaderConfiguration(header.ModuleId, CustomHeader(40.0));

            Assert.True(session.ResetHeaderToCalculated(header.ModuleId));

            var staged = session.Modules.First(module => module.ModuleId == header.ModuleId);
            Assert.True(staged.UsesCalculatedHeaderConfiguration);
            Assert.False(staged.HasHeaderConfiguration);
            Assert.Null(session.HeaderConfigurationCopy(header.ModuleId));
        }

        [Fact]
        public void Session_RejectsEditsItCannotHonour_WithoutStagingAnything()
        {
            var system = StandardSystem();
            var separator = system.Modules.First(module => module.Kind == DynamicRackModuleKind.Separator);
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);

            Assert.False(session.SetLength("no-existe", 50.0));
            Assert.False(session.SetLength(header.ModuleId, 0.0));
            Assert.False(session.SetHeaderConfiguration(separator.ModuleId, CustomHeader(40.0)));
            Assert.False(session.SetHeaderConfiguration(header.ModuleId, null));
            Assert.False(session.ResetHeaderToCalculated(separator.ModuleId));

            Assert.False(session.HasPendingChanges);
        }

        [Fact]
        public void Session_AnEditUndoneByHand_ReadsAsNoPendingChange()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var original = header.Length;
            var session = RackModuleEditSession.Begin(system);

            session.SetLength(header.ModuleId, original + 5.0);
            Assert.True(session.HasPendingChanges);

            session.SetLength(header.ModuleId, original);

            // Length is back, but the override flags the first edit set are not: the session reports the real state
            // rather than pretending nothing happened.
            Assert.True(session.HasPendingChanges);
            Assert.True(session.Modules.First(module => module.ModuleId == header.ModuleId).IsManualOverride);
        }

        // ===== Reconciliation: what the base loses ==============================================================

        [Fact]
        public void Reconciliation_Preserve_CarriesTheCustomCabecera_AcrossARebuild()
        {
            var catalog = Catalog;
            var reconciliation = new RackModuleReconciliation(new DynamicRackSystemBuilder(catalog));
            var previous = IntentsWithACustomHeader(out var marker, out var manualFondo);
            var rebuilt = StandardSystem(palletsDeep: 8);   // a different structure, standard everywhere

            var result = reconciliation.Reconcile(previous, rebuilt, RackModuleCustomizationPolicy.Preserve);

            var header = rebuilt.Modules.First(module => module.IsHeader);
            Assert.Equal(1, result.ConfigurationsPreserved);
            Assert.Equal(1, result.FondosRestored);
            Assert.False(header.UseCalculatedHeaderConfiguration);
            Assert.Equal(marker, header.AssociatedFrameConfiguration.PanelClear, 4);
            Assert.Equal(manualFondo, header.Length, 6);
            Assert.NotEmpty(header.AssociatedFrameConfiguration.Members);   // derived model refreshed, as in the base
        }

        [Fact]
        public void Reconciliation_Discard_LeavesTheRebuildsStandardCabecera_WhichIsWhatTheBaseDoesToday()
        {
            var catalog = Catalog;
            var reconciliation = new RackModuleReconciliation(new DynamicRackSystemBuilder(catalog));
            var previous = IntentsWithACustomHeader(out var marker, out _);
            var rebuilt = StandardSystem(palletsDeep: 8);

            var result = reconciliation.Reconcile(previous, rebuilt, RackModuleCustomizationPolicy.Discard);

            var header = rebuilt.Modules.First(module => module.IsHeader);
            Assert.Equal(1, result.ConfigurationsDiscarded);
            Assert.Equal(0, result.ConfigurationsPreserved);
            Assert.True(header.UseCalculatedHeaderConfiguration);
            Assert.NotEqual(marker, header.AssociatedFrameConfiguration.PanelClear);
        }

        [Fact]
        public void Reconciliation_PreservedCabecera_IsAnIndependentCopy_NotTheIntentsInstance()
        {
            var catalog = Catalog;
            var reconciliation = new RackModuleReconciliation(new DynamicRackSystemBuilder(catalog));
            var previous = IntentsWithACustomHeader(out var marker, out _);
            var rebuilt = StandardSystem(palletsDeep: 8);

            reconciliation.Reconcile(previous, rebuilt, RackModuleCustomizationPolicy.Preserve);

            var source = previous.First(module => module.IsHeader && !module.UseCalculatedHeaderConfiguration);
            var header = rebuilt.Modules.First(module => module.IsHeader);
            Assert.NotSame(source.HeaderConfiguration, header.AssociatedFrameConfiguration);

            header.AssociatedFrameConfiguration.PanelClear = 12.0;
            Assert.Equal(marker, source.HeaderConfiguration.PanelClear, 4);
        }

        [Fact]
        public void Reconciliation_AShorterRack_DropsTheExtraIntents_InsteadOfForcingThem()
        {
            var catalog = Catalog;
            var reconciliation = new RackModuleReconciliation(new DynamicRackSystemBuilder(catalog));
            var previous = RackModuleEditSession.Begin(StandardSystem(palletsDeep: 10)).Commit();
            var rebuilt = StandardSystem(palletsDeep: 4);

            var expectedDrop =
                previous.Count(module => module.IsHeader) - rebuilt.Modules.Count(module => module.IsHeader);
            Assert.True(expectedDrop > 0, "the fixture must actually shrink the rack");

            var result = reconciliation.Reconcile(previous, rebuilt, RackModuleCustomizationPolicy.Preserve);

            Assert.Equal(expectedDrop, result.IntentsDropped);
        }

        [Fact]
        public void Reconciliation_ACalculatedCabecera_IsNeverTouched_SoTheRebuiltHeightStands()
        {
            var catalog = Catalog;
            var reconciliation = new RackModuleReconciliation(new DynamicRackSystemBuilder(catalog));
            var previous = RackModuleEditSession.Begin(StandardSystem()).Commit();
            var rebuilt = StandardSystem(palletsDeep: 8);
            var expected = rebuilt.Modules
                .Where(module => module.IsHeader)
                .Select(module => module.AssociatedFrameConfiguration.Height)
                .ToList();

            var result = reconciliation.Reconcile(previous, rebuilt, RackModuleCustomizationPolicy.Preserve);

            Assert.Equal(0, result.ConfigurationsPreserved);
            Assert.Equal(0, result.FondosRestored);
            Assert.Equal(
                expected,
                rebuilt.Modules
                    .Where(module => module.IsHeader)
                    .Select(module => module.AssociatedFrameConfiguration.Height));
        }

        // ===== Neutrality: the foundation must not know about systems ===========================================

        [Fact]
        public void TheFoundation_IsNeutral_NoTypeMentionsRackSystemKind()
        {
            var types = new[]
            {
                typeof(RackModuleDescriptor),
                typeof(RackModuleEditSession),
                typeof(RackModuleReconciliation),
                typeof(RackModuleReconciliationResult)
            };

            foreach (var type in types)
            {
                var signatures = type.GetMethods()
                    .SelectMany(method => method.GetParameters()
                        .Select(parameter => parameter.ParameterType)
                        .Concat(new[] { method.ReturnType }))
                    .Concat(type.GetProperties().Select(property => property.PropertyType))
                    .Concat(type.GetConstructors().SelectMany(c => c.GetParameters().Select(p => p.ParameterType)));

                Assert.DoesNotContain(signatures, signature => signature == typeof(RackSystemKind));
            }
        }

        // ===== Fixtures ========================================================================================

        private static RackFrameConfiguration CustomHeader(double panelClear)
        {
            var catalog = Catalog;
            var configuration = new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default,
                catalog.Defaults?.Post,
                120.0,
                48.0);
            configuration.PanelClear = panelClear;
            return configuration;
        }

        /// <summary>Module intents whose first header carries BOTH a manual fondo and a custom cabecera — the two
        /// things the base's fondo-only snapshot cannot carry together.</summary>
        private static System.Collections.Generic.IReadOnlyList<DynamicRackModuleDesign> IntentsWithACustomHeader(
            out double marker,
            out double manualFondo)
        {
            marker = 40.0;
            manualFondo = 55.0;

            var session = RackModuleEditSession.Begin(StandardSystem());
            var headerId = session.Modules.First(module => module.IsHeader).ModuleId;
            session.SetLength(headerId, manualFondo);
            session.SetHeaderConfiguration(headerId, CustomHeader(marker));
            return session.Commit();
        }

        private static FrameExceptionOverride RuntimeException()
            => new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.SpecialClear,
                TargetId = "H1",
                StandardValue = "44",
                OverrideValue = "40",
                Reason = "runtime-only override"
            };
    }
}
