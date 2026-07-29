import 'package:abc_androidapp/app/domain/usecases/device/get_device_by_id_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_device_parameters_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_devices_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/update_device_coordinates_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/update_device_parameters_usecase.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DeviceBloc extends Bloc<DeviceEvent, DeviceState> {
  final GetDevicesUsecase getDevicesUsecase;
  // final GetDeviceByIdUsecase getDeviceByIdUsecase;
  // final GetDeviceParametersUsecase getDeviceParametersUsecase;
  final UpdateDeviceParameterUsecase updateDeviceParameterUsecase;
  final UpdateDeviceCoordinatesUsecase updateDeviceCoordinatesUsecase;

  DeviceBloc({
    required this.getDevicesUsecase,
    // required this.getDeviceByIdUsecase,
    // required this.getDeviceParametersUsecase,
    required this.updateDeviceParameterUsecase,
    required this.updateDeviceCoordinatesUsecase,
  }) : super(DeviceInitial()) {
    on<GetDevicesEvent>(_onGetDevices);
    // on<GetDeviceByIdEvent>(_onGetDeviceById);
    // on<GetDeviceParametersEvent>(_onGetDeviceParameters);
    on<UpdateDeviceParameterEvent>(_onUpdateDeviceParameter);
    on<UpdateDeviceCoordinatesEvent>(_onUpdateDeviceCoordinates);
  }

  Future<void> _onGetDevices(
    GetDevicesEvent event,
    Emitter<DeviceState> emit,
  ) async {
    emit(DevicesLoadingState());

    final result = await getDevicesUsecase.excute(
      status: event.status,
      filterBy: event.filterBy,
      filterQuery: event.filterQuery,
      page: event.page,
      size: event.size,
      sortBy: event.sortBy,
      isAsc: event.isAsc,
    );

    result.fold(
      (failure) => emit(DevicesErrorState(failure.message)),
      (devices) => emit(DevicesLoadedState(devices)),
    );
  }

  // Future<void> _onGetDeviceById(
  //   GetDeviceByIdEvent event,
  //   Emitter<DeviceState> emit,
  // ) async {
  //   emit(SingleDeviceLoadingState());

  //   final result = await getDeviceByIdUsecase.execute(event.deviceId);

  //   result.fold(
  //     (failure) => emit(SingleDeviceErrorState(failure.message)),
  //     (device) => emit(SingleDeviceLoadedState(device)),
  //   );
  // }

  // Future<void> _onGetDeviceParameters(
  //   GetDeviceParametersEvent event,
  //   Emitter<DeviceState> emit,
  // ) async {
  //   emit(DeviceParametersLoadingState());

  //   final result = await getDeviceParametersUsecase.execute(event.deviceId);

  //   result.fold(
  //     (failure) => emit(DeviceParametersErrorState(failure.message)),
  //     (parameters) => emit(DeviceParametersLoadedState(parameters)),
  //   );
  // }

  Future<void> _onUpdateDeviceParameter(
    UpdateDeviceParameterEvent event,
    Emitter<DeviceState> emit,
  ) async {
    emit(UpdatingDeviceParameterState());

    final result = await updateDeviceParameterUsecase.execute(event.params);

    result.fold(
      (failure) => emit(DeviceParameterUpdateErrorState(failure.message)),
      (_) => emit(DeviceParametersUpdatedState()),
    );
  }

  Future<void> _onUpdateDeviceCoordinates(
    UpdateDeviceCoordinatesEvent event,
    Emitter<DeviceState> emit,
  ) async {
    emit(UpdatingDeviceCoordinatesState());

    final result = await updateDeviceCoordinatesUsecase.execute(
      deviceId: event.deviceId,
      armCoordinate: event.params,
    );

    result.fold(
      (failure) => emit(DeviceCoordinatesUpdateErrorState(failure.message)),
      (_) => emit(DeviceCoordinatesUpdatedState()),
    );
  }

}