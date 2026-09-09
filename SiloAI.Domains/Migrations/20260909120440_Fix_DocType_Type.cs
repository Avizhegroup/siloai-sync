using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiloAI.Domains.Migrations
{
    /// <inheritdoc />
    public partial class Fix_DocType_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "fld_DocType",
                table: "tbl_RagInstructions",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "GeneralChat");

            migrationBuilder.AlterColumn<string>(
                name: "fld_DocType",
                table: "tbl_RagDocuments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "GeneralChat");

            migrationBuilder.AlterColumn<int>(
                name: "fld_InstructionKey",
                table: "tbl_AiConversations",
                type: "int",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "fld_DocType",
                table: "tbl_RagInstructions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "GeneralChat",
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "fld_DocType",
                table: "tbl_RagDocuments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "GeneralChat",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "fld_InstructionKey",
                table: "tbl_AiConversations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
