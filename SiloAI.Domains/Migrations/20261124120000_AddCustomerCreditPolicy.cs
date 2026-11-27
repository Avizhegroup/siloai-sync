using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCreditPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // true = the customer is stopped when credit runs out. Every existing customer keeps the
            // behaviour they have today (stopped); an admin can untick it per customer to allow a
            // negative balance.
            migrationBuilder.AddColumn<bool>(
                name: "fld_StopOnInsufficientCredit",
                table: "tbl_AiCustomers",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fld_StopOnInsufficientCredit",
                table: "tbl_AiCustomers");
        }
    }
}
