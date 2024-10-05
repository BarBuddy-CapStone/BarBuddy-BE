using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LouisKien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bar",
                columns: table => new
                {
                    BarId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BarName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Images = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Discount = table.Column<double>(type: "double", nullable: false),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bar", x => x.BarId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DrinkCategory",
                columns: table => new
                {
                    DrinksCategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinksCategoryName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDrinkCategory = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkCategory", x => x.DrinksCategoryId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmotionalDrinkCategory",
                columns: table => new
                {
                    EmotionalDrinksCategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CategoryName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmotionalDrinkCategory", x => x.EmotionalDrinksCategoryId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoleName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TableType",
                columns: table => new
                {
                    TableTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TypeName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinimumGuest = table.Column<int>(type: "int", nullable: false),
                    MaximumGuest = table.Column<int>(type: "int", nullable: false),
                    MinimumPrice = table.Column<double>(type: "double", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableType", x => x.TableTypeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Drink",
                columns: table => new
                {
                    DrinkId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinkCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DrinkCategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinkName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<double>(type: "double", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Image = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drink", x => x.DrinkId);
                    table.ForeignKey(
                        name: "FK_Drink_DrinkCategory_DrinkCategoryId",
                        column: x => x.DrinkCategoryId,
                        principalTable: "DrinkCategory",
                        principalColumn: "DrinksCategoryId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BarId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fullname = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dob = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Image = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Account_Bar_BarId",
                        column: x => x.BarId,
                        principalTable: "Bar",
                        principalColumn: "BarId");
                    table.ForeignKey(
                        name: "FK_Account_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Table",
                columns: table => new
                {
                    TableId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BarId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TableTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TableName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Table", x => x.TableId);
                    table.ForeignKey(
                        name: "FK_Table_Bar_BarId",
                        column: x => x.BarId,
                        principalTable: "Bar",
                        principalColumn: "BarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Table_TableType_TableTypeId",
                        column: x => x.TableTypeId,
                        principalTable: "TableType",
                        principalColumn: "TableTypeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DrinkEmotionalCategory",
                columns: table => new
                {
                    DrinkEmotionalCategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmotionalDrinkCategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinkId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkEmotionalCategory", x => x.DrinkEmotionalCategoryId);
                    table.ForeignKey(
                        name: "FK_DrinkEmotionalCategory_Drink_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drink",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkEmotionalCategory_EmotionalDrinkCategory_EmotionalDrink~",
                        column: x => x.EmotionalDrinkCategoryId,
                        principalTable: "EmotionalDrinkCategory",
                        principalColumn: "EmotionalDrinksCategoryId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    BookingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BarId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookingCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookingDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    BookingTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsIncludeDrink = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Booking_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Booking_Bar_BarId",
                        column: x => x.BarId,
                        principalTable: "Bar",
                        principalColumn: "BarId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingDrink",
                columns: table => new
                {
                    BookingDrinkId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinkId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActualPrice = table.Column<double>(type: "double", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDrink", x => x.BookingDrinkId);
                    table.ForeignKey(
                        name: "FK_BookingDrink_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingDrink_Drink_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drink",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingTable",
                columns: table => new
                {
                    BookingTableId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TableId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReservationDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ReservationTime = table.Column<TimeSpan>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTable", x => x.BookingTableId);
                    table.ForeignKey(
                        name: "FK_BookingTable_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingTable_Table_TableId",
                        column: x => x.TableId,
                        principalTable: "Table",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BarId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedback_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_Bar_BarId",
                        column: x => x.BarId,
                        principalTable: "Bar",
                        principalColumn: "BarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaymentHistory",
                columns: table => new
                {
                    PaymentHistoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProviderName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaymentFee = table.Column<double>(type: "double", nullable: false),
                    TotalPrice = table.Column<double>(type: "double", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistory", x => x.PaymentHistoryId);
                    table.ForeignKey(
                        name: "FK_PaymentHistory_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentHistory_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Bar",
                columns: new[] { "BarId", "Address", "BarName", "Description", "Discount", "Email", "EndTime", "Images", "PhoneNumber", "StartTime", "Status" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440000"), "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1", "Bar Buddy 1", "Nơi lý tưởng để thư giãn và tận hưởng âm nhạc.", 10.0, "contact@barbuddy1.com", new TimeSpan(0, 2, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/a0d4292c13b0cc51b2487f4c276cd7c0d96510872c4a855db190ff2db8e692d2?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234567", new TimeSpan(0, 18, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), "153 Tôn Thất Đạm, Bến Nghé, quận 1, Hồ Chí Minh", "Bar Buddy 2", "Quán bar phong cách trẻ trung với nhiều sự kiện thú vị.", 15.0, "contact@barbuddy2.com", new TimeSpan(0, 3, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234568", new TimeSpan(0, 17, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), "264 Đ. Nam Kỳ Khởi Nghĩa, Phường 8, Quận 3", "Bar Buddy 3", "Không gian sang trọng và dịch vụ tận tâm.", 20.0, "contact@barbuddy3.com", new TimeSpan(0, 1, 0, 0, 0), "https://vietnamnightlife.com/uploads/images/2020/02/1580805657-multi_product20-bambamoverview1.jpg.webp", "0901234569", new TimeSpan(0, 19, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440003"), "3C Đ. Tôn Đức Thắng, Bến Nghé, Quận 1, Thành phố Hồ Chí Minh", "Bar Buddy 4", "Chuyên phục vụ cocktail và đồ uống cao cấp.", 25.0, "contact@barbuddy4.com", new TimeSpan(0, 4, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/4f4bc5cae670ae75847bb24a78027e45ce8487386c0a1043f999381ae9fa4831?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234570", new TimeSpan(0, 20, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440004"), "11 Đ.Nam Quốc Cang, Phường Phạm Ngũ Lão, Quận 1", "Bar Buddy 5", "Quán bar kết hợp giữa nhạc sống và DJ.", 5.0, "contact@barbuddy5.com", new TimeSpan(0, 2, 30, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/fc1f4652930fe4a25d46a46d1933e950912b6ceace8e777840ceccd123995783?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234571", new TimeSpan(0, 18, 30, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440005"), "41 Nam Kỳ Khởi Nghĩa, Phường Nguyễn Thái Bình, Quận 1, Hồ Chí Minh", "Bar Buddy 6", "Không gian thoải mái với nhiều trò chơi giải trí.", 10.0, "contact@barbuddy6.com", new TimeSpan(0, 3, 30, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/677e2c38ccd2ea07e8a72aa6262c873572a4cfd3da719a1e25c2152169bb47c6?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234572", new TimeSpan(0, 17, 30, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440006"), "20 Đ. Nguyễn Công Trứ, Phường Nguyễn Thái Bình, Quận 1", "Bar Buddy 7", "Nơi hội tụ của những tâm hồn yêu thích âm nhạc.", 30.0, "contact@barbuddy7.com", new TimeSpan(0, 1, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/2f3601dbe8c6d0a812bccaf7ecf02686ec5b99038e314c058a00a37c16840608?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234573", new TimeSpan(0, 19, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440007"), "120 Đ. Nguyễn Huệ, Bến Nghé, Quận 1", "Bar Buddy 8", "Quán bar rooftop với tầm nhìn đẹp.", 20.0, "contact@barbuddy8.com", new TimeSpan(0, 2, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234574", new TimeSpan(0, 17, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440008"), "30 Đ. Tôn Thất Tùng, Quận 1", "Bar Buddy 9", "Quán bar dành cho các tín đồ yêu thích craft beer.", 15.0, "contact@barbuddy9.com", new TimeSpan(0, 3, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234575", new TimeSpan(0, 18, 0, 0, 0), true },
                    { new Guid("550e8400-e29b-41d4-a716-446655440009"), "25 Đ. Lê Duẩn, Quận 1", "Bar Buddy 10", "Không gian ấm cúng với các loại cocktail độc đáo.", 10.0, "contact@barbuddy10.com", new TimeSpan(0, 2, 0, 0, 0), "https://cdn.builder.io/api/v1/image/assets/TEMP/a0d4292c13b0cc51b2487f4c276cd7c0d96510872c4a855db190ff2db8e692d2?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b", "0901234576", new TimeSpan(0, 19, 0, 0, 0), true }
                });

            migrationBuilder.InsertData(
                table: "DrinkCategory",
                columns: new[] { "DrinksCategoryId", "Description", "DrinksCategoryName", "IsDrinkCategory" },
                values: new object[,]
                {
                    { new Guid("1133e20a-bc7d-4ab0-b5bb-18e8aec307a3"), "Đồ uống có cồn mạnh như vodka, whisky, gin, rum, v.v.", "Rượu mạnh", true },
                    { new Guid("351458aa-e933-4d52-b234-e69a00733548"), "Đồ uống nóng hoặc lạnh được pha từ hạt cà phê rang, bao gồm espresso, cappuccino, latte và nhiều loại khác.", "Cà phê", true },
                    { new Guid("50e55732-16cb-4989-8bf9-ba88f0726bd8"), "Đồ uống có cồn được làm từ nho lên men, có nhiều loại như vang đỏ, vang trắng và vang hồng.", "Rượu vang", true },
                    { new Guid("58f2969e-b80c-488d-88b7-e5c114913322"), "Đồ uống tự nhiên được làm từ nước ép trái cây hoặc rau củ. Các loại phổ biến gồm nước cam, nước táo, và nước ép cà rốt.", "Nước ép", true },
                    { new Guid("6b42c208-3d5e-4fed-afe4-19bf5a9659c3"), "Phiên bản không cồn của các loại cocktail, phù hợp cho những người không uống rượu.", "Mocktail", true },
                    { new Guid("7e811835-2c89-4a86-a0c7-322e7db3aba4"), "Đồ uống không cồn như soda, nước ngọt có ga, và nước ngọt có hương vị.", "Nước ngọt", true },
                    { new Guid("9670dde8-9ad9-4e28-ae79-f50e16d0604e"), "Đồ uống pha trộn thường chứa cồn, kết hợp với nước trái cây, soda hoặc các nguyên liệu khác.", "Cocktail", true },
                    { new Guid("96a86c01-ca27-4410-94d5-5ffaa3091d4d"), "Đồ uống nóng hoặc lạnh được pha từ lá trà, có nhiều loại như trà đen, trà xanh và trà thảo mộc.", "Trà", true },
                    { new Guid("d96f117a-aeb5-46c3-aa12-b0cb25627db3"), "Đồ uống có cồn được ủ từ lúa mạch, hoa bia và nước. Có nhiều loại khác nhau như lager, ale, stout.", "Bia", true }
                });

            migrationBuilder.InsertData(
                table: "EmotionalDrinkCategory",
                columns: new[] { "EmotionalDrinksCategoryId", "CategoryName" },
                values: new object[,]
                {
                    { new Guid("123f7333-5959-4f63-aa35-da79fe571dca"), "vui" },
                    { new Guid("8c3cb945-6f63-4ac8-afc1-1de8ad1df8b2"), "buồn" },
                    { new Guid("8e362786-148e-4775-90b9-ffe25bca53c8"), "đang yêu" },
                    { new Guid("c04e653e-fb8f-4f15-a651-fa28a085d97f"), "chán nản" },
                    { new Guid("c54e2200-879b-49d4-a073-2f3f524cabcc"), "tức giận" },
                    { new Guid("c9a791ba-14a9-4047-90e4-540b2ccd7007"), "hạnh phúc" }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { new Guid("70a545c0-6156-467c-a86f-547370ea4552"), "CUSTOMER" },
                    { new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), "STAFF" },
                    { new Guid("b3b5a546-519d-411b-89d0-20c824e18d11"), "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "TableType",
                columns: new[] { "TableTypeId", "Description", "IsDeleted", "MaximumGuest", "MinimumGuest", "MinimumPrice", "TypeName" },
                values: new object[,]
                {
                    { new Guid("40d5663b-1d9e-422f-b782-0f6eb73f5115"), "Bàn VIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao tại quán, phù hợp cho nhóm khách hàng từ 1-10 người, mức giá tối thiểu chỉ từ 5.000.000 VND.", false, 10, 1, 5000000.0, "Bàn VIP" },
                    { new Guid("4a8dd27d-77ec-446e-90f9-45df4e900683"), "Bàn Tiêu chuẩn 2 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 4-6 người, mức giá tối thiểu chỉ từ 500.000 VND.", false, 6, 4, 500000.0, "Bàn Tiêu chuẩn 2" },
                    { new Guid("78910745-96cc-4f05-8600-09047c23967d"), "Bàn Tiêu chuẩn 1 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 1-4 người, mức giá tối thiểu chỉ từ 200.000 VND.", false, 4, 1, 200000.0, "Bàn Tiêu chuẩn 1" },
                    { new Guid("93f03bbd-69d4-43e5-a95b-e215689b175a"), "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.", false, 1, 1, 100000.0, "Bàn Quầy Bar" },
                    { new Guid("f8b05440-b11f-48d6-877d-c14c6d1e6e23"), "Bàn SVIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao nhất tại quán, phù hợp cho nhóm khách hàng từ 1-15 người, mức giá tối thiểu chỉ từ 10.000.000 VND.", false, 15, 1, 10000000.0, "Bàn SVIP" }
                });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "AccountId", "BarId", "CreatedAt", "Dob", "Email", "Fullname", "Image", "Password", "Phone", "RoleId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-b777-446655440001"), new Guid("550e8400-e29b-41d4-a716-446655440000"), new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2195), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1980, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "admin1@barbuddy1.com", "Admin Bar Buddy1", "admin1.png", "password123", "0901234567", new Guid("b3b5a546-519d-411b-89d0-20c824e18d11"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2199), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440002"), new Guid("550e8400-e29b-41d4-a716-446655440001"), new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2206), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1992, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "staff1@barbuddy2.com", "Staff Bar Buddy2", "staff1.png", "password456", "0901234568", new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2206), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440003"), new Guid("550e8400-e29b-41d4-a716-446655440002"), new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2215), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1990, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "staff2@barbuddy3.com", "Staff Bar Buddy3", "staff2.png", "password789", "0901234569", new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2215), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440004"), null, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2219), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1985, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer1@barbuddy4.com", "Customer Bar Buddy4", "customer1.png", "password321", "0901234570", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2220), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440006"), null, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2224), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1993, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer2@barbuddy6.com", "Customer Bar Buddy6", "customer2.png", "password987", "0901234572", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2225), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440007"), new Guid("550e8400-e29b-41d4-a716-446655440006"), new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2230), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1987, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "staff3@barbuddy7.com", "Staff Bar Buddy7", "staff3.png", "password111", "0901234573", new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2232), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440009"), null, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2237), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1994, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer3@barbuddy9.com", "Customer Bar Buddy9", "customer3.png", "password333", "0901234575", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2238), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440010"), null, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2243), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1982, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer4@barbuddy10.com", "Customer Bar Buddy10", "customer4.png", "password444", "0901234576", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 5, 22, 7, 28, 387, DateTimeKind.Unspecified).AddTicks(2243), new TimeSpan(0, 7, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_BarId",
                table: "Account",
                column: "BarId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_RoleId",
                table: "Account",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_AccountId",
                table: "Booking",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_BarId",
                table: "Booking",
                column: "BarId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDrink_BookingId",
                table: "BookingDrink",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDrink_DrinkId",
                table: "BookingDrink",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTable_BookingId",
                table: "BookingTable",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTable_TableId",
                table: "BookingTable",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Drink_DrinkCategoryId",
                table: "Drink",
                column: "DrinkCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkEmotionalCategory_DrinkId",
                table: "DrinkEmotionalCategory",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkEmotionalCategory_EmotionalDrinkCategoryId",
                table: "DrinkEmotionalCategory",
                column: "EmotionalDrinkCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_AccountId",
                table: "Feedback",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_BarId",
                table: "Feedback",
                column: "BarId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_BookingId",
                table: "Feedback",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_AccountId",
                table: "PaymentHistory",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_BookingId",
                table: "PaymentHistory",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Table_BarId",
                table: "Table",
                column: "BarId");

            migrationBuilder.CreateIndex(
                name: "IX_Table_TableTypeId",
                table: "Table",
                column: "TableTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingDrink");

            migrationBuilder.DropTable(
                name: "BookingTable");

            migrationBuilder.DropTable(
                name: "DrinkEmotionalCategory");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "PaymentHistory");

            migrationBuilder.DropTable(
                name: "Table");

            migrationBuilder.DropTable(
                name: "Drink");

            migrationBuilder.DropTable(
                name: "EmotionalDrinkCategory");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "TableType");

            migrationBuilder.DropTable(
                name: "DrinkCategory");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Bar");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
