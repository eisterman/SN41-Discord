using Main_Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

var builder = Host.CreateApplicationBuilder(args);

if (builder.Configuration["Discord:Token"] == null)
    throw new InvalidOperationException("Discord Token is not configured.");

builder.Services
    .AddOptions<SN41Settings>()
    .Bind(builder.Configuration.GetSection("SN41"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddDiscordGateway(options => { options.Intents = GatewayIntents.All; })
    .AddGatewayHandlers(typeof(Program).Assembly);

var host = builder.Build();

await host.RunAsync();