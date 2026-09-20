using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.ProjectVariables;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>WPF-only characterization of the one-box and focus contract inherited from I-48.</summary>
    public sealed class G10LegacyEditorControlCharacterizationTests
    {
        [Fact, Trait("Gate", "G10-Legacy")]
        public void LINKED_PROPERTY_EDITOR_HAS_EXACTLY_ONE_TEXT_INPUT_SURFACE()
        {
            var count = StaTestRunner.Run(() =>
            {
                var editor = Editor();
                return Descendants(editor).OfType<TextBox>().Count();
            });
            Assert.Equal(1, count);
        }

        [Fact, Trait("Gate", "G10-Legacy")]
        public void FOCUS_MOVING_INSIDE_AUTOCOMPLETE_DOES_NOT_COMMIT_OR_CANCEL_DRAFT()
        {
            var result = StaTestRunner.Run(() =>
            {
                var editor = Editor();
                editor.Box.Text = "=A";
                editor.HandleFocusLeaving(editor.Candidates);
                return (editor.Session.Text, editor.Session.IsDirty, editor.FinalState.Source.Kind);
            });
            Assert.Equal("=A", result.Text);
            Assert.True(result.IsDirty);
            Assert.Equal(LinkedPropertySourceKind.Literal, result.Kind);
        }

        private static LinkedPropertyEditor Editor()
        {
            var id = VariableId.Parse("3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44");
            var editor = new LinkedPropertyEditor();
            var window = new Window { Content = editor };
            window.Show(); window.Hide();
            editor.Attach(new LinkedPropertyEditSession(LinkedPropertyEditState.Literal(6), new[]
            {
                new LinkedPropertyOption(id, "A", VariableType.Length, 10),
            }));
            return editor;
        }

        private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            {
                var child = VisualTreeHelper.GetChild(root, index);
                yield return child;
                foreach (var nested in Descendants(child)) yield return nested;
            }
        }
    }
}
