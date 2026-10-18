using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyVip.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileHeardAboutUs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeardAboutUs",
                table: "Profiles",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeardAboutUs",
                table: "Profiles");
        }
    }
}
