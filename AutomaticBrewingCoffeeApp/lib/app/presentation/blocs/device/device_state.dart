import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';
import 'package:equatable/equatable.dart';

abstract class DeviceState extends Equatable {
  const DeviceState();
  
  @override
  List<Object?> get props => [];
}

class DeviceInitial extends DeviceState {}

// Devices list states
class DevicesLoadingState extends DeviceState {}

class DevicesLoadedState extends DeviceState {
  final Pagination<Device> devicesPage;
  
  const DevicesLoadedState(this.devicesPage);
  
  @override
  List<Object?> get props => [devicesPage];
}

class DevicesErrorState extends DeviceState {
  final String message;
  
  const DevicesErrorState(this.message);
  
  @override
  List<Object?> get props => [message];
}

// Single device states
// class SingleDeviceLoadingState extends DeviceState {}

// class SingleDeviceLoadedState extends DeviceState {
//   final Device device;
  
//   const SingleDeviceLoadedState(this.device);
  
//   @override
//   List<Object?> get props => [device];
// }

// class SingleDeviceErrorState extends DeviceState {
//   final String message;
  
//   const SingleDeviceErrorState(this.message);
  
//   @override
//   List<Object?> get props => [message];
// }

// // Device parameters states
// class DeviceParametersLoadingState extends DeviceState {}

// class DeviceParametersLoadedState extends DeviceState {
//   final DeviceParameter parameters;
  
//   const DeviceParametersLoadedState(this.parameters);
  
//   @override
//   List<Object?> get props => [parameters];
// }

// class DeviceParametersErrorState extends DeviceState {
//   final String message;
  
//   const DeviceParametersErrorState(this.message);
  
//   @override
//   List<Object?> get props => [message];
// }

// Parameter update states
class UpdatingDeviceParameterState extends DeviceState {}

class DeviceParametersUpdatedState extends DeviceState {}

class DeviceParameterUpdateErrorState extends DeviceState {
  final String message;
  
  const DeviceParameterUpdateErrorState(this.message);
  
  @override
  List<Object?> get props => [message];
}

class UpdatingDeviceCoordinatesState extends DeviceState {}

class DeviceCoordinatesUpdatedState extends DeviceState {}

class DeviceCoordinatesUpdateErrorState extends DeviceState {
  final String message;

  const DeviceCoordinatesUpdateErrorState(this.message);

  @override
  List<Object?> get props => [message];
}