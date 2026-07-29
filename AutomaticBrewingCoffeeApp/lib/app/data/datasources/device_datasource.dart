import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/core/base_models/base_response.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/data/models/arm_coordinate.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';

class DeviceDatasource {
   final NetworkService api;

   DeviceDatasource({required this.api});

  Future<BaseResultRequestResponse<DeviceQuery, Pagination<Device>>>
      getDevices(DeviceQuery query) async {
        var url = "${ApiConstants.device}?${query.toParameterString()}";
    final response =
        await api.get(url);

    var jsonResponse = response.data;
    return BaseResultRequestResponse.fromJson(
      json: jsonResponse,
      fromJsonRequest: (jsonRequest) => DeviceQuery.fromJson(jsonRequest),
      fromJsonResponse: (jsonResponse) => Pagination.fromJson(
        json: jsonResponse,
        fromJsonItem: (jsonItems) => Device.fromJson(jsonItems),
      ),
    );
  }

  Future<BaseResultRequestResponse<String, Device>> getDeviceById(String deviceId) async {
    try {
      final response = await api.get('${ApiConstants.device}/$deviceId');
      var jsonResponse = response.data;
      return BaseResultRequestResponse.fromJson(
        json: jsonResponse,
        fromJsonRequest: (jsonRequest) => jsonRequest.toString(),
        fromJsonResponse: (jsonResponse) => Device.fromJson(jsonResponse),
      );
    } catch (e, stackTrace) {
      print('Error getting device id: $e');
      print(stackTrace);

      return BaseResultRequestResponse<String, Device>(
        isSuccess: false,
        message: 'Không thể lấy device. Vui lòng thử lại sau.',
        statusCode: 500,
        request: '',
        response: null,
      );
    }
  }

  Future<BaseResultRequestResponse<String, DeviceParameter>> getDeviceParameters(
      String deviceId) async {
    try {
      final response = await api.get('${ApiConstants.device}/$deviceId/parameters');
      var jsonResponse = response.data;
      return BaseResultRequestResponse.fromJson(
        json: jsonResponse,
        fromJsonRequest: (jsonRequest) => jsonRequest.toString(),
        fromJsonResponse: (jsonResponse) => DeviceParameter.fromJson(jsonResponse),
      );
    } catch (e, stackTrace) {
      print('Error creating device: $e');
      print(stackTrace);

      return BaseResultRequestResponse<String, DeviceParameter>(
        isSuccess: false,
        message: 'Chưa thể lấy các thông số của máy. Vui lòng thử lại sau.',
        statusCode: 500,
        request: '',
        response: null,
      );
    }
  }

  Future<BaseResultWithResponse<SetDeviceParameter>> setDeviceParameters(
      SetDeviceParameter request) async {
    try {
      final response = await api.patch(
        '${ApiConstants.deviceParameter}',
        data: request.toJson(),
      );
      var jsonResponse = response.data;
      return BaseResultWithResponse.fromJson(
        jsonResponse,
        (jsonRequest) => SetDeviceParameter.fromJson(jsonRequest),
      );
    } catch (e, stackTrace) {
      print('Error setting device parameters: $e');
      print(stackTrace);

      return BaseResultWithResponse<SetDeviceParameter>(
        isSuccess: false,
        message: 'Không thể cài đặt thông số của máy. Vui lòng thử lại sau.',
        statusCode: 500,
        responseRequest: null,
      );
    }
  }

  Future<BaseResult> updateDeviceCoordinates(String deviceId,
      ArmCoordinate request) async {
    try {
      final response = await api.patch(
        '${ApiConstants.device}/$deviceId/coordinates',
        data: request.toJson(),
      );
      var jsonResponse = response.data;
      return BaseResult.fromJson(
        jsonResponse
      );
    } catch (e, stackTrace) {
      print('Error setting device parameters: $e');
      print(stackTrace);

      return BaseResult(
        isSuccess: false,
        message: 'Không thể cập nhật tọa độ của máy. Vui lòng thử lại sau.',
        statusCode: 500
      );
    }
  }


}