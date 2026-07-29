class PaymentSignal {
  final String paymentId;
  final String? orderId;
  final double? paidAmount;
  final String? paymentStatus;

  PaymentSignal({
    required this.paymentId,
    this.orderId,
    this.paidAmount,
    this.paymentStatus,
  });

  factory PaymentSignal.fromJson(Map<String, dynamic> json) {
    return PaymentSignal(
      paymentId: json['paymentId'],
      orderId: json['orderId'],
      paidAmount: json['paidAmount'] != null ? 
        (json['paidAmount'] is int ? 
          (json['paidAmount'] as int).toDouble() : 
          json['paidAmount'] as double) : null,
      paymentStatus: json['paymentStatus'],
    );
  }
  
  bool get isSuccess => paymentStatus?.toLowerCase() == 'success';
}