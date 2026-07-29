import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/base_models/base_response.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/data/enums/order_status.dart';
import 'package:abc_androidapp/app/data/enums/payment_gateway.dart';
import 'package:abc_androidapp/app/data/models/order.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/data/models/product/product_attribute_selection.dart';
import 'package:dio/dio.dart';

class OrderDatasource {
  final NetworkService api;

  OrderDatasource({required this.api});

  // POST method for creating an order
  Future<BaseResultRequestResponse<CreateOrderRequest, OrderPrepared?>>
      createLocalOrder(CreateOrderRequest request) async {
    try {
      final hardcodedRequest = request.toJson();
      final response = await api.post(
        '${ApiConstants.order}',
        data: hardcodedRequest,
      );

      var jsonResponse = response.data;
      return BaseResultRequestResponse.fromJson(
        json: jsonResponse,
        fromJsonRequest: (jsonRequest) =>
            CreateOrderRequest.fromJson(jsonRequest),
        fromJsonResponse: (jsonResponse) =>
            OrderPrepared.fromJson(jsonResponse),
      );
    } catch (e, stackTrace) {
      // You can log the error here or return a BaseResult error
      print('Error creating local order: $e');
      print(stackTrace);

      String errorMessage = 'Không thể tạo đơn hàng. Vui lòng thử lại sau.';
      int statusCode = 500;
      if (e is DioException) {
        // Get message from API response
        if (e.response?.data != null &&
            e.response?.data is Map<String, dynamic>) {
          errorMessage = e.response?.data['message'] ?? 'Lỗi từ server';
          statusCode = e.response?.statusCode ?? 500;
        }
      }

      return BaseResultRequestResponse<CreateOrderRequest, OrderPrepared?>(
        isSuccess: false,
        message: errorMessage,
        statusCode: statusCode,
        request: null,
        response: null,
      );
    }
  }

  Future<BaseResult> cancelOrder(String orderId) async {
    try {
      final hardcodedRequest = {
        'orderId': orderId,
        'kioskId': ApiConstants.kioskId,
        'clientId': ApiConstants.clientId,
      };

      final response = await api.put(
        '${ApiConstants.order}/cancel',
        data: hardcodedRequest,
      );

      Map<String, dynamic> jsonResponse = response.data;
      return BaseResult.fromJson(jsonResponse);
    } catch (e, stackTrace) {
      print('Error canceling order: $e');
      print(stackTrace);

      String errorMessage = 'Không thể hủy đơn hàng. Vui lòng thử lại sau.';
      int statusCode = 500;

      if (e is DioException) {
        // Get message from API response
        if (e.response?.data != null &&
            e.response?.data is Map<String, dynamic>) {
          errorMessage = e.response?.data['message'] ?? 'Lỗi từ server';
          statusCode = e.response?.statusCode ?? 500;
        }
      }

      return BaseResult(
        isSuccess: false,
        message: errorMessage,
        statusCode: statusCode,
      );
    }
  }

  Future<BaseResultRequestResponse<OrderQueryDto, Pagination<Order>>>
      getLocalOrderPagination(OrderQueryDto query) async {
    try {
      var url = "${ApiConstants.order}?${query.toParameterString()}";
      final response = await api.get(url);

      var jsonResponse = response.data;
      return BaseResultRequestResponse.fromJson(
        json: jsonResponse,
        fromJsonRequest: (jsonRequest) => OrderQueryDto.fromJson(jsonRequest),
        fromJsonResponse: (jsonResponse) => Pagination.fromJson(
          json: jsonResponse,
          fromJsonItem: (jsonItems) => Order.fromJson(jsonItems),
        ),
      );
    } catch (e, stackTrace) {
      // You can log the error here or return a BaseResult error
      print('Error creating local order: $e');
      print(stackTrace);

      return BaseResultRequestResponse<OrderQueryDto, Pagination<Order>>(
        isSuccess: false,
        message: 'Không thể tạo đơn hàng. Vui lòng thử lại sau.',
        statusCode: 500,
        request: null,
        response: null,
      );
    }
  }
}

class CreateOrderRequest {
  final String kioskId;
  final String content;
  final String clientId;
  final PaymentGateway paymentGateway;
  final List<CreateOrderDetail> orderDetails;
  final String? discountCode;

  CreateOrderRequest({
    required this.kioskId,
    required this.content,
    required this.clientId,
    required this.orderDetails,
    this.paymentGateway = PaymentGateway.vnpay,
    this.discountCode,
  });

  factory CreateOrderRequest.fromJson(Map<String, dynamic> json) {
    return CreateOrderRequest(
      kioskId: json['kioskId'],
      content: json['content'],
      clientId: json['clientId'],
      orderDetails: (json['orderDetails'] as List)
          .map((e) => CreateOrderDetail.fromJson(e))
          .toList(),
      paymentGateway: json['paymentGateway'] != null
          ? PaymentGateway.fromString(json['paymentGateway'])
          : PaymentGateway.mpos,
      discountCode: json['discountCode'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'kioskId': kioskId,
      'content': content,
      'clientId': clientId,
      'paymentGateway': paymentGateway.value,
      'orderDetails': orderDetails.map((e) => e.toJson()).toList(),
      'discountCode': discountCode,
    };
  }
}

class CreateOrderDetail {
  final String productId;
  final String productName;
  final String productDescription;
  final int quantity;
  final double sellingPrice;
  final List<ProductAttributeSelection> selectedAttributes;

