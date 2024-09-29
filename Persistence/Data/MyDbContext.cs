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
        }
    }
}
