import 'package:abc_androidapp/app/domain/usecases/kiosk/get_kiosk_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/kiosk/update_ingredient_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/organization/get_organization_usecase.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_state.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class KioskBloc extends Bloc<KioskEvent, KioskState> {
  final GetKioskUseCase getKioskUseCase;
  final UpdateIngredientUsecase updateIngredientUsecase;

  KioskBloc({
    required this.getKioskUseCase,
    required this.updateIngredientUsecase,
  }) : super(KioskInitial()) {
    on<KioskEvent>((event, emit) {
      // TODO: implement event handler
    });

    on<GetKioskEvent>(_onGetKiosk);
    on<UpdateIngredientEvent>(_onUpdateIngredient);
  }

  Future<void> _onGetKiosk(
    GetKioskEvent event,
    Emitter<KioskState> emit,
  ) async {
    emit(KioskLoading());
    final response = await getKioskUseCase.execute();
    response.fold(
      (failure) => emit(KioskError(message: failure.message)),
      (data) => emit(KioskLoaded(
        kiosk: data,
      )),
    );
  }

    Future<void> _onUpdateIngredient(
    UpdateIngredientEvent event,
    Emitter<KioskState> emit,
  ) async {
    emit(KioskUpdateIngredientLoading());
    final response = await updateIngredientUsecase.execute(event.request);
    response.fold(
      (failure) => emit(KioskUpdateIngredientError(message: failure.message)),
      (data) => emit(KioskUpdateIngredientLoaded(
        isSuccess: data,
      )),
    );
  }
}
