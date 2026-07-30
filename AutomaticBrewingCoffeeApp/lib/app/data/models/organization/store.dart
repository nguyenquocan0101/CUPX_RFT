import 'package:abc_androidapp/app/data/models/organization/location_type.dart';

class Store {
  final String storeId;
  final String organizationId;
  final String contactPhone;
  final String name;
  final String locationAddress;
  final String locationTypeId;
  final LocationType? locationType;
  final String status;

  Store({
    required this.storeId,
    required this.organizationId,
    required this.contactPhone,
    required this.name,
    required this.locationAddress,
    required this.locationTypeId,
    this.locationType,
    required this.status,
  });

  factory Store.fromJson(Map<String, dynamic> json) {
    return Store(
      storeId: json['storeId'] as String? ?? '',
      organizationId: json['organizationId'] as String? ?? '',
      contactPhone: json['contactPhone'] as String? ?? '',
      name: json['name'] as String? ?? '',
      locationAddress: json['locationAddress'] as String? ?? '',
      locationTypeId: json['locationTypeId'] as String? ?? '',
      locationType: json['locationType'] != null
          ? LocationType.fromJson(json['locationType'] as Map<String, dynamic>)
          : null,
      status: json['status'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'storeId': storeId,
      'organizationId': organizationId,
      'contactPhone': contactPhone,
      'name': name,
      'locationAddress': locationAddress,
      'locationTypeId': locationTypeId,
      'locationType': locationType?.toJson(),
      'status': status,
    };
  }

  Store copyWith({
    String? storeId,
    String? organizationId,
    String? contactPhone,
    String? name,
    String? locationAddress,
    String? locationTypeId,
    LocationType? locationType,
    String? status,
  }) {
    return Store(
      storeId: storeId ?? this.storeId,
      organizationId: organizationId ?? this.organizationId,
      contactPhone: contactPhone ?? this.contactPhone,
      name: name ?? this.name,
      locationAddress: locationAddress ?? this.locationAddress,
      locationTypeId: locationTypeId ?? this.locationTypeId,
      locationType: locationType ?? this.locationType,
      status: status ?? this.status,
    );
  }

  @override
  String toString() {
    return 'StoreModel(storeId: $storeId, organizationId: $organizationId, contactPhone: $contactPhone, name: $name, locationAddress: $locationAddress, locationTypeId: $locationTypeId, locationType: $locationType, status: $status)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is Store &&
        other.storeId == storeId &&
        other.organizationId == organizationId &&
        other.contactPhone == contactPhone &&
        other.name == name &&
        other.locationAddress == locationAddress &&
        other.locationTypeId == locationTypeId &&
        other.locationType == locationType &&
        other.status == status;
  }

  @override
  int get hashCode {
    return storeId.hashCode ^
        organizationId.hashCode ^
        contactPhone.hashCode ^
        name.hashCode ^
        locationAddress.hashCode ^
        locationTypeId.hashCode ^
        locationType.hashCode ^
        status.hashCode;
  }
}
