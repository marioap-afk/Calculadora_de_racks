using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

internal static class Program
{
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101;

    [STAThread]
    private static void Main()
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        foreach (var mode in new[] { "none", "keydown-all-but-tab", "previewkeydown-all" })
        {
            var box = new TextBox();
            var keyDowns = 0; var textInputs = 0;
            box.KeyDown += (s, e) => { keyDowns++; if (mode == "keydown-all-but-tab" && e.Key != Key.Tab) e.Handled = true; };
            box.PreviewKeyDown += (s, e) => { if (mode == "previewkeydown-all") e.Handled = true; };
            box.PreviewTextInput += (s, e) => textInputs++;
            var w = new Window { Content = box, Width = 300, Height = 120, ShowActivated = true, Title = "wpfkeys " + mode };
            w.Show();
            var h = new WindowInteropHelper(w).Handle;
            SetForegroundWindow(h);
            w.Activate();
            box.Focus();
            Pump(300);
            var focused = ReferenceEquals(Keyboard.FocusedElement, box);
            // VK 'A' = 0x41, scan code 0x1E
            PostMessage(h, WM_KEYDOWN, (IntPtr)0x41, (IntPtr)0x001E0001);
            PostMessage(h, WM_KEYUP, (IntPtr)0x41, unchecked((IntPtr)(int)0xC01E0001));
            Pump(500);
            Console.WriteLine($"mode={mode} focused={focused} keyDowns={keyDowns} textInputs={textInputs} text='{box.Text}'");
            w.Close();
        }
        app.Shutdown();
    }

    private static void Pump(int ms)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(ms) };
        timer.Tick += (s, e) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
