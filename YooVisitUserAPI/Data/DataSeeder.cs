using Microsoft.EntityFrameworkCore;
using YooVisitUserAPI.Data;
using YooVisitUserAPI.Models;

public static class DataSeeder
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        // On utilise le service provider pour obtenir une instance du DbContext
        using (var context = serviceProvider.GetRequiredService<UserDbContext>())
        {
            // On ne fait rien si la table Users contient déjà des données
            if (context.Users.Any())
            {
                return;
            }

            var users = new UserApplication[]
            {
            new UserApplication
            {
                IdUtilisateur = Guid.Parse("d8d7b3b0-2b9a-4f9a-8b9a-7b3b0e6b7a5c"),
                Email = "agent.47@ica.org",
                HashedPassword = "$2a$11$It0jRa8NXUEAIXtMnqZnB.Bs.jhxNATOTkr2swqEkIOuNocJ/xllC",
                DateInscription = new DateTime(2025, 6, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new UserApplication
            {
                IdUtilisateur = Guid.Parse("c2a7d5a5-6b3a-4f8e-a9d8-7b3b0e6b7a5c"),
                Email = "commander.shepard@normandy.sr2",
                HashedPassword = "$2a$11$aTibKgO3RMmsmzlKgL.XwOA25iTYsS9BX6b407GUguTtRVzBA0c7O",
                DateInscription = new DateTime(2025, 6, 27, 0, 0, 0, DateTimeKind.Utc)
            }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}