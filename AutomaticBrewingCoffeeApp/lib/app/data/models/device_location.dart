class DeviceLocation {
  final String deviceId;
  final double x;
  final double y;
  final double z;

  DeviceLocation({
    required this.deviceId,
    required this.x,
    required this.y,
    required this.z,
  });

  factory DeviceLocation.fromJson(Map<String, dynamic> json) {
    return DeviceLocation(
      deviceId: json['deviceId'] as String,
      x: (json['x'] as num).toDouble(),
      y: (json['y'] as num).toDouble(),
      z: (json['z'] as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'deviceId': deviceId,
      'x': x,
      'y': y,
      'z': z,
    };
  }

  @override
  String toString() =>
      'DeviceLocation(deviceId: $deviceId, x: $x, y: $y, z: $z)';
}