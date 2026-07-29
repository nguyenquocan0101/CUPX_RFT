using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Order;

public class ExportOrderDto
{
    [Display(Name = "Mã đơn hàng")] public string OrderId { get; set; } = null!;

    [Display(Name = "Mã code")] public string? OrderCode { get; set; }

    [Display(Name = "Ki-ốt nhận đơn")] public string KioskId { get; set; } = null!;

    [Display(Name = "Thiết bị đặt hàng")] public string ClientId { get; set; } = null!;

    [Display(Name = "Tổ chức")] public string? OrganizationId { get; set; }

    [Display(Name = "Cửa hàng")] public string? StoreId { get; set; }

    [Display(Name = "Giảm giá")]
    [DisplayFormat(DataFormatString = "#,##0.##")]
    public decimal? Discount { get; set; }

    [Display(Name = "Mã giảm giá")] public string? DiscountCode { get; set; }

    [Display(Name = "Thành tiền")]
    [DisplayFormat(DataFormatString = "#,##0.##")]
    public decimal? FinalAmount { get; set; }

    [Display(Name = "Loại đơn")] public string? OrderType { get; set; }

    [Display(Name = "Cổng thanh toán")] public string? PaymentGateway { get; set; }

    [Display(Name = "Trạng thái")] public string? Status { get; set; }

    [Display(Name = "Người cập nhật cuối")]
    public string? LastUpdateBy { get; set; }

    [Display(Name = "Tổng tiền")]
    [DisplayFormat(DataFormatString = "#,##0.##")]
    public decimal? TotalAmount { get; set; }

    [Display(Name = "Ngày tạo")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime CreatedDate { get; set; }

    [Display(Name = "Ngày cập nhật")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "Ngày chờ xử lý")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime? PendingDate { get; set; }

    [Display(Name = "Ngày đang chuẩn bị")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime? PreparingDate { get; set; }

    [Display(Name = "Ngày hoàn thành")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime? CompletedDate { get; set; }

    [Display(Name = "Ngày huỷ")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime? CancelledDate { get; set; }

    [Display(Name = "Ngày thất bại")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
    public DateTime? FailedDate { get; set; }
}