using System.Linq;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The I-48 editor behavior that G10 must preserve while adding expressions.</summary>
    public sealed class G10LegacyEditorCharacterizationTests
    {
        private static readonly VariableId A = VariableId.Parse("3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44");

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_LITERAL_TYPING_IS_DRAFT_UNTIL_EXPLICIT_ENTER()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("8.5");
            Assert.Equal(6, session.Committed.CommittedLiteral);
            Assert.Equal(LinkedPropertyDraftKind.DraftLiteral, session.Draft);
            Assert.True(session.TryCommitByEnter(out _));
            Assert.Equal(8.5, session.Committed.CommittedLiteral);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_DIRECT_REFERENCE_SELECTION_COMMITS_EXACT_IDENTITY()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A");
            Assert.True(session.TrySelect(A, out _));
            Assert.Equal(LinkedPropertySourceKind.ProjectVariableReference, session.Committed.Source.Kind);
            Assert.Equal(A, session.Committed.Source.VariableId);
            Assert.Equal(6, session.Committed.CommittedLiteral);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_AUTOCOMPLETE_FILTERS_WITHOUT_AUTOSELECT_OR_MUTATION()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=a");
            Assert.Single(session.Candidates);
            Assert.Equal(LinkedPropertySourceKind.Literal, session.Committed.Source.Kind);
            Assert.True(session.IsDirty);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_ESCAPE_RESTORES_COMMITTED_LITERAL_AND_REFERENCE()
        {
            var literal = Session(LinkedPropertyEditState.Literal(6));
            literal.Type("garbage"); literal.Cancel();
            Assert.Equal("6", literal.Text);

            var reference = Session(LinkedPropertyEditState.Reference(6, A));
            reference.Type("9"); reference.Cancel();
            Assert.Equal("=A", reference.Text);
            Assert.Equal(A, reference.Committed.Source.VariableId);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_INVALID_DIRECT_REFERENCE_STAYS_PENDING_AND_UNCOMMITTED()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=Unknown");
            Assert.False(session.TryCommitByEnter(out var error));
            Assert.Contains("lista", error);
            Assert.Equal(LinkedPropertySourceKind.Literal, session.Committed.Source.Kind);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_LOSTFOCUS_NEVER_CHANGES_SOURCE_KIND()
        {
            var literal = Session(LinkedPropertyEditState.Literal(6));
            literal.Type("=A");
            Assert.False(literal.TryCommitByLostFocus(out _));
            Assert.Equal(LinkedPropertySourceKind.Literal, literal.Committed.Source.Kind);

            var reference = Session(LinkedPropertyEditState.Reference(6, A));
            reference.Type("7");
            Assert.False(reference.TryCommitByLostFocus(out _));
            Assert.Equal(LinkedPropertySourceKind.ProjectVariableReference, reference.Committed.Source.Kind);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_C4_STAGES_WITHOUT_APPLYING_AND_BLOCKS_SOURCE_CHANGES()
        {
            var literal = Session(LinkedPropertyEditState.Literal(6));
            literal.Type("7");
            Assert.Equal(LinkedPropertyStageOutcome.Ready, literal.TryStage().Outcome);
            Assert.Equal(6, literal.Committed.CommittedLiteral);

            var reference = Session(LinkedPropertyEditState.Reference(6, A));
            reference.Type("7");
            Assert.Equal(LinkedPropertyStageOutcome.Blocked, reference.TryStage().Outcome);
            Assert.Equal(A, reference.Committed.Source.VariableId);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void LEGACY_EDITOR_STILL_SERVES_THE_TWO_PRODUCTIVE_LINKED_PROPERTIES()
        {
            Assert.Equal(2, SelectiveLinkedProperties.All.Count);
            Assert.Equal(new[] { "selective.palletTolerance", "selective.verticalClearance" },
                SelectiveLinkedProperties.All.Ordered()
                    .Select(item => item.PropertyId.Value).OrderBy(value => value).ToArray());
        }

        private static LinkedPropertyEditSession Session(LinkedPropertyEditState state)
            => new LinkedPropertyEditSession(state, new[]
            {
                new LinkedPropertyOption(A, "A", VariableType.Length, 10),
            });
    }
}
