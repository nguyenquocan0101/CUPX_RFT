import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/models/order.dart';
import 'package:fpdart/fpdart.dart' as fp;

abstract class OrderRepository {
  Future<fp.Either<String, OrderPrepared>> createOrder(CreateOrderRequest request); 
  Future<Pagination<Order>> getOrderHistory(OrderQueryDto query); 
  Future<void> cancelOrder(String orderId);
}