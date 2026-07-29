part of 'app_flow_cubit.dart';

sealed class AppFlowState {}

final class AppFlowInitial extends AppFlowState {}

class AppFlowUpdate extends AppFlowState {
  AppFlow? appFlow;
  AppFlowUpdate(this.appFlow);
}
