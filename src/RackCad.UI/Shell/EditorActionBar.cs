using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// The common action bar: four NEUTRAL, position-named categories — <see cref="LeadingActions"/>,
    /// <see cref="SecondaryActions"/>, <see cref="PrimaryActions"/>, <see cref="TrailingActions"/> — that the editor
    /// fills with its own content. It hardcodes NO operation and no per-system enum; it only lays the categories out in
    /// order and, being a <see cref="WrapPanel"/>, never clips: at the minimum width the categories flow to a new line
    /// instead of being cut off. An empty category collapses (no gap). Disabled-with-reason is a property of the
    /// buttons the editor injects (see <see cref="EditorActions.Button"/>), not of this bar.
    /// </summary>
    public class EditorActionBar : WrapPanel
    {
        public static readonly DependencyProperty LeadingActionsProperty = Register(nameof(LeadingActions), 0);
        public static readonly DependencyProperty SecondaryActionsProperty = Register(nameof(SecondaryActions), 1);
        public static readonly DependencyProperty PrimaryActionsProperty = Register(nameof(PrimaryActions), 2);
        public static readonly DependencyProperty TrailingActionsProperty = Register(nameof(TrailingActions), 3);

        private readonly ContentPresenter[] slots;

        public EditorActionBar()
        {
            Orientation = Orientation.Horizontal;
            slots = new ContentPresenter[4];
            for (var i = 0; i < slots.Length; i++)
            {
                slots[i] = new ContentPresenter { Visibility = Visibility.Collapsed };
                Children.Add(slots[i]); // fixed order: leading, secondary, primary, trailing
            }
        }

        public object LeadingActions { get => GetValue(LeadingActionsProperty); set => SetValue(LeadingActionsProperty, value); }
        public object SecondaryActions { get => GetValue(SecondaryActionsProperty); set => SetValue(SecondaryActionsProperty, value); }
        public object PrimaryActions { get => GetValue(PrimaryActionsProperty); set => SetValue(PrimaryActionsProperty, value); }
        public object TrailingActions { get => GetValue(TrailingActionsProperty); set => SetValue(TrailingActionsProperty, value); }

        /// <summary>The four category hosts in canonical order (leading, secondary, primary, trailing) — a test seam.</summary>
        internal IReadOnlyList<ContentPresenter> CategoryPresenters => slots;

        private static DependencyProperty Register(string name, int slotIndex)
            => DependencyProperty.Register(name, typeof(object), typeof(EditorActionBar),
                new PropertyMetadata(null, (d, e) => ((EditorActionBar)d).SetSlot(slotIndex, e.NewValue)));

        private void SetSlot(int index, object content)
        {
            slots[index].Content = content;
            slots[index].Visibility = content == null ? Visibility.Collapsed : Visibility.Visible; // empty category → no gap
        }
    }
}
