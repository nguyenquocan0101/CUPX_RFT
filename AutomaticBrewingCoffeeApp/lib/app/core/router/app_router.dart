import 'package:abc_androidapp/app/core/dependency_injection/service_location.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/domain/repositories/product_repository.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_device_by_id_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_device_parameters_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/get_devices_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/update_device_coordinates_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/device/update_device_parameters_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/kiosk/get_kiosk_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/kiosk/update_ingredient_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/organization/get_organization_usecase.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/menu/menu_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/order/order_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/order_history/order_history_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/product/product_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/product_detail/product_detail_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_bloc.dart';
import 'package:abc_androidapp/app/presentation/screens/order_confirmation/order_confirmation_screeen.dart';
import 'package:abc_androidapp/app/presentation/screens/failure/failure_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/menu_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/order_history/order_history_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/order_processing/order_processing_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/payment/payment_info_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/product_detail/product_detail_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/success/success.screen.dart';
import 'package:abc_androidapp/app/presentation/screens/welcome/welcome_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/setting/setting_screen.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_bloc/flutter_bloc.dart' hide Transition;
import 'package:get/get.dart';

class AppRouter {
  static List<GetPage> get routes => [
        GetPage(
          name: WelcomeScreen.route,
          page: () => const WelcomeScreen(),
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: MenuScreen.route,
          page: () => MultiRepositoryProvider(
            providers: [
              RepositoryProvider<ProductRepository>(create: (_) => sl()),
              // thêm các repository khác nếu cần
            ],
            child: MultiBlocProvider(
              providers: [
                BlocProvider<MenuBloc>(create: (_) => sl()),
                BlocProvider<ProductBloc>(create: (_) => sl()),
                // thêm các bloc khác nếu cần
              ],
              child: const MenuScreen(),
            ),
          ),
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: OrderHistoryScreen.route,
          page: () => BlocProvider(
            create: (_) => OrderHistoryBloc(getOrderHistory: sl()),
            child: const OrderHistoryScreen(),
          ),
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: ProductDetailScreen.route,
          page: () {
            final product = Get.arguments as Product;
            return MultiBlocProvider(
              providers: [
                BlocProvider<ProductBloc>(create: (_) => sl()),
                BlocProvider<ProductDetailBloc>(create: (_) => sl()),
                // thêm các bloc khác nếu cần
              ],
              child: ProductDetailScreen(product: product),
            );
          },
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: OrderConfirmationScreen.route,
          page: () {
            return MultiBlocProvider(
              providers: [
                BlocProvider<OrderBloc>(create: (_) => sl()),
                BlocProvider<SignalRBloc>(create: (_) => sl()),
              ],
              child: const OrderConfirmationScreen(),
            );
          },
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: SuccessScreen.route,
          page: () => const SuccessScreen(),
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: FailureScreen.route,
          page: () => const FailureScreen(),
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: SettingScreen.route,
          page: () => MultiBlocProvider(
            providers: [
              // BlocProvider<DeviceBloc>(
              //   create: (context) => DeviceBloc(
              //     getDevicesUsecase: sl<GetDevicesUsecase>(),
              //     updateDeviceParameterUsecase:
              //         sl<UpdateDeviceParameterUsecase>(),
              //     updateDeviceCoordinatesUsecase:
              //         sl<UpdateDeviceCoordinatesUsecase>(),
              //   ),
              // ),
              BlocProvider<OrganizationBloc>(
                create: (context) => OrganizationBloc(
                  getOrganizationUseCase: sl<GetOrganizationUseCase>(),
                ),
              ),
               BlocProvider<KioskBloc>(
                create: (context) => KioskBloc(
                  getKioskUseCase: sl<GetKioskUseCase>(),
                  updateIngredientUsecase: sl<UpdateIngredientUsecase>(),
                ),
              ),
            ],
            child: const SettingScreen(),
          ),
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
        GetPage(
          name: OrderProcessingScreen.route,
          page: () {
            final orderId = Get.arguments['orderId'] as String;
            final orderCode = Get.arguments['orderCode'] as String;
            return MultiBlocProvider(
              providers: [
                BlocProvider<SignalRBloc>(create: (_) => sl()),
                BlocProvider<OrderBloc>(create: (_) => sl()),
                BlocProvider<OrganizationBloc>(
                  create: (_) => sl(),
                ),
              ],
              child: OrderProcessingScreen(orderId: orderId, orderCode: orderCode),
            );
          },
          transition: Transition.cupertino,
          transitionDuration: const Duration(milliseconds: 300),
        ),
      ];

  static void navigateToMenuScreen() {
    Get.toNamed(MenuScreen.route);
  }

  static void navigateToOrderHistoryScreen() {
    Get.toNamed(OrderHistoryScreen.route);
  }

  static void navigateToSettingScreen() {
    Get.toNamed(SettingScreen.route);
  }

  static void navigateToProductDetailScreen(Product product) {
    Get.toNamed(ProductDetailScreen.route, arguments: product);
  }

  static void navigateToOrderConfirmationScreen() {
    Get.toNamed(OrderConfirmationScreen.route);
  }

  //* navigate with continous event bloc context
  static void navigateToPaymentInfoScreen(BuildContext context) {
    Get.to(
      () => MultiBlocProvider(
        providers: [
          BlocProvider.value(
            value: BlocProvider.of<OrderBloc>(context),
          ),
          BlocProvider.value(
            value: BlocProvider.of<SignalRBloc>(context),
          ),
        ],
        child: const PaymentInfoScreen(),
      ),
    );
  }

  static void navigateToOrderProcessingScreen(String orderId, String orderCode) {
  WidgetsBinding.instance.addPostFrameCallback((_) {
    Get.toNamed(OrderProcessingScreen.route, arguments: {
      'orderId': orderId,
      'orderCode': orderCode,
    });
  });
}


  // Xóa tất cả routes trừ WelcomeScreen, sau đó push MenuScreen
  static void navigateToMenuAfterPayment() {
    Get.offAllNamed(
      MenuScreen.route,
      predicate: (route) => route.settings.name == WelcomeScreen.route,
    );
  }

}
