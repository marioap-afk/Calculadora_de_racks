#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI (INV-F1-T3B-08): the five selection contexts are presented deterministically. One(R) shows the
    /// exact RackId of the snapshot, a selection is labelled as the current selection and never as a project total, and
    /// the panel never invents or stamps a RackId.
    /// </summary>
    public class WorkspacePanelContextTests
    {
        private static WorkspacePanelViewModel Presented(SelectionContext context)
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(selection: context) };
            var panel = new WorkspacePanelViewModel(port, _ => { });
            panel.Refresh();
            return panel;
        }

        public static IEnumerable<object[]> TheFiveContexts() => new[]
        {
            new object[] { SelectionContext.None, SelectionContextKind.None },
            new object[] { SelectionContext.One("RACK-0042"), SelectionContextKind.One },
            new object[] { SelectionContext.Selection(3, 2), SelectionContextKind.Selection },
            new object[] { SelectionContext.NoIdentity, SelectionContextKind.NoIdentity },
            new object[] { SelectionContext.Diagnostic, SelectionContextKind.Diagnostic },
        };

        [Theory]
        [MemberData(nameof(TheFiveContexts))]
        public void EachContextIsPresentedWithItsKindAndATitleAndATextOfItsOwn(SelectionContext context, SelectionContextKind kind)
        {
            var panel = Presented(context);

            Assert.Equal(kind, panel.ContextKind);
            Assert.NotEmpty(panel.ContextTitle);
            Assert.NotEmpty(panel.ContextText);
        }

        [Fact]
        public void TheFiveContextsHaveFiveDifferentTitles()
        {
            var titles = TheFiveContexts().Select(args => Presented((SelectionContext)args[0]).ContextTitle).ToList();

            Assert.Equal(5, titles.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void NoneMeansNothingIsSelectedAndCarriesNoRackId()
        {
            var panel = Presented(SelectionContext.None);

            Assert.Null(panel.RackId);
            Assert.DoesNotContain("RACK", panel.ContextText, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void OneShowsTheExactRackIdOfTheSnapshot()
        {
            var panel = Presented(SelectionContext.One("RACK-0042"));

            Assert.Equal("RACK-0042", panel.RackId);
            Assert.Contains("RACK-0042", panel.ContextText);
        }

        [Fact]
        public void OneShowsWhateverIdTheSnapshotCarriesWithoutNormalizingIt()
        {
            var panel = Presented(SelectionContext.One("a-b_C.9"));

            Assert.Equal("a-b_C.9", panel.RackId);
            Assert.Contains("a-b_C.9", panel.ContextText);
        }

        [Fact]
        public void ASelectionShowsTheCountsAndIsLabelledAsTheCurrentSelectionNeverAsAProjectTotal()
        {
            var panel = Presented(SelectionContext.Selection(3, 2));
            var text = panel.ContextTitle + " " + panel.ContextText;

            Assert.Contains("3", panel.ContextText);
            Assert.Contains("2", panel.ContextText);
            Assert.Contains("selecci", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("total", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("proyecto", text, StringComparison.OrdinalIgnoreCase);
            Assert.Null(panel.RackId);
        }

        [Fact]
        public void ASelectionPresentsDifferentCountsDifferently()
        {
            var small = Presented(SelectionContext.Selection(1, 0));
            var large = Presented(SelectionContext.Selection(12, 34));

            Assert.NotEqual(small.ContextText, large.ContextText);
            Assert.Contains("12", large.ContextText);
            Assert.Contains("34", large.ContextText);
        }

        [Fact]
        public void NoIdentityAndDiagnosticCarryNoRackIdAndAreNotConfusedWithOne()
        {
            var noIdentity = Presented(SelectionContext.NoIdentity);
            var diagnostic = Presented(SelectionContext.Diagnostic);

            Assert.Null(noIdentity.RackId);
            Assert.Null(diagnostic.RackId);
            Assert.NotEqual(noIdentity.ContextText, diagnostic.ContextText);
        }

        [Fact]
        public void NoContextOtherThanOneEverCarriesARackId()
        {
            foreach (var args in TheFiveContexts().Where(a => (SelectionContextKind)a[1] != SelectionContextKind.One))
                Assert.Null(Presented((SelectionContext)args[0]).RackId);
        }

        [Fact]
        public void TheContextFollowsTheLatestSnapshot()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(selection: SelectionContext.One("R-1")) };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();
            Assert.Equal("R-1", panel.RackId);

            port.Snapshot = WorkspacePanelFixtures.Ready(selection: SelectionContext.Selection(2, 0));
            panel.Refresh();
            Assert.Equal(SelectionContextKind.Selection, panel.ContextKind);
            Assert.Null(panel.RackId);
        }

        [Fact]
        public void APanelWithoutADocumentPresentsNoContext()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.NoDocument() };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.Equal(SelectionContextKind.None, panel.ContextKind);
            Assert.Null(panel.RackId);
            Assert.NotEmpty(panel.ContextText);
        }
    }
}
