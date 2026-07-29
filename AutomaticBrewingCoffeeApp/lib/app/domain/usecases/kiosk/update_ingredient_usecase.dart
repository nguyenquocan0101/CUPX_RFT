import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/organization/update_ingredient_request.dart';
import 'package:abc_androidapp/app/domain/repositories/kiosk_repository.dart';
import 'package:fpdart/fpdart.dart';

class UpdateIngredientUsecase {
  final KioskRepository kioskRepository;

  UpdateIngredientUsecase({required this.kioskRepository});

  Future<Either<Failure, bool>> execute(UpdateIngredientRequest request) async {
    try {
      final result = await kioskRepository.updateIngredient(request);

      return Right((result));
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}