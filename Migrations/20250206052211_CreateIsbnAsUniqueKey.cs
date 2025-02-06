using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksEcommerce.Migrations
{
    /// <inheritdoc />
    public partial class CreateIsbnAsUniqueKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_books_ISBN",
                table: "books",
                column: "ISBN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_books_ISBN",
                table: "books");
        }
    }
}
