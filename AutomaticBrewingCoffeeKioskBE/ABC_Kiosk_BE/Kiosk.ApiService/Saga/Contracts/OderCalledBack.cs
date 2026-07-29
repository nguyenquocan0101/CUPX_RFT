using Domain.Enums;
using MassTransit;

namespace Kiosk.ApiService.Saga.Contracts;

public record OderCalledBack(Guid CorrelationId, string OrderId, OrderStatus Status) : CorrelatedBy<Guid>;
public record QueueOrder(Guid CorrelationId, string OrderId) : CorrelatedBy<Guid>;
