using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Helpers to inspect the shared safety dialogs (I-32). They walk the LOGICAL tree, which exists from the
    /// constructor (each dialog assigns <c>Content</c> there), so a test can assert that a control is present or absent
    /// without showing a modal window — and an "absent" assertion is meaningful only because the sibling controls are
    /// found by the same walk.
    /// </summary>
    internal static class SafetyDialogTestSupport
    {
        /// <summary>Every logical descendant of <paramref name="root"/>, itself excluded.</summary>
        public static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is not DependencyObject node)
                {
                    continue;
                }

                yield return node;
                foreach (var deeper in Descendants(node))
                {
                    yield return deeper;
                }
            }
        }

        /// <summary>True when a "Lado:" label is offered (the aisle-face selector of the desviador/tope dialogs).</summary>
        public static bool HasSideSelector(DependencyObject root)
            => Descendants(root).OfType<TextBlock>().Any(text => text.Text == "Lado:");

        /// <summary>True when a checkbox with exactly <paramref name="content"/> is offered.</summary>
        public static bool HasCheckBox(DependencyObject root, string content)
            => Descendants(root).OfType<CheckBox>().Any(box => (box.Content as string) == content);

        /// <summary>True when a text block with exactly <paramref name="text"/> is present (a sanity anchor).</summary>
        public static bool HasText(DependencyObject root, string text)
            => Descendants(root).OfType<TextBlock>().Any(block => block.Text == text);
    }
}
