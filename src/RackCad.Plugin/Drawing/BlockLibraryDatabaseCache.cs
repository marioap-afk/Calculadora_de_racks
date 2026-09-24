using System;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Diagnostics;

namespace RackCad.Plugin.Drawing
{
    /// <summary>Keeps one parsed side database for the current external block-library file signature.</summary>
    internal static class BlockLibraryDatabaseCache
    {
        private static readonly object CacheGate = new object();
        private static string cachedPath;
        private static DateTime cachedWriteUtc;
        private static long cachedLength;
        private static Database cachedLibrary;

        internal static Database Acquire(string path)
        {
            lock (CacheGate)
            {
                var info = new FileInfo(path);
                var writeUtc = info.LastWriteTimeUtc;
                var length = info.Length;

                if (cachedLibrary != null
                    && string.Equals(cachedPath, path, StringComparison.OrdinalIgnoreCase)
                    && cachedWriteUtc == writeUtc
                    && cachedLength == length)
                {
                    return cachedLibrary;
                }

                var fresh = new Database(false, true);
                try
                {
                    fresh.ReadDwgFile(
                        path,
                        FileOpenMode.OpenForReadAndAllShare,
                        allowCPConversion: true,
                        password: null);
                }
                catch (Exception ex)
                {
                    RackLog.Exception("Leer blocks-library.dwg (" + path + ")", ex);
                    fresh.Dispose();
                    return null;
                }

                cachedLibrary?.Dispose();
                cachedLibrary = fresh;
                cachedPath = path;
                cachedWriteUtc = writeUtc;
                cachedLength = length;
                return cachedLibrary;
            }
        }
    }
}
