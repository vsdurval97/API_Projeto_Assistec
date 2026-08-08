using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssisTec.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaModificacaoUtc",
                table: "OrdensServico",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimaModificacaoUtc",
                table: "OrdensServico");
        }
    }
}
