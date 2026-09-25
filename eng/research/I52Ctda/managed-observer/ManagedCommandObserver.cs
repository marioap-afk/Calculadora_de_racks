namespace I52Ctda.ManagedObserver;

// One LOG-RECORD-01 record as supplied by R-MANAGED-OBSERVER (Sequence, PID, TID and timestamp are assigned by the
// single LOG-SEQ-01 sequencer of R-NATIVE-ARX).
public sealed record ManagedRecord(string ProbeId, string StageId, ulong DeliveryId, string DriverOrSchedulerId, string ModuleId,
    ulong DocumentId, ulong DatabaseId, string CommandIdentity, string EventOrMarkerId, string Payload);

// The LOG-SEQ-01 functions R-MANAGED-OBSERVER imports (catalog R-MANAGED-OBSERVER.imports).
public interface ILogSequencer
{
    ulong LogAppend(ManagedRecord record);
    int TokenSet(string tokenId, ulong deliveryId);
    int FinishFenceIsSet();
}

// RR-MANAGED-CMD: the managed Document.CommandEnded observation of HEC-V27-C1 row 09E (M-DOC-END). It is a marker
// source only: it never runs a probe body and adds no EventId, callback, scheduler or support action.
public sealed class ManagedCommandObserver(ILogSequencer log, string probeId)
{
    public const string ModuleId = "R-MANAGED-OBSERVER";
    public const string Registration = "RR-MANAGED-CMD";
    public const string FixtureCommand = "I52CTDA_FIXTURE";
    public const ulong DeliveryCurrent = 0;
    public const ulong DeliveryNew = ulong.MaxValue;
    public const string StageCurrent = "*";
    public const string CommandCurrent = "*";

    public bool Subscribed { get; private set; }

    public void OnSubscribed(ulong documentId, ulong databaseId, bool subscribed)
    {
        Subscribed = subscribed;
        Append(StageCurrent, DeliveryCurrent, documentId, databaseId, Registration, $"phase={(subscribed ? "SUBSCRIBED" : "SUBSCRIBE-FAILED")};source=Document.CommandEnded");
    }

    public void OnUnsubscribed(ulong documentId, ulong databaseId)
    {
        Subscribed = false;
        Append(StageCurrent, DeliveryCurrent, documentId, databaseId, Registration, "phase=UNSUBSCRIBED");
    }

    // Every callback reads FINISH-FENCE-01 first (I52Ctda_FinishFenceIsSet) and never sets it.
    public void OnCommandEnded(string globalCommandName, ulong documentId, ulong databaseId)
    {
        string command = Sanitize(globalCommandName);
        if (log.FinishFenceIsSet() != 0)
        {
            Append("STG-FIXTURE-CMD", DeliveryNew, documentId, databaseId, "FINISH-FENCE-01",
                $"kind=LATE-DELIVERY;notifierAccess=NONE;registration={Registration};module={ModuleId};command={command}");
            return;
        }
        Append(StageCurrent, DeliveryCurrent, documentId, databaseId, Registration, $"phase=COMMAND-ENDED;command={command}");
        if (!string.Equals(command, FixtureCommand, StringComparison.OrdinalIgnoreCase)) return;
        // MARK-MANAGED-CMD-END@STG-FIXTURE-CMD binds to Document.CommandEnded of I52CTDA_FIXTURE.
        Append("STG-FIXTURE-CMD", DeliveryCurrent, documentId, databaseId, "MARK-MANAGED-CMD-END", $"command={command};source=Document.CommandEnded");
        log.TokenSet("TOK-MANAGED-CMD-END", DeliveryCurrent);
    }

    private void Append(string stage, ulong delivery, ulong documentId, ulong databaseId, string eventOrMarker, string payload) =>
        log.LogAppend(new ManagedRecord(probeId, stage, delivery, "DRIVER-CMD-01", ModuleId, documentId, databaseId, CommandCurrent, eventOrMarker, payload));

    public static string Sanitize(string? value) => (value ?? "").Replace(';', '_').Replace('=', '_');
}
