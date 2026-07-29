import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/domain/repositories/product_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetProductsUseCase {
  final ProductRepository productRepository;

  GetProductsUseCase(this.productRepository);

  Future<Either<Failure, Pagination<Product>>> execute(
      String? searchKey,
      String? filterBy,
      String? productStatus,
      String? productSize,
      String? productType,
      String? sortBy,
      bool isAsc,
      {int pageSize = 10,
      int pageIndex = 1}) async {
    try {
      final result = await productRepository.getAllProducts(
        searchKey,
        filterBy,
        pageSize,
        pageIndex,
        productStatus,
        productSize,
        productType,
        sortBy,
        isAsc,
      );

      return Right(result);
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}
