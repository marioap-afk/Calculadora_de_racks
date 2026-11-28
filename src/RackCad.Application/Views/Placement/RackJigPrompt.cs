#nullable enable
using System;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// I-64, sexta tanda, ui (O-3): what the command line asks while the jig shared by every system (<c>BlockPlacement</c> of the Plugin)
    /// places a block. A caller that names its prompt is asked that (a Cantilever view names its own); otherwise the jig asks the prompt of
    /// the scope the command opened; otherwise the historic text of the cabecera, which RACKCABECERA keeps.
    ///
    /// The scope exists because the commands that place something else than a cabecera (a cama, a Push Back, a Dinámico, a view of a
    /// Selectivo) reach the jig through paths that name nothing and that the window and the classic share (the dispatch of «Nuevo», the
    /// «Insertar» half of RACKEDITAR, the draw services): each command opens the scope of what it inserts (answer of the Owner on
    /// 2026-11-27: «Todas», in the window and in the classic) instead of threading a prompt through them. A command of AutoCAD runs on one
    /// thread from start to end, and the scope belongs to that thread; disposing it gives back the scope that was open before.
    /// </summary>
    public static class RackJigPrompt
    {
        /// <summary>The historic text of the jig, for every caller that names nothing outside a scope (RACKCABECERA).</summary>
        public const string Cabecera = "\nPunto de insercion de la cabecera: ";

        /// <summary>A cama: RACKCAMANUEVO, RACKNUEVO Cama, QUICKCAMA, RACKEDITAR of a cama.</summary>
        public const string Cama = "\nPunto de insercion de la cama: ";

        /// <summary>Each view of a Push Back.</summary>
        public const string PushBackView = "\nPunto de insercion de la vista Push Back: ";

        /// <summary>Each view of a Selectivo.</summary>
        public const string SelectiveView = "\nPunto de insercion de la vista Selectivo: ";

        /// <summary>Each view of a Dinámico. Without accents, as every prompt of the jig («insercion», «seccion»).</summary>
        public const string DynamicView = "\nPunto de insercion de la vista Dinamico: ";

        [ThreadStatic]
        private static string? _scoped;

        /// <summary>The prompt the jig asks: the one its caller named, or the one of the open scope, or <see cref="Cabecera"/>.</summary>
        public static string Resolve(string? named)
        {
            if (!string.IsNullOrWhiteSpace(named)) return named!;
            return string.IsNullOrWhiteSpace(_scoped) ? Cabecera : _scoped!;
        }

        /// <summary>
        /// The prompt of the views of a system; null for the cabecera (<see cref="RackSystemKind.Selective"/>, which keeps
        /// <see cref="Cabecera"/>) and for a system whose placement names its own (Cantilever) or that the jig does not place.
        /// </summary>
        public static string? ForSystem(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack: return SelectiveView;
                case RackSystemKind.PalletFlow: return DynamicView;
                case RackSystemKind.PushBack: return PushBackView;
                case RackSystemKind.Cama: return Cama;
                default: return null;
            }
        }

        /// <summary>The prompt of the views of a rack by the Kind of its envelope (RACKEDITAR); null as <see cref="ForSystem"/>.</summary>
        public static string? ForEnvelopeKind(string? kind)
        {
            switch (kind)
            {
                case RackEmbedDocument.KindSelective: return SelectiveView;
                case RackEmbedDocument.KindDynamic: return DynamicView;
                case RackEmbedDocument.KindPushBack: return PushBackView;
                case RackEmbedDocument.KindCama: return Cama;
                default: return null;
            }
        }

        /// <summary>
        /// Opens a scope in which every jig that names nothing asks <paramref name="prompt"/>; disposing it closes it. A scope with no prompt
        /// (a cabecera) keeps the one that was open, or the text of the cabecera.
        /// </summary>
        public static IDisposable Scope(string? prompt)
        {
            var previous = _scoped;
            if (!string.IsNullOrWhiteSpace(prompt)) _scoped = prompt;
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
