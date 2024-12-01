using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PairProgrammingApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "health_checks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_successful = table.Column<bool>(type: "boolean", nullable: false),
                    created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_context = table.Column<string>(type: "text", nullable: false),
                    last_updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_update_context = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_checks", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "health_checks");
        }
    }
}
