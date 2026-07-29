import 'package:abc_androidapp/app/core/base_models/base_response.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/data/models/organization/update_ingredient_request.dart';

class KioskDatasource {
  final NetworkService api;

  KioskDatasource({required this.api});

  Future<BaseResultWithResponse<Kiosk>> getKioskInfo() async {
    final response = await api.get(ApiConstants.kiosk);

    var jsonResponse = response.data;
    return BaseResultWithResponse<Kiosk>.fromJson(
      jsonResponse,
      (json) => Kiosk.fromJson(json),
    );
  }

  Future<BaseResultWithResponse<bool>> updateIngredient(
    UpdateIngredientRequest request,
  ) async {
    final response = await api.put(
      '${ApiConstants.updateIngredient(request.deviceIngredientStateId)}',
      data: request.toJson(),
    );

    var jsonResponse = response.data;
    return BaseResultWithResponse<bool>.fromJson(
      jsonResponse,
      (json) => json['success'] ?? true,
    );
  }
}
