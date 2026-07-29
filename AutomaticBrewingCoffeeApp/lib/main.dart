import 'dart:io';

import 'package:abc_androidapp/app/core/dependency_injection/service_location.dart';
import 'package:abc_androidapp/app/core/kiosk_mode/kiosk_service.dart';
import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/presentation/blocs/cart/cart_bloc.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/cubits/welcome/app_flow_cubit.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/menu_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/payment/payment_info_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/welcome/welcome_screen.dart';
import 'package:abc_androidapp/app/services/organization_service.dart';
import 'package:abc_androidapp/config/themes/app_theme.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get/get.dart';
import 'package:get_it/get_it.dart';

final sl = GetIt.instance;
final navigatorKey = GlobalKey<NavigatorState>();

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  try {
    await dotenv.load(fileName: '.env');
  } catch (_) {
    // Local builds can use the compile-time defaults in ApiConstants.
  }
  HttpOverrides.global = MyHttpOverrides();
  await setupServiceLocator();

  await KioskService.initialize();
  await Future.delayed(Duration(milliseconds: 1000));
  await KioskService.enableKioskMode();

  await _preloadAppData();

  runApp(const MainApp());
}

Future<void> _preloadAppData() async {
  try {
    await OrganizationService.instance.getOrganization();
    print('App: Essential data preloaded successfully');
  } catch (e) {
    print('App: Failed to preload data - $e');
  }
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(create: (_) => AppFlowCubit()),
        BlocProvider(create: (_) => CartBloc()),
        BlocProvider(create: (_) => CartCubit())
      ],
      child: SafeArea(
        child: GetMaterialApp(
          navigatorKey: navigatorKey,
          debugShowCheckedModeBanner: false,
          title: 'CUPX',
          theme: AppTheme.light,
          getPages: AppRouter.routes,
          initialRoute: WelcomeScreen.route,
        ),
      ),
    );
  }
}

class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback =
          (X509Certificate cert, String host, int port) => true;
  }
}
