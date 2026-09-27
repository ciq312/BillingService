using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.DAL.Db;
using OrderService.BLL;
using OrderService.DAL.Models.Orders;
using MassTransit;
using OrderService.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<OrderManager>();
builder.Services.AddDbContext<AppDbContext>(optionsAction: (Options) =>
{
    Options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbit = builder.Configuration
    .GetSection(BusOptions.SectionName)
    .Get<BusOptions>();


builder.Services.AddMassTransit(bus =>
{
    bus.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(rabbit?.Host, "/", h =>
        {
            h.Username(rabbit?.Username);
            h.Password(rabbit?.Password);
        });
    });
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.MapGet("/api/health", () => Results.Ok("healthy"));

app.MapPost("api/orders", async (OrderManager service, CreateNewOrderDTO dto, CancellationToken ct) =>
{
    Order order = await service.CreateOrderAsync(dto, ct);
    return Results.Created("api/orders", order);
});
app.Run();

