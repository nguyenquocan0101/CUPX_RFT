import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';

@immutable
abstract class OrganizationEvent extends Equatable {
  const OrganizationEvent();
}

class GetOrganizationEvent extends OrganizationEvent {

  const GetOrganizationEvent();
  
  @override
  // TODO: implement props
  List<Object?> get props => [];
}
