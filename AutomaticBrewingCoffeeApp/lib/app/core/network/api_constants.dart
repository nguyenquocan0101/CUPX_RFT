import 'package:flutter_dotenv/flutter_dotenv.dart';

class ApiConstants {
  static String get baseUrl => dotenv.env['BASE_URL'] ?? '';
  static String get kioskId => dotenv.env['KIOSK_ID'] ?? '';
  static String get clientId => dotenv.env['CLIENT_ID'] ?? '';
  static String get apiKeyHeader => dotenv.env['API_KEY_HEADER'] ?? '';
  static String get apiKey => dotenv.env['API_KEY'] ?? '';

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
  static String hubUrl = dotenv.env['SIGNALR_HUB_URL'] ?? '';
}