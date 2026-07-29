import 'package:abc_androidapp/app/data/models/product/product_attribute.dart';
import 'package:abc_androidapp/app/data/models/product/product_category.dart';
import 'package:get/get.dart';

class Product {
  final String productId;
  final String? parentId;
  final String name;
  final String? description;
  final bool isActive;
  final String size;
  final String type;
  final double price;
  final String? imageUrl;
  final ProductCategory? category;
  final List<ProductAttribute>? productAttributes;

  Product({
    required this.productId,
    this.parentId,
    required this.name,
    this.description,
    required this.isActive,
    required this.size,
    required this.type,
    required this.price,
    required this.imageUrl,
    required this.category,
    this.productAttributes
  });

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      productId: json['productId'] as String,
      parentId: json['parentId'] as String?,
      name: json['name'] as String,
      description: json['description'] as String?,
      isActive: json['isActive'] as bool? ?? true,
      size: json['size'] as String,
      type: json['type'] as String,
      price: (json['price'] as num).toDouble(),
      imageUrl: json['imageUrl'] as String?,
      category: json['productCategory'] != null
          ? ProductCategory.fromJson(json['productCategory'])
          : null,
      productAttributes: json['productAttributes'] != null
          ? (json['productAttributes'] as List)
              .map((item) => ProductAttribute.fromJson(item))
              .toList()
          : null,
    );
  }

  // List<ProductAttribute> _generateMockAttributes() {
  //   return [
  //     // Size Attribute
  //     ProductAttribute(
  //       productAttributeId: '${productId}_size',
  //       productId: productId,
  //       name: 'Kích thước',
  //       target: 'size',
  //       description: 'Chọn kích thước cho sản phẩm',
  //       displayOrder: 1,
  //       attributeOptions: [
  //         AttributeOption(
  //           attributeOptionId: '${productId}_size_s',
  //           productAttributeId: '${productId}_size',
  //           name: 'Small',
  //           value: 0,
  //           unit: null,
  //           displayOrder: 1,
  //           description: 'Kích thước nhỏ',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_size_m',
  //           productAttributeId: '${productId}_size',
  //           name: 'Medium',
  //           value: 5000,
  //           unit: 'VND',
  //           displayOrder: 2,
  //           description: 'Kích thước vừa (+5K)',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_size_l',
  //           productAttributeId: '${productId}_size',
  //           name: 'Large',
  //           value: 10000,
  //           unit: 'VND',
  //           displayOrder: 3,
  //           description: 'Kích thước lớn (+10K)',
  //         ),
  //       ],
  //     ),
      
  //     // Ice Level Attribute
  //     ProductAttribute(
  //       productAttributeId: '${productId}_ice',
  //       productId: productId,
  //       name: 'Lượng đá',
  //       target: 'ice_level',
  //       description: 'Chọn mức độ đá',
  //       displayOrder: 2,
  //       attributeOptions: [
  //         AttributeOption(
  //           attributeOptionId: '${productId}_ice_no',
  //           productAttributeId: '${productId}_ice',
  //           name: 'Không đá',
  //           value: 0,
  //           unit: null,
  //           displayOrder: 1,
  //           description: 'Không cho đá',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_ice_little',
  //           productAttributeId: '${productId}_ice',
  //           name: 'Ít đá',
  //           value: 0,
  //           unit: null,
  //           displayOrder: 2,
  //           description: 'Ít đá',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_ice_normal',
  //           productAttributeId: '${productId}_ice',
  //           name: 'Bình thường',
  //           value: 0,
  //           unit: null,
  //           displayOrder: 3,
  //           description: 'Lượng đá bình thường',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_ice_extra',
  //           productAttributeId: '${productId}_ice',
  //           name: 'Nhiều đá',
  //           value: 0,
  //           unit: null,
  //           displayOrder: 4,
  //           description: 'Nhiều đá',
  //         ),
  //       ],
  //     ),
      
  //     // Sugar Level Attribute
  //     ProductAttribute(
  //       productAttributeId: '${productId}_sugar',
  //       productId: productId,
  //       name: 'Độ ngọt',
  //       target: 'sugar_level',
  //       description: 'Chọn mức độ ngọt',
  //       displayOrder: 3,
  //       attributeOptions: [
  //         AttributeOption(
  //           attributeOptionId: '${productId}_sugar_0',
  //           productAttributeId: '${productId}_sugar',
  //           name: '0% đường',
  //           value: 0,
  //           unit: '%',
  //           displayOrder: 1,
  //           description: 'Không đường',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_sugar_25',
  //           productAttributeId: '${productId}_sugar',
  //           name: '25% đường',
  //           value: 0,
  //           unit: '%',
  //           displayOrder: 2,
  //           description: 'Ít ngọt',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_sugar_50',
  //           productAttributeId: '${productId}_sugar',
  //           name: '50% đường',
  //           value: 0,
  //           unit: '%',
  //           displayOrder: 3,
  //           description: 'Vừa ngọt',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_sugar_75',
  //           productAttributeId: '${productId}_sugar',
  //           name: '75% đường',
  //           value: 0,
  //           unit: '%',
  //           displayOrder: 4,
  //           description: 'Ngọt',
  //         ),
  //         AttributeOption(
  //           attributeOptionId: '${productId}_sugar_100',
  //           productAttributeId: '${productId}_sugar',
  //           name: '100% đường',
  //           value: 0,
  //           unit: '%',
  //           displayOrder: 5,
  //           description: 'Rất ngọt',
  //         ),
  //       ],
  //     ),
  //   ];
  // }

  List<ProductAttribute> getAttributes() {
    //return productAttributes ?? _generateMockAttributes();
    return productAttributes ?? [];
  }

  Product withEnsuredAttributes() {
    return Product(
      productId: productId,
      parentId: parentId,
      name: name,
      description: description,
      isActive: isActive,
      size: size,
      type: type,
      price: price,
      imageUrl: imageUrl,
      category: category,
      productAttributes: productAttributes ?? [],
    );
  }

  Product copyWith({
    String? productId,
    String? parentId,
    String? name,
    String? description,
    bool? isActive,
    String? size,
    String? type,
    double? price, // Tham số quan trọng nhất
    String? imageUrl,
    ProductCategory? category,
    List<ProductAttribute>? productAttributes,
  }) {
    return Product(
      productId: productId ?? this.productId,
      parentId: parentId ?? this.parentId,
      name: name ?? this.name,
      description: description ?? this.description,
      isActive: isActive ?? this.isActive,
      size: size ?? this.size,
      type: type ?? this.type,
      price: price ?? this.price,
      imageUrl: imageUrl ?? this.imageUrl,
      category: category ?? this.category,
      productAttributes: productAttributes ?? this.productAttributes,
    );
  }
}

