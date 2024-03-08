using System;
using System.Collections.Generic;

namespace WPF_MachineService.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? PaymentId { get; set; }

    public double? Total { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? DateCreated { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Payment? Payment { get; set; }
}
