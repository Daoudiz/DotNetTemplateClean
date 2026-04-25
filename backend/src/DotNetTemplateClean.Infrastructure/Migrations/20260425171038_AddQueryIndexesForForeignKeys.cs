using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddQueryIndexesForForeignKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.CreateIndex(
            name: "IX_AffectationsPersonnel_EntiteId_IsDeleted_IsActive",
            table: "AffectationsPersonnel",
            columns: ["EntiteId", "IsDeleted", "IsActive"]);

        migrationBuilder.CreateIndex(
            name: "IX_AffectationsPersonnel_PersonnelId_IsDeleted_IsActive",
            table: "AffectationsPersonnel",
            columns: ["PersonnelId", "IsDeleted", "IsActive"]);

        migrationBuilder.CreateIndex(
            name: "IX_Entites_RattachementEntiteId_IsDeleted",
            table: "Entites",
            columns: ["RattachementEntiteId", "IsDeleted"]);

        migrationBuilder.CreateIndex(
            name: "IX_Entites_TypeEntiteId_IsDeleted",
            table: "Entites",
            columns: ["TypeEntiteId", "IsDeleted"]);

        migrationBuilder.CreateIndex(
            name: "IX_Personnels_EntiteId_IsDeleted",
            table: "Personnels",
            columns: ["EntiteId", "IsDeleted"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));

        migrationBuilder.DropIndex(
            name: "IX_AffectationsPersonnel_EntiteId_IsDeleted_IsActive",
            table: "AffectationsPersonnel");

        migrationBuilder.DropIndex(
            name: "IX_AffectationsPersonnel_PersonnelId_IsDeleted_IsActive",
            table: "AffectationsPersonnel");

        migrationBuilder.DropIndex(
            name: "IX_Entites_RattachementEntiteId_IsDeleted",
            table: "Entites");

        migrationBuilder.DropIndex(
            name: "IX_Entites_TypeEntiteId_IsDeleted",
            table: "Entites");

        migrationBuilder.DropIndex(
            name: "IX_Personnels_EntiteId_IsDeleted",
            table: "Personnels");
    }
}
