using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExecutionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false),
                    OriginalPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterpretedIntent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedEntityIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedCount = table.Column<int>(type: "int", nullable: false),
                    ExecutedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasExecuted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentAuditLogs_ExecutionId",
                table: "AgentAuditLogs",
                column: "ExecutionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentAuditLogs");
        }
    }
}
