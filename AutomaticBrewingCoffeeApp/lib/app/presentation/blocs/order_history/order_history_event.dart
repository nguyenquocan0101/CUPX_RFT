part of 'order_history_bloc.dart';


@immutable
abstract class OrderHistoryEvent extends Equatable {
  
}

class GetOrderPaginationEvent extends OrderHistoryEvent 
{
  final int page;
  final int size;
  final String? orderStatus;
  final bool isAsc;
  final DateTime? startTime;
  final DateTime? endTime;

  GetOrderPaginationEvent({
    required this.page, 
    required this.size, 
    this.orderStatus, 
    this.isAsc = false,
    this.startTime,
    this.endTime,
    });
  @override
  List<Object?> get props => [page, size, orderStatus, isAsc];
}
