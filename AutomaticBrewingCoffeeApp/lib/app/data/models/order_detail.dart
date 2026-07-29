class OrderDetail {
  final String? productName;
  final double totalAmount;
  final double sellingPrice;
  final String? productDescription;
  final int quantity;

  OrderDetail({
    this.productName,
    this.productDescription,
    required this.totalAmount,
    required this.sellingPrice,
    required this.quantity,
  });

  factory OrderDetail.fromJson(Map<String, dynamic> json) {
    return OrderDetail(
      productName: json['productName'],
      totalAmount: (json['totalAmount'] as num).toDouble(),
      sellingPrice: (json['sellingPrice'] as num).toDouble(),
      productDescription: json['productDescription'],
      quantity: json['quantity'],
    );
  }
}
