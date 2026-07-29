import 'package:bloc/bloc.dart';
part 'app_flow_state.dart';

enum AppFlow { order, history, setting }

class AppFlowCubit extends Cubit<AppFlowState> {
  AppFlow? _appFlow;
  AppFlow? get appFlow => _appFlow;
  AppFlowCubit() : super(AppFlowInitial());

  void updateAppFlow(AppFlow newAppFlow) {
    _appFlow = newAppFlow;
    emit(AppFlowUpdate(newAppFlow)); //call for observer
  }
}
