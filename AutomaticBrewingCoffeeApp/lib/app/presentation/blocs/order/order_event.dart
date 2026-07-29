part of 'order_bloc.dart';

class OrderEvent extends Equatable {
  const OrderEvent();

  @override
  List<Object> get props => [];
}

class CreateOrderEvent extends OrderEvent {
  final List<CartItem> items;
  final PaymentGateway paymentGateway;
  final String? discountCode;
  CreateOrderEvent(this.paymentGateway, this.items, {this.discountCode});
}

class CancelOrderEvent extends OrderEvent {
  final String orderId;
  CancelOrderEvent(this.orderId);
}
  

