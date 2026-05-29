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
                name: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    disponibilidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    numerotelefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "productos",
                columns: new[] { "id", "descripcion", "disponibilidad", "nombre", "precio" },
                values: new object[,]
                {
                    { 1, "Bicicleta de montaña de 26 pulgadas con suspensión delantera y frenos de disco.", 15, "Bicicleta Mountain Bike", 599.99m },
                    { 2, "Bicicleta de ruta ligera y rápida, ideal para carreteras. Marco de aluminio.", 10, "Bicicleta Ruta", 799.99m },
                    { 3, "Bicicleta cómoda y práctica para desplazamientos en ciudad con canasta delantera.", 20, "Bicicleta Urbana", 449.99m }
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id", "apellido", "mail", "nombre", "numerotelefono", "passwordhash", "tipo" },
                values: new object[,]
                {
                    { 1, "Sistema", "admin@bicileteria.com", "Admin", "+34 900 000 001", "$2a$11$8woRonDE04E3Vwj5JZTPyu4og7luxj5AQ8ds5926KckIZJfmtDQ1a", "admin" },
                    { 2, "Vendedor", "usuario@bicileteria.com", "Usuario", "+34 900 000 002", "$2a$11$mjdC1U/c8fHWAER.31JV9e2MEA6f3LxkFikZ8JL82M2VSWLT6zilC", "vendedor" },
                    { 3, "Ejemplo", "cliente@bicileteria.com", "Cliente", "+34 900 000 003", "$2a$11$MJh9C6MWIZrSTPsyyuTxVuX.WROsGVOAdVnnWUnmGlWFdKnRm1J/O", "cliente" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
