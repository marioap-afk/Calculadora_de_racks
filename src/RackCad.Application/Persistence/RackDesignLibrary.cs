using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    public enum RackDesignKind
    {
        Cabecera,
        Dinamico,
        Selectivo,
        Cama,
        Larguero
    }

    /// <summary>One design file in the library: its path, the display name, its type and last-modified time.</summary>
    public sealed class RackDesignLibraryEntry
    {
        public RackDesignLibraryEntry(string path, string name, RackDesignKind kind, DateTime modifiedUtc)
        {
            Path = path;
            Name = name;
            Kind = kind;
            ModifiedUtc = modifiedUtc;
        }

        public string Path { get; }
        public string Name { get; }
        public RackDesignKind Kind { get; }
        public DateTime ModifiedUtc { get; }

        /// <summary>Last-modified time in local time, for display.</summary>
        public DateTime Modified => ModifiedUtc.ToLocalTime();

        public string KindLabel
        {
            get
            {
                switch (Kind)
                {
                    case RackDesignKind.Dinamico: return "Sistema dinámico";
                    case RackDesignKind.Selectivo: return "Selectivo";
                    case RackDesignKind.Cama: return "Cama de rodamiento";
                    case RackDesignKind.Larguero: return "Larguero";
                    default: return "Cabecera";
                }
            }
        }
    }

    /// <summary>
    /// Lists the named designs saved as <c>.rackcad.json</c> in a folder, inferring each one's type and display name by
    /// reading it. Every kind persists to disk now: cabecera, sistema dinámico, selectivo, cama and larguero
    /// (see <see cref="RackSystemKind"/> / <see cref="RackProjectStore"/>). Pure and unit-testable; no AutoCAD, no WPF.
    /// </summary>
    public static class RackDesignLibrary
    {
        public static IReadOnlyList<RackDesignLibraryEntry> List(string folder)
        {
            var entries = new List<RackDesignLibraryEntry>();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return entries;
            }

            var store = new RackProjectStore();
            foreach (var path in Directory.EnumerateFiles(folder, "*" + RackProjectStore.FileExtension))
            {
                try
                {
                    var project = store.Load(path);
                    var kind = MapKind(project.Kind);
                    var fallback = StripExtension(path);

                    // Prefer the payload's own name when it has one; otherwise the file name.
                    var payloadName = project.Header?.Name
                        ?? project.SelectiveRack?.Name
                        ?? project.Larguero?.Name;
                    var name = !string.IsNullOrWhiteSpace(payloadName) ? payloadName.Trim() : fallback;

                    entries.Add(new RackDesignLibraryEntry(path, name, kind, File.GetLastWriteTimeUtc(path)));
                }
                catch
                {
                    // Skip unreadable/foreign files rather than fail the whole listing.
                }
            }

            return entries.OrderByDescending(entry => entry.ModifiedUtc).ToList();
        }

        private static RackDesignKind MapKind(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.PalletFlow: return RackDesignKind.Dinamico;
                case RackSystemKind.SelectiveRack: return RackDesignKind.Selectivo;
                case RackSystemKind.Cama: return RackDesignKind.Cama;
                case RackSystemKind.Larguero: return RackDesignKind.Larguero;
                default: return RackDesignKind.Cabecera;
            }
        }

        /// <summary>Base file name without the <c>.rackcad.json</c> double extension (which <c>GetFileNameWithoutExtension</c> only half-strips).</summary>
        private static string StripExtension(string path)
        {
            var name = Path.GetFileName(path) ?? string.Empty;
            return name.EndsWith(RackProjectStore.FileExtension, StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - RackProjectStore.FileExtension.Length)
                : Path.GetFileNameWithoutExtension(name);
        }
    }
}
