using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bicicleteria.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "disponibilidad",
                table: "productos");

            migrationBuilder.RenameColumn(
                name: "precio",
                table: "productos",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "productos",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "descripcion",
                table: "productos",
                newName: "description");

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "imageurl",
                table: "productos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "category", "imageurl" },
                values: new object[] { "Montaña", "https://example.com/mountain-bike.jpg" });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "category", "imageurl" },
                values: new object[] { "Ruta", "https://example.com/ruta-bike.jpg" });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "category", "imageurl" },
                values: new object[] { "Urbana", "https://example.com/urbana-bike.jpg" });

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$eQL6KAHwVao.Iy3wGKpQReYVLBFtnTxHq5Lkn4ywdKjifD3OkQbSq");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 2,
                column: "passwordhash",
                value: "$2a$11$7uqe8d1uvhIIZcnofxJOfeYsMJNxCt7Zer02rVbvUs0a98/NrOek6");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 3,
                column: "passwordhash",
                value: "$2a$11$wAk9JixYhpfg/NVq2oR5w.TXKnfa1WYMfuXmCYtXgeJ30Sa88At2S");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "imageurl",
                table: "productos");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "productos",
                newName: "precio");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "productos",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "productos",
                newName: "descripcion");

            migrationBuilder.AddColumn<int>(
                name: "disponibilidad",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 1,
                column: "disponibilidad",
                value: 15);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 2,
                column: "disponibilidad",
                value: 10);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 3,
                column: "disponibilidad",
                value: 20);

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$8woRonDE04E3Vwj5JZTPyu4og7luxj5AQ8ds5926KckIZJfmtDQ1a");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 2,
                column: "passwordhash",
                value: "$2a$11$mjdC1U/c8fHWAER.31JV9e2MEA6f3LxkFikZ8JL82M2VSWLT6zilC");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 3,
                column: "passwordhash",
                value: "$2a$11$MJh9C6MWIZrSTPsyyuTxVuX.WROsGVOAdVnnWUnmGlWFdKnRm1J/O");
        }
    }
}
