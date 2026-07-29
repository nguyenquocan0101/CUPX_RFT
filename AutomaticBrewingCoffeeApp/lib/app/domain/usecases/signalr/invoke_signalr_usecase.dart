import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/domain/repositories/signalr_repository.dart';
import 'package:fpdart/fpdart.dart';

class InvokeSignalRMethodUseCase {
  final SignalRRepository signalRRepository;

  InvokeSignalRMethodUseCase({required this.signalRRepository});

  Future<Either<Failure, dynamic>> execute(String methodName, {List<dynamic>? args}) async {
    try {
      final result = await signalRRepository.invoke(methodName, args: args);
      return Right(result);
    } catch (e) {
      return Left(ServerFailure('Failed to invoke SignalR method $methodName: ${e.toString()}'));
    }
  }
}
