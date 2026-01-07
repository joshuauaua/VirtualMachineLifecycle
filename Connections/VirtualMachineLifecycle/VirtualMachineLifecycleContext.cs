using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

public partial class VirtualMachineLifecycleContext : DbContext
{
    public VirtualMachineLifecycleContext(DbContextOptions<VirtualMachineLifecycleContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditAction> AuditActions { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<EfmigrationsLock> EfmigrationsLocks { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<Snapshot> Snapshots { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<Vm> Vms { get; set; }

    public virtual DbSet<VmStatus> VmStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditAction>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(d => d.Action).WithMany(p => p.AuditLogs).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Vm).WithMany(p => p.AuditLogs).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EfmigrationsLock>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Snapshot>(entity =>
        {
            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Snapshots).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Vm).WithMany(p => p.Snapshots).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(d => d.UserType).WithMany(p => p.Users).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Vm>(entity =>
        {
            entity.HasOne(d => d.Provider).WithMany(p => p.Vms).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.VmStatus).WithMany(p => p.Vms).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VmStatus>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
