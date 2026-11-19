using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeechModelsAndVoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fld_Voices",
                table: "tbl_AiModels",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            // Speech models used to be hard-coded in OpenAISpeechServices. Seed them as ordinary
            // AI models (kind 4 = SpeechToText, 5 = TextToSpeech) and make them the global default
            // for the new features (7 = SpeechToText, 8 = TextToSpeech), so behaviour is unchanged
            // until an admin adds / assigns other models. Idempotent: skipped when the identifier
            // already exists / a global default is already assigned.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [tbl_AiModels] WHERE [fld_Identifier] = 'microsoft/mai-transcribe-2')
    INSERT INTO [tbl_AiModels] ([fld_Id], [fld_Name], [fld_Identifier], [fld_Kind],
        [fld_SupportsTextInput], [fld_SupportsTextOutput], [fld_SupportsImageInput], [fld_SupportsImageOutput],
        [fld_SupportsFileInput], [fld_SupportsFileOutput], [fld_SupportsVoiceInput], [fld_SupportsVoiceOutput],
        [fld_EmbeddingDimensions], [fld_IsDefaultRagModel],
        [fld_InputPricePerMillionTokens], [fld_OutputPricePerMillionTokens], [fld_CachedInputPricePerMillionTokens],
        [fld_IsActive], [fld_CreatedAt], [fld_UpdatedAt], [fld_Voices])
    VALUES (NEWID(), 'MAI Transcribe 2', 'microsoft/mai-transcribe-2', 4,
        0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 1, GETUTCDATE(), GETUTCDATE(), NULL);

IF NOT EXISTS (SELECT 1 FROM [tbl_AiModels] WHERE [fld_Identifier] = 'openai/tts-1-hd')
    INSERT INTO [tbl_AiModels] ([fld_Id], [fld_Name], [fld_Identifier], [fld_Kind],
        [fld_SupportsTextInput], [fld_SupportsTextOutput], [fld_SupportsImageInput], [fld_SupportsImageOutput],
        [fld_SupportsFileInput], [fld_SupportsFileOutput], [fld_SupportsVoiceInput], [fld_SupportsVoiceOutput],
        [fld_EmbeddingDimensions], [fld_IsDefaultRagModel],
        [fld_InputPricePerMillionTokens], [fld_OutputPricePerMillionTokens], [fld_CachedInputPricePerMillionTokens],
        [fld_IsActive], [fld_CreatedAt], [fld_UpdatedAt], [fld_Voices])
    VALUES (NEWID(), 'OpenAI TTS-1 HD', 'openai/tts-1-hd', 5,
        0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 1, GETUTCDATE(), GETUTCDATE(),
        'alloy,ash,coral,echo,fable,nova,onyx,sage,shimmer');

IF NOT EXISTS (SELECT 1 FROM [tbl_CustomerModelAssignments] WHERE [fld_CustomerId] IS NULL AND [fld_Feature] = 7)
    INSERT INTO [tbl_CustomerModelAssignments] ([fld_Id], [fld_CustomerId], [fld_Feature], [fld_AiModelId], [fld_CreatedAt], [fld_UpdatedAt])
    SELECT NEWID(), NULL, 7, [fld_Id], GETUTCDATE(), GETUTCDATE()
    FROM [tbl_AiModels] WHERE [fld_Identifier] = 'microsoft/mai-transcribe-2';

IF NOT EXISTS (SELECT 1 FROM [tbl_CustomerModelAssignments] WHERE [fld_CustomerId] IS NULL AND [fld_Feature] = 8)
    INSERT INTO [tbl_CustomerModelAssignments] ([fld_Id], [fld_CustomerId], [fld_Feature], [fld_AiModelId], [fld_CreatedAt], [fld_UpdatedAt])
    SELECT NEWID(), NULL, 8, [fld_Id], GETUTCDATE(), GETUTCDATE()
    FROM [tbl_AiModels] WHERE [fld_Identifier] = 'openai/tts-1-hd';");

            // Pricing rows for the new features (same approach as image generation / analysis):
            // copied from the latest OCR row, else M=1 with no floors.
            foreach (var feature in new[] { 7, 8 })
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = {feature})
BEGIN
    IF EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 4)
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        SELECT TOP (1) NEWID(), {feature}, [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], '2026-11-19T00:00:00'
        FROM [tbl_PricingSettings] WHERE [fld_Feature] = 4
        ORDER BY [fld_EffectiveFrom] DESC;
    ELSE
        INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
        VALUES (NEWID(), {feature}, 1, 0, 0, '2026-11-19T00:00:00');
END");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [tbl_CustomerModelAssignments] WHERE [fld_Feature] IN (7, 8);");
            migrationBuilder.Sql("DELETE FROM [tbl_PricingSettings] WHERE [fld_Feature] IN (7, 8);");
            migrationBuilder.Sql("DELETE FROM [tbl_AiModels] WHERE [fld_Kind] IN (4, 5);");

            migrationBuilder.DropColumn(
                name: "fld_Voices",
                table: "tbl_AiModels");
        }
    }
}
