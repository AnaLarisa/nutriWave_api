using Microsoft.EntityFrameworkCore;
using NutriWave.API.Data;
using NutriWave.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var dbConnString= Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Db connection string not set.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(dbConnString));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
