import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetDeviceByIdUsecase {
  final DeviceRepository deviceRepository;

  GetDeviceByIdUsecase({required this.deviceRepository});

  Future<Either<Failure, Device>> execute(String deviceId) async {
    try {
      final device = await deviceRepository.getDeviceById(deviceId);
      return Right(device);
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}