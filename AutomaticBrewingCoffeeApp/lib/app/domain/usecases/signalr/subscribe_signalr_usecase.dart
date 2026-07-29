import 'package:abc_androidapp/app/domain/repositories/signalr_repository.dart';

class SubscribeSignalRUseCase {
  final SignalRRepository signalRRepository;

  SubscribeSignalRUseCase({required this.signalRRepository});

  void execute<T>(String eventName, Function(T data) handler) {
    signalRRepository.subscribe<T>(eventName, handler);
  }
}