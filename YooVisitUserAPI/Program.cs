using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YooVisitUserAPI.Data;
using YooVisitUserAPI.Interfaces;

// Ajoute les using pour tes services
using YooVisitUserAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration des services ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- AJOUTE CES DEUX LIGNES ICI ---
// Recette pour le service utilisateur
builder.Services.AddScoped<IUserService, UserService>();
// Recette pour le service de token
builder.Services.AddScoped<ITokenService, TokenService>();
// ------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Le reste de ton fichier ne change pas ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    try
    {
        await DataSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur est survenue pendant le seeding de la BDD.");
    }
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
