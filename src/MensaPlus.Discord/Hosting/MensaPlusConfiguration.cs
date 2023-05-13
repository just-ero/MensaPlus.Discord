using System;

using MensaPlus.Discord.Commands;
using MensaPlus.Discord.Handlers;
using MensaPlus.Discord.Interactions;
using MensaPlus.Discord.Parsers;
using MensaPlus.Discord.Providers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenMensa;

using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Interactivity.Extensions;

namespace MensaPlus.Discord.Hosting;

public static class MensaPlusConfiguration
{
    public static string AddDiscordService(IServiceProvider serviceProvider)
    {
        IConfiguration config = serviceProvider.GetRequiredService<IConfiguration>();
        string? token = config.GetValue<string?>("MENSAPLUS_BOT_TOKEN");

        if (token is null)
        {
            string msg = "Bot token was not provided. Set the 'MENSAPLUS_BOT_TOKEN' environment variable to a valid token.";
            throw new InvalidOperationException(msg);
        }

        return token;
    }

    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        _ = services
            .AddMemoryCache()
            .AddHttpClient()
            .AddTransient<OpenMensaApi>();

        _ = services
            .Configure<DiscordGatewayClientOptions>(ConfigureClientOptions);

        _ = services
            .AddDiscordCommands(true)
            .AddCommandTree()
                .WithCommandGroup<MenuCommands>()
                .Finish();

        _ = services
            .AddInteractivity()
            .AddInteractionGroup<CategorySelectInteractions>();

        _ = services
            .AddParser<DateOnlyParser>();

        _ = services
            .AddAutocompleteProvider<CanteenAutocompleteProvider>();

        _ = services
            .AddPreparationErrorEvent<PreparationErrorHandler>()
            .AddPostExecutionEvent<PostExecutionHandler>();
    }

    private static void ConfigureClientOptions(DiscordGatewayClientOptions options)
    {
        options.Intents
            |= GatewayIntents.MessageContents;
    }

    public static void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        _ = loggingBuilder
            .AddConsole()
            .AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning)
            .AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning);
    }
}
