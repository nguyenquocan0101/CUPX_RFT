using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

public class OrderDetail : BaseModel
{
    [Key] [StringLength(50)] public string OrderDetailId { get; set; } = null!;

    [Required] [StringLength(50)] public string OrderId { get; set; } = null!;

    [Required] [StringLength(50)] public string ProductId { get; set; } = null!;

    [StringLength(450)] public string? ProductAttributes { get; set; }

    [Precision(18, 2)] public decimal TotalAmount { get; set; }

    [Precision(18, 2)] public decimal SellingPrice { get; set; }

    [StringLength(100)] public string? ProductName { get; set; }

    [StringLength(300)] public string? ProductDescription { get; set; }

    public int Quantity { get; set; }

    [ForeignKey(nameof(OrderId))] public Order? Order { get; set; } = null!;

    [ForeignKey(nameof(ProductId))] public Product? Product { get; set; }
}