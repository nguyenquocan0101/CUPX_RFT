import 'package:flutter_dotenv/flutter_dotenv.dart';

class ApiConstants {
  static const bool localMode =
      bool.fromEnvironment('CUPX_LOCAL_MODE', defaultValue: false);

  static String _env(String name, String fallback) {
    if (localMode) return fallback;
    try {
      final value = dotenv.env[name];
      return value != null && value.trim().isNotEmpty ? value : fallback;
    } catch (_) {
      return fallback;
    }
  }

  static String get baseUrl => _env('BASE_URL',
      const String.fromEnvironment('CUPX_API_BASE_URL', defaultValue: 'http://10.0.2.2:5100/api/v1'));
  static String get kioskId => _env(
        'KIOSK_ID',
        const String.fromEnvironment('CUPX_KIOSK_ID', defaultValue: ''),
      );
  static String get clientId => _env(
        'CLIENT_ID',
        const String.fromEnvironment('CUPX_CLIENT_ID', defaultValue: ''),
      );
  static String get side => _env(
        'SIDE',
        const String.fromEnvironment('CUPX_SIDE', defaultValue: 'left'),
      );
  static String get apiKeyHeader => _env('API_KEY_HEADER', 'X-API-KEY');
  static String get apiKey => _env('API_KEY',
      const String.fromEnvironment('CUPX_API_KEY', defaultValue: ''));

  static const String product = "/products";
  static const String menu = "/menus/kiosk";
  static const String menuByKiosk = "/menus/by-kiosk";
  static const String order = "/orders";
  static const String device = "/devices";
  static const String deviceParameter = "/device-parameters";
  static String productByKiosk(String productId) {
    return "$product/$productId/by-kiosk";
  }

  static const String organization = "/organizations/current";
  static const String kiosk = "/kiosks/current";
  static String updateIngredient(String ingredientStateId) {
    return "$device/ingredient/$ingredientStateId/by-kiosk";
  }
}

class SignalRConfig {
  static String hubUrl = ApiConstants._env('SIGNALR_HUB_URL',
      const String.fromEnvironment('CUPX_SIGNALR_URL', defaultValue: 'http://10.0.2.2:5100/hubs/notification'));
}
