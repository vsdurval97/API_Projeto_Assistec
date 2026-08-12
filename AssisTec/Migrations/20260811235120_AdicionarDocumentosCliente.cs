using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssisTec.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDocumentosCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Documento",
                table: "Clientes",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Documento",
                table: "Clientes");
        }
    }
}
