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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationDetail> NotificationDetails { get; set; }
        public DbSet<BarEvent> BarEvents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<TimeEvent> TimeEvents { get; set; }
        public DbSet<EventVoucher> EventVouchers { get; set; }
        public DbSet<BarTime> BarTimes { get; set; }


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
                    CategoryName = "vui",
                    Description = "Vui qua di",
                    IsDeleted = false,
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = buonId,
                    CategoryName = "buồn",
                    Description = "Vui qua di",
                    IsDeleted = false
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = hanhPhucId,
                    CategoryName = "hạnh phúc",
                    Description = "Vui qua di",
                    IsDeleted = false
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = tucGianId,
                    CategoryName = "tức giận",
                    Description = "Vui qua di",
                    IsDeleted = false
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = chanNanId,
                    CategoryName = "chán nản",
                    Description = "Vui qua di",
                    IsDeleted = false
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = dangYeuId,
                    CategoryName = "đang yêu",
                    Description = "Vui qua di",
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<DrinkEmotionalCategory>().HasData(
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330100"),
                    EmotionalDrinkCategoryId = vuiId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330000") // Coca Cola
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330101"),
                    EmotionalDrinkCategoryId = buonId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330001") // Mojito
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330102"),
                    EmotionalDrinkCategoryId = hanhPhucId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330002") // Trà Đào
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330103"),
                    EmotionalDrinkCategoryId = tucGianId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330003") // Pepsi
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330104"),
                    EmotionalDrinkCategoryId = chanNanId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330004") // Screwdriver
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330105"),
                    EmotionalDrinkCategoryId = dangYeuId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330005") // Cà phê đen
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330106"),
                    EmotionalDrinkCategoryId = vuiId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330001") // Mojito
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330107"),
                    EmotionalDrinkCategoryId = hanhPhucId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330003") // Pepsi
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330108"),
                    EmotionalDrinkCategoryId = dangYeuId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330000") // Coca Cola
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.Parse("660d7300-f30c-30c3-b827-335544330109"),
                    EmotionalDrinkCategoryId = chanNanId,
                    DrinkId = Guid.Parse("550d7300-f30c-30c3-b827-335544330002") // Trà Đào
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
                    Description = "Tọa lạc trên tầng 14, tại 87A Hàm Nghi, quận 1, nằm ở khu vực trung tâm thành phố. Đến với Bar Buddy 1 để trải nghiệm sky bar \"HOT\" nhất Sài Gòn hiện nay. Bar Buddy 1 được mệnh danh là địa điểm ăn chơi Sài Gòn xa hoa bậc nhất. Âm nhạc cuốn hút và vị ngon mê đắm của những đồ uống hảo hạng sẽ giúp bạn tận hưởng những phút giây thăng hoa. Những màn trình diễn đẳng cấp của các ca sĩ hàng đầu Việt Nam sẽ thổi bùng không khí khiến bạn không thể ngồi yên.",
                    Discount = 10,
                    Email = "contact@barbuddy1.com",
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/a0d4292c13b0cc51b2487f4c276cd7c0d96510872c4a855db190ff2db8e692d2?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234567",
                    Status = true
                },
                new Bar
                {
                    BarId = new Guid("550e8400-e29b-41d4-a716-446655440001"),
                    Address = "153 Tôn Thất Đạm, Bến Nghé, quận 1, Hồ Chí Minh",
                    BarName = "Bar Buddy 2",
                    Description = "Tọa lạc ngay giữa trung tâm thành phố Hồ Chí Minh, Bar Buddy 2 với thiết kế độc đáo, lạ mắt, mang nét thu hút riêng như lạc vào thế giới của bộ phim hành động kinh dị: Mad Max. Một không gian nổi loạn và cực ngầu mang lại cảm giác quái lạ đầy bí ẩn. Bar Buddy 2 với sự đầu tư hoành tráng bằng những thiết bị, âm thanh, ánh sáng hiện đại nhất bạn sẽ được các DJ hàng đầu chiêu đãi cùng dàn khách mời đặc biệt: Hồ Ngọc Hà, Sơn Tùng M-TP, Erik.... chắc chắn khách hàng của Atmos sẽ luôn được tiếp đón và phục vụ tận tình",
                    Discount = 15,
                    Email = "contact@barbuddy2.com",
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/7cbd7d84e2ff7b5156aa5241bd27de56fe00bcb6e309e2c77ff2c39bf3b0b236?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234568",
                    Status = true
                },
                new Bar
                {
                    BarId = new Guid("550e8400-e29b-41d4-a716-446655440002"),
                    Address = "264 Đ. Nam Kỳ Khởi Nghĩa, Phường 8, Quận 3",
                    BarName = "Bar Buddy 3",
                    Description = "Nằm tại trung tâm quận 3, Bar Buddy 3 là một trong những bar lâu năm nổi tiếng hàng đầu Sài Gòn. Không gian sang trọng nhiều vị trí đẹp và có sân khấu lớn ngay trung tâm. Sử dụng chất nhạc Vinahouse cực kỳ mạnh mẽ \"on trend\" là điều luôn hấp dẫn các vị khách tới Bar Buddy 3. Hơn thế nữa các sự kiện luôn diễn ra vào cuối tuần với dàn khách mời ca sĩ hàng đầu Việt Nam: Ưng Hoàng Phúc, Duy Mạnh, Trịnh Tuấn Vỹ… Với slogan \"Nơi thể hiện đẳng cấp của bạn\" hãy sẵn sàng thể hiện bản thân tại Bar Buddy 3 bar.",
                    Discount = 20,
                    Email = "contact@barbuddy3.com",
                    Images = "https://vietnamnightlife.com/uploads/images/2020/02/1580805657-multi_product20-bambamoverview1.jpg.webp",
                    PhoneNumber = "0901234569",
                    Status = true
                },
                new Bar
                {
                    BarId = new Guid("550e8400-e29b-41d4-a716-446655440003"),
                    Address = "3C Đ. Tôn Đức Thắng, Bến Nghé, Quận 1, Thành phố Hồ Chí Minh",
                    BarName = "Bar Buddy 4",
                    Description = "Bar Buddy 4 sở hữu vị trí đắc địa trên tầng 2, khách sạn 5 sao Le Meridien trên đường Tôn Đức Thắng. Bar Buddy 4 sở hữu không gian \"dark bar\" không lẫn vào đâu được cùng phong cách Commas, với công nghệ laser light độc nhất vô nhị tại Hồ Chí Minh. Khách hàng sẽ có những giây phút bung xõa cùng những giai điệu Hip Hop tại Bar Buddy 4, tân binh mới nhất tại Nightlife Hồ Chí Minh. ",
                    Discount = 25,
                    Email = "contact@barbuddy4.com",
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
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/2f3601dbe8c6d0a812bccaf7ecf02686ec5b99038e314c058a00a37c16840608?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234573",
                    Status = true
                },
                new Bar
                {
                    BarId = new Guid("550e8400-e29b-41d4-a716-446655440007"),
                    Address = "120 Đ. Nguyễn Huệ, Bến Nghé, Quận 1",
                    BarName = "Bar Buddy 8",
                    Description = "Quán bar rooftop với tầm nhìn đp.",
                    Discount = 20,
                    Email = "contact@barbuddy8.com",
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
                    Fullname = "Neymar Jr",
                    Dob = new DateTime(1980, 5, 1),
                    Phone = "0901234567",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://dailytrust.com/wp-content/uploads/2022/12/Neymar.jpg",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440002"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    RoleId = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805"),
                    Email = "staff1@barbuddy2.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Lionel Messi",
                    Dob = new DateTime(1992, 7, 15),
                    Phone = "0901234568",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://dailytrust.com/wp-content/uploads/2022/12/Neymar.jpg",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440003"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    RoleId = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805"),
                    Email = "staff2@barbuddy3.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Foden",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://zumroad.com/images/upload/editor/source/sport/phil-foden-biography-01_1.jpg",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"),
                    BarId = null,
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    Email = "customer1@barbuddy4.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Mbappe",
                    Dob = new DateTime(1985, 11, 30),
                    Phone = "0901234570",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://d3lbfr570u7hdr.cloudfront.net/stadiumastro/media/perform-article/2022/dec/19/kylian-mbappe_s4z7y7klnl521rdjwfihpt6ay.jpg",
                    Status = 1
                },
                new Account
                {
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440009"),
                    BarId = null,
                    RoleId = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552"),
                    Email = "customer3@barbuddy9.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Vinicius Jr",
                    Dob = new DateTime(1994, 12, 5),
                    Phone = "0901234575",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://th.bing.com/th/id/OIP.SO1-rUp8nmpmQIHFf60hYgHaEK?rs=1&pid=ImgDetMain",
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
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Thêm BarId vào TableType
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
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Thêm BarId vào TableType
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
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Thêm BarId vào TableType
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
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Thêm BarId vào TableType
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
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"), // Thêm BarId vào TableType
                    TypeName = "Bàn Quầy Bar",
                    Description = "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 1,
                    MinimumGuest = 1,
                    MinimumPrice = 100000,
                    IsDeleted = false,
                }
            );

            // BarTime Data
            modelBuilder.Entity<BarTime>().HasData(
                new BarTime
                {
                    BarTimeId = Guid.NewGuid(),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Bar Buddy 1
                    DayOfWeek = 5, // Thứ Sáu
                    StartTime = new TimeSpan(18, 0, 0), // 6:00 PM
                    EndTime = new TimeSpan(2, 0, 0) // 2:00 AM
                },
                new BarTime
                {
                    BarTimeId = Guid.NewGuid(),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Bar Buddy 2
                    DayOfWeek = 6, // Thứ Bảy
                    StartTime = new TimeSpan(19, 0, 0), // 7:00 PM
                    EndTime = new TimeSpan(3, 0, 0) // 3:00 AM
                },
                new BarTime
                {
                    BarTimeId = Guid.NewGuid(),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Bar Buddy 3
                    DayOfWeek = 0, // Chủ Nhật
                    StartTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    EndTime = new TimeSpan(1, 0, 0) // 1:00 AM
                }
                // ... thêm dữ liệu mẫu cho các quán bar khác nếu cần ...
            );

            //Table
            modelBuilder.Entity<Table>().HasData(
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330444"),
                    TableTypeId = svip,
                    TableName = "Table SVIP1-A1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330443"),
                    TableTypeId = svip,
                    TableName = "Table SVIP1-B1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330442"),
                    TableTypeId = vip,
                    TableName = "Table VIP1-A1",
                    Status = 0,
                    IsDeleted = false
                }, 
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330441"),
                    TableTypeId = vip,
                    TableName = "Table VIP1-B1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330440"),
                    TableTypeId = vip,
                    TableName = "Table VIP1-C1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330000"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-A1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330331"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-B1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330231"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-C1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330232"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-D1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330233"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-E1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330234"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-F1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330235"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-A1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330236"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-B1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330237"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-C1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330238"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-D1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330001"),
                    TableTypeId = qb,
                    TableName = "Table QA-1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330021"),
                    TableTypeId = qb,
                    TableName = "Table QB-1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330013"),
                    TableTypeId = qb,
                    TableName = "Table QC-1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330014"),
                    TableTypeId = qb,
                    TableName = "Table QD-1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330015"),
                    TableTypeId = qb,
                    TableName = "Table QE-1",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330016"),
                    TableTypeId = qb,
                    TableName = "Table QF-1",
                    Status = 0,
                    IsDeleted = false
                },
                ////////////////////////
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330111"),
                    TableTypeId = svip,
                    TableName = "SVIPA-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330112"),
                    TableTypeId = svip,
                    TableName = "SVIPB-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330113"),
                    TableTypeId = vip,
                    TableName = "VIPA-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330114"),
                    TableTypeId = vip,
                    TableName = "VIPB-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330115"),
                    TableTypeId = vip,
                    TableName = "VIPC-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330116"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-A2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330117"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-B2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330118"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-C2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330119"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-D2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330120"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-E2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330121"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-A2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330122"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-B2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330123"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-C2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330124"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-D2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330125"),
                    TableTypeId = qb,
                    TableName = "QA-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330126"),
                    TableTypeId = qb,
                    TableName = "QB-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330127"),
                    TableTypeId = qb,
                    TableName = "QC-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330128"),
                    TableTypeId = qb,
                    TableName = "QD-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330129"),
                    TableTypeId = qb,
                    TableName = "QE-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330130"),
                    TableTypeId = qb,
                    TableName = "QF-2",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330131"),
                    TableTypeId = svip,
                    TableName = "SVIPA-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330132"),
                    TableTypeId = svip,
                    TableName = "SVIPB-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330133"),
                    TableTypeId = vip,
                    TableName = "VIPA-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330134"),
                    TableTypeId = vip,
                    TableName = "VIPB-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330135"),
                    TableTypeId = vip,
                    TableName = "VIPC-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330136"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-A3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330137"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-B3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330138"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-C3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330139"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-D3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330140"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-E3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330141"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-A3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330142"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-B3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330143"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-C3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330144"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-D3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330145"),
                    TableTypeId = qb,
                    TableName = "QA-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330146"),
                    TableTypeId = qb,
                    TableName = "QB-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330147"),
                    TableTypeId = qb,
                    TableName = "QC-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330148"),
                    TableTypeId = qb,
                    TableName = "QD-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330149"),
                    TableTypeId = qb,
                    TableName = "QE-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330150"),
                    TableTypeId = qb,
                    TableName = "QF-3",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330151"),
                    TableTypeId = svip,
                    TableName = "SVIPA-4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330152"),
                    TableTypeId = vip,
                    TableName = "VIPA-4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330153"),
                    TableTypeId = vip,
                    TableName = "VIPB-4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330154"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-A4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330155"),
                    TableTypeId = tc1,
                    TableName = "Table TC1-B4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330156"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-A4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330157"),
                    TableTypeId = tc2,
                    TableName = "Table TC2-B4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330158"),
                    TableTypeId = qb,
                    TableName = "QA-4",
                    Status = 0,
                    IsDeleted = false
                },
                new Table
                {
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330159"),
                    TableTypeId = qb,
                    TableName = "QB-4",
                    Status = 0,
                    IsDeleted = false
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
        BookingTime = new TimeSpan(19, 0, 0), // Thêm thời gian đặt chỗ
        Status = 1, // Confirmed
        NumOfTable = 1, // Số lượng bàn
        CreateAt = DateTime.Now // Ngày tạo
    },
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Bar Buddy 2
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440002"), // Staff Bar Buddy 2
        BookingCode = "BB0002", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-7),
        BookingTime = new TimeSpan(20, 0, 0), // Thêm thời gian đặt chỗ
        Status = 2, // Completed
        NumOfTable = 2, // Số lượng bàn
        CreateAt = DateTime.Now // Ngày tạo
    },
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Bar Buddy 3
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440003"), // Customer Bar Buddy 3
        BookingCode = "BB0003", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-3),
        BookingTime = new TimeSpan(18, 0, 0), // Thêm thời gian đặt chỗ
        Status = 1, // Confirmed
        NumOfTable = 1, // Số lượng bàn
        CreateAt = DateTime.Now // Ngày tạo
    },
    new Booking
    {
        BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
        BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Bar Buddy 3
        AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"), // Customer Bar Buddy 3
        BookingCode = "BB0004", // Custom booking code
        BookingDate = DateTime.Now.AddDays(-2),
        BookingTime = new TimeSpan(21, 0, 0), // Thêm thời gian đặt chỗ
        Status = 1, // Confirmed
        NumOfTable = 1, // Số lượng bàn
        CreateAt = DateTime.Now // Ngày tạo
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
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(19, 0, 0) // Thời gian đặt bàn
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Booking 2 (Bar Buddy 2)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330117"), // Table B1 - Bar Buddy 2
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(20, 0, 0) // Thời gian đặt bàn
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Booking 3 (Bar Buddy 4)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330159"), // Table C1 - Bar Buddy 4
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(18, 0, 0) // Thời gian đặt bàn
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 3)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330141"), // Table D1 - Bar Buddy 3
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(21, 0, 0) // Thời gian đặt bàn
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 3)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330142"), // Table D1 - Bar Buddy 3
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(21, 0, 0) // Thời gian đặt bàn
                }, new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 3)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330143"), // Table D1 - Bar Buddy 3
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(21, 0, 0) // Thời gian đặt bàn
                },
                new BookingTable
                {
                    BookingTableId = Guid.NewGuid(),
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4 (Bar Buddy 3)
                    TableId = Guid.Parse("660d7300-f30c-30c3-b827-335544330144"), // Table D1 - Bar Buddy 3
                    ReservationDate = DateTimeOffset.Now, // Ngày đặt bàn
                    ReservationTime = new TimeSpan(21, 0, 0) // Thời gian đặt bàn
                }
            );

            modelBuilder.Entity<Notification>().HasData(
                new Notification
                {
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330160"),
                    Title = "Welcome Notification",
                    Message = "Welcome to the system!",
                    CreatedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now
                },
                new Notification
                {
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330161"),
                    Title = "System Maintenance",
                    Message = "System will undergo maintenance at midnight.",
                    CreatedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now
                },
                // Thêm thông báo mẫu thứ 3
                new Notification
                {
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330162"),
                    Title = "Booking Confirmation",
                    Message = "Your table reservation is confirmed!",
                    CreatedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now
                },
                // Thêm thông báo mẫu thứ 4
                new Notification
                {
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330163"),
                    Title = "Happy Hour Reminder",
                    Message = "Don't forget, Happy Hour starts at 5 PM today!",
                    CreatedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now
                }
                );

            modelBuilder.Entity<NotificationDetail>().HasData(
                new NotificationDetail
                {
                    NotificationDetailId = Guid.Parse("660d7300-f30c-40c3-b827-335544330170"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"),
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330160"),
                    IsRead = false,
                },
                new NotificationDetail
                {
                    NotificationDetailId = Guid.Parse("660d7300-f30c-40c3-b827-335544330171"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"),
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330161"),
                    IsRead = false,
                },
                // Thêm chi tiết thông báo thứ 3
                new NotificationDetail
                {
                    NotificationDetailId = Guid.Parse("660d7300-f30c-40c3-b827-335544330172"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"),
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330162"),
                    IsRead = false,
                },
                // Thêm chi tiết thông báo thứ 4
                new NotificationDetail
                {
                    NotificationDetailId = Guid.Parse("660d7300-f30c-40c3-b827-335544330173"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"),
                    NotificationId = Guid.Parse("660d7300-f30c-40c3-b827-335544330163"),
                    IsRead = false,
                }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37000"),
                    EventName = "Ladies Night",
                    Description = "Enjoy a night of exciting DJ performances.",
                    Images = "https://vietnamnightlife.com/uploads/images/2023/06/1685613730-single_product1-bambamladiesnight.png.webp",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37001"),
                    EventName = "Afterwork",
                    Description = "Join us for a fun-filled karaoke night.",
                    Images = "https://vietnamnightlife.com/uploads/images/2023/01/1673436242-single_product1-zionevent1.jpg.webp",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37002"),
                    EventName = "Fat Cash",
                    Description = "Learn how to make amazing cocktails with our expert mixologist.",
                    Images = "https://vietnamnightlife.com/uploads/images/2023/08/1692842361-single_product1-atmosbarevents1.png.webp",
                    IsEveryWeek = false,
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<TimeEvent>().HasData(
                new TimeEvent
                {
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37010"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37000"),
                    Date = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 5
                },
                new TimeEvent
                {
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37011"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37001"),
                    Date = new DateTimeOffset(2024, 2, 10, 0, 0, 0, TimeSpan.Zero),
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DayOfWeek = 6
                },
                new TimeEvent
                {
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37012"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37002"),
                    Date = null,
                    StartTime = new TimeSpan(15, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    DayOfWeek = 6
                },
                new TimeEvent
                {
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37013"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37000"),
                    Date = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero),
                    StartTime = new TimeSpan(21, 0, 0),
                    EndTime = new TimeSpan(23, 30, 0),
                    DayOfWeek = 5
                },
                new TimeEvent
                {
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37014"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37001"),
                    Date = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(22, 30, 0),
                    DayOfWeek = 1
                }
            );

            modelBuilder.Entity<BarEvent>().HasData(
                new BarEvent
                {
                    BarEventId = Guid.Parse("a3f23000-bd50-4f6a-9fa7-00bfc8d37020"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37000")
                },
                new BarEvent
                {
                    BarEventId = Guid.Parse("a3f23000-bd50-4f6a-9fa7-00bfc8d37021"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37001")
                },
                new BarEvent
                {
                    BarEventId = Guid.Parse("a3f23000-bd50-4f6a-9fa7-00bfc8d37022"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37002")
                },
                new BarEvent
                {
                    BarEventId = Guid.Parse("a3f23000-bd50-4f6a-9fa7-00bfc8d37023"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37000")
                },
                new BarEvent
                {
                    BarEventId = Guid.Parse("a3f23000-bd50-4f6a-9fa7-00bfc8d37024"),
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"),
                    EventId = Guid.Parse("f32e6f00-a9b8-4d39-b47e-00bfc8d37001")
                }
            );

            modelBuilder.Entity<EventVoucher>().HasData(
                new EventVoucher
                {
                    EventVoucherId = Guid.Parse("b3f23000-bd50-4f6a-9fa7-00bfc8d37030"),
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37010"),
                    EventVoucherName = "Ladies Night Discount",
                    VoucherCode = "DJNIGHT10",
                    Discount = 10,
                    MaxPrice = 100,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.Parse("b3f23000-bd50-4f6a-9fa7-00bfc8d37031"),
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37011"),
                    EventVoucherName = "Afterwork Deal",
                    VoucherCode = "KARAOKE20",
                    Discount = 20,
                    MaxPrice = 50,
                    Quantity = 30,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.Parse("b3f23000-bd50-4f6a-9fa7-00bfc8d37032"),
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37012"),
                    EventVoucherName = "Fat Cash Offer",
                    VoucherCode = "COCKTAIL15",
                    Discount = 15,
                    MaxPrice = 80,
                    Quantity = 20,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.Parse("b3f23000-bd50-4f6a-9fa7-00bfc8d37033"),
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37013"),
                    EventVoucherName = "Late Night DJ Discount",
                    VoucherCode = "LATEDJ10",
                    Discount = 10,
                    MaxPrice = 60,
                    Quantity = 40,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.Parse("b3f23000-bd50-4f6a-9fa7-00bfc8d37034"),
                    TimeEventId = Guid.Parse("e4f23000-bd50-4f6a-9fa7-00bfc8d37014"),
                    EventVoucherName = "Monday Karaoke Special",
                    VoucherCode = "MONKARAOKE5",
                    Discount = 5,
                    MaxPrice = 30,
                    Quantity = 25,
                    Status = true
                }
            );

            modelBuilder.Entity<Feedback>().HasData(
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440000"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440001"), // Neymar Jr
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Booking 1
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Bar Buddy 1
                    Rating = 5,
                    Comment = "Excellent experience! Will definitely come back.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440001"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440002"), // Lionel Messi
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Booking 2
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Bar Buddy 2
                    Rating = 4,
                    Comment = "Great atmosphere, but the service could be faster.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440002"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440003"), // Foden
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Booking 3
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Bar Buddy 3
                    Rating = 3,
                    Comment = "Good drinks, but the music was too loud for conversation.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440003"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"), // Mbappe
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Bar Buddy 3
                    Rating = 5,
                    Comment = "Loved the VIP service. Highly recommended!",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440004"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440001"), // Neymar Jr
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Booking 2
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // Bar Buddy 2
                    Rating = 4,
                    Comment = "Nice vibe, will return soon!",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440005"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440002"), // Lionel Messi
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Booking 1
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Bar Buddy 1
                    Rating = 5,
                    Comment = "Great cocktails and ambiance!",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440006"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440003"), // Foden
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), // Booking 4
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // Bar Buddy 3
                    Rating = 2,
                    Comment = "The experience was okay, but the place was too crowded.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                },
                new Feedback
                {
                    FeedbackId = Guid.Parse("770e8400-e29b-41d4-b716-446655440007"),
                    AccountId = Guid.Parse("550e8400-e29b-41d4-b777-446655440004"), // Mbappe
                    BookingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Booking 1
                    BarId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), // Bar Buddy 1
                    Rating = 3,
                    Comment = "Good drinks, but music selection was disappointing.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                }
            );

        }
    }
}