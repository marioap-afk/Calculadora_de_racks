using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14: one row per frozen situation of Proposal V5 section 3.8. No failure falls back to "open it with
    /// RACKEDITAR" and every message is plain command line text.
    /// </summary>
    public class RackProjectionRemedyTests
    {
        [Fact]
        public void REMEDY_FOR_A_MISSING_VARIANT_DEPENDS_ON_HAVING_ANOTHER_VALID_VIEW()
        {
            Assert.Equal(
                RackProjectionRemedyKind.UpdateFromAnotherView,
                Remedy(RackProjectionFailureCode.SourceAddressUnavailable, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.InsertAnotherView,
                Remedy(RackProjectionFailureCode.SourceAddressUnavailable, hasAnother: false).Kind);
        }

        [Fact]
        public void REMEDY_FOR_AN_UNEXPOSED_PAIR_IS_THAT_THE_VIEW_CANNOT_BE_PROJECTED()
        {
            Assert.Equal(
                RackProjectionRemedyKind.NotProjectable,
                Remedy(RackProjectionFailureCode.PairNotExposed, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.NotProjectable,
                Remedy(RackProjectionFailureCode.TargetNotExposed, hasAnother: true).Kind);
        }

        [Fact]
        public void REMEDY_FOR_AUTHORED_AND_PROPERTIES_GATES_STAYS_SEPARATE()
        {
            Assert.Equal(
                RackProjectionRemedyKind.UpdateFromPreferredView,
                Remedy(RackProjectionFailureCode.AuthoredDivergent, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.UpdateFromPreferredView,
                Remedy(RackProjectionFailureCode.AuthoredUnreadable, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.UnifyProperties,
                Remedy(RackProjectionFailureCode.PropertiesDivergent, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.UnifyProperties,
                Remedy(RackProjectionFailureCode.PropertiesUnreadable, hasAnother: true).Kind);
        }

        [Fact]
        public void REMEDY_FOR_RESOLVE_OUTCOMES_NAMES_THE_REAL_AUTHORITY()
        {
            Assert.Equal(
                RackProjectionRemedyKind.FixDesignInEditor,
                Remedy(RackProjectionFailureCode.ResolveOutputBlocking, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.RepairVariable,
                Remedy(RackProjectionFailureCode.ResolveBrokenReference, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.SectionCatalogUnavailable,
                Remedy(RackProjectionFailureCode.ResolveDependencyUnavailable, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.UnreadableByThisBuild,
                Remedy(RackProjectionFailureCode.ResolveUnreadable, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.UpdateThisView,
                Remedy(RackProjectionFailureCode.ResolveLegacyUnsupported, hasAnother: true, kind: RackSystemKind.PalletFlow).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.NotProjectable,
                Remedy(RackProjectionFailureCode.ResolveLegacyUnsupported, hasAnother: true, kind: RackSystemKind.Selective).Kind);
        }

        [Fact]
        public void REMEDY_FOR_AN_XREF_SIBLING_POINTS_AT_THE_SOURCE_DRAWING()
        {
            Assert.Equal(
                RackProjectionRemedyKind.FixInXrefSource,
                RackProjectionRemedySelector.For(
                    RackProjectionFailureCode.EditPreflightRejected,
                    RackSystemKind.SelectiveRack,
                    hasAnotherValidSiblingView: true,
                    isXrefDependent: true).Kind);
        }

        [Fact]
        public void REMEDY_FOR_PUSH_BACK_AND_CANTILEVER_INVALID_VIEWS_ASKS_TO_PURGE_THE_DEFINITION()
        {
            Assert.Equal(
                RackProjectionRemedyKind.PurgeAndReinsert,
                Remedy(RackProjectionFailureCode.EditPreflightRejected, hasAnother: true, kind: RackSystemKind.PushBack).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.NoAutomaticRemedy,
                Remedy(RackProjectionFailureCode.EditPreflightRejected, hasAnother: false, kind: RackSystemKind.Cantilever).Kind);
        }

        [Fact]
        public void REMEDY_FOR_A_SELECTION_PROBLEM_ASKS_TO_REVIEW_THE_SELECTION()
        {
            Assert.Equal(
                RackProjectionRemedyKind.ReviewSelection,
                Remedy(RackProjectionFailureCode.MixedSourceTypes, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.ReviewSelection,
                Remedy(RackProjectionFailureCode.MultipleSourceDefinitions, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.ReviewSelection,
                Remedy(RackProjectionFailureCode.UnsupportedMember, hasAnother: true).Kind);
            Assert.Equal(
                RackProjectionRemedyKind.NoAutomaticRemedy,
                Remedy(RackProjectionFailureCode.BlankRackId, hasAnother: true).Kind);
        }

        [Fact]
        public void REMEDY_FOR_LIBRARY_PIECES_NAMES_THE_LIBRARY_NOT_RACKEDITAR()
        {
            foreach (var code in new[]
            {
                RackProjectionFailureCode.RequiredKeyMissing,
                RackProjectionFailureCode.RequiredBlockMissing,
                RackProjectionFailureCode.RequiredLibraryMissing,
                RackProjectionFailureCode.RequiredUnknownAvailability,
                RackProjectionFailureCode.UnknownSourceRole,
            })
            {
                var remedy = Remedy(code, hasAnother: true);
                Assert.Equal(RackProjectionRemedyKind.LibraryBlocksMissing, remedy.Kind);
                Assert.DoesNotContain("RACKEDITAR", remedy.Message);
            }
        }

        [Fact]
        public void REMEDY_FOR_A_TRANSFORM_ID19_DOES_NOT_ADMIT_IS_NOT_PROJECTABLE()
        {
            foreach (var code in new[]
            {
                RackProjectionFailureCode.ReflectionNotAllowed,
                RackProjectionFailureCode.NonUnitScale,
                RackProjectionFailureCode.NonUniformScale,
                RackProjectionFailureCode.NegativeZScale,
                RackProjectionFailureCode.NormalNotWorldZ,
                RackProjectionFailureCode.NonParallelSources,
            })
            {
                Assert.Equal(RackProjectionRemedyKind.NotProjectable, Remedy(code, hasAnother: true).Kind);
            }
        }

        [Fact]
        public void EVERY_REMEDY_CARRIES_A_MESSAGE_WITHOUT_ACCENTS()
        {
            foreach (RackProjectionFailureCode code in System.Enum.GetValues(typeof(RackProjectionFailureCode)))
            {
                if (code == RackProjectionFailureCode.None)
                {
                    continue;
                }

                var remedy = Remedy(code, hasAnother: true);
                Assert.NotEqual(string.Empty, remedy.Message);
                Assert.DoesNotContain('á', remedy.Message);
                Assert.DoesNotContain('é', remedy.Message);
                Assert.DoesNotContain('í', remedy.Message);
                Assert.DoesNotContain('ó', remedy.Message);
                Assert.DoesNotContain('ú', remedy.Message);
            }
        }

        private static RackProjectionRemedy Remedy(
            RackProjectionFailureCode code,
            bool hasAnother,
            RackSystemKind kind = RackSystemKind.SelectiveRack)
            => RackProjectionRemedySelector.For(code, kind, hasAnother);
    }
}
