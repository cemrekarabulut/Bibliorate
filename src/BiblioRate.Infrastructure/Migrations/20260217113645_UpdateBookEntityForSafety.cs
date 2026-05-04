using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiblioRate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookEntityForSafety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleBookId",
                table: "Books",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleBookId",
                table: "Books");
        }
    }
}
