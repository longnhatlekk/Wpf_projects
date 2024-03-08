using System;
using System.Collections.Generic;

namespace WPF_MachineService.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public string? Description { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public bool? Status { get; set; }

    public int? ImageId { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
