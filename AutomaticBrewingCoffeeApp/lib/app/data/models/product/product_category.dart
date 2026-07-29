class ProductCategory {
  final String productCategoryId;
  final String name;
  final String description;
  final String status;
  final String imageUrl;
  final int displayOrder;

  ProductCategory({
    required this.productCategoryId,
    required this.name,
    required this.description,
    required this.status,
    required this.imageUrl,
    this.displayOrder = 0,
  });

  // Create from JSON
  factory ProductCategory.fromJson(Map<String, dynamic> json) {
    return ProductCategory(
      productCategoryId: json['productCategoryId'] ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      status: json['status'] ?? '',
      imageUrl: json['imageUrl'] ?? '',
      displayOrder: json['displayOrder'] != null
          ? int.tryParse(json['displayOrder'].toString()) ?? 0
          : 0,
    );
  }

  // Convert to JSON
  Map<String, dynamic> toJson() {
    return {
      'productCategoryId': productCategoryId,
      'name': name,
      'description': description,
      'status': status,
      'imageUrl': imageUrl,
      'displayOrder': displayOrder,
    };
  }
}