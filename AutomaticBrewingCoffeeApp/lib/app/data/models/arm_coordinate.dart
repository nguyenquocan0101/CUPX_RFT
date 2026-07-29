import 'dart:convert';

class ArmCoordinate {
  final double x;
  final double y;
  final double z;
  final double rx;
  final double ry;
  final double rz;
  final double j1;
  final double j2;
  final double j3;
  final double j4;
  final double j5;
  final double j6;

  ArmCoordinate({
    required this.x,
    required this.y,
    required this.z,
    required this.rx,
    required this.ry,
    required this.rz,
    required this.j1,
    required this.j2,
    required this.j3,
    required this.j4,
    required this.j5,
    required this.j6,
  });

  ArmCoordinate copyWith({
    double? x,
    double? y,
    double? z,
    double? rx,
    double? ry,
    double? rz,
    double? j1,
    double? j2,
    double? j3,
    double? j4,
    double? j5,
    double? j6,
  }) {
    return ArmCoordinate(
      x: x ?? this.x,
      y: y ?? this.y,
      z: z ?? this.z,
      rx: rx ?? this.rx,
      ry: ry ?? this.ry,
      rz: rz ?? this.rz,
      j1: j1 ?? this.j1,
      j2: j2 ?? this.j2,
      j3: j3 ?? this.j3,
      j4: j4 ?? this.j4,
      j5: j5 ?? this.j5,
      j6: j6 ?? this.j6,
    );
  }

  factory ArmCoordinate.fromJson(Map<String, dynamic> json) {
    return ArmCoordinate(
      x: json['X']?.toDouble() ?? 0.0,
      y: json['Y']?.toDouble() ?? 0.0,
      z: json['Z']?.toDouble() ?? 0.0,
      rx: json['RX']?.toDouble() ?? 0.0,
      ry: json['RY']?.toDouble() ?? 0.0,
      rz: json['RZ']?.toDouble() ?? 0.0,
      j1: json['J1']?.toDouble() ?? 0.0,
      j2: json['J2']?.toDouble() ?? 0.0,
      j3: json['J3']?.toDouble() ?? 0.0,
      j4: json['J4']?.toDouble() ?? 0.0,
      j5: json['J5']?.toDouble() ?? 0.0,
      j6: json['J6']?.toDouble() ?? 0.0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'X': x,
      'Y': y,
      'Z': z,
      'RX': rx,
      'RY': ry,
      'RZ': rz,
      'J1': j1,
      'J2': j2,
      'J3': j3,
      'J4': j4,
      'J5': j5,
      'J6': j6,
    };
  }

  factory ArmCoordinate.fromString(String source) {
    return ArmCoordinate.fromJson(json.decode(source));
  }
  
  String toJsonString() {
    return json.encode(toJson());
  }

}