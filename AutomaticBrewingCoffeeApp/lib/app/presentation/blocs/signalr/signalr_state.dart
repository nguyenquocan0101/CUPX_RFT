import 'package:equatable/equatable.dart';

abstract class SignalRState extends Equatable {
  const SignalRState();
  
  @override
  List<Object> get props => [];
}

class SignalRDisconnected extends SignalRState {}

class SignalRConnecting extends SignalRState {}

class SignalRConnected extends SignalRState {}

class SignalRConnectionError extends SignalRState {
  final String message;
  
  const SignalRConnectionError({required this.message});
  
  @override
  List<Object> get props => [message];
}

class SignalRSubscribed extends SignalRState {
  final String eventName;
  
  const SignalRSubscribed({required this.eventName});
  
  @override
  List<Object> get props => [eventName];
}

class SignalRSubscriptionError extends SignalRState {
  final String eventName;
  final String message;
  
  const SignalRSubscriptionError({
    required this.eventName,
    required this.message,
  });
  
  @override
  List<Object> get props => [eventName, message];
}

class SignalRUnsubscribed extends SignalRState {
  final String eventName;
  
  const SignalRUnsubscribed({required this.eventName});
  
  @override
  List<Object> get props => [eventName];
}

class SignalREventReceived extends SignalRState {
  final String eventName;
  final dynamic data;
  
  const SignalREventReceived({
    required this.eventName,
    required this.data,
  });
  
  @override
  List<Object> get props => [eventName, data ?? ''];
}

class SignalRMethodInvoking extends SignalRState {
  final String methodName;
  
  const SignalRMethodInvoking({required this.methodName});
  
  @override
  List<Object> get props => [methodName];
}

class SignalRMethodInvoked extends SignalRState {
  final String methodName;
  final dynamic result;
  
  const SignalRMethodInvoked({
    required this.methodName,
    this.result,
  });
  
  @override
  List<Object> get props => [methodName, result ?? ''];
}

class SignalRMethodError extends SignalRState {
  final String methodName;
  final String message;
  
  const SignalRMethodError({
    required this.methodName,
    required this.message,
  });
  
  @override
  List<Object> get props => [methodName, message];
}

class SignalRError extends SignalRState {
  final String message;
  
  const SignalRError({required this.message});
  
  @override
  List<Object> get props => [message];
}