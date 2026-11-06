using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSales.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "People",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Cpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_People", person => person.Id));

        migrationBuilder.CreateTable(
            name: "Vehicles",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Plate = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                ManufactureYear = table.Column<int>(nullable: false),
                Brand = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PersonId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Vehicles", vehicle => vehicle.Id);
                table.ForeignKey("FK_Vehicles_People_PersonId", vehicle => vehicle.PersonId,
                    "People", "Id", onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateIndex("IX_People_Cpf", "People", "Cpf", unique: true);
        migrationBuilder.CreateIndex("IX_Vehicles_Plate", "Vehicles", "Plate", unique: true);
        migrationBuilder.CreateIndex("IX_Vehicles_PersonId", "Vehicles", "PersonId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Vehicles");
        migrationBuilder.DropTable("People");
    }
}
