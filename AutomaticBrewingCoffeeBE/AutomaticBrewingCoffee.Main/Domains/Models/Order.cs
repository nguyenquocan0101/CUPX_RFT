using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Order : BaseModel
{
    [Key] [StringLength(50)] public string OrderId { get; set; } = null!;

    [StringLength(50)] public string? OrderCode { get; set; }

    [StringLength(50)] public string KioskId { get; set; } = null!;

    [StringLength(50)] public string ClientId { get; set; } = null!;

    [StringLength(50)] public string? OrganizationId { get; set; } = null!;

    [StringLength(50)] public string? StoreId { get; set; } = null!;

    [ForeignKey(nameof(KioskId))] public Kiosk? Kiosk { get; set; }

    [Precision(18, 2)] public decimal? Discount { get; set; }

    [StringLength(100)] public string? DiscountCode { get; set; }

    [Precision(18, 2)] public decimal? FinalAmount { get; set; }

    [StringLength(20)] public string? OrderType { get; set; }

    [StringLength(50)] public string? PaymentGateway { get; set; }

    [StringLength(20)] public string? Status { get; set; }

    [StringLength(100)] public string? UpdateBy { get; set; }

    [Precision(18, 2)] public decimal? TotalAmount { get; set; }

    public DateTime? PendingDate { get; set; }

    public DateTime? PreparingDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public DateTime? FailedDate { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [StringLength(1000)] public string? CompletedProductIds { get; set; }

    [StringLength(1000)] public string? PreparingProductIds { get; set; }

    [StringLength(1000)] public string? FailedProductIds { get; set; }

    public void Calculate(decimal discountPercent = 0)
    {
        TotalAmount = OrderDetails.Sum(x => x.TotalAmount);
        Discount = discountPercent * TotalAmount / 100;
        FinalAmount = (TotalAmount ?? 0) - (Discount ?? 0);
    }

    public void Pending(string updateBy = "System")
    {
        Status = EOrderStatus.Pending.ToString();
        UpdateBy = updateBy;
        PendingDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Preparing(string updateBy = "System")
    {
        Status = EOrderStatus.Preparing.ToString();
        UpdateBy = updateBy;
        PreparingDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Completed(string updateBy = "System")
    {
        Status = EOrderStatus.Completed.ToString();
        UpdateBy = updateBy;
        CompletedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Cancelled(string updateBy = "System")
    {
        Status = EOrderStatus.Cancelled.ToString();
        UpdateBy = updateBy;
        CancelledDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Failed(string updateBy = "System")
    {
        Status = EOrderStatus.Failed.ToString();
        UpdateBy = updateBy;
        FailedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }
}