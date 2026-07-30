import 'package:flutter_test/flutter_test.dart';
import 'package:abc_androidapp/app/core/network/api_constants.dart';

void main() {
  test('local API paths remain relative to the configured base URL', () {
    expect(ApiConstants.product, '/products');
    expect(ApiConstants.order, '/orders');
    expect(ApiConstants.organization, '/organizations/current');
    expect(ApiConstants.apiKeyHeader, 'X-API-KEY');
    expect(ApiConstants.side, 'left');
  });
}
