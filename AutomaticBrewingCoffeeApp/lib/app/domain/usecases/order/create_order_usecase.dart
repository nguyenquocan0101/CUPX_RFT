import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/enums/payment_gateway.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/domain/repositories/order_repository.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:fpdart/fpdart.dart';

class CreateOrderUseCase {
  final OrderRepository orderRepository;

  CreateOrderUseCase({required this.orderRepository});

  Future<Either<Failure, OrderPrepared>> execute(PaymentGateway paymentGateway, List<CartItem> items, String? discountCode) async {
    try {
      List<CreateOrderDetail> orderDetails = [];
      for(var item in items){
        var orderDetail = CreateOrderDetail(productId: item.id, productName: item.name, productDescription: item.description, quantity: item.quantity, sellingPrice: item.price, selectedAttributes: item.selectedAttributes);
        orderDetails.add(orderDetail);
      }
      var request = CreateOrderRequest(
        kioskId: ApiConstants.kioskId,
        orderDetails: orderDetails,
        content: "Đặt hàng từ ứng dụng",
        clientId: ApiConstants.clientId,
        paymentGateway: paymentGateway,
        discountCode: discountCode,
      );
      final result = await orderRepository.createOrder(request);

       return result.fold(
        (errorMessage) => Left(ApiFailure(errorMessage)), // ✅ Pass server message
        (orderPrepared) => Right(orderPrepared),
      );

    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}