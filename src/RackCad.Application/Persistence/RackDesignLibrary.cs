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
        Dinamico
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

        public string KindLabel => Kind == RackDesignKind.Dinamico ? "Sistema dinámico" : "Cabecera";
    }

    /// <summary>
    /// Lists the named designs saved as <c>.rackcad.json</c> in a folder, inferring each one's type and display name by
    /// reading it. Today only cabecera and sistema-dinámico persist to disk (selectivo y cama viven embebidos en el DWG),
    /// so those are the two kinds the library shows. Pure and unit-testable; no AutoCAD, no WPF.
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
                    var kind = project.Kind == RackSystemKind.PalletFlow ? RackDesignKind.Dinamico : RackDesignKind.Cabecera;
                    var fallback = StripExtension(path);
                    var name = kind == RackDesignKind.Cabecera && !string.IsNullOrWhiteSpace(project.Header?.Name)
                        ? project.Header.Name.Trim()
                        : fallback;

                    entries.Add(new RackDesignLibraryEntry(path, name, kind, File.GetLastWriteTimeUtc(path)));
                }
                catch
                {
                    // Skip unreadable/foreign files rather than fail the whole listing.
                }
            }

            return entries.OrderByDescending(entry => entry.ModifiedUtc).ToList();
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
