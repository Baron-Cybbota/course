﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using course.Data;

#nullable disable

namespace course.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250610172826_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("course.Models.Administrator", b =>
                {
                    b.Property<int>("IdAdministrator")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdAdministrator"));

                    b.Property<DateTime>("AssignmentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.HasKey("IdAdministrator");

                    b.HasIndex("IdUser")
                        .IsUnique();

                    b.ToTable("Administrators");
                });

            modelBuilder.Entity("course.Models.BlacklistEntry", b =>
                {
                    b.Property<int>("IdBlacklist")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdBlacklist"));

                    b.Property<DateTime>("BlockDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan?>("BlockDuration")
                        .HasColumnType("interval");

                    b.Property<int>("IdAdministrator")
                        .HasColumnType("integer");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("IdBlacklist");

                    b.HasIndex("IdAdministrator");

                    b.HasIndex("IdUser");

                    b.ToTable("BlacklistEntries");
                });

            modelBuilder.Entity("course.Models.BoardGame", b =>
                {
                    b.Property<int>("IdBoardGame")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdBoardGame"));

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<int?>("Difficulty")
                        .HasColumnType("integer");

                    b.Property<TimeSpan?>("EstimatedPlayTime")
                        .HasColumnType("interval");

                    b.Property<string>("Genre")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int?>("MaxPlayers")
                        .HasColumnType("integer");

                    b.Property<int?>("MinPlayers")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int?>("ReleaseYear")
                        .HasColumnType("integer");

                    b.HasKey("IdBoardGame");

                    b.ToTable("BoardGames");
                });

            modelBuilder.Entity("course.Models.Comment", b =>
                {
                    b.Property<int>("IdComment")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdComment"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("EditDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("IdPost")
                        .HasColumnType("integer");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.HasKey("IdComment");

                    b.HasIndex("IdPost");

                    b.HasIndex("IdUser");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("course.Models.Complaint", b =>
                {
                    b.Property<int>("IdComplaint")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdComplaint"));

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("IdAdministrator")
                        .HasColumnType("integer");

                    b.Property<int?>("IdComment")
                        .HasColumnType("integer");

                    b.Property<int?>("IdEvent")
                        .HasColumnType("integer");

                    b.Property<int?>("IdMessage")
                        .HasColumnType("integer");

                    b.Property<int?>("IdPost")
                        .HasColumnType("integer");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<string>("ModeratorNotes")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime?>("ResolutionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("IdComplaint");

                    b.HasIndex("IdAdministrator");

                    b.HasIndex("IdComment");

                    b.HasIndex("IdEvent");

                    b.HasIndex("IdMessage");

                    b.HasIndex("IdPost");

                    b.HasIndex("IdUser");

                    b.ToTable("Complaints", t =>
                        {
                            t.HasCheckConstraint("CHK_Complaint_Target", "((\"IdPost\" IS NOT NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdComment\" IS NOT NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NOT NULL AND \"IdMessage\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NOT NULL))");
                        });
                });

            modelBuilder.Entity("course.Models.Event", b =>
                {
                    b.Property<int>("IdEvent")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdEvent"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<int?>("IdBoardGame")
                        .HasColumnType("integer");

                    b.Property<int>("IdLocation")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<TimeSpan>("Time")
                        .HasColumnType("interval");

                    b.HasKey("IdEvent");

                    b.HasIndex("IdBoardGame");

                    b.HasIndex("IdLocation");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("course.Models.EventParticipant", b =>
                {
                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<int>("IdEvent")
                        .HasColumnType("integer");

                    b.Property<string>("ParticipationStatus")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("IdUser", "IdEvent");

                    b.HasIndex("IdEvent");

                    b.ToTable("EventParticipants");
                });

            modelBuilder.Entity("course.Models.Image", b =>
                {
                    b.Property<int>("IdImage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdImage"));

                    b.Property<int?>("IdBoardGame")
                        .HasColumnType("integer");

                    b.Property<int?>("IdEvent")
                        .HasColumnType("integer");

                    b.Property<int?>("IdPost")
                        .HasColumnType("integer");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("IdImage");

                    b.HasIndex("IdBoardGame");

                    b.HasIndex("IdEvent");

                    b.HasIndex("IdPost");

                    b.ToTable("Images", t =>
                        {
                            t.HasCheckConstraint("CHK_Image_Association", "((\"IdPost\" IS NOT NULL AND \"IdEvent\" IS NULL AND \"IdBoardGame\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdEvent\" IS NOT NULL AND \"IdBoardGame\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdEvent\" IS NULL AND \"IdBoardGame\" IS NOT NULL))");
                        });
                });

            modelBuilder.Entity("course.Models.Location", b =>
                {
                    b.Property<int>("IdLocation")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdLocation"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.HasKey("IdLocation");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("course.Models.Message", b =>
                {
                    b.Property<int>("IdMessage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdMessage"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<int?>("IdEvent")
                        .HasColumnType("integer");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<bool>("IsRead")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("SendDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("IdMessage");

                    b.HasIndex("IdEvent");

                    b.HasIndex("IdUser");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("course.Models.Post", b =>
                {
                    b.Property<int>("IdPost")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdPost"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("EditDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("IdEvent")
                        .HasColumnType("integer");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("boolean");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("IdPost");

                    b.HasIndex("IdEvent");

                    b.HasIndex("IdUser");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("course.Models.Rating", b =>
                {
                    b.Property<int>("IdRating")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdRating"));

                    b.Property<int?>("IdComment")
                        .HasColumnType("integer");

                    b.Property<int?>("IdPost")
                        .HasColumnType("integer");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer");

                    b.Property<bool>("Value")
                        .HasColumnType("boolean");

                    b.HasKey("IdRating");

                    b.HasIndex("IdComment");

                    b.HasIndex("IdPost");

                    b.HasIndex("IdUser");

                    b.ToTable("Ratings", t =>
                        {
                            t.HasCheckConstraint("CHK_Rating_Target", "(\"IdPost\" IS NULL AND \"IdComment\" IS NOT NULL) OR (\"IdPost\" IS NOT NULL AND \"IdComment\" IS NULL)");
                        });
                });

            modelBuilder.Entity("course.Models.User", b =>
                {
                    b.Property<int>("IdUser")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IdUser"));

                    b.Property<bool>("BlockStatus")
                        .HasColumnType("boolean");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("IdUser");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("course.Models.Administrator", b =>
                {
                    b.HasOne("course.Models.User", null)
                        .WithOne()
                        .HasForeignKey("course.Models.Administrator", "IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.BlacklistEntry", b =>
                {
                    b.HasOne("course.Models.Administrator", null)
                        .WithMany()
                        .HasForeignKey("IdAdministrator")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.Comment", b =>
                {
                    b.HasOne("course.Models.Post", null)
                        .WithMany()
                        .HasForeignKey("IdPost")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.Complaint", b =>
                {
                    b.HasOne("course.Models.Administrator", null)
                        .WithMany()
                        .HasForeignKey("IdAdministrator");

                    b.HasOne("course.Models.Comment", null)
                        .WithMany()
                        .HasForeignKey("IdComment");

                    b.HasOne("course.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("IdEvent");

                    b.HasOne("course.Models.Message", null)
                        .WithMany()
                        .HasForeignKey("IdMessage");

                    b.HasOne("course.Models.Post", null)
                        .WithMany()
                        .HasForeignKey("IdPost");

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.Event", b =>
                {
                    b.HasOne("course.Models.BoardGame", null)
                        .WithMany()
                        .HasForeignKey("IdBoardGame");

                    b.HasOne("course.Models.Location", null)
                        .WithMany()
                        .HasForeignKey("IdLocation")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.EventParticipant", b =>
                {
                    b.HasOne("course.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("IdEvent")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.Image", b =>
                {
                    b.HasOne("course.Models.BoardGame", null)
                        .WithMany()
                        .HasForeignKey("IdBoardGame");

                    b.HasOne("course.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("IdEvent");

                    b.HasOne("course.Models.Post", null)
                        .WithMany()
                        .HasForeignKey("IdPost");
                });

            modelBuilder.Entity("course.Models.Message", b =>
                {
                    b.HasOne("course.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("IdEvent");

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.Post", b =>
                {
                    b.HasOne("course.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("IdEvent");

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("course.Models.Rating", b =>
                {
                    b.HasOne("course.Models.Comment", null)
                        .WithMany()
                        .HasForeignKey("IdComment");

                    b.HasOne("course.Models.Post", null)
                        .WithMany()
                        .HasForeignKey("IdPost");

                    b.HasOne("course.Models.User", null)
                        .WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
