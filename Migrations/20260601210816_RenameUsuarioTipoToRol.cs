using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bicicleteria.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameUsuarioTipoToRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageurl",
                table: "productos");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "usuarios",
                newName: "rol");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$aDMvxi19.4WXx6ICMLz6I.Y2kSuLiHpUWaTDS3WydVDamXXwwKLMe");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 2,
                column: "passwordhash",
                value: "$2a$11$RAFttRFyiX2DhbUFGjQo/.OmvG9pK5RQG2pGLMNC8RMYW4GReiTV.");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 3,
                column: "passwordhash",
                value: "$2a$11$xqfICPou/6t.rdfEEaFAYe5Sf2RgTZz5UapBrgJnB.TPzFJxlOjrG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "rol",
                table: "usuarios",
                newName: "tipo");

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
                column: "imageurl",
                value: "https://example.com/mountain-bike.jpg");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 2,
                column: "imageurl",
                value: "https://example.com/ruta-bike.jpg");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "id",
                keyValue: 3,
                column: "imageurl",
                value: "https://example.com/urbana-bike.jpg");

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
    }
}
