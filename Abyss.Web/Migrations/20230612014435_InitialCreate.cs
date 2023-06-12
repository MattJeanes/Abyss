using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Abyss.Web.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Identifier = table.Column<string>(type: "text", nullable: true),
                Name = table.Column<string>(type: "text", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Servers",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Tag = table.Column<string>(type: "text", nullable: true),
                SnapshotId = table.Column<long>(type: "bigint", nullable: true),
                Size = table.Column<string>(type: "text", nullable: true),
                Resize = table.Column<string>(type: "text", nullable: true),
                Region = table.Column<string>(type: "text", nullable: true),
                DropletId = table.Column<long>(type: "bigint", nullable: true),
                StatusId = table.Column<int>(type: "integer", nullable: false),
                IPAddress = table.Column<string>(type: "text", nullable: true),
                DNSRecord = table.Column<string>(type: "text", nullable: true),
                Name = table.Column<string>(type: "text", nullable: true),
                RemindAfterMinutes = table.Column<int>(type: "integer", nullable: true),
                ReminderIntervalMinutes = table.Column<int>(type: "integer", nullable: true),
                NextReminder = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CloudType = table.Column<int>(type: "integer", nullable: false),
                ResourceId = table.Column<string>(type: "text", nullable: true),
                Alias = table.Column<string>(type: "text", nullable: true),
                Type = table.Column<int>(type: "integer", nullable: false),
                ApiBaseUrl = table.Column<string>(type: "text", nullable: true),
                ApiKey = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Servers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "GPTModels",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                Identifier = table.Column<string>(type: "text", nullable: true),
                PermissionId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GPTModels", x => x.Id);
                table.ForeignKey(
                    name: "FK_GPTModels_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "PermissionRole",
            columns: table => new
            {
                PermissionsId = table.Column<int>(type: "integer", nullable: false),
                RolesId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PermissionRole", x => new { x.PermissionsId, x.RolesId });
                table.ForeignKey(
                    name: "FK_PermissionRole_Permissions_PermissionsId",
                    column: x => x.PermissionsId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PermissionRole_Roles_RolesId",
                    column: x => x.RolesId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermission",
            columns: table => new
            {
                RoleId = table.Column<int>(type: "integer", nullable: false),
                PermissionId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_RolePermission_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermission_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                RoleId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.ForeignKey(
                    name: "FK_Users_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Revoked = table.Column<bool>(type: "boolean", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_RefreshTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserAuthentication",
            columns: table => new
            {
                UserId = table.Column<int>(type: "integer", nullable: false),
                SchemeTypeId = table.Column<int>(type: "integer", nullable: false),
                Identifier = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserAuthentication", x => new { x.UserId, x.SchemeTypeId });
                table.ForeignKey(
                    name: "FK_UserAuthentication_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GPTModels_PermissionId",
            table: "GPTModels",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_PermissionRole_RolesId",
            table: "PermissionRole",
            column: "RolesId");

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_UserId",
            table: "RefreshTokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermission_PermissionId",
            table: "RolePermission",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_RoleId",
            table: "Users",
            column: "RoleId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GPTModels");

        migrationBuilder.DropTable(
            name: "PermissionRole");

        migrationBuilder.DropTable(
            name: "RefreshTokens");

        migrationBuilder.DropTable(
            name: "RolePermission");

        migrationBuilder.DropTable(
            name: "Servers");

        migrationBuilder.DropTable(
            name: "UserAuthentication");

        migrationBuilder.DropTable(
            name: "Permissions");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Roles");
    }
}
