import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/arm_coordinate.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';
import 'package:fpdart/fpdart.dart';

class UpdateDeviceCoordinatesUsecase {
  final DeviceRepository deviceRepository;

  UpdateDeviceCoordinatesUsecase({required this.deviceRepository});

  Future<Either<Failure, void>> execute({required String deviceId, required ArmCoordinate armCoordinate}) async {
    try {
      await deviceRepository.updateDeviceCoordinates(deviceId, armCoordinate);
      return const Right(null);
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}