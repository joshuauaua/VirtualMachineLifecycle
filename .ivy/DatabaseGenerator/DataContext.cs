using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VirtualMachineLifecycle;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Vm> Vms { get; set; } = null!;
    public DbSet<Snapshot> Snapshots { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<UserType> UserTypes { get; set; } = null!;
    public DbSet<VmStatus> VmStatuses { get; set; } = null!;
    public DbSet<Provider> Providers { get; set; } = null!;
    public DbSet<AuditAction> AuditActions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserType>(entity =>
        {
            entity.ToTable("user_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DescriptionText).IsRequired().HasMaxLength(100);
            entity.HasData(UserType.GetSeedData());
        });

        modelBuilder.Entity<VmStatus>(entity =>
        {
            entity.ToTable("vm_statuses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DescriptionText).IsRequired().HasMaxLength(100);
            entity.HasData(VmStatus.GetSeedData());
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.ToTable("providers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DescriptionText).IsRequired().HasMaxLength(100);
            entity.HasData(Provider.GetSeedData());
        });

        modelBuilder.Entity<AuditAction>(entity =>
        {
            entity.ToTable("audit_actions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DescriptionText).IsRequired().HasMaxLength(100);
            entity.HasData(AuditAction.GetSeedData());
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.UserTypeId).IsRequired();
            entity.HasOne(e => e.UserType)
                .WithMany()
                .HasForeignKey(e => e.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vm>(entity =>
        {
            entity.ToTable("vms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.VmStatusId).IsRequired();
            entity.Property(e => e.ProviderId).IsRequired();
            entity.HasOne(e => e.VmStatus)
                .WithMany()
                .HasForeignKey(e => e.VmStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Snapshot>(entity =>
        {
            entity.ToTable("snapshots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.VmId).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.HasOne(e => e.Vm)
                .WithMany(v => v.Snapshots)
                .HasForeignKey(e => e.VmId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.Snapshots)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionId).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Vm)
                .WithMany(v => v.AuditLogs)
                .HasForeignKey(e => e.VmId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AuditAction)
                .WithMany()
                .HasForeignKey(e => e.ActionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public int UserTypeId { get; set; }
    public UserType UserType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Snapshot> Snapshots { get; set; } = null!;
    public ICollection<AuditLog> AuditLogs { get; set; } = null!;
}

public class Vm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int VmStatusId { get; set; }
    public VmStatus VmStatus { get; set; } = null!;
    public int ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;
    public string? Tags { get; set; }
    public DateTime? LastAction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Snapshot> Snapshots { get; set; } = null!;
    public ICollection<AuditLog> AuditLogs { get; set; } = null!;
}

public class Snapshot
{
    public int Id { get; set; }
    public int VmId { get; set; }
    public Vm Vm { get; set; } = null!;
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? VmId { get; set; }
    public Vm? Vm { get; set; }
    public int ActionId { get; set; }
    public AuditAction AuditAction { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

public class UserType
{
    public int Id { get; set; }
    public string DescriptionText { get; set; } = null!;

    public static UserType[] GetSeedData()
    {
        return new UserType[]
        {
            new UserType { Id = 1, DescriptionText = "Viewer" },
            new UserType { Id = 2, DescriptionText = "Operator" }
        };
    }
}

public class VmStatus
{
    public int Id { get; set; }
    public string DescriptionText { get; set; } = null!;

    public static VmStatus[] GetSeedData()
    {
        return new VmStatus[]
        {
            new VmStatus { Id = 1, DescriptionText = "Running" },
            new VmStatus { Id = 2, DescriptionText = "Stopped" }
        };
    }
}

public class Provider
{
    public int Id { get; set; }
    public string DescriptionText { get; set; } = null!;

    public static Provider[] GetSeedData()
    {
        return new Provider[]
        {
            new Provider { Id = 1, DescriptionText = "AWS" },
            new Provider { Id = 2, DescriptionText = "Azure" }
        };
    }
}

public class AuditAction
{
    public int Id { get; set; }
    public string DescriptionText { get; set; } = null!;

    public static AuditAction[] GetSeedData()
    {
        return new AuditAction[]
        {
            new AuditAction { Id = 1, DescriptionText = "Login" },
            new AuditAction { Id = 2, DescriptionText = "Logout" },
            new AuditAction { Id = 3, DescriptionText = "Snapshot Created" },
            new AuditAction { Id = 4, DescriptionText = "Snapshot Deleted" },
            new AuditAction { Id = 5, DescriptionText = "Snapshot Restored" },
            new AuditAction { Id = 6, DescriptionText = "VM Start" },
            new AuditAction { Id = 7, DescriptionText = "VM Stop" },
            new AuditAction { Id = 8, DescriptionText = "Reboot" },
            new AuditAction { Id = 9, DescriptionText = "Delete" },
            new AuditAction { Id = 10, DescriptionText = "View" }
        };
    }
}