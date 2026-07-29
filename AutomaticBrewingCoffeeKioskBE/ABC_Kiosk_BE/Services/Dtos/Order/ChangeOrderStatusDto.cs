using System.ComponentModel.DataAnnotations;
using Services.Validations;
using Domain.Enums;

namespace Services.Dtos.Order;

public class ChangeOrderStatusDto
{
    public OrderStatus Status { get; set; }
}