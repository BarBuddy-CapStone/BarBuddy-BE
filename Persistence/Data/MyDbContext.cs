using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext ()
        {
        }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles {  get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Bar> Bars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDrink> BookingDrinks { get; set; }
        public DbSet<BookingTable> BookingTables { get; set; }
        public DbSet<Drink> Drinks { get; set; }
        public DbSet<DrinkCategory> DrinksCategories { get; set; }
        public DbSet<DrinkEmotionalCategory> DrinksEmotionalCategories { get; set; }
        public DbSet<EmotionalDrinkCategory> EmotionalDrinksCategories { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<TableType> TableTypes { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                optionsBuilder.UseMySql(configuration.GetConnectionString("MyDB"), new MySqlServerVersion(new Version(8, 0, 23)));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Generate new GUIDs for each category
            // EmotionCategoryDrink
            var vuiId = Guid.NewGuid();
            var buonId = Guid.NewGuid();
            var hanhPhucId = Guid.NewGuid();
            var tucGianId = Guid.NewGuid();
            var chanNanId = Guid.NewGuid();
            var dangYeuId = Guid.NewGuid();

            // Seed data for EmotionalDrinkCategory
            modelBuilder.Entity<EmotionalDrinkCategory>().HasData(
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = vuiId,
                    CategoryName = "vui"
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = buonId,
                    CategoryName = "buồn"
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = hanhPhucId,
                    CategoryName = "hạnh phúc"
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = tucGianId,
                    CategoryName = "tức giận"
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = chanNanId,
                    CategoryName = "chán nản"
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = dangYeuId,
                    CategoryName = "đang yêu"
                }
            );


            // Role Data
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "ADMIN"
                },
                new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "STAFF"
                },
                new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "CUSTOMER"
                }
            );

            // Bar Data
            modelBuilder.Entity<Bar>().HasData(
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1",
                    BarName = "Bar Buddy 1",
                    Description = "Bar Buddy được thiết kế với lối kiến trúc cổ điển, lấy cảm hứng từ phong cách Tây Ban Nha với vẻ đẹp hoài cổ, độc đáo. Quán được xây dựng với những bức tường gạch thô gai góc, dùng ánh sáng màu đỏ và vàng rất huyền ảo. Không giống với những quán bar quận 1 khác thường thuê DJ chơi nhạc thì Carmen Bar lại thuê ban nhạc sống với những bài cực chill.",
                    Discount = 0,
                    Email = "booking1@barbuddy.com",
                    EndTime = new TimeSpan(18,00,00),
                    StartTime = new TimeSpan(2, 00, 00),
                    Images = "barbuddy1.png",
                    PhoneNumber = "0908880888",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1",
                    BarName = "Bar Buddy 2",
                    Description = "Bar Buddy được thiết kế với lối kiến trúc cổ điển, lấy cảm hứng từ phong cách Tây Ban Nha với vẻ đẹp hoài cổ, độc đáo. Quán được xây dựng với những bức tường gạch thô gai góc, dùng ánh sáng màu đỏ và vàng rất huyền ảo. Không giống với những quán bar quận 1 khác thường thuê DJ chơi nhạc thì Carmen Bar lại thuê ban nhạc sống với những bài cực chill.",
                    Discount = 0,
                    Email = "booking2@barbuddy.com",
                    EndTime = new TimeSpan(18, 00, 00),
                    StartTime = new TimeSpan(2, 00, 00),
                    Images = "barbuddy2.png",
                    PhoneNumber = "0907770777",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1",
                    BarName = "Bar Buddy 2",
                    Description = "Bar Buddy được thiết kế với lối kiến trúc cổ điển, lấy cảm hứng từ phong cách Tây Ban Nha với vẻ đẹp hoài cổ, độc đáo. Quán được xây dựng với những bức tường gạch thô gai góc, dùng ánh sáng màu đỏ và vàng rất huyền ảo. Không giống với những quán bar quận 1 khác thường thuê DJ chơi nhạc thì Carmen Bar lại thuê ban nhạc sống với những bài cực chill.",
                    Discount = 0,
                    Email = "booking2@barbuddy.com",
                    EndTime = new TimeSpan(18, 00, 00),
                    StartTime = new TimeSpan(2, 00, 00),
                    Images = "barbuddy3.png",
                    PhoneNumber = "0906660666",
                    Status = true
                }
            );

            // TableTypeData
            modelBuilder.Entity<TableType>().HasData(
                new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Bàn SVIP",
                    Description = "Bàn SVIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao nhất tại quán, phù hợp cho nhóm khách hàng từ 1-15 người, mức giá tối thiểu chỉ từ 10.000.000 VND.",
                    MaximumGuest = 15,
                    MinimumGuest = 1,
                    MinimumPrice = 10000000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Bàn VIP",
                    Description = "Bàn VIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao tại quán, phù hợp cho nhóm khách hàng từ 1-10 người, mức giá tối thiểu chỉ từ 5.000.000 VND.",
                    MaximumGuest = 10,
                    MinimumGuest = 1,
                    MinimumPrice = 5000000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Bàn Tiêu chuẩn 1",
                    Description = "Bàn Tiêu chuẩn 1 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 1-4 người, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 4,
                    MinimumGuest = 1,
                    MinimumPrice = 200000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Bàn Tiêu chuẩn 2",
                    Description = "Bàn Tiêu chuẩn 2 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 4-6 người, mức giá tối thiểu chỉ từ 500.000 VND.",
                    MaximumGuest = 6,
                    MinimumGuest = 4,
                    MinimumPrice = 500000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Bàn Quầy Bar",
                    Description = "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 1,
                    MinimumGuest = 1,
                    MinimumPrice = 100000,
                    IsDeleted = false,
                }
            );

            // DrinkCategoryData
            modelBuilder.Entity<DrinkCategory>().HasData(
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Nước ngọt",
                    Description = "Đồ uống không cồn như soda, nước ngọt có ga, và nước ngọt có hương vị.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Cocktail",
                    Description = "Đồ uống pha trộn thường chứa cồn, kết hợp với nước trái cây, soda hoặc các nguyên liệu khác.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Mocktail",
                    Description = "Phiên bản không cồn của các loại cocktail, phù hợp cho những người không uống rượu.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Rượu mạnh",
                    Description = "Đồ uống có cồn mạnh như vodka, whisky, gin, rum, v.v.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Bia",
                    Description = "Đồ uống có cồn được ủ từ lúa mạch, hoa bia và nước. Có nhiều loại khác nhau như lager, ale, stout.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Rượu vang",
                    Description = "Đồ uống có cồn được làm từ nho lên men, có nhiều loại như vang đỏ, vang trắng và vang hồng.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Trà",
                    Description = "Đồ uống nóng hoặc lạnh được pha từ lá trà, có nhiều loại như trà đen, trà xanh và trà thảo mộc.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Cà phê",
                    Description = "Đồ uống nóng hoặc lạnh được pha từ hạt cà phê rang, bao gồm espresso, cappuccino, latte và nhiều loại khác.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Nước ép",
                    Description = "Đồ uống tự nhiên được làm từ nước ép trái cây hoặc rau củ. Các loại phổ biến gồm nước cam, nước táo, và nước ép cà rốt.",
                    IsDrinkCategory = true
                }
            );
        }
    }
}
