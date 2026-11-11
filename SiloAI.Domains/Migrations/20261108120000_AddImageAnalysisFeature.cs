using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddImageAnalysisFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No schema change: UsageFeature.ImageAnalysis (6) and RagDocType.ImageAnalysis (6)
            // are stored in existing int columns. Data only: PricingEngine fails fast when a
            // feature has no pricing row, so give the new feature one — copied from the latest
            // OCR row (the closest existing "vision -> text" feature), so multiplier / floors start
            // identical and an admin can change them on the Financial page. Falls back to M=1, no
            // floors if OCR has no row. Idempotent.
            //
            // The AI model for the feature is deliberately NOT seeded: the admin assigns an
            // image-input / text-output model to "تحلیل تصویر" on the AI Models page.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 6)
BEGIN
    IF EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 4)
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        SELECT TOP (1) NEWID(), 6, [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], '2026-11-08T00:00:00'
        FROM [tbl_PricingSettings] WHERE [fld_Feature] = 4
        ORDER BY [fld_EffectiveFrom] DESC;
    ELSE
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        VALUES (NEWID(), 6, 1, 0, 0, '2026-11-08T00:00:00');
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [tbl_CustomerModelAssignments] WHERE [fld_Feature] = 6;");
            migrationBuilder.Sql("DELETE FROM [tbl_PricingSettings] WHERE [fld_Feature] = 6;");
        }
    }
}
