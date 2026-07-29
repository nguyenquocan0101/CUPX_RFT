import 'dart:convert';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/base_models/base_query.dart';
import 'package:abc_androidapp/app/core/base_models/base_response.dart';
import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';

class ProductDatasource {
  final NetworkService api;
  ProductDatasource({required this.api});

  Future<BaseResultRequestResponse<ProductQuery, Pagination<Product>>>
      getAllProducts(ProductQuery query) async {
    final response =
        await api.get(ApiConstants.product + query.toParameterString());

    var jsonResponse = response.data;
    return BaseResultRequestResponse.fromJson(
      json: jsonResponse,
      fromJsonRequest: (jsonRequest) => ProductQuery.fromJson(jsonRequest),
      fromJsonResponse: (jsonResponse) => Pagination.fromJson(
        json: jsonResponse,
        fromJsonItem: (jsonItems) => Product.fromJson(jsonItems),
      ),
    );
  }

  Future<BaseResultRequestResponse<String, Product>> getProductById(
      String productId) async {
    var uri = ApiConstants.productByKiosk(productId);
    final response = await api.get(uri);
    var jsonResponse = response.data;
    return BaseResultRequestResponse.fromJson(
      json: jsonResponse,
      fromJsonRequest: (jsonRequest) => jsonRequest.toString(),
      fromJsonResponse: (jsonResponse) => Product.fromJson(jsonResponse),
    );
  }

  Future<BaseResultRequestResponse<UpdateProductData, Product>> updateProduct(
      String productId, UpdateProductData data) async {
    final response = await api.get('${ApiConstants.product}/$productId');
    var jsonResponse = response.data;
    return BaseResultRequestResponse.fromJson(
      json: jsonResponse,
      fromJsonRequest: (jsonRequest) => UpdateProductData.fromJson(jsonRequest),
      fromJsonResponse: (jsonResponse) => Product.fromJson(jsonResponse),
    );
  }

  Future<BaseResultRequestResponse<String, Product>> removeProduct(
      String productId) async {
    final response = await api.get('${ApiConstants.product}/$productId');
    var jsonResponse = response.data;
    return BaseResultRequestResponse.fromJson(
      json: jsonResponse,
      fromJsonRequest: (jsonRequest) => jsonDecode(jsonRequest.toString()),
      fromJsonResponse: (jsonResponse) => Product.fromJson(jsonResponse),
    );
  }
}

class ProductQuery extends BaseQuery {
  String? status;
  String? productSize;
  String? productType;

  ProductQuery({
    this.status,
    this.productSize,
    this.productType,
    super.filterBy,
    super.filterQuery,
    super.page,
    super.size,
    super.sortBy,
    super.isAsc,
  });

  @override
  Map<String, dynamic> toMap() {
    final baseMap = super.toMap();
    return {
      ...baseMap,
      if (status != null) 'status': status,
      if (productSize != null) 'productSize': productSize,
      if (productType != null) 'productType': productType,
    };
  }

  @override
  String toParameterString() {
    final map = toMap();
    return map.entries
        .map((entry) =>
            '${Uri.encodeQueryComponent(entry.key)}=${Uri.encodeQueryComponent(entry.value.toString())}')
        .join('&');
  }

  factory ProductQuery.fromJson(Map<String, dynamic> json) {
    return ProductQuery(
      status: json['status'],
      productSize: json['productSize'],
      productType: json['productType'],
      filterBy: json['filterBy'],
      filterQuery: json['filterQuery'],
      page: json['page'] ?? 1,
      size: json['size'] ?? 10,
      sortBy: json['sortBy'],
      isAsc: json['isAsc'] ?? true,
    );
  }
}

class UpdateProductData {
  final String? parentId;
  final String name;
  final String? description;
  final bool isActive;
  final String size;
  final String type;
  final double price;

  UpdateProductData({
    this.parentId,
    required this.name,
    this.description,
    required this.isActive,
    required this.size,
    required this.type,
    required this.price,
  });

  Map<String, dynamic> toJson() {
    return {
      'parentId': parentId,
      'name': name,
      'description': description,
      'isActive': isActive,
      'size': size,
      'type': type,
      'price': price,
    };
  }

  factory UpdateProductData.fromJson(Map<String, dynamic> json) {
    return UpdateProductData(
      parentId: json['parentId'],
      name: json['name'],
      description: json['description'],
      isActive: json['isActive'] ?? false,
      size: json['size'],
      type: json['type'],
      price: (json['price'] as num).toDouble(),
    );
  }
}
