import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/enums/payment_gateway.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/domain/repositories/order_repository.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:fpdart/fpdart.dart';

class CancelOrderUsecase {
  final OrderRepository orderRepository;

  CancelOrderUsecase({required this.orderRepository});

 Future<void> execute(String orderId) async {
   await orderRepository.cancelOrder(
     orderId
   );
  }
}
