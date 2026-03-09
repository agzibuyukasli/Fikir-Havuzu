using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FikirHavuzu.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToIdea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Ideas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Ideas");
        }
    }
}
