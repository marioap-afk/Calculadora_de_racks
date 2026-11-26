using System;
using System.Collections.Generic;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// What one reading of the folder of the design library found (I-64 A-14, line D, fourth batch, part «ut», UT-06): the designs it could
    /// read, the files it could not (by their file name, so the page of the window can say how many and which instead of skipping them in
    /// silence), and whether the folder exists at all (UT: a folder that does not exist is not an empty folder). Pure data, no IO.
    /// </summary>
    public sealed class RackDesignLibraryListing
    {
        public RackDesignLibraryListing(string folder, bool folderExists, IReadOnlyList<RackDesignLibraryEntry> entries, IReadOnlyList<string> unreadable)
        {
            Folder = folder ?? string.Empty;
            FolderExists = folderExists;
            Entries = entries ?? Array.Empty<RackDesignLibraryEntry>();
            Unreadable = unreadable ?? Array.Empty<string>();
        }

        /// <summary>The folder that was read.</summary>
        public string Folder { get; }

        /// <summary>False when the folder does not exist (it is created when the first design is saved).</summary>
        public bool FolderExists { get; }

        /// <summary>The designs read, from the most recent to the oldest (the order of <see cref="RackDesignLibrary.List(string)"/>).</summary>
        public IReadOnlyList<RackDesignLibraryEntry> Entries { get; }

        /// <summary>The file names (with their extension) of the <c>.rackcad.json</c> of the folder that could not be read, in name order.</summary>
        public IReadOnlyList<string> Unreadable { get; }
    }
}
