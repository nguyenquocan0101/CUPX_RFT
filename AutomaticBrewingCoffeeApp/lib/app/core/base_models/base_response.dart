class BaseResult {
  final bool isSuccess;
  final String message;
  final int statusCode;

  BaseResult({
    required this.isSuccess,
    required this.message,
    required this.statusCode,
  });

  factory BaseResult.fromJson(Map<String, dynamic> json) {
    return BaseResult(
      isSuccess: json['isSuccess'] ?? false,
      message: json['message'] ?? '',
      statusCode: json['statusCode'] ?? 200,
    );
  }
}

class BaseResultRequestResponse<TRequest, TResponse> extends BaseResult {
  final TRequest? request;
  final TResponse? response;

  BaseResultRequestResponse({
    required bool isSuccess,
    required String message,
    required int statusCode,
    this.request,
    this.response,
  }) : super(
          isSuccess: isSuccess,
          message: message,
          statusCode: statusCode,
        );

  factory BaseResultRequestResponse.fromJson({
    required Map<String, dynamic> json,
    required TRequest Function(dynamic) fromJsonRequest,
    required TResponse Function(dynamic) fromJsonResponse,
  }) {
    return BaseResultRequestResponse(
      isSuccess: json['isSuccess'] ?? false,
      message: json['message'] ?? '',
      statusCode: json['statusCode'] ?? 200,
      request:
          json['request'] != null ? fromJsonRequest(json['request']) : null,
      response:
          json['response'] != null ? fromJsonResponse(json['response']) : null,
    );
  }
}

class BaseResultWithResponse<T> extends BaseResult {
  final T? responseRequest;

  BaseResultWithResponse({
    required bool isSuccess,
    required String message,
    required int statusCode,
    this.responseRequest,
  }) : super(
          isSuccess: isSuccess,
          message: message,
          statusCode: statusCode,
        );

  factory BaseResultWithResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic) fromJsonT,
  ) {
    return BaseResultWithResponse(
      isSuccess: json['isSuccess'] ?? false,
      message: json['message'] ?? '',
      statusCode: json['statusCode'] ?? 200,
      responseRequest: json['response'] != null
          ? fromJsonT(json['response'])
          : null,
    );
  }
}
