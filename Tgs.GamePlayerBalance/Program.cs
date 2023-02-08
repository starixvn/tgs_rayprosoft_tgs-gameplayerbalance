using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GamePlayerBalance.MessageHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
            fluentOptions.UseMessagePack();
        });
    });

// Add services to the container.

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
        options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
    foreach (var xmlFileName in xmlFileNames) c.IncludeXmlComments(xmlFileName);
});

builder.Services.AddGamePlayerBalance(
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.ConnectionString = configuration.GetConnectionString("Mongo")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.IPAddress = builder.Configuration.GetSection("POD_IP").Value
        ?? Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Single().ToString();
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalance:GameCommandClientBaseUrl");
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalance:GameProvidersClientBaseUrl");
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalance:GameFundTransferClientBaseUrl");
    });

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        _ = natsConfig
        .ConfigQueueOptions((options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.Url = configuration.GetConnectionString("Nats")!;
        })
        .AddHandler<BrandFormQueuedMessageHandler>("game.playerbalance.brand.form.queued", "tgs.game.playerbalance.brand.form")
        .AddHandler<BrandFormProcessingMessageHandler>("game.playerbalance.brand.form.processing", "tgs.game.playerbalance.brand.form")
        .AddHandler<ImportFormQueuedMessageHandler>("game.playerbalance.import.form.queued", "tgs.game.playerbalance.import.form")
        .AddHandler<ImportFormProcessingMessageHandler>("game.playerbalance.import.form.processing", "tgs.game.playerbalance.import.form")
        .AddHandler<PlayerFormQueuedMessageHandler>("game.playerbalance.player.form.queued", "tgs.game.playerbalance.player.form");
    })

    .ClearAllExchanges()
    .AddNatsGlobPatternExchange("game.playerbalance.brand.form.queued")
    .AddNatsGlobPatternExchange("game.playerbalance.brand.form.processing")
    .AddNatsGlobPatternExchange("game.playerbalance.import.form.queued")
    .AddNatsGlobPatternExchange("game.playerbalance.import.form.processing")
    .AddNatsGlobPatternExchange("game.playerbalance.player.form.queued");

builder.Services
     .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
        sp => new ConfigureNamedOptions<OtlpExporterOptions>(
            Options.DefaultName,
            exporterOptions =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var nodeIp = config.GetValue<string>("NODE_IP");

                if (!string.IsNullOrWhiteSpace(nodeIp))
                    exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

                config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
            }))
    .AddOpenTelemetryTracing(traceBuilder => traceBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddGrpcClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .AddProfileViewer()
    .Configure<AspNetCoreInstrumentationOptions>(options =>
    {
        options.Filter = httpContext =>
        {
            return !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/healthz")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !httpContext.Request.Path.StartsWithSegments("/hangfire")
                && !httpContext.Request.Path.StartsWithSegments("/metrics")
                   && !new[] { "html", "ico", "png", "jpg" }.Any(
                       ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
        };
    });

builder.Services
    .AddHealthChecks();

var app = builder.Build();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

_ = app.UseAuthorization();

_ = app.MapControllers();

app.MapProfileViewer();

app.MapHealthChecks("/healthz");

_ = app.UseGameRecordRegister();

app.Run();
