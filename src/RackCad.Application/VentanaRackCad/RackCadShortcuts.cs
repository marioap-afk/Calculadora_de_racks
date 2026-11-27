using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>T6 rl: what a command typed by the user opens in the RackCad window (answer of the Owner, 2026-11-27: «La ventana, ya»).</summary>
    public enum RackCadShortcutTarget
    {
        /// <summary>RACKVARIABLES and RVA: the page «Variables».</summary>
        Variables,

        /// <summary>RACKPROPIEDADES and RPR with a rack: the tab «Propiedades» of the editor of that rack in the window.</summary>
        RackProperties,

        /// <summary>RACKPROPIEDADES and RPR with «Proyecto»: the tab «Propiedades del proyecto» of the page «Variables».</summary>
        ProjectProperties,

        /// <summary>T7 per: RACKSECCION, the page «Perfil estructural» (it reads nothing of the drawing: the page reads the catalog).</summary>
        StructuralSection,
    }

    /// <summary>
    /// T6 rl: a command typed by the user asks the window to show what it opens, during the call and inside the command. The window shows
    /// it with the gestures it already has («Editar aquí», the first read of «Variables»); the reads those gestures hand to the door run
    /// INSIDE the command (the door keeps them, the command runs them after the call), so no second command leaves the window. The window
    /// marks the ask <see cref="Taken"/> and says what it did, or why not, in <see cref="Said"/>, for the command line.
    /// </summary>
    public sealed class RackCadShortcutAsk
    {
        public RackCadShortcutAsk(RackCadShortcutTarget target, string rackId, string note)
        {
            Target = target;
            RackId = string.IsNullOrWhiteSpace(rackId) ? null : rackId.Trim();
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        }

        public RackCadShortcutTarget Target { get; }

        /// <summary>The rack of <see cref="RackCadShortcutTarget.RackProperties"/>, or null.</summary>
        public string RackId { get; }

        /// <summary>What the command says to the window (for example, why the selection designated no rack), or null.</summary>
        public string Note { get; }

        /// <summary>Set by the window: it showed what the command opens.</summary>
        public bool Taken { get; set; }

        /// <summary>Set by the window: what it did, or why it could not, in the words of the window; null when there is nothing to add.</summary>
        public string Said { get; set; }
    }

    /// <summary>
    /// T6 rl: the door of the window to the commands typed by the user that open one of its pages (<see cref="RackCadShortcutTarget"/>). The
    /// command raises <see cref="ShortcutAsked"/>, runs inside itself the reads the window handed to the door during the call, and raises
    /// <see cref="ShortcutDone"/> when they answered (the window then shows the tab it was asked for).
    /// </summary>
    public interface IRackCadShortcutPort : IRackCadRackDoorPort
    {
        event Action<RackCadShortcutAsk> ShortcutAsked;

        event Action<RackCadShortcutAsk> ShortcutDone;
    }

    /// <summary>T6 rl: the pure rules of those commands.</summary>
    public static class RackCadShortcutRules
    {
        /// <summary>
        /// The reads of the window that a command typed by the user may run inside itself: the first read of «Variables» and the reads of
        /// «Editar aquí» of the editors with a tab «Propiedades». They only read (no undo marker); any other request, writes included, is
        /// refused during the command as today (a command is in progress).
        /// </summary>
        public static IReadOnlyList<string> InlineReads { get; } = new[]
        {
            "RACKVARIABLESLEER", "RACKSELECTIVOABRIR", "RACKDINAMICOABRIR", "RACKPUSHBACKABRIR", "RACKCANTILEVERABRIR", "RACKCAMAABRIR",
        };

        /// <summary>The command of a text the door hands to AutoCAD («RACKSELECTIVOABRIR » → RACKSELECTIVOABRIR) when it is one of <see cref="InlineReads"/>; null otherwise.</summary>
        public static string InlineReadOf(string text)
        {
            var name = (text ?? string.Empty).Trim();
            return InlineReads.FirstOrDefault(r => string.Equals(r, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The prompt of RACKPROPIEDADES, the same as the classic window (I-54 G7, D-18.1).</summary>
        public const string PropertiesPrompt = "\nSelecciona un rack o [Proyecto]: ";

        public const string NotOpenedText = "no se pudo abrir la ventana RackCad.";

        /// <summary>The rack of RACKPROPIEDADES: the only rack of the selection made before it; several, none (and why); none, ask.</summary>
        public static string DesignatedRack(IReadOnlyList<string> selectedRackIds, out string note)
        {
            note = null;
            var ids = (selectedRackIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (ids.Count == 1) return ids[0];
            if (ids.Count > 1)
                note = "Hay " + ids.Count + " racks seleccionados en AutoCAD: elige uno.";
            return null;
        }

        /// <summary>The line the command leaves in the command line.</summary>
        public static string CommandLineText(RackCadShortcutAsk ask)
        {
            var page = ask == null ? "la ventana RackCad"
                : ask.Target == RackCadShortcutTarget.Variables ? "«Variables» en la ventana RackCad"
                : ask.Target == RackCadShortcutTarget.ProjectProperties ? "«Variables» › «Propiedades del proyecto» en la ventana RackCad"
                : ask.Target == RackCadShortcutTarget.StructuralSection ? "«Nuevo» › «Perfil estructural» en la ventana RackCad" // T7 per
                : "«Propiedades» del rack en la ventana RackCad";
            if (ask == null || !ask.Taken) return "RackCad: " + NotOpenedText;
            var said = (ask.Said ?? string.Empty).Trim();
            return "RackCad: " + page + (said.Length == 0 ? "." : ": " + said);
        }

        // ------------------------------------------------------------------ what the window says

        public const string CabeceraHasNoPropertiesText =
            "Una cabecera suelta no tiene pestaña «Propiedades» en la ventana: usa «Propiedades personalizadas (ventana clásica)» del menú clásico (RACKMENU).";

        public const string NoEditorText =
            "Este rack no tiene editor en la ventana: usa «Propiedades personalizadas (ventana clásica)» del menú clásico (RACKMENU).";

        public const string NotInTheListText =
            "La lista de la ventana no tiene ese rack: pulsa «Volver a leer la lista» y vuelve a escribir RACKPROPIEDADES.";

        /// <summary>T6rl-S14: where the classic window is, for a rack the window cannot open (correction 1 of the review).</summary>
        public const string ClassicPropertiesText =
            "Usa «Propiedades personalizadas (ventana clásica)» del menú clásico (RACKMENU).";

        public const string BusyText =
            "La ventana espera otra respuesta de AutoCAD: cuando termine, vuelve a escribir la orden.";
    }
}
