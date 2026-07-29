import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/models/order.dart';
import 'package:abc_androidapp/app/domain/repositories/order_repository.dart';
import 'package:fpdart/fpdart.dart' as fp;

class OrderRepositoryImpl extends OrderRepository {
  final OrderDatasource orderDatasource;

  OrderRepositoryImpl({required this.orderDatasource});
 
  
  @override
  Future<fp.Either<String, OrderPrepared>> createOrder(CreateOrderRequest request) async {
    try {
      var result = await orderDatasource.createLocalOrder(request);
      
      if (result.isSuccess && result.response != null) {
        return fp.Right(result.response!);
      } else {
        // ✅ Return error message from server
        final errorMessage = result.message ?? 'Không thể tạo đơn hàng';
        return fp.Left(errorMessage);
      }
    } catch (e) {
      return fp.Left('Lỗi kết nối: ${e.toString()}');
    }
  }

  @override
  Future<Pagination<Order>> getOrderHistory(OrderQueryDto query) async {
    var result = await orderDatasource.getLocalOrderPagination(query);
    return result.response!;
  }
  
  @override
  Future<fp.Either<Failure, String>> cancelOrder(String orderId) async {
    try {
      final result = await orderDatasource.cancelOrder(orderId);
      if (result.isSuccess) {
        return fp.Right(result.message);
      } else {
        return fp.Left(ApiFailure(result.message));
      }
    } on ApiException catch (e) {
      return fp.Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return fp.Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}