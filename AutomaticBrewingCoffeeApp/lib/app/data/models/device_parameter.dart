import 'package:abc_androidapp/app/data/enums/device_type_enum.dart';

class ParameterValue {
  dynamic value;
  bool isSetting;

  ParameterValue({
    required this.value,
    required this.isSetting,
  });

  ParameterValue copyWith({
    dynamic value,
    bool? isSetting,
  }) {
    return ParameterValue(
      value: value ?? this.value,
      isSetting: isSetting ?? this.isSetting,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'value': value,
      'isSetting': isSetting,
    };
  }

  factory ParameterValue.fromJson(Map<String, dynamic> json) {
    return ParameterValue(
      value: json['value'],
      isSetting: json['isSetting'] as bool,
    );
  }

  @override
  String toString() => 'ParameterValue(value: $value, isSetting: $isSetting)';
}

class DeviceParameter {
  DeviceType deviceType;
  Map<String, ParameterValue> parameters;

  DeviceParameter({
    required this.deviceType,
    this.parameters = const {},
  });

  DeviceParameter copyWith({
    DeviceType? deviceType,
    Map<String, ParameterValue>? parameters,
  }) {
    return DeviceParameter(
      deviceType: deviceType ?? this.deviceType,
      parameters: parameters ?? this.parameters,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceType': describeEnum(deviceType),
      'parameters': parameters.map((key, value) => MapEntry(key, value.toJson())),
    };
  }

  factory DeviceParameter.fromJson(Map<String, dynamic> json) {
    return DeviceParameter(
      deviceType: _deviceTypeFromString(json['deviceType'] as String),
      parameters: (json['parameters'] as Map<String, dynamic>).map(
        (key, value) => MapEntry(key, ParameterValue.fromJson(value as Map<String, dynamic>)),
      ),
    );
  }

  @override
  String toString() => 'DeviceParameterDto(deviceType: $deviceType, parameters: $parameters)';

  static DeviceType _deviceTypeFromString(String value) {
    return DeviceType.values.firstWhere(
      (type) => describeEnum(type).toLowerCase() == value.toLowerCase(),
      orElse: () => DeviceType.cupDroppingMachine, // Giá trị mặc định nếu không tìm thấy
    );
  }
}

  String describeEnum(Object enumValue) {
    final String description = enumValue.toString();
    final int indexOfDot = description.indexOf('.');
    assert(indexOfDot != -1 && indexOfDot < description.length - 1);
    return description.substring(indexOfDot + 1);
  }

class DeviceParameterItem {
  final String deviceId;
  final String parameters;

  DeviceParameterItem({
    required this.deviceId,
    required this.parameters,
  });

  DeviceParameterItem copyWith({
    String? deviceId,
    String? parameters,
  }) {
    return DeviceParameterItem(
      deviceId: deviceId ?? this.deviceId,
      parameters: parameters ?? this.parameters,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceId': deviceId,
      'parameters': parameters,
    };
  }

  factory DeviceParameterItem.fromJson(Map<String, dynamic> json) {
    return DeviceParameterItem(
      deviceId: json['deviceId'] as String,
      parameters: json['parameters'] as String,
    );
  }

  @override
  String toString() => 'DeviceParameterItem(deviceId: $deviceId, parameters: $parameters)';
}

class SetDeviceParameter {
  final List<DeviceParameterItem> deviceParamsList;

  SetDeviceParameter({
    required this.deviceParamsList,
  });

  SetDeviceParameter copyWith({
    List<DeviceParameterItem>? deviceParamsList,
  }) {
    return SetDeviceParameter(
      deviceParamsList: deviceParamsList ?? this.deviceParamsList,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceParamsList': deviceParamsList.map((item) => item.toJson()).toList(),
    };
  }

  factory SetDeviceParameter.fromJson(Map<String, dynamic> json) {
    return SetDeviceParameter(
      deviceParamsList: (json['deviceParamsList'] as List)
          .map((item) => DeviceParameterItem.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }

  // Helper method for backward compatibility
  static SetDeviceParameter fromSingleDevice({
    required String deviceId,
    required String parameters,
  }) {
    return SetDeviceParameter(
      deviceParamsList: [
        DeviceParameterItem(
          deviceId: deviceId,
          parameters: parameters,
        )
      ],
    );
  }

  @override
  String toString() => 'SetDeviceParameter(deviceParamsList: $deviceParamsList)';
}