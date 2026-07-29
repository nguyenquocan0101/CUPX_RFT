using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Order;
using Services.Dtos.OrderDetail;
using Services.Dtos.Payment;
using Services.MPOS.Base;
using Services.MPOS.Data;
using Services.VNPay.Base;

namespace Services.Interfaces
{
    public interface IOrderService
    {
        Task<BaseResult<CreateOrderDto, OrderDto>> CreateOrder(CreateOrderDto createOrderDto);

        Task<BaseResult<CancelOrderDto, OrderDto>> CancelOrder(CancelOrderDto cancelOrderDto);

        Task<BaseResult<RefundOrderDto, OrderDto>> RefundOrder(string orderId, RefundOrderDto refundOrderDto);

        Task<BaseResult<OrderQueryDto, Paginate<OrderDto>>> GetOrders(OrderQueryDto orderQueryDto);

        Task<BaseResult<string, OrderDto>> GetOrder(string orderId);

        Task<BaseResult<string, OrderDto>> ChangeOrderStatus(string orderId, ChangeOrderStatusDto changeOrderStatusDto);

        Task<BaseResult<OrderDetailQueryDto, Paginate<OrderDetailDto>>> GetOrderDetails(string orderId,
            OrderDetailQueryDto orderDetailQueryDto);

        Task<BaseResult<PaymentQueryDto, Paginate<PaymentDto>>> GetPayments(string orderId,
            PaymentQueryDto paymentQueryDto);

        Task<BaseResult<CancelMPOSQRResponse>> CancelMPOSQRPayment(string orderId);

        Task<BaseResult<string, QRStatusResponse>> GetMPOSQRPaymentStatus(string orderId);

        Task<MPOSCallbackResponse> HandleMPOSPaymentCallback(MPOSCallbackRequest request);

        Task<VNPAYCallbackResponse> HandleVNPAYPaymentCallback(VNPAYCallbackRequest request);

        Task<BaseResult<object, object>> HandleOrderCompleteCallback(
            OrderKioskCompleteCallbackDto orderCompleteCallbackDto);

        Task<BaseResult<object, object>> HandleOrderFailCallback(OrderKioskFailCallbackDto orderFailCallbackDto);

        Task<MemoryStream?> ExportOrder(OrderQueryDto query);
    }
}