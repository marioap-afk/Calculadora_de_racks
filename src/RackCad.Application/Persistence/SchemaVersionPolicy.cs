using System.Globalization;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Single source of the schema-version rules shared by every persisted document (RackProjectDocument, FlowBedDocument,
    /// LargueroDocument, RackEmbedDocument). Two decisions:
    ///
    /// <list type="bullet">
    /// <item>READABILITY (<see cref="IsReadable"/>): a stored version whose MAJOR is higher than this build's is NOT
    /// readable; a missing/unparseable/older-or-same major is readable (legacy tolerance).</item>
    /// <item>WRITE VERSION (<see cref="ResolveWriteVersion"/>): the version to stamp when re-serializing a loaded document,
    /// so a re-save NEVER downgrades a newer same-major minor and NEVER copies an unparseable value verbatim.</item>
    /// </list>
    ///
    /// Pure: no I/O, no exceptions on parse (an unparseable version is treated as legacy). Version comparison is on the
    /// numeric MAJOR.MINOR; the current version is always a constant this build controls.
    /// </summary>
    public static class SchemaVersionPolicy
    {
        private const int LegacyMajor = 1;

        /// <summary>
        /// True when a document stored as <paramref name="storedVersion"/> can be read by a build writing
        /// <paramref name="currentVersion"/>: only a strictly higher MAJOR is unreadable (missing/unparseable = legacy = readable).
        /// </summary>
        public static bool IsReadable(string storedVersion, string currentVersion)
            => MajorOf(storedVersion) <= MajorOf(currentVersion);

        /// <summary>
        /// The SchemaVersion to stamp when re-writing a document loaded as <paramref name="storedVersion"/>, by a build
        /// whose version is <paramref name="currentVersion"/>:
        /// <list type="bullet">
        /// <item>stored missing / unparseable / OLDER major =&gt; <paramref name="currentVersion"/> (upgrade; never copy junk);</item>
        /// <item>stored SAME major, newer minor =&gt; the stored version (never downgrade a newer file we can still read);</item>
        /// <item>stored SAME major, older-or-equal minor =&gt; <paramref name="currentVersion"/>;</item>
        /// <item>stored NEWER major =&gt; <paramref name="currentVersion"/> (defensive; such a document is rejected at read).</item>
        /// </list>
        /// </summary>
        public static string ResolveWriteVersion(string storedVersion, string currentVersion)
        {
            if (!TryParse(storedVersion, out var storedMajor, out var storedMinor)
                || !TryParse(currentVersion, out var currentMajor, out var currentMinor)
                || storedMajor != currentMajor)
            {
                return currentVersion;
            }

            // Same major: keep whichever minor is higher so a re-save never downgrades a newer file.
            return storedMinor > currentMinor ? storedVersion.Trim() : currentVersion;
        }

        /// <summary>The MAJOR component; 1 when missing/unparseable (legacy default), matching the historical behavior.</summary>
        public static int MajorOf(string version) => TryParse(version, out var major, out _) ? major : LegacyMajor;

        private static bool TryParse(string version, out int major, out int minor)
        {
            major = LegacyMajor;
            minor = 0;
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            var parts = version.Trim().Split('.');
            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out major) || major <= 0)
            {
                major = LegacyMajor;
                return false;
            }

            if (parts.Length > 1)
            {
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minor);
            }

            return true;
        }
    }
}
