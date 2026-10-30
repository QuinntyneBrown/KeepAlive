using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class KeepAliveWorker : BackgroundService
{
    private readonly ILogger<KeepAliveWorker> _logger;
    private readonly KeepAliveOptions _options;
    private readonly IPlatformKeepAlive _platform;

    public KeepAliveWorker(
        ILogger<KeepAliveWorker> logger,
        IOptions<KeepAliveOptions> options,
        IPlatformKeepAlive platform)
    {
        _logger = logger;
        _options = options.Value;
        _platform = platform;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "KeepAlive started on {Platform} (interval: {Interval}s). Press Ctrl+C to stop.",
            _platform.Name,
            _options.IntervalSeconds);

        _platform.PreventSleep();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _platform.SimulateActivity();
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
            _platform.AllowSleep();
            _logger.LogInformation("KeepAlive stopped. Normal power behaviour restored.");
        }
    }
}
