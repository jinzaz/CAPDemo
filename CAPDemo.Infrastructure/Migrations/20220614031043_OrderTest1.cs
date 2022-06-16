using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPDemo.Infrastructure.Migrations
{
    public partial class OrderTest1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityGuid1",
                schema: "ordering",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Name1",
                schema: "ordering",
                table: "users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityGuid1",
                schema: "ordering",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name1",
                schema: "ordering",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
