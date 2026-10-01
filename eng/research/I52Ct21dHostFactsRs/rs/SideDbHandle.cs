using Autodesk.AutoCAD.DatabaseServices;
using I52Ct21d.HostFacts.Rs.Core;

namespace I52Ct21d.HostFacts.Rs;

/// <summary>
/// A side database that the RS DLL created itself (never a product database, never an open document's). It holds the host <c>Database</c>
/// and its identity in the <see cref="SideDbLedger"/>; disposing it disposes the host database and marks the ledger entry. Only
/// <see cref="SideDbWriter"/> constructs it (the constructor is internal and the scan pins its callers), so a <c>SideDbHandle</c> cannot wrap a
/// database the instrument did not create. Nothing outside <see cref="SideDbWriter"/>, <see cref="SideDbReader"/> and <see cref="ScratchSaver"/>
/// may read <see cref="Database"/> (the scan pins the callers of the getter).
/// </summary>
internal sealed class SideDbHandle : IDisposable
{
    private readonly SideDbLedger _ledger;
    private bool _disposed;

    internal Database Database { get; }

    internal string Id { get; }

    internal SideDbHandle(Database database, string id, SideDbLedger ledger)
    {
        Database = database;
        Id = id;
        _ledger = ledger;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { Database.Dispose(); }
        finally { _ledger.MarkDisposed(Id); }
    }
}
