using AtencionClientesAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");

            entity.HasKey(cliente => cliente.Id);

            entity.Property(cliente => cliente.Nombre)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(cliente => cliente.Telefono)
                .HasMaxLength(25);

            entity.Property(cliente => cliente.Correo)
                .HasMaxLength(254);
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("Empleados");

            entity.HasKey(empleado => empleado.Id);

            entity.Property(empleado => empleado.Nombre)
                .IsRequired()
                .HasMaxLength(120);
        });

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.ToTable("Turnos", table =>
            {
                table.HasCheckConstraint(
                    "CK_Turnos_Estado",
                    "[Estado] IN (0, 1, 2, 3)");

                table.HasCheckConstraint(
                    "CK_Turnos_EmpleadoEnAtencion",
                    "[Estado] <> 1 OR [EmpleadoId] IS NOT NULL");
            });

            entity.HasKey(turno => turno.Id);

            entity.Property(turno => turno.Estado)
                .HasConversion<int>();

            entity.Property(turno => turno.Version)
                .IsRowVersion();

            entity.HasOne(turno => turno.Cliente)
                .WithMany(cliente => cliente.Turnos)
                .HasForeignKey(turno => turno.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(turno => turno.Empleado)
                .WithMany(empleado => empleado.Turnos)
                .HasForeignKey(turno => turno.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(turno => new
            {
                turno.Estado,
                turno.FechaCreacion,
                turno.Id
            });

            entity.HasIndex(turno => turno.EmpleadoId)
                .IsUnique()
                .HasFilter("[EmpleadoId] IS NOT NULL AND [Estado] = 1")
                .HasDatabaseName("UX_Turnos_EmpleadoEnAtencion");
        });
    }
}