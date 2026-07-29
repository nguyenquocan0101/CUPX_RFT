import 'package:abc_androidapp/app/domain/usecases/organization/get_organization_usecase.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class OrganizationBloc extends Bloc<OrganizationEvent, OrganizationState> {
  final GetOrganizationUseCase getOrganizationUseCase;

  OrganizationBloc({
    required this.getOrganizationUseCase,
  }) : super(OrganizationInitial()) {
    on<OrganizationEvent>((event, emit) {
      // TODO: implement event handler
    });

     on<GetOrganizationEvent>(_onGetOrganization);
  }

  Future<void> _onGetOrganization(
    GetOrganizationEvent event,
    Emitter<OrganizationState> emit,
  ) async {
    emit(OrganizationLoading());
    final response = await getOrganizationUseCase.execute();
    response.fold(
      (failure) => emit(OrganizationError(message: failure.message)),
      (data) => emit(OrganizationLoaded(
        organization: data,
      )),
    );
  }
}
