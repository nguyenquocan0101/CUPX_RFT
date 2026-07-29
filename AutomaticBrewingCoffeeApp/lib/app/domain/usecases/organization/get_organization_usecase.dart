import 'package:abc_androidapp/app/core/exception/exception_handler.dart';
import 'package:abc_androidapp/app/core/exception/failure.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:abc_androidapp/app/domain/repositories/organization_repository.dart';
import 'package:fpdart/fpdart.dart';

class GetOrganizationUseCase {
  final OrganizationRepository organizationRepository;

  GetOrganizationUseCase({required this.organizationRepository});

  Future<Either<Failure, Organization>> execute() async {
    try {
      final result = await organizationRepository.getOrganizationInfo();

      return Right((result));
    } on ApiException catch (e) {
      return Left(ApiFailure(e.description ?? 'Lỗi API không xác định!'));
    } catch (e) {
      return Left(ServerFailure('Lỗi hệ thống'));
    }
  }
}
