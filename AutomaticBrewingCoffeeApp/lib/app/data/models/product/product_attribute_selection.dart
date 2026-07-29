class ProductAttributeSelection {
  final String productAttributeId;
  final String attributeOptionId;

  ProductAttributeSelection({
    required this.productAttributeId,
    required this.attributeOptionId,
  });

  Map<String, dynamic> toJson() {
    return {
      "productAttributeId": productAttributeId,
      "attributeOptionId": attributeOptionId,
    };
  }

  factory ProductAttributeSelection.fromJson(Map<String, dynamic> json) {
    return ProductAttributeSelection(
      productAttributeId: json['productAttributeId'],
      attributeOptionId: json['attributeOptionId'],
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is ProductAttributeSelection &&
        other.productAttributeId == productAttributeId &&
        other.attributeOptionId == attributeOptionId;
  }

  @override
  int get hashCode => Object.hash(productAttributeId, attributeOptionId);

  @override
  String toString() {
    return 'ProductAttributeSelection(attributeId: $productAttributeId, optionId: $attributeOptionId)';
  }
}