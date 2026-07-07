using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenericCrud.Migrations
{
    /// <inheritdoc />
    public partial class inti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryKeyColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    SoftDeleteColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DefaultSortColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DefaultSortDirection = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    PageCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PageSize = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    StatusesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusesID);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFieldSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicEntityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFieldSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFieldSections_DynamicEntities_DynamicEntityId",
                        column: x => x.DynamicEntityId,
                        principalTable: "DynamicEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicEntityId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InputType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FormOrder = table.Column<int>(type: "int", nullable: false),
                    TableOrder = table.Column<int>(type: "int", nullable: false),
                    ShowInForm = table.Column<bool>(type: "bit", nullable: false),
                    ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DynamicFieldSectionId = table.Column<int>(type: "int", nullable: true),
                    ConditionalOnFieldName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ConditionalOnValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFields_DynamicEntities_DynamicEntityId",
                        column: x => x.DynamicEntityId,
                        principalTable: "DynamicEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFields_DynamicFieldSections_DynamicFieldSectionId",
                        column: x => x.DynamicFieldSectionId,
                        principalTable: "DynamicFieldSections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DynamicFieldOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicFieldId = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFieldOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFieldOptions_DynamicFields_DynamicFieldId",
                        column: x => x.DynamicFieldId,
                        principalTable: "DynamicFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFieldValidations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicFieldId = table.Column<int>(type: "int", nullable: false),
                    MinLength = table.Column<int>(type: "int", nullable: true),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Pattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxFileSizeKb = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFieldValidations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFieldValidations_DynamicFields_DynamicFieldId",
                        column: x => x.DynamicFieldId,
                        principalTable: "DynamicFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFileConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicFieldId = table.Column<int>(type: "int", nullable: false),
                    SaveFolder = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AllowedExtensions = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MaxSizeKb = table.Column<int>(type: "int", nullable: false),
                    RenameToGuid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFileConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFileConfigs_DynamicFields_DynamicFieldId",
                        column: x => x.DynamicFieldId,
                        principalTable: "DynamicFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicForeignKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicFieldId = table.Column<int>(type: "int", nullable: false),
                    ForeignTableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ValueColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TextColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrderByColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicForeignKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicForeignKeys_DynamicFields_DynamicFieldId",
                        column: x => x.DynamicFieldId,
                        principalTable: "DynamicFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicEntities_EntityName",
                table: "DynamicEntities",
                column: "EntityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldOptions_DynamicFieldId",
                table: "DynamicFieldOptions",
                column: "DynamicFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_DynamicEntityId_FieldName",
                table: "DynamicFields",
                columns: new[] { "DynamicEntityId", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_DynamicFieldSectionId",
                table: "DynamicFields",
                column: "DynamicFieldSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldSections_DynamicEntityId",
                table: "DynamicFieldSections",
                column: "DynamicEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldValidations_DynamicFieldId",
                table: "DynamicFieldValidations",
                column: "DynamicFieldId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFileConfigs_DynamicFieldId",
                table: "DynamicFileConfigs",
                column: "DynamicFieldId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicForeignKeys_DynamicFieldId",
                table: "DynamicForeignKeys",
                column: "DynamicFieldId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicFieldOptions");

            migrationBuilder.DropTable(
                name: "DynamicFieldValidations");

            migrationBuilder.DropTable(
                name: "DynamicFileConfigs");

            migrationBuilder.DropTable(
                name: "DynamicForeignKeys");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "DynamicFields");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "DynamicFieldSections");

            migrationBuilder.DropTable(
                name: "DynamicEntities");
        }
    }
}
