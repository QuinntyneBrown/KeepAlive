using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var intervalOption = new Option<int?>(
    name: "--interval",
    description: "Interval in seconds between activity simulation");
intervalOption.AddAlias("-i");

var rootCommand = new RootCommand(
    "Keeps Windows alive and maintains Microsoft Teams presence")
{
    intervalOption
};

rootCommand.SetHandler(async (int? interval) =>
{
    var builder = Host.CreateApplicationBuilder();

    builder.Services.Configure<KeepAliveOptions>(
        builder.Configuration.GetSection("KeepAlive"));

    if (interval.HasValue)
        builder.Services.PostConfigure<KeepAliveOptions>(
            o => o.IntervalSeconds = interval.Value);

    builder.Services.AddHostedService<KeepAliveWorker>();

    using var host = builder.Build();
    await host.RunAsync();
}, intervalOption);

return await rootCommand.InvokeAsync(args);
