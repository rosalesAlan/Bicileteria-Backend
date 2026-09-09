using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bicicleteria.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "carrusel",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    categoryid = table.Column<int>(type: "integer", nullable: true),
                    range = table.Column<int>(type: "integer", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrusel", x => x.id);
                    table.ForeignKey(
                        name: "FK_carrusel_categoria_categoryid",
                        column: x => x.categoryid,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "producto",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    imageurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    categoryid = table.Column<int>(type: "integer", nullable: true),
                    availability = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto", x => x.id);
                    table.ForeignKey(
                        name: "FK_producto_categoria_categoryid",
                        column: x => x.categoryid,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    firstname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phonenumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    roleid = table.Column<int>(type: "integer", nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_roleid",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categoria",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Todas" },
                    { 2, "Mountain" },
                    { 3, "Road" },
                    { 4, "Urban" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 3, "Admin" },
                    { 4, "Cliente" }
                });

            migrationBuilder.InsertData(
                table: "carrusel",
                columns: new[] { "id", "categoryid", "name", "range" },
                values: new object[,]
                {
                    { 1, 2, "Mountain Bike Promotion", 10 },
                    { 2, 3, "Road Bike Promotion", 10 },
                    { 3, 4, "Urban Bike Promotion", 10 }
                });

            migrationBuilder.InsertData(
                table: "producto",
                columns: new[] { "id", "availability", "categoryid", "description", "imageurl", "name", "price" },
                values: new object[,]
                {
                    { 2, true, 1, "26-inch mountain bike with front suspension and disc brakes.", null, "Mountain Bike 26", 599.99m },
                    { 3, true, 2, "Lightweight and fast road bike, ideal for highways. Aluminum frame.", null, "Road Bike", 799.99m },
                    { 4, true, 3, "Comfortable and practical bike for city commuting with front basket.", null, "Urban Bike", 449.99m }
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id", "email", "firstname", "lastname", "passwordhash", "phonenumber", "roleid" },
                values: new object[,]
                {
                    { 2, "admin@bicileteria.com", "Admin", "Sistema", "$2a$11$KYw7iAZvj87Xi.S5AZJytewQeeg39BssqvS6b8/iyCc1FnGPvoLsO", "123456789", 3 },
                    { 3, "client@bicileteria.com", "Client", "Example", "$2a$11$BXDYkze2IUGuwPSq6aSVn./QN4MjJzqNRoJlC9w9o7yReVM4vUDdK", "+34 900 000 002", 4 },
                    { 4, "client@bicileteria.com", "Client", "Example", "$2a$11$FZSUFJPpi6pV3my7qOAEO.mzhqNNALZQQKRTq62ynh9etpx/OpB.a", "+34 900 000 003", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_carrusel_categoryid",
                table: "carrusel",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_producto_categoryid",
                table: "producto",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_roleid",
                table: "usuarios",
                column: "roleid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrusel");

            migrationBuilder.DropTable(
                name: "producto");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
