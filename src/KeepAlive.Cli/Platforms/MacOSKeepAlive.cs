using System.Runtime.Versioning;
using static MacOSNativeMethods;

[SupportedOSPlatform("macos")]
internal sealed class MacOSKeepAlive : IPlatformKeepAlive
{
    private const string AssertionName = "KeepAlive CLI";

    private readonly IntPtr _assertionNameRef = CFStringCreateWithCString(IntPtr.Zero, AssertionName, kCFStringEncodingUTF8);
    private uint _sleepAssertionId;
    private uint _userActivityAssertionId;

    public string Name => "macOS";

    public void PreventSleep()
    {
        if (_sleepAssertionId != 0)
            return;

        var type = CFStringCreateWithCString(IntPtr.Zero, kIOPMAssertionTypePreventUserIdleDisplaySleep, kCFStringEncodingUTF8);
        try
        {
            var result = IOPMAssertionCreateWithName(type, kIOPMAssertionLevelOn, _assertionNameRef, out _sleepAssertionId);
            if (result != kIOReturnSuccess)
                throw new InvalidOperationException($"IOPMAssertionCreateWithName failed with IOReturn 0x{result:X8}");
        }
        finally
        {
            CFRelease(type);
        }
    }

    public void AllowSleep()
    {
        if (_sleepAssertionId != 0)
        {
            IOPMAssertionRelease(_sleepAssertionId);
            _sleepAssertionId = 0;
        }

        if (_userActivityAssertionId != 0)
        {
            IOPMAssertionRelease(_userActivityAssertionId);
            _userActivityAssertionId = 0;
        }
    }

    public void SimulateActivity()
    {
        // Tell power management the user is active (resets display idle timer, wakes a dimmed display).
        IOPMAssertionDeclareUserActivity(_assertionNameRef, kIOPMUserActiveLocal, ref _userActivityAssertionId);

        // Post an F15 key press so the HID idle timer resets. Apps such as Teams read that timer
        // to decide whether the user is "Away".
        PostKey(kVK_F15, keyDown: true);
        PostKey(kVK_F15, keyDown: false);
    }

    private static void PostKey(ushort virtualKey, bool keyDown)
    {
        var evt = CGEventCreateKeyboardEvent(IntPtr.Zero, virtualKey, keyDown);
        if (evt == IntPtr.Zero)
            return;

        try
        {
            CGEventPost(kCGHIDEventTap, evt);
        }
        finally
        {
            CFRelease(evt);
        }
    }

    public void Dispose()
    {
        AllowSleep();

        if (_assertionNameRef != IntPtr.Zero)
            CFRelease(_assertionNameRef);
    }
}
