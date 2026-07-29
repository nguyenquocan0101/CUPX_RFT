part of 'order_history_bloc.dart';

@immutable
abstract class OrderHistoryState extends Equatable {
}

class OrderHistoryInitial extends OrderHistoryState {
  @override
  List<Object?> get props => [];
}

class OrderHistoryLoading extends OrderHistoryState {
  @override
  List<Object?> get props => [];
}

class OrderHistoryError extends OrderHistoryState {
  final String message;

  OrderHistoryError({required this.message});
  @override
  List<Object?> get props => [];
}

class OrderPaginationLoaded extends OrderHistoryState {
  final Pagination<Order> orderPagiantion;

  OrderPaginationLoaded({required this.orderPagiantion});
  
  @override
  List<Object?> get props => [orderPagiantion];
}
