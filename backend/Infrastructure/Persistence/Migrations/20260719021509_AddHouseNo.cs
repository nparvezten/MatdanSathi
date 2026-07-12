using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatdanSathi.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HouseNo",
                table: "VoterProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseNo",
                table: "VoterProfiles");
        }
    }
}
