using System.Reflection;
using System.Runtime.InteropServices;

namespace I52Ctda.ManagedObserver;

// [DllImport("I52CtdaNative.arx")] bindings of LOG-SEQ-01, resolved against the already-loaded R-NATIVE-ARX module
// (GetModuleHandleW), never by loading another copy. A failed binding records nothing (UNK-LOG-BINDING).
public sealed class NativeLogSequencer : ILogSequencer
{
    public const string NativeModule = "I52CtdaNative.arx";

    [StructLayout(LayoutKind.Sequential)]
    private struct I52CtdaRecord
    {
        public uint size;
        public IntPtr probeId;
        public IntPtr stageId;
        public ulong deliveryId;
        public IntPtr driverOrSchedulerId;
        public IntPtr moduleId;
        public ulong documentId;
        public ulong databaseId;
        public IntPtr commandIdentity;
        public IntPtr eventOrMarkerId;
        public IntPtr payload;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int ManagedEntry(int subscribe, ulong documentId, IntPtr probeId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern IntPtr GetModuleHandleW(string moduleName);

    [DllImport(NativeModule, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern ulong I52Ctda_LogAppend(ref I52CtdaRecord record);

    [DllImport(NativeModule, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int I52Ctda_TokenSet(string tokenId, ulong deliveryId);

    [DllImport(NativeModule, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int I52Ctda_FinishFenceIsSet();

    [DllImport(NativeModule, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int I52Ctda_BindManagedObserver(IntPtr entry);

    private static int resolverInstalled;

    public static void InstallResolver()
    {
        if (Interlocked.Exchange(ref resolverInstalled, 1) == 1) return;
        NativeLibrary.SetDllImportResolver(typeof(NativeLogSequencer).Assembly, (string name, Assembly _, DllImportSearchPath? _) =>
            string.Equals(name, NativeModule, StringComparison.OrdinalIgnoreCase) ? GetModuleHandleW(NativeModule) : IntPtr.Zero);
    }

    public static bool NativeModuleLoaded() => GetModuleHandleW(NativeModule) != IntPtr.Zero;

    // Implementation helper (not LOG-SEQ-01): hands the RR-MANAGED-CMD registration entry to R-NATIVE-ARX.
    public static bool BindEntry(ManagedEntry entry) => I52Ctda_BindManagedObserver(Marshal.GetFunctionPointerForDelegate(entry)) == 1;

    public ulong LogAppend(ManagedRecord record)
    {
        var strings = new List<IntPtr>();
        IntPtr S(string value) { IntPtr p = Marshal.StringToHGlobalUni(value); strings.Add(p); return p; }
        try
        {
            var native = new I52CtdaRecord
            {
                size = (uint)Marshal.SizeOf<I52CtdaRecord>(),
                probeId = S(record.ProbeId),
                stageId = S(record.StageId),
                deliveryId = record.DeliveryId,
                driverOrSchedulerId = S(record.DriverOrSchedulerId),
                moduleId = S(record.ModuleId),
                documentId = record.DocumentId,
                databaseId = record.DatabaseId,
                commandIdentity = S(record.CommandIdentity),
                eventOrMarkerId = S(record.EventOrMarkerId),
                payload = S(record.Payload),
            };
            return I52Ctda_LogAppend(ref native);
        }
        finally
        {
            foreach (IntPtr p in strings) Marshal.FreeHGlobal(p);
        }
    }

    public int TokenSet(string tokenId, ulong deliveryId) => I52Ctda_TokenSet(tokenId, deliveryId);
    public int FinishFenceIsSet() => I52Ctda_FinishFenceIsSet();

    public static int RecordSize => Marshal.SizeOf<I52CtdaRecord>();
}