  CreateOrderDetail({
    required this.productId,
    required this.productName,
    required this.productDescription,
    required this.quantity,
    required this.sellingPrice,
    required this.selectedAttributes,
  });

  factory CreateOrderDetail.fromJson(Map<String, dynamic> json) {
    return CreateOrderDetail(
      productId: json['productId'] ?? '',
      productName: json['productName'] ?? '',
      productDescription: json['productDescription'] ?? '',
      quantity: json['quantity'] ?? 0,
      sellingPrice: (json['sellingPrice'] is int)
          ? (json['sellingPrice'] as int).toDouble()
          : json['sellingPrice'],
      selectedAttributes: (json['productAttributes'] as List)
          .map((e) => ProductAttributeSelection.fromJson(e))
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'productName': productName,
      'productDescription': productDescription,
      'quantity': quantity,
      'sellingPrice': sellingPrice.toInt(),
      'productAttributes': selectedAttributes.map((e) => e.toJson()).toList(),
    };
  }
}

class OrderPrepared {
  final String orderId;
  final String orderCode;
  final PaymentGateway paymentGateway;
  final DateTime? orderDate;
  final String? paymentId;
  final String paymentUrl;
  final String paymentQr;
  final double finalAmount;
  final DateTime? expiredDate;

  OrderPrepared({
    required this.orderId,
    required this.orderDate,
    required this.paymentId,
    required this.paymentUrl,
    required this.paymentQr,
    required this.paymentGateway,
    required this.finalAmount,
    required this.orderCode,
    this.expiredDate,
  });

  factory OrderPrepared.fromJson(Map<String, dynamic> json) {
    return OrderPrepared(
      orderId: json['orderId'],
      orderDate:
          json['orderDate'] != null ? DateTime.parse(json['orderDate']) : null,
      paymentId: json['paymentId'],
      paymentUrl: json['paymentUrl'],
      paymentQr: json['paymentQr'],
      paymentGateway: json['paymentGateway'] != null
          ? PaymentGateway.fromString(json['paymentGateway'])
          : PaymentGateway.mpos,
      finalAmount: (json['finalAmount'] is int)
          ? (json['finalAmount'] as int).toDouble()
          : json['finalAmount'],
      orderCode: json['orderCode'] as String? ?? '',
      expiredDate: json['expiredDate'] != null
          ? DateTime.parse(json['expiredDate'])
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'orderId': orderId,
      'orderPreparedAt': orderDate?.toIso8601String(),
      'paymentId': paymentId,
      'paymentUrl': paymentUrl,
      'paymentQr': paymentQr,
      'paymentGateway': paymentGateway.value,
      'finalAmount': finalAmount,
      'orderCode': orderCode,
      'expiredDate': expiredDate?.toIso8601String(),
    };
  }
}

class OrderQueryDto {
  final int? page;
  final int? size;
  final bool isAsc;
  OrderStatus? status;
  final DateTime? fromDate;
  final DateTime? toDate;

  OrderQueryDto({
    this.page = 1,
    this.size = 10,
    this.isAsc = false,
    this.status,
    this.fromDate,
    this.toDate,
  });

  factory OrderQueryDto.fromJson(Map<String, dynamic> json) {
    return OrderQueryDto(
      page: json['page'] ?? 1,
      size: json['size'] ?? 10,
      isAsc: json['isAsc'] ?? true,
      status: json['status'] != null
          ? OrderStatusExtension.fromValueStr(json['status'])
          : null,
      fromDate:
          json['fromDate'] != null ? DateTime.parse(json['fromDate']) : null,
      toDate: json['toDate'] != null ? DateTime.parse(json['toDate']) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'page': page,
      'size': size,
      'isAsc': isAsc,
      'status': status?.value,
      'fromDate': fromDate?.toUtc().toIso8601String(),
      'toDate': toDate?.toUtc().toIso8601String(),
    };
  }

  Map<String, dynamic> toMap() {
    return {
      if (page != null) 'Page': page,
      if (size != null) 'Size': size,
      'IsAsc': isAsc,
      if (status != null) 'Status': status!.value,
      if (fromDate != null) 'FromDate': fromDate!.toUtc().toIso8601String(),
      if (toDate != null) 'ToDate': toDate!.toUtc().toIso8601String(),
    };
  }

  String toParameterString() {
    final map = toMap();
    return map.entries
        .map((entry) =>
            '${Uri.encodeQueryComponent(entry.key)}=${Uri.encodeQueryComponent(entry.value.toString())}')
        .join('&');
  }
}
