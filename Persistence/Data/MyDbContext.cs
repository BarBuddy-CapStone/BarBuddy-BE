﻿using Domain.Entities;
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
        Images = "barbuddy1.png",
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
        Images = "barbuddy2.png",
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
        Images = "barbuddy3.png",
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
        Images = "barbuddy4.png",
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
        Images = "barbuddy5.png",
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
        Images = "barbuddy6.png",
        PhoneNumber = "0901234572",
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
