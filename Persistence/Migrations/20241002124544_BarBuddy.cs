using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BarBuddy : Migration
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
                    BarId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinkCategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DrinkName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<double>(type: "double", nullable: false),
                    Image = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drink", x => x.DrinkId);
                    table.ForeignKey(
                        name: "FK_Drink_Bar_BarId",
                        column: x => x.BarId,
                        principalTable: "Bar",
                        principalColumn: "BarId",
                        onDelete: ReferentialAction.Cascade);
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
                    { new Guid("0ba113f9-1c3e-4e12-a725-5be58af28e46"), "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1", "Bar Buddy 2", "Bar Buddy được thiết kế với lối kiến trúc cổ điển, lấy cảm hứng từ phong cách Tây Ban Nha với vẻ đẹp hoài cổ, độc đáo. Quán được xây dựng với những bức tường gạch thô gai góc, dùng ánh sáng màu đỏ và vàng rất huyền ảo. Không giống với những quán bar quận 1 khác thường thuê DJ chơi nhạc thì Carmen Bar lại thuê ban nhạc sống với những bài cực chill.", 0.0, "booking2@barbuddy.com", new TimeSpan(0, 18, 0, 0, 0), "barbuddy2.png", "0907770777", new TimeSpan(0, 2, 0, 0, 0), true },
                    { new Guid("0e234b28-0409-461d-a5e8-24fa690ed5c9"), "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1", "Bar Buddy 2", "Bar Buddy được thiết kế với lối kiến trúc cổ điển, lấy cảm hứng từ phong cách Tây Ban Nha với vẻ đẹp hoài cổ, độc đáo. Quán được xây dựng với những bức tường gạch thô gai góc, dùng ánh sáng màu đỏ và vàng rất huyền ảo. Không giống với những quán bar quận 1 khác thường thuê DJ chơi nhạc thì Carmen Bar lại thuê ban nhạc sống với những bài cực chill.", 0.0, "booking2@barbuddy.com", new TimeSpan(0, 18, 0, 0, 0), "barbuddy3.png", "0906660666", new TimeSpan(0, 2, 0, 0, 0), true },
                    { new Guid("437ec242-8bec-44e0-9ff7-faca2be0a3b0"), "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1", "Bar Buddy 1", "Bar Buddy được thiết kế với lối kiến trúc cổ điển, lấy cảm hứng từ phong cách Tây Ban Nha với vẻ đẹp hoài cổ, độc đáo. Quán được xây dựng với những bức tường gạch thô gai góc, dùng ánh sáng màu đỏ và vàng rất huyền ảo. Không giống với những quán bar quận 1 khác thường thuê DJ chơi nhạc thì Carmen Bar lại thuê ban nhạc sống với những bài cực chill.", 0.0, "booking1@barbuddy.com", new TimeSpan(0, 18, 0, 0, 0), "barbuddy1.png", "0908880888", new TimeSpan(0, 2, 0, 0, 0), true }
                });

            migrationBuilder.InsertData(
                table: "DrinkCategory",
                columns: new[] { "DrinksCategoryId", "Description", "DrinksCategoryName", "IsDrinkCategory" },
                values: new object[,]
                {
                    { new Guid("3ed8536c-2c08-4885-a40f-d2ddab043213"), "Đồ uống nóng hoặc lạnh được pha từ lá trà, có nhiều loại như trà đen, trà xanh và trà thảo mộc.", "Trà", true },
                    { new Guid("58fb5e54-572c-467a-9eed-671d128466d3"), "Đồ uống pha trộn thường chứa cồn, kết hợp với nước trái cây, soda hoặc các nguyên liệu khác.", "Cocktail", true },
                    { new Guid("832ae040-99a1-4085-9223-3adb19cc1e10"), "Phiên bản không cồn của các loại cocktail, phù hợp cho những người không uống rượu.", "Mocktail", true },
                    { new Guid("838b20cb-59e2-4cea-8697-9175d65e073b"), "Đồ uống có cồn được làm từ nho lên men, có nhiều loại như vang đỏ, vang trắng và vang hồng.", "Rượu vang", true },
                    { new Guid("b66227e3-5ed5-4cfc-bdc4-f4cf3111518f"), "Đồ uống có cồn được ủ từ lúa mạch, hoa bia và nước. Có nhiều loại khác nhau như lager, ale, stout.", "Bia", true },
                    { new Guid("b7323bd5-5916-4b7e-8a97-5640ccc51d6a"), "Đồ uống không cồn như soda, nước ngọt có ga, và nước ngọt có hương vị.", "Nước ngọt", true },
                    { new Guid("bcefd6f6-6920-4b3b-ab96-b65e888501bb"), "Đồ uống tự nhiên được làm từ nước ép trái cây hoặc rau củ. Các loại phổ biến gồm nước cam, nước táo, và nước ép cà rốt.", "Nước ép", true },
                    { new Guid("dd73a16a-1b65-4a16-b362-a0684871c441"), "Đồ uống nóng hoặc lạnh được pha từ hạt cà phê rang, bao gồm espresso, cappuccino, latte và nhiều loại khác.", "Cà phê", true },
                    { new Guid("df2748ed-9505-4c53-a051-832a58178fcb"), "Đồ uống có cồn mạnh như vodka, whisky, gin, rum, v.v.", "Rượu mạnh", true }
                });

            migrationBuilder.InsertData(
                table: "EmotionalDrinkCategory",
                columns: new[] { "EmotionalDrinksCategoryId", "CategoryName" },
                values: new object[,]
                {
                    { new Guid("02400312-0f91-4f36-8bca-dfe2f6dd3474"), "vui" },
                    { new Guid("62c31517-a522-4a68-bb49-149d8d513849"), "hạnh phúc" },
                    { new Guid("acc778d4-6731-4112-8350-e91028e2a5f5"), "đang yêu" },
                    { new Guid("ceb54398-3b7b-4880-9f63-4803e5392b1d"), "chán nản" },
                    { new Guid("daa72b40-943f-4ff5-a04a-7c0540427148"), "buồn" },
                    { new Guid("db4db77f-4c8d-43a9-b8ee-6d2784a88990"), "tức giận" }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { new Guid("5809b817-cf77-4fb4-af1c-58d2e64ef440"), "ADMIN" },
                    { new Guid("90e6600e-4ed1-4069-8a59-2a441961d346"), "STAFF" },
                    { new Guid("e47efe76-b3a0-4eff-bc7d-49f86bab88cd"), "CUSTOMER" }
                });

            migrationBuilder.InsertData(
                table: "TableType",
                columns: new[] { "TableTypeId", "Description", "IsDeleted", "MaximumGuest", "MinimumGuest", "MinimumPrice", "TypeName" },
                values: new object[,]
                {
                    { new Guid("06a34a09-3316-4bae-be29-94affd7dc66b"), "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.", false, 1, 1, 100000.0, "Bàn Quầy Bar" },
                    { new Guid("62598865-2fb3-4e0e-bc46-f4b831cbbf9a"), "Bàn SVIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao nhất tại quán, phù hợp cho nhóm khách hàng từ 1-15 người, mức giá tối thiểu chỉ từ 10.000.000 VND.", false, 15, 1, 10000000.0, "Bàn SVIP" },
                    { new Guid("7dfc0d07-dd60-483b-96ff-146e69f596e6"), "Bàn Tiêu chuẩn 2 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 4-6 người, mức giá tối thiểu chỉ từ 500.000 VND.", false, 6, 4, 500000.0, "Bàn Tiêu chuẩn 2" },
                    { new Guid("949e2731-f7a0-4134-884e-9da99ae097f4"), "Bàn Tiêu chuẩn 1 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 1-4 người, mức giá tối thiểu chỉ từ 200.000 VND.", false, 4, 1, 200000.0, "Bàn Tiêu chuẩn 1" },
                    { new Guid("d5a1df46-a232-47cf-aa30-2894c5069e43"), "Bàn VIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao tại quán, phù hợp cho nhóm khách hàng từ 1-10 người, mức giá tối thiểu chỉ từ 5.000.000 VND.", false, 10, 1, 5000000.0, "Bàn VIP" }
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
                name: "IX_Drink_BarId",
                table: "Drink",
                column: "BarId");

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
