using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>One file of a package. RED skeleton.</summary>
    public sealed class StructuralPackageFile
    {
        public StructuralPackageFile(string name, byte[] content) => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public byte[] GetContent() => throw new NotImplementedException();
    }

    /// <summary>The stable codes of a refused or failed write. RED skeleton: the values are not assigned yet.</summary>
    public static class StructuralPackageWriteCodes
    {
        public const string DestinationInvalid = "";
        public const string ParentMissing = "";
        public const string DestinationExists = "";
        public const string NoFiles = "";
        public const string FileNameInvalid = "";
        public const string FileNameDuplicate = "";
        public const string ContentMissing = "";
        public const string WriteFailed = "";
        public const string PublishFailed = "";
    }

    /// <summary>The result of writing a package. RED skeleton.</summary>
    public sealed class StructuralPackageWriteResult
    {
        public bool Succeeded => throw new NotImplementedException();

        public string Code => throw new NotImplementedException();

        public string Subject => throw new NotImplementedException();

        public string Detail => throw new NotImplementedException();

        public bool CleanupFailed => throw new NotImplementedException();
    }

    /// <summary>The seam through which the publisher reaches the disk.</summary>
    public interface IStructuralPackageFileSystem
    {
        bool DirectoryExists(string path);

        bool FileExists(string path);

        void CreateDirectory(string path);

        void CreateFile(string path, byte[] content);

        void MoveDirectory(string source, string destination);

        void DeleteDirectory(string path);
    }

    /// <summary>The disk file system. RED skeleton.</summary>
    public sealed class StructuralPackageDiskFileSystem : IStructuralPackageFileSystem
    {
        public bool DirectoryExists(string path) => throw new NotImplementedException();

        public bool FileExists(string path) => throw new NotImplementedException();

        public void CreateDirectory(string path) => throw new NotImplementedException();

        public void CreateFile(string path, byte[] content) => throw new NotImplementedException();

        public void MoveDirectory(string source, string destination) => throw new NotImplementedException();

        public void DeleteDirectory(string path) => throw new NotImplementedException();
    }

    /// <summary>
    /// Publishes a package of files into a NEW folder, atomically (I-65 D-08, INV-05). RED skeleton: it compiles and does nothing yet.
    /// The type is a publisher, not a "writer": an F1 guard forbids a type name with that word in the neutral namespace.
    /// </summary>
    public sealed class StructuralPackagePublisher
    {
        public StructuralPackagePublisher() => throw new NotImplementedException();

        public StructuralPackagePublisher(IStructuralPackageFileSystem fileSystem) => throw new NotImplementedException();

        public StructuralPackageWriteResult Write(string destinationFolder, IEnumerable<StructuralPackageFile> files) =>
            throw new NotImplementedException();
    }
}
