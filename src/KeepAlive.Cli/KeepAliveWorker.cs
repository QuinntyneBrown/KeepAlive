using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class KeepAliveWorker : BackgroundService
{
    private readonly ILogger<KeepAliveWorker> _logger;
    private readonly KeepAliveOptions _options;

    public KeepAliveWorker(ILogger<KeepAliveWorker> logger, IOptions<KeepAliveOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "KeepAlive started (interval: {Interval}s). Press Ctrl+C to stop.",
            _options.IntervalSeconds);

        SetPowerState(awake: true);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                SimulateInput();
                _logger.LogDebug("Activity simulated");
                await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected on shutdown
        }
        finally
        {
            SetPowerState(awake: false);
            _logger.LogInformation("KeepAlive stopped. Normal power behaviour restored.");
        }
    }

    private void SetPowerState(bool awake)
    {
        uint flags = awake
            ? NativeMethods.ES_CONTINUOUS
                | NativeMethods.ES_SYSTEM_REQUIRED
                | NativeMethods.ES_DISPLAY_REQUIRED
            : NativeMethods.ES_CONTINUOUS;

        NativeMethods.SetThreadExecutionState(flags);
    }

    private static void SimulateInput()
    {
        var inputs = new NativeMethods.INPUT[2];

        // F15 key down — harmless key not mapped on modern keyboards
        inputs[0].type = NativeMethods.INPUT_KEYBOARD;
        inputs[0].union.ki.wVk = NativeMethods.VK_F15;

        // F15 key up
        inputs[1].type = NativeMethods.INPUT_KEYBOARD;
        inputs[1].union.ki.wVk = NativeMethods.VK_F15;
        inputs[1].union.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

        NativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<NativeMethods.INPUT>());
    }
}
