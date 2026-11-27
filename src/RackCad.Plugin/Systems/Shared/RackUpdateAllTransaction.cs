using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>
    /// T6 at (I-64, sexta tanda): the ONE outer transaction of «Actualizar todo» (RACKACTUALIZARTODO). The apply of every system that the
    /// door commands of the window already use runs inside it: each one starts its own transactions through the transaction manager, which
    /// nest in this one, so nothing they write is in the drawing until <see cref="Commit"/>, and <see cref="Abort"/> discards all of it. It
    /// is opened inside the command (whose context holds the document lock) and never from the palette; the files of the door of the window
    /// do not open transactions, so it lives here.
    /// </summary>
    internal sealed class RackUpdateAllTransaction : IDisposable
    {
        private Transaction _transaction;
        private bool _committed;

        private RackUpdateAllTransaction(Transaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>Opens the outer transaction on the database of the document of the command.</summary>
        internal static RackUpdateAllTransaction Begin(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            return new RackUpdateAllTransaction(document.Database.TransactionManager.StartTransaction());
        }

        /// <summary>Whether a transaction is open: the one of the operation, or one that something else left open (then it is not ours).</summary>
        internal static bool AnyOpen(Document document) => document?.Database.TransactionManager.TopTransaction != null;

        internal bool IsOpen => _transaction != null;

        /// <summary>Confirms everything the operation wrote; it may throw, and then nothing is confirmed by it.</summary>
        internal void Commit()
        {
            if (_transaction == null) throw new InvalidOperationException("La transacción de «Actualizar todo» ya está cerrada.");
            _transaction.Commit();
            _committed = true;
            Close();
        }

        /// <summary>Discards everything the operation wrote; idempotent.</summary>
        internal void Abort()
        {
            if (_transaction == null) return;
            try
            {
                if (!_committed) _transaction.Abort();
            }
            finally
            {
                Close();
            }
        }

        public void Dispose() => Abort();

        private void Close()
        {
            var transaction = _transaction;
            _transaction = null;
            transaction?.Dispose();
        }
    }
}
