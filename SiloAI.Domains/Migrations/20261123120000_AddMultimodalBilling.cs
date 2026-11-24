using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddMultimodalBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "fld_PricePerImageUsd", table: "tbl_AiModels",
                type: "decimal(18,8)", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_PricePerAudioMinuteUsd", table: "tbl_AiModels",
                type: "decimal(18,8)", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_PricePerMillionCharactersUsd", table: "tbl_AiModels",
                type: "decimal(18,8)", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "fld_ImageCount", table: "tbl_UsageRecords",
                type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_AudioSeconds", table: "tbl_UsageRecords",
                type: "decimal(18,3)", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "fld_Characters", table: "tbl_UsageRecords",
                type: "int", nullable: false, defaultValue: 0);

            // Features that were previously unmetered (OCR is 4, speech 7/8) need a pricing row to be
            // chargeable. Idempotent: only inserted where a feature has none, copying the SupportChat (1)
            // settings, else M=1 with no floors.
            foreach (var feature in new[] { 4, 7, 8 })
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = {feature})
BEGIN
    IF EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 1)
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        SELECT TOP (1) NEWID(), {feature}, [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], '2026-11-23T00:00:00'
        FROM [tbl_PricingSettings] WHERE [fld_Feature] = 1
        ORDER BY [fld_EffectiveFrom] DESC;
    ELSE
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        VALUES (NEWID(), {feature}, 1, 0, 0, '2026-11-23T00:00:00');
END");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "fld_PricePerImageUsd", table: "tbl_AiModels");
            migrationBuilder.DropColumn(name: "fld_PricePerAudioMinuteUsd", table: "tbl_AiModels");
            migrationBuilder.DropColumn(name: "fld_PricePerMillionCharactersUsd", table: "tbl_AiModels");
            migrationBuilder.DropColumn(name: "fld_ImageCount", table: "tbl_UsageRecords");
            migrationBuilder.DropColumn(name: "fld_AudioSeconds", table: "tbl_UsageRecords");
            migrationBuilder.DropColumn(name: "fld_Characters", table: "tbl_UsageRecords");
        }
    }
}
