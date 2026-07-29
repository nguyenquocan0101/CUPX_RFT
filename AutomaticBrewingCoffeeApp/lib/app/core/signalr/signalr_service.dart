import 'dart:async';

import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:logging/logging.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:signalr_netcore/hub_connection.dart';
import 'package:signalr_netcore/signalr_client.dart';

class SignalRService {
  // Singleton instance of SignalRService
  static final SignalRService _instance = SignalRService._internal();
  factory SignalRService() => _instance;
  SignalRService._internal();

  HubConnection? _hubConnection;
  final Logger _logger = Logger('SignalRService');
  final Map<String, List<Function>> _eventHandlers = {};
  bool _isConnecting = false;

  // Connection status stream
  final StreamController<bool> _connectionStatusController =
      StreamController<bool>.broadcast();
  Stream<bool> get connectionStatus => _connectionStatusController.stream;

  Future<void> startConnection() async {
    if (_hubConnection != null &&
        _hubConnection!.state == HubConnectionState.Connected) {
      _logger.info('SignalR already connected');
      return;
    }
    if (_isConnecting) {
      _logger.info('SignalR connection already in progress');
      return;
    }
    _isConnecting = true;

    try {
      final baseUrl = SignalRConfig.hubUrl;
      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('access_token');
      final apiKey = ApiConstants.apiKey;
      final clientId = ApiConstants.clientId;
      final kioskId = ApiConstants.kioskId;

      final fullUrl = Uri.parse(baseUrl).replace(queryParameters: {
        "apiKey": apiKey,
        "clientId": clientId,
        "kioskId": kioskId,
      }).toString();

      // Tạo _hubConnection
      _logger.info('Starting SignalR connection to: $fullUrl');
      _hubConnection = HubConnectionBuilder()
          .withUrl(fullUrl,
              options: token != null
                  ? HttpConnectionOptions(
                      accessTokenFactory: () => Future.value(token),
                      logger: _logger,
                    )
                  : HttpConnectionOptions(logger: _logger))
          .withAutomaticReconnect()
          .build();

      // Đăng ký các sự kiện hệ thống
      _hubConnection!.onreconnected(({connectionId}) {
        _logger.info('SignalR reconnected: $connectionId');
        _connectionStatusController.add(true);
        // Đăng ký lại tất cả handlers sau khi reconnect
        _registerAllHandlers();
      });

      _hubConnection!.onreconnecting(({error}) {
        _logger.warning('SignalR reconnecting: $error');
        _connectionStatusController.add(false);
      });

      _hubConnection!.onclose(({error}) {
        _logger.info('SignalR connection closed: $error');
        _connectionStatusController.add(false);
      });

      // Bắt đầu kết nối
      await _hubConnection!.start();
      _logger.info('SignalR connection started');
      _connectionStatusController.add(true);

      // Đăng ký tất cả handlers đã được lưu trước đó
      _registerAllHandlers();
    } catch (e) {
      _logger.severe('Error starting SignalR connection: $e');
      _connectionStatusController.add(false);
      rethrow; // Throw lại để caller có thể handle
    } finally {
      _isConnecting = false;
    }
  }

  Future<void> stopConnection() async {
    if (_hubConnection != null) {
      await _hubConnection!.stop();
      _hubConnection = null; // Reset connection
      _logger.info('SignalR connection stopped');
      _connectionStatusController.add(false);
    }
  }

  /// Đăng ký lắng nghe một event từ server
  /// [methodName] - tên method/event từ server
  /// [callback] - hàm callback sẽ được gọi khi nhận event
  void on(String methodName, Function callback) {
    // Lưu callback vào danh sách handlers
    _eventHandlers[methodName] ??= [];
    _eventHandlers[methodName]!.add(callback);

    // Nếu đã kết nối, đăng ký handler ngay lập tức
    if (_hubConnection != null &&
        _hubConnection!.state == HubConnectionState.Connected) {
      _registerSingleHandler(methodName, callback);
    }

    _logger.fine('Event handler added for method: $methodName');
  }

  /// Hủy đăng ký lắng nghe event
  /// [methodName] - tên method/event
  /// [callback] - callback cụ thể cần hủy (optional)
  void off(String methodName, [Function? callback]) {
    if (callback == null) {
      // Hủy tất cả handlers cho method này
      _eventHandlers.remove(methodName);
      _logger.fine('All handlers removed for method: $methodName');
    } else {
      // Hủy callback cụ thể
      _eventHandlers[methodName]?.remove(callback);
      if (_eventHandlers[methodName]?.isEmpty == true) {
        _eventHandlers.remove(methodName);
      }
      _logger.fine('Specific handler removed for method: $methodName');
    }

    // TODO: Lý tưởng nhất là cần hủy đăng ký trên HubConnection
    // Nhưng signalr_netcore package không hỗ trợ off method
    // Có thể cần implement cơ chế khác
  }

  /// Đăng ký tất cả handlers đã lưu với HubConnection
  void _registerAllHandlers() {
    if (_hubConnection == null) return;

    _eventHandlers.forEach((methodName, callbacks) {

      _hubConnection!.off(methodName);
      // Đăng ký một lần cho mỗi method name
      _hubConnection!.on(methodName, (dynamic response) {
        // Xử lý linh hoạt các kiểu dữ liệu khác nhau
        List<dynamic>? args;

        if (response == null) {
          args = [];
        } else if (response is List) {
          args = response;
        } else {
          // Nếu không phải List, wrap thành List
          args = [response];
        }

        // Gọi tất cả callbacks đã đăng ký cho method này
        for (final callback in callbacks) {
          try {
             callback(args); 
          } catch (e) {
            _logger.severe('Error executing callback for $methodName: $e');
          }
        }
      });
    });

    _logger.fine('All handlers registered with HubConnection');
  }

  /// Đăng ký một handler cụ thể (dùng khi thêm handler mới vào connection đã sẵn sàng)
  void _registerSingleHandler(String methodName, Function callback) {
    if (_hubConnection == null) return;

    // Vấn đề: signalr_netcore không hỗ trợ multiple handlers cho cùng một method
    // Giải pháp tạm thời: đăng ký lại toàn bộ handlers
    _registerAllHandlers();
  }

  /// Gọi một method trên server
  /// [methodName] - tên method trên server
  /// [args] - danh sách arguments
  Future<dynamic> invoke(String methodName, {List<dynamic>? args}) async {
    if (_hubConnection?.state != HubConnectionState.Connected) {
      throw Exception('SignalR connection not established');
    }

    try {
      final result =
          await _hubConnection!.invoke(methodName, args: args?.cast<Object>());
      _logger.fine('Successfully invoked method: $methodName');
      return result;
    } catch (e) {
      _logger.severe('Error invoking SignalR method $methodName: $e');
      rethrow;
    }
  }

  /// Kiểm tra trạng thái kết nối
  bool get isConnected => _hubConnection?.state == HubConnectionState.Connected;

  /// Lấy trạng thái kết nối hiện tại
  HubConnectionState? get connectionState => _hubConnection?.state;

  /// Cleanup resources
  void dispose() {
    stopConnection();
    _connectionStatusController.close();
    _eventHandlers.clear();
  }
}
