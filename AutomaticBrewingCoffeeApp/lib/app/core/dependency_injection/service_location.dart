import 'package:abc_androidapp/app/core/network/network_service.dart';
import 'package:abc_androidapp/app/core/signalr/signalr_service.dart';
import 'package:abc_androidapp/app/data/datasources/device_datasource.dart';
import 'package:abc_androidapp/app/data/datasources/kiosk_datasource.dart';
import 'package:abc_androidapp/app/data/datasources/menu_datasource.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/datasources/organization_datasource.dart';
import 'package:abc_androidapp/app/data/datasources/product_datasource.dart';
import 'package:abc_androidapp/app/data/repositories/device_repository_impl.dart';
import 'package:abc_androidapp/app/data/repositories/kiosk_repository_impl.dart';
import 'package:abc_androidapp/app/data/repositories/menu_repository_impl.dart';
import 'package:abc_androidapp/app/data/repositories/order_repository_impl.dart';
import 'package:abc_androidapp/app/data/repositories/organization_repository_impl.dart';
import 'package:abc_androidapp/app/data/repositories/product_repository_impl.dart';
import 'package:abc_androidapp/app/data/repositories/signalr_repository_impl.dart';
import 'package:abc_androidapp/app/domain/repositories/device_repository.dart';
import 'package:abc_androidapp/app/domain/repositories/kiosk_repository.dart';
import 'package:abc_androidapp/app/domain/repositories/menu_repository.dart';
import 'package:abc_androidapp/app/domain/repositories/order_repository.dart';
import 'package:abc_androidapp/app/domain/repositories/organization_repository.dart';
import 'package:abc_androidapp/app/domain/repositories/product_repository.dart';
import 'package:abc_androidapp/app/domain/repositories/signalr_repository.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_device_by_id_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_device_parameters_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_devices_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/update_device_coordinates_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/update_device_parameters_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/kiosk/get_kiosk_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/kiosk/update_ingredient_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/menu/get_menu_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/order/cancel_order_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/order/create_order_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/order/get_order_history.dart';
import 'package:abc_androidapp/app/domain/usecases/organization/get_organization_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/product/get_product_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/product/get_selling_products_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/connect_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/disconnect_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/invoke_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/subscribe_signalr_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/signalr/unsubscribe_signalr_usecase.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/menu/menu_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/order/order_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/product/product_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_event.dart';
import 'package:get_it/get_it.dart';
import 'package:shared_preferences/shared_preferences.dart';

final sl = GetIt.instance;

