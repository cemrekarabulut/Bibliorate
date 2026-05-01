using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiblioRate.Infrastructure.Migrations
{
    public partial class UpdateSchemaForSeeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----------------------------------------------------------------
            // Adım 1: FK'ları VARSA sil (idempotent — önceki deploy'da zaten
            //          silinmiş olabilir, tekrar hata vermez)
            // ----------------------------------------------------------------
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS `__BiblioDropFK`");

            migrationBuilder.Sql(@"
CREATE PROCEDURE `__BiblioDropFK`(IN tbl VARCHAR(128), IN fk VARCHAR(128))
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE()
          AND TABLE_NAME = tbl
          AND CONSTRAINT_NAME = fk
          AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    ) THEN
        SET @q = CONCAT('ALTER TABLE `', tbl, '` DROP FOREIGN KEY `', fk, '`');
        PREPARE s FROM @q;
        EXECUTE s;
        DEALLOCATE PREPARE s;
    END IF;
END");

            migrationBuilder.Sql("CALL `__BiblioDropFK`('BookViews',  'FK_BookViews_Users_UserId')");
            migrationBuilder.Sql("CALL `__BiblioDropFK`('Favorites',  'FK_Favorites_Users_UserId')");
            migrationBuilder.Sql("CALL `__BiblioDropFK`('Ratings',    'FK_Ratings_Users_UserId')");
            migrationBuilder.Sql("CALL `__BiblioDropFK`('Reviews',    'FK_Reviews_Users_UserId')");
            migrationBuilder.Sql("CALL `__BiblioDropFK`('SearchLogs', 'FK_SearchLogs_Users_UserId')");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS `__BiblioDropFK`");

            // ----------------------------------------------------------------
            // Adım 2: Users tablosuna Id kolonu ekle + PK'yı güncelle
            //          (sadece Id kolonu yoksa çalışır — idempotent)
            // ----------------------------------------------------------------
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS `__BiblioFixPK`");

            migrationBuilder.Sql(@"
CREATE PROCEDURE `__BiblioFixPK`()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'Users'
          AND COLUMN_NAME = 'Id'
    ) THEN
        ALTER TABLE `Users` MODIFY COLUMN `UserId` int NOT NULL;
        ALTER TABLE `Users` DROP PRIMARY KEY;
        ALTER TABLE `Users` ADD COLUMN `Id` int NOT NULL AUTO_INCREMENT FIRST, ADD PRIMARY KEY (`Id`);
    END IF;
END");

            migrationBuilder.Sql("CALL `__BiblioFixPK`()");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS `__BiblioFixPK`");

            // ----------------------------------------------------------------
            // Adım 3: FK'ları YOKSA ekle (idempotent)
            // ----------------------------------------------------------------
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS `__BiblioAddFK`");

            migrationBuilder.Sql(@"
CREATE PROCEDURE `__BiblioAddFK`(IN tbl VARCHAR(128), IN fk VARCHAR(128), IN col VARCHAR(128), IN refCol VARCHAR(128), IN onDel VARCHAR(64))
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE()
          AND TABLE_NAME = tbl
          AND CONSTRAINT_NAME = fk
          AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    ) THEN
        IF onDel = '' THEN
            SET @q = CONCAT('ALTER TABLE `', tbl, '` ADD CONSTRAINT `', fk,
                            '` FOREIGN KEY (`', col, '`) REFERENCES `Users` (`', refCol, '`)');
        ELSE
            SET @q = CONCAT('ALTER TABLE `', tbl, '` ADD CONSTRAINT `', fk,
                            '` FOREIGN KEY (`', col, '`) REFERENCES `Users` (`', refCol, '`) ON DELETE ', onDel);
        END IF;
        PREPARE s FROM @q;
        EXECUTE s;
        DEALLOCATE PREPARE s;
    END IF;
END");

            migrationBuilder.Sql("CALL `__BiblioAddFK`('BookViews',  'FK_BookViews_Users_UserId',  'UserId', 'Id', '')");
            migrationBuilder.Sql("CALL `__BiblioAddFK`('Favorites',  'FK_Favorites_Users_UserId',  'UserId', 'Id', 'CASCADE')");
            migrationBuilder.Sql("CALL `__BiblioAddFK`('Ratings',    'FK_Ratings_Users_UserId',    'UserId', 'Id', 'CASCADE')");
            migrationBuilder.Sql("CALL `__BiblioAddFK`('Reviews',    'FK_Reviews_Users_UserId',    'UserId', 'Id', 'CASCADE')");
            migrationBuilder.Sql("CALL `__BiblioAddFK`('SearchLogs', 'FK_SearchLogs_Users_UserId', 'UserId', 'Id', '')");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS `__BiblioAddFK`");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Id", table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(name: "PK_Users", table: "Users", column: "UserId");
        }
    }
}