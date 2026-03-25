using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ChangetablenameAffectationsToAffectationsPersonnel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.DropForeignKey(
            name: "FK_Affectations_Entites_EntiteId",
            table: "Affectations");

        migrationBuilder.DropForeignKey(
            name: "FK_Affectations_Fonctions_FonctionId",
            table: "Affectations");

        migrationBuilder.DropForeignKey(
            name: "FK_Affectations_Personnels_PersonnelId",
            table: "Affectations");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Affectations",
            table: "Affectations");

        migrationBuilder.RenameTable(
            name: "Affectations",
            newName: "AffectationsPersonnel");

        migrationBuilder.RenameIndex(
            name: "IX_Affectations_PersonnelId",
            table: "AffectationsPersonnel",
            newName: "IX_AffectationsPersonnel_PersonnelId");

        migrationBuilder.RenameIndex(
            name: "IX_Affectations_FonctionId",
            table: "AffectationsPersonnel",
            newName: "IX_AffectationsPersonnel_FonctionId");

        migrationBuilder.RenameIndex(
            name: "IX_Affectations_EntiteId",
            table: "AffectationsPersonnel",
            newName: "IX_AffectationsPersonnel_EntiteId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AffectationsPersonnel",
            table: "AffectationsPersonnel",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_AffectationsPersonnel_Entites_EntiteId",
            table: "AffectationsPersonnel",
            column: "EntiteId",
            principalTable: "Entites",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AffectationsPersonnel_Fonctions_FonctionId",
            table: "AffectationsPersonnel",
            column: "FonctionId",
            principalTable: "Fonctions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AffectationsPersonnel_Personnels_PersonnelId",
            table: "AffectationsPersonnel",
            column: "PersonnelId",
            principalTable: "Personnels",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.DropForeignKey(
            name: "FK_AffectationsPersonnel_Entites_EntiteId",
            table: "AffectationsPersonnel");

        migrationBuilder.DropForeignKey(
            name: "FK_AffectationsPersonnel_Fonctions_FonctionId",
            table: "AffectationsPersonnel");

        migrationBuilder.DropForeignKey(
            name: "FK_AffectationsPersonnel_Personnels_PersonnelId",
            table: "AffectationsPersonnel");

        migrationBuilder.DropPrimaryKey(
            name: "PK_AffectationsPersonnel",
            table: "AffectationsPersonnel");

        migrationBuilder.RenameTable(
            name: "AffectationsPersonnel",
            newName: "Affectations");

        migrationBuilder.RenameIndex(
            name: "IX_AffectationsPersonnel_PersonnelId",
            table: "Affectations",
            newName: "IX_Affectations_PersonnelId");

        migrationBuilder.RenameIndex(
            name: "IX_AffectationsPersonnel_FonctionId",
            table: "Affectations",
            newName: "IX_Affectations_FonctionId");

        migrationBuilder.RenameIndex(
            name: "IX_AffectationsPersonnel_EntiteId",
            table: "Affectations",
            newName: "IX_Affectations_EntiteId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Affectations",
            table: "Affectations",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Affectations_Entites_EntiteId",
            table: "Affectations",
            column: "EntiteId",
            principalTable: "Entites",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Affectations_Fonctions_FonctionId",
            table: "Affectations",
            column: "FonctionId",
            principalTable: "Fonctions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Affectations_Personnels_PersonnelId",
            table: "Affectations",
            column: "PersonnelId",
            principalTable: "Personnels",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
