using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiblioRate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityScoreToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QualityScore",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "Books");
        }
    }
}
