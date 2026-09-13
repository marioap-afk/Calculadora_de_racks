using System;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;

namespace RackCad.Plugin
{
    /// <summary>
    /// The ONLY code that touches the drawing to store the Project collection of custom properties (I-54 D-07 / ADR-0039
    /// §3): the Named Objects Dictionary, one entry, one DIRECT <see cref="Xrecord"/>, the text chunked into ≤255-character
    /// strings.
    ///
    /// <para>
    /// The entry is its own: never a child of another entry and never part of another document. The collection is read,
    /// accredited and blocked on its own, so a drawing register this build cannot read does not lock the properties, and
    /// a collection it cannot read does not lock anything else (D-07.6).
    /// </para>
    /// <para>
    /// This type reports what it FOUND and decides nothing about what it means: the text is interpreted only by
    /// <see cref="CustomPropertiesStore"/>, where the test suite can reach it — no suite can load this assembly (ADR-0003).
    /// The distinction it must not lose is ABSENT versus PRESENT-BUT-UNREADABLE: an entry that exists and cannot be read
    /// is never reported as "no properties", or the next write would replace it.
    /// </para>
    /// <para>
    /// The chunking repeats the one of the other drawing-level register on purpose (D-07.2): extracting it is debt F-13,
    /// not work of I-54. Must be called inside an open transaction that the CALLER owns.
    /// </para>
    /// </summary>
    internal static class CustomPropertiesData
    {
        /// <summary>The entry of the Named Objects Dictionary that holds the Project collection. Contractual: frozen from the first write.</summary>
        public const string DictKey = "RACKCAD_CUSTOM_PROPERTIES";

        private const int ChunkSize = 255;

        /// <summary>
        /// What the drawing holds, as the tri-state payload the store classifies. Never throws because of what the drawing
        /// contains: an entry that is not an Xrecord, an Xrecord without data or without text, and a failure of AutoCAD's
        /// own access are all PRESENT-BUT-UNREADABLE, which fails closed.
        /// </summary>
        public static CustomPropertiesPayload Read(Transaction transaction, Database database)
        {
            if (transaction == null || database == null)
            {
                return CustomPropertiesPayload.Unreadable(
                    "No hay transacción o dibujo con el que leer las propiedades personalizadas del proyecto.");
            }

            try
            {
                if (!(transaction.GetObject(database.NamedObjectsDictionaryId, OpenMode.ForRead) is DBDictionary dictionary))
                {
                    return CustomPropertiesPayload.Unreadable(
                        "El diccionario de objetos con nombre del dibujo no se pudo abrir como diccionario.");
                }

                if (!dictionary.Contains(DictKey))
                {
                    return CustomPropertiesPayload.Absent();
                }

                if (!(transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForRead) is Xrecord xrecord))
                {
                    return CustomPropertiesPayload.Unreadable(
                        "La entrada '" + DictKey + "' del dibujo existe pero no es un Xrecord de RackCad.");
                }

                using (var data = xrecord.Data)
                {
                    if (data == null)
                    {
                        return CustomPropertiesPayload.Unreadable(
                            "La entrada '" + DictKey + "' del dibujo existe pero no contiene datos.");
                    }

                    // The chunks, in the order they were stored. Whatever is not text is not part of the collection.
                    var builder = new StringBuilder();

                    foreach (TypedValue value in data)
                    {
                        if (value.TypeCode == (int)DxfCode.Text && value.Value is string chunk)
                        {
                            builder.Append(chunk);
                        }
                    }

                    return builder.Length == 0
                        ? CustomPropertiesPayload.Unreadable(
                            "La entrada '" + DictKey + "' del dibujo existe pero no contiene texto legible.")
                        : CustomPropertiesPayload.Present(builder.ToString());
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                // Only AutoCAD's own access failures. Anything else is a defect of this code, and disguising it as an
                // unreadable drawing would hide it.
                RackLog.Exception("Leer las propiedades personalizadas del proyecto", ex);

                return CustomPropertiesPayload.Unreadable(
                    "No se pudo leer la entrada '" + DictKey + "' del dibujo: " + ex.Message);
            }
        }

        /// <summary>
        /// Writes the collection as ONE direct Xrecord at <see cref="DictKey"/>, the text split into chunks of at most 255
        /// characters, every one of them <see cref="DxfCode.Text"/>. The text arrives serialized and accredited: the caller
        /// read the entry in THIS transaction, Application classified it as Absent or Readable and the write guard allowed
        /// the write (D-07.5). The store writes ASCII-only JSON, so no chunk boundary can split a surrogate pair.
        /// </summary>
        /// <exception cref="InvalidOperationException">The entry exists and is not an Xrecord: it is never replaced.</exception>
        public static void Write(Transaction transaction, Database database, string text)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("No hay texto de propiedades personalizadas que escribir.", nameof(text));
            }

            var dictionary = (DBDictionary)transaction.GetObject(database.NamedObjectsDictionaryId, OpenMode.ForRead);

            using (var buffer = new ResultBuffer())
            {
                for (var start = 0; start < text.Length; start += ChunkSize)
                {
                    buffer.Add(new TypedValue((int)DxfCode.Text, text.Substring(start, Math.Min(ChunkSize, text.Length - start))));
                }

                if (dictionary.Contains(DictKey))
                {
                    if (!(transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForRead) is Xrecord existing))
                    {
                        throw new InvalidOperationException(
                            "La entrada '" + DictKey + "' del dibujo no es un Xrecord, así que no se reemplaza.");
                    }

                    existing.UpgradeOpen();
                    existing.Data = buffer;
                    return;
                }

                var xrecord = new Xrecord();
                xrecord.Data = buffer;

                dictionary.UpgradeOpen();
                dictionary.SetAt(DictKey, xrecord);
                transaction.AddNewlyCreatedDBObject(xrecord, true);
            }
        }
    }
}
