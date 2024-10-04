using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext()
        {
        }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
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
                    Description = "Trải nghiệm không gian âm nhạc sống động, nơi bạn có thể tạm quên đi nhịp sống hối hả và thả mình vào những giai điệu sâu lắng.",
                    Discount = 10,
                    Email = "contact@barbuddy1.com",
                    EndTime = new TimeSpan(2, 00, 00),
                    StartTime = new TimeSpan(18, 00, 00),
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/a0d4292c13b0cc51b2487f4c276cd7c0d96510872c4a855db190ff2db8e692d2?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234567",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "153 Tôn Thất Đạm, Bến Nghé, quận 1, Hồ Chí Minh",
                    BarName = "Bar Buddy 2",
                    Description = "Phong cách trẻ trung, năng động, nơi diễn ra các sự kiện giải trí đa dạng dành cho những tâm hồn thích khám phá và kết nối.",
                    Discount = 15,
                    Email = "contact@barbuddy2.com",
                    EndTime = new TimeSpan(3, 00, 00),
                    StartTime = new TimeSpan(17, 00, 00),
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234568",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "264 Đ. Nam Kỳ Khởi Nghĩa, Phường 8, Quận 3",
                    BarName = "Bar Buddy 3",
                    Description = "Hòa mình vào không gian sang trọng với dịch vụ đẳng cấp, nơi mọi chi tiết đều được chăm chút để mang đến trải nghiệm tuyệt vời nhất.",
                    Discount = 20,
                    Email = "contact@barbuddy3.com",
                    EndTime = new TimeSpan(1, 00, 00),
                    StartTime = new TimeSpan(19, 00, 00),
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/2f3601dbe8c6d0a812bccaf7ecf02686ec5b99038e314c058a00a37c16840608?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234569",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "3C Đ. Tôn Đức Thắng, Bến Nghé, Quận 1, Thành phố Hồ Chí Minh",
                    BarName = "Bar Buddy 4",
                    Description = "Chuyên về cocktail cao cấp, đây là điểm đến lý tưởng để thưởng thức những món đồ uống tinh tế trong không gian thời thượng.",
                    Discount = 25,
                    Email = "contact@barbuddy4.com",
                    EndTime = new TimeSpan(4, 00, 00),
                    StartTime = new TimeSpan(20, 00, 00),
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/677e2c38ccd2ea07e8a72aa6262c873572a4cfd3da719a1e25c2152169bb47c6?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234570",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "11 Đ. Nam Quốc Cang, Phường Phạm Ngũ Lão, Quận 1",
                    BarName = "Bar Buddy 5",
                    Description = "Kết hợp hoàn hảo giữa âm nhạc sống động và DJ, tạo nên bầu không khí náo nhiệt phù hợp cho các cuộc vui thâu đêm.",
                    Discount = 5,
                    Email = "contact@barbuddy5.com",
                    EndTime = new TimeSpan(2, 30, 00),
                    StartTime = new TimeSpan(18, 30, 00),
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/fc1f4652930fe4a25d46a46d1933e950912b6ceace8e777840ceccd123995783?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234571",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "41 Nam Kỳ Khởi Nghĩa, Phường Nguyễn Thái Bình, Quận 1, Hồ Chí Minh",
                    BarName = "Bar Buddy 6",
                    Description = "Không gian thư giãn lý tưởng, nơi bạn có thể thỏa sức giải trí với các trò chơi thú vị cùng bạn bè và người thân.",
                    Discount = 10,
                    Email = "contact@barbuddy6.com",
                    EndTime = new TimeSpan(3, 30, 00),
                    StartTime = new TimeSpan(17, 30, 00),
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/4f4bc5cae670ae75847bb24a78027e45ce8487386c0a1043f999381ae9fa4831?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234572",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "20 Đ. Nguyễn Công Trứ, Phường Nguyễn Thái Bình, Quận 1",
                    BarName = "Bar Buddy 7",
                    Description = "Nếu bạn là một tín đồ âm nhạc, Bar Buddy 7 là nơi hội tụ những bản nhạc tuyệt vời giúp bạn phiêu theo từng giai điệu.",
                    Discount = 30,
                    Email = "contact@barbuddy7.com",
                    EndTime = new TimeSpan(1, 00, 00),
                    StartTime = new TimeSpan(19, 00, 00),
                    Images = "https://vietnamnightlife.com/uploads/images/2020/02/1580805657-multi_product20-bambamoverview1.jpg.webp",
                    PhoneNumber = "0901234573",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "120 Đ. Nguyễn Huệ, Bến Nghé, Quận 1",
                    BarName = "Bar Buddy 8",
                    Description = "Một trong những rooftop bar với tầm nhìn tuyệt đẹp, nơi lý tưởng để vừa thưởng thức đồ uống vừa ngắm nhìn cảnh đêm Sài Gòn.",
                    Discount = 20,
                    Email = "contact@barbuddy8.com",
                    EndTime = new TimeSpan(2, 00, 00),
                    StartTime = new TimeSpan(17, 00, 00),
                    Images = "default",
                    PhoneNumber = "0901234574",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "30 Đ. Tôn Thất Tùng, Quận 1",
                    BarName = "Bar Buddy 9",
                    Description = "Quán bar độc đáo chuyên về craft beer, dành cho những ai yêu thích hương vị đặc biệt và trải nghiệm ẩm thực thú vị.",
                    Discount = 15,
                    Email = "contact@barbuddy9.com",
                    EndTime = new TimeSpan(3, 00, 00),
                    StartTime = new TimeSpan(18, 00, 00),
                    Images = "default",
                    PhoneNumber = "0901234575",
                    Status = true
                },
                new Bar
                {
                    BarId = Guid.NewGuid(),
                    Address = "25 Đ. Lê Duẩn, Quận 1",
                    BarName = "Bar Buddy 10",
                    Description = "Mang đến không gian ấm cúng với ánh sáng nhẹ nhàng và những ly cocktail độc đáo, lý tưởng cho những buổi tối thư giãn.",
                    Discount = 10,
                    Email = "contact@barbuddy10.com",
                    EndTime = new TimeSpan(1, 30, 00),
                    StartTime = new TimeSpan(17, 00, 00),
                    Images = "default",
                    PhoneNumber = "0901234576",
                    Status = true
                }
            );

            //Account
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    RoleId = Guid.Parse("b3b5a546-519d-411b-89d0-20c824e18d11"),
                    Email = "admin1@barbuddy1.com",
                    Password = "password123",
                    Fullname = "Admin Bar Buddy1",
                    Dob = new DateTime(1980, 5, 1),
                    Phone = "0901234567",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "admin1.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    RoleId = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805"),
                    Email = "staff1@barbuddy2.com",
                    Password = "password456",
                    Fullname = "Staff Bar Buddy2",
                    Dob = new DateTime(1992, 7, 15),
                    Phone = "0901234568",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "staff1.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    RoleId = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805"),
                    Email = "staff2@barbuddy3.com",
                    Password = "password789",
                    Fullname = "Staff Bar Buddy3",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "staff2.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"),
                    BarId = null,
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    Email = "customer1@barbuddy4.com",
                    Password = "password321",
                    Fullname = "Customer Bar Buddy4",
                    Dob = new DateTime(1985, 11, 30),
                    Phone = "0901234570",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "customer1.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440006"),
                    BarId = null,
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    Email = "customer2@barbuddy6.com",
                    Password = "password987",
                    Fullname = "Customer Bar Buddy6",
                    Dob = new DateTime(1993, 2, 22),
                    Phone = "0901234572",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "customer2.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440007"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440006"),
                    RoleId = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805"),
                    Email = "staff3@barbuddy7.com",
                    Password = "password111",
                    Fullname = "Staff Bar Buddy7",
                    Dob = new DateTime(1987, 10, 13),
                    Phone = "0901234573",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "staff3.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440009"),
                    BarId = null,
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    Email = "customer3@barbuddy9.com",
                    Password = "password333",
                    Fullname = "Customer Bar Buddy9",
                    Dob = new DateTime(1994, 12, 5),
                    Phone = "0901234575",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "customer3.png",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-a716-446655440010"),
                    BarId = null,
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    Email = "customer4@barbuddy10.com",
                    Password = "password444",
                    Fullname = "Customer Bar Buddy10",
                    Dob = new DateTime(1982, 4, 15),
                    Phone = "0901234576",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "customer4.png",
                    Status = 1
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
