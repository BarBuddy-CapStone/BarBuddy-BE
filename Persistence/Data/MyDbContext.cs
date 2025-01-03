﻿using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        #region DbSet Properties
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
        public DbSet<Event> Events { get; set; }
        public DbSet<TimeEvent> TimeEvents { get; set; }
        public DbSet<EventVoucher> EventVouchers { get; set; }
        public DbSet<BarTime> BarTimes { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<FcmNotification> FcmNotifications { get; set; }
        public DbSet<FcmNotificationCustomer> FcmNotificationCustomers { get; set; }
        public DbSet<FcmUserDevice> FcmUserDevices { get; set; }
        public DbSet<BookingExtraDrink> BookingExtraDrinks { get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                optionsBuilder.UseMySql(
                    configuration.GetConnectionString("MyDB"),
                    new MySqlServerVersion(new Version(8, 0, 23))
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Seed independent entities first
            SeedIndependentEntities(modelBuilder);

            // 2. Seed dependent entities
            SeedDependentEntities(modelBuilder);

            // 3. Seed relationship entities
            SeedRelationshipEntities(modelBuilder);

            // FCM Configurations
            modelBuilder.Entity<FcmNotification>(entity =>
            {
                entity.ToTable("FcmNotifications");

                entity.HasIndex(e => new { e.Type, e.IsPublic });
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Bar)
                    .WithMany()
                    .HasForeignKey(e => e.BarId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FcmNotificationCustomer>(entity =>
            {
                entity.ToTable("FcmNotificationCustomers");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.NotificationId, e.DeviceToken })
                     .IsUnique();

                entity.HasOne(e => e.Notification)
                      .WithMany(n => n.NotificationCustomers)
                      .HasForeignKey(e => e.NotificationId);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .IsRequired(false);
            });

            modelBuilder.Entity<FcmUserDevice>(entity =>
            {
                entity.ToTable("FcmUserDevices");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.DeviceToken)
                     .IsUnique();

                entity.HasOne(e => e.Account)
                      .WithMany()
                      .HasForeignKey(e => e.AccountId)
                      .IsRequired(false);
            });
        }

        #region Seed Data Methods
        private void SeedIndependentEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(SeedData.GetRoles());
            modelBuilder.Entity<Bar>().HasData(SeedData.GetBars());
            modelBuilder.Entity<DrinkCategory>().HasData(SeedData.GetDrinkCategories());
            modelBuilder.Entity<EmotionalDrinkCategory>().HasData(SeedData.GetEmotionalDrinkCategories());
            modelBuilder.Entity<Event>().HasData(SeedData.GetEvents());
        }

        private void SeedDependentEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasData(SeedData.GetAccounts());
            var (tableTypes, tables) = SeedData.GetTablesData();
            modelBuilder.Entity<TableType>().HasData(tableTypes);
            modelBuilder.Entity<Table>().HasData(tables);
            modelBuilder.Entity<Drink>().HasData(SeedData.GetDrinks());
            modelBuilder.Entity<TimeEvent>().HasData(SeedData.GetTimeEvents());
            modelBuilder.Entity<BarTime>().HasData(SeedData.GetBarTimes());
        }

        private void SeedRelationshipEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>().HasData(SeedData.GetBookings());
            modelBuilder.Entity<BookingTable>().HasData(SeedData.GetBookingTables());
            modelBuilder.Entity<BookingDrink>().HasData(SeedData.GetBookingDrinks());
            modelBuilder.Entity<DrinkEmotionalCategory>().HasData(SeedData.GetDrinkEmotionalCategories());
            modelBuilder.Entity<EventVoucher>().HasData(SeedData.GetEventVouchers());
            modelBuilder.Entity<Feedback>().HasData(SeedData.GetFeedbacks());
        }
        #endregion

    }

    public static class Constants
    {
        public static class Ids
        {
            public static class Bars
            {
                public static readonly Guid Bar1 = Guid.NewGuid();
                public static readonly Guid Bar2 = Guid.NewGuid();
                public static readonly Guid Bar3 = Guid.NewGuid();
                public static readonly Guid Bar4 = Guid.NewGuid();
                public static readonly Guid Bar5 = Guid.NewGuid();
                public static readonly Guid Bar6 = Guid.NewGuid();
                public static readonly Guid Bar7 = Guid.NewGuid();
                public static readonly Guid Bar8 = Guid.NewGuid();
                public static readonly Guid Bar9 = Guid.NewGuid();
                public static readonly Guid Bar10 = Guid.NewGuid();
            }

            public static class EmotionalDrinkCategory
            {
                // Định nghĩa các ID cho các category cảm xúc
                public static readonly Guid vuiId = Guid.NewGuid();
                public static readonly Guid buonId = Guid.NewGuid();
                public static readonly Guid hanhPhucId = Guid.NewGuid();
                public static readonly Guid tucGianId = Guid.NewGuid();
                public static readonly Guid chanNanId = Guid.NewGuid();
                public static readonly Guid dangYeuId = Guid.NewGuid();
            }

            public static class Roles
            {
                public static readonly Guid Admin = Guid.Parse("b3b5a546-519d-411b-89d0-20c824e18d11");
                public static readonly Guid Manager = Guid.Parse("b3b5a546-519d-411b-89d0-20c824e18e22");
                public static readonly Guid Staff = Guid.Parse("a3438270-b7ed-4222-b3d8-aee52fc58805");
                public static readonly Guid Customer = Guid.Parse("70a545c0-6156-467c-a86f-547370ea4552");
            }

            public static class Accounts
            {
                public static readonly Guid AdminAccount = Guid.NewGuid();
                public static readonly Guid Manager1 = Guid.NewGuid();
                public static readonly Guid Manager2 = Guid.NewGuid();
                public static readonly Guid Manager3 = Guid.NewGuid();
                public static readonly Guid Manager4 = Guid.NewGuid();
                public static readonly Guid Manager5 = Guid.NewGuid();
                public static readonly Guid Manager6 = Guid.NewGuid();
                public static readonly Guid Manager7 = Guid.NewGuid();
                public static readonly Guid Manager8 = Guid.NewGuid();
                public static readonly Guid Manager9 = Guid.NewGuid();
                public static readonly Guid Manager10 = Guid.NewGuid();
                public static readonly Guid Staff1 = Guid.NewGuid();
                public static readonly Guid Staff2 = Guid.NewGuid();
                public static readonly Guid Staff3 = Guid.NewGuid();
                public static readonly Guid Staff4 = Guid.NewGuid();
                public static readonly Guid Staff5 = Guid.NewGuid();
                public static readonly Guid Staff6 = Guid.NewGuid();
                public static readonly Guid Staff7 = Guid.NewGuid();
                public static readonly Guid Staff8 = Guid.NewGuid();
                public static readonly Guid Staff9 = Guid.NewGuid();
                public static readonly Guid Staff10 = Guid.NewGuid();
                public static readonly Guid Customer1 = Guid.NewGuid();
                public static readonly Guid Customer2 = Guid.NewGuid();
                public static readonly Guid Tien = Guid.NewGuid();
            }

            public static class TableTypes
            {
                public static readonly Guid Svip = Guid.NewGuid();
                public static readonly Guid Vip = Guid.NewGuid();
                public static readonly Guid Standard1 = Guid.NewGuid();
                public static readonly Guid Standard2 = Guid.NewGuid();
                public static readonly Guid BarCounter = Guid.NewGuid();
            }

            public static class DrinkCategories
            {
                public static readonly Guid SoftDrink = Guid.NewGuid();
                public static readonly Guid Cocktail = Guid.NewGuid();
                public static readonly Guid Mocktail = Guid.NewGuid();
                public static readonly Guid Spirits = Guid.NewGuid();
                public static readonly Guid Beer = Guid.NewGuid();
                public static readonly Guid Wine = Guid.NewGuid();
                public static readonly Guid Tea = Guid.NewGuid();
                public static readonly Guid Coffee = Guid.NewGuid();
                public static readonly Guid Juice = Guid.NewGuid();
            }

            public static class Drinks
            {
                public static readonly Guid CocaCola = Guid.NewGuid();
                public static readonly Guid Mojito = Guid.NewGuid();
                public static readonly Guid TraDao = Guid.NewGuid();
                public static readonly Guid Pepsi = Guid.NewGuid();
                public static readonly Guid Screwdriver = Guid.NewGuid();
                public static readonly Guid BlackCoffee = Guid.NewGuid();
            }

            public static class Bookings
            {
                public static readonly Guid Booking1 = Guid.NewGuid();
                public static readonly Guid Booking2 = Guid.NewGuid();
                public static readonly Guid Booking3 = Guid.NewGuid();
                public static readonly Guid Booking4 = Guid.NewGuid();
                public static readonly Guid Booking5 = Guid.NewGuid();
                public static readonly Guid Booking6 = Guid.NewGuid();
            }

            public static class BookingTables
            {
                public static readonly Guid BookingTable1 = Guid.NewGuid();
                public static readonly Guid BookingTable2 = Guid.NewGuid();
                public static readonly Guid BookingTable3 = Guid.NewGuid();
                public static readonly Guid BookingTable4 = Guid.NewGuid();
                public static readonly Guid BookingTable5 = Guid.NewGuid();
                public static readonly Guid BookingTable6 = Guid.NewGuid();
                public static readonly Guid BookingTable7 = Guid.NewGuid();
                public static readonly Guid BookingTable8 = Guid.NewGuid();
                public static readonly Guid BookingTable9 = Guid.NewGuid();
                public static readonly Guid BookingTable10 = Guid.NewGuid();
                public static readonly Guid BookingTable11 = Guid.NewGuid();
                public static readonly Guid BookingTable12 = Guid.NewGuid();
                public static readonly Guid BookingTable13 = Guid.NewGuid();
                public static readonly Guid BookingTable14 = Guid.NewGuid();
            }

            public static class BookingDrinks
            {
                public static readonly Guid BookingDrink1 = Guid.NewGuid();
                public static readonly Guid BookingDrink2 = Guid.NewGuid();
                public static readonly Guid BookingDrink3 = Guid.NewGuid();
                public static readonly Guid BookingDrink4 = Guid.NewGuid();
            }

            public static class Notifications
            {
                public static readonly Guid Welcome = Guid.Parse("660d7300-f30c-40c3-b827-335544330160");
                public static readonly Guid Maintenance = Guid.Parse("660d7300-f30c-40c3-b827-335544330161");
                public static readonly Guid BookingConfirm = Guid.Parse("660d7300-f30c-40c3-b827-335544330162");
                public static readonly Guid HappyHour = Guid.Parse("660d7300-f30c-40c3-b827-335544330163");
                public static readonly Guid NewEvent = Guid.Parse("660d7300-f30c-40c3-b827-335544330164");
                public static readonly Guid SpecialOffer = Guid.Parse("660d7300-f30c-40c3-b827-335544330165");
            }

            public static class NotificationDetails
            {
                public static readonly Guid Welcome = Guid.Parse("660d7300-f30c-40c3-b827-335544330170");
                public static readonly Guid Maintenance = Guid.Parse("660d7300-f30c-40c3-b827-335544330171");
                public static readonly Guid BookingConfirm = Guid.Parse("660d7300-f30c-40c3-b827-335544330172");
                public static readonly Guid HappyHour = Guid.Parse("660d7300-f30c-40c3-b827-335544330173");
                public static readonly Guid NewEvent = Guid.Parse("660d7300-f30c-40c3-b827-335544330174");
                public static readonly Guid SpecialOffer = Guid.Parse("660d7300-f30c-40c3-b827-335544330175");
            }
            public static class Event
            {
                public static readonly Guid EventId = Guid.NewGuid();
                public static readonly Guid Event1 = Guid.NewGuid();
                public static readonly Guid Event2 = Guid.NewGuid();
                public static readonly Guid Event3 = Guid.NewGuid();
                public static readonly Guid Event4 = Guid.NewGuid();
                public static readonly Guid Event5 = Guid.NewGuid();
                public static readonly Guid Event6 = Guid.NewGuid();
                public static readonly Guid Event7 = Guid.NewGuid();
                public static readonly Guid Event8 = Guid.NewGuid();
                public static readonly Guid Event9 = Guid.NewGuid();
                public static readonly Guid Event10 = Guid.NewGuid();
                public static readonly Guid Event11 = Guid.NewGuid();
                public static readonly Guid Event12 = Guid.NewGuid();
                public static readonly Guid Event13 = Guid.NewGuid();
                public static readonly Guid Event14 = Guid.NewGuid();
                public static readonly Guid Event15 = Guid.NewGuid();
                public static readonly Guid Event16 = Guid.NewGuid();
                public static readonly Guid Event17 = Guid.NewGuid();
                public static readonly Guid Event18 = Guid.NewGuid();
                public static readonly Guid Event19 = Guid.NewGuid();
                public static readonly Guid Event20 = Guid.NewGuid();
                public static readonly Guid Event21 = Guid.NewGuid();
                public static readonly Guid Event22 = Guid.NewGuid();
                public static readonly Guid Event23 = Guid.NewGuid();
                public static readonly Guid Event24 = Guid.NewGuid();
                public static readonly Guid Event25 = Guid.NewGuid();
                public static readonly Guid Event26 = Guid.NewGuid();
                public static readonly Guid Event27 = Guid.NewGuid();
                public static readonly Guid Event28 = Guid.NewGuid();
                public static readonly Guid Event29 = Guid.NewGuid();
                public static readonly Guid Event30 = Guid.NewGuid();
            }
            public static class EventTime
            {
                public static readonly Guid EventTimeId = Guid.NewGuid();
                public static readonly Guid Time1 = Guid.NewGuid();
                public static readonly Guid Time2 = Guid.NewGuid();
                public static readonly Guid Time3 = Guid.NewGuid();
                public static readonly Guid Time4 = Guid.NewGuid();
                public static readonly Guid Time5 = Guid.NewGuid();
                public static readonly Guid Time6 = Guid.NewGuid();
                public static readonly Guid Time7 = Guid.NewGuid();
                public static readonly Guid Time8 = Guid.NewGuid();
                public static readonly Guid Time9 = Guid.NewGuid();
                public static readonly Guid Time10 = Guid.NewGuid();
                public static readonly Guid Time11 = Guid.NewGuid();
                public static readonly Guid Time12 = Guid.NewGuid();
                public static readonly Guid Time13 = Guid.NewGuid();
                public static readonly Guid Time14 = Guid.NewGuid();
                public static readonly Guid Time15 = Guid.NewGuid();
                public static readonly Guid Time16_1 = Guid.NewGuid();
                public static readonly Guid Time16_2 = Guid.NewGuid();
                public static readonly Guid Time17_1 = Guid.NewGuid();
                public static readonly Guid Time17_2 = Guid.NewGuid();
                public static readonly Guid Time17_3 = Guid.NewGuid();
                public static readonly Guid Time18_1 = Guid.NewGuid();
                public static readonly Guid Time19_1 = Guid.NewGuid();
                public static readonly Guid Time19_2 = Guid.NewGuid();
                public static readonly Guid Time20_1 = Guid.NewGuid();
                public static readonly Guid Time20_2 = Guid.NewGuid();
                public static readonly Guid Time20_3 = Guid.NewGuid();
                public static readonly Guid Time21 = Guid.NewGuid();
                public static readonly Guid Time22 = Guid.NewGuid();
                public static readonly Guid Time23 = Guid.NewGuid();
                public static readonly Guid Time24 = Guid.NewGuid();
                public static readonly Guid Time25 = Guid.NewGuid();
                public static readonly Guid Time26 = Guid.NewGuid();
                public static readonly Guid Time27 = Guid.NewGuid();
                public static readonly Guid Time28 = Guid.NewGuid();
                public static readonly Guid Time29 = Guid.NewGuid();
                public static readonly Guid Time30 = Guid.NewGuid();
            }
        }
    }

    public static class SeedData
    {
        public static Role[] GetRoles()
        {
            return new[]
            {
                new Role { RoleId = Constants.Ids.Roles.Admin, RoleName = "ADMIN" },
                new Role { RoleId = Constants.Ids.Roles.Manager, RoleName = "MANAGER" },
                new Role { RoleId = Constants.Ids.Roles.Staff, RoleName = "STAFF" },
                new Role { RoleId = Constants.Ids.Roles.Customer, RoleName = "CUSTOMER" }
            };
        }

        public static Bar[] GetBars()
        {
            return new[]
            {
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar1,
                    Address = "87A Hàm Nghi, Phường Nguyễn Thái Bình, Quận 1",
                    BarName = "Bar Buddy 1",
                    Description = "Tọa lạc trên tầng 14, tại 87A Hàm Nghi, quận 1, nằm ở khu vực trung tâm thành phố. Đến với Bar Buddy 1 để trải nghiệm sky bar \"HOT\" nhất Si Gòn hiện nay. Bar Buddy 1 được mệnh danh là địa điểm ăn chơi Sài Gòn xa hoa bậc nhất. Âm nhạc cuốn hút và vị ngon mê đắm của những đồ uống hảo hạng sẽ giúp bạn tận hưởng những phút giây thăng hoa. Những màn trình diễn đẳng cấp của các ca sĩ hàng đầu Việt Nam sẽ thổi bùng không khí khiến bạn không thể ngồi yên.",
                    Discount = 10,
                    Email = "contact@barbuddy1.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F878a5ef5-b291-4da1-97fa-59c8713e8979_pixelcut-export%20%282%29.jpeg?alt=media&token=8b06d065-1813-48a3-9675-96c2dff293c7, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F08173efa-639f-476e-be98-d9b699df18eb_pixelcut-export%20%281%29.jpeg?alt=media&token=70c72a88-5b77-4a24-a94f-5ce512882dc0, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2Fbbcac23a-8f7e-45de-8303-f88276ece893_pixelcut-export.jpeg?alt=media&token=3867cf44-e8e3-4721-bd99-abf79f74a727, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F200d05de-3f95-44da-8f2a-d0bc46b2df0c_1611797797-multi_product20-zionskyloungediningoverview3.jpg?alt=media&token=0d0441b6-8e30-42a4-9993-8b02def4955a, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F54a116b1-16b3-4b49-a327-c537938a90e8_1611797797-multi_product20-zionskyloungediningoverview4.jpg?alt=media&token=b988dc2c-8b78-4c3c-8517-603c61d347ee, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F10e39008-7fda-44cf-9ee1-f9f2b59b966c_1611797797-multi_product20-zionskyloungediningoverview10.jpg?alt=media&token=c632e2ca-39a3-495b-8fda-395904c45f40, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F43d3bdc0-4fe7-4b52-850d-cfb442eef9e2_1611797797-multi_product20-zionskyloungediningoverview7.jpg?alt=media&token=2f90ce2e-4fbd-4caa-8074-b1a7a3f6690c",
                    PhoneNumber = "0901234567",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar2,
                    Address = "153 Tôn Thất Đạm, Bến Nghé, quận 1, Hồ Chí Minh",
                    BarName = "Bar Buddy 2",
                    Description = "Tọa lạc ngay giữa trung tâm thành phố Hồ Chí Minh, Bar Buddy 2 với thiết kế độc đáo, lạ mắt, mang nét thu hút riêng như lạc vào thế giới của bộ phim hành động kinh dị: Mad Max. Một không gian nổi loạn và cực ngầu mang lại cảm giác quái lạ đầy bí ẩn. Bar Buddy 2 với sự đầu tư hoành tráng bằng những thiết bị, âm thanh, ánh sáng hiện đại nhất bạn sẽ được các DJ hàng đầu chiêu đãi cùng dàn khách mời đặc biệt: Hồ Ngọc Hà, Sơn Tùng M-TP, Erik.... chắc chắn khách hàng của Atmos sẽ luôn được tiếp đón và phục v tận tình",
                    Discount = 15,
                    Email = "contact@barbuddy2.com",
                    Images = "https://cdn.builder.io/api/v1/image/assets/TEMP/b69dbfb49c5c170f20ffc7e6592051456cbda4eb9cd86a4b5f73eacc4b286c1e?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b, https://cdn.builder.io/api/v1/image/assets/TEMP/2350aab32db981749c1dc25aae1418c736f1981bd711c5873409bb784d8ebec7?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b, https://cdn.builder.io/api/v1/image/assets/TEMP/0a3f53c7a6e75e85b31a7a33f12ad995bc789ec829cdda52f1d18feadabb4e1b?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b, https://cdn.builder.io/api/v1/image/assets/TEMP/0f65d417da4ce5fcefc12ad2b8eea620c32984dc13f70f6384c69537cf5362bc?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b, https://cdn.builder.io/api/v1/image/assets/TEMP/970bd0ad11008346e6a73f68eef329ce940423ef3fc8425cca3ef156a9b33fe5?placeholderIfAbsent=true&apiKey=2f0fb41b041549e2a3975f3618160d3b",
                    PhoneNumber = "0901234568",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar3,
                    Address = "264 Đ. Nam Kỳ Khởi Ngha, Phường 8, Quận 3",
                    BarName = "Bar Buddy 3",
                    Description = "Nằm tại trung tâm quận 3, Bar Buddy 3 là một trong những bar lâu năm nổi tiếng hàng đầu Sài Gòn. Không gian sang trọng nhiều vị trí đẹp và có sân khấu lớn ngay trung tâm. Sử dụng chất nhạc Vinahouse cực kỳ mạnh mẽ \"on trend\" là điều luôn hấp dẫn các vị khách tới Bar Buddy 3. Hơn thế nữa các sự kiện luôn diễn ra vào cuối tuần với dàn khách mời ca sĩ hàng đầu Việt Nam: Ưng Hoàng Phúc, Duy Mnh, Trịnh Tuấn Vỹ… Với slogan \"Nơi thể hiện đẳng cấp của bạn\" hãy sẵn sàng thể hiện bản thân tại Bar Buddy 3 bar.",
                    Discount = 20,
                    Email = "contact@barbuddy3.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2Fcafe615b-d5b6-442a-8c7c-484d73584863_1687155157-multi_product20-canalisclub9.jpg?alt=media&token=c6044e31-44f0-4441-86ce-bfb0153faa57, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F423f4310-bfb1-443a-ba1b-a9e6846318e3_1687155156-multi_product20-canalisclub1.jpg?alt=media&token=9774bcea-e8b0-4abe-8f78-11d549e0d248, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F41d7d7ad-669d-4fd2-a720-ae0d9d540904_1687155156-multi_product20-canalisclub2.jpg?alt=media&token=293977ac-56e4-4ac7-bc30-55b1cc711ee1, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F4ed2355a-1b27-48a8-a3d2-ebc90b1ddde6_1687155157-multi_product20-canalisclub3.jpg?alt=media&token=d9a34aed-3cc9-4c17-9f44-b0009c4590d1, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F3d8201c6-3b1a-45e8-b66e-c1a8b3825387_1687155157-multi_product20-canalisclub4.jpg?alt=media&token=464024d0-34af-4797-9f90-9fc666b6e230, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2Fe7e00e77-9b39-471b-bead-8cc1aa1e1cfe_1687155157-multi_product20-canalisclub5.jpg?alt=media&token=cf3dff10-2ff7-45ca-be7b-ef092d299cde",
                    PhoneNumber = "0901234569",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar4,
                    Address = "3C Đ. Tôn Đức Thắng, Bến Nghé, Quận 1, Thành phố Hồ Chí Minh",
                    BarName = "Bar Buddy 4",
                    Description = "Bar Buddy 4 sở hữu vị trí đắc địa trên tầng 2, khách sạn 5 sao Le Meridien trên đường Tôn Đức Thắng. Bar Buddy 4 sở hữu không gian \"dark bar\" không lẫn vào đâu được cùng phong cách Commas, với công nghệ laser light độc nhất vô nhị tại Hồ Chí Minh. Khách hàng sẽ có những giây phút bung xõa cùng những giai điệu Hip Hop tại Bar Buddy 4, tân binh mới nhất tại Nightlife Hồ Chí Minh.",
                    Discount = 25,
                    Email = "contact@barbuddy4.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F7eafab22-f11d-4e6d-b031-1e50844bc335_1673676803-multi_product20-boujeesaigonoverview6.jpg.webp?alt=media&token=81991d1a-289a-43c6-91d6-d8199fcdbe38, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2Fc3438817-7145-4f19-a87e-8723831581d3_1673676803-multi_product20-boujeesaigonoverview4.jpg.webp?alt=media&token=8af6376a-044c-4ef6-aaaf-47b69d94ffe3, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F99c327bf-e08e-4d1f-b404-546c4de57262_1673676803-multi_product20-boujeesaigonoverview3.jpg.webp?alt=media&token=07e09342-53da-4d0a-b088-53400246eb2d, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F343b5b07-0fec-4af9-80c8-f2755aa7b31b_1673676803-multi_product20-boujeesaigonoverview2.jpg.webp?alt=media&token=f7ea62f1-1da6-4c22-bd32-d27481cb6797, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F07bbd577-9ec0-4a44-965f-b02ce7fe3d57_1673676803-multi_product20-boujeesaigonoverview1.jpg.webp?alt=media&token=4be7209e-93ce-40cf-b8ca-67b68137317b, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F181f6a3c-dc43-4a37-9415-591d2c8c7fcc_pexels-pixabay-159291.jpg?alt=media&token=6b105d09-6b8f-40cd-bbe2-bc69a7c96d70",
                    PhoneNumber = "0901234570",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar5,
                    Address = "11 Đ.Nam Quốc Cang, Phường Phạm Ngũ Lão, Quận 1",
                    BarName = "Bar Buddy 5",
                    Description = "Quán bar kết hợp giữa nhạc sống và DJ.",
                    Discount = 5,
                    Email = "contact@barbuddy5.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F5e7b1e66-aff9-4e41-bebd-32cdee99efb2_1000000842_x4.png?alt=media&token=22bab908-654c-4ff1-a5bd-8d61cbd183d1, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F56b4ae09-67e4-4b00-b047-5a6375dc8bf2_1000000844_x4.png?alt=media&token=6bd25454-6b06-4155-ae92-a7af21b3c9df, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F033552c8-759d-40b0-a4eb-db33656b3a4b_1000000845_x4.png?alt=media&token=188f25c3-29bc-4f50-8840-ba16cb186f4b, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2Fdcc94305-8d8e-44f7-919c-3767ee81453e_1000000843_x4.png?alt=media&token=25d26056-752d-4080-b7ea-8296cd31040e, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2Fcda916e7-6ad5-4cea-9c13-4c406884f098_1000000846_x4.png?alt=media&token=d0ba9e55-72d6-49c7-9fda-4908e7d48ce3, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/images%2F14439ef9-899e-4438-9234-d3df75d0e4d9_pexels-pixabay-63633.jpg?alt=media&token=898cf18e-a22e-461b-9c7c-fb4e04a8b9d0",
                    PhoneNumber = "0901234571",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar6,
                    Address = "41 Nam Kỳ Khởi Nghĩa, Phường Nguyễn Thái Bình, Quận 1, Hồ Chí Minh",
                    BarName = "Bar Buddy 6",
                    Description = "Không gian thoải mái với nhiều trò chơi giải trí.",
                    Discount = 10,
                    Email = "contact@barbuddy6.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2Fe069d8ab-2d19-4c73-95e6-487a269a5104_1580805657-multi_product20-bambamoverview3.jpg.webp?alt=media&token=295ab199-824c-426b-a6e4-701de996a783, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2Fa5c2b7b2-d52c-4f43-bc66-6c2e5fa37e4f_1580805657-multi_product20-bambamoverview4.jpg.webp?alt=media&token=66deb9dc-077c-4863-964c-834405652085",
                    PhoneNumber = "0901234572",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar7,
                    Address = "20 Đ. Nguyễn Công Trứ, Phường Nguyễn Thái Bình, Quận 1",
                    BarName = "Bar Buddy 7",
                    Description = "Nơi hội tụ của những tâm hồn yêu thích âm nhạc.",
                    Discount = 30,
                    Email = "contact@barbuddy7.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F8bfa6be1-66dc-42f6-b7ab-872d2c8cfd8c_1592203656-multi_product20-lushsaigonoverview5.jpg.webp?alt=media&token=b1e24062-a71b-4a87-b606-8f4434eebc15, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2Fac71e678-83c0-4e51-b6b6-feec37cecaae_1592203656-multi_product20-lushsaigonoverview4.jpg.webp?alt=media&token=609ca44c-2d54-4b30-b6fe-5e8f76a76b7d",
                    PhoneNumber = "0901234573",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar8,
                    Address = "120 Đ. Nguyễn Huệ, Bến Nghé, Quận 1",
                    BarName = "Bar Buddy 8",
                    Description = "Quán bar rooftop với tầm nhìn đp.",
                    Discount = 20,
                    Email = "contact@barbuddy8.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2Fdfe78a6c-d99d-45f5-8d39-4e5564474760_1592206636-multi_product20-khu13overview6.jpg.webp?alt=media&token=83337472-0840-4370-a116-1b9a281ecfbf, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F6f8a9b89-23ac-4dea-8fa2-cd66080047e4_1592206636-multi_product20-khu13overview5.jpg.webp?alt=media&token=a3f302b2-2998-42fd-b3a0-de3c018e51c4",
                    PhoneNumber = "0901234574",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar9,
                    Address = "30 Đ. Tôn Thất Tùng, Quận 1",
                    BarName = "Bar Buddy 9",
                    Description = "Quán bar dành cho các tín đồ yêu thích craft beer.",
                    Discount = 15,
                    Email = "contact@barbuddy9.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2Fcbf97b22-afbb-42d9-9c1e-374df168ec2f_1664637430-multi_product20-1111clubsaigonoverview2.jpg.webp?alt=media&token=d6ead89e-4d3f-45a8-ac6e-5725cee8dcb1, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F842406ad-f821-44ec-aa15-7423bde20d98_1664637430-multi_product20-1111clubsaigonoverview1.jpg.webp?alt=media&token=e040cea7-11a6-4f48-ae94-a52729f0405c",
                    PhoneNumber = "0901234575",
                    Status = true,
                    TimeSlot = 1
                },
                new Bar
                {
                    BarId = Constants.Ids.Bars.Bar10,
                    Address = "25 Đ. Lê Duẩn, Quận 1",
                    BarName = "Bar Buddy 10",
                    Description = "Không gian ấm cúng với các loại cocktail độc đáo.",
                    Discount = 10,
                    Email = "contact@barbuddy10.com",
                    Images = "https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F432a674c-1b16-47df-a0dc-396d81b3327b_1700112227-multi_product20-districtk2.jpg.webp?alt=media&token=6ae45a8c-84fe-40a0-943e-6bbcc7871925, https://firebasestorage.googleapis.com/v0/b/barbuddy-b2026.appspot.com/o/other%2F9b9395a9-becb-4d13-bad4-406b83edd888_1700112227-multi_product20-districtk1.jpg.webp?alt=media&token=472d0416-0c61-4f6c-bbcd-793b554f0604",
                    PhoneNumber = "0901234576",
                    Status = true,
                    TimeSlot = 1,
                }
            };
        }

        public static Account[] GetAccounts()
        {
            return new[]
            {
                new Account
                {
                    AccountId = Constants.Ids.Accounts.AdminAccount,
                    BarId = null,
                    RoleId = Constants.Ids.Roles.Admin,
                    Email = "admin1@barbuddy.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Lionel Messi",
                    Dob = new DateTime(1980, 5, 1),
                    Phone = "0901234567",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://cdn.santino.com.vn/storage/upload/news/2023/07/Lionel-Messi-13.webp",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager1,
                    BarId = Constants.Ids.Bars.Bar1,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager1@barbuddy1.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "William",
                    Dob = new DateTime(1980, 1, 1),
                    Phone = "0901234577",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager2,
                    BarId = Constants.Ids.Bars.Bar2,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager2@barbuddy2.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Jack97",
                    Dob = new DateTime(1981, 2, 2),
                    Phone = "0901234578",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager3,
                    BarId = Constants.Ids.Bars.Bar3,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager3@barbuddy3.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Son tung",
                    Dob = new DateTime(1982, 3, 3),
                    Phone = "0901234579",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager4,
                    BarId = Constants.Ids.Bars.Bar4,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager4@barbuddy4.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Ten Hag",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager5,
                    BarId = Constants.Ids.Bars.Bar5,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Poseidon",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager6,
                    BarId = Constants.Ids.Bars.Bar6,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager6@barbuddy6.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Zeus",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager7,
                    BarId = Constants.Ids.Bars.Bar7,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager7@barbuddy7.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "7 Vap",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager8,
                    BarId = Constants.Ids.Bars.Bar8,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager8@barbuddy8.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Eric",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager9,
                    BarId = Constants.Ids.Bars.Bar9,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager9@barbuddy9.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Manchester",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Manager10,
                    BarId = Constants.Ids.Bars.Bar10,
                    RoleId = Constants.Ids.Roles.Manager,
                    Email = "manager10@barbuddy10.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Chi Dan",
                    Dob = new DateTime(1983, 4, 4),
                    Phone = "0901234580",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "default",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff1,
                    BarId = Constants.Ids.Bars.Bar1,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff1@barbuddy1.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Neymar Jr",
                    Dob = new DateTime(1992, 7, 15),
                    Phone = "0901234568",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://dailytrust.com/wp-content/uploads/2022/12/Neymar.jpg",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff2,
                    BarId = Constants.Ids.Bars.Bar2,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff2@barbuddy2.com",
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
                    AccountId = Constants.Ids.Accounts.Staff3,
                    BarId = Constants.Ids.Bars.Bar3,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff3@barbuddy3.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Antony",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fm.youtube.com%2Fwatch%3Fv%3D_mPDAQm58i8&psig=AOvVaw0zud8SdC0L84Ao4FyvJ9VC&ust=1731386997368000&source=images&cd=vfe&opi=89978449&ved=0CBAQjRxqFwoTCOCq_bC904kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff4,
                    BarId = Constants.Ids.Bars.Bar4,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff4@barbuddy4.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Jesus",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Ftwitter.com%2Fthefootyarena%2Fstatus%2F1427172222720958467&psig=AOvVaw0Hz6eyBGjK10VPztHZDsxD&ust=1731387055116000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCIDv-cm904kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff5,
                    BarId = Constants.Ids.Bars.Bar5,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Maguire",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.pinterest.com%2Fpin%2Fharry-maguire-meme--394065036155577538%2F&psig=AOvVaw0r2-KTF3CHqD3MPpY5RsGF&ust=1731387310547000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCOjE8sC-04kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff6,
                    BarId = Constants.Ids.Bars.Bar6,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Maguire",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.pinterest.com%2Fpin%2Fharry-maguire-meme--394065036155577538%2F&psig=AOvVaw0r2-KTF3CHqD3MPpY5RsGF&ust=1731387310547000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCOjE8sC-04kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff7,
                    BarId = Constants.Ids.Bars.Bar7,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Maguire",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.pinterest.com%2Fpin%2Fharry-maguire-meme--394065036155577538%2F&psig=AOvVaw0r2-KTF3CHqD3MPpY5RsGF&ust=1731387310547000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCOjE8sC-04kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff8,
                    BarId = Constants.Ids.Bars.Bar8,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Maguire",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.pinterest.com%2Fpin%2Fharry-maguire-meme--394065036155577538%2F&psig=AOvVaw0r2-KTF3CHqD3MPpY5RsGF&ust=1731387310547000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCOjE8sC-04kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff9,
                    BarId = Constants.Ids.Bars.Bar9,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Maguire",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.pinterest.com%2Fpin%2Fharry-maguire-meme--394065036155577538%2F&psig=AOvVaw0r2-KTF3CHqD3MPpY5RsGF&ust=1731387310547000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCOjE8sC-04kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Staff10,
                    BarId = Constants.Ids.Bars.Bar10,
                    RoleId = Constants.Ids.Roles.Staff,
                    Email = "staff5@barbuddy5.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Maguire",
                    Dob = new DateTime(1990, 3, 20),
                    Phone = "0901234569",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.pinterest.com%2Fpin%2Fharry-maguire-meme--394065036155577538%2F&psig=AOvVaw0r2-KTF3CHqD3MPpY5RsGF&ust=1731387310547000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCOjE8sC-04kDFQAAAAAdAAAAABAE",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BarId = null,
                    RoleId = Constants.Ids.Roles.Customer,
                    Email = "customer1@barbuddy.com",
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
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BarId = null,
                    RoleId = Constants.Ids.Roles.Customer,
                    Email = "customer3@barbuddy.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Vinicius Jr",
                    Dob = new DateTime(1994, 12, 5),
                    Phone = "0901234575",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://th.bing.com/th/id/OIP.SO1-rUp8nmpmQIHFf60hYgHaEK?rs=1&pid=ImgDetMain",
                    Status = 1
                },
                new Account
                {
                    AccountId = Constants.Ids.Accounts.Tien,
                    BarId = null,
                    RoleId = Constants.Ids.Roles.Customer,
                    Email = "tientn@email.com",
                    Password = "2757cb3cafc39af451abb2697be79b4ab61d63d74d85b0418629de8c26811b529f3f3780d0150063ff55a2beee74c4ec102a2a2731a1f1f7f10d473ad18a6a87",
                    Fullname = "Mr.Tien",
                    Dob = new DateTime(2002, 12, 5),
                    Phone = "0912345678",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Image = "https://cdn-icons-png.flaticon.com/512/4862/4862440.png",
                    Status = 1
                }
            };
        }

        public static DrinkCategory[] GetDrinkCategories()
        {
            return new[]
            {
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.SoftDrink,
                    DrinksCategoryName = "Nước ngọt",
                    Description = "Đồ uống khng cồn như soda, nước ngọt có ga, và nước ngọt có hương vị.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinksCategoryName = "Cocktail",
                    Description = "Đồ uống pha trộn thường chứa cồn, kết hợp với nước trái cây, soda hoặc các nguyên liệu khác.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Mocktail,
                    DrinksCategoryName = "Mocktail",
                    Description = "Phiên bản không cồn của các loại cocktail, phù hợp cho những người không uống rượu.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Spirits,
                    DrinksCategoryName = "Rượu mạnh",
                    Description = "Đồ uống có cồn mạnh như vodka, whisky, gin, rum, v.v.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Beer,
                    DrinksCategoryName = "Bia",
                    Description = "Đồ uống có cồn được ủ từ lúa mạch, hoa bia và nước. Có nhiều loại khác nhau như lager, ale, stout.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Wine,
                    DrinksCategoryName = "Rượu vang",
                    Description = "Đồ uống có cồn được làm từ nho lên men, có nhiều loại như vang đỏ, vang trắng và vang hồng.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Tea,
                    DrinksCategoryName = "Trà",
                    Description = "Đồ uống nóng hoặc lạnh được pha từ lá trà, có nhiều loại như trà đen, trà xanh và trà thảo mộc.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Coffee,
                    DrinksCategoryName = "Cà phê",
                    Description = "Đồ uống nóng hoặc lạnh đưc pha từ hạt cà phê rang, bao gồm espresso, cappuccino, latte và nhiều loại khác.",
                    IsDeleted = false
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Constants.Ids.DrinkCategories.Juice,
                    DrinksCategoryName = "Nước ép",
                    Description = "Đồ uống tự nhiên được làm từ nước ép trái cây hoặc rau củ. Các loi phổ biến gồm nước cam, nước táo, và nước ép cà rốt.",
                    IsDeleted = false
                }
            };
        }

        public static EmotionalDrinkCategory[] GetEmotionalDrinkCategories()
        {

            return new[]
            {
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Constants.Ids.EmotionalDrinkCategory.vuiId,
                    CategoryName = "Vui vẻ",
                    Description = "Các loại đồ uống phù hợp cho tâm trạng vui vẻ, hứng khởi",
                    IsDeleted = false,
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Constants.Ids.EmotionalDrinkCategory.buonId,
                    CategoryName = "Buồn",
                    Description = "Các loại đồ uống giúp xoa dịu tâm trạng buồn bã",
                    IsDeleted = false,
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Constants.Ids.EmotionalDrinkCategory.hanhPhucId,
                    CategoryName = "Hạnh phúc",
                    Description = "Các loại đồ uống để chia sẻ niềm vui và hạnh phúc",
                    IsDeleted = false,
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Constants.Ids.EmotionalDrinkCategory.tucGianId,
                    CategoryName = "Tức giận",
                    Description = "Các loại đồ uống giúp làm dịu cơn giận",
                    IsDeleted = false,
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Constants.Ids.EmotionalDrinkCategory.chanNanId,
                    CategoryName = "Chán nản",
                    Description = "Các loại đồ uống giúp cải thiện tâm trạng chán nản",
                    IsDeleted = false,
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Constants.Ids.EmotionalDrinkCategory.dangYeuId,
                    CategoryName = "Đang yêu",
                    Description = "Các loại đồ uống lãng mạn cho những người đang yu",
                    IsDeleted = false,
                }
            };
        }

        public static Event[] GetEvents()
        {
            return new[]
            {
                new Event
                {
                    EventId = Constants.Ids.Event.Event1,
                    BarId = Constants.Ids.Bars.Bar1,
                    EventName = "Happy Hour Friday",
                    Description = "Giảm giá 30% tất cả đồ uống từ 18h-20h mỗi thứ 6",
                    Images = "https://vietnamnightlife.com/uploads/images/2020/02/1581867705-single_product1-chillskybarhappyhour.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event2,
                    BarId = Constants.Ids.Bars.Bar2,
                    EventName = "Live Music Night",
                    Description = "Đêm nhạc acoustic với các nghệ sĩ nổi tiếng",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event3,
                    BarId = Constants.Ids.Bars.Bar3,
                    EventName = "Ladies Night",
                    Description = "Miễn phí đồ uống đầu tiên cho phái nữ",
                    Images = "https://vietnamnightlife.com/uploads/images/2023/06/1685613730-single_product1-bambamladiesnight.png.webp",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event4,
                    BarId = Constants.Ids.Bars.Bar4,
                    EventName = "Cocktail Masterclass",
                    Description = "Học cách pha chế cocktail cùng bartender chuyên nghiệp",
                    Images = "https://familyapp.com/wp-content/uploads/2021/07/summer_cocktails.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event5,
                    BarId = Constants.Ids.Bars.Bar5,
                    EventName = "Wine Tasting Night",
                    Description = "Thưởng thức các loại rượu vang đặc sắc từ khắp thế giới",
                    Images = "https://www.foodandwine.com/thmb/i3ppax3dzfKhybD3SuxsZSVHW9E=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/the-antler-room-natural-wine-bars-FT-SS1018-c1625c58d9aa4fd08c0216b6a84a6863.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event6,
                    BarId = Constants.Ids.Bars.Bar1,
                    EventName = "Jazz Night",
                    Description = "Đêm nhạc Jazz với các nghệ sĩ hàng đầu",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event7,
                    BarId = Constants.Ids.Bars.Bar2,
                    EventName = "Singles Mixer",
                    Description = "Tối giao lưu dành cho người độc thân",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event8,
                    BarId = Constants.Ids.Bars.Bar3,
                    EventName = "Karaoke Contest",
                    Description = "Cuộc thi karaoke với giải thưởng hấp dẫn",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event9,
                    BarId = Constants.Ids.Bars.Bar4,
                    EventName = "Beer Festival",
                    Description = "Lễ hội bia với nhiều loại bia thủ công",
                    Images = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRkx-FWT-R5lCzsm7Zz0cNKX1WH6gtuVakaYg&s",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event10,
                    BarId = Constants.Ids.Bars.Bar5,
                    EventName = "Salsa Night",
                    Description = "Đêm khiêu vũ Salsa sôi động",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event11,
                    BarId = Constants.Ids.Bars.Bar1,
                    EventName = "Neon Party Night",
                    Description = "Đêm tiệc sôi động với chủ đề Neon, dress code: Neon colors. Tặng ngay 1 vòng tay phát sáng cho 100 khách đầu tiên. DJ nổi tiếng và vũ công LED performance.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg", // Multiple images
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event12,
                    BarId = Constants.Ids.Bars.Bar2,
                    EventName = "Bartender Battle",
                    Description = "Cuộc thi pha chế đỉnh cao giữa các bartender hàng đầu Sài Gòn. Khách tham dự được thưởng thức miễn phí các cocktail độc đáo và vote cho bartender yêu thích.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event13,
                    BarId = Constants.Ids.Bars.Bar3,
                    EventName = "Sunday Brunch & Jazz",
                    Description = "Brunch sang trọng với âm nhạc Jazz mộc mạc. Set menu brunch đặc biệt kèm free flow rượu vang và cocktail trong 2 giờ.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true, // Sự kiện hàng tuần vào Chủ nhật
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event14,
                    BarId = Constants.Ids.Bars.Bar4,
                    EventName = "Midnight Mystery",
                    Description = "Sự kiện bí ẩn diễn ra vào lúc nửa đêm. Dress code: Black & White. Menu đồ uống được giữ bí mật cho đến phút chót.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event15,
                    BarId = Constants.Ids.Bars.Bar5,
                    EventName = "Full Moon Party",
                    Description = "Tiệc trăng tròn trên tầng thượng với DJ quốc tế. Fire show và múa lửa. Tặng body painting cho khách tham dự.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg", // Multiple images
                    IsEveryWeek = false, // Chỉ diễn ra vào đêm trăng tròn
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event16,
                    BarId = Constants.Ids.Bars.Bar1,
                    EventName = "Rock Band Night",
                    Description = "Đêm nhạc rock với ban nhạc Underground nổi tiếng. Âm thanh sống động và không khí cuồng nhiệt.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event17,
                    BarId = Constants.Ids.Bars.Bar2,
                    EventName = "Tequila Festival",
                    Description = "Lễ hội Tequila với đa dạng hương vị từ Mexico. Workshop pha chế và thưởng thức Tequila.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event18,
                    BarId = Constants.Ids.Bars.Bar3,
                    EventName = "Stand-up Comedy",
                    Description = "Đêm hài độc thoại với các nghệ sĩ hài hàng đầu.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event19,
                    BarId = Constants.Ids.Bars.Bar4,
                    EventName = "Retro Disco Night",
                    Description = "Quay ngược thời gian với nhạc Disco những năm 80s. Dress code: Retro style.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event20,
                    BarId = Constants.Ids.Bars.Bar5,
                    EventName = "Halloween Special",
                    Description = "Đêm hội Halloween với trang trí ma quái, cuộc thi hóa trang và cocktail đặc biệt.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event21,
                    BarId = Constants.Ids.Bars.Bar1,
                    EventName = "Summer Beach Party",
                    Description = "Bữa tiệc bãi biển mùa hè với DJ quốc tế, vũ công và cocktail đặc biệt.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event22,
                    BarId = Constants.Ids.Bars.Bar2,
                    EventName = "Latin Dance Night",
                    Description = "Đêm khiêu vũ Latin sôi động với các điệu Salsa, Bachata và Merengue.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event23,
                    BarId = Constants.Ids.Bars.Bar3,
                    EventName = "Whiskey Tasting Experience",
                    Description = "Trải nghiệm nếm thử whiskey từ khắp nơi trên thế giới với chuyên gia.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event24,
                    BarId = Constants.Ids.Bars.Bar4,
                    EventName = "EDM Explosion",
                    Description = "Đêm nhạc điện tử bùng nổ với các DJ hàng đầu trong nước.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event25,
                    BarId = Constants.Ids.Bars.Bar5,
                    EventName = "Acoustic Coffee Night",
                    Description = "Đêm nhạc acoustic ấm cúng với cà phê và bánh ngọt.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event26,
                    BarId = Constants.Ids.Bars.Bar1,
                    EventName = "Mixology Workshop",
                    Description = "Workshop pha chế cocktail cùng các bartender chuyên nghiệp.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event27,
                    BarId = Constants.Ids.Bars.Bar2,
                    EventName = "Wine & Jazz",
                    Description = "Thưởng thức rượu vang và nhạc Jazz trong không gian sang trọng.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event28,
                    BarId = Constants.Ids.Bars.Bar3,
                    EventName = "Singles Mixer",
                    Description = "Đêm giao lưu dành cho người độc thân với các trò chơi thú vị.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event29,
                    BarId = Constants.Ids.Bars.Bar4,
                    EventName = "Retro Game Night",
                    Description = "Đêm chơi game retro với các máy game cổ điển và cocktail theo chủ đề.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = true,
                    IsDeleted = false
                },
                new Event
                {
                    EventId = Constants.Ids.Event.Event30,
                    BarId = Constants.Ids.Bars.Bar5,
                    EventName = "Poetry & Drinks",
                    Description = "Đêm thơ và đồ uống với các nhà thơ địa phương.",
                    Images = "https://media.self.com/photos/5e70f72443731c000882cfe7/4:3/w_2560%2Cc_limit/GettyImages-125112134.jpg",
                    IsEveryWeek = false,
                    IsDeleted = false
                }
            };
        }

        public static TimeEvent[] GetTimeEvents()
        {
            return new[]
            {
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time1,
                    EventId = Constants.Ids.Event.Event1,
                    Date = null,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(20, 0, 0),
                    DayOfWeek = 5
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time2,
                    EventId = Constants.Ids.Event.Event2,
                    Date = DateTimeOffset.Now.AddDays(7),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(22, 30, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time3,
                    EventId = Constants.Ids.Event.Event3,
                    Date = null,
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 4
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time4,
                    EventId = Constants.Ids.Event.Event4,
                    Date = null,
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(21, 0, 0),
                    DayOfWeek = 2
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time5,
                    EventId = Constants.Ids.Event.Event5,
                    Date = DateTimeOffset.Now.AddDays(14),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time6,
                    EventId = Constants.Ids.Event.Event6,
                    Date = null,
                    StartTime = new TimeSpan(20, 30, 0),
                    EndTime = new TimeSpan(23, 30, 0),
                    DayOfWeek = 6
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time7,
                    EventId = Constants.Ids.Event.Event7,
                    Date = DateTimeOffset.Now.AddDays(21),
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time8,
                    EventId = Constants.Ids.Event.Event8,
                    Date = DateTimeOffset.Now.AddDays(10),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(22, 30, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time9,
                    EventId = Constants.Ids.Event.Event9,
                    Date = DateTimeOffset.Now.AddDays(30),
                    StartTime = new TimeSpan(16, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time10,
                    EventId = Constants.Ids.Event.Event10,
                    Date = null,
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 3
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time11,
                    EventId = Constants.Ids.Event.Event11,
                    Date = DateTimeOffset.Now.AddDays(15), // Specific date
                    StartTime = new TimeSpan(21, 0, 0), // 9 PM
                    EndTime = new TimeSpan(4, 0, 0), // 4 AM next day
                    DayOfWeek = null // One-time event
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time12,
                    EventId = Constants.Ids.Event.Event12,
                    Date = DateTimeOffset.Now.AddDays(25),
                    StartTime = new TimeSpan(18, 30, 0), // 6:30 PM
                    EndTime = new TimeSpan(23, 30, 0), // 11:30 PM
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time13,
                    EventId = Constants.Ids.Event.Event13,
                    Date = null,
                    StartTime = new TimeSpan(11, 0, 0), // 11 AM
                    EndTime = new TimeSpan(15, 0, 0), // 3 PM
                    DayOfWeek = 0 // Every Sunday
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time14,
                    EventId = Constants.Ids.Event.Event14,
                    Date = DateTimeOffset.Now.AddDays(13),
                    StartTime = new TimeSpan(23, 59, 0), // 11:59 PM
                    EndTime = new TimeSpan(5, 0, 0), // 5 AM next day
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time15,
                    EventId = Constants.Ids.Event.Event15,
                    Date = DateTimeOffset.Now.AddMonths(1).Date.AddDays(15), // Next full moon
                    StartTime = new TimeSpan(20, 0, 0), // 8 PM
                    EndTime = new TimeSpan(5, 0, 0), // 5 AM next day
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time16_1,
                    EventId = Constants.Ids.Event.Event16,
                    Date = DateTimeOffset.Now.AddDays(20),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time16_2,
                    EventId = Constants.Ids.Event.Event16,
                    Date = DateTimeOffset.Now.AddDays(21),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time17_1,
                    EventId = Constants.Ids.Event.Event17,
                    Date = DateTimeOffset.Now.AddDays(30),
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time17_2,
                    EventId = Constants.Ids.Event.Event17,
                    Date = DateTimeOffset.Now.AddDays(31),
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time17_3,
                    EventId = Constants.Ids.Event.Event17,
                    Date = DateTimeOffset.Now.AddDays(32),
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time18_1,
                    EventId = Constants.Ids.Event.Event18,
                    Date = null,
                    StartTime = new TimeSpan(21, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 3 // Thứ 4 hàng tuần
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time19_1,
                    EventId = Constants.Ids.Event.Event19,
                    Date = null,
                    StartTime = new TimeSpan(21, 0, 0),
                    EndTime = new TimeSpan(1, 0, 0),
                    DayOfWeek = 5 // Thứ 6
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time19_2,
                    EventId = Constants.Ids.Event.Event19,
                    Date = null,
                    StartTime = new TimeSpan(21, 0, 0),
                    EndTime = new TimeSpan(1, 0, 0),
                    DayOfWeek = 6 // Thứ 7
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time20_1,
                    EventId = Constants.Ids.Event.Event20,
                    Date = DateTimeOffset.Parse("2024-10-29"),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(2, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time20_2,
                    EventId = Constants.Ids.Event.Event20,
                    Date = DateTimeOffset.Parse("2024-10-30"),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(2, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time20_3,
                    EventId = Constants.Ids.Event.Event20,
                    Date = DateTimeOffset.Parse("2024-10-31"),
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(4, 0, 0), // Halloween night kéo dài hơn
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time21,
                    EventId = Constants.Ids.Event.Event21,
                    Date = DateTimeOffset.Now.AddDays(30),
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time22,
                    EventId = Constants.Ids.Event.Event22,
                    Date = null,
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 5 // Thứ 6
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time23,
                    EventId = Constants.Ids.Event.Event23, // ID của Whiskey Tasting
                    Date = DateTimeOffset.Now.AddDays(15),
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time24,
                    EventId = Constants.Ids.Event.Event24, // ID của EDM Explosion
                    Date = null,
                    StartTime = new TimeSpan(22, 0, 0),
                    EndTime = new TimeSpan(4, 0, 0),
                    DayOfWeek = 6 // Thứ 7
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time25,
                    EventId = Constants.Ids.Event.Event25, // ID của Acoustic Coffee Night
                    Date = null,
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(22, 30, 0),
                    DayOfWeek = 3 // Thứ 4
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time26,
                    EventId = Constants.Ids.Event.Event26, // ID của Mixology Workshop
                    Date = DateTimeOffset.Now.AddDays(20),
                    StartTime = new TimeSpan(15, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time27,
                    EventId = Constants.Ids.Event.Event27, // ID của Wine & Jazz
                    Date = null,
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 5 // Thứ 6
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time28,
                    EventId = Constants.Ids.Event.Event28, // ID của Singles Mixer
                    Date = DateTimeOffset.Now.AddDays(25),
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = null
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time29,
                    EventId = Constants.Ids.Event.Event29, // ID của Retro Game Night
                    Date = null,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    DayOfWeek = 4 // Thứ 5
                },
                new TimeEvent
                {
                    TimeEventId = Constants.Ids.EventTime.Time30,
                    EventId = Constants.Ids.Event.Event30, // ID của Poetry & Drinks
                    Date = DateTimeOffset.Now.AddDays(10),
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(22, 30, 0),
                    DayOfWeek = null
                }
            };
        }

        public static EventVoucher[] GetEventVouchers()
        {
            return new[]
            {
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event1,
                    EventVoucherName = "Happy Hour 30% Off",
                    VoucherCode = "HAPPY30",
                    Discount = 30,
                    MaxPrice = 200000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event2,
                    EventVoucherName = "Live Music Special",
                    VoucherCode = "LIVE20",
                    Discount = 20,
                    MaxPrice = 150000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event3,
                    EventVoucherName = "Ladies Night Free Drink",
                    VoucherCode = "LADIES100",
                    Discount = 100,
                    MaxPrice = 100000,
                    Quantity = 200,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event4,
                    EventVoucherName = "Masterclass Special",
                    VoucherCode = "MASTER40",
                    Discount = 40,
                    MaxPrice = 300000,
                    Quantity = 30,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event5,
                    EventVoucherName = "Wine Night",
                    VoucherCode = "WINE25",
                    Discount = 25,
                    MaxPrice = 500000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event6,
                    EventVoucherName = "Jazz Special",
                    VoucherCode = "JAZZ20",
                    Discount = 20,
                    MaxPrice = 200000,
                    Quantity = 80,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event7,
                    EventVoucherName = "Singles Night",
                    VoucherCode = "SINGLE50",
                    Discount = 50,
                    MaxPrice = 250000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event8,
                    EventVoucherName = "Karaoke Deal",
                    VoucherCode = "KARA35",
                    Discount = 35,
                    MaxPrice = 300000,
                    Quantity = 60,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event9,
                    EventVoucherName = "Beer Fest Special",
                    VoucherCode = "BEER45",
                    Discount = 45,
                    MaxPrice = 400000,
                    Quantity = 200,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event10,
                    EventVoucherName = "Salsa Night",
                    VoucherCode = "SALSA30",
                    Discount = 30,
                    MaxPrice = 150000,
                    Quantity = 70,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event11,
                    EventVoucherName = "Neon Party VIP Access",
                    VoucherCode = "NEONVIP",
                    Discount = 100, // 100% discount
                    MaxPrice = 1000000, // Giá trị cao cho VIP access
                    Quantity = 20, // Limited quantity
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event12,
                    EventVoucherName = "Bartender's Choice",
                    VoucherCode = "BATTLE50",
                    Discount = 50,
                    MaxPrice = 300000,
                    Quantity = 150, // Nhiều voucher cho sự kiện lớn
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event13,
                    EventVoucherName = "Sunday Brunch Premium",
                    VoucherCode = "JAZZ2HOUR",
                    Discount = 30,
                    MaxPrice = 800000, // High value for premium event
                    Quantity = 40,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event14,
                    EventVoucherName = "Mystery Box Special",
                    VoucherCode = "MIDNIGHT",
                    Discount = 75, // High discount
                    MaxPrice = 500000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event15,
                    EventVoucherName = "Full Moon Experience",
                    VoucherCode = "FULLMOON",
                    Discount = 40,
                    MaxPrice = 1500000, // Highest value voucher
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event16,
                    EventVoucherName = "Rock Early Bird",
                    VoucherCode = "ROCK40",
                    Discount = 40,
                    MaxPrice = 300000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event17,
                    EventVoucherName = "Rock Group Deal",
                    VoucherCode = "ROCKGROUP",
                    Discount = 25,
                    MaxPrice = 1000000,
                    Quantity = 30,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event18,
                    EventVoucherName = "Tequila Day",
                    VoucherCode = "TEQ1",
                    Discount = 30,
                    MaxPrice = 500000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event19,
                    EventVoucherName = "Tequila Day 2",
                    VoucherCode = "TEQ2",
                    Discount = 35,
                    MaxPrice = 500000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event20,
                    EventVoucherName = "Tequila Final Day",
                    VoucherCode = "TEQ3",
                    Discount = 40,
                    MaxPrice = 500000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event21,
                    EventVoucherName = "Comedy Night",
                    VoucherCode = "LAUGH25",
                    Discount = 25,
                    MaxPrice = 200000,
                    Quantity = 80,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event22,
                    EventVoucherName = "Disco Friday",
                    VoucherCode = "DISCO1",
                    Discount = 30,
                    MaxPrice = 300000,
                    Quantity = 60,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event23,
                    EventVoucherName = "Disco Saturday",
                    VoucherCode = "DISCO2",
                    Discount = 35,
                    MaxPrice = 300000,
                    Quantity = 60,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event24,
                    EventVoucherName = "Halloween Early",
                    VoucherCode = "HALL1",
                    Discount = 30,
                    MaxPrice = 400000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event25,
                    EventVoucherName = "Halloween Pre-Party",
                    VoucherCode = "HALL2",
                    Discount = 35,
                    MaxPrice = 400000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event26,
                    EventVoucherName = "Halloween Night Special",
                    VoucherCode = "HALL3",
                    Discount = 50,
                    MaxPrice = 1000000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event27,
                    EventVoucherName = "Summer Party Special",
                    VoucherCode = "SUMMER50",
                    Discount = 50,
                    MaxPrice = 500000,
                    Quantity = 100,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event28,
                    EventVoucherName = "Latin Night Discount",
                    VoucherCode = "LATIN30",
                    Discount = 30,
                    MaxPrice = 300000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event29,
                    EventVoucherName = "Latin Night Discount",
                    VoucherCode = "LATIN30",
                    Discount = 30,
                    MaxPrice = 300000,
                    Quantity = 50,
                    Status = true
                },
                new EventVoucher
                {
                    EventVoucherId = Guid.NewGuid(),
                    EventId = Constants.Ids.Event.Event30,
                    EventVoucherName = "Latin Night Discount",
                    VoucherCode = "LATIN30",
                    Discount = 30,
                    MaxPrice = 300000,
                    Quantity = 50,
                    Status = true
                }
            };
        }

        public static BarTime[] GetBarTimes()
        {
            var barTimes = new List<BarTime>();

            // Helper method để tạo BarTime
            void AddBarTime(Guid barId, int dayOfWeek, int startHour, int endHour)
            {
                barTimes.Add(new BarTime
                {
                    BarTimeId = Guid.NewGuid(),
                    BarId = barId,
                    DayOfWeek = dayOfWeek,
                    StartTime = new TimeSpan(startHour, 0, 0),
                    EndTime = new TimeSpan(endHour, 0, 0)
                });
            }

            // Bar Buddy 1 - Mở 4 ngày đầu 17:00-23:00
            for (int day = 0; day <= 4; day++)
            {
                AddBarTime(Constants.Ids.Bars.Bar1, day, 17, 23);
            }

            // Bar Buddy 2 - T3,T5,T7 (2,4,6) 17:00-23:00
            int[] bar2Days = { 2, 4, 6 };
            foreach (var day in bar2Days)
            {
                AddBarTime(Constants.Ids.Bars.Bar2, day, 17, 23);
            }

            // Bar Buddy 3 - CN,T4,T6 (0,3,5) 17:00-23:00
            int[] bar3Days = { 0, 3, 5 };
            foreach (var day in bar3Days)
            {
                AddBarTime(Constants.Ids.Bars.Bar3, day, 17, 23);
            }

            // Bar Buddy 4 - T2,T4,T6 (1,3,5) 18:00-24:00
            int[] bar4Days = { 1, 3, 5 };
            foreach (var day in bar4Days)
            {
                AddBarTime(Constants.Ids.Bars.Bar4, day, 18, 24);
            }

            // Bar Buddy 5 - T3,T5,T7 (2,4,6) 19:00-01:00
            int[] bar5Days = { 2, 4, 6 };
            foreach (var day in bar5Days)
            {
                AddBarTime(Constants.Ids.Bars.Bar5, day, 19, 1);
            }

            // Bar Buddy 6 - T4,T6,T7 (3,5,6) 20:00-02:00
            int[] bar6Days = { 3, 5, 6 };
            foreach (var day in bar6Days)
            {
                AddBarTime(Constants.Ids.Bars.Bar6, day, 20, 2);
            }

            return barTimes.ToArray();
        }

        public static Drink[] GetDrinks()
        {
            return new[]
            {
                // Đồ uống tại Bar 1
                new Drink
                {
                    DrinkId = Constants.Ids.Drinks.CocaCola,
                    BarId = Constants.Ids.Bars.Bar1,
                    DrinkCode = "D0001",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.SoftDrink,
                    DrinkName = "Coca Cola",
                    Description = "Nước ngọt có ga phổ biến, thích hợp cho mọi tâm trạng.",
                    Price = 15000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://www.coca-cola.com/content/dam/onexp/vn/home-image/coca-cola/Coca-Cola_OT%20320ml_VN-EX_Desktop.png",
                    Status = true
                },
                new Drink
                {
                    DrinkId = Constants.Ids.Drinks.Mojito,
                    BarId = Constants.Ids.Bars.Bar1,
                    DrinkCode = "D0002",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Mojito",
                    Description = "Cocktail nổi tiếng pha từ rượu rum, bạc hà và chanh, thích hợp cho không khí lãng mạn.",
                    Price = 70000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://www.liquor.com/thmb/MJRVqf-itJGY90nwUOYGXnyG-HA=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/mojito-720x720-primary-6a57f80e200c412e9a77a1687f312ff7.jpg",
                    Status = true
                },

                // Đồ uống tại Bar 2
                new Drink
                {
                    DrinkId = Constants.Ids.Drinks.TraDao,
                    BarId = Constants.Ids.Bars.Bar2,
                    DrinkCode = "D0003",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Tea,
                    DrinkName = "Trà Đào",
                    Description = "Trà đào thơm ngon, thanh mát, phù hợp cho những lúc cần thư giãn.",
                    Price = 35000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://file.hstatic.net/200000684957/article/tra-dao_e022b1a9ac564ee186007875701ac643.jpg",
                    Status = true
                },
                new Drink
                {
                    DrinkId = Constants.Ids.Drinks.Pepsi,
                    BarId = Constants.Ids.Bars.Bar2,
                    DrinkCode = "D0004",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.SoftDrink,
                    DrinkName = "Pepsi",
                    Description = "Nước ngọt có ga sảng khoái, thích hợp cho mọi dịp.",
                    Price = 15000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "http://thepizzacompany.vn/images/thumbs/000/0002364_pepsi-15l-pet_500.jpeg",
                    Status = true
                },

                // Đồ uống tại Bar 3
                new Drink
                {
                    DrinkId = Constants.Ids.Drinks.Screwdriver,
                    BarId = Constants.Ids.Bars.Bar3,
                    DrinkCode = "D0005",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Screwdriver",
                    Description = "Cocktail mạnh mẽ từ vodka và nước cam, thích hợp cho những buổi tiệc sôi động.",
                    Price = 80000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://www.liquor.com/thmb/RnOVWoIXp7OAJRS-NSDIF9Bglbc=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/LQR-screwdriver-original-4000x4000-edb2f56dd69146bba9f7fafbf69e00a0.jpg",
                    Status = true
                },
                new Drink
                {
                    DrinkId = Constants.Ids.Drinks.BlackCoffee,
                    BarId = Constants.Ids.Bars.Bar3,
                    DrinkCode = "D0006",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Coffee,
                    DrinkName = "Cà phê đen",
                    Description = "Cà phê đen đậm đà, thích hợp cho những người thích vị đắng truyền thống.",
                    Price = 15000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://suckhoedoisong.qltns.mediacdn.vn/324455921873985536/2024/8/14/2121767707dcce179f6866d132a2d6a384312f9-1723600454996-1723600455541950721311.jpg",
                    Status = true
                },
                
                // Classic Cocktails
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    BarId = Constants.Ids.Bars.Bar1,
                    DrinkCode = "D0009",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Long Island Iced Tea",
                    Description = "Cocktail mạnh mẽ pha trộn từ 5 loại rượu khác nhau với vị ngọt của coca cola",
                    Price = 190000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://food.fnr.sndimg.com/content/dam/images/food/fullset/2011/4/25/0/CCWM_Long-Island-Ice-Tea_s3x4.jpg.rend.hgtvcom.1280.960.suffix/1572356786983.webp",
                    Status = true
                },
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    BarId = Constants.Ids.Bars.Bar2,
                    DrinkCode = "D0010",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Bloody Mary",
                    Description = "Cocktail cổ điển với nước ép cà chua và vodka, gia vị đặc trưng",
                    Price = 170000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS-NIEW-vk7sDfhGEk23-RPqp7pGehRzfK3Fg&s",
                    Status = true
                },

                // Premium Cocktails
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    BarId = Constants.Ids.Bars.Bar3,
                    DrinkCode = "D0011",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Old Fashioned",
                    Description = "Cocktail premium với whiskey cao cấp, đường nâu và bitter",
                    Price = 250000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTr3StQ4qUmPQpN89YjVKWOQ2LBgUF0slFHYw&s",
                    Status = true
                },
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    BarId = Constants.Ids.Bars.Bar4,
                    DrinkCode = "D0012",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Manhattan",
                    Description = "Cocktail thượng hạng pha từ whiskey rye, vermouth đỏ và bitter",
                    Price = 230000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://www.foodandwine.com/thmb/7A6wLrQRKdzFlNK6O_JtV-SMrKQ=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/manhattan-FT-RECIPE0724-94baa533ea654ce292ca2d2c483edb77.jpeg",
                    Status = true
                },

                // Mocktails
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    BarId = Constants.Ids.Bars.Bar5,
                    DrinkCode = "D0013",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Virgin Mojito",
                    Description = "Phiên bản không cồn của Mojito với bạc hà tươi và chanh",
                    Price = 120000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTmzolqf49U34e2iDR3mDzslZ8ZmkL7g7baCA&s",
                    Status = true
                },
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    BarId = Constants.Ids.Bars.Bar1,
                    DrinkCode = "D0014",
                    DrinkCategoryId = Constants.Ids.DrinkCategories.Cocktail,
                    DrinkName = "Virgin Piña Colada",
                    Description = "Mocktail nhiệt đới với nước dừa và nước ép dứa tươi",
                    Price = 120000,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Image = "https://www.thespruceeats.com/thmb/hZgCGe6yaJLcyOgDHMv0H6-bN20=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/virgin-pina-colada-recipe-2097115-hero-images-5-25cd3a15f6204c1d87fdf8c60b6b144e.jpg",
                    Status = true
                }
            };
        }

        public static Booking[] GetBookings()
        {
            DateTime baseDate = DateTime.Now.Date;

            return new[]
            {
                new Booking
                {
                    BookingId = Constants.Ids.Bookings.Booking1,
                    BarId = Constants.Ids.Bars.Bar1,
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BookingCode = "BB0001",
                    BookingDate = baseDate.AddDays(-5),
                    BookingTime = new TimeSpan(19, 0, 0),
                    Status = 1,
                    NumOfTable = 1,
                    CreateAt = baseDate.AddDays(-6),
                    ExpireAt = baseDate.AddDays(-5).Add(new TimeSpan(21, 0, 0)),
                    Note = "VIP customer booking",
                    TotalPrice = 2000000,
                    AdditionalFee = 100000,
                    QRTicket = "qr_code_1"
                },
                new Booking
                {
                    BookingId = Constants.Ids.Bookings.Booking2,
                    BarId = Constants.Ids.Bars.Bar2,
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BookingCode = "BB0002",
                    BookingDate = baseDate.AddDays(-7),
                    BookingTime = new TimeSpan(20, 0, 0),
                    Status = 2,
                    NumOfTable = 2,
                    CreateAt = baseDate.AddDays(-8),
                    ExpireAt = baseDate.AddDays(-7).Add(new TimeSpan(22, 0, 0)),
                    Note = "Group booking",
                    TotalPrice = 3500000,
                    AdditionalFee = 150000,
                    QRTicket = "qr_code_2"
                },
                new Booking
                {
                    BookingId = Constants.Ids.Bookings.Booking3,
                    BarId = Constants.Ids.Bars.Bar3,
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BookingCode = "BB0003",
                    BookingDate = baseDate.AddDays(-3),
                    BookingTime = new TimeSpan(18, 0, 0),
                    Status = 1,
                    NumOfTable = 1,
                    CreateAt = baseDate.AddDays(-4),
                    ExpireAt = baseDate.AddDays(-3).Add(new TimeSpan(20, 0, 0)),
                    Note = "Birthday celebration",
                    TotalPrice = 1500000,
                    AdditionalFee = 50000,
                    QRTicket = "qr_code_3"
                },
                new Booking
                {
                    BookingId = Constants.Ids.Bookings.Booking4,
                    BarId = Constants.Ids.Bars.Bar3,
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BookingCode = "BB0004",
                    BookingDate = baseDate.AddDays(-2),
                    BookingTime = new TimeSpan(21, 0, 0),
                    Status = 1,
                    NumOfTable = 1,
                    CreateAt = baseDate.AddDays(-3),
                    ExpireAt = baseDate.AddDays(-2).Add(new TimeSpan(23, 0, 0)),
                    Note = "Regular booking",
                    TotalPrice = 1800000,
                    AdditionalFee = 80000,
                    QRTicket = "qr_code_4"
                },
                new Booking
                {
                    BookingId = Constants.Ids.Bookings.Booking5,
                    BarId = Constants.Ids.Bars.Bar4,
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BookingCode = "BB0005",
                    BookingDate = baseDate.AddDays(1),
                    BookingTime = new TimeSpan(19, 30, 0),
                    Status = 1,
                    NumOfTable = 2,
                    CreateAt = baseDate,
                    ExpireAt = baseDate.AddDays(1).Add(new TimeSpan(21, 30, 0)),
                    Note = "Anniversary celebration",
                    TotalPrice = 2500000,
                    AdditionalFee = 120000,
                    QRTicket = "qr_code_5"
                },
                new Booking
                {
                    BookingId = Constants.Ids.Bookings.Booking6,
                    BarId = Constants.Ids.Bars.Bar5,
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BookingCode = "BB0006",
                    BookingDate = baseDate.AddDays(2),
                    BookingTime = new TimeSpan(20, 0, 0),
                    Status = 1,
                    NumOfTable = 2,
                    CreateAt = baseDate,
                    ExpireAt = baseDate.AddDays(2).Add(new TimeSpan(22, 0, 0)),
                    Note = "Business meeting with clients",
                    TotalPrice = 3000000,
                    AdditionalFee = 150000,
                    QRTicket = "qr_code_6"
                }
            };
        }

        public static BookingTable[] GetBookingTables()
        {
            // Lấy dữ liệu bàn từ hàm GetTablesData
            var allTables = GetTablesData().Tables;
            var bookingTables = new List<BookingTable>();

            // Helper function để tìm bàn theo bar và loại
            Table FindTable(Guid barId, string tableTypePrefix)
            {
                return allTables.FirstOrDefault(t =>
                    t.TableName.Contains($"{tableTypePrefix}{barId.ToString().Substring(0, 8)}"));
            }

            try
            {
                // Booking 1 - Bar 1 (1 bàn SVIP)
                var table1 = FindTable(Constants.Ids.Bars.Bar1, "SVIP");
                if (table1 != null)
                {
                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable1,
                        BookingId = Constants.Ids.Bookings.Booking1,
                        TableId = table1.TableId
                    });
                }

                // Booking 2 - Bar 2 (2 bàn VIP)
                var tables2 = allTables.Where(t =>
                    t.TableName.Contains($"VIP{Constants.Ids.Bars.Bar2.ToString().Substring(0, 8)}")
                ).Take(2).ToList();

                if (tables2.Count >= 2)
                {
                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable2,
                        BookingId = Constants.Ids.Bookings.Booking2,
                        TableId = tables2[0].TableId
                    });

                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable3,
                        BookingId = Constants.Ids.Bookings.Booking2,
                        TableId = tables2[1].TableId
                    });
                }

                // Booking 3 - Bar 3 (1 bàn TC2)
                var table3 = FindTable(Constants.Ids.Bars.Bar3, "TC2");
                if (table3 != null)
                {
                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable4,
                        BookingId = Constants.Ids.Bookings.Booking3,
                        TableId = table3.TableId
                    });
                }

                // Booking 4 - Bar 3 (1 bàn TC1)
                var table4 = FindTable(Constants.Ids.Bars.Bar3, "TC1");
                if (table4 != null)
                {
                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable5,
                        BookingId = Constants.Ids.Bookings.Booking4,
                        TableId = table4.TableId
                    });
                }

                // Booking 5 - Bar 4 (2 bàn VIP)
                var tables5 = allTables.Where(t =>
                    t.TableName.Contains($"VIP{Constants.Ids.Bars.Bar4.ToString().Substring(0, 8)}")
                ).Take(2).ToList();

                if (tables5.Count >= 2)
                {
                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable6,
                        BookingId = Constants.Ids.Bookings.Booking5,
                        TableId = tables5[0].TableId
                    });

                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable7,
                        BookingId = Constants.Ids.Bookings.Booking5,
                        TableId = tables5[1].TableId
                    });
                }

                // Booking 6 - Bar 5 (2 bàn SVIP)
                var tables6 = allTables.Where(t =>
                    t.TableName.Contains($"SVIP{Constants.Ids.Bars.Bar5.ToString().Substring(0, 8)}")
                ).Take(2).ToList();

                if (tables6.Count >= 2)
                {
                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable8,
                        BookingId = Constants.Ids.Bookings.Booking6,
                        TableId = tables6[0].TableId
                    });

                    bookingTables.Add(new BookingTable
                    {
                        BookingTableId = Constants.Ids.BookingTables.BookingTable9,
                        BookingId = Constants.Ids.Bookings.Booking6,
                        TableId = tables6[1].TableId
                    });
                }

                return bookingTables.ToArray();
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error in GetBookingTables: {ex.Message}");
                return Array.Empty<BookingTable>();
            }
        }

        public static DrinkEmotionalCategory[] GetDrinkEmotionalCategories()
        {
            return new[]
            {
                // Coca Cola - phù hợp khi vui, hạnh phúc
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.CocaCola,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.vuiId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.CocaCola,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.hanhPhucId
                },

                // Mojito - phù hợp khi vui, hạnh phúc, đang yêu
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Mojito,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.vuiId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Mojito,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.hanhPhucId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Mojito,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.dangYeuId
                },

                // Trà Đào - phù hợp khi buồn, chán nản
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.TraDao,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.buonId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.TraDao,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.chanNanId
                },

                // Pepsi - phù hợp khi vui, hạnh phúc
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Pepsi,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.vuiId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Pepsi,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.hanhPhucId
                },

                // Screwdriver - phù hợp khi tức giận, buồn
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Screwdriver,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.tucGianId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.Screwdriver,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.buonId
                },

                // Cà phê đen - phù hợp khi chán nản, tức giận
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.BlackCoffee,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.chanNanId
                },
                new DrinkEmotionalCategory
                {
                    DrinkEmotionalCategoryId = Guid.NewGuid(),
                    DrinkId = Constants.Ids.Drinks.BlackCoffee,
                    EmotionalDrinkCategoryId = Constants.Ids.EmotionalDrinkCategory.tucGianId
                }
            };
        }
        //public static EventVoucher[] GetEventVouchers() => new EventVoucher[] { /* ... */ };
        public static Feedback[] GetFeedbacks()
        {
            return new[]
            {
                // Feedback cho Booking1 tại Bar1 bởi Customer1
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BookingId = Constants.Ids.Bookings.Booking1,
                    BarId = Constants.Ids.Bars.Bar1,
                    Rating = 5,
                    Comment = "Không gian tuyệt vời, nhân viên phục vụ chuyên nghiệp. Đồ uống ngon và đa dạng.",
                    CommentEmotionalForDrink = "Mojito ở đây làm rất ngon, phù hợp với không khí vui vẻ của nhóm.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddDays(-4),
                    LastUpdatedTime = DateTimeOffset.Now.AddDays(-4)
                },

                // Feedback cho Booking2 tại Bar2 bởi Customer1
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BookingId = Constants.Ids.Bookings.Booking2,
                    BarId = Constants.Ids.Bars.Bar2,
                    Rating = 4,
                    Comment = "Âm nhc sôi động, không khí náo nhiệt. Phù hợp cho nhóm bạn đi chơi.",
                    CommentEmotionalForDrink = "Cocktail ở đây khá đặc biệt, phù hợp với tâm trạng vui vẻ.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddDays(-6),
                    LastUpdatedTime = DateTimeOffset.Now.AddDays(-6)
                },

                // Feedback cho Booking3 tại Bar3 bởi Customer2
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BookingId = Constants.Ids.Bookings.Booking3,
                    BarId = Constants.Ids.Bars.Bar3,
                    Rating = 5,
                    Comment = "Tổ chức sinh nhật ở đây rất tuyệt! Nhân viên nhiệt tình hỗ trợ.",
                    CommentEmotionalForDrink = "Các loại đồ uống phù hợp với không khí tiệc tùng, đặc biệt là các loại cocktail.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddDays(-2),
                    LastUpdatedTime = DateTimeOffset.Now.AddDays(-2)
                },

                // Feedback cho Booking4 tại Bar3 bởi Customer2
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BookingId = Constants.Ids.Bookings.Booking4,
                    BarId = Constants.Ids.Bars.Bar3,
                    Rating = 3,
                    Comment = "Đồ uống ngon nhưng hơi ồn ào vào cuối tuần.",
                    CommentEmotionalForDrink = "Trà đào ở đây khá ngon, giúp thư giãn sau một ngày làm việc mệt mỏi.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddDays(-1),
                    LastUpdatedTime = DateTimeOffset.Now.AddDays(-1)
                },

                // Feedback cho Booking5 tại Bar4 bởi Customer1
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    AccountId = Constants.Ids.Accounts.Customer1,
                    BookingId = Constants.Ids.Bookings.Booking5,
                    BarId = Constants.Ids.Bars.Bar4,
                    Rating = 5,
                    Comment = "Địa điểm hoàn hảo để tổ chức kỷ niệm! View đẹp, không gian lãng mạn.",
                    CommentEmotionalForDrink = "Mojito và các loại cocktail rất phù hợp cho không khí lãng mạn.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddHours(-12),
                    LastUpdatedTime = DateTimeOffset.Now.AddHours(-12)
                },
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    AccountId = Constants.Ids.Accounts.Customer2,
                    BookingId = Constants.Ids.Bookings.Booking6,
                    BarId = Constants.Ids.Bars.Bar5,
                    Rating = 5,
                    Comment = "Địa điểm hoàn hảo để tổ chức kỷ niệm! View đẹp, không gian lãng mạn.",
                    CommentEmotionalForDrink = "Đồ uống rất phù hợp cho không khí lãng mạn.",
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddHours(-12),
                    LastUpdatedTime = DateTimeOffset.Now.AddHours(-12)
                }
            };
        }

        public static BookingDrink[] GetBookingDrinks()
        {
            return new[]
            {
                // Booking 5 - Anniversary celebration
                new BookingDrink
                {
                    BookingDrinkId = Constants.Ids.BookingDrinks.BookingDrink1,
                    BookingId = Constants.Ids.Bookings.Booking5,
                    DrinkId = Constants.Ids.Drinks.Mojito, // Mojito cho không khí lãng mạn
                    Quantity = 2, // Đặt 2 ly cho cặp đôi
                    ActualPrice = 70000, // Giá gốc của Mojito
                },
                new BookingDrink
                {
                    BookingDrinkId = Constants.Ids.BookingDrinks.BookingDrink2,
                    BookingId = Constants.Ids.Bookings.Booking5,
                    DrinkId = Constants.Ids.Drinks.TraDao, // Trà đào để giải khát
                    Quantity = 2,
                    ActualPrice = 35000,
                },

                // Booking 6 - Business meeting
                new BookingDrink
                {
                    BookingDrinkId = Constants.Ids.BookingDrinks.BookingDrink3,
                    BookingId = Constants.Ids.Bookings.Booking6,
                    DrinkId = Constants.Ids.Drinks.Screwdriver, // Cocktail cho khách VIP
                    Quantity = 3, // Đặt 3 ly cho nhóm khách
                    ActualPrice = 80000, // Giá gốc của Screwdriver
                },
                new BookingDrink
                {
                    BookingDrinkId = Constants.Ids.BookingDrinks.BookingDrink4,
                    BookingId = Constants.Ids.Bookings.Booking6,
                    DrinkId = Constants.Ids.Drinks.BlackCoffee, // Cà phê đen cho cuộc họp
                    Quantity = 4, // Đặt 4 ly cho cả nhóm
                    ActualPrice = 15000, // Giá gốc của Cà phê đen
                }
            };
        }

        public static (TableType[] TableTypes, Table[] Tables) GetTablesData()
        {
            var tableTypes = new List<TableType>();
            var tables = new List<Table>();

            // Dictionary để map BarId với số thứ tự
            var barNumberMap = new Dictionary<Guid, int>
            {
                { Constants.Ids.Bars.Bar1, 1 },
                { Constants.Ids.Bars.Bar2, 2 },
                { Constants.Ids.Bars.Bar3, 3 },
                { Constants.Ids.Bars.Bar4, 4 },
                { Constants.Ids.Bars.Bar5, 5 },
                { Constants.Ids.Bars.Bar6, 6 },
                { Constants.Ids.Bars.Bar7, 7 },
                { Constants.Ids.Bars.Bar8, 8 },
                { Constants.Ids.Bars.Bar9, 9 },
                { Constants.Ids.Bars.Bar10, 10 }
            };

            // Xử lý cho từng bar
            foreach (var barIdPair in barNumberMap)
            {
                var barId = barIdPair.Key;
                var barNumber = barIdPair.Value;

                // Tạo các loại bàn cho mỗi bar
                var svipType = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Bàn SVIP",
                    Description = "Bàn SVIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao nhất tại quán, phù hợp cho nhóm khách hàng từ 1-15 người, mức giá tối thiểu chỉ từ 10.000.000 VND.",
                    MaximumGuest = 15,
                    MinimumGuest = 1,
                    MinimumPrice = 10000000,
                    IsDeleted = false
                };
                tableTypes.Add(svipType);

                var vipType = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Bàn VIP",
                    Description = "Bàn VIP phù hợp cho khách hàng muốn trải nghiệm dịch vụ chất lượng cao tại quán, phù hợp cho nhóm khách hàng từ 1-10 người, mức giá tối thiểu chỉ từ 5.000.000 VND.",
                    MaximumGuest = 10,
                    MinimumGuest = 1,
                    MinimumPrice = 5000000,
                    IsDeleted = false
                };
                tableTypes.Add(vipType);

                var standard1Type = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Bàn Tiêu chuẩn 1",
                    Description = "Bàn Tiêu chuẩn 1 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 1-4 người, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 4,
                    MinimumGuest = 1,
                    MinimumPrice = 200000,
                    IsDeleted = false
                };
                tableTypes.Add(standard1Type);

                var standard2Type = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Bàn Tiêu chuẩn 2",
                    Description = "Bàn Tiêu chuẩn 2 phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán, phù hợp cho nhóm khách hàng từ 4-6 người, mức giá tối thiểu chỉ từ 500.000 VND.",
                    MaximumGuest = 6,
                    MinimumGuest = 4,
                    MinimumPrice = 500000,
                    IsDeleted = false
                };
                tableTypes.Add(standard2Type);

                var barCounterType = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Bàn Quầy Bar",
                    Description = "Bàn Quầy Bar phù hợp cho khách hàng muốn trải nghiệm dịch vụ tiêu chuẩn tại quán và được phụ vụ trực tiếp bởi các Bartender, mức giá tối thiểu chỉ từ 200.000 VND.",
                    MaximumGuest = 1,
                    MinimumGuest = 1,
                    MinimumPrice = 100000,
                    IsDeleted = false
                };
                tableTypes.Add(barCounterType);

                // Tạo các bàn tương ứng với mỗi loại bàn
                var allTypes = new[] { svipType, vipType, standard1Type, standard2Type, barCounterType };
                foreach (var type in allTypes)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        string tableCode = type.TypeName switch
                        {
                            "Bàn SVIP" => $"SVIP{barNumber}-{(char)('A' + i - 1)}",
                            "Bàn VIP" => $"VIP{barNumber}-{(char)('A' + i - 1)}",
                            "Bàn Tiêu chuẩn 1" => $"TC1{barNumber}-{(char)('A' + i - 1)}",
                            "Bàn Tiêu chuẩn 2" => $"TC2{barNumber}-{(char)('A' + i - 1)}",
                            _ => $"QB{barNumber}-{(char)('A' + i - 1)}" // Bàn Quầy Bar
                        };

                        tables.Add(new Table
                        {
                            TableId = Guid.NewGuid(),
                            TableTypeId = type.TableTypeId,
                            TableName = $"Table {tableCode}",
                            Status = 0,
                            IsDeleted = false
                        });
                    }
                }
            }

            return (tableTypes.ToArray(), tables.ToArray());
        }
    }
}