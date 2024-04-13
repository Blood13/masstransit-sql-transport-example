using Example.Sender;
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
        tc.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Sender"));
        tc.AddSource("Sender")
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
    x.UsingPostgres();

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

var host = builder.Build();
host.Run();
