import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/domain/repositories/signalr_repository.dart';
import 'package:fpdart/fpdart.dart';

class ConnectSignalRUseCase {
  final SignalRRepository signalRRepository;

  ConnectSignalRUseCase({required this.signalRRepository});

  Future<Either<Failure, void>> execute() async {
    try {
      await signalRRepository.connect();
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure('Failed to connect to SignalR hub: ${e.toString()}'));
    }
  }
  
}