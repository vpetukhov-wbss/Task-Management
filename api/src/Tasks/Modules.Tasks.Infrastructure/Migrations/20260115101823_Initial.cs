#pragma warning disable IDE0161, MA0051
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Tasks.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
    {
	private static readonly string[] columns = new[] { "BoardId", "Status" };
	private static readonly string[] columnsArray = new[] { "UserId", "Priority" };
	private static readonly string[] columnsArray0 = new[] { "UserId", "Status" };

	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tasks");

            migrationBuilder.CreateTable(
                name: "boards",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tasks_boards_BoardId",
                        column: x => x.BoardId,
                        principalSchema: "tasks",
                        principalTable: "boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_boards_UserId",
                schema: "tasks",
                table: "boards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_BoardId",
                schema: "tasks",
                table: "tasks",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_BoardId_Status",
                schema: "tasks",
                table: "tasks",
                columns: columns);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_Priority",
                schema: "tasks",
                table: "tasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_Status",
                schema: "tasks",
                table: "tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId",
                schema: "tasks",
                table: "tasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId_Priority",
                schema: "tasks",
                table: "tasks",
                columns: columnsArray);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId_Status",
                schema: "tasks",
                table: "tasks",
                columns: columnsArray0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasks",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "boards",
                schema: "tasks");
        }
    }