Future<void> setupServiceLocator() async {
  //Dio Network
  sl.registerLazySingleton(() => NetworkService());

  // External
  final sharedPreferences = await SharedPreferences.getInstance();
  sl.registerLazySingleton<SharedPreferences>(() => sharedPreferences);

  //Datasource - 1
  sl.registerLazySingleton<ProductDatasource>(
      () => ProductDatasource(api: sl()));
  sl.registerLazySingleton<MenuDatasource>(() => MenuDatasource(api: sl()));
  sl.registerLazySingleton<OrderDatasource>(() => OrderDatasource(api: sl()));
  sl.registerLazySingleton<DeviceDatasource>(() => DeviceDatasource(api: sl()));
  sl.registerLazySingleton<OrganizationDatasource>(
      () => OrganizationDatasource(api: sl()));
  sl.registerLazySingleton<KioskDatasource>(() => KioskDatasource(api: sl()));

  //Repositories - 2
  sl.registerLazySingleton<ProductRepository>(
      () => ProductRepositoryImpl(productDatasource: sl()));
  sl.registerLazySingleton<MenuRepository>(
      () => MenuRepositoryImpl(menuDatasource: sl()));
  sl.registerLazySingleton<OrderRepository>(
      () => OrderRepositoryImpl(orderDatasource: sl()));
  sl.registerLazySingleton<DeviceRepository>(
      () => DeviceRepositoryImpl(deviceDatasource: sl()));
  sl.registerLazySingleton<SignalRRepository>(() =>
      SignalRRepositoryImpl(signalRService: sl() // Replace with your actual URL
          ));
  sl.registerLazySingleton<OrganizationRepository>(
      () => OrganizationRepositoryImpl(organizationDatasource: sl()));
  sl.registerLazySingleton<KioskRepository>(
      () => KioskRepositoryImpl(kioskDatasource: sl()));

  //UseCases - 3
  //* Product
  sl.registerLazySingleton<GetProductsUseCase>(() => GetProductsUseCase(sl()));
  sl.registerLazySingleton<GetProductUseCase>(() => GetProductUseCase(sl()));

  //* Menu
  sl.registerLazySingleton<GetMenuUseCase>(
      () => GetMenuUseCase(menuRepository: sl()));

  //* Order
  sl.registerLazySingleton<CreateOrderUseCase>(
      () => CreateOrderUseCase(orderRepository: sl()));
  sl.registerLazySingleton<CancelOrderUsecase>(
      () => CancelOrderUsecase(orderRepository: sl()));
  sl.registerLazySingleton<GetOrderHistoryUseCase>(
      () => GetOrderHistoryUseCase(orderRepository: sl()));

  //* Device
  sl.registerLazySingleton<GetDevicesUsecase>(
      () => GetDevicesUsecase(deviceRepository: sl()));
  sl.registerLazySingleton<GetDeviceByIdUsecase>(
      () => GetDeviceByIdUsecase(deviceRepository: sl()));
  sl.registerLazySingleton<GetDeviceParametersUsecase>(
      () => GetDeviceParametersUsecase(deviceRepository: sl()));
  sl.registerLazySingleton<UpdateDeviceParameterUsecase>(
      () => UpdateDeviceParameterUsecase(deviceRepository: sl()));
  sl.registerLazySingleton<UpdateDeviceCoordinatesUsecase>(
      () => UpdateDeviceCoordinatesUsecase(deviceRepository: sl()));
  sl.registerLazySingleton(() => SignalRService());

  //* SignalR
  sl.registerLazySingleton<ConnectSignalRUseCase>(
      () => ConnectSignalRUseCase(signalRRepository: sl()));

  // Đăng ký các usecase khác của SignalR nếu cần
  sl.registerLazySingleton<DisconnectSignalRUseCase>(
      () => DisconnectSignalRUseCase(signalRRepository: sl()));
  sl.registerLazySingleton<SubscribeSignalRUseCase>(
      () => SubscribeSignalRUseCase(signalRRepository: sl()));
  sl.registerLazySingleton<UnsubscribeSignalRUseCase>(
      () => UnsubscribeSignalRUseCase(signalRRepository: sl()));
  sl.registerLazySingleton<InvokeSignalRMethodUseCase>(
      () => InvokeSignalRMethodUseCase(signalRRepository: sl()));

  //* Organization
  sl.registerLazySingleton<GetOrganizationUseCase>(
      () => GetOrganizationUseCase(organizationRepository: sl()));

  //* Kiosk
  sl.registerLazySingleton<GetKioskUseCase>(
      () => GetKioskUseCase(kioskRepository: sl()));
  sl.registerLazySingleton<UpdateIngredientUsecase>(
      () => UpdateIngredientUsecase(kioskRepository: sl()));

  // Blocs - 4
  sl.registerFactory<ProductBloc>(
    () => ProductBloc(
      getProductsUseCase: sl(),
      getProductUseCase: sl(),
    ),
  );
  sl.registerFactory<MenuBloc>(
    () => MenuBloc(getMenuUseCase: sl()),
  );
  sl.registerFactory<OrderBloc>(
    () => OrderBloc(
      createOrderUseCase: sl(),
      cancelOrderUsecase: sl(),
    ),
  );
  sl.registerFactory<DeviceBloc>(
    () => DeviceBloc(
      getDevicesUsecase: sl(),
      // getDeviceByIdUsecase: sl(),
      // getDeviceParametersUsecase: sl(),
      updateDeviceParameterUsecase: sl(),
      updateDeviceCoordinatesUsecase: sl(),
    ),
  );
  sl.registerFactory<SignalRBloc>(
    () => SignalRBloc(
      connectSignalRUseCase: sl(),
      disconnectSignalRUseCase: sl(),
      subscribeSignalRUseCase: sl(),
      unsubscribeSignalRUseCase: sl(),
      invokeSignalRMethodUseCase: sl(),
    ),
  );
  sl.registerFactory<OrganizationBloc>(
    () => OrganizationBloc(getOrganizationUseCase: sl()),
  );
  sl.registerFactory<KioskBloc>(
    () => KioskBloc(
      getKioskUseCase: sl(),
      updateIngredientUsecase: sl(),
    ),
  );
}
