using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace I52Ctda.ControlPlane.V35;

// D-3 fail-closed guard. A governed run is human-free only if AutoCAD never waits on a modal window. A modal window
// disables the window that owns it, so the control plane samples the exact PID's top-level windows while it runs: a
// visible, enabled window whose owner chain holds a visible, disabled window of the same PID is a modal (interactive)
// state. Confirmed across consecutive samples, the control plane terminates the PID at once (nobody may answer it) and
// records the windows it saw, so the row can only be UNKNOWN.
public sealed record V35WindowSnapshot(bool Modal, IReadOnlyList<string> Windows);

public sealed class V35InteractiveWatch(int requiredConsecutiveSamples = 4)
{
    private int consecutive;

    public string? Observed { get; private set; }

    // True once an interactive state is confirmed (sticky).
    public bool Sample(V35WindowSnapshot snapshot)
    {
        if (Observed is not null) return true;
        consecutive = snapshot.Modal ? consecutive + 1 : 0;
        if (consecutive < requiredConsecutiveSamples) return false;
        Observed = "modal window over a disabled owner; top-level windows: " + string.Join(" | ", snapshot.Windows);
        return true;
    }
}

public static class V35Windows
{
    private delegate bool EnumWindowsProc(IntPtr window, IntPtr parameter);
    private const uint GwOwner = 4;

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr window, uint command);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder text, int capacity);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, StringBuilder text, int capacity);

    public static V35WindowSnapshot Snapshot(Process process)
    {
        uint pid = (uint)process.Id;
        var mine = new List<IntPtr>();
        EnumWindows((window, _) =>
        {
            if (GetWindowThreadProcessId(window, out uint owner) != 0 && owner == pid) mine.Add(window);
            return true;
        }, IntPtr.Zero);
        var set = mine.ToHashSet();
        bool modal = false;
        var windows = new List<string>();
        foreach (IntPtr window in mine.Where(IsWindowVisible))
        {
            var title = new StringBuilder(256); var cls = new StringBuilder(256);
            GetWindowText(window, title, title.Capacity); GetClassName(window, cls, cls.Capacity);
            windows.Add($"{cls}:'{title}'{(IsWindowEnabled(window) ? "" : " disabled")}");
            if (!IsWindowEnabled(window)) continue;
            IntPtr owner = GetWindow(window, GwOwner);
            for (int depth = 0; owner != IntPtr.Zero && depth < 16; depth++, owner = GetWindow(owner, GwOwner))
                if (set.Contains(owner) && IsWindowVisible(owner) && !IsWindowEnabled(owner)) { modal = true; break; }
        }
        return new(modal, windows);
    }
}
