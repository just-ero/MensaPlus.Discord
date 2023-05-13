using Microsoft.Extensions.Hosting;

using Remora.Discord.Hosting.Extensions;

namespace MensaPlus.Discord.Hosting;

public static class MensaPlusHost
{
    public static IHost Build(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .AddDiscordService(MensaPlusConfiguration.AddDiscordService)
            .ConfigureServices(MensaPlusConfiguration.ConfigureServices)
            .ConfigureLogging(MensaPlusConfiguration.ConfigureLogging)
            .UseConsoleLifetime()
            .Build();
    }
}
