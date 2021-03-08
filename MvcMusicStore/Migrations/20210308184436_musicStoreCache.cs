using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MvcMusicStoreCore.Migrations
{
    public partial class musicStoreCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicStoreCache",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(449)", maxLength: 449, nullable: false),
                    Value = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ExpiresAtTime = table.Column<DateTimeOffset>(nullable: false),
                    SlidingExpirationInSeconds = table.Column<long>(nullable: true),
                    AbsoluteExpiration = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicStoreCache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicStoreCache_ExpiresAtTime",
                table: "MusicStoreCache",
                column: "ExpiresAtTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicStoreCache");
        }
    }
}
