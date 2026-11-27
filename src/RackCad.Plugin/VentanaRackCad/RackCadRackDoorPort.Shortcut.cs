using System;
using System.Collections.Generic;
using RackCad.Application.VentanaRackCad;
using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T6 rl (answer of the Owner, 2026-11-27: «La ventana, ya»): RACKVARIABLES, RVA, RACKPROPIEDADES and RPR open their page in the RackCad
    /// window. The command is the gesture of the user, so the reads the window hands to this door while the command asks it
    /// (<see cref="CollectInlineReads"/>) are not sent to AutoCAD as a second command: the door keeps the request as always and only notes
    /// its command, and the command runs it inside itself after the call (only the reads of <see cref="RackCadShortcutRules.InlineReads"/>;
    /// any other request is refused as during any command). This file holds no AutoCAD transaction and no lock.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadShortcutPort
    {
        /// <summary>The reads noted while a typed command asks the window, for that drawing.</summary>
        private sealed class InlineReadsOfCommand
        {
            public long Document;
            public readonly List<string> Commands = new();
        }

        private static InlineReadsOfCommand _inlineReads;

        public event Action<RackCadShortcutAsk> ShortcutAsked;

        public event Action<RackCadShortcutAsk> ShortcutDone;

        internal void AskShortcut(RackCadShortcutAsk ask) => Raise(() => ShortcutAsked?.Invoke(ask));

        internal void SayShortcutDone(RackCadShortcutAsk ask) => Raise(() => ShortcutDone?.Invoke(ask));

        /// <summary>
        /// Runs <paramref name="ask"/> (the call to the window) and returns the reads the window handed to the door meanwhile, for
        /// <paramref name="document"/>: their requests wait in the door, and the command runs them inside itself.
        /// </summary>
        internal static IReadOnlyList<string> CollectInlineReads(AcDocument document, Action ask)
        {
            var collected = new InlineReadsOfCommand { Document = KeyOf(document) };
            _inlineReads = collected;
            try
            {
                ask();
            }
            finally
            {
                _inlineReads = null;
            }

            return collected.Commands;
        }

        // From Send: while a typed command asks the window, a read of the list is kept and noted (run inside the command); any other text,
        // or another drawing, is refused as during any command. False when no command asks: Send goes on as always.
        private static bool KeptInline(string text, AcDocument document, Action<AcDocument> keep, out RackCadClassicResult result)
        {
            result = null;
            var inline = _inlineReads;
            if (inline == null) return false;

            var command = RackCadShortcutRules.InlineReadOf(text);
            if (command == null || inline.Document != KeyOf(document) || inline.Commands.Count > 0)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
                return true;
            }

            keep(document);
            inline.Commands.Add(command);
            result = RackCadClassicResult.Started();
            return true;
        }
    }
}
