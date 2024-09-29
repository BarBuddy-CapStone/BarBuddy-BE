﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence.Data;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20240928144828_JavisNgo")]
    partial class JavisNgo
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Account", b =>
                {
                    b.Property<string>("AccountId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BarId")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTimeOffset>("Dob")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Fullname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("AccountId");

                    b.HasIndex("BarId");

                    b.HasIndex("RoleId");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Domain.Entities.Bar", b =>
                {
                    b.Property<string>("BarId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("BarName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<double>("Discount")
                        .HasColumnType("double");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("time(6)");

                    b.Property<string>("Images")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("time(6)");

                    b.Property<bool>("Status")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("BarId");

                    b.ToTable("Bar");
                });

            modelBuilder.Entity("Domain.Entities.Booking", b =>
                {
                    b.Property<string>("BookingId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BarId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTimeOffset>("BookingDate")
                        .HasColumnType("datetime(6)");

                    b.Property<TimeSpan>("BookingTime")
                        .HasColumnType("time(6)");

                    b.Property<bool>("IsIncludeDrink")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Note")
                        .HasColumnType("longtext");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("BookingId");

                    b.HasIndex("AccountId");

                    b.HasIndex("BarId");

                    b.ToTable("Booking");
                });

            modelBuilder.Entity("Domain.Entities.BookingDrink", b =>
                {
                    b.Property<string>("BookingDrinkId")
                        .HasColumnType("varchar(255)");

                    b.Property<double>("ActualPrice")
                        .HasColumnType("double");

                    b.Property<string>("BookingId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DrinkId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("BookingDrinkId");

                    b.HasIndex("BookingId");

                    b.HasIndex("DrinkId");

                    b.ToTable("BookingDrink");
                });

            modelBuilder.Entity("Domain.Entities.BookingTable", b =>
                {
                    b.Property<string>("BookingTableId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BookingId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTimeOffset>("ReservationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<TimeSpan>("ReservationTime")
                        .HasColumnType("time(6)");

                    b.Property<string>("TableId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("BookingTableId");

                    b.HasIndex("BookingId");

                    b.HasIndex("TableId");

                    b.ToTable("BookingTable");
                });

            modelBuilder.Entity("Domain.Entities.Drink", b =>
                {
                    b.Property<string>("DrinkId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DrinkCategoryId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DrinkName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<double>("Price")
                        .HasColumnType("double");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("DrinkId");

                    b.HasIndex("DrinkCategoryId");

                    b.ToTable("Drink");
                });

            modelBuilder.Entity("Domain.Entities.DrinkCategory", b =>
                {
                    b.Property<string>("DrinksCategoryId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DrinksCategoryName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("DrinksCategoryId");

                    b.ToTable("DrinkCategory");
                });

            modelBuilder.Entity("Domain.Entities.DrinkEmotionalCategory", b =>
                {
                    b.Property<string>("DrinkEmotionalCategoryId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DrinkId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("EmotionalDrinkCategoryId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("DrinkEmotionalCategoryId");

                    b.HasIndex("DrinkId");

                    b.HasIndex("EmotionalDrinkCategoryId");

                    b.ToTable("DrinkEmotionalCategory");
                });

            modelBuilder.Entity("Domain.Entities.EmotionalDrinkCategory", b =>
                {
                    b.Property<string>("EmotionalDrinksCategoryId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("EmotionalDrinksCategoryId");

                    b.ToTable("EmotionalDrinkCategory");
                });

            modelBuilder.Entity("Domain.Entities.Feedback", b =>
                {
                    b.Property<string>("FeedbackId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BarId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BookingId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTimeOffset>("FeedbackDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.HasKey("FeedbackId");

                    b.HasIndex("AccountId");

                    b.HasIndex("BarId");

                    b.HasIndex("BookingId");

                    b.ToTable("Feedback");
                });

            modelBuilder.Entity("Domain.Entities.PaymentHistory", b =>
                {
                    b.Property<string>("PaymentHistoryId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BookingId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Note")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Status")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("TotalPrice")
                        .HasColumnType("double");

                    b.Property<string>("TransactionCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("PaymentHistoryId");

                    b.HasIndex("AccountId");

                    b.HasIndex("BookingId");

                    b.ToTable("PaymentHistory");
                });

            modelBuilder.Entity("Domain.Entities.Role", b =>
                {
                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("RoleId");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("Domain.Entities.Table", b =>
                {
                    b.Property<string>("TableId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BarId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TableTypeId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("TableId");

                    b.HasIndex("BarId");

                    b.HasIndex("TableTypeId");

                    b.ToTable("Table");
                });

            modelBuilder.Entity("Domain.Entities.TableType", b =>
                {
                    b.Property<string>("TableTypeId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("MaximumGuest")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("MinimumGuest")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<double>("MiniumPrice")
                        .HasColumnType("double");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("TableTypeId");

                    b.ToTable("TableType");
                });

            modelBuilder.Entity("Domain.Entities.Account", b =>
                {
                    b.HasOne("Domain.Entities.Bar", "Bar")
                        .WithMany("Accounts")
                        .HasForeignKey("BarId");

                    b.HasOne("Domain.Entities.Role", "Role")
                        .WithMany("Accounts")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bar");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Domain.Entities.Booking", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("Bookings")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Bar", "Bar")
                        .WithMany("Bookings")
                        .HasForeignKey("BarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Bar");
                });

            modelBuilder.Entity("Domain.Entities.BookingDrink", b =>
                {
                    b.HasOne("Domain.Entities.Booking", "Booking")
                        .WithMany("BookingDrinks")
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Drink", "Drink")
                        .WithMany("BookingDrinks")
                        .HasForeignKey("DrinkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Booking");

                    b.Navigation("Drink");
                });

            modelBuilder.Entity("Domain.Entities.BookingTable", b =>
                {
                    b.HasOne("Domain.Entities.Booking", "Booking")
                        .WithMany("BookingTables")
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Table", "Table")
                        .WithMany("BookingTables")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Booking");

                    b.Navigation("Table");
                });

            modelBuilder.Entity("Domain.Entities.Drink", b =>
                {
                    b.HasOne("Domain.Entities.DrinkCategory", "DrinkCategory")
                        .WithMany("Drinks")
                        .HasForeignKey("DrinkCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DrinkCategory");
                });

            modelBuilder.Entity("Domain.Entities.DrinkEmotionalCategory", b =>
                {
                    b.HasOne("Domain.Entities.Drink", "Drink")
                        .WithMany("DrinkEmotionalCategories")
                        .HasForeignKey("DrinkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.EmotionalDrinkCategory", "EmotionalDrinkCategory")
                        .WithMany()
                        .HasForeignKey("EmotionalDrinkCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Drink");

                    b.Navigation("EmotionalDrinkCategory");
                });

            modelBuilder.Entity("Domain.Entities.Feedback", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("Feedbacks")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Bar", "Bar")
                        .WithMany("Feedbacks")
                        .HasForeignKey("BarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Booking", "Booking")
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Bar");

                    b.Navigation("Booking");
                });

            modelBuilder.Entity("Domain.Entities.PaymentHistory", b =>
                {
                    b.HasOne("Domain.Entities.Account", "Account")
                        .WithMany("PaymentHistories")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Booking", "Booking")
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Booking");
                });

            modelBuilder.Entity("Domain.Entities.Table", b =>
                {
                    b.HasOne("Domain.Entities.Bar", "Bar")
                        .WithMany("Tables")
                        .HasForeignKey("BarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.TableType", "TableType")
                        .WithMany("Tables")
                        .HasForeignKey("TableTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bar");

                    b.Navigation("TableType");
                });

            modelBuilder.Entity("Domain.Entities.Account", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("Feedbacks");

                    b.Navigation("PaymentHistories");
                });

            modelBuilder.Entity("Domain.Entities.Bar", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Bookings");

                    b.Navigation("Feedbacks");

                    b.Navigation("Tables");
                });

            modelBuilder.Entity("Domain.Entities.Booking", b =>
                {
                    b.Navigation("BookingDrinks");

                    b.Navigation("BookingTables");
                });

            modelBuilder.Entity("Domain.Entities.Drink", b =>
                {
                    b.Navigation("BookingDrinks");

                    b.Navigation("DrinkEmotionalCategories");
                });

            modelBuilder.Entity("Domain.Entities.DrinkCategory", b =>
                {
                    b.Navigation("Drinks");
                });

            modelBuilder.Entity("Domain.Entities.Role", b =>
                {
                    b.Navigation("Accounts");
                });

            modelBuilder.Entity("Domain.Entities.Table", b =>
                {
                    b.Navigation("BookingTables");
                });

            modelBuilder.Entity("Domain.Entities.TableType", b =>
                {
                    b.Navigation("Tables");
                });
#pragma warning restore 612, 618
        }
    }
}
