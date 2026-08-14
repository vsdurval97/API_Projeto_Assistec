using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssisTec.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCadastroClienteSefaz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoBairro",
                table: "Clientes",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCep",
                table: "Clientes",
                type: "TEXT",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCodigoMunicipioIbge",
                table: "Clientes",
                type: "TEXT",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCodigoPais",
                table: "Clientes",
                type: "TEXT",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoComplemento",
                table: "Clientes",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoLogradouro",
                table: "Clientes",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoMunicipio",
                table: "Clientes",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoNumero",
                table: "Clientes",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoPais",
                table: "Clientes",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoUf",
                table: "Clientes",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndicadorInscricaoEstadual",
                table: "Clientes",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InscricaoEstadual",
                table: "Clientes",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPessoa",
                table: "Clientes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoBairro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoCep",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoCodigoMunicipioIbge",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoCodigoPais",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoComplemento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoLogradouro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoMunicipio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoNumero",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoPais",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoUf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IndicadorInscricaoEstadual",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "InscricaoEstadual",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoPessoa",
                table: "Clientes");
        }
    }
}
