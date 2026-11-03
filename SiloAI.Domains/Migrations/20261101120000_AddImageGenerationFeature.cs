using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddImageGenerationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No schema change: UsageFeature.ImageGeneration (5) and RagDocType.ImageGeneration (5)
            // are stored in existing int columns. Data only:
            //
            // PricingEngine fails fast when a feature has no pricing row, so give the new feature
            // one — copied from the latest OCR pricing row (the closest existing "vision" feature),
            // so formula parameters (multiplier / floors) start identical and an admin can change
            // them on the Financial page. If OCR has no row either, fall back to M=1, no floors;
            // the real values are then entered on the Financial page. Idempotent.
            //
            // The AI model for the feature is deliberately NOT seeded: the admin adds a model of
            // the image-generation kind (e.g. openai/gpt-image-2) on the AI Models page and assigns
            // it to "تولید تصویر".
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 5)
BEGIN
    IF EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 4)
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        SELECT TOP (1) NEWID(), 5, [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], '2026-11-01T00:00:00'
        FROM [tbl_PricingSettings] WHERE [fld_Feature] = 4
        ORDER BY [fld_EffectiveFrom] DESC;
    ELSE
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        VALUES (NEWID(), 5, 1, 0, 0, '2026-11-01T00:00:00');
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [tbl_CustomerModelAssignments] WHERE [fld_Feature] = 5;");
            migrationBuilder.Sql("DELETE FROM [tbl_PricingSettings] WHERE [fld_Feature] = 5;");
        }
    }
}
