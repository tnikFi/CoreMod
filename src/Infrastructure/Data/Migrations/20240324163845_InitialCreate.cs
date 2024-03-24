using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guild_settings",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    command_prefix = table.Column<string>(type: "text", nullable: true),
                    welcome_message = table.Column<string>(type: "text", nullable: true),
                    log_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    report_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guild_settings", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "moderations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    moderator_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    related_case_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    job_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moderations", x => x.id);
                    table.ForeignKey(
                        name: "fk_moderations_moderations_related_case_id",
                        column: x => x.related_case_id,
                        principalTable: "moderations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_moderations_related_case_id",
                table: "moderations",
                column: "related_case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_settings");

            migrationBuilder.DropTable(
                name: "moderations");
        }
    }
}
