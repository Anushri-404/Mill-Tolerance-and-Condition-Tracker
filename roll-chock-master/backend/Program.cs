using RollChockBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5210");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IChockRepository, ChockRepository>();

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
