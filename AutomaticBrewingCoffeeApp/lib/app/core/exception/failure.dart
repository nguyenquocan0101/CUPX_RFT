abstract class Failure {
  final String message;

  Failure(this.message);
}

class ServerFailure extends Failure {
  ServerFailure(String message) : super(message);
}

class NetworkFailure extends Failure {
  NetworkFailure(String message) : super(message);
}

class ApiFailure extends Failure {
  ApiFailure(String message) : super(message);
}

class CacheFailure extends Failure {
  CacheFailure(String message) : super(message);
}
