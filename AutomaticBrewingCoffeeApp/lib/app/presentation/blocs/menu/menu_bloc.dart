import 'package:abc_androidapp/app/data/models/menu.dart';
import 'package:abc_androidapp/app/domain/usecases/menu/get_menu_usecase.dart';
import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:meta/meta.dart';

part 'menu_event.dart';
part 'menu_state.dart';

class MenuBloc extends Bloc<MenuEvent, MenuState> {
  final GetMenuUseCase getMenuUseCase;
  MenuBloc({
    required this.getMenuUseCase,
  }) : super(MenuInitial()) {
    on<MenuEvent>((event, emit) {
      // TODO: implement event handler
    });

    on<GetMenuEvent>((event, emit) async {
      print("GetMenuEvent called");
      emit(MenuLoading());
      final response = await getMenuUseCase.execute();
      response.fold(
        (failure) => emit(MenuError(message: failure.message)),
        (data) => emit(MenuLoaded(
          menuInKiosk: data,
        )),
      );
    });
  }
}
