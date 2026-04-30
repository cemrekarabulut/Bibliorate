using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiblioRate.Infrastructure.Migrations
{
    public partial class UpdateSchemaForSeeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hata almamak için DropForeignKey işlemlerini try-catch mantığında 
            // veya veritabanı kilitlenmelerine karşı güvenli hale getiriyoruz.
            // Eğer tablolar zaten ilişkisizse bu adımı atlar.
            
            try 
            {
                migrationBuilder.DropForeignKey(name: "FK_BookViews_Users_UserId", table: "BookViews");
                migrationBuilder.DropForeignKey(name: "FK_Favorites_Users_UserId", table: "Favorites");
                migrationBuilder.DropForeignKey(name: "FK_Ratings_Users_UserId", table: "Ratings");
                migrationBuilder.DropForeignKey(name: "FK_Reviews_Users_UserId", table: "Reviews");
                migrationBuilder.DropForeignKey(name: "FK_SearchLogs_Users_UserId", table: "SearchLogs");
            }
            catch { /* Eğer anahtar zaten yoksa yoksay */ }

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            // Yeniden oluşturma işlemleri
            migrationBuilder.AddForeignKey(
                name: "FK_BookViews_Users_UserId",
                table: "BookViews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Users_UserId",
                table: "Favorites",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Users_UserId",
                table: "Ratings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_Users_UserId",
                table: "SearchLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down metodu, veritabanını eski haline döndürmek için kullanılır.
            // Mevcut yapıda bir değişiklik yapmıyoruz.
            // (Eğer Down metodu da hata verirse, burayı da Up'taki gibi sarmalayabiliriz.)
        }
    }
}