#nullable enable
using System;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// I-64, sexta tanda, ui (O-3): what the command line asks while the jig shared by every system (<c>BlockPlacement</c> of the Plugin)
    /// places a block. A caller that names its prompt is asked that (a Cantilever view names its own); otherwise the jig asks the prompt of
    /// the scope the command opened; otherwise the historic text of the cabecera, so RACKCABECERA and every other command read what they
    /// always read.
    ///
    /// The scope exists because a command of the RackCad window that places something else than a cabecera (a new cama, a new Push Back, a
    /// view of a Selectivo) reaches the jig through paths that name nothing and that the classic commands share (the dispatch of «Nuevo»,
    /// the «Insertar» half of RACKEDITAR): the command opens the scope around its insertion instead of threading a prompt through them. A
    /// command of AutoCAD runs on one thread from start to end, and the scope belongs to that thread; disposing it gives back the scope that
    /// was open before.
    /// </summary>
    public static class RackJigPrompt
    {
        /// <summary>The historic text of the jig, for every caller that names nothing outside a scope.</summary>
        public const string Cabecera = "\nPunto de insercion de la cabecera: ";

        /// <summary>RACKCAMANUEVO: a new cama of the window.</summary>
        public const string NewCama = "\nPunto de insercion de la cama: ";

        /// <summary>RACKPUSHBACKNUEVO: each view of a new Push Back of the window.</summary>
        public const string PushBackView = "\nPunto de insercion de la vista Push Back: ";

        /// <summary>RACKSELECTIVOINSERTAR: each view of «Insertar vista…» of a Selectivo of the window.</summary>
        public const string SelectiveView = "\nPunto de insercion de la vista Selectivo: ";

        [ThreadStatic]
        private static string? _scoped;

        /// <summary>The prompt the jig asks: the one its caller named, or the one of the open scope, or <see cref="Cabecera"/>.</summary>
        public static string Resolve(string? named)
        {
            if (!string.IsNullOrWhiteSpace(named)) return named!;
            return string.IsNullOrWhiteSpace(_scoped) ? Cabecera : _scoped!;
        }

        /// <summary>Opens a scope in which every jig that names nothing asks <paramref name="prompt"/>; disposing it closes it.</summary>
        public static IDisposable Scope(string prompt)
        {
            var previous = _scoped;
            _scoped = prompt;
            return new Restore(previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly string? _previous;
            private bool _disposed;

            internal Restore(string? previous) => _previous = previous;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _scoped = _previous;
            }
        }
    }
}
