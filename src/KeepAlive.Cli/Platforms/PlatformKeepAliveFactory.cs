using System.Runtime.InteropServices;

internal static class PlatformKeepAliveFactory
{
    public static bool IsSupported => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();

    public static IPlatformKeepAlive Create()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsKeepAlive();

        if (OperatingSystem.IsMacOS())
            return new MacOSKeepAlive();

        throw new PlatformNotSupportedException(
            $"KeepAlive supports Windows and macOS. Current platform: {RuntimeInformation.OSDescription}");
    }
}
