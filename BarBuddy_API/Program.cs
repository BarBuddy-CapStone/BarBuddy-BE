using BarBuddy_API.DependencyInjection;
using BarBuddy_API.Middleware;
using Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// route https
//builder.WebHost.UseUrls("https://0.0.0.0");

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
// DI
builder.Services.AddPresentation(builder.Configuration);

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        builder =>
//        {
//            builder.AllowAnyOrigin()
//                   .AllowAnyHeader()
//                   .AllowAnyMethod();
//        });
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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
//app.UseCors("AllowAll");
//app.UseCors("AllowEditorSwagger");
app.UseCors("AllowReactBarBuddy");

// Middleware configuration
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<BookingHub>("/bookingHub");
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>("/notificationHub");
});

app.MapControllers();

app.Run();
