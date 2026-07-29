import 'package:abc_androidapp/app/data/models/product/product_attribute.dart';
import 'package:abc_androidapp/app/data/models/product/product_attribute_selection.dart';

class CartItem {
  final String id;
  final String name;
  final String description;
  final double price;
  final String? picUrl;
  int quantity;
  final List<ProductAttributeSelection> selectedAttributes;

  CartItem({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.picUrl, 
    this.quantity = 1,
    required this.selectedAttributes,
  });

  double get total => price * quantity;

  String get uniqueKey {
    final attributeKeys = selectedAttributes
        .map((attr) => '${attr.productAttributeId}:${attr.attributeOptionId}')
        .toList()
      ..sort();

    return '$id|${attributeKeys.join('|')}';
  }

  CartItem copyWith({
    String? productId,
    String? productName,
    String? productDescription,
    int? quantity,
    double? sellingPrice,
    String? picUrl,
    List<ProductAttributeSelection>? productAttributes,
  }) {
    return CartItem(
      id: productId ?? this.id,
      name: productName ?? this.name,
      description: productDescription ?? this.description,
      price: sellingPrice ?? this.price,
      picUrl: picUrl ?? this.picUrl,
      quantity: quantity ?? this.quantity,
      selectedAttributes: productAttributes ?? this.selectedAttributes,
    );
  }


  //  @override
  // bool operator ==(Object other) {
  //   if (identical(this, other)) return true;
  //   return other is CartItem &&
  //          other.id == id &&
  //          _attributesEqual(other.selectedAttributes, selectedAttributes);
  // }

  // bool _attributesEqual(List<ProductAttributeSelection> a, List<ProductAttributeSelection> b) {
  //   if (a.length != b.length) return false;
  //   for (int i = 0; i < a.length; i++) {
  //     if (a[i].productAttributeId != b[i].productAttributeId ||
  //         a[i].attributeOptionId != b[i].attributeOptionId) {
  //       return false;
  //     }
  //   }
  //   return true;
  // }
}

