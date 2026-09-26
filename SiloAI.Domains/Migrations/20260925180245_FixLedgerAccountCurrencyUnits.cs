using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class FixLedgerAccountCurrencyUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Accounts were backfilled at 0 while the legacy Customer.RemainingCredit column
            // (which is USD-denominated — the admin UI enters and displays it as dollars)
            // still held the customer's real credit. Rebase every account that has no ledger
            // activity yet from that USD credit, converted with the oldest effective FX rate,
            // and record each conversion as the account's first ledger transaction so
            // SUM(transactions) == balance keeps holding. Accounts that already have
            // transactions (real top-ups/charges) are deliberately left untouched.
            migrationBuilder.Sql(
                """
                DECLARE @fx decimal(18,2) = (
                    SELECT TOP (1) [fld_TomanPerUsd]
                    FROM [tbl_FxRateSettings]
                    ORDER BY [fld_EffectiveFrom] ASC);

                IF @fx IS NOT NULL
                BEGIN
                    ;WITH targets AS (
                        SELECT a.[fld_Id] AS AccountId,
                               c.[fld_Id] AS CustomerId,
                               CAST(c.[fld_RemainingCredit] * @fx AS decimal(18,2)) AS BalanceToman
                        FROM [tbl_LedgerAccounts] a
                        INNER JOIN [tbl_AiCustomers] c ON c.[fld_Id] = a.[fld_CustomerId]
                        WHERE NOT EXISTS (SELECT 1 FROM [tbl_LedgerTransactions] t WHERE t.[fld_AccountId] = a.[fld_Id])
                          AND c.[fld_RemainingCredit] > 0)
                    INSERT INTO [tbl_LedgerTransactions]
                        ([fld_Id], [fld_AccountId], [fld_Type], [fld_Amount], [fld_BalanceAfter],
                         [fld_IdempotencyKey], [fld_UsageRecordId], [fld_Description], [fld_CreatedAt])
                    SELECT NEWID(), AccountId, 1, BalanceToman, BalanceToman,
                           'customer-backfill:' + CAST(CustomerId AS nvarchar(20)), NULL,
                           N'اعتبار اولیه (تبدیل از دلار)', SYSUTCDATETIME()
                    FROM targets;

                    ;WITH targets AS (
                        SELECT a.[fld_Id] AS AccountId,
                               CAST(c.[fld_RemainingCredit] * @fx AS decimal(18,2)) AS BalanceToman
                        FROM [tbl_LedgerAccounts] a
                        INNER JOIN [tbl_AiCustomers] c ON c.[fld_Id] = a.[fld_CustomerId]
                        WHERE NOT EXISTS (SELECT 1 FROM [tbl_LedgerTransactions] t WHERE t.[fld_AccountId] = a.[fld_Id])
                          AND c.[fld_RemainingCredit] > 0)
                    UPDATE a
                    SET [fld_BalanceToman] = t.BalanceToman,
                        [fld_UpdatedAt] = SYSUTCDATETIME()
                    FROM [tbl_LedgerAccounts] a
                    INNER JOIN targets t ON t.AccountId = a.[fld_Id];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data correction only — reverting would require knowing which balances were
            // backfilled, so the Down path intentionally leaves the data as-is.
        }
    }
}
