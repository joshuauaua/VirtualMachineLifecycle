using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

[Table("vms")]
[Index("ProviderId", Name = "IX_vms_ProviderId")]
[Index("VmStatusId", Name = "IX_vms_VmStatusId")]
public partial class Vm
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int VmStatusId { get; set; }

    public int ProviderId { get; set; }

    public string? Tags { get; set; }

    public DateTime? LastAction { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Vm")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [ForeignKey("ProviderId")]
    [InverseProperty("Vms")]
    public virtual Provider Provider { get; set; } = null!;

    [InverseProperty("Vm")]
    public virtual ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();

    [ForeignKey("VmStatusId")]
    [InverseProperty("Vms")]
    public virtual VmStatus VmStatus { get; set; } = null!;
}
