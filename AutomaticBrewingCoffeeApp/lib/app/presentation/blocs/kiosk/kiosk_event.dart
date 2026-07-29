import 'package:abc_androidapp/app/data/models/organization/update_ingredient_request.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';

@immutable
abstract class KioskEvent extends Equatable {
  const KioskEvent();
}

class GetKioskEvent extends KioskEvent {

  const GetKioskEvent();

  @override
  List<Object?> get props => [];
}

class UpdateIngredientEvent extends KioskEvent {
  final UpdateIngredientRequest request;

  const UpdateIngredientEvent({required this.request});

  @override
  List<Object?> get props => [request];
}