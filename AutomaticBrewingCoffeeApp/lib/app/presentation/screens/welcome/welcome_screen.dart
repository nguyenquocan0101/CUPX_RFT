import 'package:abc_androidapp/app/core/network/api_constants.dart';
import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/core/kiosk_mode/admin_exit_detector.dart'; // Thêm import
import 'package:abc_androidapp/app/presentation/cubits/welcome/app_flow_cubit.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';
import 'dart:math' as math;

class WelcomeScreen extends StatefulWidget {
  static const String route = "/welcome";

  const WelcomeScreen({super.key});

  @override
  State<WelcomeScreen> createState() => _WelcomeScreenState();
}

class _WelcomeScreenState extends State<WelcomeScreen>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _rotationAnimation;
  bool _isLogoTapped = false;

  // Primary brand color
  final Color primaryColor = const Color(0xFF57B7E7);
  final String kioskSide = ApiConstants.side;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 20),
    )..repeat();
    _rotationAnimation =
        Tween<double>(begin: 0, end: 2 * math.pi).animate(_controller);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AdminExitDetector(
      // Wrap WelcomeScreen với AdminExitDetector
      child: BlocListener<AppFlowCubit, AppFlowState>(
        listenWhen: (previous, current) => current is AppFlowUpdate,
        listener: (context, state) {
          final flow = (state as AppFlowUpdate).appFlow;
          if (flow == AppFlow.order) {
            AppRouter.navigateToMenuScreen();
            return;
          }
          if (flow == AppFlow.history) {
            AppRouter.navigateToOrderHistoryScreen();
            return;
          }

          if (flow == AppFlow.setting) {
            AppRouter.navigateToSettingScreen();
            return;
          }
        },
        child: Scaffold(
          backgroundColor: Colors.white,
          body: Stack(
            children: [
              // Background elements
              Positioned(
                top: -100,
                right: -100,
                child: Container(
                  width: 250,
                  height: 250,
                  decoration: BoxDecoration(
                    color: primaryColor.withOpacity(0.04),
                    shape: BoxShape.circle,
                  ),
                ),
              ),
              Positioned(
                bottom: -80,
                left: -80,
                child: Container(
                  width: 180,
                  height: 180,
                  decoration: BoxDecoration(
                    color: primaryColor.withOpacity(0.03),
                    shape: BoxShape.circle,
                  ),
                ),
              ),
              Positioned(
                top: MediaQuery.of(context).padding.top +
                    18, // Account for status bar
                right: 22,
                child: Material(
                  color: Colors.transparent,
                  borderRadius: BorderRadius.circular(24),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(24),
                    onTap: () => context
                        .read<AppFlowCubit>()
                        .updateAppFlow(AppFlow.setting),
                    child: Opacity(
                      opacity: 0.0,
                      child: Icon(
                        TDIcons.setting,
                        color: primaryColor,
                        size: 35,
                      ),
                    ),
                  ),
                ),
              ),

              // Floating beans
              for (int i = 0; i < 12; i++) _buildFloatingBean(i),

              // Main content
              SafeArea(
                child: Center(
                  child: SingleChildScrollView(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 32),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const SizedBox(height: 40),

                          // Logo animation
                          GestureDetector(
                            onTapDown: (_) =>
                                setState(() => _isLogoTapped = true),
                            onTapUp: (_) =>
                                setState(() => _isLogoTapped = false),
                            onTapCancel: () =>
                                setState(() => _isLogoTapped = false),
                            child: AnimatedScale(
                              scale: _isLogoTapped ? 0.97 : 1.0,
                              duration: const Duration(milliseconds: 150),
                              child: _buildLogo(),
                            ),
                          ),

                          const SizedBox(height: 32),

                          // Brand name
                          Text(
                            'CUPX',
                            style: TextStyle(
                              fontSize: 44,
                              fontWeight: FontWeight.w300,
                              letterSpacing: 1.2,
                              color: primaryColor,
                            ),
                          ),

                          const SizedBox(height: 8),

                          // Tagline
                          Text(
                            'Pha cà phê tự động',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.w300,
                              color: Colors.black54,
                              letterSpacing: 0.5,
                            ),
                          ),

                          const SizedBox(height: 48),

                          // Machine ID card
                          Container(
                            width: 240,
                            padding: const EdgeInsets.symmetric(
                                vertical: 16, horizontal: 20),
                            decoration: BoxDecoration(
                              color: Colors.grey.shade50,
                              borderRadius: BorderRadius.circular(14),
                              border: Border.all(color: Colors.grey.shade100),
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.black.withOpacity(0.03),
                                  blurRadius: 10,
                                  spreadRadius: 0,
                                  offset: const Offset(0, 2),
                                ),
                              ],
                            ),
                            child: Column(
                              children: [
                                Text(
                                  'PHÍA MÁY: ${kioskSide.toLowerCase() == 'left' ? 'TRÁI' : 'PHẢI'}',
                                  style: TextStyle(
                                    color: Colors.black87,
                                    fontSize: 15,
                                    fontWeight: FontWeight.w500,
                                    letterSpacing: 1.2,
                                  ),
                                ),
                                const SizedBox(height: 8),
                                Divider(color: Colors.grey.shade200, height: 1),
                              ],
                            ),
                          ),

                          const SizedBox(height: 48),

                          // Order button
                          _buildCustomButton(
                            text: 'Đặt Hàng',
                            icon: Icons.arrow_forward_rounded,
                            isPrimary: true,
                            onTap: () => context
                                .read<AppFlowCubit>()
                                .updateAppFlow(AppFlow.order),
                          ),

                          const SizedBox(height: 16),

                          const SizedBox(height: 40),

                          // Animated dots
                          Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: List.generate(5, (index) {
                              return AnimatedBuilder(
                                animation: _controller,
                                builder: (context, child) {
                                  final t =
                                      _controller.value * 1.5 + index * 0.2;
                                  final y = math.sin(2 * math.pi * t) * 3;
                                  final opacity =
                                      0.5 + math.cos(2 * math.pi * t) * 0.3;
                                  return Transform.translate(
                                    offset: Offset(0, y),
                                    child: Opacity(
                                      opacity: opacity,
                                      child: Container(
                                        margin: const EdgeInsets.symmetric(
                                            horizontal: 4),
                                        width: 6,
                                        height: 6,
                                        decoration: BoxDecoration(
                                          color: primaryColor,
                                          shape: BoxShape.circle,
                                        ),
                                      ),
                                    ),
                                  );
                                },
                              );
                            }),
                          ),

                          const SizedBox(height: 60),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  // Các methods _buildLogo, _buildCustomButton, _buildFloatingBean giữ nguyên...
  Widget _buildLogo() {
    return Container(
      width: 110,
      height: 110,
      decoration: BoxDecoration(
        color: Colors.white,
        shape: BoxShape.circle,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.08),
            blurRadius: 15,
            spreadRadius: 2,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Center(
        child: AnimatedBuilder(
          animation: _rotationAnimation,
          builder: (context, child) {
            return Transform.rotate(
              angle: _rotationAnimation.value,
              child: Icon(
                TDIcons.bean,
                size: 56,
                color: primaryColor,
              ),
            );
          },
        ),
      ),
    );
  }

  Widget _buildCustomButton({
    required String text,
    required IconData icon,
    required bool isPrimary,
    required VoidCallback onTap,
  }) {
    return ConstrainedBox(
      constraints: const BoxConstraints(maxWidth: 340),
      child: SizedBox(
        width: double.infinity,
        height: 56,
        child: Material(
          color: isPrimary ? primaryColor : Colors.transparent,
          borderRadius: BorderRadius.circular(14),
          child: InkWell(
            onTap: onTap,
            borderRadius: BorderRadius.circular(14),
            child: Container(
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(14),
                border: isPrimary
                    ? null
                    : Border.all(color: primaryColor, width: 1.5),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    text,
                    style: TextStyle(
                      color: isPrimary ? Colors.white : primaryColor,
                      fontSize: 16,
                      fontWeight: FontWeight.w500,
                      letterSpacing: 0.5,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Icon(
                    icon,
                    color: isPrimary ? Colors.white : primaryColor,
                    size: 20,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildFloatingBean(int index) {
    final random = math.Random(index);
    final size = 14.0 + random.nextDouble() * 10;
    final left = random.nextDouble() * MediaQuery.of(context).size.width;
    final top = random.nextDouble() * MediaQuery.of(context).size.height;
    final opacity = 0.05 + random.nextDouble() * 0.08;
    final duration = 5 + random.nextDouble() * 10;

    return Positioned(
      left: left,
      top: top,
      child: AnimatedBuilder(
        animation: _controller,
        builder: (context, child) {
          final t = _controller.value * duration;
          final y = math.sin(2 * math.pi * t / duration) * 12;
          final rotation = math.sin(2 * math.pi * t / duration) * 0.15;

          return Opacity(
            opacity: opacity,
            child: Transform.translate(
              offset: Offset(0, y),
              child: Transform.rotate(
                angle: rotation,
                child: Icon(
                  TDIcons.bean,
                  size: size,
                  color: primaryColor,
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
