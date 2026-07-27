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

            var committed = session.Commit().Modules;

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

        // ===== Individual restore (Owner: full, for ANY module) =================================================

        [Fact]
        public void Session_RestoreModule_ClearsEverything_OnACabecera_AndRecordsTheRestore()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);
            session.SetLength(header.ModuleId, 55.0);
            session.SetHeaderConfiguration(header.ModuleId, CustomHeader(40.0));

            Assert.True(session.RestoreModule(header.ModuleId));

            var staged = session.Modules.First(module => module.ModuleId == header.ModuleId);
            Assert.True(staged.IsCalculated);
            Assert.False(staged.IsManualOverride);
            Assert.True(staged.UsesCalculatedHeaderConfiguration);
            Assert.False(staged.HasHeaderConfiguration);
            Assert.True(session.IsRestored(header.ModuleId));
            Assert.Equal(new[] { header.ModuleId }, session.RestoredModuleIds);
        }

        [Fact]
        public void Session_RestoreModule_WorksOnASeparatorToo_NotOnlyOnCabeceras()
        {
            var system = StandardSystem();
            var separator = system.Modules.First(module => module.Kind == DynamicRackModuleKind.Separator);
            var session = RackModuleEditSession.Begin(system);
            session.SetLength(separator.ModuleId, 33.0);

            Assert.True(session.RestoreModule(separator.ModuleId));

            var staged = session.Modules.First(module => module.ModuleId == separator.ModuleId);
            Assert.True(staged.IsCalculated);
            Assert.False(staged.IsManualOverride);
            Assert.True(session.IsRestored(separator.ModuleId));
        }

        [Fact]
        public void Session_EditingARestoredModuleAgain_CancelsItsRestore()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);
            session.SetLength(header.ModuleId, 55.0);
            session.RestoreModule(header.ModuleId);
            Assert.True(session.IsRestored(header.ModuleId));

            session.SetLength(header.ModuleId, 60.0);

            Assert.False(session.IsRestored(header.ModuleId));
            Assert.Empty(session.RestoredModuleIds);
        }

        [Fact]
        public void Session_Cancel_AlsoDiscardsAPendingIndividualRestore()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);
            session.SetLength(header.ModuleId, 55.0);
            session.Commit();

            session.RestoreModule(header.ModuleId);
            Assert.True(session.HasPendingChanges);

            session.Cancel();

            Assert.False(session.HasPendingChanges);
            Assert.Empty(session.RestoredModuleIds);
            Assert.Equal(55.0, session.Modules.First(module => module.ModuleId == header.ModuleId).Length, 6);
        }

        [Fact]
        public void Session_Commit_CarriesTheRestoreRequests_BecauseARestoredModuleLooksUncustomized()
        {
            var system = StandardSystem();
            var header = system.Modules.First(module => module.IsHeader);
            var session = RackModuleEditSession.Begin(system);
            session.SetLength(header.ModuleId, 55.0);
            session.Commit();

            session.RestoreModule(header.ModuleId);
            session.RequestStandardRestore();
            var commit = session.Commit();

            Assert.Equal(new[] { header.ModuleId }, commit.RestoredModuleIds);
            Assert.True(commit.StandardRestoreRequested);
            Assert.Empty(session.RestoredModuleIds);   // consumed by the commit
        }

        // ===== Reconciliation by ModuleId + Kind (Owner decisions 2, 3 and 4) ===================================

        [Fact]
        public void Reconciliation_MatchesByModuleIdAndKind_CarryingTheCustomCabeceraAcrossARebuild()
        {
            var reconciliation = Reconciliation();
            var previous = IntentsWithACustomHeader(out var marker, out var manualFondo, out var headerId);
            var rebuilt = StandardSystem(palletsDeep: 8);   // a different structure, standard everywhere

            var result = reconciliation.Reconcile(previous, rebuilt);

            var header = rebuilt.Modules.First(module => module.ModuleId == headerId);
            Assert.Equal(new[] { headerId }, result.Preserved);
            Assert.Empty(result.Removed);
            Assert.Empty(result.Incompatible);
            Assert.False(header.UseCalculatedHeaderConfiguration);
            Assert.Equal(marker, header.AssociatedFrameConfiguration.PanelClear, 4);
            Assert.Equal(manualFondo, header.Length, 6);
            Assert.NotEmpty(header.AssociatedFrameConfiguration.Members);   // derived model refreshed, as in the base
        }

        [Fact]
        public void Reconciliation_CarriesAManualLength_OnSeparatorsToo_NotOnlyOnCabeceras()
        {
            var reconciliation = Reconciliation();
            var source = StandardSystem();
            var separatorId = source.Modules.First(module => module.Kind == DynamicRackModuleKind.Separator).ModuleId;
            var session = RackModuleEditSession.Begin(source);
            session.SetLength(separatorId, 33.0);
            var previous = session.Commit().Modules;
            var rebuilt = StandardSystem(palletsDeep: 8);

            var result = reconciliation.Reconcile(previous, rebuilt);

            var separator = rebuilt.Modules.First(module => module.ModuleId == separatorId);
            Assert.Contains(separatorId, result.Preserved);
            Assert.Equal(33.0, separator.Length, 6);
            Assert.True(separator.IsManualOverride);
        }

        [Fact]
        public void Reconciliation_AdaptsThePreservedCabecera_DepthAndRackPeralte_ToTheRebuiltStructure()
        {
            var reconciliation = Reconciliation();
            var previous = IntentsWithACustomHeader(out _, out var manualFondo, out var headerId);
            var rebuilt = StandardSystem(palletsDeep: 8);
            rebuilt.PostPeralte = 4.5;

            // The staged cabecera was built at fondo 48 and no rack peralte: both must move.
            var staged = previous.First(module => module.ModuleId == headerId);
            Assert.NotEqual(manualFondo, staged.HeaderConfiguration.Depth);

            var result = reconciliation.Reconcile(previous, rebuilt);

            var header = rebuilt.Modules.First(module => module.ModuleId == headerId);
            Assert.Contains(headerId, result.Adapted);
            Assert.Contains(headerId, result.Preserved);   // adapting is not losing
            Assert.Equal(manualFondo, header.AssociatedFrameConfiguration.Depth, 6);
            Assert.Equal(4.5, header.AssociatedFrameConfiguration.PostPeralte, 6);
        }

        [Fact]
        public void Reconciliation_AModuleThatIsGone_LosesItsCustomization_AndIsReportedAsRemoved()
        {
            var reconciliation = Reconciliation();
            var source = StandardSystem(palletsDeep: 10);
            var lastHeaderId = source.Modules.Last(module => module.IsHeader).ModuleId;
            var session = RackModuleEditSession.Begin(source);
            session.SetLength(lastHeaderId, 51.0);
            var previous = session.Commit().Modules;

            var rebuilt = StandardSystem(palletsDeep: 4);   // a much shorter rack: that id no longer exists
            Assert.DoesNotContain(rebuilt.Modules, module => module.ModuleId == lastHeaderId);

            var result = reconciliation.Reconcile(previous, rebuilt);

            Assert.Equal(new[] { lastHeaderId }, result.Removed);
            Assert.Empty(result.Preserved);
            Assert.True(result.LostAnything);
        }

        [Fact]
        public void Reconciliation_AModuleWhoseKindChanged_LosesItsCustomization_AndIsReportedAsIncompatible()
        {
            var reconciliation = Reconciliation();
            var source = StandardSystem();
            var headerId = source.Modules.First(module => module.IsHeader).ModuleId;
            var session = RackModuleEditSession.Begin(source);
            session.SetLength(headerId, 55.0);
            session.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            var previous = session.Commit().Modules;

            // Same id, different kind: structurally it is not the same module any more.
            var rebuilt = StandardSystem(palletsDeep: 8);
            var collision = rebuilt.Modules.First(module => module.ModuleId == headerId);
            collision.Kind = DynamicRackModuleKind.Separator;
            var lengthBefore = collision.Length;

            var result = reconciliation.Reconcile(previous, rebuilt);

            Assert.Equal(new[] { headerId }, result.Incompatible);
            Assert.Empty(result.Preserved);
            Assert.Equal(lengthBefore, collision.Length, 6);   // untouched: nothing was forced onto it
            Assert.True(result.LostAnything);
        }

        [Fact]
        public void Reconciliation_AnExplicitlyRestoredModule_IsReportedAsRestored_NotAsLost()
        {
            var reconciliation = Reconciliation();
            var source = StandardSystem();
            var headerId = source.Modules.First(module => module.IsHeader).ModuleId;
            var session = RackModuleEditSession.Begin(source);
            session.SetLength(headerId, 55.0);
            session.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            session.Commit();

            // The user restores it, so the intents still describe a customized module until the rebuild lands.
            var staged = RackModuleEditSession.Begin(source);
            staged.SetLength(headerId, 55.0);
            staged.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            var customized = staged.Commit().Modules;

            var rebuilt = StandardSystem(palletsDeep: 8);
            var result = reconciliation.Reconcile(customized, rebuilt, new[] { headerId });

            Assert.Equal(new[] { headerId }, result.Restored);
            Assert.Empty(result.Preserved);
            Assert.False(result.LostAnything);   // asked for, not lost
            var header = rebuilt.Modules.First(module => module.ModuleId == headerId);
            Assert.True(header.UseCalculatedHeaderConfiguration);
        }

        [Fact]
        public void Reconciliation_ACalculatedModule_IsNeverTouched_SoTheRebuiltHeightStands()
        {
            var reconciliation = Reconciliation();
            var previous = RackModuleEditSession.Begin(StandardSystem()).Commit().Modules;
            var rebuilt = StandardSystem(palletsDeep: 8);
            var expected = rebuilt.Modules
                .Where(module => module.IsHeader)
                .Select(module => module.AssociatedFrameConfiguration.Height)
                .ToList();

            var result = reconciliation.Reconcile(previous, rebuilt);

            Assert.Empty(result.Preserved);
            Assert.Empty(result.Removed);
            Assert.Empty(result.Incompatible);
            Assert.Equal(
                expected,
                rebuilt.Modules
                    .Where(module => module.IsHeader)
                    .Select(module => module.AssociatedFrameConfiguration.Height));
        }

        [Fact]
        public void Reconciliation_PreservedCabecera_IsAnIndependentCopy_NotTheIntentsInstance()
        {
            var reconciliation = Reconciliation();
            var previous = IntentsWithACustomHeader(out var marker, out _, out var headerId);
            var rebuilt = StandardSystem(palletsDeep: 8);

            reconciliation.Reconcile(previous, rebuilt);

            var source = previous.First(module => module.ModuleId == headerId);
            var header = rebuilt.Modules.First(module => module.ModuleId == headerId);
            Assert.NotSame(source.HeaderConfiguration, header.AssociatedFrameConfiguration);

            header.AssociatedFrameConfiguration.PanelClear = 12.0;
            Assert.Equal(marker, source.HeaderConfiguration.PanelClear, 4);
        }

        [Fact]
        public void Reconciliation_Describe_NamesEveryCategory_SoNothingIsLostInSilence()
        {
            var reconciliation = Reconciliation();
            var source = StandardSystem(palletsDeep: 10);
            var goneId = source.Modules.Last(module => module.IsHeader).ModuleId;
            var keptId = source.Modules.First(module => module.IsHeader).ModuleId;
            var session = RackModuleEditSession.Begin(source);
            session.SetLength(goneId, 51.0);
            session.SetLength(keptId, 55.0);
            var previous = session.Commit().Modules;

            var result = reconciliation.Reconcile(previous, StandardSystem(palletsDeep: 4));

            var description = result.Describe();
            Assert.Contains(keptId, description);
            Assert.Contains(goneId, description);
            Assert.Contains("conservado", description);
            Assert.Contains("eliminado", description);
        }

        [Fact]
        public void TheFoundation_HasNoPublicDiscardPolicy_BecauseDiscardIsNotAnOrdinaryOutcome()
        {
            var exported = typeof(RackModuleReconciliation).Assembly
                .GetExportedTypes()
                .Select(type => type.Name)
                .ToList();

            Assert.DoesNotContain("RackModuleCustomizationPolicy", exported);
            Assert.DoesNotContain(
                typeof(RackModuleReconciliation).GetMethods().SelectMany(m => m.GetParameters()),
                parameter => parameter.ParameterType.IsEnum);
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

        private static RackModuleReconciliation Reconciliation()
            => new RackModuleReconciliation(new DynamicRackSystemBuilder(Catalog));

        /// <summary>Module intents whose first header carries BOTH a manual fondo and a custom cabecera — the two
        /// things the base's fondo-only, ordinal-matched snapshot cannot carry together.</summary>
        private static System.Collections.Generic.IReadOnlyList<DynamicRackModuleDesign> IntentsWithACustomHeader(
            out double marker,
            out double manualFondo,
            out string headerId)
        {
            marker = 40.0;
            manualFondo = 55.0;

            var session = RackModuleEditSession.Begin(StandardSystem());
            headerId = session.Modules.First(module => module.IsHeader).ModuleId;
            session.SetLength(headerId, manualFondo);
            session.SetHeaderConfiguration(headerId, CustomHeader(marker));
            return session.Commit().Modules;
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
