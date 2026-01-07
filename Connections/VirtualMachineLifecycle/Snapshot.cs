using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

[Table("snapshots")]
[Index("CreatedBy", Name = "IX_snapshots_CreatedBy")]
[Index("VmId", Name = "IX_snapshots_VmId")]
public partial class Snapshot
{
    [Key]
    public int Id { get; set; }

    public int VmId { get; set; }

    public int CreatedBy { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Snapshots")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("VmId")]
    [InverseProperty("Snapshots")]
    public virtual Vm Vm { get; set; } = null!;
}
