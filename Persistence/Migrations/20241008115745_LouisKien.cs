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
                    Comment = table.Column<string>(type: "longtext", nullable: true)
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
                    Status = table.Column<int>(type: "int", nullable: false)
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
                    { new Guid("5fc58d36-18dc-4f24-a4e6-2ec8969c51f6"), "Đồ uống có cồn mạnh như vodka, whisky, gin, rum, v.v.", "Rượu mạnh", true },
                    { new Guid("5fd662a6-10c4-4cda-97dd-dfde1603d8ec"), "Phiên bản không cồn của các loại cocktail, phù hợp cho những người không uống rượu.", "Mocktail", true },
                    { new Guid("6d8eafd8-fb10-47c5-9272-03ad4585fc6e"), "Đồ uống nóng hoặc lạnh được pha từ lá trà, có nhiều loại như trà đen, trà xanh và trà thảo mộc.", "Trà", true },
                    { new Guid("7a8bb8fd-f323-4a3d-b7c6-bfe0fc28f77a"), "Đồ uống không cồn như soda, nước ngọt có ga, và nước ngọt có hương vị.", "Nước ngọt", true },
                    { new Guid("91ff9f16-79b0-4fe3-a9c0-7b616c60e62c"), "Đồ uống có cồn được làm từ nho lên men, có nhiều loại như vang đỏ, vang trắng và vang hồng.", "Rượu vang", true },
                    { new Guid("9ffef6ce-d455-41ae-8548-e5b710d169c1"), "Đồ uống có cồn được ủ từ lúa mạch, hoa bia và nước. Có nhiều loại khác nhau như lager, ale, stout.", "Bia", true },
                    { new Guid("a17d366a-2f40-4d8f-a8d1-40a2eecd28ca"), "Đồ uống tự nhiên được làm từ nước ép trái cây hoặc rau củ. Các loại phổ biến gồm nước cam, nước táo, và nước ép cà rốt.", "Nước ép", true },
                    { new Guid("a9b2130d-2c27-4db1-87e5-892259c635ca"), "Đồ uống nóng hoặc lạnh được pha từ hạt cà phê rang, bao gồm espresso, cappuccino, latte và nhiều loại khác.", "Cà phê", true },
                    { new Guid("e1dbebd0-8140-4aa2-98f0-d154cca1e249"), "Đồ uống pha trộn thường chứa cồn, kết hợp với nước trái cây, soda hoặc các nguyên liệu khác.", "Cocktail", true }
                });

            migrationBuilder.InsertData(
                table: "EmotionalDrinkCategory",
                columns: new[] { "EmotionalDrinksCategoryId", "CategoryName" },
                values: new object[,]
                {
                    { new Guid("1f9562c1-8b23-4381-8d60-e0440ac87d43"), "chán nản" },
                    { new Guid("2c7117c4-0cb0-4ca1-bd1f-42644da62670"), "hạnh phúc" },
                    { new Guid("2e6a5ca5-2ca9-4444-97f7-0ef3c958e1f0"), "tức giận" },
                    { new Guid("302f5f86-deaf-4af1-a224-629b3a0cec21"), "đang yêu" },
                    { new Guid("a71aa92a-91ee-4fce-9fed-33169a9116d3"), "buồn" },
                    { new Guid("e7e8e0e5-b2fc-44b5-be85-7e0ceec369c2"), "vui" }
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
                    { new Guid("25385f1f-6a45-4426-a4f2-b40a3946abd3"), "Bàn Tiêu chuẩn 1 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 1-4 người, mức giá tối thiểu chỉ từ 200.000 VND.", false, 4, 1, 200000.0, "Bàn Tiêu chuẩn 1" },
                    { new Guid("34956623-47b9-41a2-b7fc-d949b57fa704"), "Bàn SVIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao nhất tại quán, phù hợp cho nhóm khách hàng từ 1-15 người, mức giá tối thiểu chỉ từ 10.000.000 VND.", false, 15, 1, 10000000.0, "Bàn SVIP" },
                    { new Guid("88aa24ed-028d-42af-8476-796d68736096"), "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.", false, 1, 1, 100000.0, "Bàn Quầy Bar" },
                    { new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a"), "Bàn VIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao tại quán, phù hợp cho nhóm khách hàng từ 1-10 người, mức giá tối thiểu chỉ từ 5.000.000 VND.", false, 10, 1, 5000000.0, "Bàn VIP" },
                    { new Guid("ea17dcb7-5e93-43b3-be82-828cc2615d8d"), "Bàn Tiêu chuẩn 2 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 4-6 người, mức giá tối thiểu chỉ từ 500.000 VND.", false, 6, 4, 500000.0, "Bàn Tiêu chuẩn 2" }
                });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "AccountId", "BarId", "CreatedAt", "Dob", "Email", "Fullname", "Image", "Password", "Phone", "RoleId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-b777-446655440001"), new Guid("550e8400-e29b-41d4-a716-446655440000"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2726), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1980, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "admin1@barbuddy1.com", "Admin Bar Buddy1", "admin1.png", "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87", "0901234567", new Guid("b3b5a546-519d-411b-89d0-20c824e18d11"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2730), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440002"), new Guid("550e8400-e29b-41d4-a716-446655440001"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2740), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1992, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "staff1@barbuddy2.com", "Staff Bar Buddy2", "staff1.png", "password456", "0901234568", new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2740), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440003"), new Guid("550e8400-e29b-41d4-a716-446655440002"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2747), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1990, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "staff2@barbuddy3.com", "Staff Bar Buddy3", "staff2.png", "password789", "0901234569", new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2747), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440004"), null, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2752), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1985, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer1@barbuddy4.com", "Customer Bar Buddy4", "customer1.png", "password321", "0901234570", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2753), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440006"), null, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2758), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1993, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer2@barbuddy6.com", "Customer Bar Buddy6", "customer2.png", "password987", "0901234572", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2759), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440007"), new Guid("550e8400-e29b-41d4-a716-446655440006"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2763), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1987, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "staff3@barbuddy7.com", "Staff Bar Buddy7", "staff3.png", "password111", "0901234573", new Guid("a3438270-b7ed-4222-b3d8-aee52fc58805"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2764), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440009"), null, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2768), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1994, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer3@barbuddy9.com", "Customer Bar Buddy9", "customer3.png", "password333", "0901234575", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2769), new TimeSpan(0, 7, 0, 0, 0)) },
                    { new Guid("550e8400-e29b-41d4-b777-446655440010"), null, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2773), new TimeSpan(0, 7, 0, 0, 0)), new DateTimeOffset(new DateTime(1982, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "customer4@barbuddy10.com", "Customer Bar Buddy10", "customer4.png", "password444", "0901234576", new Guid("70a545c0-6156-467c-a86f-547370ea4552"), 1, new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2773), new TimeSpan(0, 7, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "Drink",
                columns: new[] { "DrinkId", "CreatedDate", "Description", "DrinkCategoryId", "DrinkCode", "DrinkName", "Image", "Price", "Status", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("550d7300-f30c-30c3-b827-335544330000"), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2944), "Nước ngọt có ga phổ biến.", new Guid("7a8bb8fd-f323-4a3d-b7c6-bfe0fc28f77a"), "D0001", "Coca Cola", "https://www.coca-cola.com/content/dam/onexp/vn/home-image/coca-cola/Coca-Cola_OT%20320ml_VN-EX_Desktop.png", 15000.0, true, new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2944) },
                    { new Guid("550d7300-f30c-30c3-b827-335544330001"), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2949), "Cocktail nổi tiếng pha từ rượu rum và bạc hà.", new Guid("e1dbebd0-8140-4aa2-98f0-d154cca1e249"), "D0002", "Mojito", "https://www.liquor.com/thmb/MJRVqf-itJGY90nwUOYGXnyG-HA=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/mojito-720x720-primary-6a57f80e200c412e9a77a1687f312ff7.jpg", 70000.0, true, new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2950) },
                    { new Guid("550d7300-f30c-30c3-b827-335544330002"), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2953), "Trà xanh kết hợp với vị ngọt của đào.", new Guid("5fd662a6-10c4-4cda-97dd-dfde1603d8ec"), "D0003", "Trà Đào", "https://file.hstatic.net/200000684957/article/tra-dao_e022b1a9ac564ee186007875701ac643.jpg", 35000.0, true, new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2954) },
                    { new Guid("550d7300-f30c-30c3-b827-335544330003"), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2957), "Nước ngọt có ga, phổ biến tương tự Coca Cola.", new Guid("7a8bb8fd-f323-4a3d-b7c6-bfe0fc28f77a"), "D0004", "Pepsi", "http://thepizzacompany.vn/images/thumbs/000/0002364_pepsi-15l-pet_500.jpeg", 15000.0, true, new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2957) },
                    { new Guid("550d7300-f30c-30c3-b827-335544330004"), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2960), "Cocktail kết hợp giữa rượu vodka và nước cam.", new Guid("5fc58d36-18dc-4f24-a4e6-2ec8969c51f6"), "D0005", "Screwdriver", "https://www.liquor.com/thmb/RnOVWoIXp7OAJRS-NSDIF9Bglbc=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/LQR-screwdriver-original-4000x4000-edb2f56dd69146bba9f7fafbf69e00a0.jpg", 80000.0, true, new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2961) },
                    { new Guid("550d7300-f30c-30c3-b827-335544330005"), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2964), "Cà phê đen pha đậm, không đường, không sữa.", new Guid("a9b2130d-2c27-4db1-87e5-892259c635ca"), "D0006", "Cà phê đen", "https://suckhoedoisong.qltns.mediacdn.vn/324455921873985536/2024/8/14/2121767707dcce179f6866d132a2d6a384312f9-1723600454996-1723600455541950721311.jpg", 15000.0, true, new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2965) }
                });

            migrationBuilder.InsertData(
                table: "Table",
                columns: new[] { "TableId", "BarId", "IsDeleted", "Status", "TableName", "TableTypeId" },
                values: new object[,]
                {
                    { new Guid("660d7300-f30c-30c3-b827-335544330000"), new Guid("550e8400-e29b-41d4-a716-446655440000"), false, 0, "Table A1", new Guid("25385f1f-6a45-4426-a4f2-b40a3946abd3") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330001"), new Guid("550e8400-e29b-41d4-a716-446655440000"), false, 0, "Table B1", new Guid("88aa24ed-028d-42af-8476-796d68736096") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330002"), new Guid("550e8400-e29b-41d4-a716-446655440002"), false, 0, "Table C1", new Guid("ea17dcb7-5e93-43b3-be82-828cc2615d8d") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330003"), new Guid("550e8400-e29b-41d4-a716-446655440000"), false, 0, "Table A2", new Guid("34956623-47b9-41a2-b7fc-d949b57fa704") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330004"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330005"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330006"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330007"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330008"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330009"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330010"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") },
                    { new Guid("660d7300-f30c-30c3-b827-335544330011"), new Guid("550e8400-e29b-41d4-a716-446655440001"), false, 0, "Table B2", new Guid("98f0023d-7f14-4bc4-b8ac-8e29f5ff9e4a") }
                });

            migrationBuilder.InsertData(
                table: "Booking",
                columns: new[] { "BookingId", "AccountId", "BarId", "BookingCode", "BookingDate", "BookingTime", "CreateAt", "IsIncludeDrink", "Note", "Status" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440000"), new Guid("550e8400-e29b-41d4-b777-446655440001"), new Guid("550e8400-e29b-41d4-a716-446655440000"), "BB0001", new DateTimeOffset(new DateTime(2024, 10, 3, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2988), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2983), false, null, 1 },
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), new Guid("550e8400-e29b-41d4-b777-446655440002"), new Guid("550e8400-e29b-41d4-a716-446655440001"), "BB0002", new DateTimeOffset(new DateTime(2024, 10, 1, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(2999), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(2996), false, null, 2 },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), new Guid("550e8400-e29b-41d4-b777-446655440003"), new Guid("550e8400-e29b-41d4-a716-446655440002"), "BB0003", new DateTimeOffset(new DateTime(2024, 10, 5, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3004), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(3001), false, null, 1 },
                    { new Guid("550e8400-e29b-41d4-a716-446655440003"), new Guid("550e8400-e29b-41d4-b777-446655440004"), new Guid("550e8400-e29b-41d4-a716-446655440003"), "BB0004", new DateTimeOffset(new DateTime(2024, 10, 6, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3009), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Local).AddTicks(3007), false, null, 1 }
                });

            migrationBuilder.InsertData(
                table: "BookingTable",
                columns: new[] { "BookingTableId", "BookingId", "ReservationDate", "ReservationTime", "TableId" },
                values: new object[,]
                {
                    { new Guid("10bba50b-45f8-4b11-bf9b-7b7a5a3f5a9d"), new Guid("550e8400-e29b-41d4-a716-446655440002"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3034), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330002") },
                    { new Guid("543b332e-2ea8-4df5-8b9f-5df7246dfdb4"), new Guid("550e8400-e29b-41d4-a716-446655440003"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3041), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330006") },
                    { new Guid("6f3f337b-7261-49be-ba95-a31e8a2413ca"), new Guid("550e8400-e29b-41d4-a716-446655440003"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3047), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330008") },
                    { new Guid("b0728607-ecd5-4c56-97b8-b90385af6d37"), new Guid("550e8400-e29b-41d4-a716-446655440001"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3031), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330001") },
                    { new Guid("b61a63c3-24f1-4fa0-80ee-f20e0a27808d"), new Guid("550e8400-e29b-41d4-a716-446655440000"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3026), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330000") },
                    { new Guid("bdf6dfad-1fdf-49e7-9abe-0b8a0bb425a6"), new Guid("550e8400-e29b-41d4-a716-446655440003"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3037), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330005") },
                    { new Guid("ccd1cc0a-5204-4bb4-ab73-92627e59b03b"), new Guid("550e8400-e29b-41d4-a716-446655440003"), new DateTimeOffset(new DateTime(2024, 10, 8, 18, 57, 44, 513, DateTimeKind.Unspecified).AddTicks(3044), new TimeSpan(0, 7, 0, 0, 0)), new TimeSpan(0, 0, 0, 0, 0), new Guid("660d7300-f30c-30c3-b827-335544330007") }
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
