import 'dart:async';
import 'dart:io';

import 'package:dio/dio.dart';

class ExceptionHandler {
  static ApiException handleException(dynamic error) {
    try {
      // Xử lý lỗi từ Dio (API)
      if (error is DioException) {
        final responseData = error.response?.data;
        return ApiException(
          traceId: responseData?['traceId'],
          code: error.response?.statusCode ?? -1,
          message: responseData?['message'] ?? 'Lỗi từ máy chủ',
          description: responseData?['description'],
          timestamp: responseData?['timestamp'],
          originalException: error,
        );
      }

      // Xử lý các lỗi về kết nối mạng
      if (error is SocketException) {
        return ApiException(
          code: -1,
          message: 'Không có kết nối mạng',
          description: 'Vui lòng kiểm tra lại kết nối Internet',
          timestamp: DateTime.now().toIso8601String(),
          originalException: error,
        );
      }

      // Xử lý lỗi HTTP
      if (error is HttpException) {
        return ApiException(
          code: -2,
          message: 'Lỗi HTTP',
          description: 'Máy chủ phản hồi không hợp lệ',
          timestamp: DateTime.now().toIso8601String(),
          originalException: error,
        );
      }

      // Xử lý lỗi timeout
      if (error is TimeoutException) {
        return ApiException(
          code: -3,
          message: 'Yêu cầu hết thời gian chờ',
          description: 'Máy chủ không phản hồi kịp thời',
          timestamp: DateTime.now().toIso8601String(),
          originalException: error,
        );
      }

      // Xử lý lỗi không xác định
      return ApiException(
        code: -4,
        message: 'Lỗi không xác định',
        description: error.toString(),
        timestamp: DateTime.now().toIso8601String(),
        originalException: error,
      );
    } catch (e) {
      // Trường hợp bản thân ExceptionHandler bị lỗi
      return ApiException(
        code: -5,
        message: 'Lỗi khi xử lý ngoại lệ',
        description: e.toString(),
        timestamp: DateTime.now().toIso8601String(),
        originalException: e,
      );
    }
  }
}

class ApiException implements Exception {
  final String? traceId;
  final int code;
  final String message;
  final String? description;
  final String timestamp;
  final dynamic originalException;

  ApiException({
    this.traceId,
    required this.code,
    required this.message,
    this.description,
    required this.timestamp,
    this.originalException,
  });

  // Tạo ApiException từ JSON
  factory ApiException.fromJson(Map<String, dynamic> json) {
    return ApiException(
      traceId: json['traceId'],
      code: json['code'] ?? -1,
      message: json['message'] ?? 'Lỗi không xác định',
      description: json['description'],
      timestamp: json['timestamp'] ?? DateTime.now().toIso8601String(),
    );
  }

  @override
  String toString() {
    return 'ApiException: '
        'traceId=$traceId, '
        'code=$code, '
        'message=$message, '
        'description=$description, '
        'timestamp=$timestamp, '
        'originalException=$originalException';
  }
}
