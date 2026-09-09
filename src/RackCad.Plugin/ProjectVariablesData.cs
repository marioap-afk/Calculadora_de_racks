using System;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;

namespace RackCad.Plugin
{
    /// <summary>
    /// The ONLY code that touches the drawing to store the project-variable register: the Named Objects
    /// Dictionary, one entry, one <see cref="Xrecord"/>, the JSON chunked into ≤255-character strings.
    ///
    /// <para>
    /// The NOD is the store because its uniqueness is STRUCTURAL — there is exactly one per
    /// <see cref="Database"/> — and because it is not part of the block table, so the drawing-wide sweep of
    /// rack definitions never sees it and needs no new exclusion. The alternative that would have cost less
    /// to write, hanging the register off Model Space's extension dictionary, has uniqueness only by
    /// convention: every paper layout is another block table record, and the entry would be claiming to
    /// belong to a block it has nothing to do with.
    /// </para>
    /// <para>
    /// This type reports what it FOUND and decides nothing about what it means. Versions, unknown types and
    /// unknown definition kinds are schema questions, and they are answered in Application where the test
    /// suite can reach them — no suite can load this assembly (ADR-0003).
    /// </para>
    /// <para>
    /// The distinction it must not lose is ABSENT versus PRESENT-BUT-UNREADABLE. A missing entry is a drawing
    /// with no variables, which is valid; an entry that exists and cannot be read is corruption, and reporting
    /// it as "no variables" would let the next write erase every variable the drawing had.
    /// </para>
    /// <para>Must be called inside an open transaction.</para>
    /// </summary>
    internal static class ProjectVariablesData
    {
        /// <summary>The entry of the Named Objects Dictionary that holds the drawing's register.</summary>
        public const string DictKey = "RACKCAD_PROJECT";

        private const int ChunkSize = 255;

        /// <summary>
        /// What the drawing holds. Never throws: an AutoCAD failure while reading is reported as
        /// PRESENT-BUT-UNREADABLE, which fails closed — the register is not overwritten on a read this build
        /// could not complete.
        /// </summary>
        public static ProjectVariablesPayload Read(Transaction transaction, Database database)
        {
            if (transaction == null || database == null)
            {
                return ProjectVariablesPayload.Unreadable(
                    "No hay transacción o dibujo con el que leer las variables de proyecto.");
            }

            try
            {
                var dictionary = (DBDictionary)transaction.GetObject(
                    database.NamedObjectsDictionaryId,
                    OpenMode.ForRead);

                if (!dictionary.Contains(DictKey))
                {
                    return ProjectVariablesPayload.Absent();
                }

                if (!(transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForRead) is Xrecord xrecord))
                {
                    return ProjectVariablesPayload.Unreadable(
                        "La entrada '" + DictKey + "' del dibujo existe pero no es un Xrecord de RackCad.");
                }

                if (xrecord.Data == null)
                {
                    return ProjectVariablesPayload.Unreadable(
                        "La entrada '" + DictKey + "' del dibujo existe pero no contiene datos.");
                }

                var builder = new StringBuilder();

                foreach (TypedValue value in xrecord.Data)
                {
                    if (value.TypeCode == (int)DxfCode.Text)
                    {
                        builder.Append(value.Value as string);
                    }
                }

                return builder.Length == 0
                    ? ProjectVariablesPayload.Unreadable(
                        "La entrada '" + DictKey + "' del dibujo existe pero no contiene texto legible.")
                    : ProjectVariablesPayload.Present(builder.ToString());
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("Leer las variables de proyecto del dibujo", ex);

                return ProjectVariablesPayload.Unreadable(
                    "No se pudo leer la entrada '" + DictKey + "' del dibujo: " + ex.Message);
            }
        }

        /// <summary>
        /// Writes the register. The caller is responsible for having established that writing is allowed —
        /// see <see cref="ProjectVariablesRegistry"/>, which is where that guard is applied.
        /// </summary>
        public static void Write(Transaction transaction, Database database, string json)
        {
            if (transaction == null || database == null || string.IsNullOrEmpty(json))
            {
                return;
            }

            var dictionary = (DBDictionary)transaction.GetObject(
                database.NamedObjectsDictionaryId,
                OpenMode.ForWrite);

            var buffer = new ResultBuffer();

            for (var i = 0; i < json.Length; i += ChunkSize)
            {
                buffer.Add(new TypedValue(
                    (int)DxfCode.Text,
                    json.Substring(i, Math.Min(ChunkSize, json.Length - i))));
            }

            if (dictionary.Contains(DictKey))
            {
                var existing = (Xrecord)transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForWrite);
                existing.Data = buffer;
                return;
            }

            var xrecord = new Xrecord { Data = buffer };
            dictionary.SetAt(DictKey, xrecord);
            transaction.AddNewlyCreatedDBObject(xrecord, true);
        }
    }
}
