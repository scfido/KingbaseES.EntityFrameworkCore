using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kingbase_demo.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kdb_Blog_Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ids = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Sex = table.Column<bool>(type: "boolean", nullable: false),
                    Sexy = table.Column<bool>(type: "boolean", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Ager = table.Column<int>(type: "integer", nullable: true),
                    Birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Birthy = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Money = table.Column<float>(type: "real", nullable: false),
                    Moneies = table.Column<float>(type: "real", nullable: true),
                    Pi = table.Column<double>(type: "double precision", nullable: false),
                    Pis = table.Column<double>(type: "double precision", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
                    States = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kdb_Blog_Tests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kdb_Blog_Tests");
        }
    }
}
