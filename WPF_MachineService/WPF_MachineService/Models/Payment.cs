using System;
using System.Collections.Generic;

namespace WPF_MachineService.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public bool? Status { get; set; }

    public double? Amount { get; set; }

    public string? Method { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
