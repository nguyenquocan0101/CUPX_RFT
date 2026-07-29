import 'package:abc_androidapp/app/core/base_models/base_query.dart';
import 'package:abc_androidapp/app/data/enums/device_status_enum.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';

class Device {
  final String deviceId;
  final String serialNumber;
  final String name;
  final String description;
  final String status;
  final DateTime createdDate;
  final DateTime? updatedDate;
  DeviceParameter? deviceParameter;
  final double? x;
  final double? y;
  final double? z;
  final double? rx;
  final double? ry;
  final double? rz;
  final double? j1;
  final double? j2;
  final double? j3;
  final double? j4;
  final double? j5;
  final double? j6;


  Device({
    required this.deviceId,
    required this.serialNumber,
    required this.name,
    required this.description,
    required this.status,
    required this.createdDate,
    this.updatedDate,
    this.deviceParameter,
     this.x,
    this.y,
    this.z,
    this.rx,
    this.ry,
    this.rz,
    this.j1,
    this.j2,
    this.j3,
    this.j4,
    this.j5,
    this.j6,
  });

  factory Device.fromJson(Map<String, dynamic> json) {
    return Device(
      deviceId: json['deviceId'] as String,
      serialNumber: json['serialNumber'] as String,
      name: json['name'] as String,
      description: json['description'] as String,
      status: json['status'] as String,
      createdDate: DateTime.parse(json['createdDate'] as String),
      updatedDate: json['updatedDate'] != null 
          ? DateTime.parse(json['updatedDate'] as String) 
          : null,
      x: json['x'] != null ? (json['x'] as num).toDouble() : null,
      y: json['y'] != null ? (json['y'] as num).toDouble() : null,
      z: json['z'] != null ? (json['z'] as num).toDouble() : null,
      rx: json['rx'] != null ? (json['rx'] as num).toDouble() : null,
      ry: json['ry'] != null ? (json['ry'] as num).toDouble() : null,
      rz: json['rz'] != null ? (json['rz'] as num).toDouble() : null,
      j1: json['j1'] != null ? (json['j1'] as num).toDouble() : null,
      j2: json['j2'] != null ? (json['j2'] as num).toDouble() : null,
      j3: json['j3'] != null ? (json['j3'] as num).toDouble() : null,
      j4: json['j4'] != null ? (json['j4'] as num).toDouble() : null,
      j5: json['j5'] != null ? (json['j5'] as num).toDouble() : null,
      j6: json['j6'] != null ? (json['j6'] as num).toDouble() : null,
    );
  }

   @override
  String toString() {
    return 'Device(deviceId: $deviceId, serialNumber: $serialNumber, name: $name, '
        'description: $description, status: $status, createdDate: $createdDate, '
        'updatedDate: $updatedDate, deviceParameter: $deviceParameter, '
        'x: $x, y: $y, z: $z, j1: $j1, j2: $j2, j3: $j3, j4: $j4, j5: $j5, j6: $j6, '
        'rx: $rx, ry: $ry, rz: $rz)';
  }
}

class DeviceQuery extends BaseQuery {
  DeviceStatus? status;

  DeviceQuery({
    this.status,
    super.filterBy,
    super.filterQuery,
    super.page,
    super.size,
    super.sortBy,
    super.isAsc,
  });

  @override
  Map<String, dynamic> toMap() {
    final baseMap = super.toMap();
    return {
      ...baseMap,
      if (status != null) 'status': status.toString().split('.').last,
    };
  }

  @override
  String toParameterString() {
    final map = toMap();
    return map.entries
        .map((entry) =>
            '${Uri.encodeQueryComponent(entry.key)}=${Uri.encodeQueryComponent(entry.value.toString())}')
        .join('&');
  }

  factory DeviceQuery.fromJson(Map<String, dynamic> json) {
    String? statusStr = json['status'];
    DeviceStatus? deviceStatus;
    
    if (statusStr != null) {
      try {
        deviceStatus = DeviceStatus.values.firstWhere(
          (e) => e.toString().split('.').last.toLowerCase() == statusStr.toLowerCase(),
        );
      } catch (_) {
        deviceStatus = null;
      }
    }

    return DeviceQuery(
      status: deviceStatus,
      filterBy: json['filterBy'],
      filterQuery: json['filterQuery'],
      page: json['page'] ?? 1,
      size: json['size'] ?? 10,
      sortBy: json['sortBy'],
      isAsc: json['isAsc'] ?? true,
    );
  }
}