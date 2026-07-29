
class ProductAttribute {
  final String productAttributeId;
  final String productId;
  final String label;
  final String? description;
  final int displayOrder;
  final int? defaultAmount;
  final String? unit;
  final List<AttributeOption>? attributeOptions;

  ProductAttribute({
    required this.productAttributeId,
    required this.productId,
    required this.label,
    this.defaultAmount,
    this.description,
    required this.displayOrder,
    this.attributeOptions,
    this.unit,
  });

  factory ProductAttribute.fromJson(Map<String, dynamic> json) {
    return ProductAttribute(
      productAttributeId: json['productAttributeId'] as String,
      productId: json['productId'] as String,
      label: json['label'] as String,
      defaultAmount: json['defaultAmount'] as int?,
      description: json['description'] as String?,
      displayOrder: json['displayOrder'] as int,
      unit: json['unit'] as String?,
      attributeOptions: json['attributeOptions'] != null
          ? (json['attributeOptions'] as List)
              .map((option) => AttributeOption.fromJson(option))
              .toList()
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'productAttributeId': productAttributeId,
      'productId': productId,
      'label': label,
      'defaultAmount': defaultAmount,
      'description': description,
      'displayOrder': displayOrder,
      'attributeOptions': attributeOptions?.map((option) => option.toJson()).toList(),
      'unit': unit,
    };
  }
}

class AttributeOption {
  final String attributeOptionId;
  final String productAttributeId;
  final String name;
  final double value;
  final String? unit;
  final int displayOrder;
  final String? description;
  final bool isDefault;

  AttributeOption({
    required this.attributeOptionId,
    required this.productAttributeId,
    required this.name,
    required this.value,
    this.unit,
    required this.displayOrder,
    this.description,
    required this.isDefault,
  });

  factory AttributeOption.fromJson(Map<String, dynamic> json) {
    return AttributeOption(
      attributeOptionId: json['attributeOptionId'] as String,
      productAttributeId: json['productAttributeId'] as String,
      name: json['name'] as String,
      value: (json['value'] as num).toDouble(),
      unit: json['unit'] as String?,
      displayOrder: json['displayOrder'] as int,
      description: json['description'] as String?,
      isDefault: json['isDefault'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'attributeOptionId': attributeOptionId,
      'productAttributeId': productAttributeId,
      'name': name,
      'value': value,
      'unit': unit,
      'displayOrder': displayOrder,
      'description': description,
      'isDefault': isDefault,
    };
  }
}