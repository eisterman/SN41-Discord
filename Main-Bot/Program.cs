using Main_Bot;
using Main_Bot.Features;
using Main_Bot.Features.GoodByePhrases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

if (builder.Configuration["Discord:Token"] == null)
    throw new InvalidOperationException("Discord Token is not configured.");

builder.Services
    .AddOptions<SN41Settings>()
    .Bind(builder.Configuration.GetSection("SN41"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddSingleton<IGoodByePhrasesProvider, GoodByePhrasesAssemblyProvider>()
    .AddSingleton<AntiSpamLogicSingleton>();

builder.Services
    .AddHttpClient()
    .AddDiscordGateway(options => { options.Intents = GatewayIntents.All; })
    .AddApplicationCommands()
    .AddGatewayHandlers(typeof(Program).Assembly);

var host = builder.Build();

// Add application commands and component interaction from Modules
// The modules behave as if they were transient services, so they are created for each command/interaction,
// but you can use classic Dependency Injection in them.
host.AddModules(typeof(Program).Assembly);

await host.RunAsync();