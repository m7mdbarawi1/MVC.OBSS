using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OBSS.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserTypeIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_TypeDesc",
                table: "UserTypes",
                column: "TypeDesc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTypes_TypeDesc",
                table: "UserTypes");
        }
    }
}
