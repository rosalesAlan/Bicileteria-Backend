using Microsoft.EntityFrameworkCore;
using Bicicleteria.Backend.Models;
using BCrypt.Net;

namespace Bicicleteria.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Usuarios { get; set; }
        public DbSet<Product> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar convenciones de PostgreSQL (minúsculas para tablas y columnas)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToLower());

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower());
                }
            }

            // Configuración de la tabla Usuarios (anteriormente Users)
            modelBuilder.Entity<User>().ToTable("usuarios");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.Apellido).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.NumeroTelefono).HasMaxLength(20);
            modelBuilder.Entity<User>().Property(u => u.Mail).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<User>().Property(u => u.Tipo).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);

            // Configuración de la tabla Productos (anteriormente Products)
            modelBuilder.Entity<Product>().ToTable("productos");
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Nombre).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>().Property(p => p.Descripcion).HasMaxLength(1000);
            modelBuilder.Entity<Product>().Property(p => p.Precio).HasPrecision(10, 2);
            modelBuilder.Entity<Product>().Property(p => p.Disponibilidad).HasDefaultValue(0);

            // Generar hashes de contraseñas usando BCrypt
            var passwordHash1 = BCrypt.Net.BCrypt.HashPassword("Demo1234");
            var passwordHash2 = BCrypt.Net.BCrypt.HashPassword("Demo1234");
            var passwordHash3 = BCrypt.Net.BCrypt.HashPassword("Demo1234");

            // Datos semilla para Usuarios
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    NumeroTelefono = "+34 900 000 001",
                    Mail = "admin@bicileteria.com",
                    Tipo = "admin",
                    PasswordHash = passwordHash1
                },
                new User
                {
                    Id = 2,
                    Nombre = "Usuario",
                    Apellido = "Vendedor",
                    NumeroTelefono = "+34 900 000 002",
                    Mail = "usuario@bicileteria.com",
                    Tipo = "vendedor",
                    PasswordHash = passwordHash2
                },
                new User
                {
                    Id = 3,
                    Nombre = "Cliente",
                    Apellido = "Ejemplo",
                    NumeroTelefono = "+34 900 000 003",
                    Mail = "cliente@bicileteria.com",
                    Tipo = "cliente",
                    PasswordHash = passwordHash3
                }
            );

            // Datos semilla para Productos
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Nombre = "Bicicleta Mountain Bike",
                    Descripcion = "Bicicleta de montaña de 26 pulgadas con suspensión delantera y frenos de disco.",
                    Precio = 599.99m,
                    Disponibilidad = 15
                },
                new Product
                {
                    Id = 2,
                    Nombre = "Bicicleta Ruta",
                    Descripcion = "Bicicleta de ruta ligera y rápida, ideal para carreteras. Marco de aluminio.",
                    Precio = 799.99m,
                    Disponibilidad = 10
                },
                new Product
                {
                    Id = 3,
                    Nombre = "Bicicleta Urbana",
                    Descripcion = "Bicicleta cómoda y práctica para desplazamientos en ciudad con canasta delantera.",
                    Precio = 449.99m,
                    Disponibilidad = 20
                }
            );
        }
    }
}
