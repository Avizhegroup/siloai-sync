using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToRagDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fld_CustomerId",
                table: "tbl_RagDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RagDocuments_fld_CustomerId",
                table: "tbl_RagDocuments",
                column: "fld_CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_RagDocuments_tbl_AiCustomers_fld_CustomerId",
                table: "tbl_RagDocuments",
                column: "fld_CustomerId",
                principalTable: "tbl_AiCustomers",
                principalColumn: "fld_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_RagDocuments_tbl_AiCustomers_fld_CustomerId",
                table: "tbl_RagDocuments");

            migrationBuilder.DropIndex(
                name: "IX_tbl_RagDocuments_fld_CustomerId",
                table: "tbl_RagDocuments");

            migrationBuilder.DropColumn(
                name: "fld_CustomerId",
                table: "tbl_RagDocuments");
        }
    }
}
