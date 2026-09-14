using Microsoft.EntityFrameworkCore;
using PXbankAPI.Domain.Interfaces;
using PXbankAPI.Infrastructure.Context;
using PXbankAPI.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<PXbankDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IRepositorioMotorista, RepositorioMotorista>();
builder.Services.AddScoped<IRepositorioTransacao, RepositorioTransacao>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();