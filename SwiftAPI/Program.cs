using SwiftAPI.Services;
using SwiftAPI.Data;
using Microsoft.Data.Sqlite;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ISwiftMTService, SwiftMTService>();

var dbPath = Path.Combine(AppContext.BaseDirectory, "swift.db");
var connectionString = $"Data Source={dbPath}";
builder.Services.AddSingleton<ISqliteConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
builder.Services.AddScoped<IMT103Repository, SqliteMT103Repository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IMT103Repository>();
    await repo.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
