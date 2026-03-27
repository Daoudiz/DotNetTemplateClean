using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UseEnumsAddEntiteToPersonnel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Personnels",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "EntiteId",
            table: "Personnels",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Personnels_EntiteId",
            table: "Personnels",
            column: "EntiteId");

        migrationBuilder.AddForeignKey(
            name: "FK_Personnels_Entites_EntiteId",
            table: "Personnels",
            column: "EntiteId",
            principalTable: "Entites",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Personnels_Entites_EntiteId",
            table: "Personnels");

        migrationBuilder.DropIndex(
            name: "IX_Personnels_EntiteId",
            table: "Personnels");

        migrationBuilder.DropColumn(
            name: "Email",
            table: "Personnels");

        migrationBuilder.DropColumn(
            name: "EntiteId",
            table: "Personnels");
    }
}
