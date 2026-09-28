using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddAiModelManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_AiModels",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    fld_Identifier = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    fld_Kind = table.Column<int>(type: "int", nullable: false),
                    fld_SupportsTextInput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsTextOutput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsImageInput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsImageOutput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsFileInput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsFileOutput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsVoiceInput = table.Column<bool>(type: "bit", nullable: false),
                    fld_SupportsVoiceOutput = table.Column<bool>(type: "bit", nullable: false),
                    fld_EmbeddingDimensions = table.Column<int>(type: "int", nullable: true),
                    fld_IsDefaultRagModel = table.Column<bool>(type: "bit", nullable: false),
                    fld_InputPricePerMillionTokens = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    fld_OutputPricePerMillionTokens = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    fld_CachedInputPricePerMillionTokens = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    fld_IsActive = table.Column<bool>(type: "bit", nullable: false),
                    fld_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fld_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_AiModels", x => x.fld_Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_CustomerModelAssignments",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_CustomerId = table.Column<int>(type: "int", nullable: true),
                    fld_Feature = table.Column<int>(type: "int", nullable: false),
                    fld_AiModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fld_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CustomerModelAssignments", x => x.fld_Id);
                    table.ForeignKey(
                        name: "FK_tbl_CustomerModelAssignments_tbl_AiCustomers_fld_CustomerId",
                        column: x => x.fld_CustomerId,
                        principalTable: "tbl_AiCustomers",
                        principalColumn: "fld_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_CustomerModelAssignments_tbl_AiModels_fld_AiModelId",
                        column: x => x.fld_AiModelId,
                        principalTable: "tbl_AiModels",
                        principalColumn: "fld_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_tbl_AiModels_fld_Identifier",
                table: "tbl_AiModels",
                column: "fld_Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_tbl_AiModels_fld_IsDefaultRagModel",
                table: "tbl_AiModels",
                column: "fld_IsDefaultRagModel",
                unique: true,
                filter: "[fld_IsDefaultRagModel] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_tbl_CustomerModelAssignments_fld_CustomerId_fld_Feature",
                table: "tbl_CustomerModelAssignments",
                columns: new[] { "fld_CustomerId", "fld_Feature" },
                unique: true,
                filter: "[fld_CustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_tbl_CustomerModelAssignments_fld_Feature_Default",
                table: "tbl_CustomerModelAssignments",
                column: "fld_Feature",
                unique: true,
                filter: "[fld_CustomerId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CustomerModelAssignments_fld_AiModelId",
                table: "tbl_CustomerModelAssignments",
                column: "fld_AiModelId");

            // --- Seed data ---------------------------------------------------------------
            // Reproduces exactly what appsettings.json currently configures, so behavior does
            // not change the moment this migration runs — only where the configuration lives.
            // Source values, from the OpenAI / AiPricing sections being removed:
            //   OpenAI:MainModel / VoiceModel / RagModel = "openai/gpt-4.1-mini" (all three the
            //     same model today), AiPricing:Models["openai/gpt-4.1-mini"] = 0.4 / 1.6 / 0.1
            //     (input / output / cached, USD per million tokens).
            //   OpenAI:EmbeddingModel = "openai/text-embedding-3-small", EmbeddingDimensions = 1536.
            // The embedding model's price was NOT previously tracked anywhere in this app (no
            // AiPricing entry existed for it), so 0.02 USD/million input tokens below is a
            // placeholder based on the well-known public price for this model — an admin should
            // verify it on the new AI Models page before relying on it for real cost reporting.

            var chatModelId = new Guid("24f2d74d-ba09-4e65-bfbf-bc37240f715d");
            var embeddingModelId = new Guid("9c52c340-7363-497f-b95b-00bcad51a40d");
            var seededAt = new DateTime(2026, 9, 26, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "tbl_AiModels",
                columns: new[]
                {
                    "fld_Id", "fld_Name", "fld_Identifier", "fld_Kind",
                    "fld_SupportsTextInput", "fld_SupportsTextOutput",
                    "fld_SupportsImageInput", "fld_SupportsImageOutput",
                    "fld_SupportsFileInput", "fld_SupportsFileOutput",
                    "fld_SupportsVoiceInput", "fld_SupportsVoiceOutput",
                    "fld_EmbeddingDimensions", "fld_IsDefaultRagModel",
                    "fld_InputPricePerMillionTokens", "fld_OutputPricePerMillionTokens", "fld_CachedInputPricePerMillionTokens",
                    "fld_IsActive", "fld_CreatedAt", "fld_UpdatedAt"
                },
                values: new object[,]
                {
                    {
                        chatModelId, "GPT-4.1 Mini (چت و تصویر)", "openai/gpt-4.1-mini", 2 /* Normal */,
                        true, true,   // text in/out
                        true, false,  // image in (OCR/vision) / out
                        false, false, // file in/out
                        false, false, // voice in/out
                        null, false,
                        0.4m, 1.6m, 0.1m,
                        true, seededAt, seededAt
                    },
                    {
                        embeddingModelId, "Text Embedding 3 Small", "openai/text-embedding-3-small", 1 /* Rag */,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        1536, true,
                        0.02m, 0m, 0m,
                        true, seededAt, seededAt
                    }
                });

            migrationBuilder.InsertData(
                table: "tbl_CustomerModelAssignments",
                columns: new[] { "fld_Id", "fld_CustomerId", "fld_Feature", "fld_AiModelId", "fld_CreatedAt", "fld_UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("cdd341b8-aab9-4c77-862e-68615d9bb9b2"), null, 1 /* SupportChat */, chatModelId, seededAt, seededAt },
                    { new Guid("b3411a40-b869-463a-8a58-c60a2e4e7f02"), null, 2 /* Report */,      chatModelId, seededAt, seededAt },
                    { new Guid("631574b2-41aa-4206-82a3-2c96af67e196"), null, 3 /* PageAgent */,    chatModelId, seededAt, seededAt },
                    { new Guid("a83a4368-2dd7-4bea-9a74-2a8e32cea3d4"), null, 4 /* Ocr */,          chatModelId, seededAt, seededAt }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_CustomerModelAssignments");

            migrationBuilder.DropTable(
                name: "tbl_AiModels");
        }
    }
}
