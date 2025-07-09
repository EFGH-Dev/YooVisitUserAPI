using Microsoft.EntityFrameworkCore;
using YooVisitUserAPI.Models; // Assure-toi que le using pointe vers ta classe User
using BCrypt.Net;

namespace YooVisitUserAPI.Data;

public class UserDbContext : DbContext
{
    // Le constructeur est nécessaire pour l'injection de dépendances.
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    // Chaque DbSet<T> représente une table dans ta base de données.
    // Ici, on déclare une table "Users" qui contiendra des objets de type "User".
    public DbSet<UserApplication> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
