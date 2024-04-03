﻿// <auto-generated />
using System;
using Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240403151548_AddMinimumReportRoleToGuildSettings")]
    partial class AddMinimumReportRoleToGuildSettings
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Models.GuildSettings", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("CommandPrefix")
                        .HasColumnType("text")
                        .HasColumnName("command_prefix");

                    b.Property<decimal?>("LogChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("log_channel_id");

                    b.Property<decimal?>("MinimumReportRole")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("minimum_report_role");

                    b.Property<decimal?>("ReportChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("report_channel_id");

                    b.Property<string>("WelcomeMessage")
                        .HasColumnType("text")
                        .HasColumnName("welcome_message");

                    b.HasKey("GuildId")
                        .HasName("pk_guild_settings");

                    b.ToTable("guild_settings", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Moderation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("JobId")
                        .HasColumnType("text")
                        .HasColumnName("job_id");

                    b.Property<decimal>("ModeratorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("moderator_id");

                    b.Property<string>("Reason")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("reason");

                    b.Property<int?>("RelatedCaseId")
                        .HasColumnType("integer")
                        .HasColumnName("related_case_id");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.Property<byte>("Type")
                        .HasColumnType("smallint")
                        .HasColumnName("type");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_moderations");

                    b.HasIndex("RelatedCaseId")
                        .HasDatabaseName("ix_moderations_related_case_id");

                    b.ToTable("moderations", (string)null);
                });

            modelBuilder.Entity("Domain.Models.Moderation", b =>
                {
                    b.HasOne("Domain.Models.Moderation", "RelatedCase")
                        .WithMany()
                        .HasForeignKey("RelatedCaseId")
                        .HasConstraintName("fk_moderations_moderations_related_case_id");

                    b.Navigation("RelatedCase");
                });
#pragma warning restore 612, 618
        }
    }
}