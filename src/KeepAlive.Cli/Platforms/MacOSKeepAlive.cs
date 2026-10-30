using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using static MacOSNativeMethods;

[SupportedOSPlatform("macos")]
internal sealed class MacOSKeepAlive : IPlatformKeepAlive
{
    private const string AssertionName = "KeepAlive CLI";

    private readonly ILogger<MacOSKeepAlive> _logger;
    private readonly IntPtr _assertionNameRef = CFStringCreateWithCString(IntPtr.Zero, AssertionName, kCFStringEncodingUTF8);
    private uint _sleepAssertionId;
    private uint _userActivityAssertionId;
    private bool? _accessibilityTrusted;

    public MacOSKeepAlive(ILogger<MacOSKeepAlive> logger)
    {
        _logger = logger;
    }

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

        // Posting synthetic key events requires Accessibility access; without it CGEventPost is a no-op.
        if (!EnsureAccessibilityTrusted())
            return;

        // Post an F15 key press so the HID idle timer resets. Apps such as Teams read that timer
        // to decide whether the user is "Away".
        PostKey(kVK_F15, keyDown: true);
        PostKey(kVK_F15, keyDown: false);
    }

    /// <summary>
    /// Checks Accessibility access. The first check asks macOS to show its permission prompt;
    /// later checks are silent and log once when access is granted mid-run.
    /// </summary>
    private bool EnsureAccessibilityTrusted()
    {
        if (_accessibilityTrusted is null)
        {
            _accessibilityTrusted = IsAccessibilityTrustedWithPrompt();
            if (!_accessibilityTrusted.Value)
            {
                _logger.LogWarning(
                    "Accessibility access is not granted, so simulated keypresses are skipped. Sleep is still prevented, " +
                    "but apps that watch for input (e.g. Teams) may mark you idle. Allow your terminal app in " +
                    "System Settings > Privacy & Security > Accessibility; keypresses start automatically once it is granted.");
            }

            return _accessibilityTrusted.Value;
        }

        if (!_accessibilityTrusted.Value && AXIsProcessTrusted())
        {
            _accessibilityTrusted = true;
            _logger.LogInformation("Accessibility access granted. Simulated keypresses are now active.");
        }

        return _accessibilityTrusted.Value;
    }

    private static bool IsAccessibilityTrustedWithPrompt()
    {
        var key = CFStringCreateWithCString(IntPtr.Zero, kAXTrustedCheckOptionPrompt, kCFStringEncodingUTF8);
        var options = IntPtr.Zero;
        try
        {
            var kCFBooleanTrue = Marshal.ReadIntPtr(GetCoreFoundationExport("kCFBooleanTrue"));
            options = CFDictionaryCreate(
                IntPtr.Zero,
                [key],
                [kCFBooleanTrue],
                1,
                GetCoreFoundationExport("kCFTypeDictionaryKeyCallBacks"),
                GetCoreFoundationExport("kCFTypeDictionaryValueCallBacks"));

            return AXIsProcessTrustedWithOptions(options);
        }
        finally
        {
            if (options != IntPtr.Zero)
                CFRelease(options);
            CFRelease(key);
        }
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
