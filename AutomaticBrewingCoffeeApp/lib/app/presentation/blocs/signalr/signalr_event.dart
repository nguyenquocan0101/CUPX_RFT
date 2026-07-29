
import 'package:equatable/equatable.dart';

abstract class SignalREvent extends Equatable {
  const SignalREvent();

  @override
  List<Object> get props => [];
}

class ConnectEvent extends SignalREvent {}

class DisconnectEvent extends SignalREvent {}

class ConnectionStatusChanged extends SignalREvent {
  final bool isConnected;

  const ConnectionStatusChanged({required this.isConnected});

  @override
  List<Object> get props => [isConnected];
}

class SubscribeEvent extends SignalREvent {
  final String eventName;

  const SubscribeEvent({required this.eventName});

  @override
  List<Object> get props => [eventName];
}

class UnsubscribeEvent extends SignalREvent {
  final String eventName;

  const UnsubscribeEvent({required this.eventName});

  @override
  List<Object> get props => [eventName];
}

class DataReceivedEvent extends SignalREvent {
  final String eventName;
  final dynamic data;

  const DataReceivedEvent({
    required this.eventName,
    required this.data,
  });

  @override
  List<Object> get props => [eventName, data ?? ''];
}

class InvokeMethodEvent extends SignalREvent {
  final String methodName;
  final List<dynamic>? args;

  const InvokeMethodEvent({
    required this.methodName,
    this.args,
  });
  
  @override
  List<Object> get props => [methodName, args ?? []];
}