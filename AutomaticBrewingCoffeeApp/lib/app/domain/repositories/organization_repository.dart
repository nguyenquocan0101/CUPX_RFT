
import 'package:abc_androidapp/app/data/models/organization/organization.dart';

abstract class OrganizationRepository {
  Future<Organization> getOrganizationInfo();
}
