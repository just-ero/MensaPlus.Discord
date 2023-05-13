using System;

using MensaPlus.Discord.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Remora.Discord.Commands.Services;
using Remora.Results;

IHost host = MensaPlusHost.Build(args);

IServiceProvider services = host.Services;
ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();

SlashService slashService = services.GetRequiredService<SlashService>();
Result updateSlash = await slashService.UpdateSlashCommandsAsync();
if (!updateSlash.IsSuccess)
{
    logger.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error.Message);
}

await host.RunAsync();
