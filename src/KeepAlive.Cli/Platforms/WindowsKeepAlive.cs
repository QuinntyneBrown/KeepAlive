using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
internal sealed class WindowsKeepAlive : IPlatformKeepAlive
{
    public string Name => "Windows";

    public void PreventSleep() =>
        WindowsNativeMethods.SetThreadExecutionState(
            WindowsNativeMethods.ES_CONTINUOUS
            | WindowsNativeMethods.ES_SYSTEM_REQUIRED
            | WindowsNativeMethods.ES_DISPLAY_REQUIRED);

    public void AllowSleep() =>
        WindowsNativeMethods.SetThreadExecutionState(WindowsNativeMethods.ES_CONTINUOUS);

    public void SimulateActivity()
    {
        var inputs = new WindowsNativeMethods.INPUT[2];

        // F15 key down — harmless key not mapped on modern keyboards
        inputs[0].type = WindowsNativeMethods.INPUT_KEYBOARD;
        inputs[0].union.ki.wVk = WindowsNativeMethods.VK_F15;

        // F15 key up
        inputs[1].type = WindowsNativeMethods.INPUT_KEYBOARD;
        inputs[1].union.ki.wVk = WindowsNativeMethods.VK_F15;
        inputs[1].union.ki.dwFlags = WindowsNativeMethods.KEYEVENTF_KEYUP;

        WindowsNativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<WindowsNativeMethods.INPUT>());
    }

    public void Dispose() => AllowSleep();
}
