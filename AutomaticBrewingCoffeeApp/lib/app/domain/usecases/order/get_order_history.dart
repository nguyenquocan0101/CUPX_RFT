import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/models/order.dart' as model_order;
import 'package:abc_androidapp/app/domain/repositories/order_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetOrderHistoryUseCase {
  final OrderRepository orderRepository;

  GetOrderHistoryUseCase({required this.orderRepository});

  Future<Either<Failure, Pagination<model_order.Order>>> execute(OrderQueryDto query) async {
    try {
      final result = await orderRepository.getOrderHistory(query);
      return Right(result);
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}
