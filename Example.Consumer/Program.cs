using Example.Consumer;
using Example.Contracts;
using MassTransit;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tc =>
    {
        tc.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Consumer"));
        tc.AddSource("Consumer")
            .AddSource("MassTransit")
            .AddOtlpExporter(c =>
            {
                c.Endpoint = new Uri("http://jaeger:4317");
            });
    })
    .WithMetrics(m =>
    {
        m.AddOtlpExporter();
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MessageConsumer>();

    x.UsingPostgres((context, cfg) =>
    {
        cfg.ReceiveEndpoint(nameof(Message), e =>
        {
            e.SetReceiveMode(SqlReceiveMode.PartitionedOrdered); // here it is
            e.ConfigureConsumer<MessageConsumer>(context);
        });
    });

    var connectionBuilder = new NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("TransportDbConnection"));

    x.AddOptions<SqlTransportOptions>().Configure(options =>
    {
        options.Host = connectionBuilder.Host;
        options.Database = connectionBuilder.Database;
        options.Schema = "messaging";
        options.Role = "messaging";
        options.Username = connectionBuilder.Username;
        options.Password = connectionBuilder.Password;
        options.AdminUsername = connectionBuilder.Username;
        options.AdminPassword = connectionBuilder.Password;
    });
});

builder.Services.AddPostgresMigrationHostedService(create: true, delete: false);

var host = builder.Build();
host.Run();
