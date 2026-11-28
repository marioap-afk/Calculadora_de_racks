using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// One design file in the library: its path, the display name, its canonical <see cref="RackSystemKind"/>, the exact
    /// visible "Tipo" label (resolved from the registry) and last-modified time.
    /// </summary>
    public sealed class RackDesignLibraryEntry
    {
        public RackDesignLibraryEntry(string path, string name, RackSystemKind kind, string kindLabel, DateTime modifiedUtc)
        {
            Path = path;
            Name = name;
            Kind = kind;
            KindLabel = kindLabel;
            ModifiedUtc = modifiedUtc;
        }

        public string Path { get; }
        public string Name { get; }

        /// <summary>The canonical system kind (see <see cref="RackSystemKind"/>).</summary>
        public RackSystemKind Kind { get; }

        /// <summary>The exact, user-visible "Tipo" label, resolved from the registered descriptor for <see cref="Kind"/>.</summary>
        public string KindLabel { get; }

        public DateTime ModifiedUtc { get; }

        /// <summary>Last-modified time in local time, for display.</summary>
        public DateTime Modified => ModifiedUtc.ToLocalTime();
    }

    /// <summary>
    /// Lists the named designs saved as <c>.rackcad.json</c> in a folder, inferring each one's canonical kind and display
    /// name by reading it. The visible type label comes from the <see cref="SystemRegistry"/> descriptor for the kind —
    /// there is no parallel kind enum and no label switch. Pure and unit-testable; no AutoCAD, no WPF.
    /// </summary>
    public static class RackDesignLibrary
    {
        public static IReadOnlyList<RackDesignLibraryEntry> List(string folder)
            => List(folder, SystemRegistry.Default);

        /// <summary>
        /// Overload that resolves the visible labels from an explicit <paramref name="registry"/> (a seam for tests);
        /// <see cref="List(string)"/> uses <see cref="SystemRegistry.Default"/>.
        /// </summary>
        public static IReadOnlyList<RackDesignLibraryEntry> List(string folder, SystemRegistry registry)
            => Read(folder, registry).Entries;

        /// <summary>
        /// The same reading as <see cref="List(string)"/>, saying also which files it could not read and whether the folder exists (I-64
        /// A-14, fourth batch, part «ut», UT-06). The rules of each entry are the ones of the list: one reading, two answers.
        /// </summary>
        public static RackDesignLibraryListing Read(string folder) => Read(folder, SystemRegistry.Default);

        /// <summary><see cref="Read(string)"/> with the labels of an explicit <paramref name="registry"/> (a seam for tests).</summary>
        public static RackDesignLibraryListing Read(string folder, SystemRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            var entries = new List<RackDesignLibraryEntry>();
            var unreadable = new List<string>();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return new RackDesignLibraryListing(folder, false, entries, unreadable);
            }

            var store = new RackProjectStore();
            foreach (var path in Directory.EnumerateFiles(folder, "*" + RackProjectStore.FileExtension))
            {
                try
                {
                    var project = store.Load(path);

                    // Tolerant, coherent with the skip-unreadable rule below: a kind with no registered descriptor is
                    // omitted rather than shown with an invented label, and it never breaks the rest of the listing. In
                    // practice unreachable, because the store only ever produces registered kinds.
                    var entry = EntryOf(path, project, registry);
                    if (entry == null)
                    {
                        unreadable.Add(Path.GetFileName(path));
                        continue;
                    }

                    entries.Add(entry);
                }
                catch
                {
                    // Skip unreadable/foreign files rather than fail the whole listing; the reading says which they are.
                    unreadable.Add(Path.GetFileName(path));
                }
            }

            unreadable.Sort(StringComparer.CurrentCultureIgnoreCase);
            return new RackDesignLibraryListing(
                folder, true, entries.OrderByDescending(entry => entry.ModifiedUtc).ToList(), unreadable);
        }

        /// <summary>
        /// The entry of ONE design file already loaded, with the rules of the list (its name and its label); null when its kind has no
        /// registered descriptor. The RackCad window opens a chosen design in its classic editor with it (T4 «ut», UT-03), so that editor
        /// receives the same entry the classic library window would have given it.
        /// </summary>
        public static RackDesignLibraryEntry EntryOf(string path, RackProject project, SystemRegistry registry)
        {
            if (project == null || registry == null || !registry.TryGet(project.Kind, out var descriptor))
            {
                return null;
            }

            var fallback = StripExtension(path);

            // Prefer the payload's own name when it has one; otherwise the file name. Cama (FlowBed) and dinámico
            // (DynamicDesign) deliberately fall back to the file name — their payload names are not consulted.
            var payloadName = project.Header?.Name
                ?? project.SelectiveRack?.Name
                ?? project.Larguero?.Name;
            var name = !string.IsNullOrWhiteSpace(payloadName) ? payloadName.Trim() : fallback;

            return new RackDesignLibraryEntry(path, name, project.Kind, descriptor.LibraryLabel, File.GetLastWriteTimeUtc(path));
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
