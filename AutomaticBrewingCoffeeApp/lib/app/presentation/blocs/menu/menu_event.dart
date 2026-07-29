part of 'menu_bloc.dart';

@immutable
abstract class MenuEvent extends Equatable {
  const MenuEvent();
}

class GetMenuEvent extends MenuEvent {

  const GetMenuEvent();
  
  @override
  // TODO: implement props
  List<Object?> get props => [];
}
