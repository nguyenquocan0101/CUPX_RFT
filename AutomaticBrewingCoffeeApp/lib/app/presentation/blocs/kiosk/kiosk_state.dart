import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:flutter/material.dart';

@immutable
sealed class KioskState {}

final class KioskInitial extends KioskState {}

class KioskLoading extends KioskState {}

class KioskLoaded extends KioskState {
  final Kiosk kiosk;
  KioskLoaded({required this.kiosk});
}

class KioskError extends KioskState {
  final String message;
  KioskError({required this.message});
}

final class KioskUpdateIngredient extends KioskState {}

class KioskUpdateIngredientLoading extends KioskState {}

class KioskUpdateIngredientLoaded extends KioskState {
  final bool isSuccess;
  KioskUpdateIngredientLoaded({required this.isSuccess});
}

class KioskUpdateIngredientError extends KioskState {
  final String message;
  KioskUpdateIngredientError({required this.message});
}