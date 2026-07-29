import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/enums/payment_gateway.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/domain/usecases/order/cancel_order_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/order/create_order_usecase.dart';
import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';

part 'order_event.dart';
part 'order_state.dart';

class OrderBloc extends Bloc<OrderEvent, OrderState> {
  final CreateOrderUseCase createOrderUseCase;
  final CancelOrderUsecase cancelOrderUsecase;
  OrderBloc({
    required this.createOrderUseCase,
    required this.cancelOrderUsecase,
  }) : super(OrderInitial()) {
    on<OrderEvent>((event, emit) {
   
    });

    on<CreateOrderEvent>((event, emit) async {
      emit(OrderLoading());
      final response = await createOrderUseCase.execute(event.paymentGateway, event.items, event.discountCode);
      response.fold(
        (failure) => emit(OrderError(message: failure.message)),
        (data) => emit(CreateOrderDone(
          orderPreparedInfo: data,
        )),
      );
    });
    on<CancelOrderEvent>((event, emit) async {
      emit(OrderLoading());
      
      final result = await cancelOrderUsecase.execute(
        event.orderId
      );

    });
  }
}
