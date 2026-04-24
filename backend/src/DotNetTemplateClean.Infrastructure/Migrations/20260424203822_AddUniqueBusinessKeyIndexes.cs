using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddUniqueBusinessKeyIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.AlterColumn<string>(
            name: "Matricule",
            table: "Personnels",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Libelle",
            table: "Entites",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            table: "Entites",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_Personnels_Matricule",
            table: "Personnels",
            column: "Matricule",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Entites_Code",
            table: "Entites",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Entites_Libelle",
            table: "Entites",
            column: "Libelle",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.DropIndex(
            name: "IX_Personnels_Matricule",
            table: "Personnels");

        migrationBuilder.DropIndex(
            name: "IX_Entites_Code",
            table: "Entites");

        migrationBuilder.DropIndex(
            name: "IX_Entites_Libelle",
            table: "Entites");

        migrationBuilder.AlterColumn<string>(
            name: "Matricule",
            table: "Personnels",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Libelle",
            table: "Entites",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(250)",
            oldMaxLength: 250);

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            table: "Entites",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(250)",
            oldMaxLength: 250);
    }
}
