using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedCaseReferenceToModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PardonedCaseNumber",
                table: "Moderations",
                newName: "RelatedCaseCaseNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "RelatedCaseGuildId",
                table: "Moderations",
                type: "decimal(20,0)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moderations_Moderations_RelatedCaseGuildId_RelatedCaseCaseNumber",
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
                newName: "PardonedCaseNumber");
        }
    }
}
