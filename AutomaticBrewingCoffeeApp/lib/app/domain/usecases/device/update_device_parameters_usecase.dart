import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';
import 'package:fpdart/fpdart.dart';

class UpdateDeviceParameterUsecase {
  final DeviceRepository deviceRepository;

  UpdateDeviceParameterUsecase({required this.deviceRepository});

  Future<Either<Failure, void>> execute(SetDeviceParameter setParams) async {
    try {
      await deviceRepository.updateDeviceParameter(setParams);
      return const Right(null);
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}