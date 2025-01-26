using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcProject_dotnet8.Data.Migrations
{
    /// <inheritdoc />
    public partial class Order_Test_Update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderCount",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCount",
                table: "Orders");
        }
    }
}
