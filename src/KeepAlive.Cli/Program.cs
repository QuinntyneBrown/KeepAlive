using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var intervalOption = new Option<int?>(
    name: "--interval",
    description: "Interval in seconds between activity simulation");
intervalOption.AddAlias("-i");

var rootCommand = new RootCommand(
    "Keeps your machine awake and maintains Microsoft Teams presence")
{
    intervalOption
};

rootCommand.SetHandler(async (int? interval) =>
{
    if (!PlatformKeepAliveFactory.IsSupported)
    {
        Console.Error.WriteLine(
            $"KeepAlive supports Windows and macOS only. Current platform: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        Environment.ExitCode = 1;
        return;
    }

    var builder = Host.CreateApplicationBuilder();

    builder.Services.Configure<KeepAliveOptions>(
        builder.Configuration.GetSection("KeepAlive"));

    if (interval.HasValue)
        builder.Services.PostConfigure<KeepAliveOptions>(
            o => o.IntervalSeconds = interval.Value);

    builder.Services.AddSingleton(_ => PlatformKeepAliveFactory.Create());
    builder.Services.AddHostedService<KeepAliveWorker>();

    using var host = builder.Build();
    await host.RunAsync();
}, intervalOption);

return await rootCommand.InvokeAsync(args);
