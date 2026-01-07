using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;

[Table("vm_statuses")]
public partial class VmStatus
{
    [Key]
    public int Id { get; set; }

    public string DescriptionText { get; set; } = null!;

    [InverseProperty("VmStatus")]
    public virtual ICollection<Vm> Vms { get; set; } = new List<Vm>();
}
