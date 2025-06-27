using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using YooVisitUserAPI.Data;

namespace YooVisitUserAPI.Data;

// Cette classe est une "usine" qui explique aux outils EF Core comment créer votre DbContext.
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        // On crée un IConfiguration pour lire le fichier appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            // On lui dit de chercher à la racine du projet
            .SetBasePath(Directory.GetCurrentDirectory())
            // On lui dit de charger ce fichier spécifique
            .AddJsonFile("appsettings.json")
            .Build();

        // On prépare les options pour notre DbContext
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

        // On lit la chaîne de connexion DANS le fichier appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // On dit aux options d'utiliser Npgsql avec cette chaîne de connexion
        optionsBuilder.UseNpgsql(connectionString);

        // On retourne une nouvelle instance du DbContext avec les bonnes options
        return new UserDbContext(optionsBuilder.Options);
    }
}
