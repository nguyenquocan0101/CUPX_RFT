import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/enums/order_status.dart';
import 'package:abc_androidapp/app/data/models/order.dart';
import 'package:abc_androidapp/app/domain/usecases/order/get_order_history.dart';
import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:meta/meta.dart';

part 'order_history_event.dart';
part 'order_history_state.dart';

class OrderHistoryBloc extends Bloc<OrderHistoryEvent, OrderHistoryState> {
  final GetOrderHistoryUseCase getOrderHistory;
  OrderHistoryBloc({
    required this.getOrderHistory,
  }) : super(OrderHistoryInitial()) {
    on<OrderHistoryEvent>((event, emit) {});

    on<GetOrderPaginationEvent>((event, emit) async {
      emit(OrderHistoryLoading());
      var orderStatusEnum = event.orderStatus == null
          ? null
          : OrderStatusExtension.fromValueStr(event.orderStatus!);
      var query = OrderQueryDto(
          page: event.page,
          size: event.size,
          isAsc: event.isAsc,
          status: orderStatusEnum,
          fromDate: event.startTime,toDate: event.endTime);
      final response = await getOrderHistory.execute(query);
      response.fold(
        (failure) => emit(OrderHistoryError(message: failure.message)),
        (data) => emit(OrderPaginationLoaded(
          orderPagiantion: data,
        )),
      );
    });
  }
}
