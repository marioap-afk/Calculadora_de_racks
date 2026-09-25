using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using CoreApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: ExtensionApplication(typeof(I52Ctda.ManagedObserver.ObserverApplication))]

namespace I52Ctda.ManagedObserver;

// Loaded only by DRIVER-SCRIPT-01 `_.NETLOAD` for rows that register RR-MANAGED-CMD (and by the R3 smoke). Initialize
// only hands the registration entry to R-NATIVE-ARX; DRIVER-CMD-01 subscribes the exact scratch document in REGISTER
// and CLN-BASE unsubscribes it. The assembly stays resident until process exit and then holds no governed state.
public sealed class ObserverApplication : IExtensionApplication
{
    private static readonly NativeLogSequencer.ManagedEntry EntryDelegate = Entry;  // kept alive for the process
    private static ManagedCommandObserver? observer;
    private static Document? subscribedDocument;

    public void Initialize()
    {
        try
        {
            NativeLogSequencer.InstallResolver();
            if (NativeLogSequencer.NativeModuleLoaded()) NativeLogSequencer.BindEntry(EntryDelegate);
        }
        catch (System.Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
            // Cannot bind LOG-SEQ-01: the module records nothing and the driver's REGISTER finds no entry (UNKNOWN).
        }
    }

    public void Terminate() { }

    private static int Entry(int subscribe, ulong documentId, IntPtr probeId)
    {
        try
        {
            string probe = Marshal.PtrToStringUni(probeId) ?? "";
            observer ??= new ManagedCommandObserver(new NativeLogSequencer(), probe);
            Document? document = null;
            foreach (Document candidate in CoreApplication.DocumentManager)
                if ((ulong)candidate.UnmanagedObject.ToInt64() == documentId) document = candidate;
            ulong database = document is null ? 0 : (ulong)document.Database.UnmanagedObject.ToInt64();
            if (subscribe == ManagedCommandObserver.SmokeFenceRead) return observer.OnSmokeFenceRead(documentId, database);
            if (subscribe == 1)
            {
                if (document is null || subscribedDocument is not null) { observer.OnSubscribed(documentId, database, false); return 1; }
                document.CommandEnded += OnCommandEnded;
                subscribedDocument = document;
                observer.OnSubscribed(documentId, database, true);
                return 0;
            }
            if (subscribedDocument is null) return 1;
            subscribedDocument.CommandEnded -= OnCommandEnded;
            subscribedDocument = null;
            observer.OnUnsubscribed(documentId, database);
            return 0;
        }
        catch (System.Exception)
        {
            return 2;
        }
    }

    private static void OnCommandEnded(object? sender, CommandEventArgs e)
    {
        if (observer is null || sender is not Document document) return;
        observer.OnCommandEnded(e.GlobalCommandName, (ulong)document.UnmanagedObject.ToInt64(), (ulong)document.Database.UnmanagedObject.ToInt64());
    }
}
