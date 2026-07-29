import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:dio/dio.dart';
import 'api_constants.dart';
import 'api_interceptor.dart';

class DioClient {
  static final DioClient _instance = DioClient._internal();
  late final Dio dio;

  factory DioClient() {
    return _instance;
  }

  Map<String, String> get _headers => {
     ApiConstants.apiKeyHeader: ApiConstants.apiKey,
  };

  DioClient._internal() {
    dio = Dio(BaseOptions(
      baseUrl: ApiConstants.baseUrl,
      connectTimeout: const Duration(seconds: 60),
      receiveTimeout: const Duration(seconds: 60),
      headers: {
        'Content-Type': 'application/json',
        ..._headers,
      },
    ));

    dio.interceptors.add(ApiInterceptor());
  }
}
