using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTemplateClean.Infrastructure.Migrations;

    /// <inheritdoc />
    public partial class _20260429_FixSnapshotDrift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));
            // No-op migration: aligns snapshot with the current model after previous drift.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder, nameof(migrationBuilder));
            // Intentionally empty.
        }
    }

