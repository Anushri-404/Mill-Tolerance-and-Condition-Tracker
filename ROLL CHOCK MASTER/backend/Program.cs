using RollChockBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Port/host now comes from appsettings.json locally (already has "Urls": "http://localhost:5210"),
// and from ASPNETCORE_URLS in Docker/Render. Removed the UseUrls() override — it bound to
// loopback-only and would have blocked Render's proxy from ever reaching the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IChockRepository, ChockRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("RollChockFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("RollChockFrontend");
app.MapControllers();

app.Run();