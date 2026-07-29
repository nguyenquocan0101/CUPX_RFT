import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/menu.dart';
import 'package:abc_androidapp/app/domain/repositories/menu_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetMenuUseCase {
  final MenuRepository menuRepository;

  GetMenuUseCase({required this.menuRepository});

  Future<Either<Failure, Menu>> execute() async {
    try {
      final result = await menuRepository.getMenu();

      return Right((result));
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}
