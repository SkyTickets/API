using API.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Model;

public partial class PostgresContext(DbContextOptions<PostgresContext> options) : DbContext(options)
{
public virtual DbSet<Airline> Airlines { get; set; }

public virtual DbSet<Airport> Airports { get; set; }

public virtual DbSet<Flight> Flights { get; set; }

public virtual DbSet<Ticket> Tickets { get; set; }

public virtual DbSet<User> Users { get; set; }


protected override void OnModelCreating(ModelBuilder modelBuilder)
{
modelBuilder
 .HasPostgresEnum<Role>(name: "role")
.HasPostgresEnum<TicketStatus>(name: "ticket_status")
.HasPostgresEnum<ClassOfService>(name: "class_of_service");

modelBuilder.Entity<Airline>(entity =>
{
entity.HasKey(e => e.AlId).HasName("airlines_pk");

entity.ToTable("airlines", "skytickets");

entity.Property(e => e.AlId)
.ValueGeneratedNever()
.HasColumnName("al_id");
entity.Property(e => e.AlEmail).HasColumnName("al_email");
entity.Property(e => e.AlName)
.HasMaxLength(45)
.HasColumnName("al_name");
});

modelBuilder.Entity<Airport>(entity =>
{
entity.HasKey(e => e.ApId).HasName("airports_pk");

entity.ToTable("airports", "skytickets");

entity.Property(e => e.ApId)
.ValueGeneratedNever()
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

modelBuilder.Entity<Flight>(entity =>
{
entity.HasKey(e => e.FId).HasName("flights_pk");

entity.ToTable("flights", "skytickets");

entity.Property(e => e.FId)
.ValueGeneratedNever()
.HasColumnName("f_id");
entity.Property(e => e.FAirline).HasColumnName("f_airline");
entity.Property(e => e.FArrivalAirport).HasColumnName("f_arrival_airport");
entity.Property(e => e.FArrivalTime)
.HasColumnType("timestamp with time zone")
.HasColumnName("f_arrival_time");
entity.Property(e => e.FDepartureAirport).HasColumnName("f_departure_airport");
entity.Property(e => e.FDepartureTime)
.HasColumnType("timestamp with time zone")
.HasColumnName("f_departure_time");
entity.Property(e => e.FSeatsCount).HasColumnName("f_seats_count");
entity.Property(e => e.FPrice).HasColumnName("f_price");

entity.HasOne(d => d.FAirlineNavigation).WithMany(p => p.Flights)
.HasForeignKey(d => d.FAirline)
.HasConstraintName("flights_airlines_fk");

entity.HasOne(d => d.FArrivalAirportNavigation).WithMany(p => p.FlightFArrivalAirportNavigations)
.HasForeignKey(d => d.FArrivalAirport)
.HasConstraintName("flights_airports_fk1");

entity.HasOne(d => d.FDepartureAirportNavigation).WithMany(p => p.FlightFDepartureAirportNavigations)
.HasForeignKey(d => d.FDepartureAirport)
.HasConstraintName("flights_airports_fk");
});

modelBuilder.Entity<Ticket>(entity =>
{
entity.HasKey(e => e.TId).HasName("tickets_pk");

entity.ToTable("tickets", "skytickets");

entity.Property(e => e.TId)
.ValueGeneratedNever()
.HasColumnName("t_id");
entity.Property(e => e.TBoughtDate)
.HasColumnType("timestamp without time zone")
.HasColumnName("t_bought_date");
entity.Property(e => e.TStatus).HasColumnName("t_status");
entity.Property(e => e.TFlight).HasColumnName("t_flight");
entity.Property(e => e.TTotalPrice).HasColumnName("t_total_price");
entity.Property(e => e.TClass).HasColumnName("t_class");
entity.Property(e => e.TUser).HasColumnName("t_user");

entity.HasOne(d => d.TFlightNavigation).WithMany(p => p.Tickets)
.HasForeignKey(d => d.TFlight)
.HasConstraintName("tickets_flights_fk");

entity.HasOne(d => d.TUserNavigation).WithMany(p => p.Tickets)
.HasForeignKey(d => d.TUser)
.HasConstraintName("tickets_users_fk");
});

modelBuilder.Entity<User>(entity =>
{
entity.HasKey(e => e.UId).HasName("users_pk");

entity.ToTable("users", "skytickets");

entity.HasIndex(e => e.UEmail, "users_unique").IsUnique();

entity.HasIndex(e => e.UPhone, "users_unique1").IsUnique();

entity.HasIndex(e => e.UPassportNumber, "users_unique2").IsUnique();

entity.Property(e => e.UId)
.ValueGeneratedNever()
.HasColumnName("u_id");
entity.Property(e => e.UBirthdate).HasColumnName("u_birthdate");
entity.Property(e => e.UEmail).HasColumnName("u_email");
entity.Property(e => e.UName)
.HasMaxLength(30)
.HasColumnName("u_name");
entity.Property(e => e.UPassportNumber)
.HasPrecision(6)
.HasColumnName("u_passport_number");
entity.Property(e => e.URole).HasColumnName("u_role");
entity.Property(e => e.UPassportSerial)
.HasPrecision(4)
.HasColumnName("u_passport_serial");
entity.Property(e => e.UPassword).HasColumnName("u_password");
entity.Property(e => e.UPatronymic)
.HasMaxLength(45)
.HasColumnName("u_patronymic");
entity.Property(e => e.UPhone)
.HasMaxLength(20)
.HasColumnName("u_phone");
entity.Property(e => e.USurname)
.HasMaxLength(45)
.HasColumnName("u_surname");
});

OnModelCreatingPartial(modelBuilder);
}

partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
