using Contracts;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using PaymentService.API;

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
            cfg.ConfigureEndpoints(context);
        });
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/health", () => Results.Ok("healthy"));

app.Run();

