using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>T6 rl: a rack the user selected in AutoCAD before RLY or RR: its id, its name and whether the reference selected is its planta.</summary>
    public sealed record WarehouseSelectedRack(string RackId, string Name, bool IsPlanta);

    /// <summary>
    /// T6 rl (AL-S14; answer of the Owner, 2026-11-27: «La página «Almacén»»): RLY or RR, typed by the user, open the page «Almacén» of the
    /// window on their tab and read the drawing INSIDE their own command (the gesture is the shortcut: no second command leaves the window).
    /// The command asks the page, during the call, what to read: the rack designated by the selection made before it
    /// (<see cref="DesignatedRackId"/>), or the seed the page takes by hand. The window answers in <see cref="Read"/> (null: nothing to read,
    /// and <see cref="Note"/> says why) and marks the ask <see cref="Taken"/>; the command then reads and hands the reading back.
    /// </summary>
    public sealed class WarehousePageAsk
    {
        public WarehousePageAsk(WarehouseLayoutMode mode, string designatedRackId, string designationNote)
        {
            Mode = mode;
            DesignatedRackId = string.IsNullOrWhiteSpace(designatedRackId) ? null : designatedRackId.Trim();
            DesignationNote = string.IsNullOrWhiteSpace(designationNote) ? null : designationNote.Trim();
        }

        /// <summary>The tab: «Rejilla» for RLY, «Rellenar un área» for RR.</summary>
        public WarehouseLayoutMode Mode { get; }

        /// <summary>The one rack selected in AutoCAD before the shortcut, or null (none, or several).</summary>
        public string DesignatedRackId { get; }

        /// <summary>Why the selection designated no rack (several racks selected), or null.</summary>
        public string DesignationNote { get; }

        /// <summary>Set by the window: it has the page and showed it.</summary>
        public bool Taken { get; set; }

        /// <summary>Set by the window: what the command reads for the page (number 0: it is no request of the door), or null.</summary>
        public WarehouseReadRequest Read { get; set; }

        /// <summary>Set by the window when it asks nothing: why, in the words of the page.</summary>
        public string Note { get; set; }
    }

    /// <summary>T6 rl: the rules of RLY and RR, pure (the commands live in <c>RackLayoutCommands.Ventana.cs</c>).</summary>
    public static partial class WarehouseLayoutRules
    {
        /// <summary>T6 rl: the shortcut that opens «Rejilla». The full command RACKLAYOUT keeps its classic window.</summary>
        public const string GridShortcut = "RLY";

        /// <summary>T6 rl: the shortcut that opens «Rellenar un área». The full command RACKRELLENAR keeps its classic window.</summary>
        public const string FillShortcut = "RR";

        public static string ShortcutOf(WarehouseLayoutMode mode) => mode == WarehouseLayoutMode.Fill ? FillShortcut : GridShortcut;

        public static string TabName(WarehouseLayoutMode mode) => mode == WarehouseLayoutMode.Fill ? "Rellenar un área" : "Rejilla";

        /// <summary>
        /// T6 rl: the rack the selection made before RLY or RR designates as the seed: the only rack of the selection (any of its views; the
        /// planta selected, if any, is the seed, AL-S2). Nothing selected: null and no note (the page takes the seed it takes by hand). Several
        /// racks: null, and <paramref name="note"/> says that none was taken and what to do.
        /// </summary>
        public static string DesignatedSeed(IReadOnlyList<WarehouseSelectedRack> selected, WarehouseLayoutMode mode, out string note)
        {
            note = null;
            var ids = (selected ?? Array.Empty<WarehouseSelectedRack>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.RackId))
                .Select(r => r.RackId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (ids.Count == 0) return null;
            if (ids.Count == 1) return ids[0];

            var shortcut = ShortcutOf(mode);
            note = "Hay " + ids.Count.ToString(CultureInfo.InvariantCulture) + " racks seleccionados en AutoCAD: " + shortcut
                   + " no tomó ninguno como semilla y la página sigue con la suya. Para usar otro, déjalo seleccionado solo a él y vuelve a escribir "
                   + shortcut + ", o elígelo en «Rack semilla».";
            return null;
        }

        /// <summary>T6 rl: why the shortcut reads nothing while the page waits for AutoCAD (the reason of «Llevar al almacén…», correction 7).</summary>
        public static string ShortcutWaitText(WarehouseLayoutMode mode) =>
            "«Almacén» espera a AutoCAD (lee la planta o coloca): cuando termine, vuelve a escribir " + ShortcutOf(mode) + " o pulsa «Volver a leer».";

        /// <summary>
        /// T6 rl: the line RLY or RR leave in the command line: the page they opened and what they read, then the note (why the selection
        /// designated no rack); without a reading, why they read nothing.
        /// </summary>
        public static string ShortcutCommandLineText(WarehouseLayoutMode mode, WarehouseReading reading, string note)
        {
            var head = "RackCad: «Almacén» › «" + TabName(mode) + "» en la ventana RackCad";
            var why = (note ?? string.Empty).Trim();
            if (reading != null)
            {
                var read = head + ": " + (reading.IsRead ? ReadingSummary(reading) : Lower(reading.Failure).TrimEnd('.')) + ".";
                return why.Length == 0 ? read : read + " " + why;
            }

            why = why.TrimEnd('.');
            return why.Length == 0 ? head + "." : head + ": " + Lower(why) + ".";
        }
    }
}
