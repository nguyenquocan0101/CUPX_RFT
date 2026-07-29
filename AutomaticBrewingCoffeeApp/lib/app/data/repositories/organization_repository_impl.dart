
import 'package:abc_androidapp/app/data/datasources/organization_datasource.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:abc_androidapp/app/domain/repositories/organization_repository.dart';

class OrganizationRepositoryImpl extends OrganizationRepository {
  final OrganizationDatasource organizationDatasource;

  OrganizationRepositoryImpl({required this.organizationDatasource});

  @override
  Future<Organization> getOrganizationInfo() async {
    var result = await organizationDatasource.getOrganizationInfo();
    return result.responseRequest!;
  }
}

