import 'package:abc_androidapp/app/core/base_models/base_response.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';

class OrganizationDatasource {
  final NetworkService api;

  OrganizationDatasource({required this.api});

  Future<BaseResultWithResponse<Organization>>
      getOrganizationInfo() async {
    final response =
        await api.get(ApiConstants.organization);

    var jsonResponse = response.data;
     return BaseResultWithResponse<Organization>.fromJson(
      jsonResponse,
      (json) => Organization.fromJson(json),
    );
  }

}