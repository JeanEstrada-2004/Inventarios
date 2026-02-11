using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventarios.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposUnidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Etiqueta",
                table: "HerramientasUnidades",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "HerramientasUnidades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Etiqueta",
                table: "HerramientasUnidades");

            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "HerramientasUnidades");
        }
    }
}
