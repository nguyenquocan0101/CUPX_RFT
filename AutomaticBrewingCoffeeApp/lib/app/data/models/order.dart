import 'package:abc_androidapp/app/data/enums/order_status.dart';
import 'package:abc_androidapp/app/data/models/order_detail.dart';


class Order {
  final String orderId;
  final OrderPaymentData? orderData;
  final OrderStatus status;
  final DateTime? createdAt;
  final bool? isSynced;
  final List<OrderDetail> orderDetails;

  Order({
    required this.orderId,
    required this.orderData,
    required this.status,
    this.createdAt,
    this.isSynced = false,
    required this.orderDetails,
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      orderId: json['orderId'],
      orderData: OrderPaymentData.fromJson(json['orderData']),
      status: OrderStatusExtension.fromValueStr(json["status"]),
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : null,
      isSynced: json['isSynced'],
      orderDetails: (json['orderDetails'] as List<dynamic>)
              .map((e) => OrderDetail.fromJson(e))
              .toList(),
    );
  }
}

class OrderPaymentData {
  final String orderId;
  final double? discount;
  final double? finalAmount;
  final double? totalAmount;
  final String? orderType;
  final String? paymentGateway;
  final String? status;
  final String? lastUpdateBy;
  final String? paymentUrl;
  final String? paymentQr;
  final List<OrderDetail> orderDetails;

  OrderPaymentData({
    required this.orderId,
    this.discount,
    this.finalAmount,
    this.totalAmount,
    this.orderType,
    this.paymentGateway,
    this.status,
    this.lastUpdateBy,
    this.paymentUrl,
    this.paymentQr,
    required this.orderDetails,
  });

  factory OrderPaymentData.fromJson(Map<String, dynamic> json) {
    return OrderPaymentData(
      orderId: json['orderId'],
      discount: (json['discount'] as num?)?.toDouble(),
      finalAmount: (json['finalAmount'] as num?)?.toDouble(),
      totalAmount: (json['totalAmount'] as num?)?.toDouble(),
      orderType: json['orderType'],
      paymentGateway: json['paymentGateway'],
      status: json['status'],
      lastUpdateBy: json['lastUpdateBy'],
      paymentUrl: json['paymentUrl'],
      paymentQr: json['paymentQr'],
      orderDetails: (json['orderDetails'] as List<dynamic>? ?? [])
          .map((e) => OrderDetail.fromJson(e))
          .toList(),
    );
  }
}

