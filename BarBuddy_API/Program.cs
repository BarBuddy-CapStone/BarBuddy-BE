using Application.IService;
using Application.Service;
using BarBuddy_API.DependencyInjection;
using BarBuddy_API.Middleware;
using Infrastructure.DependencyInjection;
using Infrastructure.Vnpay.Config;
using Infrastructure.Zalopay.Config;
using Microsoft.OpenApi.Models;
using Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
// DI
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarBuddy API V1");
        c.RoutePrefix = string.Empty;
        c.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactBarBuddy");

// Middleware configuration
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
