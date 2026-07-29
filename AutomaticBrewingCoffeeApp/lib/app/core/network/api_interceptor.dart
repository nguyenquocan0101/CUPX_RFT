import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'api_constants.dart';

class ApiInterceptor extends Interceptor {
  @override
  void onRequest(
      RequestOptions options, RequestInterceptorHandler handler) async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('access_token');

    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    return handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      final refreshed = await _refreshToken();

      if (refreshed) {
        final prefs = await SharedPreferences.getInstance();
        final newToken = prefs.getString('access_token');

        final opts = err.requestOptions;
        opts.headers['Authorization'] = 'Bearer $newToken';

        final cloneReq = await Dio().fetch(opts);
        return handler.resolve(cloneReq);
      }
    }

    return handler.next(err);
  }

  Future<bool> _refreshToken() async {
    final prefs = await SharedPreferences.getInstance();
    final refreshToken = prefs.getString('refresh_token');

    if (refreshToken == null) return false;

    try {
      final response = await Dio().post(
        '${ApiConstants.baseUrl}/auth/refresh',
        data: {'refreshToken': refreshToken},
      );

      final payload = response.data['response'] ?? response.data;
      final newAccessToken = payload['accessToken'] ?? payload['access_token'];
      final newRefreshToken = payload['refreshToken'] ?? payload['refresh_token'];

      await prefs.setString('access_token', newAccessToken);
      await prefs.setString('refresh_token', newRefreshToken);

      return true;
    } catch (e) {
      return false;
    }
  }
}
