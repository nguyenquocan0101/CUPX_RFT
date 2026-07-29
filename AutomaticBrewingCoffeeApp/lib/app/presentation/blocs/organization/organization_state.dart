import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:flutter/material.dart';

@immutable
sealed class OrganizationState {}

final class OrganizationInitial extends OrganizationState {}

class OrganizationLoading extends OrganizationState {}

class OrganizationLoaded extends OrganizationState {
  final Organization organization;
  OrganizationLoaded({required this.organization});
}

class OrganizationError extends OrganizationState {
  final String message;
  OrganizationError({required this.message});
}
