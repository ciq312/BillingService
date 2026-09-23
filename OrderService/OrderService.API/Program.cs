using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.DAL.Db;
using OrderService.BLL;
using OrderService.DAL.Models.Orders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<OrderManager>();
builder.Services.AddDbContext<AppDbContext>(optionsAction: (Options) =>
{
    Options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.MapGet("/api/health", () => Results.Ok("healthy"));

app.MapPost("api/orders", async (OrderManager service, MakeNewOrderDTO dto) =>
{
    Order order = await service.MakeNewOrder(dto);
return Results.Created("api/orders", order);
});
app.Run();

