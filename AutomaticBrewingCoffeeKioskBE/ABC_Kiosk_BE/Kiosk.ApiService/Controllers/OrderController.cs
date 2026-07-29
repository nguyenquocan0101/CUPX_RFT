using AutomaticBrewingCoffee.API.Constants;
using Kiosk.ApiService.Saga.Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Order;
using Services.Interfaces;

namespace Kiosk.ApiService.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/orders")]
    [ApiController]
    public class OrderController(IRequestClient<PrepareOrder> prepareOrder, IOrderService orderService, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpPost("prepare")]
        [EndpointSummary("API for create order")]
        public async Task<IActionResult> PrepareOrder([FromBody] CreateLocalOrderDto createLocalOrderDto)
        {
            try
            {
                var response = await prepareOrder.GetResponse<OrderPrepared>(new PrepareOrder()
                {
                    CorrelationId = Guid.NewGuid(),
                    Request = createLocalOrderDto,
                    PaymentGateway = createLocalOrderDto.PaymentGateway.ToString(),
                });
                var dataOject = response.Message;
                var result = new BaseResult<CreateLocalOrderDto, OrderPreparedDto>
                {
                    IsSuccess = true,
                    Message = "Accepted",
                    Request = createLocalOrderDto,
                    Response = new OrderPreparedDto(dataOject.OrderId, dataOject.PaymentUrl, dataOject.PaymentQr, dataOject.OrderPrepareddAt, dataOject.OrderDetails),
                    StatusCode = StatusCodes.Status202Accepted,
                };
                return StatusCode(result.StatusCode, result);
            }
            catch (RequestFaultException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResult<CreateLocalOrderDto, OrderPreparedDto>()
                {
                    Message = ex.Message,
                    IsSuccess = false,
                    Request = createLocalOrderDto,
                    StatusCode = StatusCodes.Status500InternalServerError,
                });
            }
        }

        [HttpPost("/api/v1/payment-callback")]
        [EndpointSummary("API for cloud calling back for fire workflow flow")]
        public async Task<IActionResult> CallbackForOrder(CallbackOrderDto data)
        {
            var order = orderService.GetOrderWithOutBaseRsReturn(data.OrderId);
            if(order == null) return BadRequest("order not exist");

            await publishEndpoint.Publish(new OderCalledBack(Guid.NewGuid(), data.OrderId, data.Status));

            return Accepted();
        }

        [HttpGet()]
        public async Task<IActionResult> GetOrders([FromQuery] OrderQueryDto query)
        {

            var result = await orderService.GetOrders(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(string orderId)
        {

            var result = await orderService.GetOrder(orderId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
