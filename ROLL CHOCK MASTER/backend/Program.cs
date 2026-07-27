using RollChockBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Run on its own port, separate from the SPM backend (5103).
builder.WebHost.UseUrls("http://localhost:5210");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IChockRepository, ChockRepository>();

// Allow the Roll Chock frontend (its own separate Vite app) to call this API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("RollChockFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("RollChockFrontend");
app.MapControllers();

app.Run();
