import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetDeviceParametersUsecase {
  final DeviceRepository deviceRepository;

  GetDeviceParametersUsecase({required this.deviceRepository});

  Future<Either<Failure, DeviceParameter>> execute(String deviceId) async {
    try {
      final parameters = await deviceRepository.getDeviceParameters(deviceId);
      return Right(parameters);
   } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}