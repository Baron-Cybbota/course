using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace course.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardGames",
                columns: table => new
                {
                    IdBoardGame = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: true),
                    MinPlayers = table.Column<int>(type: "integer", nullable: true),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: true),
                    EstimatedPlayTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Genre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGames", x => x.IdBoardGame);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    IdLocation = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.IdLocation);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlockStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.IdUser);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    IdEvent = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdBoardGame = table.Column<int>(type: "integer", nullable: true),
                    IdLocation = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.IdEvent);
                    table.ForeignKey(
                        name: "FK_Events_BoardGames_IdBoardGame",
                        column: x => x.IdBoardGame,
                        principalTable: "BoardGames",
                        principalColumn: "IdBoardGame");
                    table.ForeignKey(
                        name: "FK_Events_Locations_IdLocation",
                        column: x => x.IdLocation,
                        principalTable: "Locations",
                        principalColumn: "IdLocation",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Administrators",
                columns: table => new
                {
                    IdAdministrator = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrators", x => x.IdAdministrator);
                    table.ForeignKey(
                        name: "FK_Administrators_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    IdEvent = table.Column<int>(type: "integer", nullable: false),
                    ParticipationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipants", x => new { x.IdUser, x.IdEvent });
                    table.ForeignKey(
                        name: "FK_EventParticipants_Events_IdEvent",
                        column: x => x.IdEvent,
                        principalTable: "Events",
                        principalColumn: "IdEvent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventParticipants_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    IdMessage = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    IdEvent = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SendDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.IdMessage);
                    table.ForeignKey(
                        name: "FK_Messages_Events_IdEvent",
                        column: x => x.IdEvent,
                        principalTable: "Events",
                        principalColumn: "IdEvent");
                    table.ForeignKey(
                        name: "FK_Messages_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    IdPost = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    IdEvent = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.IdPost);
                    table.ForeignKey(
                        name: "FK_Posts_Events_IdEvent",
                        column: x => x.IdEvent,
                        principalTable: "Events",
                        principalColumn: "IdEvent");
                    table.ForeignKey(
                        name: "FK_Posts_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistEntries",
                columns: table => new
                {
                    IdBlacklist = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAdministrator = table.Column<int>(type: "integer", nullable: false),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BlockDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlockDuration = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistEntries", x => x.IdBlacklist);
                    table.ForeignKey(
                        name: "FK_BlacklistEntries_Administrators_IdAdministrator",
                        column: x => x.IdAdministrator,
                        principalTable: "Administrators",
                        principalColumn: "IdAdministrator");
                    table.ForeignKey(
                        name: "FK_BlacklistEntries_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    IdComment = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPost = table.Column<int>(type: "integer", nullable: false),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.IdComment);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_IdPost",
                        column: x => x.IdPost,
                        principalTable: "Posts",
                        principalColumn: "IdPost",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    IdImage = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPost = table.Column<int>(type: "integer", nullable: true),
                    IdEvent = table.Column<int>(type: "integer", nullable: true),
                    IdBoardGame = table.Column<int>(type: "integer", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.IdImage);
                    table.CheckConstraint("CHK_Image_Association", "((\"IdPost\" IS NOT NULL AND \"IdEvent\" IS NULL AND \"IdBoardGame\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdEvent\" IS NOT NULL AND \"IdBoardGame\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdEvent\" IS NULL AND \"IdBoardGame\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_Images_BoardGames_IdBoardGame",
                        column: x => x.IdBoardGame,
                        principalTable: "BoardGames",
                        principalColumn: "IdBoardGame");
                    table.ForeignKey(
                        name: "FK_Images_Events_IdEvent",
                        column: x => x.IdEvent,
                        principalTable: "Events",
                        principalColumn: "IdEvent");
                    table.ForeignKey(
                        name: "FK_Images_Posts_IdPost",
                        column: x => x.IdPost,
                        principalTable: "Posts",
                        principalColumn: "IdPost");
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    IdComplaint = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    IdAdministrator = table.Column<int>(type: "integer", nullable: true),
                    IdPost = table.Column<int>(type: "integer", nullable: true),
                    IdComment = table.Column<int>(type: "integer", nullable: true),
                    IdEvent = table.Column<int>(type: "integer", nullable: true),
                    IdMessage = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolutionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModeratorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.IdComplaint);
                    table.CheckConstraint("CHK_Complaint_Target", "((\"IdPost\" IS NOT NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdComment\" IS NOT NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NOT NULL AND \"IdMessage\" IS NULL) OR (\"IdPost\" IS NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_Complaints_Administrators_IdAdministrator",
                        column: x => x.IdAdministrator,
                        principalTable: "Administrators",
                        principalColumn: "IdAdministrator");
                    table.ForeignKey(
                        name: "FK_Complaints_Comments_IdComment",
                        column: x => x.IdComment,
                        principalTable: "Comments",
                        principalColumn: "IdComment");
                    table.ForeignKey(
                        name: "FK_Complaints_Events_IdEvent",
                        column: x => x.IdEvent,
                        principalTable: "Events",
                        principalColumn: "IdEvent");
                    table.ForeignKey(
                        name: "FK_Complaints_Messages_IdMessage",
                        column: x => x.IdMessage,
                        principalTable: "Messages",
                        principalColumn: "IdMessage");
                    table.ForeignKey(
                        name: "FK_Complaints_Posts_IdPost",
                        column: x => x.IdPost,
                        principalTable: "Posts",
                        principalColumn: "IdPost");
                    table.ForeignKey(
                        name: "FK_Complaints_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    IdRating = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    IdPost = table.Column<int>(type: "integer", nullable: true),
                    IdComment = table.Column<int>(type: "integer", nullable: true),
                    Value = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.IdRating);
                    table.CheckConstraint("CHK_Rating_Target", "(\"IdPost\" IS NULL AND \"IdComment\" IS NOT NULL) OR (\"IdPost\" IS NOT NULL AND \"IdComment\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Ratings_Comments_IdComment",
                        column: x => x.IdComment,
                        principalTable: "Comments",
                        principalColumn: "IdComment");
                    table.ForeignKey(
                        name: "FK_Ratings_Posts_IdPost",
                        column: x => x.IdPost,
                        principalTable: "Posts",
                        principalColumn: "IdPost");
                    table.ForeignKey(
                        name: "FK_Ratings_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Administrators_IdUser",
                table: "Administrators",
                column: "IdUser",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistEntries_IdAdministrator",
                table: "BlacklistEntries",
                column: "IdAdministrator");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistEntries_IdUser",
                table: "BlacklistEntries",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IdPost",
                table: "Comments",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IdUser",
                table: "Comments",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IdAdministrator",
                table: "Complaints",
                column: "IdAdministrator");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IdComment",
                table: "Complaints",
                column: "IdComment");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IdEvent",
                table: "Complaints",
                column: "IdEvent");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IdMessage",
                table: "Complaints",
                column: "IdMessage");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IdPost",
                table: "Complaints",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IdUser",
                table: "Complaints",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_IdEvent",
                table: "EventParticipants",
                column: "IdEvent");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IdBoardGame",
                table: "Events",
                column: "IdBoardGame");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IdLocation",
                table: "Events",
                column: "IdLocation");

            migrationBuilder.CreateIndex(
                name: "IX_Images_IdBoardGame",
                table: "Images",
                column: "IdBoardGame");

            migrationBuilder.CreateIndex(
                name: "IX_Images_IdEvent",
                table: "Images",
                column: "IdEvent");

            migrationBuilder.CreateIndex(
                name: "IX_Images_IdPost",
                table: "Images",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdEvent",
                table: "Messages",
                column: "IdEvent");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdUser",
                table: "Messages",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IdEvent",
                table: "Posts",
                column: "IdEvent");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IdUser",
                table: "Posts",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_IdComment",
                table: "Ratings",
                column: "IdComment");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_IdPost",
                table: "Ratings",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_IdUser",
                table: "Ratings",
                column: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistEntries");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "EventParticipants");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Administrators");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BoardGames");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
