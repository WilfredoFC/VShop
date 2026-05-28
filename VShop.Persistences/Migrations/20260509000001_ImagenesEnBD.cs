using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VShop.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class ImagenesEnBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlImagen",
                table: "ProductoImagenes");

            migrationBuilder.AddColumn<byte[]>(
                name: "Datos",
                table: "ProductoImagenes",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoContenido",
                table: "ProductoImagenes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Datos",
                table: "ProductoImagenes");

            migrationBuilder.DropColumn(
                name: "TipoContenido",
                table: "ProductoImagenes");

            migrationBuilder.AddColumn<string>(
                name: "UrlImagen",
                table: "ProductoImagenes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
