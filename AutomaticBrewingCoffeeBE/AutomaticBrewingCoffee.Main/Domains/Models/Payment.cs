using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Payment : BaseModel
{
    [Key] [StringLength(50)] public string PaymentId { get; set; } = null!;

    [StringLength(50)] public string? OrderId { get; set; }

    [StringLength(255)] public string? PaymentContent { get; set; }

    [Precision(18, 2)] public decimal? RequiredAmount { get; set; }

    public DateTime? ExpiredDate { get; set; }

    [StringLength(100)] public string? ReferenceId { get; set; }

    [Precision(18, 2)] public decimal? PaidAmount { get; set; }

    [Precision(18, 2)] public decimal? RefundedAmount { get; set; }

    [StringLength(50)] public string? PaymentStatus { get; set; }

    [StringLength(100)] public string? CreateBy { get; set; }

    [StringLength(100)] public string? UpdateBy { get; set; }

    [ForeignKey(nameof(OrderId))] public virtual Order? Order { get; set; }

    public DateTime? PaymentDate { get; set; }


    public void Pending(DateTime expiredDate)
    {
        PaidAmount = 0;
        CreateBy = CreateBy;
        ExpiredDate = expiredDate;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Pending.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Success()
    {
        PaidAmount = RequiredAmount;
        CreateBy = CreateBy;
        CreatedDate = CreatedDate;
        ExpiredDate = ExpiredDate;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Success.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Failed()
    {
        PaidAmount = 0;
        CreateBy = CreateBy;
        CreatedDate = CreatedDate;
        ExpiredDate = ExpiredDate;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Failed.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Cancelled()
    {
        PaidAmount = 0;
        CreateBy = CreateBy;
        CreatedDate = CreatedDate;
        ExpiredDate = ExpiredDate;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Cancelled.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Expired()
    {
        PaidAmount = 0;
        CreateBy = CreateBy;
        CreatedDate = CreatedDate;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        ExpiredDate = DateTime.UtcNow;
    }

    public bool CheckExpired()
    {
        return ExpiredDate < DateTime.UtcNow;
    }

    public void Error()
    {
        PaidAmount = 0;
        CreateBy = null;
        CreatedDate = CreatedDate;
        ExpiredDate = null;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Error.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Refunding()
    {
        PaidAmount = PaidAmount;
        CreateBy = null;
        CreatedDate = CreatedDate;
        ExpiredDate = null;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Refunding.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Refunded(decimal? refundedAmount = 0)
    {
        PaidAmount = 0;
        CreateBy = null;
        CreatedDate = CreatedDate;
        ExpiredDate = null;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Refunded.ToString();
        PaymentDate = DateTime.UtcNow;
        RefundedAmount = refundedAmount;
    }

    public void RefundFailed()
    {
        PaidAmount = PaidAmount;
        CreateBy = null;
        CreatedDate = CreatedDate;
        ExpiredDate = null;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.RefundFailed.ToString();
        PaymentDate = DateTime.UtcNow;
    }

    public void Reversed()
    {
        PaidAmount = PaidAmount;
        CreateBy = null;
        CreatedDate = CreatedDate;
        ExpiredDate = null;
        UpdateBy = UpdateBy;
        UpdatedDate = DateTime.UtcNow;
        PaymentStatus = EPaymentStatus.Reversed.ToString();
        PaymentDate = DateTime.UtcNow;
    }
}