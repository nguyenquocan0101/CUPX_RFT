
import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/datasources/device_datasource.dart';
import 'package:abc_androidapp/app/data/models/arm_coordinate.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';

class DeviceRepositoryImpl extends DeviceRepository {
  final DeviceDatasource deviceDatasource;

  DeviceRepositoryImpl({required this.deviceDatasource});
  
  @override
  Future<Pagination<Device>> getDevices(DeviceQuery deviceQuery) async {
    var result = await deviceDatasource.getDevices(deviceQuery);
    return result.response!;
  }

  @override
  Future<Device> getDeviceById(String deviceId) async {
    var result = await deviceDatasource.getDeviceById(deviceId);
    return result.response!;
  }

  @override
  Future<DeviceParameter> getDeviceParameters(String deviceId) async {
    var result = await deviceDatasource.getDeviceParameters(deviceId);
    return result.response!;
  }

  @override
  Future<void> updateDeviceParameter(
      SetDeviceParameter setParams) async {
    await deviceDatasource.setDeviceParameters(setParams);
  }

  @override
  Future<void> updateDeviceCoordinates(String deviceId, ArmCoordinate armCoordinate) async {
    await deviceDatasource.updateDeviceCoordinates(deviceId, armCoordinate);
  }
  
}
