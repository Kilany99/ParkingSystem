﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParkingSystem.Data;

#nullable disable

namespace ParkingSystem.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ParkingSystem.Models.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("ParkingZoneId")
                        .HasColumnType("int");

                    b.Property<string>("PlateNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ReservationId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ParkingZoneId");

                    b.HasIndex("PlateNumber")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("ParkingSystem.Models.ParkingSpot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CurrentReservationId")
                        .HasColumnType("int");

                    b.Property<int>("Floor")
                        .HasColumnType("int");

                    b.Property<int>("ParkingZoneId")
                        .HasColumnType("int");

                    b.Property<int?>("ReservationId")
                        .HasColumnType("int");

                    b.Property<string>("SpotNumber")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CurrentReservationId");

                    b.HasIndex("ParkingZoneId");

                    b.ToTable("ParkingSpots");
                });

            modelBuilder.Entity("ParkingSystem.Models.ParkingZone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("HourlyRate")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsFull")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("SpotsPerFloor")
                        .HasColumnType("int");

                    b.Property<int>("TotalFloors")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ParkingZones");
                });

            modelBuilder.Entity("ParkingSystem.Models.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Method")
                        .HasColumnType("int");

                    b.Property<int>("ReservationId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReservationId")
                        .IsUnique();

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("ParkingSystem.Models.Reservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CarId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("EntryTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExitTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("bit");

                    b.Property<int>("ParkingSpotId")
                        .HasColumnType("int");

                    b.Property<string>("QRCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<decimal?>("TotalAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.HasIndex("ParkingSpotId");

                    b.HasIndex("UserId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("ParkingSystem.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("ResetPasswordToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResetPasswordTokenExpiration")
                        .HasColumnType("datetime2");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ParkingSystem.Models.Car", b =>
                {
                    b.HasOne("ParkingSystem.Models.ParkingZone", "ParkingZone")
                        .WithMany("Cars")
                        .HasForeignKey("ParkingZoneId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ParkingSystem.Models.User", "User")
                        .WithMany("Cars")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ParkingZone");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ParkingSystem.Models.ParkingSpot", b =>
                {
                    b.HasOne("ParkingSystem.Models.Reservation", "CurrentReservation")
                        .WithMany()
                        .HasForeignKey("CurrentReservationId");

                    b.HasOne("ParkingSystem.Models.ParkingZone", "ParkingZone")
                        .WithMany("ParkingSpots")
                        .HasForeignKey("ParkingZoneId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CurrentReservation");

                    b.Navigation("ParkingZone");
                });

            modelBuilder.Entity("ParkingSystem.Models.Payment", b =>
                {
                    b.HasOne("ParkingSystem.Models.Reservation", "Reservation")
                        .WithOne("Payment")
                        .HasForeignKey("ParkingSystem.Models.Payment", "ReservationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("ParkingSystem.Models.Reservation", b =>
                {
                    b.HasOne("ParkingSystem.Models.Car", "Car")
                        .WithMany("Reservations")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ParkingSystem.Models.ParkingSpot", "ParkingSpot")
                        .WithMany("Reservations")
                        .HasForeignKey("ParkingSpotId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ParkingSystem.Models.User", "User")
                        .WithMany("Reservations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("ParkingSpot");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ParkingSystem.Models.Car", b =>
                {
                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("ParkingSystem.Models.ParkingSpot", b =>
                {
                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("ParkingSystem.Models.ParkingZone", b =>
                {
                    b.Navigation("Cars");

                    b.Navigation("ParkingSpots");
                });

            modelBuilder.Entity("ParkingSystem.Models.Reservation", b =>
                {
                    b.Navigation("Payment")
                        .IsRequired();
                });

            modelBuilder.Entity("ParkingSystem.Models.User", b =>
                {
                    b.Navigation("Cars");

                    b.Navigation("Reservations");
                });
#pragma warning restore 612, 618
        }
    }
}
