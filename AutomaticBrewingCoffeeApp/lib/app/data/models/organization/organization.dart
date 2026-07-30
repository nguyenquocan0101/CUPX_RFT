import 'package:abc_androidapp/app/data/models/organization/store.dart';

class Organization {
  final String organizationId;
  final String name;
  final String organizationCode;
  final String description;
  final String contactPhone;
  final String contactEmail;
  final String logoUrl;
  final String taxId;
  final String status;
  final DateTime createdDate;
  final bool isDeleted;
  final Store? store;

  Organization({
    required this.organizationId,
    required this.name,
    required this.organizationCode,
    required this.description,
    required this.contactPhone,
    required this.contactEmail,
    required this.logoUrl,
    required this.taxId,
    required this.status,
    required this.createdDate,
    required this.isDeleted,
    this.store,
  });

  factory Organization.fromJson(Map<String, dynamic> json) {
    return Organization(
      organizationId: json['organizationId'] as String? ?? '',
      name: json['name'] as String? ?? '',
      organizationCode: json['organizationCode'] as String? ?? '',
      description: json['description'] as String? ?? '',
      contactPhone: json['contactPhone'] as String? ?? '',
      contactEmail: json['contactEmail'] as String? ?? '',
      logoUrl: json['logoUrl'] as String? ?? '',
      taxId: json['taxId'] as String? ?? '',
      status: json['status'] as String? ?? '',
      createdDate: DateTime.parse(json['createdDate'] as String),
      isDeleted: json['isDeleted'] as bool? ?? false,
      store: json['store'] != null 
          ? Store.fromJson(json['store'] as Map<String, dynamic>)
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'organizationId': organizationId,
      'name': name,
      'organizationCode': organizationCode,
      'description': description,
      'contactPhone': contactPhone,
      'contactEmail': contactEmail,
      'logoUrl': logoUrl,
      'taxId': taxId,
      'status': status,
      'createdDate': createdDate.toIso8601String(),
      'isDeleted': isDeleted,
      'store': store?.toJson(),
    };
  }

  Organization copyWith({
    String? organizationId,
    String? name,
    String? organizationCode,
    String? description,
    String? contactPhone,
    String? contactEmail,
    String? logoUrl,
    String? taxId,
    String? status,
    DateTime? createdDate,
    bool? isDeleted,
    Store? store,
  }) {
    return Organization(
      organizationId: organizationId ?? this.organizationId,
      name: name ?? this.name,
      organizationCode: organizationCode ?? this.organizationCode,
      description: description ?? this.description,
      contactPhone: contactPhone ?? this.contactPhone,
      contactEmail: contactEmail ?? this.contactEmail,
      logoUrl: logoUrl ?? this.logoUrl,
      taxId: taxId ?? this.taxId,
      status: status ?? this.status,
      createdDate: createdDate ?? this.createdDate,
      isDeleted: isDeleted ?? this.isDeleted,
      store: store ?? this.store,
    );
  }

  @override
  String toString() {
    return 'OrganizationModel(organizationId: $organizationId, name: $name, organizationCode: $organizationCode, description: $description, contactPhone: $contactPhone, contactEmail: $contactEmail, logoUrl: $logoUrl, taxId: $taxId, status: $status, createdDate: $createdDate, isDeleted: $isDeleted, store: $store)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is Organization &&
        other.organizationId == organizationId &&
        other.name == name &&
        other.organizationCode == organizationCode &&
        other.description == description &&
        other.contactPhone == contactPhone &&
        other.contactEmail == contactEmail &&
        other.logoUrl == logoUrl &&
        other.taxId == taxId &&
        other.status == status &&
        other.createdDate == createdDate &&
        other.isDeleted == isDeleted &&
        other.store == store;
  }

  @override
  int get hashCode {
    return organizationId.hashCode ^
        name.hashCode ^
        organizationCode.hashCode ^
        description.hashCode ^
        contactPhone.hashCode ^
        contactEmail.hashCode ^
        logoUrl.hashCode ^
        taxId.hashCode ^
        status.hashCode ^
        createdDate.hashCode ^
        isDeleted.hashCode ^
        store.hashCode;
  }
}
