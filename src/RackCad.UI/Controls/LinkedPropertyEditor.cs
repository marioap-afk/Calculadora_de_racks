using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RackCad.Application.ProjectVariables;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// ONE field that edits a property whether a number or a project variable governs it (I-48 G4C).
    ///
    /// <para>
    /// It shows <c>6</c> for a literal and <c>=Holgura General</c> for a reference, and the same field edits
    /// both. There is no permanent Link/Unlink pair as the primary UX: a governed field stops being read-only
    /// (Proposal V2 R-11), so the user starts typing over <c>=Holgura General</c> the way they would over any
    /// number. Typing mutates NOTHING — it produces a draft.
    /// </para>
    /// <para>
    /// <b>The control owns no semantics.</b> Enter, Escape, LostFocus, the bare <c>=</c>, the refusal to
    /// auto-select a single candidate and the 20.13 freeze all live in
    /// <see cref="LinkedPropertyEditSession"/>, which is pure. What lives here is only what WPF has to do:
    /// paint the text, open a list, move a selection and report focus. It never consults the register, never
    /// resolves an id, never judges a type and never writes a binding.
    /// </para>
    /// <para>
    /// It is a COMPOSITE: a text box plus its own candidate list. Moving focus between the two is NOT the
    /// composite losing focus (<see cref="OwnsFocus"/>), and that rule is load-bearing — without it, merely
    /// opening the list would commit the pending literal and turn case 1 of question 20.13 into case 2.
    /// </para>
    /// <para>
    /// It participates in the inherited two-phase pending protocol, so a generic write boundary validates it
    /// with everything else and a draft is never lost in silence.
    /// </para>
    /// </summary>
    public class LinkedPropertyEditor : Grid, IPendingTextField
    {
        private readonly TextBox box = new TextBox();
        private readonly ListBox candidates = new ListBox { MaxHeight = 140 };
        private readonly Popup popup;
        private LinkedPropertyEditSession session;
        private bool programmatic;

        public LinkedPropertyEditor()
        {
            popup = new Popup
            {
                PlacementTarget = box,
                Placement = PlacementMode.Bottom,
                StaysOpen = true,
                Child = new Border
                {
                    Background = SystemColors.WindowBrush,
                    BorderBrush = SystemColors.ActiveBorderBrush,
                    BorderThickness = new Thickness(1),
                    Child = candidates,
                },
            };

            Children.Add(box);
            Children.Add(popup);

            box.TextChanged += Box_TextChanged;
            box.PreviewKeyDown += Box_PreviewKeyDown;
            box.LostKeyboardFocus += Box_LostKeyboardFocus;
            candidates.MouseDoubleClick += Candidates_Commit;
            candidates.PreviewMouseLeftButtonUp += Candidates_Commit;
        }

        /// <summary>The field's name as the user reads it, for the messages of a blocked boundary.</summary>
        public string Label { get; set; } = "Propiedad vinculable";

        /// <summary>Raised when a commit changed the committed state, so the window can recompute.</summary>
        public event EventHandler Committed;

        /// <summary>Raised when a gesture was refused, with the reason, so the window can show it.</summary>
        public event EventHandler<string> Refused;

        /// <summary>The session that owns the semantics. Null until <see cref="Attach"/>.</summary>
        public LinkedPropertyEditSession Session => session;

        /// <summary>The FINAL declared state of this property — what the reconciler consumes.</summary>
        public LinkedPropertyEditState FinalState => session?.Committed;

        public bool IsDirty => session != null && session.IsDirty;

        /// <summary>The text box, so a window can give it focus when a boundary sends the user back here.</summary>
        internal TextBox Box => box;

        /// <summary>Whether the editor holds keyboard focus, counting its own candidate list.</summary>
        public bool HasFocus { get; private set; }

        /// <summary>
        /// Brings the user back to this field after a boundary refused because of it. It selects the pending
        /// text so the correction — Enter, Escape or retyping — starts where the problem is.
        /// </summary>
        public void FocusForCorrection()
        {
            box.Focus();
            box.SelectAll();
            HasFocus = true;
        }

        /// <summary>The candidate list, to be able to assert that focus inside it is not a composite LostFocus.</summary>
        internal ListBox Candidates => candidates;

        // ---------------------------------------------------------------- test seams
        //
        // Las tres rutas de WPF que no se pueden disparar con eventos sintetizados de forma fiable -clic en un
        // item del popup, movimiento de seleccion por teclado y Enter con un candidato navegado- se exponen
        // como costuras para que se ejerciten TAL CUAL las llama el control. No anaden semantica: cada una es
        // el mismo metodo privado que usa el gesto real.

        /// <summary>Lo que hace un clic humano sobre la opcion seleccionada.</summary>
        internal void SelectFromList() => Candidates_Commit(candidates, null);

        /// <summary>Lo que hace Abajo/Arriba: mover la seleccion, sin confirmar nada.</summary>
        internal void MoveSelectionForTest(int delta) => MoveSelection(delta);

        /// <summary>Lo que hace Enter.</summary>
        internal void CommitByEnterForTest() => CommitByEnter();

        /// <summary>
        /// Adopts a session and shows its committed state. Replacing the session is an EXPLICIT discard, the
        /// same shape as a load in the inherited protocol.
        /// </summary>
        public void Attach(LinkedPropertyEditSession newSession)
        {
            session = newSession ?? throw new ArgumentNullException(nameof(newSession));
            popup.IsOpen = false;
            Write(session.Text);
            RefreshCandidates();
        }

        /// <summary>
        /// Whether <paramref name="target"/> belongs to this composite. Focus moving to the candidate list is
        /// focus STAYING in the editor, so nothing may be committed, cancelled or converted because of it.
        /// </summary>
        public bool OwnsFocus(DependencyObject target)
        {
            if (target == null)
            {
                return false;
            }

            if (ReferenceEquals(target, box) || ReferenceEquals(target, candidates) ||
                ReferenceEquals(target, this) || ReferenceEquals(target, popup))
            {
                return true;
            }

            // A list item lives in the popup's own visual tree, so walking up from it never reaches this Grid.
            for (var node = target; node != null; node = VisualOrLogicalParent(node))
            {
                if (ReferenceEquals(node, candidates) || ReferenceEquals(node, box) || ReferenceEquals(node, this))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The composite lost focus to <paramref name="newFocus"/>. Commits ONLY when the source does not
        /// change; a pending draft over a reference stays pending instead of silently unlinking.
        /// </summary>
        public void HandleFocusLeaving(DependencyObject newFocus)
        {
            if (session == null || OwnsFocus(newFocus))
            {
                // Not a composite LostFocus at all.
                return;
            }

            popup.IsOpen = false;

            if (!session.IsDirty)
            {
                return;
            }

            if (session.TryCommitByLostFocus(out var error))
            {
                Write(session.Text);
                Committed?.Invoke(this, EventArgs.Empty);
                return;
            }

            // The draft SURVIVES: losing it silently would discard a human intention, and committing it would
            // change the source without an explicit gesture.
            Refused?.Invoke(this, error);
        }

        // ---------------------------------------------------------------- pending protocol (C4)

        public bool TryStage(out string error)
        {
            error = null;

            if (session == null)
            {
                return true;
            }

            var stage = session.TryStage();

            if (stage.Outcome == LinkedPropertyStageOutcome.Blocked)
            {
                error = Label + ": " + stage.Error;
                return false;
            }

            return true;
        }

        public void ApplyStaged()
        {
            if (session == null)
            {
                return;
            }

            var wasDirty = session.IsDirty;
            session.ApplyStaged();

            if (!wasDirty || session.IsDirty)
            {
                return;
            }

            Write(session.Text);
            Committed?.Invoke(this, EventArgs.Empty);
        }

        public void ShowCommitted()
        {
            if (session == null)
            {
                return;
            }

#if DEBUG
            if (session.IsDirty)
            {
                throw new InvalidOperationException(
                    "ShowCommitted() sobre '" + Label + "' con una edicion pendiente sin comprometer ni descartar "
                        + "(I-43, INV-14). Compromete el campo antes, o descartalo con ResetToCommitted.");
            }
#endif
            Write(session.Text);
        }

        public void ResetToCommitted()
        {
            if (session == null)
            {
                return;
            }

            session.Cancel();
            popup.IsOpen = false;
            Write(session.Text);
        }

        // ---------------------------------------------------------------- gestures

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (programmatic || session == null)
            {
                return;
            }

            session.Type(box.Text);
            RefreshCandidates();
        }

        private void Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (session == null)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    CommitByEnter();
                    return;

                case Key.Escape:
                    e.Handled = true;
                    ResetToCommitted();
                    return;

                case Key.Down:
                case Key.Up:
                    if (!popup.IsOpen)
                    {
                        return;
                    }

                    // EXPLICIT keyboard navigation: it moves the selection, and only then can Enter confirm.
                    e.Handled = true;
                    MoveSelection(e.Key == Key.Down ? 1 : -1);
                    return;
            }
        }

        private void Box_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var target = e.NewFocus as DependencyObject;

            if (!OwnsFocus(target))
            {
                HasFocus = false;
            }

            HandleFocusLeaving(target);
        }

        private void Candidates_Commit(object sender, RoutedEventArgs e)
        {
            // A human click on an option IS explicit selection, and it carries the identity.
            if (candidates.SelectedItem is LinkedPropertyOption option)
            {
                Select(option);
            }
        }

        /// <summary>
        /// Enter. A candidate the user NAVIGATED to is an explicit selection; otherwise the session decides,
        /// and it never resolves a reference from text.
        /// </summary>
        private void CommitByEnter()
        {
            if (popup.IsOpen && candidates.SelectedItem is LinkedPropertyOption option)
            {
                Select(option);
                return;
            }

            if (session.TryCommitByEnter(out var error))
            {
                popup.IsOpen = false;
                Write(session.Text);
                Committed?.Invoke(this, EventArgs.Empty);
                return;
            }

            Refused?.Invoke(this, error);
        }

        private void Select(LinkedPropertyOption option)
        {
            if (!session.TrySelect(option.VariableId, out var error))
            {
                Refused?.Invoke(this, error);
                return;
            }

            popup.IsOpen = false;
            Write(session.Text);
            Committed?.Invoke(this, EventArgs.Empty);
        }

        private void MoveSelection(int delta)
        {
            if (candidates.Items.Count == 0)
            {
                return;
            }

            var next = candidates.SelectedIndex + delta;
            candidates.SelectedIndex = Math.Min(candidates.Items.Count - 1, Math.Max(0, next));
        }

        /// <summary>
        /// Repaints the candidate list. The selection is deliberately CLEARED on every keystroke: leaving one
        /// selected would let Enter confirm a reference the user never chose, which is exactly the auto-select
        /// the contract forbids even when the filter leaves a single result.
        /// </summary>
        private void RefreshCandidates()
        {
            if (session == null)
            {
                return;
            }

            var querying = session.IsQuerying;

            candidates.ItemsSource = querying ? session.Candidates : (IReadOnlyList<LinkedPropertyOption>)null;
            candidates.SelectedIndex = -1;
            popup.IsOpen = querying && session.Candidates.Count > 0;
        }

        private void Write(string text)
        {
            programmatic = true;
            try
            {
                box.Text = text ?? string.Empty;
                box.CaretIndex = box.Text.Length;
            }
            finally
            {
                programmatic = false;
            }
        }

        private static DependencyObject VisualOrLogicalParent(DependencyObject node)
        {
            var logical = LogicalTreeHelper.GetParent(node);

            if (logical != null)
            {
                return logical;
            }

            return node is System.Windows.Media.Visual || node is System.Windows.Media.Media3D.Visual3D
                ? System.Windows.Media.VisualTreeHelper.GetParent(node)
                : null;
        }
    }
}
