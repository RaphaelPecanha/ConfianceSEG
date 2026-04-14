using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEG.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelasIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<int>(
                name: "id_sapiens",
                table: "usuarios",
                type: "INT(11)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INT(11)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<int>(
                name: "id_sapiens",
                table: "usuarios",
                type: "INT(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INT(11)",
                oldNullable: true);
        }
    }
}
