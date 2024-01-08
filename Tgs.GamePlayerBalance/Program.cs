using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.MessageHandlers;

var filterInstrumentations = new[] { "html", "ico", "png", "jpg" };

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
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
    foreach (var xmlFileName in xmlFileNames)
    {
        c.IncludeXmlComments(xmlFileName);
    }
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
        options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalance:GameCommandClientBaseUrl")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalance:GameProvidersClientBaseUrl")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalance:GameFundTransferClientBaseUrl")!;
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
        .AddHandler<BrandFormByAwcAelottoQueuedMessageHandler>(NatsConst.GamePlayerBalanceBrandFormByAwcAelotto, NatsConst.GamePlayerBalanceBrandFormByAwcAelotto)
        .AddHandler<BrandFormByAwcAwsQueuedMessageHandler>(NatsConst.GamePlayerBalanceBrandFormByAwcAws, NatsConst.GamePlayerBalanceBrandFormByAwcAws)
        .AddHandler<BrandFormByAwcFacaiQueuedMessageHandler>(NatsConst.GamePlayerBalanceBrandFormByAwcFacai, NatsConst.GamePlayerBalanceBrandFormByAwcFacai)
        .AddHandler<BrandFormQueuedMessageHandler>(NatsConst.GamePlayerBalanceBrandFormQueued, NatsConst.GamePlayerBalanceBrandFormQueued)
        .AddHandler<BrandFormProcessingMessageHandler>(NatsConst.GamePlayerBalanceBrandFormProcessing, NatsConst.GamePlayerBalanceBrandFormProcessing)
        .AddHandler<ImportFormQueuedMessageHandler>(NatsConst.GamePlayerBalanceImportFormQueued, NatsConst.GamePlayerBalanceImportFormQueued)
        .AddHandler<ImportFormProcessingMessageHandler>(NatsConst.GamePlayerBalanceImportFormProcessing, NatsConst.GamePlayerBalanceImportFormProcessing)
        .AddHandler<PlayerFormQueuedMessageHandler>(NatsConst.GamePlayerBalancePlayerFormQueued, NatsConst.GamePlayerBalancePlayerFormQueued)
        .AddProcessor<PlayerBalanceMessageProcessor>(NatsConst.GamePlayerBalancePlayerBalance, NatsConst.GamePlayerBalancePlayerBalance)
        .AddHandler<PlayerBalanceAddMessageHandler>(NatsConst.GamePlayerBalancePlayerBalanceAdd, NatsConst.GamePlayerBalancePlayerBalanceAdd)
        .AddHandler<PlayerBalanceDeleteMessageHandler>(NatsConst.GamePlayerBalancePlayerBalanceDelete, NatsConst.GamePlayerBalancePlayerBalanceDelete);
    })
    .ClearAllExchanges()
    .AddNatsGlobPatternExchange("*");

builder.Services
    .AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddSource("Valhalla.*")
        .AddAspNetCoreInstrumentation(options => options.Filter =
            httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/healthz")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !httpContext.Request.Path.StartsWithSegments("/hangfire")
            && !httpContext.Request.Path.StartsWithSegments("/metrics")
            && !filterInstrumentations.Any(ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddGrpcCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddOtlpExporter())
    .WithMetrics(meterBuiler => meterBuiler
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddEnvironmentVariableDetector())
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .Services
    .AddInterceptionService()
    .AddServiceUtiltiyInterceptors();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseSwagger();
_ = app.UseSwaggerUI();

_ = app.MapControllers();

app.MapHealthChecks("/healthz");

_ = app.UseRegister();
_ = app.InitialSnowflakeWork();
_ = app.InitializeBrandFormAsync();
_ = app.InitializeUseImportFormAsync();
_ = app.InitializePlayerFormAsync();
_ = app.InitializePlayerTempBalanceAsync();

app.Run();
