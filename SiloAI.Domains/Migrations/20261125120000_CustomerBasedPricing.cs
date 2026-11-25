using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class CustomerBasedPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Pricing becomes a property of each customer.
            migrationBuilder.AddColumn<decimal>(
                name: "fld_TomanPerUsd", table: "tbl_AiCustomers",
                type: "decimal(18,2)", nullable: false, defaultValue: 100000m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_Multiplier", table: "tbl_AiCustomers",
                type: "decimal(18,4)", nullable: false, defaultValue: 2m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_FloorToman", table: "tbl_AiCustomers",
                type: "decimal(18,2)", nullable: false, defaultValue: 1000m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_FloorUsd", table: "tbl_AiCustomers",
                type: "decimal(18,4)", nullable: false, defaultValue: 0.01m);

            // 2) Every existing customer keeps exactly what they are billed with today: copy the
            //    latest global exchange rate and the latest SupportChat pricing row onto them.
            migrationBuilder.Sql(
                """
                DECLARE @fx decimal(18,2) = (SELECT TOP (1) [fld_TomanPerUsd] FROM [tbl_FxRateSettings]
                                             WHERE [fld_EffectiveFrom] <= SYSDATETIME() ORDER BY [fld_EffectiveFrom] DESC);
                IF @fx IS NOT NULL AND @fx > 0
                    UPDATE [tbl_AiCustomers] SET [fld_TomanPerUsd] = @fx;

                IF EXISTS (SELECT 1 FROM [tbl_PricingSettings] WHERE [fld_Feature] = 1 AND [fld_EffectiveFrom] <= SYSDATETIME())
                BEGIN
                    DECLARE @m decimal(18,4), @ft decimal(18,2), @fu decimal(18,4);
                    SELECT TOP (1) @m = [fld_Multiplier], @ft = [fld_FloorToman], @fu = [fld_FloorUsd]
                    FROM [tbl_PricingSettings]
                    WHERE [fld_Feature] = 1 AND [fld_EffectiveFrom] <= SYSDATETIME()
                    ORDER BY [fld_EffectiveFrom] DESC;

                    UPDATE [tbl_AiCustomers] SET [fld_Multiplier] = @m, [fld_FloorToman] = @ft, [fld_FloorUsd] = @fu;
                END
                """);

            // 3) The global configuration is gone.
            migrationBuilder.DropTable(name: "tbl_FxRateSettings");
            migrationBuilder.DropTable(name: "tbl_PricingSettings");

            // 4) The credit lives only in the ledger (tbl_LedgerAccounts.fld_BalanceToman); the second,
            //    USD copy on the customer is removed so the two can never disagree again.
            migrationBuilder.DropColumn(name: "fld_RemainingCredit", table: "tbl_AiCustomers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "fld_RemainingCredit", table: "tbl_AiCustomers",
                type: "decimal(18,8)", nullable: false, defaultValue: 0m);

            // Rebuild the legacy USD copy from the ledger (Toman ÷ the customer's own rate).
            migrationBuilder.Sql(
                """
                UPDATE c SET c.[fld_RemainingCredit] = CASE WHEN c.[fld_TomanPerUsd] > 0 THEN a.[fld_BalanceToman] / c.[fld_TomanPerUsd] ELSE 0 END
                FROM [tbl_AiCustomers] c INNER JOIN [tbl_LedgerAccounts] a ON a.[fld_CustomerId] = c.[fld_Id];
                """);

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

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FxRateSettings_fld_EffectiveFrom",
                table: "tbl_FxRateSettings",
                column: "fld_EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PricingSettings_fld_Feature_fld_EffectiveFrom",
                table: "tbl_PricingSettings",
                columns: new[] { "fld_Feature", "fld_EffectiveFrom" });

            // Seed one global row per feature from the most common customer profile so the previous
            // schema is usable again after a rollback.
            migrationBuilder.Sql(
                """
                DECLARE @now datetime2 = SYSDATETIME();
                INSERT INTO [tbl_FxRateSettings] ([fld_Id], [fld_TomanPerUsd], [fld_EffectiveFrom])
                VALUES (NEWID(), COALESCE((SELECT TOP (1) [fld_TomanPerUsd] FROM [tbl_AiCustomers] ORDER BY [fld_Id]), 100000), @now);

                INSERT INTO [tbl_PricingSettings] ([fld_Id], [fld_Feature], [fld_Multiplier], [fld_FloorToman], [fld_FloorUsd], [fld_EffectiveFrom])
                SELECT NEWID(), f.[Feature],
                       COALESCE((SELECT TOP (1) [fld_Multiplier] FROM [tbl_AiCustomers] ORDER BY [fld_Id]), 2),
                       COALESCE((SELECT TOP (1) [fld_FloorToman] FROM [tbl_AiCustomers] ORDER BY [fld_Id]), 1000),
                       COALESCE((SELECT TOP (1) [fld_FloorUsd] FROM [tbl_AiCustomers] ORDER BY [fld_Id]), 0.01),
                       @now
                FROM (VALUES (1),(2),(3),(4),(7),(8)) AS f([Feature]);
                """);

            migrationBuilder.DropColumn(name: "fld_TomanPerUsd", table: "tbl_AiCustomers");
            migrationBuilder.DropColumn(name: "fld_Multiplier", table: "tbl_AiCustomers");
            migrationBuilder.DropColumn(name: "fld_FloorToman", table: "tbl_AiCustomers");
            migrationBuilder.DropColumn(name: "fld_FloorUsd", table: "tbl_AiCustomers");
        }
    }
}
