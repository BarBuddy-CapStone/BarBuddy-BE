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
    }
}
