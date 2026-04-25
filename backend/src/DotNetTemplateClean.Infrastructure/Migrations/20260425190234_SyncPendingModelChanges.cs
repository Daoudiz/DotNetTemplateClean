using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SyncPendingModelChanges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.DropIndex(
            name: "IX_Personnels_EntiteId",
            table: "Personnels");

        migrationBuilder.DropIndex(
            name: "IX_Entites_RattachementEntiteId",
            table: "Entites");

        migrationBuilder.DropIndex(
            name: "IX_Entites_TypeEntiteId",
            table: "Entites");

        migrationBuilder.DropIndex(
            name: "IX_AffectationsPersonnel_EntiteId",
            table: "AffectationsPersonnel");

        migrationBuilder.DropIndex(
            name: "IX_AffectationsPersonnel_PersonnelId",
            table: "AffectationsPersonnel");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.CreateIndex(
            name: "IX_Personnels_EntiteId",
            table: "Personnels",
            column: "EntiteId");

        migrationBuilder.CreateIndex(
            name: "IX_Entites_RattachementEntiteId",
            table: "Entites",
            column: "RattachementEntiteId");

        migrationBuilder.CreateIndex(
            name: "IX_Entites_TypeEntiteId",
            table: "Entites",
            column: "TypeEntiteId");

        migrationBuilder.CreateIndex(
            name: "IX_AffectationsPersonnel_EntiteId",
            table: "AffectationsPersonnel",
            column: "EntiteId");

        migrationBuilder.CreateIndex(
            name: "IX_AffectationsPersonnel_PersonnelId",
            table: "AffectationsPersonnel",
            column: "PersonnelId");
    }
}
