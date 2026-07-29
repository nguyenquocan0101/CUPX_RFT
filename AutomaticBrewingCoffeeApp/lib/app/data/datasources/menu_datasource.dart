import 'package:abc_androidapp/app/core/base_models/base_response.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/data/models/menu.dart';

class MenuDatasource {
  final NetworkService api;

  MenuDatasource({required this.api});

  Future<BaseResultRequestResponse<String, Menu?>> getMenuById(
      String menuId) async {
    try {
      final response = await api.get('${ApiConstants.menu}');
      var jsonResponse = response.data;
      return BaseResultRequestResponse.fromJson(
        json: jsonResponse,
        fromJsonRequest: (jsonRequest) => jsonRequest.toString(),
        fromJsonResponse: (jsonResponse) => Menu.fromJson(jsonResponse),
      );
    } catch (e, stackTrace) {
      // Bạn có thể ghi log ở đây hoặc trả về một BaseResult lỗi
      print('Error getting menu by id: $e');
      print(stackTrace);

      return BaseResultRequestResponse<String, Menu?>(
        isSuccess: false,
        message: 'Không thể tải menu. Vui lòng thử lại sau.',
        statusCode: 500,
        request: menuId,
        response: null,
      );
    }
  }

  Future<BaseResultRequestResponse<String, Menu?>> getMenu() async {
    try {
      final response = await api.get('${ApiConstants.menuByKiosk}');
      var jsonResponse = response.data;
      return BaseResultRequestResponse.fromJson(
        json: jsonResponse,
        fromJsonRequest: (jsonRequest) => jsonRequest.toString(),
        fromJsonResponse: (jsonResponse) => Menu.fromJson(jsonResponse),
      );
    } catch (e, stackTrace) {
      // Bạn có thể ghi log ở đây hoặc trả về một BaseResult lỗi
      print('Error getting menu by id: $e');
      print(stackTrace);

      return BaseResultRequestResponse<String, Menu?>(
        isSuccess: false,
        message: 'Không thể tải menu. Vui lòng thử lại sau.',
        statusCode: 500,
        response: null,
      );
    }
  }
}
