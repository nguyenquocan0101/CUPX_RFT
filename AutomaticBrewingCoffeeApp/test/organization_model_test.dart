import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('parses the local organization response when optional fields are absent', () {
    final organization = Organization.fromJson({
      'organizationId': 'org-local',
      'name': 'Local CUPX',
      'organizationCode': 'LOCAL',
      'status': 'Active',
      'createdDate': '2026-01-01T00:00:00.000Z',
      'isDeleted': false,
      'store': {
        'storeId': 'store-local',
        'organizationId': 'org-local',
        'name': 'Local Store',
        'locationAddress': 'Local',
        'status': 'Active',
      },
    });

    expect(organization.name, 'Local CUPX');
    expect(organization.description, isEmpty);
    expect(organization.store?.contactPhone, isEmpty);
    expect(organization.store?.locationTypeId, isEmpty);
  });
}
