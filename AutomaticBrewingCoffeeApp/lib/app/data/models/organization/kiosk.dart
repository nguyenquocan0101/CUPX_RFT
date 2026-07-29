class Kiosk {
  final String kioskId;
  final String kioskVersionId;
  final String menuId;
  final String apiKey;
  final String hostname;
  final String position;
  final DateTime warrantyTime;
  final String storeId;
  final String location;
  final String status;
  final DateTime installedDate;
  final DateTime createdDate;
  final List<KioskDeviceMapping> kioskDevices;

  Kiosk({
    required this.kioskId,
    required this.kioskVersionId,
    required this.menuId,
    required this.apiKey,
    required this.hostname,
    required this.position,
    required this.warrantyTime,
    required this.storeId,
    required this.location,
    required this.status,
    required this.installedDate,
    required this.createdDate,
    required this.kioskDevices,
  });

  factory Kiosk.fromJson(Map<String, dynamic> json) {
    return Kiosk(
      kioskId: json['kioskId'] ?? '',
      kioskVersionId: json['kioskVersionId'] ?? '',
      menuId: json['menuId'] ?? '',
      apiKey: json['apiKey'] ?? '',
      hostname: json['hostname'] ?? '',
      position: json['position'] ?? '',
      warrantyTime: DateTime.parse(json['warrantyTime']),
      storeId: json['storeId'] ?? '',
      location: json['location'] ?? '',
      status: json['status'] ?? '',
      installedDate: DateTime.parse(json['installedDate']),
      createdDate: DateTime.parse(json['createdDate']),
      kioskDevices: (json['kioskDevices'] as List<dynamic>?)
          ?.map((item) => KioskDeviceMapping.fromJson(item))
          .toList() ?? [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'kioskId': kioskId,
      'kioskVersionId': kioskVersionId,
      'menuId': menuId,
      'apiKey': apiKey,
      'hostname': hostname,
      'position': position,
      'warrantyTime': warrantyTime.toIso8601String(),
      'storeId': storeId,
      'location': location,
      'status': status,
      'installedDate': installedDate.toIso8601String(),
      'createdDate': createdDate.toIso8601String(),
      'kioskDevices': kioskDevices.map((device) => device.toJson()).toList(),
    };
  }
}

class KioskDeviceMapping {
  final String kioskDeviceMappingId;
  final String deviceId;
  final String kioskId;
  final String status;
  final KioskDevice device;

  KioskDeviceMapping({
    required this.kioskDeviceMappingId,
    required this.deviceId,
    required this.kioskId,
    required this.status,
    required this.device,
  });

  factory KioskDeviceMapping.fromJson(Map<String, dynamic> json) {
    return KioskDeviceMapping(
      kioskDeviceMappingId: json['kioskDeviceMappingId'] ?? '',
      deviceId: json['deviceId'] ?? '',
      kioskId: json['kioskId'] ?? '',
      status: json['status'] ?? '',
      device: KioskDevice.fromJson(json['device']),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'kioskDeviceMappingId': kioskDeviceMappingId,
      'deviceId': deviceId,
      'kioskId': kioskId,
      'status': status,
      'device': device.toJson(),
    };
  }
}

class KioskDevice {
  final String deviceId;
  final String name;
  final String description;
  final String deviceModelId;
  final DeviceModel deviceModel;
  final String serialNumber;
  final String status;
  final DateTime createdDate;
  final DateTime updatedDate;
  final List<DeviceIngredientState> deviceIngredientStates;

  KioskDevice({
    required this.deviceId,
    required this.name,
    required this.description,
    required this.deviceModelId,
    required this.deviceModel,
    required this.serialNumber,
    required this.status,
    required this.createdDate,
    required this.updatedDate,
    required this.deviceIngredientStates,
  });

  factory KioskDevice.fromJson(Map<String, dynamic> json) {
    return KioskDevice(
      deviceId: json['deviceId'] ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      deviceModelId: json['deviceModelId'] ?? '',
      deviceModel: DeviceModel.fromJson(json['deviceModel']),
      serialNumber: json['serialNumber'] ?? '',
      status: json['status'] ?? '',
      createdDate: DateTime.parse(json['createdDate']),
      updatedDate: DateTime.parse(json['updatedDate']),
      deviceIngredientStates: (json['deviceIngredientStates'] as List<dynamic>?)
          ?.map((item) => DeviceIngredientState.fromJson(item))
          .toList() ?? [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceId': deviceId,
      'name': name,
      'description': description,
      'deviceModelId': deviceModelId,
      'deviceModel': deviceModel.toJson(),
      'serialNumber': serialNumber,
      'status': status,
      'createdDate': createdDate.toIso8601String(),
      'updatedDate': updatedDate.toIso8601String(),
      'deviceIngredientStates': deviceIngredientStates.map((state) => state.toJson()).toList(),
    };
  }
}

class DeviceModel {
  final String deviceModelId;
  final String modelName;
  final String manufacturer;
  final String deviceTypeId;
  final DeviceType deviceType;
  final String status;
  final DateTime createdDate;
  final DateTime? updatedDate;

  DeviceModel({
    required this.deviceModelId,
    required this.modelName,
    required this.manufacturer,
    required this.deviceTypeId,
    required this.deviceType,
    required this.status,
    required this.createdDate,
    this.updatedDate,
  });

  factory DeviceModel.fromJson(Map<String, dynamic> json) {
    return DeviceModel(
      deviceModelId: json['deviceModelId'] ?? '',
      modelName: json['modelName'] ?? '',
      manufacturer: json['manufacturer'] ?? '',
      deviceTypeId: json['deviceTypeId'] ?? '',
      deviceType: DeviceType.fromJson(json['deviceType']),
      status: json['status'] ?? '',
      createdDate: DateTime.parse(json['createdDate']),
      updatedDate: json['updatedDate'] != null 
          ? DateTime.parse(json['updatedDate']) 
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceModelId': deviceModelId,
      'modelName': modelName,
      'manufacturer': manufacturer,
      'deviceTypeId': deviceTypeId,
      'deviceType': deviceType.toJson(),
      'status': status,
      'createdDate': createdDate.toIso8601String(),
      'updatedDate': updatedDate?.toIso8601String(),
    };
  }
}

class DeviceType {
  final String deviceTypeId;
  final String name;
  final String description;
  final String status;
  final DateTime createdDate;
  final DateTime? updatedDate;
  final bool isMobileDevice;

  DeviceType({
    required this.deviceTypeId,
    required this.name,
    required this.description,
    required this.status,
    required this.createdDate,
    this.updatedDate,
    required this.isMobileDevice,
  });

  factory DeviceType.fromJson(Map<String, dynamic> json) {
    return DeviceType(
      deviceTypeId: json['deviceTypeId'] ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      status: json['status'] ?? '',
      createdDate: DateTime.parse(json['createdDate']),
      updatedDate: json['updatedDate'] != null
          ? DateTime.parse(json['updatedDate'])
          : null,
      isMobileDevice: json['isMobileDevice'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceTypeId': deviceTypeId,
      'name': name,
      'description': description,
      'status': status,
      'createdDate': createdDate.toIso8601String(),
      'updatedDate': updatedDate?.toIso8601String(),
      'isMobileDevice': isMobileDevice,
    };
  }
}

class DeviceIngredientState {
  final String deviceIngredientStateId;
  final String deviceId;
  final int maxCapacity;
  final int minCapacity;
  final int warningPercent;
  final String ingredientType;
  final int currentCapacity;
  final String capacityLevel;
  final String unit;
  final bool isWarning;
  final bool isRenewable;
  final bool isPrimary;

  DeviceIngredientState({
    required this.deviceIngredientStateId,
    required this.deviceId,
    required this.maxCapacity,
    required this.minCapacity,
    required this.warningPercent,
    required this.ingredientType,
    required this.currentCapacity,
    required this.capacityLevel,
    required this.unit,
    required this.isWarning,
    required this.isRenewable,
    required this.isPrimary,
  });

  factory DeviceIngredientState.fromJson(Map<String, dynamic> json) {
    return DeviceIngredientState(
      deviceIngredientStateId: json['deviceIngredientStateId'] ?? '',
      deviceId: json['deviceId'] ?? '',
      maxCapacity: json['maxCapacity'] ?? 0,
      minCapacity: json['minCapacity'] ?? 0,
      warningPercent: json['warningPercent'] ?? 0,
      ingredientType: json['ingredientType'] ?? '',
      currentCapacity: json['currentCapacity'] ?? 0,
      capacityLevel: json['capacityLevel'] ?? '',
      unit: json['unit'] ?? '',
      isWarning: json['isWarning'] ?? false,
      isRenewable: json['isRenewable'] ?? false,
      isPrimary: json['isPrimary'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceIngredientStateId': deviceIngredientStateId,
      'deviceId': deviceId,
      'maxCapacity': maxCapacity,
      'minCapacity': minCapacity,
      'warningPercent': warningPercent,
      'ingredientType': ingredientType,
      'currentCapacity': currentCapacity,
      'capacityLevel': capacityLevel,
      'unit': unit,
      'isWarning': isWarning,
      'isRenewable': isRenewable,
      'isPrimary': isPrimary,
    };
  }

  double get capacityPercentage {
    if (maxCapacity == 0) return 0.0;
    return (currentCapacity / maxCapacity) * 100;
  }

  bool get isLowCapacity {
    return capacityPercentage <= warningPercent;
  }
}