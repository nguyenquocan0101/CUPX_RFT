import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/models/arm_coordinate.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';

abstract class DeviceRepository {
  Future<Pagination<Device>> getDevices(DeviceQuery deviceQuery);
  Future<Device> getDeviceById(String deviceId);
  Future<DeviceParameter> getDeviceParameters(String deviceId);
  Future<void> updateDeviceParameter(SetDeviceParameter setParams);
  Future<void> updateDeviceCoordinates(String deviceId, ArmCoordinate armCoordinate);
}