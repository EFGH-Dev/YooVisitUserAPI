﻿using Microsoft.EntityFrameworkCore;
using YooVisitUserAPI.Models;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace YooVisitUserAPI.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            // On logue avec le contexte de Program, car DataSeeder est static
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            await context.Database.MigrateAsync();

            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("La base de données contient déjà des utilisateurs. Pas de seeding nécessaire.");
                return;
            }

            logger.LogInformation("Base de données vide. Démarrage du seeding...");

            var users = new UserApplication[]
            {
                new UserApplication
                {
                    IdUtilisateur = Guid.NewGuid(),
                    Email = "fabrice.guthier@gmail.com",
                    // On utilise la version simple maintenant que le bug de l'IDE est résolu
                    HashedPassword = global::BCrypt.Net.BCrypt.HashPassword("MdpFabrice123!"),
                    DateInscription = DateTime.UtcNow
                },
                new UserApplication
                {
                    IdUtilisateur = Guid.NewGuid(),
                    Email = "guthier.emilie@gmail.com",
                    HashedPassword = global::BCrypt.Net.BCrypt.HashPassword("MdpEmilie123!"),
                    DateInscription = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeding terminé. Deux utilisateurs de test ont été créés.");
        }
    }
}
