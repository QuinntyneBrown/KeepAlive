using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

internal static class PlatformKeepAliveFactory
{
    public static bool IsSupported => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();

    public static IPlatformKeepAlive Create(ILoggerFactory loggerFactory)
    {
        if (OperatingSystem.IsWindows())
            return new WindowsKeepAlive();

        if (OperatingSystem.IsMacOS())
            return new MacOSKeepAlive(loggerFactory.CreateLogger<MacOSKeepAlive>());

        throw new PlatformNotSupportedException(
            $"KeepAlive supports Windows and macOS. Current platform: {RuntimeInformation.OSDescription}");
    }
}
