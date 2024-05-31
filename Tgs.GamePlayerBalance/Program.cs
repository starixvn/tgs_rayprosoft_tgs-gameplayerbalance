using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NATS.Client.Core;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GamePlayerBalance;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.MessageHandlers;
using Tgs.MessageQueue.Nats.Serializers;

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
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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
	(sp, dbBuilder) =>
	{
		var config = sp.GetRequiredService<IConfiguration>();
		_ = dbBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
	},
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
		options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalanceConnectionStrings:GameProvidersClientBaseUrl")!;
	},
	(options, sp) =>
	{
		var configuration = sp.GetRequiredService<IConfiguration>();
		options.BaseUrl = configuration.GetValue<Uri>("GamePlayerBalanceConnectionStrings:GameFundTransferClientBaseUrl")!;
	});

// HostedService
builder.Services.AddHostedService<InitializeHostedService>();

builder.Services
	.AddKeyedSingleton("Nats", (sp, _) => new NatsConnection(NatsOpts.Default with
	{
		Url = builder.Configuration.GetConnectionString("Nats")!,
		SerializerRegistry = TgsNatsSerializerRegistration.Default
	}))
	.AddMessageQueue()
	.AddNatsMessageQueue(config => config
	.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
		.AddHandler<BrandFormByAwcAelottoQueuedMessageHandler>(GamePlayerBalanceNatsConst.BrandFormByAwcAelotto, GamePlayerBalanceNatsConst.BrandFormByAwcAelotto)
		.AddHandler<BrandFormByAwcAwsQueuedMessageHandler>(GamePlayerBalanceNatsConst.BrandFormByAwcAws, GamePlayerBalanceNatsConst.BrandFormByAwcAws)
		.AddHandler<BrandFormByAwcFacaiQueuedMessageHandler>(GamePlayerBalanceNatsConst.BrandFormByAwcFacai, GamePlayerBalanceNatsConst.BrandFormByAwcFacai)
		.AddHandler<BrandFormQueuedMessageHandler>(GamePlayerBalanceNatsConst.BrandFormQueued, GamePlayerBalanceNatsConst.BrandFormQueued)
		.AddHandler<BrandFormProcessingMessageHandler>(GamePlayerBalanceNatsConst.BrandFormProcessing, GamePlayerBalanceNatsConst.BrandFormProcessing)
		.AddHandler<ImportFormQueuedMessageHandler>(GamePlayerBalanceNatsConst.ImportFormQueued, GamePlayerBalanceNatsConst.ImportFormQueued)
		.AddHandler<ImportFormProcessingMessageHandler>(GamePlayerBalanceNatsConst.ImportFormProcessing, GamePlayerBalanceNatsConst.ImportFormProcessing)
		.AddHandler<PlayerFormQueuedMessageHandler>(GamePlayerBalanceNatsConst.PlayerFormQueued, GamePlayerBalanceNatsConst.PlayerFormQueued))
	.ClearAllExchanges()
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.BrandFormByAwcAelotto)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.BrandFormByAwcAws)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.BrandFormByAwcFacai)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.BrandFormQueued)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.BrandFormProcessing)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.ImportFormQueued)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.ImportFormProcessing)
	.AddNatsGlobPatternExchange(GamePlayerBalanceNatsConst.PlayerFormQueued);

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

app.Run();
