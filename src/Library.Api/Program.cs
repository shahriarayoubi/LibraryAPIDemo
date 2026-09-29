using Library.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("LibraryAPIDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'LibraryAPIDatabase' was not found.");

builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlServerOptions =>
        {
            sqlServerOptions.MigrationsAssembly(
                typeof(LibraryDbContext).Assembly.FullName);

            sqlServerOptions.EnableRetryOnFailure();
        });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<LibraryDbContext>(
        name: "database",
        tags: new[] { "ready" });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false   // runs no registered checks — just proves Kestrel/the pipeline is up
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")   // runs the DB check
});

app.Run();
