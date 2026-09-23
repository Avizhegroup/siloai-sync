using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fld_TurnIndex",
                table: "tbl_AiChatSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tbl_FxRateSettings",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_TomanPerUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_FxRateSettings", x => x.fld_Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_LedgerAccounts",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_CustomerId = table.Column<int>(type: "int", nullable: false),
                    fld_BalanceToman = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    fld_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fld_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_LedgerAccounts", x => x.fld_Id);
                    table.ForeignKey(
                        name: "FK_tbl_LedgerAccounts_tbl_AiCustomers_fld_CustomerId",
                        column: x => x.fld_CustomerId,
                        principalTable: "tbl_AiCustomers",
                        principalColumn: "fld_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_PricingSettings",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_Feature = table.Column<int>(type: "int", nullable: false),
                    fld_Multiplier = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    fld_FloorToman = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_FloorUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    fld_EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PricingSettings", x => x.fld_Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_UsageRecords",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_CustomerId = table.Column<int>(type: "int", nullable: false),
                    fld_Feature = table.Column<int>(type: "int", nullable: false),
                    fld_Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    fld_InputTokens = table.Column<int>(type: "int", nullable: false),
                    fld_CachedTokens = table.Column<int>(type: "int", nullable: false),
                    fld_OutputTokens = table.Column<int>(type: "int", nullable: false),
                    fld_CostUsd = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    fld_FxRateUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_MultiplierUsed = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    fld_FloorTomanUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_ChargeToman = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    fld_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UsageRecords", x => x.fld_Id);
                    table.ForeignKey(
                        name: "FK_tbl_UsageRecords_tbl_AiCustomers_fld_CustomerId",
                        column: x => x.fld_CustomerId,
                        principalTable: "tbl_AiCustomers",
                        principalColumn: "fld_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_LedgerTransactions",
                columns: table => new
                {
                    fld_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fld_Type = table.Column<int>(type: "int", nullable: false),
                    fld_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fld_IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    fld_UsageRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    fld_Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fld_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_LedgerTransactions", x => x.fld_Id);
                    table.ForeignKey(
                        name: "FK_tbl_LedgerTransactions_tbl_LedgerAccounts_fld_AccountId",
                        column: x => x.fld_AccountId,
                        principalTable: "tbl_LedgerAccounts",
                        principalColumn: "fld_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_LedgerTransactions_tbl_UsageRecords_fld_UsageRecordId",
                        column: x => x.fld_UsageRecordId,
                        principalTable: "tbl_UsageRecords",
                        principalColumn: "fld_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FxRateSettings_fld_EffectiveFrom",
                table: "tbl_FxRateSettings",
                column: "fld_EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_LedgerAccounts_fld_CustomerId",
                table: "tbl_LedgerAccounts",
                column: "fld_CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_LedgerTransactions_fld_AccountId",
                table: "tbl_LedgerTransactions",
                column: "fld_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_LedgerTransactions_fld_CreatedAt",
                table: "tbl_LedgerTransactions",
                column: "fld_CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_LedgerTransactions_fld_UsageRecordId",
                table: "tbl_LedgerTransactions",
                column: "fld_UsageRecordId");

            migrationBuilder.CreateIndex(
                name: "UX_tbl_LedgerTransactions_fld_IdempotencyKey",
                table: "tbl_LedgerTransactions",
                column: "fld_IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PricingSettings_fld_Feature_fld_EffectiveFrom",
                table: "tbl_PricingSettings",
                columns: new[] { "fld_Feature", "fld_EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UsageRecords_fld_CreatedAt",
                table: "tbl_UsageRecords",
                column: "fld_CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UsageRecords_fld_CustomerId",
                table: "tbl_UsageRecords",
                column: "fld_CustomerId");

            var seedNow = new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc);

            // Every existing customer gets exactly one ledger account, starting at zero.
            // Credit from this point on changes only through ledger transactions.
            migrationBuilder.Sql(
                """
                INSERT INTO [tbl_LedgerAccounts] ([fld_Id], [fld_CustomerId], [fld_BalanceToman], [fld_CreatedAt], [fld_UpdatedAt])
                SELECT NEWID(), [fld_Id], 0, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM [tbl_AiCustomers] c
                WHERE NOT EXISTS (SELECT 1 FROM [tbl_LedgerAccounts] a WHERE a.[fld_CustomerId] = c.[fld_Id]);
                """);

            // Initial exchange rate — change by inserting a new row with a later EffectiveFrom.
            migrationBuilder.InsertData(
                table: "tbl_FxRateSettings",
                columns: new[] { "fld_Id", "fld_TomanPerUsd", "fld_EffectiveFrom" },
                values: new object[] { new Guid("8c4f1c2e-0f0a-4b6d-9f4a-0e6a0a6d0001"), 100000m, seedNow });

            // One pricing row per UsageFeature (SupportChat=1, Report=2, PageAgent=3, Ocr=4).
            migrationBuilder.InsertData(
                table: "tbl_PricingSettings",
                columns: new[] { "fld_Id", "fld_Feature", "fld_Multiplier", "fld_FloorToman", "fld_FloorUsd", "fld_EffectiveFrom" },
                values: new object[,]
                {
                    { new Guid("8c4f1c2e-0f0a-4b6d-9f4a-0e6a0a6d0101"), 1, 2m, 1000m, 0.01m, seedNow },
                    { new Guid("8c4f1c2e-0f0a-4b6d-9f4a-0e6a0a6d0102"), 2, 2m, 5000m, 0.05m, seedNow },
                    { new Guid("8c4f1c2e-0f0a-4b6d-9f4a-0e6a0a6d0103"), 3, 2m, 2000m, 0.02m, seedNow },
                    { new Guid("8c4f1c2e-0f0a-4b6d-9f4a-0e6a0a6d0104"), 4, 2m, 3000m, 0.03m, seedNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_FxRateSettings");

            migrationBuilder.DropTable(
                name: "tbl_LedgerTransactions");

            migrationBuilder.DropTable(
                name: "tbl_PricingSettings");

            migrationBuilder.DropTable(
                name: "tbl_LedgerAccounts");

            migrationBuilder.DropTable(
                name: "tbl_UsageRecords");

            migrationBuilder.DropColumn(
                name: "fld_TurnIndex",
                table: "tbl_AiChatSessions");
        }
    }
}
