using Microsoft.EntityFrameworkCore;
using Bicileteria.Models;
using BCrypt.Net;

namespace Bicileteria.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Username).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);

            // Configuración de la tabla Products
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(1000);
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);

            // Generar hashes de contraseñas usando BCrypt
            var passwordHash1 = BCrypt.Net.BCrypt.HashPassword("Demo1234");
            var passwordHash2 = BCrypt.Net.BCrypt.HashPassword("Demo1234");
            var passwordHash3 = BCrypt.Net.BCrypt.HashPassword("Demo1234");

            // Datos semilla para Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@bicileteria.com",
                    PasswordHash = passwordHash1
                },
                new User
                {
                    Id = 2,
                    Username = "usuario",
                    Email = "usuario@bicileteria.com",
                    PasswordHash = passwordHash2
                },
                new User
                {
                    Id = 3,
                    Username = "cliente",
                    Email = "cliente@bicileteria.com",
                    PasswordHash = passwordHash3
                }
            );

            // Datos semilla para Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Bicicleta Mountain Bike",
                    Description = "Bicicleta de montaña de 26 pulgadas con suspensión delantera y frenos de disco.",
                    Price = 599.99m
                },
                new Product
                {
                    Id = 2,
                    Name = "Bicicleta Ruta",
                    Description = "Bicicleta de ruta ligera y rápida, ideal para carreteras. Marco de aluminio.",
                    Price = 799.99m
                },
                new Product
                {
                    Id = 3,
                    Name = "Bicicleta Urbana",
                    Description = "Bicicleta cómoda y práctica para desplazamientos en ciudad con canasta delantera.",
                    Price = 449.99m
                }
            );
        }
    }
}
