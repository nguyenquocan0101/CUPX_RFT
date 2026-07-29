import 'package:abc_androidapp/app/core/signalr/signalr_service.dart';
import 'package:abc_androidapp/app/domain/repositories/signalr_repository.dart';

class SignalRRepositoryImpl implements SignalRRepository {
  final SignalRService _signalRService;
  
  SignalRRepositoryImpl({
    required SignalRService signalRService,
  }) : _signalRService = signalRService;

  @override
  Future<void> connect() async {
    await _signalRService.startConnection();
  }
  
  @override
  Future<void> disconnect() async {
    await _signalRService.stopConnection();
  }
  
  @override
  void subscribe<T>(String eventName, Function(T data) handler) {
    _signalRService.on(eventName, (List<dynamic>? args) {
      if (args != null && args.isNotEmpty) {
        handler(args[0] as T);
      }
    });
  }
  
  @override
  void unsubscribe(String eventName) {
    _signalRService.off(eventName);
  }
  
  @override
  Future<dynamic> invoke(String methodName, {List<dynamic>? args}) {
    return _signalRService.invoke(methodName, args: args);
  }
  
  @override
  Stream<bool> get connectionStatus => _signalRService.connectionStatus;
  
  @override
  bool get isConnected => _signalRService.isConnected;
}