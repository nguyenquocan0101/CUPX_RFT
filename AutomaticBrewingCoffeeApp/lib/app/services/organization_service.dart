import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:abc_androidapp/app/domain/usecases/organization/get_organization_usecase.dart';
import 'package:abc_androidapp/app/core/dependency_injection/service_location.dart';

class OrganizationService {
  static OrganizationService? _instance;
  static OrganizationService get instance =>
      _instance ??= OrganizationService._();
  OrganizationService._();

  Organization? _cachedOrganization;
  DateTime? _lastFetchTime;
  final Duration _cacheExpiry = Duration(hours: 24);
  bool _isLoading = false;

  bool get hasData => _cachedOrganization != null;

  Organization? get organization => _cachedOrganization;

  bool get _hasValidCache {
    if (_cachedOrganization == null || _lastFetchTime == null) return false;
    return DateTime.now().difference(_lastFetchTime!) < _cacheExpiry;
  }

  Future<Organization?> getOrganization() async {
    if (_isLoading) return _cachedOrganization;

    if (_hasValidCache) {
      print('Organization: Using cached data');
      return _cachedOrganization;
    }

    // Fetch từ API
    try {
      _isLoading = true;
      print('Organization: Fetching from API...');

      final getOrgUseCase = sl<GetOrganizationUseCase>();
      final result = await getOrgUseCase.execute();

      await result.fold(
        (failure) {
          print('Organization: Failed to fetch - ${failure.message}');
          throw Exception(failure.message);
        },
        (organization) async {
          _cachedOrganization = organization;
          _lastFetchTime = DateTime.now();
          print('Organization: Fetched and cached successfully');
        },
      );

      return _cachedOrganization;
    } catch (e) {
      print('Organization: Error - $e');
      return _cachedOrganization; // Return cached data if available
    } finally {
      _isLoading = false;
    }
  }

  // Force refresh data
  Future<Organization?> refresh() async {
    _cachedOrganization = null;
    _lastFetchTime = null;
    return await getOrganization();
  }

  // Clear cache
  void clearCache() {
    _cachedOrganization = null;
    _lastFetchTime = null;
    print('🗑️ Organization: Cache cleared');
  }

  // Get store name quickly
  String get storeName => _cachedOrganization?.store?.name ?? 'Cửa hàng';

  // Get store address quickly
  String get storeAddress => _cachedOrganization?.store?.locationAddress ?? '';
}
