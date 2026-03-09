using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FikirHavuzu.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToIdea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Ideas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Ideas");
        }
    }
}
