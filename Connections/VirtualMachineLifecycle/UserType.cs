using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

[Table("user_types")]
public partial class UserType
{
    [Key]
    public int Id { get; set; }

    public string DescriptionText { get; set; } = null!;

    [InverseProperty("UserType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
