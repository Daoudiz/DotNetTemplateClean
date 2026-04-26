using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260426183000_DecoupleApplicationUserFromEntite")]
public partial class DecoupleApplicationUserFromEntite : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUsers_Entites_EntiteId",
            table: "AspNetUsers");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_EntiteId",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "DateRecrutement",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "EntiteId",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "Matricule",
            table: "AspNetUsers");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.AddColumn<DateTime>(
            name: "DateRecrutement",
            table: "AspNetUsers",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "EntiteId",
            table: "AspNetUsers",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Matricule",
            table: "AspNetUsers",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_EntiteId",
            table: "AspNetUsers",
            column: "EntiteId");

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUsers_Entites_EntiteId",
            table: "AspNetUsers",
            column: "EntiteId",
            principalTable: "Entites",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
