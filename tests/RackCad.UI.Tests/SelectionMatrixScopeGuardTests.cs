using System.Collections.Generic;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-34 — the guard against an UNDEFINED <see cref="SelectionMatrixScope"/> value. A C# enum is just an int, so a
    /// cast, a stale persisted number or a future member added without revisiting these switches can carry a value none
    /// of them knows. Both sites used to answer that with their <c>default:</c> branch, which meant "Todo":
    /// <see cref="SelectionMatrixModel.ApplyScope"/> would rewrite EVERY cell of the grid and
    /// <see cref="SelectionMatrixScopeLabels.For"/> would caption the unknown scope "Todo".
    /// <para>
    /// Silently widening a scope to the whole grid is the worst possible failure for a safety matrix — it is exactly
    /// the mass edit the user did not ask for. The rule is now fail-closed: an unknown scope mutates nothing, notifies
    /// nothing and has no caption of its own.
    /// </para>
    /// </summary>
    public sealed class SelectionMatrixScopeGuardTests
    {
        /// <summary>Values no member of the enum defines: past the last one, negative, and a large cast.</summary>
        public static IEnumerable<object[]> UndefinedScopes => new[]
        {
            new object[] { (SelectionMatrixScope)4 },
            new object[] { (SelectionMatrixScope)(-1) },
            new object[] { (SelectionMatrixScope)99 }
        };

        [Theory]
        [MemberData(nameof(UndefinedScopes))]
        public void ApplyScope_WithAnUndefinedScope_ChangesNothingAndNotifiesNothing(SelectionMatrixScope undefined)
        {
            var model = new SelectionMatrixModel(4, 3); // all ON
            var scopeEvents = 0;
            var cellEvents = 0;
            var bulkEvents = 0;
            model.ScopeApplied += (_, __) => scopeEvents++;
            model.CellChanged += (_, __) => cellEvents++;
            model.BulkChanged += (_, __) => bulkEvents++;

            var changed = model.ApplyScope(undefined, 1, 1, value: false);

            Assert.Empty(changed);
            Assert.Equal(12, model.SelectedCount); // NOT one cell was rewritten
            Assert.Equal(0, model.UnselectedCount);
            Assert.Equal(0, scopeEvents);
            Assert.Equal(0, cellEvents);
            Assert.Equal(0, bulkEvents);
        }

        /// <summary>The same guard when the grid is already partly OFF: an unknown scope must not "repair" it either.</summary>
        [Fact]
        public void ApplyScope_WithAnUndefinedScope_LeavesAPartlyDisabledGridExactlyAsItWas()
        {
            var model = SelectionMatrixModel.WithUnselected(
                3, 3, new[] { new SelectionMatrixCell(0, 0), new SelectionMatrixCell(2, 1) });
            var raised = 0;
            model.ScopeApplied += (_, __) => raised++;

            Assert.Empty(model.ApplyScope((SelectionMatrixScope)7, 0, 0, value: true));

            Assert.Equal(2, model.UnselectedCount);
            Assert.False(model.IsSelected(0, 0));
            Assert.False(model.IsSelected(2, 1));
            Assert.Equal(0, raised);
        }

        [Theory]
        [MemberData(nameof(UndefinedScopes))]
        public void Labels_ForAnUndefinedScope_DoNotBorrowTheAllCaption(SelectionMatrixScope undefined)
        {
            foreach (var labels in new[] { SelectionMatrixScopeLabels.ByFrente, SelectionMatrixScopeLabels.ByPoste })
            {
                var caption = labels.For(undefined);

                Assert.NotEqual(labels.AllLabel, caption);
                Assert.NotEqual("Todo", caption);
                Assert.True(string.IsNullOrEmpty(caption)); // no caption at all, rather than a misleading one
            }
        }

        /// <summary>The four DEFINED members keep their captions exactly as the dialogs declare them.</summary>
        [Fact]
        public void Labels_ForTheDefinedScopes_AreUnchanged()
        {
            var porPoste = SelectionMatrixScopeLabels.ByPoste;

            Assert.Equal("Celda", porPoste.For(SelectionMatrixScope.Cell));
            Assert.Equal("Nivel", porPoste.For(SelectionMatrixScope.Row));
            Assert.Equal("Poste", porPoste.For(SelectionMatrixScope.Column));
            Assert.Equal("Todo", porPoste.For(SelectionMatrixScope.All));
        }

        /// <summary>The editor already refused an undeclared scope; this pins that an UNDEFINED one is refused too,
        /// and for the same visible reason, so the two guards cannot drift apart.</summary>
        [Theory]
        [MemberData(nameof(UndefinedScopes))]
        public void Editor_RefusesAnUndefinedScope(SelectionMatrixScope undefined)
        {
            var model = new SelectionMatrixModel(3, 2);
            var editor = new SelectionMatrixBulkEditor(model);
            editor.TrySetPrimary(1, 1);

            Assert.False(editor.Supports(undefined));
            Assert.False(editor.CanApply(undefined));
            Assert.False(string.IsNullOrWhiteSpace(editor.DisabledReason(undefined)));
            Assert.Empty(editor.Apply(undefined, activate: false));
            Assert.Equal(6, model.SelectedCount);
        }
    }
}
