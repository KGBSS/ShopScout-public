using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixing_tables_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAdditives_Products_ProductId",
                table: "ProductAdditives");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAllergens_Products_ProductId",
                table: "ProductAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductLabels_Products_ProductId",
                table: "ProductLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPackaging_Products_ProductId",
                table: "ProductPackaging");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProductBrand_Brands_BrandsId",
                table: "ProductProductBrand");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProductCategory_Categories_CategoriesId",
                table: "ProductProductCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProductCountry_Countries_CountriesId",
                table: "ProductProductCountry");

            migrationBuilder.DropTable(
                name: "ProductIngredientAnalysis");

            migrationBuilder.DropTable(
                name: "ProductNutrientLevels");

            migrationBuilder.DropIndex(
                name: "IX_ProductLabels_ProductId",
                table: "ProductLabels");

            migrationBuilder.DropIndex(
                name: "IX_ProductAllergens_ProductId",
                table: "ProductAllergens");

            migrationBuilder.DropIndex(
                name: "IX_ProductAdditives_ProductId",
                table: "ProductAdditives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPackaging",
                table: "ProductPackaging");

            migrationBuilder.DropIndex(
                name: "IX_ProductPackaging_ProductId",
                table: "ProductPackaging");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Brands",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductLabels");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductAllergens");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductAdditives");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "ProductPackaging");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductPackaging");

            migrationBuilder.DropColumn(
                name: "Recycling",
                table: "ProductPackaging");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "ProductPackaging");

            migrationBuilder.RenameTable(
                name: "ProductPackaging",
                newName: "ProductPackagings");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "ProductCountries");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "ProductCategories");

            migrationBuilder.RenameTable(
                name: "Brands",
                newName: "ProductBrands");

            migrationBuilder.RenameColumn(
                name: "LabelTag",
                table: "ProductLabels",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AllergenTag",
                table: "ProductAllergens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AdditiveTag",
                table: "ProductAdditives",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "Fat",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackagingMaterialId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackagingPartId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Salt",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaturatedFat",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sugars",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "ProductPackagings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartId",
                table: "ProductPackagings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Recyclable",
                table: "ProductPackagings",
                type: "bit",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPackagings",
                table: "ProductPackagings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCountries",
                table: "ProductCountries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategories",
                table: "ProductCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBrands",
                table: "ProductBrands",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PackagingMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackagingParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductAdditive",
                columns: table => new
                {
                    AdditivesId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductAdditive", x => new { x.AdditivesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductProductAdditive_ProductAdditives_AdditivesId",
                        column: x => x.AdditivesId,
                        principalTable: "ProductAdditives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductAdditive_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductAllergen",
                columns: table => new
                {
                    AllergensId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductAllergen", x => new { x.AllergensId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductProductAllergen_ProductAllergens_AllergensId",
                        column: x => x.AllergensId,
                        principalTable: "ProductAllergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductAllergen_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductLabel",
                columns: table => new
                {
                    LabelsId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductLabel", x => new { x.LabelsId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductProductLabel_ProductLabels_LabelsId",
                        column: x => x.LabelsId,
                        principalTable: "ProductLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductLabel_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductPackaging",
                columns: table => new
                {
                    PackagingId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductPackaging", x => new { x.PackagingId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductProductPackaging_ProductPackagings_PackagingId",
                        column: x => x.PackagingId,
                        principalTable: "ProductPackagings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductPackaging_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductAttribute",
                columns: table => new
                {
                    AttributesId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductAttribute", x => new { x.AttributesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductProductAttribute_ProductAttributes_AttributesId",
                        column: x => x.AttributesId,
                        principalTable: "ProductAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductAttribute_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_PackagingMaterialId",
                table: "Products",
                column: "PackagingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PackagingPartId",
                table: "Products",
                column: "PackagingPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackagings_MaterialId",
                table: "ProductPackagings",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackagings_PartId",
                table: "ProductPackagings",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductAdditive_ProductsId",
                table: "ProductProductAdditive",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductAllergen_ProductsId",
                table: "ProductProductAllergen",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductAttribute_ProductsId",
                table: "ProductProductAttribute",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductLabel_ProductsId",
                table: "ProductProductLabel",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductPackaging_ProductsId",
                table: "ProductProductPackaging",
                column: "ProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPackagings_PackagingMaterials_MaterialId",
                table: "ProductPackagings",
                column: "MaterialId",
                principalTable: "PackagingMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPackagings_PackagingParts_PartId",
                table: "ProductPackagings",
                column: "PartId",
                principalTable: "PackagingParts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProductBrand_ProductBrands_BrandsId",
                table: "ProductProductBrand",
                column: "BrandsId",
                principalTable: "ProductBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProductCategory_ProductCategories_CategoriesId",
                table: "ProductProductCategory",
                column: "CategoriesId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProductCountry_ProductCountries_CountriesId",
                table: "ProductProductCountry",
                column: "CountriesId",
                principalTable: "ProductCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PackagingMaterials_PackagingMaterialId",
                table: "Products",
                column: "PackagingMaterialId",
                principalTable: "PackagingMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PackagingParts_PackagingPartId",
                table: "Products",
                column: "PackagingPartId",
                principalTable: "PackagingParts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPackagings_PackagingMaterials_MaterialId",
                table: "ProductPackagings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPackagings_PackagingParts_PartId",
                table: "ProductPackagings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProductBrand_ProductBrands_BrandsId",
                table: "ProductProductBrand");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProductCategory_ProductCategories_CategoriesId",
                table: "ProductProductCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProductCountry_ProductCountries_CountriesId",
                table: "ProductProductCountry");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PackagingMaterials_PackagingMaterialId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PackagingParts_PackagingPartId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "PackagingMaterials");

            migrationBuilder.DropTable(
                name: "PackagingParts");

            migrationBuilder.DropTable(
                name: "ProductProductAdditive");

            migrationBuilder.DropTable(
                name: "ProductProductAllergen");

            migrationBuilder.DropTable(
                name: "ProductProductAttribute");

            migrationBuilder.DropTable(
                name: "ProductProductLabel");

            migrationBuilder.DropTable(
                name: "ProductProductPackaging");

            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.DropIndex(
                name: "IX_Products_PackagingMaterialId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PackagingPartId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPackagings",
                table: "ProductPackagings");

            migrationBuilder.DropIndex(
                name: "IX_ProductPackagings_MaterialId",
                table: "ProductPackagings");

            migrationBuilder.DropIndex(
                name: "IX_ProductPackagings_PartId",
                table: "ProductPackagings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCountries",
                table: "ProductCountries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategories",
                table: "ProductCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBrands",
                table: "ProductBrands");

            migrationBuilder.DropColumn(
                name: "Fat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackagingMaterialId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackagingPartId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaturatedFat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sugars",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "ProductPackagings");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "ProductPackagings");

            migrationBuilder.DropColumn(
                name: "Recyclable",
                table: "ProductPackagings");

            migrationBuilder.RenameTable(
                name: "ProductPackagings",
                newName: "ProductPackaging");

            migrationBuilder.RenameTable(
                name: "ProductCountries",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "ProductCategories",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "ProductBrands",
                newName: "Brands");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProductLabels",
                newName: "LabelTag");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProductAllergens",
                newName: "AllergenTag");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProductAdditives",
                newName: "AdditiveTag");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductLabels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductAllergens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductAdditives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "ProductPackaging",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductPackaging",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Recycling",
                table: "ProductPackaging",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                table: "ProductPackaging",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPackaging",
                table: "ProductPackaging",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Brands",
                table: "Brands",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductIngredientAnalysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AnalysisTag = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductIngredientAnalysis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductIngredientAnalysis_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductNutrientLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NutrientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductNutrientLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductNutrientLevels_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductLabels_ProductId",
                table: "ProductLabels",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAllergens_ProductId",
                table: "ProductAllergens",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAdditives_ProductId",
                table: "ProductAdditives",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackaging_ProductId",
                table: "ProductPackaging",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductIngredientAnalysis_ProductId",
                table: "ProductIngredientAnalysis",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductNutrientLevels_ProductId",
                table: "ProductNutrientLevels",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAdditives_Products_ProductId",
                table: "ProductAdditives",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAllergens_Products_ProductId",
                table: "ProductAllergens",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLabels_Products_ProductId",
                table: "ProductLabels",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPackaging_Products_ProductId",
                table: "ProductPackaging",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProductBrand_Brands_BrandsId",
                table: "ProductProductBrand",
                column: "BrandsId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProductCategory_Categories_CategoriesId",
                table: "ProductProductCategory",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProductCountry_Countries_CountriesId",
                table: "ProductProductCountry",
                column: "CountriesId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
