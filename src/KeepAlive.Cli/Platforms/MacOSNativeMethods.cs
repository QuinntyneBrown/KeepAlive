using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("macos")]
internal static class MacOSNativeMethods
{
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string IOKit = "/System/Library/Frameworks/IOKit.framework/IOKit";
    private const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    internal const uint kCFStringEncodingUTF8 = 0x08000100;
    internal const uint kIOPMAssertionLevelOn = 255;
    internal const uint kIOPMUserActiveLocal = 0;
    internal const int kIOReturnSuccess = 0;

    /// <summary>Prevents the display (and therefore the system) from idle-sleeping.</summary>
    internal const string kIOPMAssertionTypePreventUserIdleDisplaySleep = "PreventUserIdleDisplaySleep";

    internal const uint kCGHIDEventTap = 0;

    /// <summary>Virtual key code for F15 on macOS (unmapped on modern keyboards).</summary>
    internal const ushort kVK_F15 = 0x71;

    // CoreFoundation

    [DllImport(CoreFoundation, CharSet = CharSet.Ansi)]
    internal static extern IntPtr CFStringCreateWithCString(IntPtr allocator, string cStr, uint encoding);

    [DllImport(CoreFoundation)]
    internal static extern void CFRelease(IntPtr cf);

    // IOKit power management

    [DllImport(IOKit)]
    internal static extern int IOPMAssertionCreateWithName(
        IntPtr assertionType, uint assertionLevel, IntPtr assertionName, out uint assertionId);

    [DllImport(IOKit)]
    internal static extern int IOPMAssertionRelease(uint assertionId);

    [DllImport(IOKit)]
    internal static extern int IOPMAssertionDeclareUserActivity(
        IntPtr assertionName, uint userType, ref uint assertionId);

    // CoreGraphics events

    [DllImport(CoreGraphics)]
    internal static extern IntPtr CGEventCreateKeyboardEvent(
        IntPtr source, ushort virtualKey, [MarshalAs(UnmanagedType.I1)] bool keyDown);

    [DllImport(CoreGraphics)]
    internal static extern void CGEventPost(uint tap, IntPtr eventRef);
}
