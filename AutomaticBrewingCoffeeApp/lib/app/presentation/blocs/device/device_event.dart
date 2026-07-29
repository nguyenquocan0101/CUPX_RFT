import 'package:abc_androidapp/app/data/enums/device_status_enum.dart';
import 'package:abc_androidapp/app/data/models/arm_coordinate.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';
import 'package:equatable/equatable.dart';

abstract class DeviceEvent extends Equatable {
  const DeviceEvent();

  @override
  List<Object?> get props => [];
}

class GetDevicesEvent extends DeviceEvent {
  final DeviceStatus? status;
  final String? filterBy;
  final String? filterQuery;
  final int? page;
  final int? size;
  final String? sortBy;
  final bool? isAsc;

  const GetDevicesEvent({
    this.status,
    this.filterBy,
    this.filterQuery,
    this.page,
    this.size,
    this.sortBy,
    this.isAsc,
  });

  @override
  List<Object?> get props => [
        status,
        filterBy,
        filterQuery,
        page,
        size,
        sortBy,
        isAsc,
      ];
}

// class GetDeviceByIdEvent extends DeviceEvent {
//   final String deviceId;

//   const GetDeviceByIdEvent({required this.deviceId});

//   @override
//   List<Object?> get props => [deviceId];
// }

// class GetDeviceParametersEvent extends DeviceEvent {
//   final String deviceId;

//   const GetDeviceParametersEvent({required this.deviceId});

//   @override
//   List<Object?> get props => [deviceId];
// }

class UpdateDeviceParameterEvent extends DeviceEvent {
  final SetDeviceParameter params;

  const UpdateDeviceParameterEvent({required this.params});

  @override
  List<Object?> get props => [params];
}

class UpdateDeviceCoordinatesEvent extends DeviceEvent {
  final String deviceId;
  final ArmCoordinate params;

  const UpdateDeviceCoordinatesEvent({required this.deviceId, required this.params});

  @override
  List<Object?> get props => [deviceId, params];
}
