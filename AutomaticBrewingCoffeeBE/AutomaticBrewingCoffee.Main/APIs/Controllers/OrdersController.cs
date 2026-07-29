using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Order;
using Services.Dtos.OrderDetail;
using Services.Dtos.Payment;
using Services.Interfaces;
using Services.MPOS.Base;
using Services.VNPay.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/orders")]
    [ApiController]
    [TrimStrings]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Create a new order",
            Description = "Creates a new order with details such as items, payment method, and order type."
        )]
        public async Task<ActionResult<BaseResult<CreateOrderDto, OrderDto>>> Post(CreateOrderDto createOrderDto)
        {
            var response = await _orderService.CreateOrder(createOrderDto);
            return StatusCode(response.StatusCode, response);
        }

        // [HttpPut("{orderId}/status")]
        // [Authorizes(nameof(ERoleName.Admin))]
        // [SwaggerOperation(
        //     Summary = "Update only order status",
        //     Description = "Updates the status of an order by its ID. For example: Created → Processing → Completed."
        // )]
        // public async Task<ActionResult<BaseResult<string, OrderDto>>> Put(
        //     [FromRoute] string orderId,
        //     [FromBody] ChangeOrderStatusDto changeOrderStatusDto
        // )
        // {
        //     var response = await _orderService.ChangeOrderStatus(orderId, changeOrderStatusDto);
        //     return StatusCode(response.StatusCode, response);
        // }

        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of orders",
            Description =
                "Retrieves a paginated list of orders with optional filters like status, order type, and payment gateway."
        )]
        public async Task<ActionResult<BaseResult<OrderQueryDto, Paginate<OrderDto>>>> Get(
            [FromQuery] OrderQueryDto orderQueryDto)
        {
            var response = await _orderService.GetOrders(orderQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{orderId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get order details",
            Description = "Retrieves detailed information about a specific order by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, OrderDto>>> Get(string orderId)
        {
            var response = await _orderService.GetOrder(orderId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{orderId}/order-details")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get order detail of a order",
            Description = "Retrieves detailed information about a order-details by order id."
        )]
        public async Task<ActionResult<BaseResult<OrderDetailQueryDto, Paginate<OrderDetailDto>>>> Get(
            [FromRoute] string orderId,
            [FromQuery] OrderDetailQueryDto orderDetailQueryDto
        )
        {
            var response = await _orderService.GetOrderDetails(orderId, orderDetailQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{orderId}/payment")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get payments of a order",
            Description = "Retrieves detailed information about a specific payment by order id."
        )]
        public async Task<ActionResult<BaseResult<PaymentQueryDto, Paginate<PaymentDto>>>> Get(
            [FromRoute] string orderId,
            [FromQuery] PaymentQueryDto paymentQueryDto
        )
        {
            var response = await _orderService.GetPayments(orderId, paymentQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // [HttpPost("{orderId}/mpos-cancel")]
        // [SwaggerOperation(Summary = "Cancel QR payment from MPOS")]
        // public async Task<ActionResult<BaseResult<CancelMPOSQRResponse>>> CancelMPOSQR([FromRoute] string orderId)
        // {
        //     var response = await _orderService.CancelMPOSQRPayment(orderId);
        //     return StatusCode(response.StatusCode, response);
        // }

        // [HttpGet("{orderId}/mpos-system")]
        // [SwaggerOperation(Summary = "Get QR payment status from MPOS")]
        // public async Task<ActionResult<BaseResult<CancelMPOSQRResponse>>> GetMPOSQRStatus([FromRoute] string orderId)
        // {
        //     var response = await _orderService.GetMPOSQRPaymentStatus(orderId);
        //     return StatusCode(response.StatusCode, response);
        // }

        [HttpPut("{orderId}/refund")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(Summary = "Refund the order")]
        public async Task<ActionResult<BaseResult<RefundOrderDto, OrderDto>>> RefundOrder([FromRoute] string orderId,
            [FromBody] RefundOrderDto refundOrderDto)
        {
            var response = await _orderService.RefundOrder(orderId, refundOrderDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("mpos-system/payment-callback")]
        [SwaggerOperation(Summary = "Get the transaction status from MPOS")]
        public async Task<ActionResult<MPOSCallbackRequest>> HandleMPOSPaymentCallback(
            [FromBody] MPOSCallbackRequest request)
        {
            var response = await _orderService.HandleMPOSPaymentCallback(request);
            return StatusCode(response.ResCode, response);
        }

        [HttpGet("vnpay-system/payment-callback")]
        [SwaggerOperation(Summary = "Get the transaction status from VNPay")]
        public async Task<ActionResult> HandleVNPayPaymentCallback([FromQuery] VNPAYCallbackRequest request)
        {
            var response = await _orderService.HandleVNPAYPaymentCallback(request);
            return Ok(response);
        }

        [HttpPut("complete")]
        [ApiKeyAuth]
        [SwaggerOperation(Summary = "Get the status of order from kiosk (Webhook)")]
        public async Task<ActionResult> HandleOrderCompleteCallback([FromBody] OrderKioskCompleteCallbackDto request)
        {
            var response = await _orderService.HandleOrderCompleteCallback(request);
            return Ok(response);
        }

        [HttpPut("fail")]
        [ApiKeyAuth]
        [SwaggerOperation(Summary = "Get the status of order from kiosk (Webhook)")]
        public async Task<ActionResult> HandleOrderFailCallback([FromBody] OrderKioskFailCallbackDto request)
        {
            var response = await _orderService.HandleOrderFailCallback(request);
            return Ok(response);
        }

        [HttpPut("cancel")]
        [ApiKeyAuth]
        [SwaggerOperation(Summary = "Cancel the order from kiosk (Webhook)")]
        public async Task<ActionResult<BaseResult<CancelOrderDto, OrderDto>>> CancelOrder(
            [FromBody] CancelOrderDto request)
        {
            var response = await _orderService.CancelOrder(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("export")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(Summary = "Export the order")]
        public async Task<ActionResult> ExportOrder([FromQuery] OrderQueryDto query)
        {
            var response = await _orderService.ExportOrder(query);

            if (response is null)
            {
                return NotFound();
            }

            return File(response, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
        }
    }
}