using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToRagInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fld_CustomerId",
                table: "tbl_RagInstructions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RagInstructions_fld_CustomerId",
                table: "tbl_RagInstructions",
                column: "fld_CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_RagInstructions_tbl_AiCustomers_fld_CustomerId",
                table: "tbl_RagInstructions",
                column: "fld_CustomerId",
                principalTable: "tbl_AiCustomers",
                principalColumn: "fld_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_RagInstructions_tbl_AiCustomers_fld_CustomerId",
                table: "tbl_RagInstructions");

            migrationBuilder.DropIndex(
                name: "IX_tbl_RagInstructions_fld_CustomerId",
                table: "tbl_RagInstructions");

            migrationBuilder.DropColumn(
                name: "fld_CustomerId",
                table: "tbl_RagInstructions");
        }
    }
}
