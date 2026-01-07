using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

[Table("audit_logs")]
[Index("ActionId", Name = "IX_audit_logs_ActionId")]
[Index("UserId", Name = "IX_audit_logs_UserId")]
[Index("VmId", Name = "IX_audit_logs_VmId")]
public partial class AuditLog
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? VmId { get; set; }

    public int ActionId { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Details { get; set; }

    [ForeignKey("ActionId")]
    [InverseProperty("AuditLogs")]
    public virtual AuditAction Action { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("VmId")]
    [InverseProperty("AuditLogs")]
    public virtual Vm? Vm { get; set; }
}
