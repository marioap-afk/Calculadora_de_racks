using System;
using System.Windows;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// What an editor has pending that closing would discard, and the question to ask before discarding it.
    ///
    /// <para>ADR-0029 D8: dirty belongs to a transactional SCOPE, not to the window, and <c>NotApplicable</c> is a
    /// legitimate value. A window that has no scope with real pending work returns <see cref="None"/> and closes
    /// straight away — no dialog is invented for a product that has no transaction.</para>
    /// </summary>
    public sealed class EditorPendingWork
    {
        /// <summary>Nothing is pending: the window may close without asking.</summary>
        public static readonly EditorPendingWork None = new EditorPendingWork(null);

        private EditorPendingWork(string question) => Question = question;

        /// <summary>Declares pending work with the question the user must answer before losing it.</summary>
        public static EditorPendingWork Pending(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException("Pending work must carry the question to ask.", nameof(question));
            }

            return new EditorPendingWork(question);
        }

        /// <summary>Pending only when <paramref name="hasPendingWork"/>; otherwise <see cref="None"/>. The helper
        /// exists so a window reads as one expression over the authority it already owns.</summary>
        public static EditorPendingWork When(bool hasPendingWork, string question)
            => hasPendingWork ? Pending(question) : None;

        public bool HasPendingWork => Question != null;

        /// <summary>What to ask before discarding. Null when nothing is pending.</summary>
        public string Question { get; }
    }

    /// <summary>
    /// The confirmation seam. Production shows a <see cref="MessageBox"/>; a test substitutes the answer.
    ///
    /// <para>It exists because <c>MessageBox.Show</c> has no seam at all, so a close policy built on it would be
    /// untestable — and an untestable close policy is exactly the kind that silently loses work. Deliberately tiny:
    /// one delegate and a scope to replace it. No framework, no container, no new package.</para>
    /// </summary>
    public static class EditorDiscardPrompt
    {
        private static Func<string, Window, bool> confirm = ShowMessageBox;

        /// <summary>True when the user accepts losing the pending work.</summary>
        public static bool Confirm(string question, Window owner = null) => confirm(question, owner);

        /// <summary>Replaces the prompt for the duration of the returned scope. For tests only.</summary>
        public static IDisposable Substitute(Func<string, bool> replacement)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));

            return Substitute((question, _) => replacement(question));
        }

        /// <summary>As <see cref="Substitute(Func{string,bool})"/>, when the test also inspects the owner.</summary>
        public static IDisposable Substitute(Func<string, Window, bool> replacement)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));

            var previous = confirm;
            confirm = replacement;
            return new Restore(() => confirm = previous);
        }

        // Los mismos parametros que el dialogo que el configurador de cabecera ya mostraba antes de I-39B, para que
        // enrutarlo por esta costura no cambie lo que el usuario ve: mismo titulo, mismos botones, mismo icono.
        private static bool ShowMessageBox(string question, Window owner) =>
            (owner == null
                ? MessageBox.Show(question, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                : MessageBox.Show(owner, question, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            == MessageBoxResult.Yes;

        private sealed class Restore : IDisposable
        {
            private Action restore;

            public Restore(Action restore) => this.restore = restore;

            public void Dispose()
            {
                var pending = restore;
                restore = null;
                pending?.Invoke();
            }
        }
    }

    /// <summary>
    /// The ONE place an editor decides whether it may close (ADR-0029 D7).
    ///
    /// <para>Every route — the Close button, Escape, the system button and <c>Alt+F4</c> — reaches WPF's
    /// <c>OnClosing</c>, so a window that consults this from that single override gets one coherent policy for all
    /// four instead of four different behaviours. No route inserts, updates or saves; the policy only decides
    /// whether the window may go away.</para>
    /// </summary>
    public static class EditorClosePolicy
    {
        /// <summary>True when the window may close: either nothing is pending, or the user confirmed discarding it.</summary>
        public static bool MayClose(EditorPendingWork pending)
        {
            if (pending == null) throw new ArgumentNullException(nameof(pending));

            return !pending.HasPendingWork || EditorDiscardPrompt.Confirm(pending.Question);
        }
    }
}
