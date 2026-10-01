namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>
/// Pure path arithmetic used by the designation and by the scratch-root guard. Comparison is ordinal and case-insensitive on the
/// normalized full path (Windows paths). It is a PATH check, not a file-identity check: a hard link, a junction or an 8.3 short name
/// that aliases another path is not detected here (the reparse-point inspection below catches junctions and symbolic links only).
/// </summary>
public static class PathRegions
{
    public static string Norm(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    /// <summary>True when the path is the root of a drive or share (<c>C:\</c>, <c>\server\share\</c>): Norm would trim it to a non-root text.</summary>
    public static bool IsDriveRoot(string path)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetPathRoot(full);
        return !string.IsNullOrEmpty(root) && string.Equals(full, root, StringComparison.OrdinalIgnoreCase);
    }

    public static bool Same(string a, string b) => string.Equals(Norm(a), Norm(b), StringComparison.OrdinalIgnoreCase);

    /// <summary>True when <paramref name="child"/> is <paramref name="parent"/> or lies inside it.</summary>
    public static bool IsInside(string child, string parent)
    {
        var c = Norm(child);
        var p = Norm(parent);
        if (string.Equals(c, p, StringComparison.OrdinalIgnoreCase)) return true;
        return c.StartsWith(p + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    public static bool Overlaps(string a, string b) => IsInside(a, b) || IsInside(b, a);

    public static string DirOf(string path) => Path.GetDirectoryName(Norm(path)) ?? "";

    /// <summary>
    /// Why a path text is not acceptable as a declared absolute path, or an empty string. Refused: not fully qualified, device paths
    /// (<c>\\?\</c>, <c>\\.\</c>), a <c>..</c> segment in the raw text, a colon after the drive (alternate data stream marker), a
    /// segment that ends with a dot or a space, a control character.
    /// </summary>
    public static string TextProblem(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "EMPTY";
        if (!Path.IsPathFullyQualified(path)) return "NOT_ABSOLUTE";
        if (path.StartsWith("\\\\?\\", StringComparison.Ordinal) || path.StartsWith("\\\\.\\", StringComparison.Ordinal)) return "DEVICE_PATH";
        foreach (var ch in path)
            if (ch < ' ') return "CONTROL_CHARACTER";
        if (IsDriveRoot(path)) return "DRIVE_ROOT";
        var colon = path.IndexOf(':', 2 < path.Length ? 2 : path.Length);
        if (colon >= 0) return "ALTERNATE_STREAM_MARKER";
        foreach (var segment in path.Split('\\', '/'))
        {
            if (segment == "..") return "PARENT_SEGMENT";
            if (segment.Length > 0 && segment != "." && (segment.EndsWith('.') || segment.EndsWith(' '))) return "TRAILING_DOT_OR_SPACE";
        }
        return "";
    }

    /// <summary>True when any segment of the normalized path is named <c>src</c> (the product source tree must never be a root).</summary>
    public static bool HasSrcSegment(string path)
    {
        foreach (var segment in Norm(path).Split(Path.DirectorySeparatorChar))
            if (string.Equals(segment, "src", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>
    /// The first reparse point (junction or symbolic link) among the path itself and every existing ancestor, or an empty string.
    /// Reads attributes only.
    /// </summary>
    public static string FirstReparsePoint(string path)
    {
        var current = Norm(path);
        while (!string.IsNullOrEmpty(current))
        {
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                return current;
            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrEmpty(parent) || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase)) break;
            current = parent;
        }
        return "";
    }
}
