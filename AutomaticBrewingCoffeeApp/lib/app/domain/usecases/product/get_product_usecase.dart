import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/domain/repositories/product_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetProductUseCase {
  final ProductRepository productRepository;

  GetProductUseCase(this.productRepository);

  Future<Either<Failure, Product?>> execute(String productId) async {
    try {
      final result = await productRepository.getProductById(productId);

      return Right(result);
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}
