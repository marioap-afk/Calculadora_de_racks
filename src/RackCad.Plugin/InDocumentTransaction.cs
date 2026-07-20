using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09): run a body inside the standard "lock the document, start a transaction, commit,
    /// dispose" boilerplate that the commands repeat.
    ///
    /// USE ONLY for simple, mechanically-equivalent single-phase work. The body must not let any <c>DBObject</c>
    /// escape the transaction — the return value carries the plain data read inside (ids, strings, structs), never a
    /// live object. This helper must NOT wrap <c>Regen</c>, purges performed after the commit, or multi-phase flows;
    /// those keep their explicit hand-written transaction blocks. It does not change lock mode, OpenMode,
    /// forceValidity or commit order versus the boilerplate it replaces.
    /// </summary>
    internal static class InDocumentTransaction
    {
        /// <summary>Lock + start transaction + commit around a body that returns a value. No DBObject may escape.</summary>
        public static T Run<T>(Document document, Func<Transaction, T> body)
        {
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var result = body(transaction);
                transaction.Commit();
                return result;
            }
        }

        /// <summary>Lock + start transaction + commit around a body with no return value.</summary>
        public static void Run(Document document, Action<Transaction> body)
        {
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                body(transaction);
                transaction.Commit();
            }
        }
    }
}
