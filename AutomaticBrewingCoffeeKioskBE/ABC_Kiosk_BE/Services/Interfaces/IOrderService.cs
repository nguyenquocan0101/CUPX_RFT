using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Pagination;
using Services.Base;
using Services.Dtos.Order;
using Services.Dtos.Payment;

namespace Services.Interfaces
{
    public interface IOrderService
    {
        Task<BaseResult<OrderQueryDto, Paginate<LocalOrderDto>>> GetOrders(OrderQueryDto orderQueryDto);
        Task<BaseResult<string, LocalOrderDto>> GetOrder(string orderId);
        LocalOrder? GetOrderWithOutBaseRsReturn(string orderId);
        Task<PrepareOrderDto> CreateOrder(CreateLocalOrderDto createOrderDto);
    }
}
