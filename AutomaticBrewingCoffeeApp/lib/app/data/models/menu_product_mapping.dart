import 'package:abc_androidapp/app/data/models/product/product.dart';

//Product added in Menu
class MenuProductMapping {
  final String menuId;
  final String productId;
  final int displayOrder;
  final String? statusInMenu;
  final Product product;
  final double sellingPrice;
  final bool isAvailable;

  MenuProductMapping({
    required this.menuId,
    required this.productId,
    this.displayOrder = 0,
    this.statusInMenu,
    required this.product,
    required this.sellingPrice,
    required this.isAvailable
  });

  factory MenuProductMapping.fromJson(Map<String, dynamic> json) {
    final originalProduct = Product.fromJson(json['product']);
    final sellingPrice = (json['sellingPrice'] as num?)?.toDouble() ?? originalProduct.price;
    return MenuProductMapping(
      menuId: json['menuId'] as String,
      productId: json['productId'] as String,
      displayOrder: json['displayOrder'] != null
          ? int.tryParse(json['displayOrder'].toString()) ?? 0
          : 0,
      statusInMenu: json['statusInMenu'],
      product: originalProduct.copyWith(price: sellingPrice),
      isAvailable: json['isAvailable'] as bool? ?? true,
      sellingPrice: (json['sellingPrice'] as num?)?.toDouble() ?? 0.0,
    );
  }
}
