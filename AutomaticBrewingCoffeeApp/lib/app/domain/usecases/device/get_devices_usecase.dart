import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/enums/device_status_enum.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetDevicesUsecase {
  final DeviceRepository deviceRepository;

  GetDevicesUsecase({required this.deviceRepository});

  Future<Either<Failure, Pagination<Device>>> excute({
    DeviceStatus? status,
    String? filterBy,
    String? filterQuery,
    int? page,
    int? size,
    String? sortBy,
    bool? isAsc,
  }) async {
    try {
      final deviceQuery = DeviceQuery(
        status: status,
        filterBy: filterBy,
        filterQuery: filterQuery,
        page: page ?? 1,
        size: size ?? 10,
        sortBy: sortBy,
        isAsc: isAsc ?? true,
      );

      final result = await deviceRepository.getDevices(deviceQuery);
      
      await Future.wait(
        result.items.map((device) async {
          try {
            device.deviceParameter = null;
          } catch (e) {
            // Bỏ qua lỗi khi lấy thông số thiết bị
          }
        })
      );
      
      return Right(result);
      
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}