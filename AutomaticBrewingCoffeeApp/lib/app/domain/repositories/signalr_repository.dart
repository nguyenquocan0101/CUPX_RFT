abstract class SignalRRepository {
  Future<void> connect();
  Future<void> disconnect();
  void subscribe<T>(String eventName, Function(T data) handler);
  void unsubscribe(String eventName);
  Future<dynamic> invoke(String methodName, {List<dynamic>? args});
  Stream<bool> get connectionStatus;
  bool get isConnected;
}