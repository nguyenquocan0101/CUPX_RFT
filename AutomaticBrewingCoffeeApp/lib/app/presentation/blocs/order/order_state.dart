part of 'order_bloc.dart';

sealed class OrderState extends Equatable {
  const OrderState();
  
  @override
  List<Object> get props => [];
}

class OrderLoading extends OrderState {}


final class OrderInitial extends OrderState {}

class CreateOrderDone extends OrderState {
  final OrderPrepared orderPreparedInfo;
  CreateOrderDone({required this.orderPreparedInfo});
}

class OrderError extends OrderState {
  final String message;
  OrderError({required this.message});
}
