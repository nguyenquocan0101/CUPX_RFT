import 'dart:async';
import 'package:abc_androidapp/app/domain/usecases/signalr/connect_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/disconnect_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/invoke_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/subscribe_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/unsubscribe_signalr_usecase.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_state.dart';
import 'package:bloc/bloc.dart';

class SignalRBloc extends Bloc<SignalREvent, SignalRState> {
  final ConnectSignalRUseCase connectSignalRUseCase;
  final DisconnectSignalRUseCase disconnectSignalRUseCase;
  final SubscribeSignalRUseCase subscribeSignalRUseCase;
  final UnsubscribeSignalRUseCase unsubscribeSignalRUseCase;
  final InvokeSignalRMethodUseCase invokeSignalRMethodUseCase;

  StreamSubscription<bool>? _connectionStatusSubscription;

  SignalRBloc({
    required this.connectSignalRUseCase,
    required this.disconnectSignalRUseCase,
    required this.subscribeSignalRUseCase,
    required this.unsubscribeSignalRUseCase,
    required this.invokeSignalRMethodUseCase,
  }) : super(SignalRDisconnected()) {
    on<ConnectEvent>(_onConnect);
    on<DisconnectEvent>(_onDisconnect);
    on<ConnectionStatusChanged>(_onConnectionStatusChanged);
    on<SubscribeEvent>(_onSubscribe);
    on<UnsubscribeEvent>(_onUnsubscribe);
    on<DataReceivedEvent>(_onDataReceived);
    on<InvokeMethodEvent>(_onInvoke);

    // Listen to connection status
    _connectionStatusSubscription = connectSignalRUseCase.signalRRepository.connectionStatus
        .listen((isConnected) {
      add(ConnectionStatusChanged(isConnected: isConnected));
    });
  }

  Future<void> _onConnect(
    ConnectEvent event, 
    Emitter<SignalRState> emit
  ) async {
    emit(SignalRConnecting());
    
    final result = await connectSignalRUseCase.execute();
    
    result.fold(
      (failure) => emit(SignalRConnectionError(message: failure.message)),
      (_) => emit(SignalRConnected()),
    );
  }
  
  Future<void> _onDisconnect(
    DisconnectEvent event, 
    Emitter<SignalRState> emit
  ) async {
    await connectSignalRUseCase.signalRRepository.disconnect();
    emit(SignalRDisconnected());
  }

  Future<void> _onConnectionStatusChanged(
    ConnectionStatusChanged event, 
    Emitter<SignalRState> emit
  ) async {
    if (event.isConnected) {
      emit(SignalRConnected());
    } else {
      emit(SignalRDisconnected());
    }
  }
  
  Future<void> _onSubscribe(
    SubscribeEvent event,
    Emitter<SignalRState> emit
  ) async {
    try {
      // Connect if not already connected
      if (!connectSignalRUseCase.signalRRepository.isConnected) {
        final result = await connectSignalRUseCase.execute();
        if (result.isLeft()) {
          emit(SignalRConnectionError(
            message: 'Failed to connect before subscribing: ${result.fold(
              (failure) => failure.message,
              (_) => '',
            )}'
          ));
          return;
        }
      }

      subscribeSignalRUseCase.execute<dynamic>(
        event.eventName,
        (data) {
          add(DataReceivedEvent(
            eventName: event.eventName,
            data: data,
          ));
        },
      );

      emit(SignalRSubscribed(eventName: event.eventName));
    } catch (e) {
      emit(SignalRError(message: 'Failed to subscribe to ${event.eventName}: $e'));
    }
  }

  Future<void> _onUnsubscribe(
    UnsubscribeEvent event,
    Emitter<SignalRState> emit
  ) async {
    try {
      connectSignalRUseCase.signalRRepository.unsubscribe(event.eventName);
      //emit(SignalRUnsubscribed(eventName: event.eventName));
    } catch (e) {
      emit(SignalRError(message: 'Failed to unsubscribe from ${event.eventName}: $e'));
    }
  }

  Future<void> _onDataReceived(
    DataReceivedEvent event,
    Emitter<SignalRState> emit
  ) async {
    emit(SignalREventReceived(
      eventName: event.eventName,
      data: event.data,
    ));
  }

  Future<void> _onInvoke(
    InvokeMethodEvent event,
    Emitter<SignalRState> emit
  ) async {
    emit(SignalRMethodInvoking(methodName: event.methodName));
    
    try {
      if (!connectSignalRUseCase.signalRRepository.isConnected) {
        final result = await connectSignalRUseCase.execute();
        if (result.isLeft()) {
          emit(SignalRMethodError(
            methodName: event.methodName,
            message: 'Failed to connect before invoking method: ${result.fold(
              (failure) => failure.message,
              (_) => '',
            )}',
          ));
          return;
        }
      }
      
      // Invoke the method
      final result = await connectSignalRUseCase.signalRRepository.invoke(
        event.methodName,
        args: event.args,
      );
      
      emit(SignalRMethodInvoked(
        methodName: event.methodName,
        result: result,
      ));
    } catch (e) {
      emit(SignalRMethodError(
        methodName: event.methodName,
        message: 'Error invoking method: $e',
      ));
    }
  }

  @override
  Future<void> close() {
    _connectionStatusSubscription?.cancel();
    return super.close();
  }
}