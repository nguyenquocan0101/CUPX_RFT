part of 'menu_bloc.dart';

@immutable
sealed class MenuState {}

final class MenuInitial extends MenuState {}

class MenuLoading extends MenuState {}

class MenuLoaded extends MenuState {
  final Menu menuInKiosk;
  MenuLoaded({required this.menuInKiosk});
}

class MenuError extends MenuState {
  final String message;
  MenuError({required this.message});
}
