using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class GlobalFxRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) The dollar-to-Toman rate is a single GLOBAL setting again (historical rows, latest wins).
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

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FxRateSettings_fld_EffectiveFrom",
                table: "tbl_FxRateSettings",
                column: "fld_EffectiveFrom");

            // 2) Seed the global rate with the one customers are billed with today (the most common
            //    per-customer rate; 100000 when there are no customers).
            migrationBuilder.Sql(
                """
                DECLARE @fx decimal(18,2) = (SELECT TOP (1) [fld_TomanPerUsd] FROM [tbl_AiCustomers]
                                             GROUP BY [fld_TomanPerUsd] ORDER BY COUNT(*) DESC);
                IF @fx IS NULL OR @fx <= 0 SET @fx = 100000;

                INSERT INTO [tbl_FxRateSettings] ([fld_Id], [fld_TomanPerUsd], [fld_EffectiveFrom])
                VALUES (NEWID(), @fx, SYSDATETIME());
                """);

            // 3) Only the multiplier stays per customer; rate and the two minimum-charge columns go.
            migrationBuilder.DropColumn(name: "fld_TomanPerUsd", table: "tbl_AiCustomers");
            migrationBuilder.DropColumn(name: "fld_FloorToman", table: "tbl_AiCustomers");
            migrationBuilder.DropColumn(name: "fld_FloorUsd", table: "tbl_AiCustomers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "fld_TomanPerUsd", table: "tbl_AiCustomers",
                type: "decimal(18,2)", nullable: false, defaultValue: 100000m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_FloorToman", table: "tbl_AiCustomers",
                type: "decimal(18,2)", nullable: false, defaultValue: 1000m);

            migrationBuilder.AddColumn<decimal>(
                name: "fld_FloorUsd", table: "tbl_AiCustomers",
                type: "decimal(18,4)", nullable: false, defaultValue: 0.01m);

            migrationBuilder.Sql(
                """
                DECLARE @fx decimal(18,2) = (SELECT TOP (1) [fld_TomanPerUsd] FROM [tbl_FxRateSettings]
                                             WHERE [fld_EffectiveFrom] <= SYSDATETIME() ORDER BY [fld_EffectiveFrom] DESC);
                IF @fx IS NOT NULL AND @fx > 0
                    UPDATE [tbl_AiCustomers] SET [fld_TomanPerUsd] = @fx;
                """);

            migrationBuilder.DropTable(name: "tbl_FxRateSettings");
        }
    }
}
