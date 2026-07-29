import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/domain/repositories/kiosk_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetKioskUseCase {
  final KioskRepository kioskRepository;

  GetKioskUseCase({required this.kioskRepository});

  Future<Either<Failure, Kiosk>> execute() async {
    try {
      final result = await kioskRepository.getKioskInfo();

      return Right((result));
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}