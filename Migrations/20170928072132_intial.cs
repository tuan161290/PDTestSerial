using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PDTestSerial.Migrations
{
    public partial class intial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JigModels",
                columns: table => new
                {
                    JigModelID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Channel = table.Column<int>(nullable: false),
                    IsSetInJig = table.Column<bool>(nullable: false),
                    JigDesciption = table.Column<string>(nullable: true),
                    JigID = table.Column<int>(nullable: false),
                    JigPos = table.Column<int>(nullable: false),
                    JigTestResult = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JigModels", x => x.JigModelID);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionID = table.Column<uint>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PosDescription = table.Column<string>(nullable: true),
                    PositionValue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionID);
                });

            migrationBuilder.CreateTable(
                name: "SWSettings",
                columns: table => new
                {
                    SWSettingID = table.Column<uint>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LOADTest = table.Column<bool>(nullable: false),
                    PDCTest = table.Column<bool>(nullable: false),
                    SBUTest = table.Column<bool>(nullable: false),
                    UD3Test = table.Column<bool>(nullable: false),
                    UO2Test = table.Column<bool>(nullable: false),
                    UO3Test = table.Column<bool>(nullable: false),
                    VCONNTest = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SWSettings", x => x.SWSettingID);
                });

            migrationBuilder.CreateTable(
                name: "ValueSettings",
                columns: table => new
                {
                    ValueSettingID = table.Column<uint>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueSettings", x => x.ValueSettingID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JigModels");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "SWSettings");

            migrationBuilder.DropTable(
                name: "ValueSettings");
        }
    }
}
