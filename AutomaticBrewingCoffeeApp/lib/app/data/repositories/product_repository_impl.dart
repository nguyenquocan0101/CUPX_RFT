import 'package:abc_androidapp/app/data/datasources/product_datasource.dart';
import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/domain/repositories/product_repository.dart';

class ProductRepositoryImpl implements ProductRepository {
  final ProductDatasource productDatasource;
  ProductRepositoryImpl({
    required this.productDatasource,
  });

  @override
  Future<Pagination<Product>> getAllProducts(
    String? searchKey,
    String? filterBy,
    int pageSize,
    int pageIndex,
    String? productStatus,
    String? productSize,
    String? productType,
    String? sortBy,
    bool isAsc,
  ) async {
    var productQuery = ProductQuery(
      status: productStatus,
      productSize: productSize,
      productType: productType,
      filterQuery: searchKey,
      filterBy: filterBy,
      page: pageIndex,
      size: pageSize,
      sortBy: sortBy,
      isAsc: isAsc,
    );
    var result = await productDatasource.getAllProducts(productQuery);

    return result.response ?? Pagination.empty();
  }

  @override
  Future<Product?> getProductById(String id) async {
    var result = await productDatasource.getProductById(id);

    return result.response;
  }

  @override
  Future<bool> updateProduct(Product product) {
    // TODO: implement updateProduct
    throw UnimplementedError();
  }

  @override
  Future<bool> deleteProduct(String id) {
    // TODO: implement deleteProduct
    throw UnimplementedError();
  }
}
