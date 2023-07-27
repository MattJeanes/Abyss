using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abyss.Web.Migrations;

/// <inheritdoc />
public partial class RemoveOldServerColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DropletId",
            table: "Servers");

        migrationBuilder.DropColumn(
            name: "Region",
            table: "Servers");

        migrationBuilder.DropColumn(
            name: "Resize",
            table: "Servers");

        migrationBuilder.DropColumn(
            name: "Size",
            table: "Servers");

        migrationBuilder.DropColumn(
            name: "SnapshotId",
            table: "Servers");

        migrationBuilder.DropColumn(
            name: "Tag",
            table: "Servers");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "DropletId",
            table: "Servers",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Region",
            table: "Servers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Resize",
            table: "Servers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Size",
            table: "Servers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "SnapshotId",
            table: "Servers",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Tag",
            table: "Servers",
            type: "text",
            nullable: true);
    }
}
