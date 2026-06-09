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

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CarouselItem> CarouselItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar convenciones de PostgreSQL (minúsculas para tablas y columnas)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entity.SetTableName(tableName.ToLower());
                }

                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName();
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        property.SetColumnName(columnName.ToLower());
                    }
                }
            }

            // Configuración de la tabla Roles
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Role>().HasKey(r => r.Id);
            modelBuilder.Entity<Role>().Property(r => r.Name).IsRequired().HasMaxLength(100);

            // Configuración de la tabla Users
            modelBuilder.Entity<User>().ToTable("usuarios");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.LastName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<User>().Property(u => u.PhoneNumber).HasMaxLength(20);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la tabla Categories
            modelBuilder.Entity<Category>().ToTable("categoria");
            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Category>().Property(c => c.Name).IsRequired().HasMaxLength(200);

            // Configuración de la tabla Products
            modelBuilder.Entity<Product>().ToTable("producto");
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(1000);
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);
            modelBuilder.Entity<Product>().Property(p => p.ImageUrl).HasMaxLength(500);
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);


            // Configuración de la tabla CarouselItems
            modelBuilder.Entity<CarouselItem>().ToTable("carrusel");
            modelBuilder.Entity<CarouselItem>().HasKey(c => c.Id);
            modelBuilder.Entity<CarouselItem>().Property(c => c.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<CarouselItem>().Property(c => c.Range).HasMaxLength(100);
            modelBuilder.Entity<CarouselItem>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Generar hashes de contraseñas usando BCrypt
            var passwordHash1 = BCrypt.Net.BCrypt.HashPassword("Demo1234");
            var passwordHash2 = BCrypt.Net.BCrypt.HashPassword("Demo5678");
            var passwordHash3 = BCrypt.Net.BCrypt.HashPassword("Demo9012");

            Console.WriteLine($"Hash de contraseña para Demo1234: {passwordHash1}");
            // Datos semilla para Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 3, Name = "Admin" },
                new Role { Id = 4, Name = "Cliente" }
            );

            // Datos semilla para Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    FirstName = "Admin",
                    LastName = "Sistema",
                    Email = "admin@bicileteria.com",
                    PhoneNumber = "123456789",
                    RoleId = 3,
                    PasswordHash = passwordHash1
                },
                new User
                {
                    Id = 3,
                    FirstName = "Client",
                    LastName = "Example",
                    Email = "client@bicileteria.com",
                    PhoneNumber = "+34 900 000 002",
                    RoleId = 4,
                    PasswordHash = passwordHash2
                },
                new User
                {
                    Id = 4,
                    FirstName = "Client",
                    LastName = "Example",
                    Email = "client@bicileteria.com",
                    PhoneNumber = "+34 900 000 003",
                    RoleId = 4,
                    PasswordHash = passwordHash3
                }
            );

            // Datos semilla para Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Todas" },
                new Category { Id = 2, Name = "Mountain" },
                new Category { Id = 3, Name = "Road" },
                new Category { Id = 4, Name = "Urban" }
            );

            // Datos semilla para Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 2,
                    Name = "Mountain Bike 26",
                    Description = "26-inch mountain bike with front suspension and disc brakes.",
                    Price = 599.99m,
                    CategoryId = 1,
                    Availability = true
                },
                new Product
                {
                    Id = 3,
                    Name = "Road Bike",
                    Description = "Lightweight and fast road bike, ideal for highways. Aluminum frame.",
                    Price = 799.99m,
                    CategoryId = 2,
                    Availability = true
                },
                new Product
                {
                    Id = 4,
                    Name = "Urban Bike",
                    Description = "Comfortable and practical bike for city commuting with front basket.",
                    Price = 449.99m,
                    CategoryId = 3,
                    Availability = true
                }
            );

            // Datos semilla para CarouselItems
            modelBuilder.Entity<CarouselItem>().HasData(
                new CarouselItem
                {
                    Id = 1,
                    Name = "Mountain Bike Promotion",
                    CategoryId = 2,
                    Range = 10
                },
                new CarouselItem
                {
                    Id = 2,
                    Name = "Road Bike Promotion",
                    CategoryId = 3,
                    Range = 10
                },
                new CarouselItem
                {
                    Id = 3,
                    Name = "Urban Bike Promotion",
                    CategoryId = 4,
                    Range = 10
                }
            );
        }
    }
}
