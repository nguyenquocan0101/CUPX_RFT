import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';

abstract class ProductRepository {
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
  );

  Future<Product?> getProductById(String id);

  //Future<void> addProduct(Product product);

  Future<bool> updateProduct(Product product);

  Future<bool> deleteProduct(String id);
}
