using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.API;
using PaymentService.Application;
using PaymentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var rabbit = builder.Configuration
    .GetSection(BusOptions.SectionName)
    .Get<BusOptions>();

builder.Services.AddOpenApi();
builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<OrderCreatedConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbit?.Host, "/", h =>
        {
            h.Username(rabbit?.Username);
            h.Password(rabbit?.Password);
        });
        cfg.UseConsumeFilter(typeof(InboxFilter<>), context);
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<PaymentManager>();
builder.Services.AddDbContext<AppDbContext>(optionsAction: (Options) =>
{
    Options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/api/health", () => Results.Ok("healthy"));

app.Run();
