using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>One file of a package: a simple name and its bytes. The bytes are copied in and copied out.</summary>
    public sealed class StructuralPackageFile
    {
        private readonly byte[] _content;

        public StructuralPackageFile(string name, byte[] content)
        {
            Name = name;
            _content = content == null ? null : (byte[])content.Clone();
        }

        public string Name { get; }

        /// <summary>A copy of the bytes, or <c>null</c> when there were none.</summary>
        public byte[] GetContent() => _content == null ? null : (byte[])_content.Clone();
    }

    /// <summary>The stable codes of a refused or failed write. A code is never renamed or reused.</summary>
    public static class StructuralPackageWriteCodes
    {
        public const string DestinationInvalid = "PACKAGE-DESTINATION-INVALID";
        public const string ParentMissing = "PACKAGE-PARENT-MISSING";
        public const string DestinationExists = "PACKAGE-DESTINATION-EXISTS";
        public const string NoFiles = "PACKAGE-NO-FILES";
        public const string FileNameInvalid = "PACKAGE-FILE-NAME-INVALID";
        public const string FileNameDuplicate = "PACKAGE-FILE-NAME-DUPLICATE";
        public const string ContentMissing = "PACKAGE-CONTENT-MISSING";
        public const string WriteFailed = "PACKAGE-WRITE-FAILED";
        public const string PublishFailed = "PACKAGE-PUBLISH-FAILED";
    }

    /// <summary>The result of writing a package: published whole, or not at all.</summary>
    public sealed class StructuralPackageWriteResult
    {
        internal StructuralPackageWriteResult(bool succeeded, string code, string subject, string detail, bool cleanupFailed)
        {
            Succeeded = succeeded;
            Code = code;
            Subject = subject;
            Detail = detail;
            CleanupFailed = cleanupFailed;
        }

        public bool Succeeded { get; }

        /// <summary>One of <see cref="StructuralPackageWriteCodes"/>, or <c>null</c> when the write succeeded.</summary>
        public string Code { get; }

        /// <summary>What the code is about (a path or a file name), or <c>null</c>.</summary>
        public string Subject { get; }

        /// <summary>The message of the failure that was caught, or <c>null</c>.</summary>
        public string Detail { get; }

        /// <summary>True when, after a failure, the temporary folder could not be removed. It is the only way a failed write leaves anything behind.</summary>
        public bool CleanupFailed { get; }
    }

    /// <summary>
    /// The seam through which the writer reaches the disk. It exists so that a failure can be injected into every step; the product
    /// uses <see cref="StructuralPackageDiskFileSystem"/>.
    /// </summary>
    public interface IStructuralPackageFileSystem
    {
        bool DirectoryExists(string path);

        bool FileExists(string path);

        void CreateDirectory(string path);

        /// <summary>Creates the file and writes the bytes; it fails when the file already exists.</summary>
        void CreateFile(string path, byte[] content);

        /// <summary>Renames <paramref name="source"/> to <paramref name="destination"/>; it fails when the destination exists.</summary>
        void MoveDirectory(string source, string destination);

        void DeleteDirectory(string path);
    }

    public sealed class StructuralPackageDiskFileSystem : IStructuralPackageFileSystem
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);

        public bool FileExists(string path) => File.Exists(path);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public void CreateFile(string path, byte[] content)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(content, 0, content.Length);
                stream.Flush(true);
            }
        }

        public void MoveDirectory(string source, string destination) => Directory.Move(source, destination);

        public void DeleteDirectory(string path) => Directory.Delete(path, true);
    }

    /// <summary>
    /// Writes a package of files into a NEW folder, atomically (I-65 D-08, INV-05). It is neutral: it knows nothing of what the files
    /// are for.
    ///
    /// <para>
    /// The destination must not exist and its parent must. The files are written into a temporary folder that is a sibling of the
    /// destination and the package is published with ONE rename. A destination that already exists is refused without touching
    /// anything: nothing is ever overwritten. Whatever fails, in the writing of any file or in the rename, no destination folder and no
    /// temporary folder remain. Names must be simple (no separators, no <c>.</c> or <c>..</c>), unique and usable on every system.
    /// </para>
    /// <para>
    /// The type is a publisher, not a "writer": an F1 guard (<c>StructuralAnalysisModelBuilderTests</c>) forbids a type name with that word in the neutral
    /// namespace. The file keeps the name <c>StructuralPackageWriter.cs</c> that the contract of F3-T2 fixes.
    /// </para>
    /// </summary>
    public sealed class StructuralPackagePublisher
    {
        private const string InvalidNameCharacters = "/\\:<>\"|?*";

        private readonly IStructuralPackageFileSystem _fileSystem;

        public StructuralPackagePublisher()
            : this(new StructuralPackageDiskFileSystem())
        {
        }

        public StructuralPackagePublisher(IStructuralPackageFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public StructuralPackageWriteResult Write(string destinationFolder, IEnumerable<StructuralPackageFile> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            var list = files.ToList();
            var refusal = Validate(destinationFolder, list, out var parent);

            if (refusal != null)
            {
                return refusal;
            }

            var temporary = Path.Combine(parent, Path.GetFileName(destinationFolder) + ".tmp-" + Guid.NewGuid().ToString("N"));

            try
            {
                _fileSystem.CreateDirectory(temporary);

                foreach (var file in list)
                {
                    _fileSystem.CreateFile(Path.Combine(temporary, file.Name), file.GetContent());
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                return Failed(StructuralPackageWriteCodes.WriteFailed, destinationFolder, exception, temporary);
            }

            try
            {
                _fileSystem.MoveDirectory(temporary, destinationFolder);
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                return Failed(StructuralPackageWriteCodes.PublishFailed, destinationFolder, exception, temporary);
            }

            return new StructuralPackageWriteResult(true, null, destinationFolder, null, false);
        }

        private StructuralPackageWriteResult Validate(string destinationFolder, List<StructuralPackageFile> files, out string parent)
        {
            parent = null;

            if (string.IsNullOrWhiteSpace(destinationFolder) || string.IsNullOrEmpty(Path.GetFileName(destinationFolder)))
            {
                return Refused(StructuralPackageWriteCodes.DestinationInvalid, destinationFolder);
            }

            parent = Path.GetDirectoryName(destinationFolder);

            if (string.IsNullOrEmpty(parent))
            {
                return Refused(StructuralPackageWriteCodes.DestinationInvalid, destinationFolder);
            }

            if (files.Count == 0)
            {
                return Refused(StructuralPackageWriteCodes.NoFiles, destinationFolder);
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                if (file == null || !IsSimpleName(file.Name))
                {
                    return Refused(StructuralPackageWriteCodes.FileNameInvalid, file?.Name);
                }

                if (!seen.Add(file.Name))
                {
                    return Refused(StructuralPackageWriteCodes.FileNameDuplicate, file.Name);
                }

                if (file.GetContent() == null)
                {
                    return Refused(StructuralPackageWriteCodes.ContentMissing, file.Name);
                }
            }

            if (!_fileSystem.DirectoryExists(parent))
            {
                return Refused(StructuralPackageWriteCodes.ParentMissing, parent);
            }

            if (_fileSystem.DirectoryExists(destinationFolder) || _fileSystem.FileExists(destinationFolder))
            {
                return Refused(StructuralPackageWriteCodes.DestinationExists, destinationFolder);
            }

            return null;
        }

        /// <summary>A simple name: not blank, not <c>.</c> or <c>..</c>, no separator, no reserved or control character, no edge space or dot.</summary>
        private static bool IsSimpleName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == ".." || name.Contains("..", StringComparison.Ordinal))
            {
                return false;
            }

            if (name != name.Trim() || name.EndsWith(".", StringComparison.Ordinal))
            {
                return false;
            }

            return name.All(character => character >= 0x20 && InvalidNameCharacters.IndexOf(character) < 0);
        }

        private StructuralPackageWriteResult Failed(string code, string destinationFolder, Exception exception, string temporary)
        {
            var cleanupFailed = false;

            try
            {
                if (_fileSystem.DirectoryExists(temporary))
                {
                    _fileSystem.DeleteDirectory(temporary);
                }
            }
            catch (Exception cleanup) when (!(cleanup is OutOfMemoryException))
            {
                cleanupFailed = true;
            }

            return new StructuralPackageWriteResult(false, code, destinationFolder, exception.Message, cleanupFailed);
        }

        private static StructuralPackageWriteResult Refused(string code, string subject) =>
            new StructuralPackageWriteResult(false, code, subject, null, false);
    }
}
