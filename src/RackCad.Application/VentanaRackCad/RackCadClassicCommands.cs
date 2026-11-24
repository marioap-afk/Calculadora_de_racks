#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// What the RackCad window can ask AutoCAD to run while its sections are in their intermediate step (I-64 A-14, phase 1: "cada sección
    /// abre su ventana actual"). Each action is one flow that already exists behind an AutoCAD command; the window adds a door, not a
    /// second flow.
    /// </summary>
    public enum RackCadClassicAction
    {
        NewSelective,
        NewDynamic,
        NewPushBack,
        NewCantilever,
        NewHeader,
        NewFlowBed,
        NewLarguero,
        StructuralSection,
        Variables,
        RackList,
        TotalBom,
        DesignLibrary,
        ClassicMenu,
        Help,
    }

    /// <summary>How a request to run a classic flow ended. A refusal is a value, never a thrown exception.</summary>
    public enum RackCadClassicStatus
    {
        /// <summary>The command was handed to AutoCAD; it runs as soon as AutoCAD takes it.</summary>
        Started,

        /// <summary>A command is in progress: nothing was sent, so nothing can be typed into that command.</summary>
        Busy,

        /// <summary>There is no active document to run it in.</summary>
        NoDocument,

        /// <summary>The host failed; the failure is contained and reported as a value.</summary>
        Failed,
    }

    /// <summary>The answer of the port: the status and, when it is not <see cref="RackCadClassicStatus.Started"/>, why.</summary>
    public sealed record RackCadClassicResult(RackCadClassicStatus Status, string? Reason)
    {
        public bool IsStarted => Status == RackCadClassicStatus.Started;

        public static RackCadClassicResult Started() => new(RackCadClassicStatus.Started, null);

        public static RackCadClassicResult Without(RackCadClassicStatus status, string reason) => new(status, reason);
    }

    /// <summary>
    /// The port through which the RackCad window runs a classic flow (I-64 A-14, phase 1). The host hands the command to AutoCAD only when
    /// there is an active document and no command is in progress, so the text never lands inside another command. It reads and writes
    /// nothing of the drawing by itself.
    /// </summary>
    public interface IRackCadClassicCommandPort
    {
        RackCadClassicResult Run(RackCadClassicAction action);
    }

    /// <summary>
    /// The one place that says which AutoCAD command runs each classic action and which keyword of RACKNUEVO names each system (I-64 A-14,
    /// phase 1). The Plugin sends the text and offers the keywords; nothing else spells them.
    /// </summary>
    public static class RackCadClassicCommands
    {
        /// <summary>The command that opens the editor of a new system, by its keyword. It runs exactly the path of the menu.</summary>
        public const string NewCommand = "RACKNUEVO";

        /// <summary>The command that opens the design library, with the path of the menu.</summary>
        public const string LibraryCommand = "RACKBIBLIOTECA";

        /// <summary>The command of the classic RackCad menu, reachable from the window while the sections migrate.</summary>
        public const string ClassicMenuCommand = "RACKMENU";

        private static readonly IReadOnlyList<(string Keyword, RackSystemKind Kind, RackCadClassicAction Action)> NewSystems = new[]
        {
            ("Selectivo", RackSystemKind.SelectiveRack, RackCadClassicAction.NewSelective),
            ("PushBack", RackSystemKind.PushBack, RackCadClassicAction.NewPushBack),
            ("Dinamico", RackSystemKind.PalletFlow, RackCadClassicAction.NewDynamic),
            ("Cantilever", RackSystemKind.Cantilever, RackCadClassicAction.NewCantilever),
            ("Cabecera", RackSystemKind.Selective, RackCadClassicAction.NewHeader),
            ("Cama", RackSystemKind.Cama, RackCadClassicAction.NewFlowBed),
            ("Larguero", RackSystemKind.Larguero, RackCadClassicAction.NewLarguero),
        };

        /// <summary>The keywords of RACKNUEVO, in the order of the menu of the window.</summary>
        public static IReadOnlyList<string> NewKeywords
        {
            get
            {
                var keywords = new List<string>(NewSystems.Count);
                foreach (var system in NewSystems) keywords.Add(system.Keyword);
                return keywords;
            }
        }

        /// <summary>The system that a keyword of RACKNUEVO names, ignoring case; false for any other text.</summary>
        public static bool TryKindOfKeyword(string? keyword, out RackSystemKind kind)
        {
            foreach (var system in NewSystems)
            {
                if (!string.Equals(system.Keyword, keyword?.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                kind = system.Kind;
                return true;
            }

            kind = default;
            return false;
        }

        /// <summary>
        /// The text that the host sends to AutoCAD for the action: the command and, for a new system, its keyword, each followed by a space
        /// (the space ends a word of the command line). Every action has one.
        /// </summary>
        public static string TextOf(RackCadClassicAction action)
        {
            foreach (var system in NewSystems)
                if (system.Action == action) return NewCommand + " " + system.Keyword + " ";

            switch (action)
            {
                case RackCadClassicAction.StructuralSection: return "RACKSECCION ";
                case RackCadClassicAction.Variables: return "RACKVARIABLES ";
                case RackCadClassicAction.RackList: return "RACKLISTA ";
                case RackCadClassicAction.TotalBom: return "RACKBOMTOTAL ";
                case RackCadClassicAction.DesignLibrary: return LibraryCommand + " ";
                case RackCadClassicAction.ClassicMenu: return ClassicMenuCommand + " ";
                case RackCadClassicAction.Help: return "RACKAYUDA ";
                default: throw new ArgumentOutOfRangeException(nameof(action), action, "The action has no classic command.");
            }
        }
    }
}
