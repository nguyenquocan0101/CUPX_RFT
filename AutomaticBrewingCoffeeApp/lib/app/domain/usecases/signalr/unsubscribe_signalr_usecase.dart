import 'package:abc_androidapp/app/domain/repositories/signalr_repository.dart';

class UnsubscribeSignalRUseCase {
  final SignalRRepository signalRRepository;

  UnsubscribeSignalRUseCase({required this.signalRRepository});

  void execute(String eventName) {
    signalRRepository.unsubscribe(eventName);
  }
}