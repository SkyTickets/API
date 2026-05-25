using API.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;


namespace API.Model;

public partial class PostgresContext(DbContextOptions<PostgresContext> options) : DbContext(options)
{
    public virtual DbSet<AdditionalService> AdditionalServices { get; set; }

    public virtual DbSet<Airline> Airlines { get; set; }

    public virtual DbSet<Airplane> Airplanes { get; set; }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    public virtual DbSet<Passenger> Passengers { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<BookingStatus>(schema: "skytickets", name: "booking_status")
            .HasPostgresEnum<ClassOfService>(schema: "skytickets", name: "class_of_service")
            .HasPostgresEnum<Role>(schema: "skytickets", name: "role");

        modelBuilder.Entity<AdditionalService>(entity =>
        {
            entity.HasKey(e => e.AsId).HasName("additional_services_pk");

            entity.ToTable("additional_services", "skytickets");

            entity.Property(e => e.AsId)
                .ValueGeneratedOnAdd()
                .HasColumnName("as_id");
            entity.Property(e => e.AsName)
                .HasMaxLength(100)
                .HasColumnName("as_name");
            entity.Property(e => e.AsPrice).HasColumnName("as_price");
        });

        modelBuilder.Entity<Airline>(entity =>
        {
            entity.HasKey(e => e.AlId).HasName("airlines_pk");

            entity.ToTable("airlines", "skytickets");

            entity.Property(e => e.AlId)
                .ValueGeneratedOnAdd()
                .HasColumnName("al_id");
            entity.Property(e => e.AlEmail).HasColumnName("al_email");
            entity.Property(e => e.AlName)
                .HasMaxLength(45)
                .HasColumnName("al_name");
        });

        modelBuilder.Entity<Airplane>(entity =>
        {
            entity.HasKey(e => e.PlId).HasName("airplanes_pk");

            entity.ToTable("airplanes", "skytickets");

            entity.Property(e => e.PlId)
                .ValueGeneratedOnAdd()
                .HasColumnName("pl_id");
            entity.Property(e => e.PlBusinessSeats)
                .HasDefaultValue(0)
                .HasColumnName("pl_business_seats");
            entity.Property(e => e.PlComfortSeats)
                .HasDefaultValue(0)
                .HasColumnName("pl_comfort_seats");
            entity.Property(e => e.PlEconomySeats)
                .HasDefaultValue(0)
                .HasColumnName("pl_economy_seats");
            entity.Property(e => e.PlFirstClassSeats)
                .HasDefaultValue(0)
                .HasColumnName("pl_first_class_seats");
            entity.Property(e => e.PlModel)
                .HasMaxLength(50)
                .HasColumnName("pl_model");
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.ApId).HasName("airports_pk");

            entity.ToTable("airports", "skytickets");

            entity.Property(e => e.ApId)
                .ValueGeneratedOnAdd()
                .HasColumnName("ap_id");
            entity.Property(e => e.ApBuilding)
                .HasMaxLength(10)
                .HasColumnName("ap_building");
            entity.Property(e => e.ApCity)
                .HasMaxLength(30)
                .HasColumnName("ap_city");
            entity.Property(e => e.ApCountry)
                .HasMaxLength(25)
                .HasColumnName("ap_country");
            entity.Property(e => e.ApName)
                .HasMaxLength(45)
                .HasColumnName("ap_name");
            entity.Property(e => e.ApStreet)
                .HasMaxLength(50)
                .HasColumnName("ap_street");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BId).HasName("bookings_pk");

            entity.ToTable("bookings", "skytickets");

            entity.Property(e => e.BId)
                .ValueGeneratedOnAdd()
                .HasColumnName("b_id");
            entity.Property(e => e.BCreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("b_created_at");
            entity.Property(e => e.BFlight).HasColumnName("b_flight");
            entity.Property(e => e.BStatus).HasColumnName("b_status");
            entity.Property(e => e.BTotalPrice).HasColumnName("b_total_price");
            entity.Property(e => e.BUser).HasColumnName("b_user");

            entity.HasOne(d => d.BFlightNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BFlight)
                .HasConstraintName("bookings_flights_fk");

            entity.HasOne(d => d.BUserNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BUser)
                .HasConstraintName("bookings_users_fk");
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("flights_pk");

            entity.ToTable("flights", "skytickets");

            entity.Property(e => e.FId)
                .ValueGeneratedOnAdd()
                .HasColumnName("f_id");
            entity.Property(e => e.FAirline).HasColumnName("f_airline");
            entity.Property(e => e.FAirplane).HasColumnName("f_airplane");
            entity.Property(e => e.FArrivalAirport).HasColumnName("f_arrival_airport");
            entity.Property(e => e.FArrivalTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("f_arrival_time");
            entity.Property(e => e.FBasePrice).HasColumnName("f_base_price");
            entity.Property(e => e.FDepartureAirport).HasColumnName("f_departure_airport");
            entity.Property(e => e.FDepartureTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("f_departure_time");

            entity.HasOne(d => d.FAirlineNavigation).WithMany(p => p.Flights)
                .HasForeignKey(d => d.FAirline)
                .HasConstraintName("flights_airlines_fk");

            entity.HasOne(d => d.FAirplaneNavigation).WithMany(p => p.Flights)
                .HasForeignKey(d => d.FAirplane)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("flights_airplanes_fk");

            entity.HasOne(d => d.FArrivalAirportNavigation).WithMany(p => p.FlightFArrivalAirportNavigations)
                .HasForeignKey(d => d.FArrivalAirport)
                .HasConstraintName("flights_arr_airports_fk");

            entity.HasOne(d => d.FDepartureAirportNavigation).WithMany(p => p.FlightFDepartureAirportNavigations)
                .HasForeignKey(d => d.FDepartureAirport)
                .HasConstraintName("flights_dep_airports_fk");
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.HasKey(e => e.PId).HasName("passengers_pk");

            entity.ToTable("passengers", "skytickets");

            entity.HasIndex(e => new { e.PPassportSerial, e.PPassportNumber }, "passengers_unique_passport").IsUnique();

            entity.Property(e => e.PId)
                .ValueGeneratedOnAdd()
                .HasColumnName("p_id");
            entity.Property(e => e.PBirthdate).HasColumnName("p_birthdate");
            entity.Property(e => e.PName)
                .HasMaxLength(30)
                .HasColumnName("p_name");
            entity.Property(e => e.PPassportNumber)
                .HasMaxLength(6)
                .HasColumnName("p_passport_number");
            entity.Property(e => e.PPassportSerial)
                .HasMaxLength(4)
                .HasColumnName("p_passport_serial");
            entity.Property(e => e.PPatronymic)
                .HasMaxLength(45)
                .HasColumnName("p_patronymic");
            entity.Property(e => e.PSurname)
                .HasMaxLength(45)
                .HasColumnName("p_surname");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TId).HasName("tickets_pk");

            entity.ToTable("tickets", "skytickets");

            entity.Property(e => e.TId)
                .ValueGeneratedOnAdd()
                .HasColumnName("t_id");
            entity.Property(e => e.TBooking).HasColumnName("t_booking");
            entity.Property(e => e.TClass).HasColumnName("t_class");
            entity.Property(e => e.TPassenger).HasColumnName("t_passenger");
            entity.Property(e => e.TPrice).HasColumnName("t_price");

            entity.HasOne(d => d.TBookingNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TBooking)
                .HasConstraintName("tickets_bookings_fk");

            entity.HasOne(d => d.TPassengerNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TPassenger)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_passengers_fk");

            entity.HasMany(d => d.TsServices).WithMany(p => p.TsTickets)
                .UsingEntity<Dictionary<string, object>>(
                    "TicketService",
                    r => r.HasOne<AdditionalService>().WithMany()
                        .HasForeignKey("TsService")
                        .HasConstraintName("ts_services_fk"),
                    l => l.HasOne<Ticket>().WithMany()
                        .HasForeignKey("TsTicket")
                        .HasConstraintName("ts_tickets_fk"),
                    j =>
                    {
                        j.HasKey("TsTicket", "TsService").HasName("ticket_services_pk");
                        j.ToTable("ticket_services", "skytickets");
                        j.IndexerProperty<int>("TsTicket").HasColumnName("ts_ticket");
                        j.IndexerProperty<int>("TsService").HasColumnName("ts_service");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UId).HasName("users_pk");

            entity.ToTable("users", "skytickets");

            entity.HasIndex(e => e.UEmail, "users_unique_email").IsUnique();

            entity.HasIndex(e => e.UPhone, "users_unique_phone").IsUnique();

            entity.Property(e => e.UId)
                .ValueGeneratedOnAdd()
                .HasColumnName("u_id");
            entity.Property(e => e.UBirthdate).HasColumnName("u_birthdate");
            entity.Property(e => e.UEmail).HasColumnName("u_email");
            entity.Property(e => e.UName)
                .HasMaxLength(30)
                .HasColumnName("u_name");
            entity.Property(e => e.UPassword).HasColumnName("u_password");
            entity.Property(e => e.UPatronymic)
                .HasMaxLength(45)
                .HasColumnName("u_patronymic");
            entity.Property(e => e.UPhone)
                .HasMaxLength(20)
                .HasColumnName("u_phone");
            entity.Property(e => e.URole).HasColumnName("u_role");
            entity.Property(e => e.USurname)
                .HasMaxLength(45)
                .HasColumnName("u_surname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
