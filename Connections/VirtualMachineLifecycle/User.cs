using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

[Table("users")]
[Index("UserTypeId", Name = "IX_users_UserTypeId")]
public partial class User
{
    [Key]
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public int UserTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();

    [ForeignKey("UserTypeId")]
    [InverseProperty("Users")]
    public virtual UserType UserType { get; set; } = null!;
}
