using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGuildIdAsModerationPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moderations_Moderations_RelatedCaseGuildId_RelatedCaseCaseNumber",
                table: "Moderations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Moderations",
                table: "Moderations");

            migrationBuilder.DropIndex(
                name: "IX_Moderations_RelatedCaseGuildId_RelatedCaseCaseNumber",
                table: "Moderations");

            migrationBuilder.DropColumn(
                name: "RelatedCaseGuildId",
                table: "Moderations");

            migrationBuilder.RenameColumn(
                name: "RelatedCaseCaseNumber",
                table: "Moderations",
                newName: "RelatedCaseId");

            migrationBuilder.RenameColumn(
                name: "CaseNumber",
                table: "Moderations",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Moderations",
                table: "Moderations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Moderations_RelatedCaseId",
                table: "Moderations",
                column: "RelatedCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Moderations_Moderations_RelatedCaseId",
                table: "Moderations",
                column: "RelatedCaseId",
                principalTable: "Moderations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moderations_Moderations_RelatedCaseId",
                table: "Moderations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Moderations",
                table: "Moderations");

            migrationBuilder.DropIndex(
                name: "IX_Moderations_RelatedCaseId",
                table: "Moderations");

            migrationBuilder.RenameColumn(
                name: "RelatedCaseId",
                table: "Moderations",
                newName: "RelatedCaseCaseNumber");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Moderations",
                newName: "CaseNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "RelatedCaseGuildId",
                table: "Moderations",
                type: "decimal(20,0)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Moderations",
                table: "Moderations",
                columns: new[] { "GuildId", "CaseNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Moderations_RelatedCaseGuildId_RelatedCaseCaseNumber",
                table: "Moderations",
                columns: new[] { "RelatedCaseGuildId", "RelatedCaseCaseNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_Moderations_Moderations_RelatedCaseGuildId_RelatedCaseCaseNumber",
                table: "Moderations",
                columns: new[] { "RelatedCaseGuildId", "RelatedCaseCaseNumber" },
                principalTable: "Moderations",
                principalColumns: new[] { "GuildId", "CaseNumber" });
        }
    }
}
