using Microsoft.EntityFrameworkCore;
using YooVisitUserAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. On récupère la chaîne de connexion depuis appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. On enregistre le DbContext dans le système d'injection de dépendances.
//    On lui dit d'utiliser le pilote PostgreSQL (UseNpgsql) avec la chaîne de connexion.
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Lancement de la migration et du seeding de la base de données...");

    try
    {
        // 1. Récupère le contexte de la base de données
        // REMPLACE "YooVisitUserDbContext" par le VRAI NOM de ta classe DbContext
        var context = services.GetRequiredService<UserDbContext>();

        // 2. Applique les migrations en attente.
        // C'est cette ligne qui va créer la table "Users" !
        context.Database.Migrate();

        // 3. Lance ton seeder SEULEMENT APRÈS que les migrations soient passées
        // La ligne 23 de ton Program.cs qui appelle le seeder doit être déplacée ici.
        DataSeeder.Seed(services);

        logger.LogInformation("Migration et seeding terminés avec succès.");
    }
    catch (Exception ex)
    {
        // Si ça plante, on log l'erreur pour comprendre pourquoi
        logger.LogError(ex, "Une erreur est survenue lors de la migration ou du seeding.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
