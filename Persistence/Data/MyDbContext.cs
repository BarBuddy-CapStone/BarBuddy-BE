using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
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
                    RoleId = Guid.Parse("b3b5a546-519d-411b-89d0-20c824e18d11"),
                    RoleName = "ADMIN"
                },
                new Role
                {
                    RoleId = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805"),
                    RoleName = "STAFF"
                },
                new Role
                {
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    RoleName = "CUSTOMER"
                }
            );

            // Bar Data
            modelBuilder.Entity<Bar>().HasData(
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440000"),
        Address = "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1",
        BarName = "Bar Buddy 1",
        Description = "Nơi lý tưởng để thư giãn và tận hưởng âm nhạc.",
        Discount = 10,
        Email = "contact@barbuddy1.com",
        EndTime = new TimeSpan(2, 0, 0),
        StartTime = new TimeSpan(18, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/a0d4292c13b0cc51b2487f4c276cd7c0d96510872c4a855db190ff2db8e692d2?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234567",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440001"),
        Address = "153 Tôn Thất Đạm, Bến Nghé, quận 1, Hồ Chí Minh",
        BarName = "Bar Buddy 2",
        Description = "Quán bar phong cách trẻ trung với nhiều sự kiện thú vị.",
        Discount = 15,
        Email = "contact@barbuddy2.com",
        EndTime = new TimeSpan(3, 0, 0),
        StartTime = new TimeSpan(17, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234568",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440002"),
        Address = "264 Đ. Nam Kỳ Khởi Nghĩa, Phường 8, Quận 3",
        BarName = "Bar Buddy 3",
        Description = "Không gian sang trọng và dịch vụ tận tâm.",
        Discount = 20,
        Email = "contact@barbuddy3.com",
        EndTime = new TimeSpan(1, 0, 0),
        StartTime = new TimeSpan(19, 0, 0),
        Images = "https://vietnamnightlife.com/uploads/images/2020/02/1580805657-multi_product20-bambamoverview1.jpg.webp",
        PhoneNumber = "0901234569",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440003"),
        Address = "3C Đ. Tôn Đức Thắng, Bến Nghé, Quận 1, Thành phố Hồ Chí Minh",
        BarName = "Bar Buddy 4",
        Description = "Chuyên phục vụ cocktail và đồ uống cao cấp.",
        Discount = 25,
        Email = "contact@barbuddy4.com",
        EndTime = new TimeSpan(4, 0, 0),
        StartTime = new TimeSpan(20, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/4f4bc5cae670ae75847bb24a78027e45ce8487386c0a1043f999381ae9fa4831?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234570",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440004"),
        Address = "11 Đ.Nam Quốc Cang, Phường Phạm Ngũ Lão, Quận 1",
        BarName = "Bar Buddy 5",
        Description = "Quán bar kết hợp giữa nhạc sống và DJ.",
        Discount = 5,
        Email = "contact@barbuddy5.com",
        EndTime = new TimeSpan(2, 30, 0),
        StartTime = new TimeSpan(18, 30, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/fc1f4652930fe4a25d46a46d1933e950912b6ceace8e777840ceccd123995783?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234571",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440005"),
        Address = "41 Nam Kỳ Khởi Nghĩa, Phường Nguyễn Thái Bình, Quận 1, Hồ Chí Minh",
        BarName = "Bar Buddy 6",
        Description = "Không gian thoải mái với nhiều trò chơi giải trí.",
        Discount = 10,
        Email = "contact@barbuddy6.com",
        EndTime = new TimeSpan(3, 30, 0),
        StartTime = new TimeSpan(17, 30, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/677e2c38ccd2ea07e8a72aa6262c873572a4cfd3da719a1e25c2152169bb47c6?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234572",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440006"),
        Address = "20 Đ. Nguyễn Công Trứ, Phường Nguyễn Thái Bình, Quận 1",
        BarName = "Bar Buddy 7",
        Description = "Nơi hội tụ của những tâm hồn yêu thích âm nhạc.",
        Discount = 30,
        Email = "contact@barbuddy7.com",
        EndTime = new TimeSpan(1, 0, 0),
        StartTime = new TimeSpan(19, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/2f3601dbe8c6d0a812bccaf7ecf02686ec5b99038e314c058a00a37c16840608?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234573",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440007"),
        Address = "120 Đ. Nguyễn Huệ, Bến Nghé, Quận 1",
        BarName = "Bar Buddy 8",
        Description = "Quán bar rooftop với tầm nhìn đẹp.",
        Discount = 20,
        Email = "contact@barbuddy8.com",
        EndTime = new TimeSpan(2, 0, 0),
        StartTime = new TimeSpan(17, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234574",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440008"),
        Address = "30 Đ. Tôn Thất Tùng, Quận 1",
        BarName = "Bar Buddy 9",
        Description = "Quán bar dành cho các tín đồ yêu thích craft beer.",
        Discount = 15,
        Email = "contact@barbuddy9.com",
        EndTime = new TimeSpan(3, 0, 0),
        StartTime = new TimeSpan(18, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234575",
        Status = true
    },
    new Bar
    {
        BarId = new Guid("550e8400-e29b-41d4-a716-446655440009"),
        Address = "25 Đ. Lê Duẩn, Quận 1",
        BarName = "Bar Buddy 10",
        Description = "Không gian ấm cúng với các loại cocktail độc đáo.",
        Discount = 10,
        Email = "contact@barbuddy10.com",
        EndTime = new TimeSpan(2, 0, 0),
        StartTime = new TimeSpan(19, 0, 0),
        Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/a0d4292c13b0cc51b2487f4c276cd7c0d96510872c4a855db190ff2db8e692d2?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
        PhoneNumber = "0901234576",
        Status = true
    }
);



            //Account
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440001"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    RoleId = Guid.Parse("b3b5a546-519d-411b-89d0-20c824e18d11"),
                    Email = "admin1@barbuddy1.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440002"),
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440003"),
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"),
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440006"),
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440007"),
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440009"),
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
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440010"),
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
            var svip = Guid.NewGuid();
            var vip = Guid.NewGuid();
            var tc1 = Guid.NewGuid();
            var tc2 = Guid.NewGuid();
            var qb = Guid.NewGuid();

            modelBuilder.Entity<TableType>().HasData(
                new TableType
                {
                    TableTypeId = svip,
                    TypeName = "Bàn SVIP",
                    Description = "Bàn SVIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao nhất tại quán, phù hợp cho nhóm khách hàng từ 1-15 người, mức giá tối thiểu chỉ từ 10.000.000 VND.",
                    MaximumGuest = 15,
                    MinimumGuest = 1,
                    MinimumPrice = 10000000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = vip,
                    TypeName = "Bàn VIP",
                    Description = "Bàn VIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao tại quán, phù hợp cho nhóm khách hàng từ 1-10 người, mức giá tối thiểu chỉ từ 5.000.000 VND.",
                    MaximumGuest = 10,
                    MinimumGuest = 1,
                    MinimumPrice = 5000000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = tc1,
                    TypeName = "Bàn Tiêu chuẩn 1",
                    Description = "Bàn Tiêu chuẩn 1 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 1-4 người, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 4,
                    MinimumGuest = 1,
                    MinimumPrice = 200000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = tc2,
                    TypeName = "Bàn Tiêu chuẩn 2",
                    Description = "Bàn Tiêu chuẩn 2 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 4-6 người, mức giá tối thiểu chỉ từ 500.000 VND.",
                    MaximumGuest = 6,
                    MinimumGuest = 4,
                    MinimumPrice = 500000,
                    IsDeleted = false,
                },
                new TableType
                {
                    TableTypeId = qb,
                    TypeName = "Bàn Quầy Bar",
                    Description = "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 1,
                    MinimumGuest = 1,
                    MinimumPrice = 100000,
                    IsDeleted = false,
                }
            );

            //Table
            modelBuilder.Entity<Table>().HasData(
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330000"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    TableTypeId = tc1,
                    TableName = "Table A1",
                    Status = 0,
                    IsDeleted = true
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330001"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    TableTypeId = qb,
                    TableName = "Table B1",
                    Status = 0,
                    IsDeleted = true
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330002"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    TableTypeId = tc2,
                    TableName = "Table C1",
                    Status = 0,
                    IsDeleted = true
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330003"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    TableTypeId = svip,
                    TableName = "Table A2",
                    Status = 0,
                    IsDeleted = true
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330004"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330005"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330006"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330007"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330008"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330009"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330010"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }, new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330011"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    TableTypeId = vip,
                    TableName = "Table B2",
                    Status = 0,
                    IsDeleted = true
                }
            );


            // DrinkCategoryData
            var drinkCate1 = Guid.NewGuid();
            var drinkCate2 = Guid.NewGuid();
            var drinkCate3 = Guid.NewGuid();
            var drinkCate4 = Guid.NewGuid();
            var drinkCate5 = Guid.NewGuid();
            var drinkCate6 = Guid.NewGuid();
            var drinkCate7 = Guid.NewGuid();
            var drinkCate8 = Guid.NewGuid();
            var drinkCate9 = Guid.NewGuid();

            modelBuilder.Entity<DrinkCategory>().HasData(
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate1,
                    DrinksCategoryName = "Nước ngọt",
                    Description = "Đồ uống không cồn như soda, nước ngọt có ga, và nước ngọt có hương vị.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate2,
                    DrinksCategoryName = "Cocktail",
                    Description = "Đồ uống pha trộn thường chứa cồn, kết hợp với nước trái cây, soda hoặc các nguyên liệu khác.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate3,
                    DrinksCategoryName = "Mocktail",
                    Description = "Phiên bản không cồn của các loại cocktail, phù hợp cho những người không uống rượu.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate4,
                    DrinksCategoryName = "Rượu mạnh",
                    Description = "Đồ uống có cồn mạnh như vodka, whisky, gin, rum, v.v.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate5,
                    DrinksCategoryName = "Bia",
                    Description = "Đồ uống có cồn được ủ từ lúa mạch, hoa bia và nước. Có nhiều loại khác nhau như lager, ale, stout.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate6,
                    DrinksCategoryName = "Rượu vang",
                    Description = "Đồ uống có cồn được làm từ nho lên men, có nhiều loại như vang đỏ, vang trắng và vang hồng.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate7,
                    DrinksCategoryName = "Trà",
                    Description = "Đồ uống nóng hoặc lạnh được pha từ lá trà, có nhiều loại như trà đen, trà xanh và trà thảo mộc.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate8,
                    DrinksCategoryName = "Cà phê",
                    Description = "Đồ uống nóng hoặc lạnh được pha từ hạt cà phê rang, bao gồm espresso, cappuccino, latte và nhiều loại khác.",
                    IsDrinkCategory = true
                },
                new DrinkCategory
                {
                    DrinksCategoryId = drinkCate9,
                    DrinksCategoryName = "Nước ép",
                    Description = "Đồ uống tự nhiên được làm từ nước ép trái cây hoặc rau củ. Các loại phổ biến gồm nước cam, nước táo, và nước ép cà rốt.",
                    IsDrinkCategory = true
                }
            );

            //Drink Data
            modelBuilder.Entity<Drink>().HasData(
            new Drink
            {
                DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330000"),
                DrinkCode = "D0001",
                DrinkCategoryId = drinkCate1,
                DrinkName = "Coca Cola",
                Description = "Nước ngọt có ga phổ biến.",
                Price = 15000,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Image = "https://www.coca-cola.com/content/dam/onexp/vn/home-image/coca-cola/Coca-Cola_OT%20320ml_VN-EX_Desktop.png",
                Status = true
            },
            new Drink
            {
                DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330001"),
                DrinkCode = "D0002",
                DrinkCategoryId = drinkCate2,
                DrinkName = "Mojito",
                Description = "Cocktail nổi tiếng pha từ rượu rum và bạc hà.",
                Price = 70000,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Image = "https://www.liquor.com/thmb/MJRVqf-itJGY90nwUOYGXnyG-HA=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/mojito-720x720-primary-6a57f80e200c412e9a77a1687f312ff7.jpg",
                Status = true
            },
            new Drink
            {
                DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330002"),
                DrinkCode = "D0003",
                DrinkCategoryId = drinkCate3,
                DrinkName = "Trà Đào",
                Description = "Trà xanh kết hợp với vị ngọt của đào.",
                Price = 35000,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Image = "https://file.hstatic.net/200000684957/article/tra-dao_e022b1a9ac564ee186007875701ac643.jpg",
                Status = true
            },
            new Drink
            {
                DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330003"),
                DrinkCode = "D0004",
                DrinkCategoryId = drinkCate1,
                DrinkName = "Pepsi",
                Description = "Nước ngọt có ga, phổ biến tương tự Coca Cola.",
                Price = 15000,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Image = "http://thepizzacompany.vn/images/thumbs/000/0002364_pepsi-15l-pet_500.jpeg",
                Status = true
            },
            new Drink
            {
                DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330004"),
                DrinkCode = "D0005",
                DrinkCategoryId = drinkCate4,
                DrinkName = "Screwdriver",
                Description = "Cocktail kết hợp giữa rượu vodka và nước cam.",
                Price = 80000,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Image = "https://www.liquor.com/thmb/RnOVWoIXp7OAJRS-NSDIF9Bglbc=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/LQR-screwdriver-original-4000x4000-edb2f56dd69146bba9f7fafbf69e00a0.jpg",
                Status = true
            },
            new Drink
            {
                DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330005"),
                DrinkCode = "D0006",
                DrinkCategoryId = drinkCate8,
                DrinkName = "Cà phê đen",
                Description = "Cà phê đen pha đậm, không đường, không sữa.",
                Price = 15000,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Image = "https://suckhoedoisong.qltns.mediacdn.vn/324455921873985536/2024/8/14/2121767707dcce179f6866d132a2d6a384312f9-1723600454996-1723600455541950721311.jpg",
                Status = true
            }
            );
            modelBuilder.Entity<Booking>().HasData(
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Bar Buddy 1
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440001"), // Admin Bar Buddy 1
        BookingCode = "BB0001", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-5),
        Status = 1, // Confirmed
    },
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Bar Buddy 2
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440002"), // Staff Bar Buddy 2
        BookingCode = "BB0002", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-7),
        Status = 2, // Completed
    },
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Bar Buddy 3
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440003"), // Customer Bar Buddy 3
        BookingCode = "BB0003", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-3),
        Status = 1, // Confirmed
    },
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Bar Buddy 4
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"), // Customer Bar Buddy 4
        BookingCode = "BB0004", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-2),
        Status = 1, // Confirmed
    }
);
            // BookingTable Data
            // BookingTable Data
            modelBuilder.Entity<BookingTable>().HasData(
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Booking 1 (Bar Buddy 1)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330000"), // Table A1 - Bar Buddy 1
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Booking 2 (Bar Buddy 2)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330001"), // Table B1 - Bar Buddy 2
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Booking 3 (Bar Buddy 3)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330002"), // Table C1 - Bar Buddy 3
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 4)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330005"), // Table D1 - Bar Buddy 4
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 4)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330006"), // Table D1 - Bar Buddy 4
                }, new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 4)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330007"), // Table D1 - Bar Buddy 4
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 4)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330008"), // Table D1 - Bar Buddy 4
                }
            );


        }
    }
}
