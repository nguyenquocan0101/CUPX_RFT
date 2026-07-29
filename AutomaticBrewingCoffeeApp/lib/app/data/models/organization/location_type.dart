class LocationType {
  final String locationTypeId;
  final String name;
  final String description;
  final DateTime createdDate;

  LocationType({
    required this.locationTypeId,
    required this.name,
    required this.description,
    required this.createdDate,
  });

  factory LocationType.fromJson(Map<String, dynamic> json) {
    return LocationType(
      locationTypeId: json['locationTypeId'] as String,
      name: json['name'] as String,
      description: json['description'] as String,
      createdDate: DateTime.parse(json['createdDate'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'locationTypeId': locationTypeId,
      'name': name,
      'description': description,
      'createdDate': createdDate.toIso8601String(),
    };
  }

  LocationType copyWith({
    String? locationTypeId,
    String? name,
    String? description,
    DateTime? createdDate,
  }) {
    return LocationType(
      locationTypeId: locationTypeId ?? this.locationTypeId,
      name: name ?? this.name,
      description: description ?? this.description,
      createdDate: createdDate ?? this.createdDate,
    );
  }

  @override
  String toString() {
    return 'LocationType(locationTypeId: $locationTypeId, name: $name, description: $description, createdDate: $createdDate)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is LocationType &&
        other.locationTypeId == locationTypeId &&
        other.name == name &&
        other.description == description &&
        other.createdDate == createdDate;
  }

  @override
  int get hashCode {
    return locationTypeId.hashCode ^
        name.hashCode ^
        description.hashCode ^
        createdDate.hashCode;
  }
}