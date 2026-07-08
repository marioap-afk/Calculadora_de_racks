using System;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// Reads/writes a rack's serialized design (JSON, incl. its Id + Name) on a block reference's extension
    /// dictionary, as an <see cref="Xrecord"/> whose data is the JSON chunked into ≤255-char strings. This is
    /// what lets a drawn rack be reopened and edited. AutoCAD-only; the JSON is produced by
    /// <c>SelectivePalletDesignStore</c>. Must be called inside an open write/read transaction.
    /// </summary>
    internal static class RackBlockData
    {
        /// <summary>Key under the entity's extension dictionary that holds the rack payload.</summary>
        public const string DictKey = "RACKCAD_SELECTIVE";

        private const int ChunkSize = 255;

        public static void Write(Transaction transaction, ObjectId entityId, string json)
        {
            if (transaction == null || entityId.IsNull || string.IsNullOrEmpty(json))
            {
                return;
            }

            var entity = (DBObject)transaction.GetObject(entityId, OpenMode.ForWrite);
            if (entity.ExtensionDictionary.IsNull)
            {
                entity.CreateExtensionDictionary();
            }

            var dictionary = (DBDictionary)transaction.GetObject(entity.ExtensionDictionary, OpenMode.ForWrite);

            var buffer = new ResultBuffer();
            for (var i = 0; i < json.Length; i += ChunkSize)
            {
                buffer.Add(new TypedValue((int)DxfCode.Text, json.Substring(i, Math.Min(ChunkSize, json.Length - i))));
            }

            if (dictionary.Contains(DictKey))
            {
                var existing = (Xrecord)transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForWrite);
                existing.Data = buffer;
            }
            else
            {
                var xrecord = new Xrecord { Data = buffer };
                dictionary.SetAt(DictKey, xrecord);
                transaction.AddNewlyCreatedDBObject(xrecord, true);
            }
        }

        public static string Read(Transaction transaction, ObjectId entityId)
        {
            if (transaction == null || entityId.IsNull)
            {
                return null;
            }

            var entity = (DBObject)transaction.GetObject(entityId, OpenMode.ForRead);
            if (entity.ExtensionDictionary.IsNull)
            {
                return null;
            }

            var dictionary = (DBDictionary)transaction.GetObject(entity.ExtensionDictionary, OpenMode.ForRead);
            if (!dictionary.Contains(DictKey))
            {
                return null;
            }

            var xrecord = (Xrecord)transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForRead);
            if (xrecord?.Data == null)
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (TypedValue value in xrecord.Data)
            {
                if (value.TypeCode == (int)DxfCode.Text)
                {
                    builder.Append(value.Value as string);
                }
            }

            return builder.Length == 0 ? null : builder.ToString();
        }
    }
}
